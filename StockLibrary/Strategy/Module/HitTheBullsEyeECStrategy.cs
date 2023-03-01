//using System.Collections.Generic;

//namespace StockLibrary.StrategyManager.EntryAndClears
//{
//    public class HitTheBullsEyeECStrategy: EntryAndClearingStrategy
//    {
//        public HitTheBullsEyeECStrategy(Bot bot) : base(bot)
//        {

//        }

//        bool signalBuyAble(StockData stockData, int timeIdx)
//        {
//            List<CandleData> priceTable = stockData.priceTable();
//            const int HIGH_RANGE_LEN = 5;
//            //[매수 Setup조건]
//            //1) 현재봉 고가가 최근 5개봉 고가 중 최고가 
//            var nowCandle = priceTable[timeIdx];
//            for (int i = timeIdx + 1; i < timeIdx + HIGH_RANGE_LEN; ++i) {
//                var candle = priceTable[i];
//                if (nowCandle.highPrice_ <= candle.highPrice_) {
//                    return false;
//                }
//            }

//            const int CLOSE_RANGE_LEN = 2;
//            //2) 현재봉 종가가 2개 전봉 고가보다 크다   
//            for (int i = timeIdx + 1; i <= timeIdx + CLOSE_RANGE_LEN; ++i) {
//                var candle = priceTable[i];
//                if (nowCandle.price_ <= candle.highPrice_) {
//                    return false;
//                }
//            }

//            //3) 현재봉의 50지수이동평균선이 전봉의 50지수이동평균선 보다 크다.
//            var prevCandle = priceTable[timeIdx + 1];
//            var prevEma50 = prevCandle.calc_[(int) EVALUATION_DATA.EMA_50];
//            var ema50 = nowCandle.calc_[(int) EVALUATION_DATA.EMA_50];
//            if (prevEma50 >= ema50) {
//                return false;
//            }

//            return true;
//        }

//        bool signalSellAble(StockData stockData, int timeIdx)
//        {
//            List<CandleData> priceTable = stockData.priceTable();
//            const int HIGH_RANGE_LEN = 5;
//            //[매도 Setup조건]
//            //1) 현재봉 저가 최근 5개봉 저가 중 최저가 
//            var nowCandle = priceTable[timeIdx];
//            for (int i = timeIdx + 1; i < timeIdx + HIGH_RANGE_LEN; ++i) {
//                var candle = priceTable[i];
//                if (nowCandle.lowPrice_ >= candle.lowPrice_) {
//                    return false;
//                }
//            }

//            const int CLOSE_RANGE_LEN = 2;
//            //2) 현재봉 종가가 2개 전봉 저가보다 작다
//            for (int i = timeIdx + 1; i <= timeIdx + CLOSE_RANGE_LEN; ++i) {
//                var candle = priceTable[i];
//                if (nowCandle.price_ >= candle.lowPrice_) {
//                    return false;
//                }
//            }

//            //3) 현재봉의 50지수이동평균선이 전봉의 50지수이동평균선 보다 작다.
//            var prevCandle = priceTable[timeIdx + 1];
//            var prevEma50 = prevCandle.calc_[(int) EVALUATION_DATA.EMA_50];
//            var ema50 = nowCandle.calc_[(int) EVALUATION_DATA.EMA_50];
//            if (prevEma50 <= ema50) {
//                return false;
//            }

//            return true;
//        }
//        const int BUY_LEN = 2;
//        const int PROFIT_TIME = 3;

//        protected override bool buy(StockData stockData, int timeIdx)
//        {
//            int TERM = 7;
//            List<CandleData> priceTable = stockData.priceTable();
//            if (priceTable == null) {
//                return false;
//            }
//            int lastTime = priceTable.Count - TERM;
//            if (lastTime < timeIdx) {
//                return false;
//            }

//            stockData.resetBuyEntryPrice();
//            for (int i = timeIdx; i < timeIdx + BUY_LEN; ++i) {
//                if (this.signalBuyAble(stockData, i)) {
//                    // 청산 조건
//                    var priceCandle = priceTable[i + 2];
//                    stockData.buyEntryPrice_ = (priceCandle.highPrice_ + priceCandle.lowPrice_) / 2;
//                    break;
//                }
//            }
//            if (stockData.buyEntryPrice_ < 0) {
//                return false;
//            }

//            var nowCandle = priceTable[timeIdx];
//            // 손절 가격 설정
//            var atr30 = nowCandle.calc_[(int) EVALUATION_DATA.ATR_30];
//            stockData.payOffLostCut_ = stockData.buyEntryPrice_ - (atr30 * PROFIT_TIME);

//            // 손익 가격 설정
//            stockData.payOffProfit_ = nowCandle.highPrice_ + ((nowCandle.highPrice_ - stockData.buyEntryPrice_) * PROFIT_TIME);

//            return true;
//        }

//        public override bool buyPayOffAtCompleteCandle(StockData stockData, int timeIdx = -1)
//        {
//            if (timeIdx == -1) {
//                timeIdx = 캔들완성_IDX;
//            }
//            if (signalSellAble(stockData, 캔들완성_IDX)) {
//                return true;
//            }

//            List<CandleData> priceTable = stockData.priceTable();
//            if (priceTable == null) {
//                return false;
//            }

//            var nowCandle = priceTable[캔들완성_IDX];
//            if (nowCandle.price_ > stockData.payOffProfit_) {
//                return true;
//            }

//            if (nowCandle.price_ < stockData.payOffLostCut_) {
//                return true;
//            }
//            return false;
//        }

//        public override bool buyPayOffAtRealCandle(StockData stockData)
//        {
//            List<CandleData> priceTable = stockData.priceTable();
//            if (priceTable == null) {
//                return false;
//            }

//            var nowCandle = priceTable[0];
//            if (nowCandle.price_ > stockData.payOffProfit_) {
//                return true;
//            }

//            if (nowCandle.price_ < stockData.payOffLostCut_) {
//                return true;
//            }
//            return false;
//        }

//        protected override bool sell(StockData stockData, int timeIdx)
//        {
//            int TERM = 7;
//            List<CandleData> priceTable = stockData.priceTable();
//            if (priceTable == null) {
//                return false;
//            }
//            int lastTime = priceTable.Count - TERM;
//            if (lastTime < timeIdx) {
//                return false;
//            }

//            stockData.resetBuyEntryPrice();
//            for (int i = timeIdx; i < timeIdx + BUY_LEN; ++i) {
//                if (this.signalSellAble(stockData, i)) {
//                    // 청산 조건
//                    var priceCandle = priceTable[i + 2];
//                    stockData.buyEntryPrice_ = (priceCandle.highPrice_ + priceCandle.lowPrice_) / 2;
//                    break;
//                }
//            }
//            if (stockData.buyEntryPrice_ < 0) {
//                return false;
//            }

//            var nowCandle = priceTable[timeIdx];
//            // 손절 가격 설정
//            var atr30 = nowCandle.calc_[(int) EVALUATION_DATA.ATR_30];
//            stockData.payOffLostCut_ = stockData.buyEntryPrice_ + (atr30 * PROFIT_TIME);

//            // 손익 가격 설정
//            stockData.payOffProfit_ = nowCandle.lowPrice_ - ((stockData.buyEntryPrice_ - nowCandle.lowPrice_) * PROFIT_TIME);
//            return true;
//        }

//        public override bool sellPayOffAtCompleteCandle(StockData stockData, int timeIdx = -1)
//        {
//            if (timeIdx == -1) {
//                timeIdx = 캔들완성_IDX;
//            }
//            if (this.signalBuyAble(stockData, 캔들완성_IDX)) {
//                return true;
//            }

//            List<CandleData> priceTable = stockData.priceTable();
//            if (priceTable == null) {
//                return false;
//            }

//            var nowCandle = priceTable[캔들완성_IDX];
//            if (nowCandle.price_ < stockData.payOffProfit_) {
//                return true;
//            }

//            if (nowCandle.price_ > stockData.payOffLostCut_) {
//                return true;
//            }
//            return false;
//        }
        
//        public override bool sellPayOffAtRealCandle(StockData stockData)
//        {
//            List<CandleData> priceTable = stockData.priceTable();
//            if (priceTable == null) {
//                return false;
//            }

//            var nowCandle = priceTable[0];
//            if (nowCandle.price_ < stockData.payOffProfit_) {
//                return true;
//            }

//            if (nowCandle.price_ > stockData.payOffLostCut_) {
//                return true;
//            }
//            return false;
//        }
//    }
//}
