//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UtilLibrary;

//namespace StockLibrary.StrategyManager.EntryAndClears
//{
//    public class BackTestECStrategy: EntryAndClearingStrategy
//    {
//        public BackTestECStrategy(Bot bot) : base(bot)
//        {
//            this.addFillter(new EmaTrendFundFilter());
//        }

//        public override bool useBackTestResult()
//        {
//            return true;
//        }

//        public override void changeMA(EVALUATION_DATA ma)
//        {
//            filterPool_.Clear();
//            switch (ma) {
//                case EVALUATION_DATA.SMA_3:
//                this.addFillter(new SmaTrendFundFilter());
//                break;
//                case EVALUATION_DATA.EMA_3:
//                this.addFillter(new EmaTrendFundFilter());
//                break;
//                case EVALUATION_DATA.WMA_3:
//                this.addFillter(new WmaTrendFundFilter());
//                break;

//                default:
//                return;
//            }
//            MA_FAST = EVALUATION_DATA.EMA_5;
//        }
//        const int TERM = 2;

//        // 매수 조건
//        protected override bool buy(StockData stockData, int timeIdx = 0)
//        {
//            if (timeIdx == -1) {
//                timeIdx = 캔들완성_IDX;
//            }
//            var priceTable = this.priceTable(stockData, TERM);
//            if (priceTable == null) {
//                return false;
//            }

//            if (this.checkFilter(stockData) != TRADING_STATUS.매수) {
//                return false;
//            }

//#if false
//            var list = stockData.tradeModuleList();
//            BackTestRecoder tradeModule = null;
//            foreach (var recode in list) {
//                if (recode.buyTradeModule_.buy(stockData, timeIdx)) {
//                    tradeModule = recode;
//                    break;
//                }
//            }
//            if (tradeModule == null) {
//                return false;
//            }
//            stockData.setTradeModule(tradeModule);
//#else
//            var recode = stockData.tradeModule();
//            if (recode != null) {
//                if (recode.buyTradeModule_.buy(stockData, timeIdx)) {
//                    return true;
//                }
//            }
//#endif
//            return true;
//        }

//        // 청산 조건
//        public override bool buyPayOffAtCompleteCandle(StockData stockData, int timeIdx = -1)
//        {
//            if (timeIdx == -1) {
//                timeIdx = 캔들완성_IDX;
//            }
//            var priceTable = this.priceTable(stockData, TERM);
//            if (priceTable == null) {
//                return false;
//            }

//            var recode = stockData.tradeModule();
//            if (recode != null) {
//                if (recode.sellTradeModule_.sell(stockData, this.캔들완성_IDX)) {
//                    return true;
//                }
//            } else {
//                var prevCandle = priceTable[timeIdx + 1];
//                var nowCandle = priceTable[timeIdx];
//                if (prevCandle.price_ > nowCandle.price_) {
//                    return true;
//                }
//            }
//            return false;
//        }

//        // 실시간 청산 조건
//        public override bool buyPayOffAtRealCandle(StockData stockData)
//        {
//            return false;
//        }

//        //-----------------------------------------------------------------------//
//        // 매도 조건
//        protected override bool sell(StockData stockData, int timeIdx = 0)
//        {
//            if (timeIdx == -1) {
//                timeIdx = 캔들완성_IDX;
//            }
//            var priceTable = this.priceTable(stockData, TERM);
//            if (priceTable == null) {
//                return false;
//            }

//            if (this.checkFilter(stockData) != TRADING_STATUS.매도) {
//                return false;
//            }
//#if false
//            var list = stockData.tradeModuleList();
//            BackTestRecoder tradeModule = null;
//            foreach (var recode in list) {
//                if (recode.buyTradeModule_.sell(stockData, timeIdx)) {
//                    tradeModule = recode;
//                    break;
//                }
//            }
//            if (tradeModule == null) {
//                return false;
//            }
//            stockData.setTradeModule(tradeModule);
//#else
//            var recode = stockData.tradeModule();
//            if (recode != null) {
//                if (recode.buyTradeModule_.sell(stockData, timeIdx)) {
//                    return true;
//                }
//            }
//#endif
//            return false;
//        }

//        // 청산 조건
//        public override bool sellPayOffAtCompleteCandle(StockData stockData, int timeIdx = -1)
//        {
//            if (timeIdx == -1) {
//                timeIdx = 캔들완성_IDX;
//            }
//            var priceTable = this.priceTable(stockData, TERM);
//            if (priceTable == null) {
//                return false;
//            }
//            var recode = stockData.tradeModule();
//            if (recode != null) {
//                if (recode.sellTradeModule_.buy(stockData, this.캔들완성_IDX)) {
//                    return true;
//                }
//            }
//            else {
//                var prevCandle = priceTable[timeIdx + 1];
//                var nowCandle = priceTable[timeIdx];
//                if (prevCandle.price_ < nowCandle.price_) {
//                    return true;
//                }
//            }
//            return false;
//        }

//        // 실시간 청산 조건
//        public override bool sellPayOffAtRealCandle(StockData stockData)
//        {
//            return false;
//        }
//    }
//}
