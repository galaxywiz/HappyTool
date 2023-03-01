using StockLibrary.StrategyManager.Trade;
using System;
using System.Collections.Generic;
using System.Reflection;
using UtilLibrary;

namespace StockLibrary.StrategyManager.StrategyModuler
{
    public enum LONG_SHORT_TRADE
    {
        ONLY_LONG,
        ONLY_SHORT,
        BOTH,
    }
    // 진입과 청산을 관여하는 전략을 구현
    public class StrategyModuleGetter: SingleTon<StrategyModuleGetter>
    {
        public StrategyModule getStrategyModule(string name, Bot bot)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Type type = assembly.GetType("StockLibrary.StrategyManager.StrategyModuler." + name);
            if (type == null) {
                return null;
            }

            StrategyModule strategyModule = Activator.CreateInstance(type, bot) as StrategyModule;
            if (strategyModule == null) {
                return null;
            }
            return strategyModule;
        }
    }

    //@@@ 진입 로직과, 청산 로직을 클래스로 분리 시켜서 조합할까?!
    public abstract class StrategyModule
    {
        protected Bot bot_;
        public bool haveNetConnect_ = false;           // 인터넷 연결이 필요한 전략인가 (파이썬 전용)
        protected AssetBettor assetBettor_;            // 배팅 관리자

        // 슈카 방송에 나온 주름을 펴서 먹을 생각 하면 주름을 펴서 깨짐
        // 롱은 long
        // 숏은 short 으로 한 방향으로만 노리자.
        public LONG_SHORT_TRADE tradeLongShort_ = LONG_SHORT_TRADE.ONLY_LONG;       // 매수만 할건지, 매도도 같이 할건지, 매수/매도 할건지
        public List<Calculater> calculaterList_ = new List<Calculater>();
        public StrategyModule(Bot bot)
        {
            bot_ = bot;
            assetBettor_ = new AssetBettor();
        }

        bool initCacl_ = false;
        protected virtual void initCacluaterList()
        {
            if (initCacl_) {
                return;
            }
            initCacl_ = true;

            // 지표 계산
            calculaterList_.Add(new MACDCalculater());
            calculaterList_.Add(new MACalculater());

            calculaterList_.Add(new RSI14Calculater());
            calculaterList_.Add(new ADXCalculater());
            calculaterList_.Add(new CCICalculater());
            //       calculaterList_.Add(new StochasticCalculater());
            calculaterList_.Add(new ATRCalculater());

            calculaterList_.Add(new WilliamsCalculater());
            calculaterList_.Add(new UltimateCalculater());
            calculaterList_.Add(new BullBearCalculater());
            calculaterList_.Add(new ParabolicSAR());

            calculaterList_.Add(new BollingerCalculater());
            calculaterList_.Add(new PriceChannelCalculater());
        }

        public void setBettor(AssetBettor assetBettor)
        {
            assetBettor_ = assetBettor;
        }

        public string getBettorName()
        {
            if (assetBettor_ == null) {
                assetBettor_ = new AssetBettor();
            }
            return assetBettor_.GetType().Name;
        }

        public virtual bool useBackTestResult()
        {
            return false;
        }

        public virtual string name()
        {
            string name = "None";
            return name;
        }

        public virtual string dbString()
        {
            string var = string.Format("{0},", this.name());
            return var;
        }

        public virtual void parseDBString(string dbString)
        {
            var split = dbString.Split(',');
        }

        protected List<CandleData> priceTable(StockData stockData, int need)
        {
            List<CandleData> priceTable = stockData.priceTable();
            if (priceTable == null) {
                return null;
            }
            int lastTime = priceTable.Count - need;
            if (lastTime < 0) {
                return null;
            }
            return priceTable;
        }

        protected List<TradeStrategy> tradeStrategyPool_ = new List<TradeStrategy>();
        public void addTradeStrategy(TradeStrategy strategy)
        {
            tradeStrategyPool_.Add(strategy);
        }

        protected int 캔들완성_IDX = PublicVar.캔들완성_IDX;
        protected int 캔들생성_IDX = 0;

        public virtual int countTradeBuy(StockData stockData, int maxCount)
        {
            return assetBettor_.countTradeBuy(stockData, maxCount);
        }

        protected bool checkBuyTrade(StockData stockData, int timeIdx)
        {
            if (tradeStrategyPool_.Count > 0) {
                foreach (var filter in tradeStrategyPool_) {
                    if (filter.buy(stockData, timeIdx) == false) {
                        return false;
                    }
                }
            }
            return true;
        }

        protected abstract bool buy(StockData stockData, int timeIdx = 1);

        public virtual int countTradeSell(StockData stockData, int maxCount)
        {
            return assetBettor_.countTradeSell(stockData, maxCount);
        }
       
        protected bool checkSellTrade(StockData stockData, int timeIdx)
        {
            if (tradeStrategyPool_.Count > 0) {
                foreach (var filter in tradeStrategyPool_) {
                    if (filter.sell(stockData, timeIdx) == false) {
                        return false;
                    }
                }
            }
            return true;
        }
        protected abstract bool sell(StockData stockData, int timeIdx = 1);

        public TRADING_STATUS checkEntryStrategy(StockData stockData)
        {
            if (this.buy(stockData)) {
                return TRADING_STATUS.매수;
            }
            if (this.sell(stockData)) {
                return TRADING_STATUS.매도;
            }
            return TRADING_STATUS.모니터닝;
        }

        // 몇개 살까?
        public virtual int getTradingCount(StockData stockData, int maxCount)
        {
            //   return Math.Min(maxCount, 3);
            return Math.Min(maxCount, 1);
        }

        // 분봉 완료 된것을 시점을 팔지를 봄
        public virtual bool buyPayOffAtCompleteCandle(StockData stockData, int timeIdx = -1)
        {
            return this.sell(stockData, 캔들완성_IDX);
        }

        // 현재 분봉 보면서 긴급 break;
        public virtual bool buyPayOffAtRealCandle(StockData stockData)
        {
            //    return this.sell(stockData, 캔들생성_IDX);
            return false;
        }

        public virtual bool sellPayOffAtCompleteCandle(StockData stockData, int timeIdx = -1)
        {
            return this.buy(stockData, 캔들완성_IDX);
        }

        public virtual bool sellPayOffAtRealCandle(StockData stockData)
        {
            //    return this.buy(stockData, 캔들생성_IDX);
            return false;
        }

        const int TICK_SUB = 2;     // 직전 캔들과 센터 값이 3틱 이내면 청산

        // 내부에서 쓰는 함수들
        protected bool checkPriceEquality(StockData stockData, int timeIdx, int checkPrevCnt)
        {
            var priceTable = this.priceTable(stockData, checkPrevCnt);
            for (int i = 0; i < checkPrevCnt; ++i) {
                var pCandle = priceTable[timeIdx + i + 1];
                var pc = pCandle.centerPrice_;
                var candle = priceTable[timeIdx + i];
                var c = candle.centerPrice_;
                var temp = Math.Abs(pc - c);

                var tickSub = (int) (temp / stockData.oneTickSize());
                if (tickSub < TICK_SUB) {
                    return true;
                }
            }
            return false;
        }
    }


    public class ResistanceLineOverECStrategy: StrategyModule
    {
        const int NEED = 3;
        const int REVERSE_TREND = 3;
        Trade.TradeStrategy resistanceLineFilter_ = null;
        Trade.TradeStrategy candleUp_ = null;
        Trade.TradeStrategy maUp_ = null;

        public ResistanceLineOverECStrategy(Bot bot) : base(bot)
        {
            this.setMA(EVALUATION_DATA.EMA_100);
        }

        EVALUATION_DATA upper_;

        public void setMA(EVALUATION_DATA upper)
        {
            upper_ = upper;

            tradeStrategyPool_.Clear();
            resistanceLineFilter_ = new ResistanceLineOverFilter();
            maUp_ = new MaTrendTradeStrategy(upper);
            candleUp_ = new CandleUpTradeStrategy(2);

            this.addTradeStrategy(resistanceLineFilter_);
            this.addTradeStrategy(maUp_);
        }

        public override string name()
        {
            return string.Format("up[{0}]", upper_);
        }

        protected override bool buy(StockData stockData, int timeIdx = 1)
        {
            if (timeIdx == -1) {
                timeIdx = 캔들완성_IDX;
            }

            var priceTable = this.priceTable(stockData, NEED);
            if (priceTable == null) {
                return false;
            }

            // 3개 뒤까지 골든크로스인지 체크
            if (checkBuyTrade(stockData, timeIdx) == false) {
                return false;
            }

            if (candleUp_.buy(stockData) == false) {
                return false;
            }

            return true;
        }

        const int AVG_STAND = 4;
        public override bool buyPayOffAtCompleteCandle(StockData stockData, int timeIdx = -1)
        {
            if (timeIdx == -1) {
                timeIdx = 캔들완성_IDX;
            }
            var priceTable = this.priceTable(stockData, NEED);
            if (priceTable == null) {
                return false;
            }

            // 3캔들 평균값이 비슷해 지면 팔기
            if (checkPriceEquality(stockData, timeIdx, AVG_STAND)) {
                return true;
            }

            // 음봉 3개 뜨면 팔기
            bool isAllMinus = true;
            for (int i = 0; i < REVERSE_TREND; ++i) {
                var candle = priceTable[timeIdx + i];
                if (candle.isPlusCandle()) {
                    isAllMinus = false;
                    break;
                }
            }
            if (isAllMinus) {
                return true;
            }

            return false;
        }

        protected override bool sell(StockData stockData, int timeIdx = 1)
        {
            if (timeIdx == -1) {
                timeIdx = 캔들완성_IDX;
            }

            var priceTable = this.priceTable(stockData, NEED);
            if (priceTable == null) {
                return false;
            }

            if (checkSellTrade(stockData, timeIdx) == false) {
                return false;
            }

            if (candleUp_.sell(stockData) == false) {
                return false;
            }

            return true;
        }

        public override bool sellPayOffAtCompleteCandle(StockData stockData, int timeIdx = -1)
        {
            if (timeIdx == -1) {
                timeIdx = 캔들완성_IDX;
            }
            var priceTable = this.priceTable(stockData, NEED);
            if (priceTable == null) {
                return false;
            }

            // 3캔들 평균값이 비슷해 지면 팔기
            if (checkPriceEquality(stockData, timeIdx, AVG_STAND)) {
                return true;
            }

            // 양봉 3개 뜨면 팔기
            bool isAllPlus = true;
            for (int i = 0; i < REVERSE_TREND; ++i) {
                var candle = priceTable[timeIdx + i];
                if (candle.isMinusCandle()) {
                    isAllPlus = false;
                    break;
                }
            }
            if (isAllPlus) {
                return true;
            }

            return false;
        }
    }
}
