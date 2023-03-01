using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockLibrary.StrategyManager.Trade
{
    //------------------------------------------------------------//
    // 캔들이 계속 상승 / 하강중인지 판단
    public class CandleUpTradeStrategy: TradeStrategy
    {
        int TERM_ = 2;
        public CandleUpTradeStrategy(int term = 2)
        {
            TERM_ = term;
        }

        public override bool buy(StockData stockData, int timeIdx)
        {
            var priceTable = this.priceTable(stockData, TERM_);
            if (priceTable == null) {
                return false;
            }
            double body = double.MinValue;
            // 이제 이게 계속 상승 페이스인지 체크
            for (int i = TERM_; i >= 0; --i) {
                var candle = priceTable[timeIdx + i];
                if (body <= candle.heightOfCandleBody()) {
                    body = candle.heightOfCandleBody();
                }
                else {
                    return false;
                }
                if (candle.isMinusCandle()) {
                    return false;
                }
            }
            return true;
        }

        public override bool sell(StockData stockData, int timeIdx)
        {
            var priceTable = this.priceTable(stockData, TERM_);
            if (priceTable == null) {
                return false;
            }
            double body = double.MaxValue;
            // 이제 이게 계속 상승 페이스인지 체크
            for (int i = TERM_; i >= 0; --i) {
                var candle = priceTable[timeIdx + i];
                if (body >= candle.heightOfCandleBody()) {
                    body = candle.heightOfCandleBody();
                }
                else {
                    return false;
                }
                if (candle.isPlusCandle()) {
                    return false;
                }
            }
            return true;
        }
    }
}
