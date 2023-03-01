using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockLibrary.StrategyManager.Trade
{
    public class MaOverTradeStrategy: TradeStrategy
    {
        const int TERM = 1;
        public EVALUATION_DATA MA_STAND;
        public EVALUATION_DATA MA_FAST, MA_SLOW;
        public MaOverTradeStrategy()
        {
            MA_FAST = EVALUATION_DATA.SMA_20;
            MA_SLOW = EVALUATION_DATA.SMA_50;
            MA_STAND = EVALUATION_DATA.SMA_100;
        }

        public MaOverTradeStrategy(EVALUATION_DATA fast, EVALUATION_DATA slow, EVALUATION_DATA stand)
        {
            MA_FAST = fast;
            MA_SLOW = slow;
            MA_STAND = stand;
        }

        public override bool buy(StockData stockData, int timeIdx)
        {
            var priceTable = this.priceTable(stockData, TERM);
            if (priceTable == null) {
                return false;
            }

            var nowCandle = priceTable[timeIdx];
            var ma = nowCandle.calc_[(int) MA_STAND];
            var slow = nowCandle.calc_[(int) MA_SLOW];
            var fast = nowCandle.calc_[(int) MA_FAST];
            if (ma > slow) {
                return false;
            }
            if (ma > fast) {
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

            var nowCandle = priceTable[timeIdx];
            var ma = nowCandle.calc_[(int) MA_STAND];
            var slow = nowCandle.calc_[(int) MA_SLOW];
            var fast = nowCandle.calc_[(int) MA_FAST];
            if (ma < slow) {
                return false;
            }
            if (ma < fast) {
                return false;
            }
            return true;
        }
    }
}
