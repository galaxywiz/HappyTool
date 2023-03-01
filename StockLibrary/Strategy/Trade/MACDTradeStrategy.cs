using StockLibrary.StrategyManager.Trade;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockLibrary.Strategy.Trade
{
    public class MACDTradeStrategy: TradeStrategy
    {
        const int TERM = 3;
        public MACDTradeStrategy()
        {
        }

        protected bool buySignal(List<CandleData> priceTable, int timeIdx)
        {
            if (priceTable.Count < TERM) {
                return false;
            }
            var nowCandle = priceTable[timeIdx];
            var nowMacd = nowCandle.calc_[(int) EVALUATION_DATA.MACD];
            var nowSignal = nowCandle.calc_[(int) EVALUATION_DATA.MACD_SIGNAL];

            var prevCandle = priceTable[timeIdx + 1];
            var prevMacd = prevCandle.calc_[(int) EVALUATION_DATA.MACD];
            var prevSignal = prevCandle.calc_[(int) EVALUATION_DATA.MACD_SIGNAL];

            if (prevMacd < prevSignal) {
                if (nowMacd > nowSignal) {
                    return true;
                }
            }

            return false;
        }

        protected bool sellSignal(List<CandleData> priceTable, int timeIdx)
        {
            if (priceTable.Count < TERM) {
                return false;
            }
            var nowCandle = priceTable[timeIdx];
            var nowMacd = nowCandle.calc_[(int) EVALUATION_DATA.MACD];
            var nowSignal = nowCandle.calc_[(int) EVALUATION_DATA.MACD_SIGNAL];

            var prevCandle = priceTable[timeIdx + 1];
            var prevMacd = prevCandle.calc_[(int) EVALUATION_DATA.MACD];
            var prevSignal = prevCandle.calc_[(int) EVALUATION_DATA.MACD_SIGNAL];

            if (prevMacd > prevSignal) {
                if (nowMacd < nowSignal) {
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
            if (this.buySignal(priceTable, timeIdx)) {
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
            if (this.sellSignal(priceTable, timeIdx)) {
                return true;
            }
            return false;
        }
    }
}
