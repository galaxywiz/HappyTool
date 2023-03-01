using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;

namespace StockLibrary.StrategyManager.Trade
{
    public abstract class TradeStrategy
    {
        protected int 캔들완성_IDX = PublicVar.캔들완성_IDX;
        protected int 캔들생성_IDX = 0;

        public abstract bool buy(StockData stockData, int timeIdx);
        public abstract bool sell(StockData stockData, int timeIdx);
        public bool buy(StockData stockData)
        {
            return buy(stockData, 캔들완성_IDX);
        }
        public bool sell(StockData stockData)
        {
            return sell(stockData, 캔들완성_IDX);
        }

        protected List<CandleData> priceTable(StockData stockData, int term = 0)
        {
            List<CandleData> priceTable = stockData.priceTable();
            if (priceTable == null) {
                return null;
            }
            int lastTime = priceTable.Count - term;
            if (lastTime < 0) {
                return null;
            }
            return priceTable;
        }
    }

    //------------------------------------------------------------//
    // 지지 저항선 찾기
    public class SupportAndResistanceFilter: TradeStrategy
    {
        public override bool buy(StockData stockData, int timeIdx)
        {
            var priceTable = this.priceTable(stockData);
            if (priceTable == null) {
                return false;
            }
            var nowPrice = priceTable[timeIdx].price_;
            var prevPrice = priceTable[timeIdx].centerPrice_;

            // 지난번 종가보다 커야 한다. 즉, 바닥이 확인될때 까지 진입 금지.
            if (prevPrice < nowPrice) {
                return true;
            }
            return false;
        }

        public override bool sell(StockData stockData, int timeIdx)
        {
            var priceTable = this.priceTable(stockData);
            if (priceTable == null) {
                return false;
            }
            var nowPrice = priceTable[timeIdx].price_;
            var prevPrice = priceTable[timeIdx].centerPrice_;

            // 지난번 종가보다 작아야 들어감. 즉, 저항이 확인될때 까지 기다림.
            if (nowPrice < prevPrice) {
                return true;
            }

            return false;
        }
    }

    //------------------------------------------------------------//
    // 저항선 돌파 체크
    public class ResistanceLineOverFilter: TradeStrategy
    {
        protected const int TERM = 15;

        public override bool buy(StockData stockData, int timeIdx)
        {
            var priceTable = this.priceTable(stockData);
            if (priceTable == null) {
                return false;
            }
            var nowPrice = priceTable[timeIdx].price_;
            double highMax = double.MinValue;
            for (int i = 1; i < TERM; ++i) {
                var high= priceTable[timeIdx].centerPrice_;
                if (high > highMax) {
                    highMax = high;
                }
            }

            // 뒤의 25개 봉보다 고점을 찍으면 매수
            if (highMax < nowPrice) {
                return true;
            }
            return false;
        }

        public override bool sell(StockData stockData, int timeIdx)
        {
            var priceTable = this.priceTable(stockData);
            if (priceTable == null) {
                return false;
            }
            var nowPrice = priceTable[timeIdx].price_;
            double lowMax = double.MaxValue;
            for (int i = 1; i < TERM; ++i) {
                var low = priceTable[timeIdx].centerPrice_;
                if (low < lowMax) {
                    lowMax = low;
                }
            }

            // 뒤의 20개 봉보다 저점을 찍으면 매도
            if (lowMax > nowPrice) {
                return true;
            }
            return false;
        }
    }

    //------------------------------------------------------------//
    // 캔들이 양봉인지, 음봉인지
    public class CandleFilter: TradeStrategy
    {
        public override bool buy(StockData stockData, int timeIdx)
        {
            var priceTable = this.priceTable(stockData);
            if (priceTable == null) {
                return false;
            }
            var nowCanlde = priceTable[timeIdx];
            if (nowCanlde.isPlusCandle()) {
                return true;
            }
            return false;
        }

        public override bool sell(StockData stockData, int timeIdx)
        {
            var priceTable = this.priceTable(stockData);
            if (priceTable == null) {
                return false;
            }
            var nowCanlde = priceTable[timeIdx];
            if (nowCanlde.isMinusCandle()) {
                return true;
            }
            return false;
        }
    }

    public class CandleVolumeFilter: TradeStrategy
    {
        public double VOL_TIME = 1.3f;
        public override bool buy(StockData stockData, int timeIdx)
        {
            var priceTable = this.priceTable(stockData);
            if (priceTable == null) {
                return false;
            }
            var nowCandle = priceTable[timeIdx];
            var prevCandle = priceTable[timeIdx + 1];
            if (prevCandle.volume_ == 0) {
                return false;
            }
            if (prevCandle.volume_ * VOL_TIME < nowCandle.volume_) {
                return true;
            }
            return false;
        }

        public override bool sell(StockData stockData, int timeIdx)
        {
            return buy(stockData);
        }
    }
   

    //------------------------------------------------------------//
    public class HitTheBullsEyeFilter: TradeStrategy
    {
        public override bool buy(StockData stockData, int timeIdx)
        {
            List<CandleData> priceTable = stockData.priceTable();
            const int HIGH_RANGE_LEN = 5;
            //[매수 Setup조건]
            //1) 현재봉 고가가 최근 5개봉 고가 중 최고가 
            var nowCandle = priceTable[timeIdx];
            for (int i = timeIdx + 1; i < timeIdx + HIGH_RANGE_LEN; ++i) {
                var candle = priceTable[i];
                if (nowCandle.highPrice_ <= candle.highPrice_) {
                    return false;
                }
            }

            const int CLOSE_RANGE_LEN = 2;
            //2) 현재봉 종가가 2개 전봉 고가보다 크다   
            for (int i = timeIdx + 1; i <= timeIdx + CLOSE_RANGE_LEN; ++i) {
                var candle = priceTable[i];
                if (nowCandle.price_ <= candle.highPrice_) {
                    return false;
                }
            }

            //3) 현재봉의 50지수이동평균선이 전봉의 50지수이동평균선 보다 크다.
            var prevCandle = priceTable[timeIdx + 1];
            var prevEma50 = prevCandle.calc_[(int) EVALUATION_DATA.EMA_50];
            var ema50 = nowCandle.calc_[(int) EVALUATION_DATA.EMA_50];
            if (prevEma50 >= ema50) {
                return false;
            }

            return true;
        }

        public override bool sell(StockData stockData, int timeIdx)
        {
            List<CandleData> priceTable = stockData.priceTable();
            const int HIGH_RANGE_LEN = 5;
            //[매도 Setup조건]
            //1) 현재봉 저가 최근 5개봉 저가 중 최저가 
            var nowCandle = priceTable[timeIdx];
            for (int i = timeIdx + 1; i < timeIdx + HIGH_RANGE_LEN; ++i) {
                var candle = priceTable[i];
                if (nowCandle.lowPrice_ >= candle.lowPrice_) {
                    return false;
                }
            }

            const int CLOSE_RANGE_LEN = 2;
            //2) 현재봉 종가가 2개 전봉 저가보다 작다
            for (int i = timeIdx + 1; i <= timeIdx + CLOSE_RANGE_LEN; ++i) {
                var candle = priceTable[i];
                if (nowCandle.price_ >= candle.lowPrice_) {
                    return false;
                }
            }

            //3) 현재봉의 50지수이동평균선이 전봉의 50지수이동평균선 보다 작다.
            var prevCandle = priceTable[timeIdx + 1];
            var prevEma50 = prevCandle.calc_[(int) EVALUATION_DATA.EMA_50];
            var ema50 = nowCandle.calc_[(int) EVALUATION_DATA.EMA_50];
            if (prevEma50 <= ema50) {
                return false;
            }

            return true;
        }
    }

    //------------------------------------------------------------//
    // RSI 수치가 상승 / 하강인지 체크
    public class RSIFilter: TradeStrategy
    {
        const int PREV_IDX = 5;
        const int ALLOW_IDX = 3;
        const int STAND = 50;

        public override bool buy(StockData stockData, int timeIdx)
        {
            List<CandleData> priceTable = stockData.priceTable();
            if (priceTable == null) {
                return false;
            }
            if (priceTable.Count < PREV_IDX) {
                return false;
            }

            for (int i = timeIdx; i < timeIdx + PREV_IDX; ++i) {
                var rsi = priceTable[i].calc_[(int) EVALUATION_ITEM.RSI];
                if (rsi < STAND) {
                    return false;
                }
            }
            //과거 n개 분동이 모두 50 넘었으면
            if (ALLOW_IDX > 0) {
                int idx = ALLOW_IDX;
                var prevRsi = priceTable[idx].calc_[(int) EVALUATION_ITEM.RSI];
                for (; idx >= 0; --idx) {
                    var rsi = priceTable[idx].calc_[(int) EVALUATION_ITEM.RSI];

                    // rsi 가 상승 추세가 아니면 false
                    if (prevRsi > rsi) {
                        return false;
                    }
                }
            }
            return true;
        }

        public override bool sell(StockData stockData, int timeIdx)
        {
            List<CandleData> priceTable = stockData.priceTable();
            if (priceTable == null) {
                return false;
            }
            if (priceTable.Count < PREV_IDX) {
                return false;
            }

            for (int i = timeIdx; i < timeIdx + PREV_IDX; ++i) {
                var rsi = priceTable[i].calc_[(int) EVALUATION_ITEM.RSI];
                if (rsi > STAND) {
                    return false;
                }
            }
            if (ALLOW_IDX > 0) {
                int idx = ALLOW_IDX;
                var prevRsi = priceTable[idx].calc_[(int) EVALUATION_ITEM.RSI];
                for (; idx >= 0; --idx) {
                    var rsi = priceTable[idx].calc_[(int) EVALUATION_ITEM.RSI];

                    // rsi 가 하강 추세가 아니면 false
                    if (prevRsi < rsi) {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
