using System;
using System.Collections.Generic;
using TicTacTec.TA.Library;

namespace StockLibrary
{
    //인베스팅 쪽 기술적 지표를 참고해서 모두 구현을 시킨다.
    //http://kr.investing.com/equities/hyundai-motor-technical

    //나중엔 여기랑도 연결을..
    //https://www.quantconnect.com/forum/discussion/416/indicator-examples-bb-macd-sma-ema-rsi-atr

    // TA 라이브러리 기본 파라메터 설명은 요기서
    //https://mrjbq7.github.io/ta-lib/
    public abstract class Calculater
    {
        public abstract void calculate(ref List<CandleData> priceTable);
        public abstract bool buySignal(List<CandleData> priceTable);
        public abstract bool sellSignal(List<CandleData> priceTable);
    }

    public class MACalculater: Calculater
    {
        readonly int[] period_ = PublicVar.avgMax;
        protected EVALUATION_DATA signal_ = EVALUATION_DATA.SMA_100;
        public double[] calcAvg(List<CandleData> priceTable, Core.MAType type, int avgTime, out int outNBElement)
        {
            List<double> endmt = new List<double>();
            outNBElement = 0;

            for (int i = priceTable.Count - 1; i >= 0; --i) {
                CandleData candle = priceTable[i];
                endmt.Add(candle.avgPrice());
            }

            double[] maOutput = new double[priceTable.Count];
            int outBegIdx;
            switch (type) {
                case Core.MAType.Sma:
                    Core.Sma(0, endmt.Count - 1, endmt.ToArray(), avgTime, out outBegIdx, out outNBElement, maOutput);
                    break;
                case Core.MAType.Ema:
                    Core.Ema(0, endmt.Count - 1, endmt.ToArray(), avgTime, out outBegIdx, out outNBElement, maOutput);
                    break;
                case Core.MAType.Wma:
                    Core.Wma(0, endmt.Count - 1, endmt.ToArray(), avgTime, out outBegIdx, out outNBElement, maOutput);
                    break;
            }
            return maOutput;
        }

        public override void calculate(ref List<CandleData> priceTable)
        {
            int maxIndex = priceTable.Count - 1;
            int[] avgMax = PublicVar.avgMax;
            List<double> endmt = new List<double>();

            for (int i = priceTable.Count - 1; i >= 0; --i) {
                CandleData candle = priceTable[i];
                endmt.Add(candle.avgPrice());
            }
            int avgIndex = 0;
            foreach (int avgTime in avgMax) {
                int outBegIdx, outNBElement;
                double[] smaOutput = new double[priceTable.Count];
                double[] emaOutput = new double[priceTable.Count];
                double[] wmaOutput = new double[priceTable.Count];

                Core.Sma(0, endmt.Count - 1, endmt.ToArray(), avgTime, out outBegIdx, out outNBElement, smaOutput);
              
                int dataIdx = 0;
                for (int index = outNBElement - 1; index >= 0; --index) {
                    CandleData candle = priceTable[dataIdx++];
                    candle.calc_[(int) EVALUATION_DATA.SMA_3 + avgIndex] = smaOutput[index];
                }
                avgIndex++;
            }                     
        }

        public override bool buySignal(List<CandleData> priceTable)
        {
            if (priceTable.Count < 2) {
                return false;
            }
            var nowCandle = priceTable[1];
            var nowPrice = nowCandle.price_;
            var stand = nowCandle.calc_[(int) signal_];
            if (nowPrice > stand) {
                return true;
            }
            return false;
        }
        public override bool sellSignal(List<CandleData> priceTable)
        {
            if (priceTable.Count < 2) {
                return false;
            }
            var nowCandle = priceTable[1];
            var nowPrice = nowCandle.price_;
            var stand = nowCandle.calc_[(int) signal_];
            if (nowPrice < stand) {
                return true;
            }
            return false;
        }
    }

    public class EMACalculater: MACalculater
    {
        public EMACalculater()
        {
            signal_ = EVALUATION_DATA.EMA_100;
        }

        public override void calculate(ref List<CandleData> priceTable)
        {
            int maxIndex = priceTable.Count - 1;
            int[] avgMax = PublicVar.avgMax;
            List<double> endmt = new List<double>();

            for (int i = priceTable.Count - 1; i >= 0; --i) {
                CandleData candle = priceTable[i];
                endmt.Add(candle.avgPrice());
            }
            int avgIndex = 0;
            foreach (int avgTime in avgMax) {
                int outBegIdx, outNBElement;
                double[] smaOutput = new double[priceTable.Count];
                double[] emaOutput = new double[priceTable.Count];
                double[] wmaOutput = new double[priceTable.Count];

                Core.Ema(0, endmt.Count - 1, endmt.ToArray(), avgTime, out outBegIdx, out outNBElement, emaOutput);

                int dataIdx = 0;
                for (int index = outNBElement - 1; index >= 0; --index) {
                    CandleData candle = priceTable[dataIdx++];
                    candle.calc_[(int) EVALUATION_DATA.EMA_3 + avgIndex] = emaOutput[index];
                }
                avgIndex++;
            }
        }
    }

    public class WMACalculater: MACalculater
    {
        public WMACalculater()
        {
            signal_ = EVALUATION_DATA.WMA_100;
        }

        public override void calculate(ref List<CandleData> priceTable)
        {
            int maxIndex = priceTable.Count - 1;
            int[] avgMax = PublicVar.avgMax;
            List<double> endmt = new List<double>();

            for (int i = priceTable.Count - 1; i >= 0; --i) {
                CandleData candle = priceTable[i];
                endmt.Add(candle.avgPrice());
            }
            int avgIndex = 0;
            foreach (int avgTime in avgMax) {
                int outBegIdx, outNBElement;
                double[] smaOutput = new double[priceTable.Count];
                double[] emaOutput = new double[priceTable.Count];
                double[] wmaOutput = new double[priceTable.Count];

                Core.Wma(0, endmt.Count - 1, endmt.ToArray(), avgTime, out outBegIdx, out outNBElement, emaOutput);

                int dataIdx = 0;
                for (int index = outNBElement - 1; index >= 0; --index) {
                    CandleData candle = priceTable[dataIdx++];
                    candle.calc_[(int) EVALUATION_DATA.WMA_3 + avgIndex] = emaOutput[index];
                }
                avgIndex++;
            }
        }

    }

    class BollingerCalculater: Calculater
    {
        public int bollingerTerm_ = PublicVar.bollingerTerm;
        public int bollingerAnalysisTerm_ = PublicVar.bollingerAnalysisTerm;

        public override void calculate(ref List<CandleData> priceTable)
        {
            List<double> endmt = new List<double>();
            for (int i = priceTable.Count - 1; i >= 0; --i) {
                CandleData candle = priceTable[i];
                var price = candle.avgPrice();
                endmt.Add(price);
            }

            int outBegIdx, outNBElement;

            double[] outputUp = new double[priceTable.Count];
            double[] outputCenter = new double[priceTable.Count];
            double[] outputDown = new double[priceTable.Count];
            Core.MAType maType = Core.MAType.Sma;
            Core.Bbands(0, endmt.Count - 1, endmt.ToArray(), this.bollingerTerm_, 2, 2, maType, out outBegIdx, out outNBElement, outputUp, outputCenter, outputDown);

            int dataIdx = 0;
            for (int index = outNBElement - 1; index >= 0; --index) {
                CandleData candle = priceTable[dataIdx++];
                candle.calc_[(int) EVALUATION_DATA.SMA_BOLLINGER_UP] = outputUp[index];
                candle.calc_[(int) EVALUATION_DATA.SMA_BOLLINGER_CENTER] = outputCenter[index];
                candle.calc_[(int) EVALUATION_DATA.SMA_BOLLINGER_DOWN] = outputDown[index];
            }

            maType = Core.MAType.Ema;
            Core.Bbands(0, endmt.Count - 1, endmt.ToArray(), this.bollingerTerm_, 2, 2, maType, out outBegIdx, out outNBElement, outputUp, outputCenter, outputDown);

            dataIdx = 0;
            for (int index = outNBElement - 1; index >= 0; --index) {
                CandleData candle = priceTable[dataIdx++];
                candle.calc_[(int) EVALUATION_DATA.EMA_BOLLINGER_UP] = outputUp[index];
                candle.calc_[(int) EVALUATION_DATA.EMA_BOLLINGER_CENTER] = outputCenter[index];
                candle.calc_[(int) EVALUATION_DATA.EMA_BOLLINGER_DOWN] = outputDown[index];
            }

            maType = Core.MAType.Wma;
            Core.Bbands(0, endmt.Count - 1, endmt.ToArray(), this.bollingerTerm_, 2, 2, maType, out outBegIdx, out outNBElement, outputUp, outputCenter, outputDown);

            dataIdx = 0;
            for (int index = outNBElement - 1; index >= 0; --index) {
                CandleData candle = priceTable[dataIdx++];
                candle.calc_[(int) EVALUATION_DATA.WMA_BOLLINGER_UP] = outputUp[index];
                candle.calc_[(int) EVALUATION_DATA.WMA_BOLLINGER_CENTER] = outputCenter[index];
                candle.calc_[(int) EVALUATION_DATA.WMA_BOLLINGER_DOWN] = outputDown[index];
            }
        }
        public override bool buySignal(List<CandleData> priceTable)
        {
            if (priceTable.Count < 2) {
                return false;
            }
            var nowCandle = priceTable[1];
            var nowPrice = nowCandle.price_;
            var stand = nowCandle.calc_[(int) EVALUATION_DATA.EMA_BOLLINGER_CENTER] + (nowCandle.calc_[(int) EVALUATION_DATA.EMA_BOLLINGER_UP] - nowCandle.calc_[(int) EVALUATION_DATA.EMA_BOLLINGER_CENTER]);
            if (nowPrice > stand) {
                return true;
            }
            return false;
        }
        public override bool sellSignal(List<CandleData> priceTable)
        {
            if (priceTable.Count < 2) {
                return false;
            }
            var nowCandle = priceTable[1];
            var nowPrice = nowCandle.price_;
            var stand = nowCandle.calc_[(int) EVALUATION_DATA.EMA_BOLLINGER_CENTER] - (nowCandle.calc_[(int) EVALUATION_DATA.EMA_BOLLINGER_CENTER] - nowCandle.calc_[(int) EVALUATION_DATA.WMA_BOLLINGER_DOWN]);
            if (nowPrice < stand) {
                return true;
            }
            return false;
        }
    }

    class EnvelopeCalculater: Calculater
    {
        double percent_ = PublicVar.envelopePercent;
        public void setEnvelopePercent(double percent)
        {
            this.percent_ = percent;
        }

        public override void calculate(ref List<CandleData> priceTable)
        {
            int maxIndex = priceTable.Count;
            int envelopeTerm = PublicVar.envelopeTerm;

            // ***단순 macd 구하기
            for (int index = 0; index < maxIndex; ++index) {
                int sumIndexMax = index + envelopeTerm;
                if (sumIndexMax > maxIndex) {
                    break;
                }
                double sum = 0;
                for (int sumIndex = index; sumIndex < sumIndexMax; ++sumIndex) {
                    sum += priceTable[sumIndex].avgPrice();
                }
                CandleData candle = priceTable[index];
                if (envelopeTerm == 0.0f) {
                    continue;
                }
                double calcPrice = sum / envelopeTerm;
                candle.calc_[(int) EVALUATION_DATA.ENVELOPE_UP] = calcPrice + (calcPrice * this.percent_ / 100);
                candle.calc_[(int) EVALUATION_DATA.ENVELOPE_CENTER] = calcPrice;
                candle.calc_[(int) EVALUATION_DATA.ENVELOPE_DOWN] = calcPrice - (calcPrice * this.percent_ / 100);
            }
        }
        public override bool buySignal(List<CandleData> priceTable)
        {
            if (priceTable.Count < 2) {
                return false;
            }
            var nowCandle = priceTable[1];
            var nowPrice = nowCandle.price_;
            var stand = nowCandle.calc_[(int) EVALUATION_DATA.EMA_BOLLINGER_CENTER] + (nowCandle.calc_[(int) EVALUATION_DATA.EMA_BOLLINGER_UP] - nowCandle.calc_[(int) EVALUATION_DATA.EMA_BOLLINGER_CENTER]);
            if (nowPrice > stand) {
                return true;
            }
            return false;
        }
        public override bool sellSignal(List<CandleData> priceTable)
        {
            if (priceTable.Count < 2) {
                return false;
            }
            var nowCandle = priceTable[1];
            var nowPrice = nowCandle.price_;
            var stand = nowCandle.calc_[(int) EVALUATION_DATA.EMA_BOLLINGER_CENTER] - (nowCandle.calc_[(int) EVALUATION_DATA.EMA_BOLLINGER_CENTER] - nowCandle.calc_[(int) EVALUATION_DATA.WMA_BOLLINGER_DOWN]);
            if (nowPrice < stand) {
                return true;
            }
            return false;
        }
    }

    class PriceChannelCalculater: Calculater
    {
        public double period_ = PublicVar.priceChannel;

        public override void calculate(ref List<CandleData> priceTable)
        {
            int maxIndex = priceTable.Count;
            if (maxIndex <= this.period_) {
                return;
            }

            // ***단순 macd 구하기
            for (int index = 1; index < maxIndex; ++index) {
                double max = double.MinValue;
                double min = double.MaxValue;

                for (int j = 1; j < this.period_; ++j) {
                    if ((index + j) >= maxIndex) {
                        continue;
                    }
                    CandleData temp = priceTable[index + j];
                    max = Math.Max(max, temp.highPrice_);
                    min = Math.Min(min, temp.lowPrice_);
                }
                CandleData candle = priceTable[index];

                candle.calc_[(int) EVALUATION_DATA.PRICE_CHANNEL_UP] = max;
                candle.calc_[(int) EVALUATION_DATA.PRICE_CHANNEL_CENTER] = (max + min) / 2;
                candle.calc_[(int) EVALUATION_DATA.PRICE_CHANNEL_DOWN] = min;
            }
        }
        public override bool buySignal(List<CandleData> priceTable)
        {
            if (priceTable.Count < 2) {
                return false;
            }
            var nowCandle = priceTable[1];
            var nowPrice = nowCandle.price_;
            var stand = nowCandle.calc_[(int) EVALUATION_DATA.PRICE_CHANNEL_UP];
            if (nowPrice > stand) {
                return true;
            }
            return false;
        }
        public override bool sellSignal(List<CandleData> priceTable)
        {
            if (priceTable.Count < 2) {
                return false;
            }
            var nowCandle = priceTable[1];
            var nowPrice = nowCandle.price_;
            var stand = nowCandle.calc_[(int) EVALUATION_DATA.PRICE_CHANNEL_DOWN];
            if (nowPrice < stand) {
                return true;
            }
            return false;
        }
    }

    class MACDCalculater: Calculater
    {
        public int[] macdDay_ = PublicVar.macdDay;

        public override void calculate(ref List<CandleData> priceTable)
        {
            List<double> endmt = new List<double>();
            for (int i = priceTable.Count - 1; i >= 0; --i) {
                CandleData candle = priceTable[i];
                endmt.Add(candle.avgPrice());
            }
            int outBegIdx, outNBElement;
            double[] outputMacd = new double[priceTable.Count];
            double[] outputSignal = new double[priceTable.Count];
            double[] outputHist = new double[priceTable.Count];
            Core.MAType maType = Core.MAType.Ema;
            Core.MacdExt(0, endmt.Count - 1, endmt.ToArray(), 
                this.macdDay_[0], maType, 
                this.macdDay_[1], maType, 
                this.macdDay_[2], maType,
                out outBegIdx, out outNBElement, outputMacd, outputSignal, outputHist);

            int dataIdx = 0;
            for (int index = outNBElement - 1; index >= 0; --index) {
                CandleData candle = priceTable[dataIdx++];
                candle.calc_[(int) EVALUATION_DATA.MACD] = outputMacd[index];
                candle.calc_[(int) EVALUATION_DATA.MACD_SIGNAL] = outputSignal[index];
                candle.calc_[(int) EVALUATION_DATA.MACD_OSCIL] = outputHist[index];
            }
        }

        public override bool buySignal(List<CandleData> priceTable)
        {
            if (priceTable.Count < 3) {
                return false;
            }
            var nowCandle = priceTable[1];
            var nowMacd = nowCandle.calc_[(int) EVALUATION_DATA.MACD];
            var nowSignal = nowCandle.calc_[(int) EVALUATION_DATA.MACD_SIGNAL];

            var prevCandle = priceTable[2];

            if (nowMacd > nowSignal) {
                return true;
            }

            return false;
        }

        public override bool sellSignal(List<CandleData> priceTable)
        {
            if (priceTable.Count < 3) {
                return false;
            }
            var nowCandle = priceTable[1];
            var nowMacd = nowCandle.calc_[(int) EVALUATION_DATA.MACD];
            var nowSignal = nowCandle.calc_[(int) EVALUATION_DATA.MACD_SIGNAL];

            var prevCandle = priceTable[2];

            if (nowMacd < nowSignal) {
                return true;
            }
            return false;
        }
    }

    class RSI14Calculater: Calculater
    {
        // 엑셀 검증 완료
        //http://stockcharts.com/school/doku.php?id=chart_school:technical_indicators:relative_strength_index_rsi
        //http://paxnet.moneta.co.kr/WWW/html/lecture/power/ChartClass/class37.htm

        public int rsiTerm_ = PublicVar.rsiTerm;

        public override void calculate(ref List<CandleData> priceTable)
        {
            List<double> endmt = new List<double>();
            for (int i = priceTable.Count - 1; i >= 0; --i) {
                CandleData candle = priceTable[i];
                endmt.Add(candle.avgPrice());
            }
            int outBegIdx, outNBElement;
            double[] outputRSI = new double[priceTable.Count];
            double[] outputRSISignal = new double[priceTable.Count];
            Core.Rsi(0, endmt.Count - 1, endmt.ToArray(), this.rsiTerm_, out outBegIdx, out outNBElement, outputRSI);
            
            int dataIdx = 0;
            for (int index = outNBElement - 1; index >= 0; --index) {
                CandleData candle = priceTable[dataIdx++];
                candle.calc_[(int) EVALUATION_DATA.RSI] = outputRSI[index];
            }

            // 시그널 계산
            endmt.Clear();
            for (int i = priceTable.Count - 1; i >= 0; --i) {
                CandleData candle = priceTable[i];
                endmt.Add(candle.calc_[(int) EVALUATION_DATA.RSI]);
            }
            Core.Sma(0, endmt.Count - 1, endmt.ToArray(), 6, out outBegIdx, out outNBElement, outputRSISignal);
            dataIdx = 0;
            for (int index = outNBElement - 1; index >= 0; --index) {
                CandleData candle = priceTable[dataIdx++];
                candle.calc_[(int) EVALUATION_DATA.RSI_SIGNAL] = outputRSISignal[index];
            }
        }

        const double OVER_BOUGHT = 70.0f;
        const double OVER_SOLD = 30.0f;
        public override bool buySignal(List<CandleData> priceTable)
        {
            if (priceTable.Count < 3) {
                return false;
            }
            var nowCandle = priceTable[1];
            var now = nowCandle.calc_[(int) EVALUATION_DATA.RSI];

            var prevCandle = priceTable[2];
            var prev = prevCandle.calc_[(int) EVALUATION_DATA.RSI];

            if (prev < OVER_SOLD && OVER_SOLD < now) {
                return true;
            }

            return false;
        }
        public override bool sellSignal(List<CandleData> priceTable)
        {
            if (priceTable.Count < 3) {
                return false;
            }
            var nowCandle = priceTable[1];
            var now = nowCandle.calc_[(int) EVALUATION_DATA.RSI];

            var prevCandle = priceTable[2];
            var prev = prevCandle.calc_[(int) EVALUATION_DATA.RSI];
            if (prev > OVER_BOUGHT && OVER_BOUGHT > now) {
                return true;
            }
            return false;
        }
    }

    class StochasticCalculater: Calculater
    {
        //http://stockcharts.com/school/doku.php?id=chart_school:technical_indicators:stochastic_oscillator_fast_slow_and_full
        //http://how2stock.blogspot.kr/2013/06/blog-post.html
        //http://paxnet.moneta.co.kr/WWW/html/lecture/power/ChartClass/class38.htm
        //http://www.nanumtrading.com/fx-%EB%B0%B0%EC%9A%B0%EA%B8%B0/%EC%B0%A8%ED%8A%B8-%EB%B3%B4%EC%A1%B0%EC%A7%80%ED%91%9C-%EC%9D%B4%ED%95%B4/04-%EC%8A%A4%ED%86%A0%EC%BA%90%EC%8A%A4%ED%8B%B1/

        public int fastKTerm_ = PublicVar.stochN;
        public int slowKTerm_ = PublicVar.stochK;
        public int slowK_ma_ = 0;
        public int slowD_Term_ = PublicVar.stochD;
        public int slowD_ma_ = 0;

        private void calculateSlowKD(ref List<CandleData> priceTable)
        {
            List<double> highmt = new List<double>();
            List<double> lowmt = new List<double>();
            List<double> endmt = new List<double>();
            for (int i = priceTable.Count - 1; i >= 0; --i) {
                CandleData candle = priceTable[i];
                highmt.Add(candle.highPrice_);
                lowmt.Add(candle.lowPrice_);
                endmt.Add(candle.price_);
            }
            int outBegIdx, outNBElement;
            double[] outputK = new double[priceTable.Count];
            double[] outputD = new double[priceTable.Count];

            Core.MAType maType = Core.MAType.Ema;
            Core.Stoch(0, endmt.Count - 1, highmt.ToArray(), lowmt.ToArray(), endmt.ToArray()
                , this.fastKTerm_, this.slowKTerm_, maType, this.slowKTerm_, maType,
                out outBegIdx, out outNBElement, outputK, outputD);

            int dataIdx = 0;
            for (int index = outNBElement - 1; index >= 0; --index) {
                CandleData candle = priceTable[dataIdx++];
                candle.calc_[(int) EVALUATION_DATA.STOCHASTIC_K] = outputK[index];
                candle.calc_[(int) EVALUATION_DATA.STOCHASTIC_D] = outputD[index];
            }
        }

        public int stochRsiTerm_ = PublicVar.rsiTerm;
        // 엑셀 검증 완료
        private void calculateRSI14(ref List<CandleData> priceTable)
        {
            List<double> highmt = new List<double>();
            List<double> lowmt = new List<double>();
            List<double> endmt = new List<double>();
            for (int i = priceTable.Count - 1; i >= 0; --i) {
                CandleData candle = priceTable[i];
                highmt.Add(candle.highPrice_);
                lowmt.Add(candle.lowPrice_);
                endmt.Add(candle.price_);
            }
            int outBegIdx, outNBElement;
            double[] outputK = new double[priceTable.Count];
            double[] outputD = new double[priceTable.Count];

            Core.MAType maType = Core.MAType.Ema;
            Core.StochRsi(0, endmt.Count - 1, endmt.ToArray(),
                this.stochRsiTerm_, this.fastKTerm_, this.slowKTerm_, maType,
                out outBegIdx, out outNBElement, outputK, outputD);

            int dataIdx = 0;
            for (int index = outNBElement - 1; index >= 0; --index) {
                CandleData candleModify = priceTable[dataIdx++];
                candleModify.calc_[(int) EVALUATION_DATA.STOCHASTIC_RSI_K] = outputK[index];
                candleModify.calc_[(int) EVALUATION_DATA.STOCHASTIC_RSI_D] = outputD[index];
            }
        }

        public override void calculate(ref List<CandleData> priceTable)
        {
            this.calculateSlowKD(ref priceTable);
            this.calculateRSI14(ref priceTable);
        }
        const double BUY_AREA = 0.25f;
        const double SELL_AREA = 0.75f;

        public override bool buySignal(List<CandleData> priceTable)
        {
            if (priceTable.Count < 2) {
                return false;
            }
            var nowCandle = priceTable[1];
            var now = nowCandle.calc_[(int) EVALUATION_DATA.STOCHASTIC_RSI_K];

            if (now <= BUY_AREA) {
                return true;
            }

            return false;
        }
        public override bool sellSignal(List<CandleData> priceTable)
        {
            if (priceTable.Count < 3) {
                return false;
            }
            var nowCandle = priceTable[1];
            var now = nowCandle.calc_[(int) EVALUATION_DATA.STOCHASTIC_RSI_K];

            if (now >= SELL_AREA) {
                return true;
            }
            return false;
        }
    }

    //adx 지표 매매 설명
    //https://layhope.tistory.com/235

    class ADXCalculater: Calculater
    {
        // 엑셀 검증 ok
        //http://stockcharts.com/school/doku.php?id=chart_school:technical_indicators:average_directional_index_adx

        public int adxTerm_ = PublicVar.adxTerm;
        public override void calculate(ref List<CandleData> priceTable)
        {
            List<double> highmt = new List<double>();
            List<double> lowmt = new List<double>();
            List<double> endmt = new List<double>();
            for (int i = priceTable.Count - 1; i >= 0; --i) {
                CandleData candle = priceTable[i];
                highmt.Add(candle.highPrice_);
                lowmt.Add(candle.lowPrice_);
                endmt.Add(candle.price_);
            }
            int outBegIdx, outNBElement;
            double[] outputAdx = new double[priceTable.Count];
            double[] outputDi = new double[priceTable.Count];
            double[] outputPlus = new double[priceTable.Count];

            Core.Adx(0, endmt.Count - 1, highmt.ToArray(), lowmt.ToArray(), endmt.ToArray(),
                this.adxTerm_, out outBegIdx, out outNBElement, outputAdx);
            int dataIdx = 0;
            for (int index = outNBElement - 1; index >= 0; --index) {
                CandleData candle = priceTable[dataIdx++];
                candle.calc_[(int) EVALUATION_DATA.ADX] = outputAdx[index];
            }

            Core.MinusDI(0, endmt.Count - 1, highmt.ToArray(), lowmt.ToArray(), endmt.ToArray(),
               this.adxTerm_, out outBegIdx, out outNBElement, outputDi);
            dataIdx = 0;
            for (int index = outNBElement - 1; index >= 0; --index) {
                CandleData candle = priceTable[dataIdx++];
                candle.calc_[(int) EVALUATION_DATA.DI_MINUS] = outputDi[index];
            }

            Core.PlusDI(0, endmt.Count - 1, highmt.ToArray(), lowmt.ToArray(), endmt.ToArray(),
               this.adxTerm_, out outBegIdx, out outNBElement, outputPlus);
            dataIdx = 0;
            for (int index = outNBElement - 1; index >= 0; --index) {
                CandleData candle = priceTable[dataIdx++];
                candle.calc_[(int) EVALUATION_DATA.DI_PLUS] = outputPlus[index];
            }
        }

        const double ADX_VALUE = 20.0f;
        const double ADXUP_VALUE = 40.0f;
        public override bool buySignal(List<CandleData> priceTable)
        {
            if (priceTable.Count < 3) {
                return false;
            }
            var nowCandle = priceTable[1];
            double adx = nowCandle.calc_[(int) EVALUATION_DATA.ADX];

            var prevCandle = priceTable[2];
            double oldAdx = prevCandle.calc_[(int) EVALUATION_DATA.ADX];
            if (adx < ADX_VALUE
                && oldAdx < adx) {
                return true;
            }

            return false;
        }
        public override bool sellSignal(List<CandleData> priceTable)
        {
            if (priceTable.Count < 3) {
                return false;
            }
            var nowCandle = priceTable[1];
            double adx = nowCandle.calc_[(int) EVALUATION_DATA.ADX];

            var prevCandle = priceTable[2];
            double oldAdx = prevCandle.calc_[(int) EVALUATION_DATA.ADX];
            if (adx < ADXUP_VALUE
                && oldAdx > ADXUP_VALUE) {
                return true;
            }
            return false;
        }
    }

    class WilliamsCalculater: Calculater
    {
        // 엑셀 검증 ok
        //http://stockcharts.com/school/doku.php?id=chart_school:technical_indicators:williams_r
        public int williams_ = PublicVar.williams;

        public override void calculate(ref List<CandleData> priceTable)
        {
            List<double> highmt = new List<double>();
            List<double> lowmt = new List<double>();
            List<double> endmt = new List<double>();
            for (int i = priceTable.Count - 1; i >= 0; --i) {
                CandleData candle = priceTable[i];
                highmt.Add(candle.highPrice_);
                lowmt.Add(candle.lowPrice_);
                endmt.Add(candle.price_);
            }
            int outBegIdx, outNBElement;
            double[] outputWil = new double[priceTable.Count];

            Core.WillR(0, endmt.Count - 1, highmt.ToArray(), lowmt.ToArray(), endmt.ToArray(),
                this.williams_, out outBegIdx, out outNBElement, outputWil);

            int dataIdx = 0;
            for (int index = outNBElement - 1; index >= 0; --index) {
                CandleData candle = priceTable[dataIdx++];
                candle.calc_[(int) EVALUATION_DATA.WILLIAMS] = outputWil[index];
            }
        }

        const double OVER_BOUGHT = -25.0f;
        const double OVER_SOLD = -75.0f;

        public override bool buySignal(List<CandleData> priceTable)
        {
            if (priceTable.Count < 2) {
                return false;
            }
            var nowCandle = priceTable[1];
            double williamR = nowCandle.calc_[(int) EVALUATION_DATA.WILLIAMS];
            if (williamR >= OVER_BOUGHT) {
                return true;
            }

            return false;
        }
        public override bool sellSignal(List<CandleData> priceTable)
        {
            if (priceTable.Count < 2) {
                return false;
            }
            var nowCandle = priceTable[1];
            double williamR = nowCandle.calc_[(int) EVALUATION_DATA.WILLIAMS];
            if (williamR <= OVER_SOLD) {
                return true;
            }

            return false;
        }
    }

    class CCICalculater: Calculater
    {
        // 엑셀 검증 ok
        //http://stockcharts.com/school/doku.php?id=chart_school:technical_indicators:commodity_channel_index_cci

        public int cciTerm_ = PublicVar.cciTerm;

        public override void calculate(ref List<CandleData> priceTable)
        {
            List<double> highmt = new List<double>();
            List<double> lowmt = new List<double>();
            List<double> endmt = new List<double>();
            for (int i = priceTable.Count - 1; i >= 0; --i) {
                CandleData candle = priceTable[i];
                highmt.Add(candle.highPrice_);
                lowmt.Add(candle.lowPrice_);
                endmt.Add(candle.price_);
            }
            int outBegIdx, outNBElement;
            double[] outputCCI = new double[priceTable.Count];

            Core.Cci(0, endmt.Count - 1, highmt.ToArray(), lowmt.ToArray(), endmt.ToArray(),
                this.cciTerm_, out outBegIdx, out outNBElement, outputCCI);

            int dataIdx = 0;
            for (int index = outNBElement - 1; index >= 0; --index) {
                CandleData candle = priceTable[dataIdx++];
                candle.calc_[(int) EVALUATION_DATA.CCI] = outputCCI[index];
            }
        }

        const double OVER_BOUGHT = 100.0f;
        const double OVER_SOLD = -100.0f;
        public override bool buySignal(List<CandleData> priceTable)
        {
            if (priceTable.Count < 3) {
                return false;
            }
            var nowCandle = priceTable[1];
            double now = nowCandle.calc_[(int) EVALUATION_DATA.CCI];

            var prevCandle = priceTable[2];
            double prev = prevCandle.calc_[(int) EVALUATION_DATA.CCI];
            if (prev <= OVER_SOLD && OVER_SOLD < now) {
                return true;
            }
            if (prev <= OVER_BOUGHT && OVER_BOUGHT < now) {
                return true;
            }
            return false;
        }
        public override bool sellSignal(List<CandleData> priceTable)
        {
            if (priceTable.Count < 3) {
                return false;
            }
            var nowCandle = priceTable[1];
            double now = nowCandle.calc_[(int) EVALUATION_DATA.CCI];

            var prevCandle = priceTable[2];
            double prev = prevCandle.calc_[(int) EVALUATION_DATA.CCI];
            if (prev >= OVER_BOUGHT && OVER_BOUGHT > now) {
                return true;
            }
            if (prev >= OVER_SOLD && OVER_SOLD > now) {
                return true;
            }
            return false;
        }
    }

    class ATRCalculater: Calculater
    {
        //http://stockcharts.com/school/doku.php?id=chart_school:technical_indicators:average_true_range_atr
        //엑셀 검증 완료

        public int atrTerm_ = PublicVar.atrTerm;
        public override void calculate(ref List<CandleData> priceTable)
        {
            List<double> highmt = new List<double>();
            List<double> lowmt = new List<double>();
            List<double> endmt = new List<double>();
            for (int i = priceTable.Count - 1; i >= 0; --i) {
                CandleData candle = priceTable[i];
                highmt.Add(candle.highPrice_);
                lowmt.Add(candle.lowPrice_);
                endmt.Add(candle.price_);
            }
            int outBegIdx, outNBElement;
            double[] outputAtr = new double[priceTable.Count];

            Core.Atr(0, endmt.Count - 1, highmt.ToArray(), lowmt.ToArray(), endmt.ToArray(),
                this.atrTerm_, out outBegIdx, out outNBElement, outputAtr);

            int dataIdx = 0;
            for (int index = outNBElement - 1; index >= 0; --index) {
                CandleData candle = priceTable[dataIdx++];
                candle.calc_[(int) EVALUATION_DATA.ATR] = outputAtr[index];
            }

            // atr 30 값
            Core.Atr(0, endmt.Count - 1, highmt.ToArray(), lowmt.ToArray(), endmt.ToArray(),
               30, out outBegIdx, out outNBElement, outputAtr);

            dataIdx = 0;
            for (int index = outNBElement - 1; index >= 0; --index) {
                CandleData candle = priceTable[dataIdx++];
                candle.calc_[(int) EVALUATION_DATA.ATR_30] = outputAtr[index];
            }
        }
        const double OVER_BOUGHT = 100.0f;
        const double OVER_SOLD = -100.0f;
        public override bool buySignal(List<CandleData> priceTable)
        {
            if (priceTable.Count < 3) {
                return false;
            }
            return false;
        }
        public override bool sellSignal(List<CandleData> priceTable)
        {
            if (priceTable.Count < 3) {
                return false;
            }
            return false;
        }
    }

    class UltimateCalculater: Calculater
    {
        //엑셀 검증 ok
        //http://stockcharts.com/school/doku.php?id=chart_school:technical_indicators:ultimate_oscillator

        readonly int u7_ = PublicVar.ultimate7;
        readonly int u14_ = PublicVar.ultimate14;
        readonly int u28_ = PublicVar.ultimate28;

        public override void calculate(ref List<CandleData> priceTable)
        {
            List<double> highmt = new List<double>();
            List<double> lowmt = new List<double>();
            List<double> endmt = new List<double>();
            for (int i = priceTable.Count - 1; i >= 0; --i) {
                CandleData candle = priceTable[i];
                highmt.Add(candle.highPrice_);
                lowmt.Add(candle.lowPrice_);
                endmt.Add(candle.price_);
            }
            int outBegIdx, outNBElement;
            double[] output = new double[priceTable.Count];

            Core.UltOsc(0, endmt.Count - 1, highmt.ToArray(), lowmt.ToArray(), endmt.ToArray(),
                this.u7_, this.u14_, this.u28_, out outBegIdx, out outNBElement, output);

            int dataIdx = 0;
            for (int index = outNBElement - 1; index >= 0; --index) {
                CandleData candle = priceTable[dataIdx++];
                candle.calc_[(int) EVALUATION_DATA.ULTIMATE_OSCIL] = output[index];
            }
        }
        const double OVER_BOUGHT = 100.0f;
        const double OVER_SOLD = -100.0f;
        public override bool buySignal(List<CandleData> priceTable)
        {
            if (priceTable.Count < 3) {
                return false;
            }
            var nowCandle = priceTable[1];
            double now = nowCandle.calc_[(int) EVALUATION_DATA.CCI];

            var prevCandle = priceTable[2];
            double prev = prevCandle.calc_[(int) EVALUATION_DATA.CCI];
            if (prev <= OVER_SOLD && OVER_SOLD < now) {
                return true;
            }
            if (prev <= OVER_BOUGHT && OVER_BOUGHT < now) {
                return true;
            }
            return false;
        }
        public override bool sellSignal(List<CandleData> priceTable)
        {
            if (priceTable.Count < 3) {
                return false;
            }
            var nowCandle = priceTable[1];
            double now = nowCandle.calc_[(int) EVALUATION_DATA.CCI];

            var prevCandle = priceTable[2];
            double prev = prevCandle.calc_[(int) EVALUATION_DATA.CCI];
            if (prev >= OVER_BOUGHT && OVER_BOUGHT > now) {
                return true;
            }
            if (prev >= OVER_SOLD && OVER_SOLD > now) {
                return true;
            }
            return false;
        }
    }

    //class ROCCalculater: Calculater
    //{
    //    //엑셀 검증 ok
    //    //http://stockcharts.com/school/doku.php?id=chart_school:technical_indicators:rate_of_change_roc_and_momentum

    //    public int rocTerm_ = PublicVar.rocTerm;
    //    public override void calculate(ref List<CandleData> priceTable)
    //    {
    //        List<double> highmt = new List<double>();
    //        List<double> lowmt = new List<double>();
    //        List<double> endmt = new List<double>();
    //        for (int i = priceTable.Count - 1; i >= 0; --i) {
    //            CandleData candle = priceTable[i];
    //            highmt.Add(candle.highPrice_);
    //            lowmt.Add(candle.lowPrice_);
    //            endmt.Add(candle.price_);
    //        }
    //        int outBegIdx, outNBElement;
    //        double[] output = new double[priceTable.Count];

    //        Core.Roc(0, endmt.Count - 1, endmt.ToArray(),
    //            this.rocTerm_, out outBegIdx, out outNBElement, output);

    //        int dataIdx = 0;
    //        for (int index = outNBElement - 1; index >= 0; --index) {
    //            CandleData candle = priceTable[dataIdx++];
    //            candle.calc_[(int) EVALUATION_DATA.ROC] = output[index];
    //        }
    //    }
    //}

    class ParabolicSAR: Calculater
    {
        public float accelerate_ = 0.02f;
        public float maximum_ = 0.2f;
        public override void calculate(ref List<CandleData> priceTable)
        {
            List<double> highmt = new List<double>();
            List<double> lowmt = new List<double>();
            List<double> endmt = new List<double>();
            for (int i = priceTable.Count - 1; i >= 0; --i) {
                CandleData candle = priceTable[i];
                highmt.Add(candle.highPrice_);
                lowmt.Add(candle.lowPrice_);
                endmt.Add(candle.price_);
            }
            int outBegIdx, outNBElement;
            double[] output = new double[priceTable.Count];

            Core.Sar(0, endmt.Count - 1, highmt.ToArray(), lowmt.ToArray(), this.accelerate_, this.maximum_,
                 out outBegIdx, out outNBElement, output);

            int dataIdx = 0;
            for (int index = outNBElement - 1; index >= 0; --index) {
                CandleData candle = priceTable[dataIdx++];
                candle.calc_[(int) EVALUATION_DATA.PARABOLIC_SAR] = output[index];
            }
        }
        public override bool buySignal(List<CandleData> priceTable)
        {
            if (priceTable.Count < 3) {
                return false;
            }
            var nowCandle = priceTable[1];
            double now = nowCandle.calc_[(int) EVALUATION_DATA.PARABOLIC_SAR];
            double nowPrice = nowCandle.price_;

            var prevCandle = priceTable[2];
            double prev = prevCandle.calc_[(int) EVALUATION_DATA.PARABOLIC_SAR];
            double prevPrice = prevCandle.price_;

            // 파라볼라 위치가 위아래가 바뀌면 추세 전환
            if (prev > prevPrice && now < nowPrice) {
                return true;
            }
            return false;
        }

        public override bool sellSignal(List<CandleData> priceTable)
        {
            if (priceTable.Count < 3) {
                return false;
            }
            var nowCandle = priceTable[1];
            double now = nowCandle.calc_[(int) EVALUATION_DATA.PARABOLIC_SAR];
            double nowPrice = nowCandle.price_;

            var prevCandle = priceTable[2];
            double prev = prevCandle.calc_[(int) EVALUATION_DATA.PARABOLIC_SAR];
            double prevPrice = prevCandle.price_;

            // 파라볼라 위치가 위아래가 바뀌면 추세 전환
            if (prev < prevPrice && now > nowPrice) {
                return true;
            }
            return false;
        }
    }

    class BullBearCalculater: Calculater
    {
        //엑셀 검증 ok
        //https://www.tradinformed.com/how-to-calculate-the-elder-ray-technical-indicator-in-excel/
        //http://www.bullbearings.co.uk/traders.views.php?gid=5&id=753

        static readonly int term = PublicVar.bullbearTerm;
        readonly double emaFactor = 2.0f / (double) (term + 1);

        public override void calculate(ref List<CandleData> priceTable)
        {
            int maxIndex = priceTable.Count;
            maxIndex--;

            if (maxIndex < term) {
                return;
            }

            maxIndex -= term;
            double beforeEma = 0.0f;
            for (int index = maxIndex; index >= 0; --index) {
                CandleData candle = priceTable[index];

                double ema = 0.0f;
                if (index == maxIndex) {
                    for (int subIdx = index; subIdx < term + index; ++subIdx) {
                        ema += priceTable[subIdx].price_;
                    }
                    if (term == 0) {
                        continue;
                    }
                    ema /= term;
                }
                else {
                    double close = candle.price_;
                    ema = ((close - beforeEma) * this.emaFactor) + beforeEma;
                }

                candle.calc_[(int) EVALUATION_DATA.BULL_POWER] = candle.highPrice_ - ema;
                candle.calc_[(int) EVALUATION_DATA.BEAR_POWER] = candle.lowPrice_ - ema;

                beforeEma = ema;
            }
        }
        public override bool buySignal(List<CandleData> priceTable)
        {
            if (priceTable.Count < 3) {
                return false;
            }
            var nowCandle = priceTable[1];
            double now = nowCandle.calc_[(int) EVALUATION_DATA.BULL_POWER];

            var prevCandle = priceTable[2];
            double prev = prevCandle.calc_[(int) EVALUATION_DATA.BULL_POWER];
            if (prev < now) {
                return true;
            }
            
            return false;
        }
        public override bool sellSignal(List<CandleData> priceTable)
        {
            if (priceTable.Count < 3) {
                return false;
            }
            var nowCandle = priceTable[1];
            double now = nowCandle.calc_[(int) EVALUATION_DATA.BEAR_POWER];

            var prevCandle = priceTable[2];
            double prev = prevCandle.calc_[(int) EVALUATION_DATA.BEAR_POWER];
            if (prev > now) {
                return true;
            }
            return false;
        }
    }
}
