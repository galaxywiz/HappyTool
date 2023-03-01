using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockLibrary.StrategyManager.Trade
{
    class PriceChanelTradeStrategy: TradeStrategy
    {
        const int TERM = 3;
        public PriceChanelTradeStrategy()
        {
        }

        protected bool buySignal(List<CandleData> priceTable, int timeIdx)
        {
            if (priceTable.Count < TERM) {
                return false;
            }
            var nowCandle = priceTable[timeIdx];
            var nowPrice = nowCandle.price_;
            var channel = nowCandle.calc_[(int) EVALUATION_DATA.PRICE_CHANNEL_UP];

            if (channel < nowPrice) {
                return true;
            }
            return false;
        }

        protected bool sellSignal(List<CandleData> priceTable, int timeIdx)
        {
            if (priceTable.Count < TERM) {
                return false;
            }
            var nowCandle = priceTable[timeIdx];
            var nowPrice = nowCandle.price_;
            var channel = nowCandle.calc_[(int) EVALUATION_DATA.PRICE_CHANNEL_DOWN];

            if (channel > nowPrice) {
                return true;
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
