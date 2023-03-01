using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockLibrary.StrategyManager.Trade
{
    //------------------------------------------------------------//
    // 장기 이평선을 보고 전체 추세를 판단 한다.
    public class MaTrendTradeStrategy: TradeStrategy
    {
        public EVALUATION_DATA MA_STAND = EVALUATION_DATA.SMA_200;
        const int TERM = 3;
        public MaTrendTradeStrategy()
        {
            MA_STAND = EVALUATION_DATA.SMA_200;
        }

        public MaTrendTradeStrategy(EVALUATION_DATA ma)
        {
            MA_STAND = ma;
        }

        public override bool buy(StockData stockData, int timeIdx)
        {
            var priceTable = this.priceTable(stockData, TERM);
            if (priceTable == null) {
                return false;
            }

            for (int i = 0; i < TERM; ++i) {
                var nowCandle = priceTable[timeIdx + i];
                var ma = nowCandle.calc_[(int) MA_STAND];
                // 중간값 50 <= 평균값 0 

                if (nowCandle.centerPrice_ <= ma) {
                    return false;
                }
            }

            return true;
        }

        public override bool sell(StockData stockData, int timeIdx)
        {
            var priceTable = this.priceTable(stockData, TERM);
            if (priceTable == null) {
                return false;
            }
            for (int i = 0; i < TERM; ++i) {
                var nowCandle = priceTable[timeIdx + i];
                var ma = nowCandle.calc_[(int) MA_STAND];
                if (nowCandle.centerPrice_ >= ma) {
                    return false;
                }
            }
            return true;
        }
    }
}
