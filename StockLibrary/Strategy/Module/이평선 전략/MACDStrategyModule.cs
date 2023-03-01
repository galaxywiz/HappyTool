using StockLibrary.Strategy.Trade;
using StockLibrary.StrategyManager.Trade;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace StockLibrary.StrategyManager.StrategyModuler
{
    public class MACDStrategyModule: StrategyModule
    {
        const int NEED = 3;
        TradeStrategy candleUp_ = null;
        TradeStrategy macd_ = null;
        public MACDStrategyModule(Bot bot) : base(bot)
        {
            assetBettor_ = new MAAssetBettor();
            calculaterList_.Add(new MACDCalculater());
           
            candleUp_ = new CandleUpTradeStrategy(3);
            macd_ = new MACDTradeStrategy();
           
            tradeStrategyPool_.Clear();
            this.addTradeStrategy(macd_);
            this.addTradeStrategy(candleUp_);
        }
        public override string name()
        {
            return string.Format("macd [{0}], signal [{1}], oscil [{2}], better [{3}]",
                PublicVar.macdDay[0], PublicVar.macdDay[1], PublicVar.macdDay[2], this.getBettorName());
        }

        public override string dbString()
        {
            var att = string.Format("None");
            return att;
        }

        public override void parseDBString(string dbString)
        {
            var tokens = dbString.Split(',');
            if (tokens.Length < 3) {
                return;
            }
        }

        protected override bool buy(StockData stockData, int timeIdx = 1)
        {
            if (this.tradeLongShort_ == LONG_SHORT_TRADE.ONLY_SHORT) {
                return false;
            }
            if (timeIdx == -1) {
                timeIdx = 캔들완성_IDX;
            }

            var priceTable = this.priceTable(stockData, NEED);
            if (priceTable == null) {
                return false;
            }

            if (this.checkBuyTrade(stockData, timeIdx)) {
                return true;
            }
           
            return false;
        }

        public override bool buyPayOffAtCompleteCandle(StockData stockData, int timeIdx = -1)
        {
            if (timeIdx == -1) {
                timeIdx = 캔들완성_IDX;
            }
            var priceTable = this.priceTable(stockData, NEED);
            if (priceTable == null) {
                return false;
            }

            if (macd_.sell(stockData, timeIdx)) {
                return true;
            }

            return false;
        }

        protected override bool sell(StockData stockData, int timeIdx = 1)
        {
            if (this.tradeLongShort_ == LONG_SHORT_TRADE.ONLY_LONG) {
                return false;
            }
            if (timeIdx == -1) {
                timeIdx = 캔들완성_IDX;
            }

            var priceTable = this.priceTable(stockData, NEED);
            if (priceTable == null) {
                return false;
            }

            if (this.checkSellTrade(stockData, timeIdx)) {
                return true;
            }
           
            return false;
        }

        public override bool sellPayOffAtCompleteCandle(StockData stockData, int timeIdx = -1)
        {
            if (timeIdx == -1) {
                timeIdx = 캔들완성_IDX;
            }
            var priceTable = this.priceTable(stockData, NEED);
            if (priceTable == null) {
                return false;
            }

            if (macd_.buy(stockData, timeIdx)) {
                return true;
            }

            return false;
        }
    }
}
