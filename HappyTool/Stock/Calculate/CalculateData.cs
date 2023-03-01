using HappyTool.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using UtilLibrary;

namespace HappyTool.Stock.Calculate
{
    //인베스팅 쪽 기술적 지표를 참고해서 모두 구현을 시킨다.
    //http://kr.investing.com/equities/hyundai-motor-technical

    abstract class Calculater
    {
        public abstract void calculate(ref List<CandleData> priceTable);
    }

    class AvgCalculater : Calculater
    {
        public override void calculate(ref List<CandleData> priceTable)
        {
            int maxIndex = priceTable.Count;
            int[] avgMax = PublicVar.avgMax;
            int avgIndex = 0;

            foreach (int avgTime in avgMax) {
                // ***단순 macd 구하기
                for (int index = 0; index < maxIndex; ++index) {
                    int sumIndexMax = index + avgTime;
                    if (sumIndexMax > maxIndex) {
                        break;
                    }
                    double sum = 0;
                    for (int sumIndex = index; sumIndex < sumIndexMax; ++sumIndex) {
                        sum += priceTable[sumIndex].price_;
                    }
                    CandleData priceData = priceTable[index];
                    if (avgTime == 0.0f) {
                        continue;
                    }
                    priceData.calc_[(int) EVALUATION_DATA.SMA_START + avgIndex] = sum / (double) avgTime;
                }

                // *** 지수 macd구하기
                // EMA(지수이동평균) = 전일지수이동평균 +{c×(금일종가지수-전일지수이동평균)}
                // ※ 단, 0 < c < 1(9일의 경우 0.2, 12일의 경우 0.15, 26일의 경우엔 가중치 0.075 사용)
                //   c = 2 / (n + 1)
                if ((avgMax[avgIndex] + 1) == 0) {
                    continue;
                }
                double multiplier = 2.0f / (double) (avgMax[avgIndex] + 1);
                multiplier = Math.Min(0.999999f, Math.Max(multiplier, 0.000001f));

                int startIdx = maxIndex - avgMax[(int) avgIndex];
                startIdx--;
                double beforeEma = 0.0f;
                for (int index = startIdx; index >= 0; --index) {
                    double ema = 0.0f;
                    if (index == startIdx) {
                        ema = priceTable[index].calc_[(int) EVALUATION_DATA.SMA_START + avgIndex];
                    } else {
                        double close = priceTable[index].price_;
                        ema = multiplier * (close - beforeEma) + beforeEma;
                    }
                    CandleData priceData = priceTable[index];
                    priceData.calc_[(int) EVALUATION_DATA.EMA_START + avgIndex] = ema;

                    beforeEma = ema;
                }
                ++avgIndex;
            }
        }
    }

    class BollingerCalculater : Calculater
    {
        public override void calculate(ref List<CandleData> priceTable)
        {
            int BOLLIGER_TERM = PublicVar.bollingerTerm;

            int maxIndex = Math.Max (priceTable.Count - BOLLIGER_TERM, 0);
            int[] avgMax = PublicVar.avgMax;

            for (int index = 0; index < maxIndex; ++index) {
                //int sumIndexMax = index + avgMax[(int) AVG_SAMPLEING.AVG_MAX - 1]; //120 이전 부터 그리기
                //if (sumIndexMax > maxIndex) {
                //    break;
                //}
                double mean = 0.0f;
                for (int day = index; day < BOLLIGER_TERM + index; ++day) {
                    mean += priceTable[day].price_;
                }
                mean = mean / BOLLIGER_TERM;

                double sumDeviation = 0.0f;
                for (int day = index; day < BOLLIGER_TERM + index; ++day) {
                    sumDeviation = sumDeviation + (priceTable[day].price_ - mean) * (priceTable[day].price_ - mean);
                }
                sumDeviation = Math.Sqrt(sumDeviation / BOLLIGER_TERM);
                double BOLLIGER_CONSTANT = PublicVar.bollingerConst;

                CandleData priceData = priceTable[index];
                double lower = mean - (sumDeviation * BOLLIGER_CONSTANT); 
                double upper = mean + (sumDeviation * BOLLIGER_CONSTANT);

                priceData.calc_[(int) EVALUATION_DATA.BOLLINGER_LOWER] = lower;
                priceData.calc_[(int) EVALUATION_DATA.BOLLINGER_CENTER] = (double) (upper + lower) / 2;
                priceData.calc_[(int) EVALUATION_DATA.BOLLINGER_UPPER] = upper;
            }
        }
    }

    class MacdCalculater : Calculater
    {
        //http://stockcharts.com/school/doku.php?id=chart_school:technical_indicators:moving_average_convergence_divergence_macd
        private double average(List<CandleData> priceTable, int startIndex, int count)
        {
            double sum = 0.0f;
            int maxIndex = startIndex - count;
            if (maxIndex < 0) {
                return sum;
            }
            if (startIndex < 0) {
                return sum;
            }

            for (int index = startIndex; index > maxIndex; --index) {
                sum += priceTable[index].price_;
            }
            if (count == 0) {
                return 0;
            }
            return sum / count;
        }

        private double macdSum(List<CandleData> priceTable, int startIndex, int count)
        {
            double sum = 0.0f;
            int maxIdx = Math.Min(startIndex + count - 1, priceTable.Count);
            for (int idx = maxIdx; idx >= startIndex; --idx) {
                sum += priceTable[idx].calc_[(int) EVALUATION_DATA.MACD];
            }
            return sum;
        }

        public override void calculate(ref List<CandleData> priceTable)
        {
            // MACD 설명 사이트
            // http://investexcel.net/how-to-calculate-macd-in-excel/
            // 공식이 좀 만만치 않군...
            int[] MACD_DAY = PublicVar.macdDay;
            Hashtable ema12day = new Hashtable();
            Hashtable ema26day = new Hashtable();

            //계산하는 가장 옛날 시간을 잡아야 함.
            int CALC_SIZE = 0;
            for (int i = priceTable.Count - 1; i >= 0; --i) {
                if (priceTable[i].price_ != 0) {
                    CALC_SIZE = i;
                    break;
                }
            }

            //-----------------------------------------------------//
            // macd 12라인 계산
            int days = MACD_DAY[0];
            int startday = CALC_SIZE;
            int startIdx = startday - days;
            if (startIdx < 1) {
                Logger.getInstance.consolePrint("! macd 초기화 문제 샘플링 자체 없음");
                return;
            }

            // 첫 스타트 지점 계산
            ema12day[startIdx + 1] = this.average(priceTable, startday, days);

            // 이를 기반으로 후기 내용 계산
            for (int i = startIdx; i >= 0; --i) {
                CandleData priceDataModify = priceTable[i];
                ema12day[i] = (((double) priceDataModify.price_ * (double) (2.0f / (double) (days + 1))) + ((double) ema12day[i + 1] * (1.0f - ((double) (2.0f / (double) (days + 1))))));
            }

            //-----------------------------------------------------//
            // macd 26라인, macd 값 계산
            days = MACD_DAY[1];
            startIdx = startday - MACD_DAY[1] + 1;
            int macdStartIdx = startIdx;
            if (startIdx < 1) {
                Logger.getInstance.consolePrint("! macd 초기화 문제, 문제 있음");
                return;
            }

            // 첫 스타트 지점 계산
            ema26day[startIdx] = this.average(priceTable, startday, days);
            CandleData priceData = priceTable[startIdx];
            priceData.calc_[(int) EVALUATION_DATA.MACD] = (double) ema12day[startIdx] - (double) ema26day[startIdx];

            // 이를 기반으로 후기 내용 계산
            startIdx--;
            for (int i = startIdx; i >= 0; --i) {
                CandleData priceDataModify = priceTable[i];
                ema26day[i] = (((double) priceDataModify.price_ * (double) (2.0f / (double) (days + 1))) + (((double) ema26day[i + 1] * (1.0f - (double) (2.0f / (double) (days + 1))))));
                priceDataModify.calc_[(int) EVALUATION_DATA.MACD] = (double) ema12day[i] - (double) ema26day[i];
            }

            //-----------------------------------------------------//
            // macd 9라인 계산
            days = MACD_DAY[2];
            startIdx = macdStartIdx - days + 1;
            if (startIdx < 1) {
                Logger.getInstance.consolePrint("! macd 초기화 문제, macd9 초기화 에러");
                return;
            }
            priceData = priceTable[startIdx];
            priceData.calc_[(int) EVALUATION_DATA.MACD_SIGNAL] = this.macdSum(priceTable, startIdx, days) / days;
            priceData.calc_[(int) EVALUATION_DATA.MACD_OSCIL] = priceData.calc_[(int) EVALUATION_DATA.MACD] - priceData.calc_[(int) EVALUATION_DATA.MACD_SIGNAL];

            startIdx--;
            for (int i = startIdx; i >= 0; --i) {
                CandleData priceDataModify = priceTable[i];
                CandleData priceDataNext = priceTable[i + 1];

                priceDataModify.calc_[(int) EVALUATION_DATA.MACD_SIGNAL] = (priceDataModify.calc_[(int) EVALUATION_DATA.MACD] * (double) (2.0f / (double) (days + 1)) + priceDataNext.calc_[(int) EVALUATION_DATA.MACD_SIGNAL] * (double) (1 - (double) (2.0f / (double) (days + 1))));
                priceDataModify.calc_[(int) EVALUATION_DATA.MACD_OSCIL] = priceDataModify.calc_[(int) EVALUATION_DATA.MACD] - priceDataModify.calc_[(int) EVALUATION_DATA.MACD_SIGNAL];
            }
        }
    }

    class BNFCalculater : Calculater
    {
        double percent_ = PublicVar.bnfPercent;
        public void setBnfPercent(double percent)
        {
            percent_ = percent;
        }

        public override void calculate(ref List<CandleData> priceTable)
        {
            int maxIndex = priceTable.Count;
            int bnfTerm = PublicVar.bnfTerm;

            // ***단순 macd 구하기
            for (int index = 0; index < maxIndex; ++index)
            {
                int sumIndexMax = index + bnfTerm;
                if (sumIndexMax > maxIndex)
                {
                    break;
                }
                double sum = 0;
                for (int sumIndex = index; sumIndex < sumIndexMax; ++sumIndex)
                {
                    sum += priceTable[sumIndex].price_;
                }
                CandleData priceData = priceTable[index];
                if (bnfTerm == 0.0f)
                {
                    continue;
                }
                double calcPrice = sum / (double)bnfTerm;
                priceData.calc_[(int)EVALUATION_DATA.BNF_UPPER] = calcPrice + (calcPrice * percent_ / 100);
                priceData.calc_[(int)EVALUATION_DATA.BNF_CENTER] = calcPrice;
                priceData.calc_[(int)EVALUATION_DATA.BNF_LOWER] = calcPrice - (calcPrice * percent_ / 100);
            }
        }
    }

    class RSI14Calculater : Calculater
    {
        // 엑셀 검증 완료
        //http://stockcharts.com/school/doku.php?id=chart_school:technical_indicators:relative_strength_index_rsi
        //http://paxnet.moneta.co.kr/WWW/html/lecture/power/ChartClass/class37.htm

        private double average(Hashtable datas, int start, int end)
        {
            double sum = 0.0f;

            if (start > end) {
                return sum;
            }
            if (start < 0) {
                return sum;
            }

            for (int i = start; i < end; ++i) {
                sum += (double) datas[i];
            }

            if ((end - start) == 0) {
                return 0;
            }
            return sum / (end - start);
        }

        public override void calculate(ref List<CandleData> priceTable)
        {
            int maxIndex = priceTable.Count;
            maxIndex--;

            if (maxIndex < PublicVar.rsiTerm) {
                return;
            }

            Hashtable gain = new Hashtable();
            Hashtable lose = new Hashtable();

            maxIndex--;
            for (int index = maxIndex; index >= 0; --index) {
                double priceNow = priceTable[index].price_;
                double priceBefore = priceTable[index + 1].price_;

                double change = priceNow - priceBefore;

                double losePrice = 0;
                double gainPrice = 0;
                if (change < 0) {
                    losePrice = Math.Abs(change);
                }
                else {
                    gainPrice = change;
                }

                gain[index] = gainPrice;
                lose[index] = losePrice;
            }

            int startIdx = maxIndex - PublicVar.rsiTerm;
            //처음열은 평균으로 계산
            double nowAvgGain = this.average(gain, startIdx, startIdx + PublicVar.rsiTerm);
            double nowAvgLose = this.average(gain, startIdx, startIdx + PublicVar.rsiTerm);
            if (nowAvgLose == 0.0f) {
                return;
            }
            double rs = nowAvgGain / nowAvgLose;
            if ((1 + rs) == 0.0f) {
                return;
            }
            CandleData priceDataModify = priceTable[startIdx];
            priceDataModify.calc_[(int) EVALUATION_DATA.RSI] = (nowAvgLose <= 0.0f) ? 100 : (100 - (100 / (1 + rs)));

            double beforeAvgGain = nowAvgGain;
            double beforeAvgLose = nowAvgLose;

            startIdx--;
            for (int index = startIdx; index >= 0; --index) {
                nowAvgGain = ((beforeAvgGain * (PublicVar.rsiTerm - 1)) + (double) gain[index]) / PublicVar.rsiTerm;
                nowAvgLose = ((beforeAvgLose * (PublicVar.rsiTerm - 1)) + (double) lose[index]) / PublicVar.rsiTerm;
                if (nowAvgLose == 0.0f) {
                    continue;
                }
                rs = nowAvgGain / nowAvgLose;

                priceDataModify = priceTable[index];
                if ((1 + rs) == 0.0f) {
                    continue;
                }
                priceDataModify.calc_[(int) EVALUATION_DATA.RSI] = (nowAvgLose <= 0.0f) ? 100 : (100 - (100 / (1 + rs)));

                beforeAvgGain = nowAvgGain;
                beforeAvgLose = nowAvgLose;
            }
        }
    }

    class StochasticCalculater : Calculater
    {
        //http://stockcharts.com/school/doku.php?id=chart_school:technical_indicators:stochastic_oscillator_fast_slow_and_full
        //http://how2stock.blogspot.kr/2013/06/blog-post.html
        //http://paxnet.moneta.co.kr/WWW/html/lecture/power/ChartClass/class38.htm
        //http://www.nanumtrading.com/fx-%EB%B0%B0%EC%9A%B0%EA%B8%B0/%EC%B0%A8%ED%8A%B8-%EB%B3%B4%EC%A1%B0%EC%A7%80%ED%91%9C-%EC%9D%B4%ED%95%B4/04-%EC%8A%A4%ED%86%A0%EC%BA%90%EC%8A%A4%ED%8B%B1/
        // @@@ rsi 는 검증됬는데, slow K D 검증은 필요함
        private double highestPrice(List<CandleData> priceTable, int start, int end)
        {
            double price = 0.0f;

            if (start > end) {
                return price;
            }
            if (start < 0) {
                return price;
            }

            for (int i = start; i < end; ++i) {
                price = Math.Max(price, priceTable[i].highPrice_);
            }

            return price;
        }
        private double lowestPrice(List<CandleData> priceTable, int start, int end)
        {
            double price = priceTable[start].lowPrice_;

            if (start > end) {
                return price;
            }
            if (start < 0) {
                return price;
            }

            for (int i = start; i < end; ++i) {
                price = Math.Min(price, priceTable[i].lowPrice_);
            }

            return price;
        }

        int termN = PublicVar.stochN;
        int termM = PublicVar.stochK;
        int termL = PublicVar.stochD;

        private void calculateSlowKD(ref List<CandleData> priceTable)
        {
            int maxIndex = priceTable.Count;
            maxIndex--;

            Hashtable fastK = new Hashtable();
            Hashtable fastD = new Hashtable();

            int startIdx = maxIndex - termN;

            for (int index = startIdx; index >= 0; --index) {
                double highest = this.highestPrice(priceTable, index, index + termN);
                double lowest = this.lowestPrice(priceTable, index, index + termN);
                
                double closePrice = priceTable[index].price_;

                if ((highest - lowest) == 0.0f) {
                    continue;
                }

                fastK[index] = (closePrice - lowest) / (highest - lowest) * 100.0f;
                if (fastK.Count < termM) {
                    return;
                }
                double sum = 0.0f;
                for (int subIdx = index; subIdx < index + termM; ++subIdx) {
                    sum += (double) fastK[subIdx];
                }
                if (termM == 0.0f) {
                    continue;
                }
                fastD[index] = sum / termM;

                CandleData priceDataModify = priceTable[index];
                priceDataModify.calc_[(int) EVALUATION_DATA.STOCHASTIC_K] = (double) fastK[index];
                priceDataModify.calc_[(int) EVALUATION_DATA.STOCHASTIC_D] = (double) fastD[index];
            }

            // 슬로캐스틱 구하는 거이긴 한데, 지금 처리는
            // 패스트 캐스토캐스틱으로 처리 (어차피 알고리즘 판단이므로... kr.investing에서도 fast사용)
            /*
            startIdx = maxIndex - termM - termN - termL - 1;
            for (int index = startIdx; index >= 0; --index) {
                PriceData priceDataModify = priceTable[index];

                double sum = 0.0f;
                for (int subIdx = index; subIdx < index + termM; ++subIdx) {
                    sum += (double) fastD[subIdx];
                }
                if (termM == 0.0f) {
                    continue;
                }
                priceDataModify.calc_[(int) EVALUATION_DATA.STOCHASTIC_K] = sum / termM;
            }

            startIdx = maxIndex - termM - termN - termL - 1;
            for (int index = startIdx; index >= 0; --index) {
                PriceData priceDataModify = priceTable[index];

                double sum = 0.0f;
                for (int subIdx = index; subIdx < index + termL; ++subIdx) {
                    sum += priceTable[subIdx].calc_[(int) EVALUATION_DATA.STOCHASTIC_K];
                }
                if (termM == 0.0f) {
                    continue;
                }
                priceDataModify.calc_[(int) EVALUATION_DATA.STOCHASTIC_D] = sum / termL;
            }
            */
        }

        private double highestRSI(List<CandleData> priceTable, int start, int end)
        {
            double rsi = 0.0f;

            if (start > end) {
                return rsi;
            }
            if (start < 0) {
                return rsi;
            }

            for (int i = start; i < end; ++i) {
                rsi = Math.Max(rsi, priceTable[i].calc_[(int) EVALUATION_DATA.RSI]);
            }

            return rsi;
        }
        private double lowestRSI(List<CandleData> priceTable, int start, int end)
        {
            double rsi = priceTable[start].calc_[(int) EVALUATION_DATA.RSI];

            if (start > end) {
                return rsi;
            }
            if (start < 0) {
                return rsi;
            }

            for (int i = start; i < end; ++i) {
                rsi = Math.Min(rsi, priceTable[i].calc_[(int) EVALUATION_DATA.RSI]);
            }

            return rsi;
        }

        // 엑셀 검증 완료
        private void calculateRSI14(ref List<CandleData> priceTable)
        {
            int maxIndex = priceTable.Count;
            maxIndex--;
            maxIndex--;

            int startIdx = maxIndex - PublicVar.rsiTerm;
            for (int index = startIdx; index >= 0; --index) {
                CandleData priceDataModify = priceTable[index];

                double rsi = priceDataModify.calc_[(int) EVALUATION_DATA.RSI];
                double highestRSI = this.highestRSI(priceTable, index, index + PublicVar.rsiTerm);
                double lowestRSI = this.lowestRSI(priceTable, index, index + PublicVar.rsiTerm);

                if ((highestRSI - lowestRSI) == 0.0f) {
                    continue;
                }

                priceDataModify.calc_[(int) EVALUATION_DATA.STOCHASTIC_RSI] = (rsi - lowestRSI) / (highestRSI - lowestRSI);
            }
        }

        public override void calculate(ref List<CandleData> priceTable)
        {
            this.calculateSlowKD(ref priceTable);
            this.calculateRSI14(ref priceTable);
        }
    }

    class ADXCalculater : Calculater
    {
        // 엑셀 검증 ok
        //http://stockcharts.com/school/doku.php?id=chart_school:technical_indicators:average_directional_index_adx
        public override void calculate(ref List<CandleData> priceTable)
        {
            int maxIndex = priceTable.Count;
            maxIndex--;

            if (maxIndex < ((PublicVar.adxTerm * 2) + 1)) {
                return;
            }

            Hashtable tr1 = new Hashtable();
            Hashtable dmPlus1 = new Hashtable();
            Hashtable dmMinus1 = new Hashtable();
            maxIndex--;
            for (int index = maxIndex; index >= 0; --index) {
                CandleData priceDataBefore = priceTable[index + 1];
                CandleData priceData = priceTable[index];
                double high = priceData.highPrice_;
                double low = priceData.lowPrice_;
                double close = priceDataBefore.price_;

                tr1[index] = Math.Max(Math.Max(high - low, Math.Abs(high - close)), Math.Abs(low - close));
                dmPlus1[index] = ((high - priceDataBefore.highPrice_) > (priceDataBefore.lowPrice_ - low) ? Math.Max(high - priceDataBefore.highPrice_, 0) : 0);
                dmMinus1[index] = ((priceDataBefore.lowPrice_ - low) > (high - priceDataBefore.highPrice_) ? Math.Max(priceDataBefore.lowPrice_ - low, 0) : 0);
            }
            Hashtable tr14 = new Hashtable();
            Hashtable dmPlus14 = new Hashtable();
            Hashtable dmMinus14 = new Hashtable();

            Hashtable dx = new Hashtable();

            maxIndex -= PublicVar.adxTerm;
            for (int index = maxIndex; index >= 0; --index) {
                CandleData priceData = priceTable[index];
                if (index == maxIndex) {
                    double tra14 = 0.0f, dmp14 = 0.0f, dms14 = 0.0f;
                    
                    for (int subIdx = index; subIdx < PublicVar.adxTerm + index; ++subIdx) {
                        tra14 += (double) tr1[subIdx];
                        dmp14 += (double) dmPlus1[subIdx];
                        dms14 += (double) dmMinus1[subIdx];
                    }
                    tr14[index] = tra14;
                    dmPlus14[index] = dmp14;
                    dmMinus14[index] = dms14;
                }
                else {
                    tr14[index] = (double) tr14[index + 1] - ((double) tr14[index + 1] / PublicVar.adxTerm) + (double) tr1[index];
                    dmPlus14[index] = (double) dmPlus14[index + 1] - ((double) dmPlus14[index + 1] / PublicVar.adxTerm) + (double) dmPlus1[index];
                    dmMinus14[index] = (double) dmMinus14[index + 1] - ((double) dmMinus14[index + 1] / PublicVar.adxTerm) + (double) dmMinus1[index];
                }
                if ((double) tr14[index] == 0.0f) {
                    return;
                }
                double diPlus14 = 100 * ((double) dmPlus14[index] / (double) tr14[index]);
                double diMinus14 = 100 * ((double) dmMinus14[index] / (double) tr14[index]);

                double di14Diff = Math.Abs(diPlus14 - diMinus14);
                double di14Sum = diPlus14 + diMinus14;

                if (di14Sum == 0.0f) {
                    return;
                }
                dx[index] = 100 * (di14Diff / di14Sum);

                priceData.calc_[(int) EVALUATION_DATA.DI_PLUS] = diPlus14;
                priceData.calc_[(int) EVALUATION_DATA.DI_MINUS] = diMinus14;
            }

            maxIndex -= PublicVar.adxTerm;
            for (int index = maxIndex; index >= 0; --index) {
                CandleData priceData = priceTable[index];

                if (index == maxIndex) {
                    double sum = 0.0f;
                    for (int subIdx = index; subIdx < PublicVar.adxTerm + index; ++subIdx) {
                        sum += (double) dx[subIdx];
                    }
                    priceData.calc_[(int) EVALUATION_DATA.ADX] = sum / PublicVar.adxTerm;
                } else {
                    CandleData priceDataBefore = priceTable[index + 1];
                    priceData.calc_[(int) EVALUATION_DATA.ADX] = 
                        (priceDataBefore.calc_[(int) EVALUATION_DATA.ADX] * (PublicVar.adxTerm - 1) + (double) dx[index]) / PublicVar.adxTerm;
                }
            }
        }
    }

    class WilliamsCalculater : Calculater
    {
        // 엑셀 검증 ok
        //http://stockcharts.com/school/doku.php?id=chart_school:technical_indicators:williams_r
        int williams = PublicVar.williams;

        public override void calculate(ref List<CandleData> priceTable)
        {
            int maxIndex = priceTable.Count;
            maxIndex--;

            if (maxIndex < williams) {
                return;
            }

            maxIndex -= williams;
            for (int index = maxIndex; index >= 0; --index) {
                CandleData priceData = priceTable[index];
                double high = priceData.highPrice_, low = priceData.lowPrice_;

                for (int subIdx = index; subIdx < williams + index; ++subIdx) {
                    CandleData priceDataBefore = priceTable[subIdx];
                    high = Math.Max(high, priceDataBefore.highPrice_);
                    low = Math.Min(low, priceDataBefore.lowPrice_);
                }
                double close = priceData.price_;

                if ((high - low) == 0.0f) {
                    continue;
                }

                priceData.calc_[(int) EVALUATION_DATA.WILLIAMS] = (high - close) / (high - low) * -100;
            }
        }
    }

    class CCICalculater : Calculater
    {
        // 엑셀 검증 ok
        //http://stockcharts.com/school/doku.php?id=chart_school:technical_indicators:commodity_channel_index_cci
        public override void calculate(ref List<CandleData> priceTable)
        {
            int maxIndex = priceTable.Count;
            maxIndex--;

            if (maxIndex < PublicVar.cciTerm) {
                return;
            }

            Hashtable typical = new Hashtable();
            for (int index = maxIndex; index >= 0; --index) {
                CandleData priceData = priceTable[index];
                typical[index] = (double) (priceData.price_ + priceData.highPrice_ + priceData.lowPrice_) / 3.0f;
            }

            maxIndex -= PublicVar.cciTerm;
            for (int index = maxIndex; index >= 0; --index) {
                CandleData priceData = priceTable[index];

                double smaTp = 0.0f, meanDev = 0.0f;
                for (int subIdx = index; subIdx < PublicVar.cciTerm + index; ++subIdx) {
                    smaTp += (double) typical[subIdx];
                }
                smaTp /= PublicVar.cciTerm;

                for (int subIdx = index; subIdx < PublicVar.cciTerm + index; ++subIdx) {
                    meanDev = meanDev + Math.Abs(smaTp - (double) typical[subIdx]);
                }
                meanDev /= PublicVar.cciTerm;
                if (meanDev == 0.0f) {
                    continue;
                }
                priceData.calc_[(int) EVALUATION_DATA.CCI] = ((double) typical[index] - smaTp) / (0.015 * meanDev);
            }
        }
    }

    class ATRCalculater : Calculater
    {
        //http://stockcharts.com/school/doku.php?id=chart_school:technical_indicators:average_true_range_atr
        //엑셀 검증 완료
        public override void calculate(ref List<CandleData> priceTable)
        {
            int maxIndex = priceTable.Count;
            maxIndex--;

            if (maxIndex < PublicVar.atrTerm) {
                return;
            }

            Hashtable tr = new Hashtable();

            double hl = priceTable[maxIndex].highPrice_ - priceTable[maxIndex].lowPrice_;
            tr[maxIndex] = hl;

            maxIndex--;
            for (int index = maxIndex; index >= 0; --index) {
                CandleData priceDataBefore = priceTable[index + 1];
                CandleData priceData = priceTable[index];

                double close = priceDataBefore.price_;
                double high = priceData.highPrice_;
                double low = priceData.lowPrice_;

                hl = high - low;
                double hcp = Math.Abs(high - close);
                double lcp = Math.Abs(low - close);

                tr[index] = Math.Max(Math.Max(hl, hcp), lcp);
            }

            maxIndex -= PublicVar.atrTerm;
            for (int index = maxIndex; index >= 0; --index) {
                CandleData priceData = priceTable[index];

                if (index == maxIndex) {
                    double sum = 0.0f;
                    for (int sumIdx = index; sumIdx < PublicVar.atrTerm + index; ++sumIdx) {
                        sum += (double) tr[sumIdx];
                    }
                    priceData.calc_[(int) EVALUATION_DATA.ATR] = sum / PublicVar.atrTerm;
                }
                else {
                    priceData.calc_[(int) EVALUATION_DATA.ATR] = ((priceTable[index + 1].calc_[(int) EVALUATION_DATA.ATR] * (PublicVar.atrTerm - 1)) + (double) tr[index]) / PublicVar.atrTerm;
                }
            }
        }
    }

    // 정확히 뭔지 모르므로 패스...
    class HighsLowsCalculater : Calculater
    {
        //http://stockcharts.com/school/doku.php?id=chart_school:technical_indicators:price_channels
        int highlowsTerm = PublicVar.highlowsTerm;
        public override void calculate(ref List<CandleData> priceTable)
        {
            int maxIndex = priceTable.Count;
            maxIndex--;

            if (maxIndex < PublicVar.cciTerm) {
                return;
            }

            for (int index = maxIndex; index >= 0; --index) {
            }
        }
    }

    class UltimateCalculater : Calculater
    {
        //엑셀 검증 ok
        //http://stockcharts.com/school/doku.php?id=chart_school:technical_indicators:ultimate_oscillator

        int u7 = PublicVar.ultimate7;
        int u14 = PublicVar.ultimate14;
        int u28 = PublicVar.ultimate28;

        public override void calculate(ref List<CandleData> priceTable)
        {
            int maxIndex = priceTable.Count;
            maxIndex--;

            if (maxIndex < u28) {
                return;
            }

            Hashtable buyPress = new Hashtable();
            Hashtable trueRange = new Hashtable();
            
            maxIndex--;
            for (int index = maxIndex; index >= 0; --index) {
                CandleData priceDataBefore = priceTable[index + 1];
                CandleData priceData = priceTable[index];

                double beforeClose = priceDataBefore.price_;
                double high = priceData.highPrice_;
                double low = priceData.lowPrice_;
                double close = priceData.price_;

                buyPress[index] = close - Math.Min(beforeClose, low);
                trueRange[index] = Math.Max(high, beforeClose) - Math.Min(low, beforeClose);
            }

            Hashtable avg7 = new Hashtable();
            Hashtable avg14 = new Hashtable();

            int startIdx = maxIndex;
            maxIndex = startIdx - u7;
            for (int index = maxIndex; index >= 0; --index) {
                double sumBuy = 0.0f, sumTrue = 0.0f;
                for (int subIdx = index; subIdx < u7 + index; ++subIdx) {
                    sumBuy += (double) buyPress[subIdx];
                    sumTrue += (double) trueRange[subIdx];
                }
                if (sumTrue == 0.0f) {
                    return;
                }
                avg7[index] = sumBuy / sumTrue;
            }

            maxIndex = startIdx - u14;
            for (int index = maxIndex; index >= 0; --index) {
                double sumBuy = 0.0f, sumTrue = 0.0f;
                for (int subIdx = index; subIdx < u14 + index; ++subIdx) {
                    sumBuy += (double) buyPress[subIdx];
                    sumTrue += (double) trueRange[subIdx];
                }
                if (sumTrue == 0.0f) {
                    return;
                }
                avg14[index] = sumBuy / sumTrue;
            }

            maxIndex = startIdx - u28;
            for (int index = maxIndex; index >= 0; --index) {
                double sumBuy = 0.0f, sumTrue = 0.0f;
                for (int subIdx = index; subIdx < u28 + index; ++subIdx) {
                    sumBuy += (double) buyPress[subIdx];
                    sumTrue += (double) trueRange[subIdx];
                }
                if (sumTrue == 0.0f) {
                    return;
                }
                double avg28 = sumBuy / sumTrue;

                CandleData priceData = priceTable[index];
                priceData.calc_[(int) EVALUATION_DATA.ULTIMATE_OSCIL] = 100.0f * (((4 * (double) avg7[index]) + (2 * (double) avg14[index]) + avg28) / (4 + 2 + 1));
            }
        }
    }

    class ROCCalculater : Calculater
    {
        //엑셀 검증 ok
        //http://stockcharts.com/school/doku.php?id=chart_school:technical_indicators:rate_of_change_roc_and_momentum

        public override void calculate(ref List<CandleData> priceTable)
        {
            int maxIndex = priceTable.Count;
            maxIndex--;

            if (maxIndex < PublicVar.cciTerm) {
                return;
            }

            maxIndex -= PublicVar.rocTerm;
            for (int index = maxIndex; index >= 0; --index) {
                CandleData priceData = priceTable[index];

                double passPrice = priceTable[index + PublicVar.rocTerm].price_;
                double price = priceData.price_;

                if (passPrice == 0.0f) {
                    continue;
                }

                priceData.calc_[(int) EVALUATION_DATA.ROC] = (price - passPrice) / passPrice * 100.0f;
            }
        }
    }

    class BullBearCalculater : Calculater
    {
        //엑셀 검증 ok
        //https://www.tradinformed.com/how-to-calculate-the-elder-ray-technical-indicator-in-excel/
        //http://www.bullbearings.co.uk/traders.views.php?gid=5&id=753

        static int term = PublicVar.bullbearTerm;
        double emaFactor = (double) 2.0f / (double) (term + 1);

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
                CandleData priceData = priceTable[index];

                double ema = 0.0f;
                if (index == maxIndex) {
                    for (int subIdx = index; subIdx < term + index; ++subIdx) {
                        ema += priceTable[subIdx].price_;
                    }
                    if (term == 0) {
                        continue;
                    }
                    ema /= term;
                } else {
                    double close = priceData.price_;
                    ema = ((close - beforeEma) * emaFactor) + beforeEma;
                }

                priceData.calc_[(int) EVALUATION_DATA.BULL_POWER] = priceData.highPrice_ - ema;
                priceData.calc_[(int) EVALUATION_DATA.BEAR_POWER] = priceData.lowPrice_ - ema;

                beforeEma = ema;
            }
        }
    }
}
