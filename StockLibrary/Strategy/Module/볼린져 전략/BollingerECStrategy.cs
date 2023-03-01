//using StockLibrary.StrategyManager.ProfitSafer;
//using System;
//using System.Collections.Generic;
//using System.Reflection;
//using UtilLibrary;

//namespace StockLibrary.StrategyManager.EntryAndClears
//{
 
//    /*
//     * 기본 컨셉은 ma 20 과 같은 단기이평선 or 볼린저 center 를 돌파 할때
//     * 볼리저 상단 / 하단에 맞으면 청산 하는 전략임
//     * 
//     */

//    public class BollingerECStrategy: EntryAndClearingStrategy
//    {
//        public BollingerECStrategy(Bot bot) : base(bot)
//        { }

//        protected override void setFilter()
//        {
//          //  this.addFillter(new SupportAndResistanceFilter());
//            this.addFillter(new EmaBollengerFundFilter());
//            this.addFillter(new EmaTrendFundFilter());
//            this.changeMA(EVALUATION_DATA.WMA_50);
//            this.changeBollinger(EVALUATION_DATA.EMA_BOLLINGER_UP, EVALUATION_DATA.EMA_BOLLINGER_CENTER, EVALUATION_DATA.EMA_BOLLINGER_DOWN);            
//        }

//        public override string name()
//        {
//            string name = "filter[";
//            foreach (var filter in filterPool_) {
//                name += string.Format("{0},", filter.GetType().Name);
//            }
//            name += string.Format("]/ma[{0}]", MA_FAST);
//            name += string.Format("]/bol[{0}/{1}/{2}]", BOL_UP, BOL_CENTER, BOL_DOWN);
//            return name;
//        }


//        public override string dbString()
//        {
//            string var = string.Format("{0},{1},{2},{3}", MA_FAST, BOL_UP, BOL_CENTER, BOL_DOWN);
//            foreach (var filter in filterPool_) {
//                var += string.Format(",{0}", filter.GetType().Name);
//            }
//            return var;
//        }

//        public override void parseDBString(string dbString)
//        {
//            var split = dbString.Split(',');
//            int index = 0;
//            if (split.Length >= 4) {
//                var ma = (EVALUATION_DATA) Enum.Parse(typeof(EVALUATION_DATA), split[index++]);
//                this.changeMA(ma);

//                var bup = (EVALUATION_DATA) Enum.Parse(typeof(EVALUATION_DATA), split[index++]);
//                var bcenter = (EVALUATION_DATA) Enum.Parse(typeof(EVALUATION_DATA), split[index++]);
//                var bdown = (EVALUATION_DATA) Enum.Parse(typeof(EVALUATION_DATA), split[index++]);
//                this.changeBollinger(bup, bcenter, bdown);
//            }
//            for (; index < split.Length; index++) {
//                var filterName = split[index];
//                Assembly assembly = Assembly.GetExecutingAssembly();
//                Type filterType = assembly.GetType("StockLibrary." + filterName.ToString());
//                if (filterType == null) {
//                    return;
//                }
//                var filter = Activator.CreateInstance(filterType) as FundFilter;
//                this.addFillter(filter);
//            }
//        }

//        protected double bollingerWidth(StockData stockData, int timeIdx)
//        {
//            List<CandleData> priceTable = stockData.priceTable();
//            double upper = priceTable[timeIdx].calc_[(int) BOL_UP];
//            double lower = priceTable[timeIdx].calc_[(int) BOL_DOWN];
//            return (upper - lower);
//        }

//        protected double bollingerWidthPercent(StockData stockData, int timeIdx)
//        {
//            var percent = bollingerWidth(stockData, timeIdx) / 100;
//            return percent;
//        }

//        protected const int TERM = 2;
//        protected const double DIV = 2;

//        protected override bool buy(StockData stockData, int timeIdx = 0)
//        {
//            var priceTable = this.priceTable(stockData, TERM);
//            if (priceTable == null) {
//                return false;
//            }

//            var nowCandle = priceTable[timeIdx];

//            var bwidth = bollingerWidth(stockData, timeIdx);
//            var candleWidth = nowCandle.height();
//            if ((bwidth / DIV) < candleWidth) {
//                return false;
//            }
//            if (this.checkFilter(stockData) != TRADING_STATUS.매수) {
//                return false;
//            }

//            double center = nowCandle.calc_[(int) BOL_CENTER];
//            double close = nowCandle.price_;          // 종가
//            double start = nowCandle.startPrice_;     // 시가
//                                                      // 볼린저 센터를 돌파하는 캔들이면 매수
//            if (Util.isRange(start, center, close)) {
//                stockData.payOffLostCut_ = nowCandle.price_ - (bwidth / 2);
//                stockData.payOffProfit_ = nowCandle.price_ + (bwidth / 2);
//                return true;
//            }

//            return false;
//        }

//        public override bool buyPayOffAtCompleteCandle(StockData stockData, int timeIdx = -1)
//        {
//            if (timeIdx == -1) {
//                timeIdx = 캔들완성_IDX;
//            }
//            var priceTable = this.priceTable(stockData, TERM);
//            if (priceTable == null) {
//                return false;
//            }

//            var nowCandle = priceTable[timeIdx];
//            var price = nowCandle.price_;
//            // 가격이 볼린저 상단을 돌파하면 청산
//            double high = nowCandle.highPrice_;
//            double bollingerHigh = nowCandle.calc_[(int) BOL_UP];
//            if (bollingerHigh < price) {
//                stockData.payOffCode_ = PAY_OFF_CODE.볼린저청산;
//                return true;
//            }

//            // 가격이 볼린저 하단을 터치하면 청산
//            double low = nowCandle.lowPrice_;
//            double bollingerLow = nowCandle.calc_[(int) BOL_DOWN];
//            if (bollingerLow > price) {
//                stockData.payOffCode_ = PAY_OFF_CODE.볼린저손절;
//                return true;
//            }

//            //// 가격이 ma 라인을 깨면 청산
//            //double ma = nowCandle.calc_[(int) TREND_STAND];
//            //var stand = (ma + low) / 2;
//            //if (stand > price) {
//            //    stockData.payOffCode_ = PAY_OFF_CODE.ma터치손절;
//            //    return true;
//            //}
//            //if (nowCandle.price_ > stockData.payOffProfit_) {
//            //    stockData.payOffCode_ = PAY_OFF_CODE.설정청산;
//            //    return true;
//            //}

//            //if (nowCandle.price_ < stockData.payOffLostCut_) {
//            //    stockData.payOffCode_ = PAY_OFF_CODE.설정손절;
//            //    return true;
//            //}

//            return false;
//        }

//        public override bool buyPayOffAtRealCandle(StockData stockData)
//        {
//            return false;
//            // return this.buyPayOffAtCompleteCandle(stockData, 0);
//        }

//        protected override bool sell(StockData stockData, int timeIdx = 0)
//        {
//            var priceTable = this.priceTable(stockData, TERM);
//            if (priceTable == null) {
//                return false;
//            }

//            var nowCandle = priceTable[timeIdx];
//            var bwidth = bollingerWidth(stockData, timeIdx);
//            var candleWidth = nowCandle.height();
//            if ((bwidth / DIV) < candleWidth) {
//                return false;
//            }
//            if (this.checkFilter(stockData) != TRADING_STATUS.매도) {
//                return false;
//            }

//            double center = nowCandle.calc_[(int) BOL_CENTER];
//            // 볼린저 센터를 돌파하는 캔들이면 매도
//            double close = nowCandle.price_;
//            double start = nowCandle.startPrice_;
//            if (Util.isRange(close, center, start)) {
//                stockData.payOffLostCut_ = nowCandle.price_ + (bwidth / 2);
//                stockData.payOffProfit_ = nowCandle.price_ - (bwidth / 2);
//                return true;
//            }

//            return false;
//        }

//        public override bool sellPayOffAtCompleteCandle(StockData stockData, int timeIdx = -1)
//        {
//            if (timeIdx == -1) {
//                timeIdx = 캔들완성_IDX;
//            }

//            var priceTable = this.priceTable(stockData, TERM);
//            if (priceTable == null) {
//                return false;
//            }

//            var nowCandle = priceTable[timeIdx];
//            var price = nowCandle.price_;
//            // 가격이 볼린저 하단을 돌파하면 청산
//            double low = nowCandle.lowPrice_;
//            double bollingerLow = nowCandle.calc_[(int) BOL_DOWN];
//            if (bollingerLow > price) {
//                stockData.payOffCode_ = PAY_OFF_CODE.볼린저청산;
//                return true;
//            }

//            // 가격이 상단을 터치하면 손절
//            double bollingerHigh = nowCandle.calc_[(int) BOL_UP];
//            double high = nowCandle.highPrice_;
//            if (bollingerHigh < price) {
//                stockData.payOffCode_ = PAY_OFF_CODE.볼린저손절;
//                return true;
//            }

//            // 가격이 ma 라인을 깨면 청산
//            //double ma = nowCandle.calc_[(int) TREND_STAND];
//            //var stand = (ma + high) / 2;
//            //if (stand < price) {
//            //    stockData.payOffCode_ = PAY_OFF_CODE.ma터치손절;
//            //    return true;
//            //}

//            //if (nowCandle.price_ < stockData.payOffProfit_) {
//            //    stockData.payOffCode_ = PAY_OFF_CODE.설정청산;
//            //    return true;
//            //}

//            //if (nowCandle.price_ > stockData.payOffLostCut_) {
//            //    stockData.payOffCode_ = PAY_OFF_CODE.설정손절;
//            //    return true;
//            //}

//            return false;
//        }

//        public override bool sellPayOffAtRealCandle(StockData stockData)
//        {
//            return false;
//            //  return this.sellPayOffAtCompleteCandle(stockData, 0);
//        }
//    }
//}
