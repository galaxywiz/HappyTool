using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockLibrary.StrategyManager.Trade
{
    //------------------------------------------------------------//
    // 볼린저 중앙선 기울기 체크
    public class BollengerTradeStrategy: TradeStrategy
    {
        public double SLOPE_BOLLINGER = 0.01;
        const int TERM = 1;
        protected EVALUATION_DATA SLOPE_STAND = EVALUATION_DATA.SMA_BOLLINGER_CENTER;

        public override bool buy(StockData stockData, int timeIdx)
        {
            var priceTable = this.priceTable(stockData, TERM);
            if (priceTable == null) {
                return false;
            }

            var slope = stockData.slope(SLOPE_STAND, timeIdx);
            if (slope <= SLOPE_BOLLINGER) {
                return false;
            }
            return true;
        }

        public override bool sell(StockData stockData, int timeIdx)
        {
            var priceTable = this.priceTable(stockData, TERM);
            if (priceTable == null) {
                return false;
            }

            var slope = stockData.slope(SLOPE_STAND, timeIdx);
            if (slope >= -SLOPE_BOLLINGER) {
                return false;
            }
            return true;
        }
    }
}
