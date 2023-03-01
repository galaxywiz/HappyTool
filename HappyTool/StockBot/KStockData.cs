using HappyTool.Stock;
using StockLibrary;
using System;
using UtilLibrary;

namespace HappyTool
{
    // 한국 주식용 데이터.. 그냥 주식용 데이터임.
    public class KStockData :StockData
    {
        public DateTime buyTime_ { get; set; }
        public bool sellOffReady_ = false;
        public bool wasBuyed_ = false;

        public KStockData(string code, string name) : base(code, name)
        {
            resetBuyInfo();
        }

        public override Object Clone()
        {
            KStockData clone = new KStockData(code_, name_);
            StockData copy = (StockData) clone;
            this.copyPriceDatas(ref copy);

            clone.buyCount_ = buyCount_;
            clone.buyPrice_ = buyPrice_;
            return clone;
        }

        public TRADING_STATUS position_ = TRADING_STATUS.모니터닝;
        public override Boolean isBuyedItem()
        {
            if (position_ == TRADING_STATUS.모니터닝) {
                return false;
            }
            return true;
        }

        public override void resetBuyInfo()
        {
            position_ = TRADING_STATUS.모니터닝;

            sellOffReady_ = false;

            minProfitRate_ = 10000;
            maxProfitRate_ = -10000;
            isOredered_ = false;
        }

        public double minProfitRate_ = 10000;
        public double maxProfitRate_ = -10000;           
        public override void updateProfitPrice()
        {
            base.updateProfitPrice();
            var bot = ControlGet.getInstance.stockBot();
            this.makeYesterdayCandle(bot);
            maxProfitRate_ = Math.Max(maxProfitRate_, this.nowProfitRate());
            minProfitRate_ = Math.Min(minProfitRate_, this.nowProfitRate());
        }

        public bool isOredered_ = false;
        public void setBuyInfo(string count, string buyPrice)
        {
            if (int.TryParse(count, out buyCount_) == false) {
                return;
            }
            isOredered_ = false;
            if (buyCount_ == 0) {
                position_ = TRADING_STATUS.모니터닝;
                return;
            }
            if (double.TryParse(buyPrice, out buyPrice_) == false) {
                return;
            }
            position_ = TRADING_STATUS.매수;
            buyTime_ = this.nowDateTime();
            if (buyTime_ == DateTime.MinValue) {
                buyTime_ = DateTime.Now;
            }
            lastChejanDate_ = nowDateTime();
        }

        public override string logExpectedRange(string currenySymbol = "원")
        {
            return base.logExpectedRange(currenySymbol);
        }

        public override BackTestRecoder newBackTestRecoder()
        {
            return new StockBackTestRecoder();
        }

        public double nowOnePureProfit(int buyCount = 1)
        {
            double now = this.nowPrice();
            if (now == 0) {
                return 0;
            }
            double nowPrice = (double) now;
            var rate = UtilLibrary.Util.calcProfitRate(buyPrice_, nowPrice);

            /*
            실질 수익금 = 수익금 - (매수수수료 + 매도수수료 + 세금)
            ------------------------------------------------------------------
            ※ 매수 수수료 = 0.015 %
            ※ 매도 수수료 = 0.015 %
            ※ 세금 = 0.3 %
            */
            const double FEE_TEX = 0.015 / 100;
            const double TEX = 0.3 / 100;

            var ret = this.nowOneProfit() * buyCount - (((buyPrice_ * buyCount) * FEE_TEX) + ((now * buyCount) * FEE_TEX) + ((now * buyCount) * TEX));
            return ret;
        }

        public double nowPureProfit()
        {
            return this.nowOnePureProfit(buyCount_);
        }

        public override double nowProfit()
        {
            return this.nowOneProfit() * buyCount_;
        }

        public override double nowOneProfit()
        {
            return this.nowPrice() - buyPrice_;
        }

        public override double lostCutProfit()
        {
            var lostCut = this.buyPrice_ - (this.buyPrice_ * PublicVar.loseCutRate);
            if (lostCut > 0) {
                lostCut = -lostCut;
            }
            return lostCut;
        }

        public override double targetProfit()
        {
            var targetProfit = this.buyPrice_ + (this.buyPrice_ * PublicVar.profitRate);
            return targetProfit;
        }

        public DateTime dataReceivedTime_;
        protected override void sortPrice()
        {
            if (priceTable_.Count == 0) {
                return;
            }
            base.sortPrice();
            dataReceivedTime_ = DateTime.Now;
        }

        public double totalBuyPrice()
        {
            return buyCount_ * buyPrice_;
        }

        // rate 는 1이 100% 임
        public double nowProfitRate()
        {
            double now = this.nowPrice();
            if (now == 0) {
                return 0;
            }
            double profit = this.nowPureProfit();
            if (profit == 0) {
                return 0;
            }
            var rate = profit / totalBuyPrice();
            return rate;
        }

        public bool isSellTime(Bot bot)
        {
            return this.isSellTime(bot, buyPrice_);
        }
    }
}
