//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace StockLibrary.StrategyManager.EntryAndClears
//{
//    public class BollingerTouchECStrategy: BollingerECStrategy
//    {
//        public BollingerTouchECStrategy(Bot bot) : base(bot)
//        { }
//        protected override bool buy(StockData stockData, int timeIdx = 0)
//        {
//            var priceTable = this.priceTable(stockData, TERM);
//            if (priceTable == null) {
//                return false;
//            }
           
//            var width = bollingerWidth(stockData, timeIdx);
//            var candleWidth = priceTable[timeIdx].height();
//            if ((width / 2) < candleWidth) {
//                return false;
//            }
//            if (this.checkFilter(stockData) != TRADING_STATUS.매수) {
//                return false;
//            }

//            var nowCandle = priceTable[timeIdx];
//            double center = nowCandle.calc_[(int) EVALUATION_DATA.SMA_BOLLINGER_CENTER];
              
//            if (nowCandle.lowPrice_ < center) {
//                return true;
//            }
//            return false;
//        }

//        protected override bool sell(StockData stockData, int timeIdx = 0)
//        {
//            var priceTable = this.priceTable(stockData, TERM);
//            if (priceTable == null) {
//                return false;
//            }

//            var width = bollingerWidth(stockData, timeIdx);
//            var candleWidth = priceTable[timeIdx].height();
//            if ((width / 2) < candleWidth) {
//                return false;
//            }
//            if (this.checkFilter(stockData) != TRADING_STATUS.매도) {
//                return false;
//            }

//            var nowCandle = priceTable[timeIdx];
//            double center = nowCandle.calc_[(int) EVALUATION_DATA.SMA_BOLLINGER_CENTER];
//            if (nowCandle.highPrice_ > center) {
//                return true;
//            }
//            return false;
//        }
//    }
//}
