using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockLibrary.StrategyManager.Trade
{
    //------------------------------------------------------------//
    // 단기 이평선의 기울기 확인
    public class MaSlopeTradeStrategy: TradeStrategy
    {
        const int TERM = 1;
        public EVALUATION_DATA MA_STAND = EVALUATION_DATA.SMA_50;
        public double SLOPE_MA = 0f;
        // 캔들 생성에서 확인이라...

        public override bool buy(StockData stockData, int timeIdx)
        {
            var priceTable = this.priceTable(stockData, TERM);
            if (priceTable == null) {
                return false;
            }
            // 기울기가 오르고 있어야함.
            double slope = stockData.slope(MA_STAND, timeIdx);
            if (slope < SLOPE_MA) {
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
            // 기울기가 내려가야함.
            double slope = stockData.slope(MA_STAND, timeIdx);
            if (slope > -SLOPE_MA) {
                return false;
            }
            return true;
        }
    }
}
