using StockLibrary.StrategyManager.Trade;
using System;

// 지금 테스트 해보니
// 무제한일때 최장 17일 들고 있었지만 그나마? 안정적인 수익율을 보여줌
// 일일로 하면 전혀 수익을 못냄

namespace StockLibrary.StrategyManager.StrategyModuler
{
    //https://blog.naver.com/chartist/30108478692
    public class GoldenCrossStrategyModule: StrategyModule
    {
        const int NEED = 1;     // 이거 1로 해야 함. 2,3 으로 하는 순간 매수 / 매도 신호가 바뀌면서 실제 매매 타이밍을 놓침.
        TradeStrategy goldenCross_ = null;
        TradeStrategy candleUp_ = null;
        TradeStrategy payOffCross_ = null;
        Calculater maCalculater_ = null;
        public GoldenCrossStrategyModule(Bot bot) : base(bot)
        {
            assetBettor_ = new MAAssetBettor();
            this.setMA(EVALUATION_DATA.EMA_10, EVALUATION_DATA.EMA_50, EVALUATION_DATA.EMA_200);
        }

        protected override void initCacluaterList()
        {
            calculaterList_.Clear();
            switch (bFast_) {
                case EVALUATION_DATA.EMA_3:
                case EVALUATION_DATA.EMA_5:
                case EVALUATION_DATA.EMA_10:
                case EVALUATION_DATA.EMA_20:
                case EVALUATION_DATA.EMA_50:
                case EVALUATION_DATA.EMA_100:
                case EVALUATION_DATA.EMA_200:
                    maCalculater_ = new EMACalculater();
                    break;
                case EVALUATION_DATA.WMA_3:
                case EVALUATION_DATA.WMA_5:
                case EVALUATION_DATA.WMA_10:
                case EVALUATION_DATA.WMA_20:
                case EVALUATION_DATA.WMA_50:
                case EVALUATION_DATA.WMA_100:
                case EVALUATION_DATA.WMA_200:
                    maCalculater_ = new WMACalculater();
                    break;
                default:
                    maCalculater_ = new MACalculater();
                    break;
            }   
            calculaterList_.Add(maCalculater_);
        }

        EVALUATION_DATA bFast_, bSlow_, bUpper_;
        EVALUATION_DATA pFast_, pSlow_;
        public void setMA(EVALUATION_DATA bFast, EVALUATION_DATA bSlow, EVALUATION_DATA upper)
        {
            pFast_ = bFast_ = bFast;
            pSlow_ = bSlow_ = bSlow;
            bUpper_ = upper;

            tradeStrategyPool_.Clear();
            goldenCross_ = new MaGoldCrossTradeStrategy(bFast_, bSlow_);
            payOffCross_ = new MaGoldCrossTradeStrategy(bFast_, bSlow_);
            candleUp_ = new CandleUpTradeStrategy(2);

            this.addTradeStrategy(goldenCross_);
            this.addTradeStrategy(new MaTrendTradeStrategy(bUpper_));
            this.addTradeStrategy(candleUp_);
            this.initCacluaterList();
        }

        public void setMA(EVALUATION_DATA bFast, EVALUATION_DATA bSlow, EVALUATION_DATA upper, EVALUATION_DATA pFast, EVALUATION_DATA pSlow)
        {
            this.setMA(bFast, bSlow, upper);
            pFast_ = pFast;
            pSlow_ = pSlow;

            payOffCross_ = new MaGoldCrossTradeStrategy(pFast_, pSlow_);
        }

        public override string name()
        {
            return string.Format("mode[{0}]: trade[{1}/{2}], up[{3}], payoffCross[{4}/{5}], better[{6}], myShare[{7}]",
                tradeLongShort_, bFast_, bSlow_, bUpper_, pFast_, pSlow_, this.getBettorName(), PublicFutureVar.myShareRate);
        }

        public override string dbString()
        {
            var att = string.Format("{0}, {1}, {2}, {3}, {4}, {5}", bFast_, bSlow_, bUpper_, pFast_, pSlow_, tradeLongShort_);
            return att;
        }

        public override void parseDBString(string dbString)
        {
            var tokens = dbString.Split(',');
            if (tokens.Length < 3) {
                return;
            }
            bFast_ = (EVALUATION_DATA) Enum.Parse(typeof(EVALUATION_DATA), tokens[0]);
            bSlow_ = (EVALUATION_DATA) Enum.Parse(typeof(EVALUATION_DATA), tokens[1]);
            bUpper_ = (EVALUATION_DATA) Enum.Parse(typeof(EVALUATION_DATA), tokens[2]);
            if (tokens.Length >= 3) {
                pFast_ = (EVALUATION_DATA) Enum.Parse(typeof(EVALUATION_DATA), tokens[3]);
                pSlow_ = (EVALUATION_DATA) Enum.Parse(typeof(EVALUATION_DATA), tokens[4]);
                tradeLongShort_ = (LONG_SHORT_TRADE) Enum.Parse(typeof(LONG_SHORT_TRADE), tokens[5]);
            }
            else {
                pFast_ = bFast_;
                pSlow_ = bSlow_;
            }
            this.setMA(bFast_, bSlow_, bUpper_, pFast_, pSlow_);
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

            if (checkBuyTrade(stockData, timeIdx) == false) {
                return false;
            }

            return true;
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

            // 크로스 넘어가면... 이건 손절;;;
            if (payOffCross_.sell(stockData, timeIdx)) {
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

            if (checkSellTrade(stockData, timeIdx) == false) {
                return false;
            }

            return true;
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

            // 크로스 넘어가면... 이건 손절;;;
            if (payOffCross_.buy(stockData, timeIdx)) {
                return true;
            }

            return false;
        }
    }


    public class GoldenCorssECStrategy_00: GoldenCrossStrategyModule
    {
        public GoldenCorssECStrategy_00(Bot bot) : base(bot)
        {
            this.setMA(EVALUATION_DATA.SMA_10, EVALUATION_DATA.SMA_200, EVALUATION_DATA.SMA_100);
            assetBettor_ = new MAAssetBettor();
        }
    }

    public class GoldenCorssECStrategy_01: GoldenCrossStrategyModule
    {
        public GoldenCorssECStrategy_01(Bot bot) : base(bot)
        {
            this.setMA(EVALUATION_DATA.SMA_3, EVALUATION_DATA.SMA_50, EVALUATION_DATA.SMA_100);
            assetBettor_ = new MAAssetBettor();
        }
    }

    public class GoldenCorssECStrategy_02: GoldenCrossStrategyModule
    {
        public GoldenCorssECStrategy_02(Bot bot) : base(bot)
        {
            this.setMA(EVALUATION_DATA.SMA_20, EVALUATION_DATA.SMA_50, EVALUATION_DATA.SMA_100);
            assetBettor_ = new MAAssetBettor();
        }
    }

    public class GoldenCorssECStrategy_03: GoldenCrossStrategyModule
    {
        public GoldenCorssECStrategy_03(Bot bot) : base(bot)
        {
            this.setMA(EVALUATION_DATA.WMA_20, EVALUATION_DATA.WMA_50, EVALUATION_DATA.WMA_100);
            assetBettor_ = new MAAssetBettor();
        }
    }

    public class GoldenCorssECStrategy_04: GoldenCrossStrategyModule
    {
        public GoldenCorssECStrategy_04(Bot bot) : base(bot)
        {
            this.setMA(EVALUATION_DATA.EMA_10, EVALUATION_DATA.EMA_50, EVALUATION_DATA.EMA_200);
            assetBettor_ = new AssetBettor();
        }
    }
}
