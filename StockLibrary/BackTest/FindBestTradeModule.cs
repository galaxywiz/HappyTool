using KiwoomCode;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using UtilLibrary;
using static StockLibrary.StrategyModuleList;

namespace StockLibrary
{
    public abstract class TradeModuleFilter
    {
        public abstract IEnumerable<BackTestRecoder> doOrderBy(IEnumerable<BackTestRecoder> srcList);
        public abstract bool doFilter(BackTestRecoder recoder, double lostCut);
    }
    public enum FINDER_MODE
    {
        쾌속,                 // 기본적인 rsi / candle, 파라볼릭만 셋팅
        보통,              // buy는 serach로 거른 pool에서, sell은 genPool에서 통상 1~3분 소요, 선물 기본옵션  (cpu 스코어 1.8만에서)
        느리게,            // buy / sell 를 genPool에서 모든 경우에 대해 찾기 통상 6~10여분 소요, 이건 cpu 파워가 좀더 쎄지면 시도
    }
    public class FindBestTradeModule
    {
        // loop 를 너무 돌아서 허들을 낮춤, 어차피 1주일 마다 뽑은 최적의 모듈가운데에서 하는거니 확율이 높지 않을까?
        // LOOP 는 제곱 값만큼 도니까, 적당히 숫자 조절 할것.
        readonly Bot bot_ = null;
        public FINDER_MODE defaultMode_ = FINDER_MODE.느리게;

        readonly TradeModuleFilter filter_;
        readonly Dictionary<FINDER_MODE, List<CombineStrategyModule>> coreTestModule_ = new Dictionary<FINDER_MODE, List<CombineStrategyModule>>();
        readonly Dictionary<FINDER_MODE, List<CombineStrategyModule>> serachingBuyModuleList_ = new Dictionary<FINDER_MODE, List<CombineStrategyModule>>();
        readonly int 쾌속_모듈_갯수 = PublicVar.quickPoolCount;

        // 초기값은 선물에서 잡은 값 기반임. 건들이지마!!!
        public FindBestTradeModule(Bot bot, TradeModuleFilter filter, FINDER_MODE mode = FINDER_MODE.보통)
        {
            this.bot_ = bot;
            this.filter_ = filter;
            this.defaultMode_ = mode;

            var moduleList = StrategyModuleList.getInstance;
            /* 
             * 실행 복잡도 N = (poolSize * poolSize) * 2
             * 
             * getTradedModulePool = 매매 기록을 보고 가장 빈도 높은거 20여개 
             * getDefaultPool = 각 종목 테스트 한 286개
             * getGenPool = 모든 조합 1464개
             * 
             */
            moduleList.initDefaultModule();
            var defaultPool = moduleList.getDefaultPool();
            if (defaultPool.Count == 0) {
                MessageBox.Show("매매 모듈이 없습니다.");
            }

            this.coreTestModule_.Add(FINDER_MODE.보통, new List<CombineStrategyModule>(defaultPool.Values));
            this.coreTestModule_.Add(FINDER_MODE.느리게, moduleList.getGenPool());

            this.serachingBuyModuleList_.Add(FINDER_MODE.보통, new List<CombineStrategyModule>(defaultPool.Values));
            this.serachingBuyModuleList_.Add(FINDER_MODE.느리게, moduleList.getGenPool());
        }

        public string serachingItemName_ = string.Empty;
        public bool have(StockData stockData)
        {
            return serachingItemName_ == stockData.name_;
        }

        //-------------------------------------------------------------------------------------------------
        // 테스팅 메인
        List<BackTestRecoder> doTesting(StockData stockData, IEnumerable<StrategyModule> loopX, bool isBuySearch, FINDER_MODE mode)
        {
            // lock 회피 하기 위해 레코드 미리 pool 확보
            var tempPool = new List<BackTestRecoder>();
            var coreTestModule = this.coreTestModule_[mode];
            var moduleCount = coreTestModule.Count - 1;
            var loopXCount = loopX.Count();
            for (int i = 0; i < loopXCount * coreTestModule.Count; ++i) {
                tempPool.Add(stockData.newBackTestRecoder());
            }

            // 확보한 레코드에 쓰도록 테스팅
            int idx = 0;
            foreach (var xModule in loopX) {
                if (isBuySearch) {
                    Parallel.For(0, moduleCount, (j) => {
                        var recode = tempPool[idx * loopXCount + j];
                        this.bot_.doBackTestAnStock(stockData, coreTestModule[j], xModule, ref recode);
                    });
                }
                else {
                    Parallel.For(0, moduleCount, (j) => {
                        var recode = tempPool[idx * loopXCount + j];
                        this.bot_.doBackTestAnStock(stockData, xModule, coreTestModule[j], ref recode);
                    });
                }
                idx++;
            }

            // 취합 과정. 쓸모있는것만 넣는다.
            var margePool = new List<BackTestRecoder>();
            foreach (var recode in tempPool) {
                if (this.filter_.doFilter(recode, stockData.lostCutProfit())) {
                    margePool.Add(recode);
                }
            }
            tempPool = null;
            return margePool;
        }

        bool needChangeTradeModule(StockData stockData)
        {
            if (stockData.tradeModule() == null) {
                return true;
            }
            var tradeModule = stockData.tradeModule();
            if (tradeModule == null) {
                return true;
            }

            var tempRecode = stockData.newBackTestRecoder();

            this.bot_.doBackTestAnStock(stockData, tradeModule.buyTradeModule_, tradeModule.sellTradeModule_, ref tempRecode);
            if (this.filter_.doFilter(tempRecode, stockData.lostCutProfit())) {
                //Logger.getInstance.print(Log.주식봇, "{0} 의 모듈 이전꺼 {1}/{2} 계속 사용", stockData.name_, tradeModule.buyTradeModule_.getName(), tradeModule.sellTradeModule_.getName());
                return false;
            }
            var time = stockData.nowDateTime() - stockData.tradeModuleUpdateTime_;
            Logger.getInstance.print(Log.주식봇, "{0} 의 모듈, {1} 분만에 갱신, 이전꺼 {2}/{3} 실패. 다시 찾기", stockData.name_, time.TotalMinutes, tradeModule.buyTradeModule_.getName(), tradeModule.sellTradeModule_.getName());
            return true;
        }

        /// <summary>
        /// 모든 모듈에 대한 테스팅을 함
        /// </summary>
        readonly Stopwatch findAllModuleWatch_ = new Stopwatch();
        public void runAllModule(StockData stockData)
        {
            var fund = bot_.fundManagement_;
            if (fund == null) {
                return;
            }

            if (stockData == null) {
                return;
            }

            if (fund.strategyModule_.useBackTestResult() == false) {
                return;
            }

            if (stockData.priceDataCount() == 0) {
                return;
            }

            if (stockData.isBuyedItem()) {
                if (stockData.tradeModulesCount() != 0) {
                    return;
                }
            }

            if (defaultMode_ == FINDER_MODE.쾌속) {
                List<BackTestRecoder> tmp = new List<BackTestRecoder>();
                var recode = stockData.newBackTestRecoder();
                var strategyList = StrategyModuleList.getInstance;

                recode.buyTradeModule_ = strategyList.parseModule("gen[CandleTradeStrategy][ParabolicSARTradeStrategy]_2,");
                recode.sellTradeModule_ = strategyList.parseModule("gen[CandleTradeStrategy][ParabolicSARTradeStrategy]_2,");
                tmp.Add(recode);
                stockData.tradeModuleList_ = new List<BackTestRecoder>(tmp);
                return;
            }

            this.serachingItemName_ = stockData.name_;
            if (this.needChangeTradeModule(stockData) == false) {
                return;
            }

            FINDER_MODE mode = this.defaultMode_;
            if (stockData.tradeModuleUpdateTime_.AddMinutes(PublicVar.tradeModuleUpdateMins) >= stockData.nowDateTime()) {
                if (stockData.tradeModulesCount() == 0) {
                    mode = FINDER_MODE.보통;
                }
            }

            this.findAllModuleWatch_.Restart();
            var coreTestModule = this.coreTestModule_[mode];
            var serachingBuyModuleList = this.serachingBuyModuleList_[mode];

            IEnumerable<BackTestRecoder> recoderList = null;
            try {
                stockData.resetFinedBestRecoders();               
                List<BackTestRecoder> testedList = new List<BackTestRecoder>();
           
                // 테스트 함수
                // buy를 중점으로 찾기
                var retBackTested = this.doTesting(stockData, serachingBuyModuleList, true, mode);
                if (retBackTested == null || retBackTested.Count == 0) {
                    return;
                }
                testedList.AddRange(retBackTested);

                if (coreTestModule.Count != serachingBuyModuleList.Count) {
                    // 그 다음 sell을 찾는데, buy로 잘 찾은건 고정하고 sell을 찾는다.
                    var bestBuy = retBackTested.Select(x => x.buyTradeModule_);

                    // buy를 기준으로 sell 의 다양성을 테스팅 한다.
                    retBackTested = this.doTesting(stockData, bestBuy, false, mode);
                    if (retBackTested != null) {
                        testedList.AddRange(retBackTested);
                    }
                }

                // 이 결과를 조합해서 다시 필터링 한다.
                var finalList = this.filter_.doOrderBy(testedList);
                if (finalList != null) {
                    recoderList = finalList;
                    return;
                }
            }
            catch (Exception e) {
                Logger.getInstance.print(Log.에러, "[{0}] 종목 백테스팅 에러 [{1}], stack[{2}]", stockData.name_, e.Message, e.StackTrace);
            }
            finally {
                this.applyResult(stockData, recoderList);
                this.findAllModuleWatch_.Stop();
                this.serachingItemName_ = string.Empty;
            }
        }

        void applyResult(StockData stockData, IEnumerable<BackTestRecoder> recoderList)
        {
            var logger = Logger.getInstance;
            var code = stockData.code_;

            try {
                stockData.tradeModuleUpdateTime_ = stockData.nowDateTime();

                if (recoderList == null) {
                    if (stockData.isBuyedItem()) {
                        // 산거면 이전 모듈 복구
                        this.bot_.loadFromDB(stockData);
                    }
                    return;
                }

                stockData.resetFinedBestRecoders();
                stockData.tradeModuleList_ = new List<BackTestRecoder>(recoderList);

                if (stockData.tradeModulesCount() == 0) {
                    if (stockData.isBuyedItem() == false) {
                        this.bot_.removeStock(code);
                        this.bot_.updatePoolView();
                        return;
                    }
                }

                this.bot_.saveTradeModule(stockData);

                if (this.bot_.nowStockMarketTime()) {
                    this.bot_.updatePoolView();
                }
            }
            catch (Exception e) {
                logger.print(Log.백테스팅, "{0} 종목 적용중 에러 {1} / {2}", code, e.Message, e.StackTrace);
            }
        }
    }
}
