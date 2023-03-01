using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockLibrary.StrategyManager.Trade
{
    //------------------------------------------------------------//
    // 골든 크로스 필터
    public class MaGoldCrossTradeStrategy: TradeStrategy
    {
        const int TERM = 3;
        protected EVALUATION_DATA MA_FAST;
        protected EVALUATION_DATA MA_SLOW;
        public MaGoldCrossTradeStrategy()
        {
            MA_FAST = EVALUATION_DATA.SMA_20;
            MA_SLOW = EVALUATION_DATA.SMA_50;
        }

        public MaGoldCrossTradeStrategy(EVALUATION_DATA fast, EVALUATION_DATA slow)
        {
            MA_FAST = fast;
            MA_SLOW = slow;
        }

        protected bool crossUp(StockData stockData, int timeIdx)
        {
            var priceTable = this.priceTable(stockData, TERM);
            if (priceTable == null) {
                return false;
            }

            var prevCandle = priceTable[timeIdx + 1];
            var fast = prevCandle.calc_[(int) MA_FAST];
            var slow = prevCandle.calc_[(int) MA_SLOW];
            if (fast < slow) {
                var nowCandle = priceTable[timeIdx];
                fast = nowCandle.calc_[(int) MA_FAST];
                slow = nowCandle.calc_[(int) MA_SLOW];
                if (fast > slow) {
                    return true;
                }
            }
            return false;
        }

        protected bool crossDown(StockData stockData, int timeIdx)
        {
            var priceTable = this.priceTable(stockData, TERM);
            if (priceTable == null) {
                return false;
            }

            var prevCandle = priceTable[timeIdx + 1];
            var fast = prevCandle.calc_[(int) MA_FAST];
            var slow = prevCandle.calc_[(int) MA_SLOW];
            if (fast > slow) {
                var nowCandle = priceTable[timeIdx];
                fast = nowCandle.calc_[(int) MA_FAST];
                slow = nowCandle.calc_[(int) MA_SLOW];
                if (fast < slow) {
                    return true;
                }
            }
            return false;
        }

        public override bool buy(StockData stockData, int timeIdx)
        {
            var priceTable = this.priceTable(stockData, TERM);
            if (priceTable == null) {
                return false;
            }
            if (this.crossUp(stockData, timeIdx)) {
                return true;
            }
            return false;
        }

        public override bool sell(StockData stockData, int timeIdx)
        {
            var priceTable = this.priceTable(stockData, TERM);
            if (priceTable == null) {
                return false;
            }
            if (this.crossDown(stockData, timeIdx)) {
                return true;
            }
            return false;
        }
    }
}
