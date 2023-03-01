using StockLibrary.StrategyManager.Trade;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilLibrary;

namespace StockLibrary.StrategyManager.StrategyModuler
{
    // 이 전략의 아이디어.
    //https://stock79.tistory.com/172
    //https://stock79.tistory.com/234
    //https://blog.naver.com/chartist/30101139870
    public class PriceChanelStrategyModule: StrategyModule
    {
        TradeStrategy candleUp_ = null;
        TradeStrategy priceChanel_ = null;
        TradeStrategy maUp_ = null;

        public PriceChanelStrategyModule(Bot bot) : base(bot)
        {
            assetBettor_ = new MAAssetBettor();

            candleUp_ = new CandleUpTradeStrategy(2);
            priceChanel_ = new PriceChanelTradeStrategy();
            maUp_ = new MaTrendTradeStrategy(EVALUATION_DATA.SMA_100);
            
            this.addTradeStrategy(priceChanel_);
            this.addTradeStrategy(candleUp_);
            this.addTradeStrategy(maUp_);

            this.setMACaculater();
        }

        void setMACaculater()
        {
            calculaterList_.Clear();
            calculaterList_.Add(new PriceChannelCalculater());
        }

        public override string name()
        {
            return string.Format("up[{0}]", candleUp_);
        }

        protected override bool buy(StockData stockData, int timeIdx = 1)
        {
            if (timeIdx == -1) {
                timeIdx = 캔들완성_IDX;
            }

            var priceTable = this.priceTable(stockData, 1);
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
            var priceTable = this.priceTable(stockData, 1);
            if (priceTable == null) {
                return false;
            }

            var min = bot_.priceTypeMin();
            if (stockData.positionHaveMin() >= (min * 3)) {
                return true;
            }

            return false;
        }

        protected override bool sell(StockData stockData, int timeIdx = 1)
        {
            if (timeIdx == -1) {
                timeIdx = 캔들완성_IDX;
            }

            var priceTable = this.priceTable(stockData, 1);
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
            var priceTable = this.priceTable(stockData, 1);
            if (priceTable == null) {
                return false;
            }

            var min = bot_.priceTypeMin();
            if (stockData.positionHaveMin() >= (min * 3)) {
                return true;
            }

            return false;
        }
    }
}
