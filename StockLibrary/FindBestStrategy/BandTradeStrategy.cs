using System.Collections.Generic;

namespace StockLibrary.StrategyForTrade
{
    public class BandTechTradeModlue: TechTradeModlue
    {
        public virtual bool payoff(StockData stockData, int timeIdx, bool isUpper)
        {
            return true;
        }
    }

    // envelope 기준으로 가격이 밑에 있는가?
    public class EnvelopeTechTradeModlue: BandTechTradeModlue
    {
        // envelope 기준으로 가격이 밑을 크로스 상향 돌파 하는가?
        public override bool buy(StockData stockData, int timeIdx)
        {
            List<CandleData> priceTable = stockData.priceTable();
            if (priceTable == null) {
                return false;
            }
            const int TERM = 1;
            int lastTime = priceTable.Count - PublicVar.envelopeTerm - TERM;     // envelope 만들려면 25일 데이터가 필요함.
            if (lastTime < timeIdx) {
                return false;
            }
            if (timeIdx < TERM) {
                return false;
            }

            double nowPrice = priceTable[timeIdx].price_;
            double envelopePrice = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.ENVELOPE_DOWN];

            double yesterPrice = priceTable[timeIdx + TERM].price_;
            double yesterEnvelopePrice = priceTable[timeIdx + TERM].calc_[(int) EVALUATION_DATA.ENVELOPE_DOWN];

            // 어제 envelope 하단에 있다가 오늘은 envelope 돌파함
            if (yesterPrice < yesterEnvelopePrice) {
                if (nowPrice > envelopePrice) {
                    return true;
                }
            }
            return false;
        }

        public override bool sell(StockData stockData, int timeIdx)
        {
            List<CandleData> priceTable = stockData.priceTable();
            if (priceTable == null) {
                return false;
            }
            const int TERM = 1;
            int lastTime = priceTable.Count - PublicVar.envelopeTerm - TERM;     // envelope 만들려면 25일 데이터가 필요함.
            if (lastTime < timeIdx) {
                return false;
            }
            if (timeIdx < TERM) {
                return false;
            }

            double nowPrice = priceTable[timeIdx].price_;
            double envelopePrice = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.ENVELOPE_UP];

            double yesterPrice = priceTable[timeIdx + TERM].price_;
            double yesterEnvelopePrice = priceTable[timeIdx + TERM].calc_[(int) EVALUATION_DATA.ENVELOPE_UP];

            // 어제 envelope 상단에 있다가 오늘은 envelope 상단 밑으로 내려감
            if (yesterPrice > yesterEnvelopePrice) {
                if (nowPrice < envelopePrice) {
                    return true;
                }
            }
            return false;
        }

        public override bool payoff(StockData stockData, int timeIdx, bool isUpper)
        {
            List<CandleData> priceTable = stockData.priceTable();
            if (priceTable == null) {
                return false;
            }
            const int TERM = 1;
            int lastTime = priceTable.Count - PublicVar.envelopeTerm - TERM;     // envelope 만들려면 25일 데이터가 필요함.
            if (lastTime < timeIdx) {
                return false;
            }
            if (timeIdx < TERM) {
                return false;
            }

            double nowPrice = priceTable[timeIdx].price_;
            double envelopePrice = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.ENVELOPE_CENTER];

            double yesterPrice = priceTable[timeIdx + TERM].price_;
            double yesterEnvelopePrice = priceTable[timeIdx + TERM].calc_[(int) EVALUATION_DATA.ENVELOPE_CENTER];

            // 매수 포지션
            if (isUpper) {
                // low 에서 돌파시 샀으니
                if (yesterPrice < yesterEnvelopePrice) {
                    if (nowPrice > envelopePrice) {
                        return true;
                    }
                }
            }
            // 매도 포지션
            else {
                // upper 하락시 샀으니
                if (yesterPrice > yesterEnvelopePrice) {
                    if (nowPrice < envelopePrice) {
                        return true;
                    }
                }
            }
            return false;
        }
    }

    public class DonchianChannelTechTradeModlue: BandTechTradeModlue
    {
        const int TERM = 1;

        // envelope 기준으로 가격이 밑을 크로스 상향 돌파 하는가?
        public override bool buy(StockData stockData, int timeIdx)
        {
            List<CandleData> priceTable = stockData.priceTable();
            if (priceTable == null) {
                return false;
            }
            int lastTime = priceTable.Count - PublicVar.priceChannel - TERM;
            if (lastTime < timeIdx) {
                return false;
            }

            double nowPrice = priceTable[timeIdx].price_;
            double tradeVal = priceTable[timeIdx + TERM].calc_[(int) EVALUATION_DATA.PRICE_CHANNEL_DOWN];

            // 돈키언 체널 돌파시 매수
            if (nowPrice < tradeVal) {
                return true;
            }

            return false;
        }

        public override bool sell(StockData stockData, int timeIdx)
        {
            List<CandleData> priceTable = stockData.priceTable();
            if (priceTable == null) {
                return false;
            }

            int lastTime = priceTable.Count - PublicVar.priceChannel - TERM;
            if (lastTime < timeIdx) {
                return false;
            }

            double nowPrice = priceTable[timeIdx].price_;
            double tradeVal = priceTable[timeIdx + TERM].calc_[(int) EVALUATION_DATA.PRICE_CHANNEL_UP];

            // 돈키언 체널 돌파시 매도
            if (nowPrice > tradeVal) {
                return true;
            }
            return false;
        }

        public override bool payoff(StockData stockData, int timeIdx, bool isUpper)
        {
            List<CandleData> priceTable = stockData.priceTable();
            if (priceTable == null) {
                return false;
            }
            int lastTime = priceTable.Count - PublicVar.priceChannel - TERM;
            if (lastTime < timeIdx) {
                return false;
            }

            double nowPrice = priceTable[timeIdx].price_;
            double centerTradeVal = priceTable[timeIdx + TERM].calc_[(int) EVALUATION_DATA.PRICE_CHANNEL_CENTER];

            if (isUpper) {
                if (nowPrice > centerTradeVal) {
                    return true;
                }
            }
            else {
                if (centerTradeVal > nowPrice) {
                    return true;
                }
            }
            return false;
        }
    }
    public class ReverseDonchianChannelTechTradeModlue: DonchianChannelTechTradeModlue
    {
        const int TERM = 1;

        // envelope 기준으로 가격이 밑을 크로스 상향 돌파 하는가?
        public override bool buy(StockData stockData, int timeIdx)
        {
            List<CandleData> priceTable = stockData.priceTable();
            if (priceTable == null) {
                return false;
            }
            int lastTime = priceTable.Count - PublicVar.priceChannel - TERM;
            if (lastTime < timeIdx) {
                return false;
            }

            double nowPrice = priceTable[timeIdx].price_;
            double tradeVal = priceTable[timeIdx + TERM].calc_[(int) EVALUATION_DATA.PRICE_CHANNEL_UP];

            // 돈키언 체널 돌파시 매수
            if (nowPrice > tradeVal) {
                return true;
            }
            return false;
        }

        public override bool sell(StockData stockData, int timeIdx)
        {
            List<CandleData> priceTable = stockData.priceTable();
            if (priceTable == null) {
                return false;
            }

            int lastTime = priceTable.Count - PublicVar.priceChannel - TERM;
            if (lastTime < timeIdx) {
                return false;
            }

            double nowPrice = priceTable[timeIdx].price_;
            double tradeVal = priceTable[timeIdx + TERM].calc_[(int) EVALUATION_DATA.PRICE_CHANNEL_DOWN];

            // 돈키언 체널 돌파시 매도
            if (nowPrice < tradeVal) {
                return true;
            }
            return false;
        }

        public override bool payoff(StockData stockData, int timeIdx, bool isUpper)
        {
            List<CandleData> priceTable = stockData.priceTable();
            if (priceTable == null) {
                return false;
            }
            int lastTime = priceTable.Count - PublicVar.priceChannel - TERM;
            if (lastTime < timeIdx) {
                return false;
            }

            double nowPrice = priceTable[timeIdx].price_;
            double centerTradeVal = priceTable[timeIdx + TERM].calc_[(int) EVALUATION_DATA.PRICE_CHANNEL_CENTER];

            if (isUpper) {
                if (nowPrice < centerTradeVal) {
                    return true;
                }
            }
            else {
                if (centerTradeVal < nowPrice) {
                    return true;
                }
            }
            return false;
        }
    }

    // 볼린저 하단 / 상단에서 사서, 중단에 판다.
    public class BollingerTechTradeModlue: BandTechTradeModlue
    {
        protected double bollingerLower(StockData stockData, int timeIdx)
        {
            List<CandleData> priceTable = stockData.priceTable();
            if (priceTable == null) {
                return 0.0f;
            }
            if (priceTable.Count <= timeIdx) {
                return 0.0f;
            }
            return priceTable[timeIdx].calc_[(int) EVALUATION_DATA.SMA_BOLLINGER_DOWN];
        }
        protected double bollingerUpper(StockData stockData, int timeIdx)
        {
            List<CandleData> priceTable = stockData.priceTable();
            if (priceTable == null) {
                return 0.0f;
            }
            if (priceTable.Count <= timeIdx) {
                return 0.0f;
            }
            return priceTable[timeIdx].calc_[(int) EVALUATION_DATA.SMA_BOLLINGER_UP];
        }
        protected double bollingerCenter(StockData stockData, int timeIdx)
        {
            List<CandleData> priceTable = stockData.priceTable();
            if (priceTable == null) {
                return 0.0f;
            }
            if (priceTable.Count <= timeIdx) {
                return 0.0f;
            }
            return priceTable[timeIdx].calc_[(int) EVALUATION_DATA.SMA_BOLLINGER_CENTER];
        }

        protected double bollinger1Persent(StockData stockData, int timeIdx)
        {
            List<CandleData> priceTable = stockData.priceTable();
            if (priceTable == null) {
                return 0.0f;
            }
            double bollUpPrice = this.bollingerUpper(stockData, timeIdx);
            double bollLowPrice = this.bollingerLower(stockData, timeIdx);

            double value = (bollUpPrice - bollLowPrice) / 100.0f;
            return value;
        }

        // 상단 돌파시 구입
        public override bool buy(StockData stockData, int timeIdx)
        {
            List<CandleData> priceTable = stockData.priceTable();
            if (priceTable == null) {
                return false;
            }
            int lastTime = priceTable.Count - PublicVar.bollingerTerm - 1;     // 볼린져 벤드 만들려면 데이터가 필요함.
            if (lastTime < timeIdx) {
                return false;
            }

            // 볼린저선 계산 전 데이터는 제외
            double yesterTradePrice = this.bollingerUpper(stockData, timeIdx + 1);
            double yesterPrice = priceTable[timeIdx + 1].price_;

            double nowTradePrice = this.bollingerUpper(stockData, timeIdx);
            double nowPrice = priceTable[timeIdx].price_;

            // 볼린저선 상향 돌파시
            if (yesterPrice < yesterTradePrice) {
                if (nowPrice >= nowTradePrice) {
                    return true;
                }
            }

            return false;
        }
        public override bool sell(StockData stockData, int timeIdx)
        {
            List<CandleData> priceTable = stockData.priceTable();
            if (priceTable == null) {
                return false;
            }
            int lastTime = priceTable.Count - PublicVar.bollingerTerm - 1;     // 볼린져 벤드 만들려면 데이터가 필요함.
            if (lastTime < timeIdx) {
                return false;
            }

            // 하단 돌파시 매도 포지션 신호
            double yesterTradePrice = this.bollingerLower(stockData, timeIdx + 1);
            double yesterPrice = priceTable[timeIdx + 1].price_;

            double nowTradePrice = this.bollingerLower(stockData, timeIdx);
            double nowPrice = priceTable[timeIdx].price_;

            if (yesterPrice > yesterTradePrice) {
                if (nowPrice <= nowTradePrice) {
                    return true;
                }
            }
            return false;
        }

        // 가격이 볼린저 중단과 상단의 가운데 이상이면 판다.
        public override bool payoff(StockData stockData, int timeIdx, bool isUpper)
        {
            List<CandleData> priceTable = stockData.priceTable();
            if (priceTable == null) {
                return false;
            }
            int lastTime = priceTable.Count - PublicVar.bollingerTerm;     // 볼린져 벤드 만들려면 데이터가 필요함.
            if (lastTime < timeIdx) {
                return false;
            };

            double yesterTradePrice = this.bollingerCenter(stockData, timeIdx + 1);
            double yesterPrice = priceTable[timeIdx + 1].price_;

            double nowTradePrice = this.bollingerCenter(stockData, timeIdx);
            double nowPrice = priceTable[timeIdx].price_;

            // 매수포지션
            if (isUpper) {
                if (yesterPrice > yesterTradePrice) {
                    if (nowPrice <= nowTradePrice) {
                        return true;
                    }
                }
            }
            //매도 포지션
            else {
                if (yesterPrice < yesterTradePrice) {
                    if (nowPrice >= nowTradePrice) {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool isBandBigger(StockData stockData, int timeIdx)
        {
            List<CandleData> priceTable = stockData.priceTable();
            if (priceTable == null) {
                return false;
            }
            int lastTime = priceTable.Count - PublicVar.bollingerTerm - 1;     // 볼린져 벤드 만들려면 데이터가 필요함.
            if (lastTime < timeIdx) {
                return false;
            }

            double yesterWitdh = priceTable[timeIdx + 1].calc_[(int) EVALUATION_DATA.SMA_BOLLINGER_UP]
                                - priceTable[timeIdx + 1].calc_[(int) EVALUATION_DATA.SMA_BOLLINGER_DOWN];
            double todayWidth = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.SMA_BOLLINGER_UP]
                                - priceTable[timeIdx].calc_[(int) EVALUATION_DATA.SMA_BOLLINGER_DOWN];

            if ((yesterWitdh * 1.5f) < todayWidth) {
                return true;
            }
            return false;
        }

    }
}
