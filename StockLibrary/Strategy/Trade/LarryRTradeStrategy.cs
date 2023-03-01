using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockLibrary.StrategyManager.Trade
{
    //------------------------------------------------------------//
    // 추세 돌파
    public class LarryRTradeStrategy: TradeStrategy
    {
        protected const int TERM = 400;
        protected const int SCOPE_TERM = 20;

        protected const double LOSTCUT_RATE = 0.05f;
        protected const double ALLOW_BUY = 70;
        protected const double ALLOW_SELL = 30;

        protected double getNoice(List<CandleData> priceTable, int timeIdx)
        {
            double kSum = 0;
            for (int i = 1; i <= SCOPE_TERM; i++) {
                var candle = priceTable[timeIdx + i];
                var start = candle.startPrice_;
                var end = candle.price_;
                var high = candle.highPrice_;
                var low = candle.lowPrice_;
                if (high != low) {
                    var k = 1 - (Math.Abs(start - end) / (high - low));
                    kSum += k;
                }
            }

            var kAvg = (kSum / SCOPE_TERM);
            if (double.IsNaN(kAvg) || double.IsInfinity(kAvg)) {
                return double.MaxValue;
            }
            return kAvg;
        }
        protected double getAvgHight(List<CandleData> priceTable, int timeIdx)
        {
            double kSum = 0;
            for (int i = 1; i <= SCOPE_TERM; i++) {
                var candle = priceTable[timeIdx + i + 0];
                var k = candle.height();
                kSum += k;
            }

            var kAvg = (kSum / SCOPE_TERM);
            if (double.IsNaN(kAvg) || double.IsInfinity(kAvg)) {
                return double.MaxValue;
            }
            return kAvg;
        }

        public override bool buy(StockData stockData, int timeIdx)
        {
            var priceTable = this.priceTable(stockData, TERM);
            if (priceTable == null) {
                return false;
            }

            var scopeAvg = this.getNoice(priceTable, timeIdx);
            if (scopeAvg == double.MaxValue) {
                return false;
            }

            // 지금 오르는 상승폭이 올라가 있어야함.
            var nowCandle = priceTable[timeIdx];
            if (nowCandle.candleClosePosition() < ALLOW_BUY) {
                return false;
            }

            var height = nowCandle.height();
            var stand = nowCandle.startPrice_ + (height * scopeAvg);            //(height + scopeAvg);

            var nowPrice = nowCandle.price_;
            if (nowPrice > stand) {
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

            var scopeAvg = this.getNoice(priceTable, timeIdx);
            if (scopeAvg == double.MaxValue) {
                return false;
            }

            // 지금 오르는 상승폭이 올라가 있어야함.
            var nowCandle = priceTable[timeIdx];
            if (nowCandle.candleClosePosition() > ALLOW_SELL) {
                return false;
            }

            var height = nowCandle.height();
            var stand = nowCandle.startPrice_ - (height * scopeAvg);            //(height + scopeAvg);

            var nowPrice = nowCandle.price_;
            if (nowPrice < stand) {
                return true;
            }
            return false;
        }
    }
}
