using HappyFuture.Officer;
using StockLibrary;
using StockLibrary.StrategyManager;
using StockLibrary.StrategyForTrade;
using System;
using System.Collections.Generic;
using System.Text;
using UtilLibrary;

namespace HappyFuture.TradeStrategyFinder
{
    // 기존 데이터를 일자별로 갱신하면서 최적의 매매 타이밍을 찾아 좀
    class TestBot: FutureBot
    {
        TestBotLogForGoogleSheet tradeTestLog_;
        public bool isTotalTest_ = false;
        readonly DateTime testStartTime_ = DateTime.Now;
        readonly StockTradeModuleHistory moduleHistory_ = new StockTradeModuleHistory();
        public double initAccountMoney_ = 0;

        const double FAIL_LIMIT_RATE = 0.4f;

        public TestBot()
        {
            this.start();
            this.test_ = true;
            this.activeTrade();
        }
        
        public override bool start()
        {
            this.tradeFilter_ = new FutureTradeModuleFilter();
            this.stockModuleSearcher_ = new StockLibrary.FindBestTradeModule(this, new FutureTradeModuleFilter(), PublicVar.finderMode);
            this.telegram_ = ControlGet.getInstance.futureBot().telegram_;

            string sheetName = "매매 테스트 기록";
            string sheetId = "18wi7r0qI8RfhlvPXBG1JtRFAl38EW51twCldw4-9VcE";
            if (CodeModule.백테스트_매매_로그) {
                this.tradeTestLog_ = new TestBotLogForGoogleSheet(sheetId, sheetName);
            }
            futureDBHandler_ = null;
            return true;
        }

        public TradeStrategyTestRecoder recoder_ = null;

        public void setFundManagement(StrategyManagement fund)
        {
            this.fundManagement_ = fund;
            this.fundManagement_.setBot(this);
        }

        public void initRecode()
        {
            this.recoder_ = new TradeStrategyTestRecoder(this);
            this.recoder_.clear();
        }

        public void clearAllStocks()
        {
            this.stockPool_.Clear();
        }

        public void pullingMoney()
        {
            if (saveMyMoney_ > 8000) {
                const double LIMIT_START_MONEY = 9000;
                var totalAccount = this.totalAccountMoney();
                if (totalAccount < LIMIT_START_MONEY) {
                    var sub = LIMIT_START_MONEY - totalAccount;
                    if (sub > saveMyMoney_) {
                        sub = saveMyMoney_;
                    }
                    saveMyMoney_ -= sub;

                    accountMoney_ += sub;
                }
            }
        }
        bool initPulling_ = false;
        public void pullingNextYear(DateTime now)
        {
            if (now.Month != 1) {
                return;
            }
            if (initPulling_ == false) {
                initPulling_ = true;
            }
            this.allPayOff();
            var money = accountMoney_ - initAccountMoney_;
            if (money <= 0) {
                return;
            }
            saveMyMoney_ += money;
            accountMoney_ -= money;
        }

        public void checkMarginCanTrade()
        {
            StockDataEach eachDo = (string code, StockData stockData) => {
                FutureData futureData = stockData as FutureData;
                if (futureData.position_ == TRADING_STATUS.모니터닝) {
                    if (accountMoney_ < futureData.trustMargin()) {
                        futureData.canTrade_ = false;
                    }
                    else {
                        futureData.canTrade_ = true;
                    }
                }
            };
            this.doStocks(eachDo);
        }

        // pool 에 db 데이터 기반으로 1일 단위로 갱신
        // blance 를 조합하면서 최적의 blance 를 찾는 알고리즘 개발
        // 이를 기반으로 올해 실제 테스트를 해봄
        public override void buyStock(string code, int tradeCount, double tradePrice)
        {
            FutureData futureData = (FutureData) this.getStockDataCode(code);
            if (futureData == null) {
                return;
            }
            if (futureData.buyCount_ > 0) {
                return;
            }

            CandleData candle = futureData.realCandle();
            if (tradePrice < 0) {
                // 이건 즉시 구입 모드...
                tradePrice = candle.startPrice_ - futureData.tickSize_;
            }
            else {
                if (candle.isInnerCandle(tradePrice) == false) {
                    return;
                }
            }
            
            var canMoney = (futureData.trustMargin() * tradeCount);
            var tempMoney = this.accountMoney_ - canMoney;
            tempMoney -= ((PublicFutureVar.pureFeeTex / 2) * tradeCount);
            if (tempMoney < 0) {
                return;
            }
            this.accountMoney_ = tempMoney;

            futureData.resetBuyInfo();
            futureData.buyCount_ = tradeCount;
            futureData.buyPrice_ = tradePrice;
            futureData.canBuyCount_ = 0;
            futureData.canSellCount_ = 0;

            futureData.position_ = TRADING_STATUS.매수;
            futureData.lastChejanDate_ = candle.date_;
        }

        public override void sellStock(string code, int tradeCount, double tradePrice)
        {
            FutureData futureData = (FutureData) this.getStockDataCode(code);
            if (futureData == null) {
                return;
            }
            if (futureData.buyCount_ > 0) {
                return;
            }

            CandleData candle = futureData.realCandle();
            if (tradePrice < 0) {
                tradePrice = candle.startPrice_ + futureData.tickSize_;
            }
            else {
                if (candle.isInnerCandle(tradePrice) == false) {
                    return;
                }
            }
            
            var canMoney = (futureData.trustMargin() * tradeCount);
            var tempMoney = this.accountMoney_ - canMoney;
            tempMoney -= ((PublicFutureVar.pureFeeTex / 2) * tradeCount);
            if (tempMoney < 0) {
                return;
            }
            this.accountMoney_ = tempMoney;

            futureData.resetBuyInfo();
            futureData.buyCount_ = tradeCount;
            futureData.buyPrice_ = tradePrice;
            futureData.canBuyCount_ = 0;
            futureData.canSellCount_ = 0;

            futureData.position_ = TRADING_STATUS.매도;
            futureData.lastChejanDate_ = candle.date_;
        }

        public override double totalAccountMoney()
        {
            double total = this.accountMoney_;
            StockDataEach eachDo = (string code, StockData stockData) => {
                FutureData futureData = stockData as FutureData;
                if (futureData.isBuyedItem() == false) {
                    return;
                }
                if (futureData.buyCount_ > 0) {
                    double profit = futureData.nowProfit() + (futureData.trustMargin() * futureData.buyCount_);
                    total = total + profit;
                }
            };
            this.doStocks(eachDo);

            return total;
        }

        public bool checkStrategyFail()
        {
            var init = initAccountMoney_;
            var sub = init - totalAccountMoney();
            if (sub > (init * FAIL_LIMIT_RATE)) {   // 20% 이하로 빠지면 실패한 전략
                return true;
            }
            return false;
        }

        void addTradeRecode(FutureData futureData, string log, string why)
        {
            BackTestRecode recode = new BackTestRecode {
                code_ = futureData.code_,
                buyModule_ = "없음",
                sellModule_ = "없음",
                tradeModuelAvgProfit_ = 0,
            };
            BackTestRecoder tradeModule = futureData.tradeModule();

            if (tradeModule != null) {
                if (tradeModule.buyTradeModule_ != null) {
                    recode.buyModule_ = tradeModule.buyTradeModule_.getName();
                }
                if (tradeModule.sellTradeModule_ != null) {
                    recode.sellModule_ = tradeModule.sellTradeModule_.getName();
                }
                recode.tradeModuelAvgProfit_ = tradeModule.avgProfitRate_;
            }
            recode.position_ = futureData.position_;
            recode.buyTime_ = futureData.lastChejanDate_;
            recode.buyPrice_ = futureData.buyPrice_;
            var realCandle = futureData.realCandle();
            recode.payoffTime_ = realCandle.date_;
            recode.payoffPrice_ = futureData.nowPrice();
            recode.haveTime_ = recode.payoffTime_ - recode.buyTime_;
           
            if (Program.happyFuture_.futureDlg_.checkBox_reverse.Checked) {
                recode.minProfit_ = -futureData.minOneProfit_;
                recode.maxProfit_ = -futureData.maxProfit_;
                recode.oneProfit_ = -futureData.nowOneProfit();
                recode.profit_ = -futureData.nowProfit() - (PublicFutureVar.pureFeeTex * futureData.buyCount_);
            }
            else {
                recode.minProfit_ = futureData.minOneProfit_;
                recode.maxProfit_ = futureData.maxProfit_;
                recode.oneProfit_ = futureData.nowOneProfit();
                recode.profit_ = futureData.nowProfit() - (PublicFutureVar.pureFeeTex * futureData.buyCount_);
            }
            recode.buyCount_ = futureData.buyCount_;
            recode.ticks_ = futureData.nowProfitTicks();

            recode.whyPayOff_ = log;
            recode.payOffCode_ = futureData.payOffCode_;

            // 기록 추가
            this.recoder_.addRecode(recode);
            this.recoder_.calcTotal();
            this.recoder_.minProfitRate_ = Math.Min(this.todayTotalProfitRate_, this.recoder_.minProfitRate_);
            this.recoder_.maxProfitRate_ = Math.Max(this.todayTotalProfitRate_, this.recoder_.maxProfitRate_);
            this.recoder_.saveMyMoney_ = this.saveMyMoney_;

            if (CodeModule.백테스트_매매_로그) {
                if (this.isTotalTest_ && this.fundManagement_ != null) {
                    IList<Object> obj = new List<Object> {
                        this.testStartTime_,                         // 테스트시간
                        this.fundManagement_.GetType().Name,    // 전략이름
                        futureData.name_,                       // 종목명
                        futureData.minOneProfit_,                  // 최소이익
                        futureData.maxProfit_,                  // 최대이익
                        futureData.nowOneProfit(),              // 청산값
                        futureData.payOffCode_.ToString(),                 // 청산코드
                        why,                                    // 청산이유
                        futureData.payOffCode_
                    };
                    if (tradeModule != null) {
                        obj.Add(recode.buyModule_);
                        obj.Add(recode.sellModule_);
                    }

                    this.tradeTestLog_.addLog(obj);
                }
            }
        }

        public override void payOff(string code, string why)
        {
            FutureData futureData = (FutureData) this.getStockDataCode(code);
            StringBuilder log = new StringBuilder();
            bool isWin = false;

            try {
                if (futureData == null) {
                    return;
                }

                if (futureData.buyCount_ <= 0) {
                    Logger.getInstance.print(KiwoomCode.Log.에러, "{0}은 산게 아닌데 왜 payoff??", code);
                    return;
                }

                double nowProfit = futureData.nowProfit();        
                if (Program.happyFuture_.futureDlg_.checkBox_reverse.Checked) {
                    nowProfit = -nowProfit;
                }
                // 포지션 청산시, trade 모듈 다시 찾기
                isWin = nowProfit > PublicFutureVar.pureFeeTex;
                if (isWin) {
                    this.tradeWinCount_++;                   
                }
                futureData.setTradeBanTime();       // 다음전 트레이딩 못하게

                futureData.statisticsCount_.updateCount(isWin);
                this.tradeCount_++;

                why = why.Replace("\n", " ");
                
                log.AppendFormat("$$$ {0} [", futureData.nowDateTime());
                log.AppendFormat("$$$ [{0}:{1}] 선물 [{2}] 포지션 청산,", futureData.name_, futureData.code_, futureData.position_);
                log.AppendFormat(" - 구입가: {0:##,###0.#####} / 청산시점가: {1:##,###0.#####}.", futureData.buyPrice_, futureData.nowPrice());
                log.AppendFormat(" - 청산 이유: {0}\n\t\t{0}\n\n", futureData.payOffCode_, why);

                log.AppendFormat("* 예상 [");
                BackTestRecoder recode = futureData.tradeModule();
                if (recode != null) {
                    log.AppendFormat(" - 전략 buy[{0}] | sell[{1}]", recode.buyTradeModule_.getName(), recode.sellTradeModule_.getName());
                    log.AppendFormat(" - 예상 승률: {0:##.##}%/{1}시도, 평균 이익금: {2:##.###0.#####} $, 평균 보유시간 {3}분", recode.expectedWinRate_ * 100, recode.tradeCount_, recode.avgProfit_, recode.avgHaveTime_);
                    log.AppendFormat(" - 기대 범위 : {0}]", futureData.logExpectedRange());
                }
                log.AppendFormat("* 실제 결과 [");
                log.AppendFormat(" - 보유 시간: {0} ~ {1} : {2}분", futureData.lastChejanDate_.ToString("yy/MM/dd HH:mm"), futureData.nowDateTime().ToString("yy/MM/dd HH:mm"), futureData.positionHaveMin());
                log.AppendFormat(" - 보유중 최저 이익 [{0:##.###0.#####} $] / 최대 이익 [{0:##.###0.#####} $] ", futureData.minOneProfit_, futureData.maxProfit_);
                log.AppendFormat(" - {0} 개 x {1:##,###0.#####}], 개당 {2:##,###0.#####}으로 청산]", futureData.buyCount_, futureData.nowPrice(), futureData.nowOneProfit());

                log.AppendFormat("[예상이익: {0:##.###0.#####} $]", futureData.nowProfit());
            }
            catch (Exception e) {
                Logger.getInstance.print(KiwoomCode.Log.에러, "{0} pay off중 에러 {1}", futureData.name_, e.Message);
            }
            finally {
                double nowProfit = futureData.nowProfit();
                if (Program.happyFuture_.futureDlg_.checkBox_reverse.Checked) {
                    nowProfit = -nowProfit;
                }
                this.nowTotalProfit_ += nowProfit;
                this.todayTotalProfitRate_ = Util.calcProfitRate(this.yesterAccountMoney_, (this.yesterAccountMoney_ + this.nowTotalProfit_)) * 100;

                // 위의 로그를 다 남기고 현재 정보를 초기화 함
                var canMoney = (futureData.trustMargin() * futureData.buyCount_);
                this.accountMoney_ += (canMoney + nowProfit);
                this.accountMoney_ -= ((PublicFutureVar.pureFeeTex / 2) * futureData.buyCount_);  // 수수료 빼야...;

                // 이겼으면 이익금의 반을 내 몫으로 땜.
                if (isWin) {
                    var myShare = nowProfit * PublicFutureVar.myShareRate;
                    saveMyMoney_ += myShare;
                    this.accountMoney_ -= myShare;
                }
                this.addTradeRecode(futureData, log.ToString(), why);

                futureData.resetBuyInfo();
                this.researchTradeModuel(futureData, true);
            }
        }

        protected override bool checkContinueTrade()
        {
            return true;
        }

        protected override void sendbreakDownTradeLog()
        {
        }

        public void updateTodayProfit()
        {
            this.yesterAccountMoney_ = this.accountMoney_ + this.nowTotalProfit_;
            this.todayTotalProfitRate_ = 0.0f;
            this.nowTotalProfit_ = 0.0f;
        }

        public override void requestMargineInfo()
        {
        }

        void loadTradeModules(string code, DateTime startTime)
        {
            if (PublicVar.allTradeModuleUse) {
                return;
            }
            StockTradeModuleList moduleList = this.moduleHistory_.getModuleList(code);
            if (moduleList == null) {
                moduleList = new StockTradeModuleList(code);
                FutureBot bot = ControlGet.getInstance.futureBot();
                bot.selectTradeModuleList(ref moduleList, startTime);
                Logger.getInstance.print(KiwoomCode.Log.주식봇, "{0} 의 모듈이력 {1}개 로드", code, moduleList.count());
                this.moduleHistory_.addTradeModuleList(moduleList);
            }
        }
        
        public void updateTradeModuleList()
        {
            if (PublicVar.allTradeModuleUse) {
                return;
            }
            FutureBot bot = ControlGet.getInstance.futureBot();
            this.moduleHistory_.updateDB(bot);
        }

        public override void searchBestTradeModuleAnStock(StockData stockData, bool force = false)
        {
            if (PublicVar.allTradeModuleUse) {
                base.searchBestTradeModuleAnStock(stockData, force);
                return;
            }

            if (stockData.priceDataCount() < PublicVar.priceTableCount) {
                return;
            }

            if (this.moduleHistory_.updateTradeModule(ref stockData) == false) {
                // 검색 (상위 futurebot을 실행)
                base.searchBestTradeModuleAnStock(stockData, force);

                this.moduleHistory_.setTradeModule(stockData);
            }
        }

        public override bool nowStockMarketTime()
        {
            return true;
        }

        void setFurureDataInfo(FutureData futureData)
        {
            FutureBot bot = ControlGet.getInstance.futureBot();
            var code = futureData.code_;

            if (bot.engine_.connected()) {
                // 실제 hts 에서 받은 데이터를 가지고 셋팅.
                FutureData realData = bot.getStockDataCode(code) as FutureData;
                if (realData == null) {
                    StockDataEach eachDo = (string realCode, StockData stockData) => {
                        if (realData != null) {
                            return;
                        }
                        string catCode = code;
                        string catRealCode = realCode.Substring(0, realCode.Length - 3);

                        if (catCode == catRealCode) {
                            realData = stockData as FutureData;
                        }
                    };
                    bot.doStocks(eachDo);
                    if (realData == null) {
                        return;
                    }
                }
                futureData.tickSize_ = realData.tickSize_;
                futureData.tickValue_ = realData.tickValue_;
                futureData.margineMoney_ = realData.margineMoney_;
            }
            else {
                if (setFutureDataInfo(futureData) == false) {
                    return;
                }
            }
            futureData.endDays_ = 30; // 디폴트
            futureData.tradeEndTime_ = DateTime.Now;
        }
        DateTime simulRunTime_ = DateTime.MinValue;
        public override bool isSummerTime()
        {
            DateTime now = simulRunTime_;
            if (now == DateTime.MinValue) {
                now = DateTime.Now;
            }
            this.checkSummerTime(now);
            if (this.summerTime_ == SUMMER_TIME.ACTIVE_TIME) {
                return true;
            }
            return false;
        }

        public void addCandle(string code, CandleData candle, double testLostCut, double testProfit)
        {
            simulRunTime_ = candle.date_;
            FutureData futureData = this.getStockDataCode(code) as FutureData;
            if (futureData == null) {
                futureData = new FutureData(code, code);
                this.setFurureDataInfo(futureData);
                this.addStockData(futureData);
                this.loadTradeModules(code, candle.date_);
            }
            futureData.updatePrice(candle);
            futureData.updatePriceTable(this);

            DateTime now = futureData.nowDateTime();
            if (this.isSummerTime()) {
                if (now.Hour == 7 && now.Minute == 0) {
                    now = now.AddDays(1);
                    now = now.AddHours(-1);
                    futureData.tradeEndTime_ = now;
                    futureData.resetTradeBanTime();
                    futureData.makeYesterdayCandle(this);
                }
            }
            else {
                if (now.Hour == 8 && now.Minute == 0) {
                    now = now.AddDays(1);
                    now = now.AddHours(-1);
                    futureData.tradeEndTime_ = now;
                    futureData.resetTradeBanTime();
                    futureData.makeYesterdayCandle(this);
                }
            }
        }

        public void addRefCandle(string code, CandleData candle, REF_PRICE_TYPE type, double testLostCut, double testProfit)
        {
            FutureData futureData = this.getRefStockDataCode(code, type) as FutureData;
            if (futureData == null) {
                FutureBot bot = ControlGet.getInstance.futureBot();
                futureData = new FutureData(code, code);
                var pool = this.refStockPool_[(int) type];
                pool.Add(code, futureData);
            }
            futureData.updatePrice(candle);
            futureData.updatePriceTable(this);

            DateTime now = futureData.nowDateTime();
            if (this.isSummerTime()) {
                if (now.Hour == 7 && now.Minute == 0) {
                    now = now.AddDays(1);
                    now = now.AddHours(-1);
                    futureData.tradeEndTime_ = now;
                    futureData.resetTradeBanTime();
                }
            }
            else {
                if (now.Hour == 8 && now.Minute == 0) {
                    now = now.AddDays(1);
                    now = now.AddHours(-1);
                    futureData.tradeEndTime_ = now;
                    futureData.resetTradeBanTime();
                }
            }
        }

        public void setBuyAbleCount()
        {
            StockDataEach eachDo = (string code, StockData stockData) => {
                FutureData futureData = stockData as FutureData;
                double trustMargin = futureData.trustMargin();
                if (trustMargin <= 0) {
                    trustMargin = 3000;
                }
                int able = (int) (this.accountMoney_ / trustMargin);
                futureData.canBuyCount_ = able;
                futureData.canSellCount_ = able;
            };
            this.doStocks(eachDo);
        }

        public override void saveToDBPriceAt(StockData stockData)
        {
        }

        public override void saveToDB(bool force=false)
        {
        }

        public override void loadFromDB()
        {
        }

        public override void updateTradeRecode(FutureData futureData)
        {
        }

        public override bool loadFromDB(StockData stockData)
        {
            return true;
        }
        protected override void updateTradeModuleAllStock()
        {
        }

        public override void saveTradeModule(StockData stockData)
        {
        }

        public override void updatePoolView()
        {
        }
    }
}
