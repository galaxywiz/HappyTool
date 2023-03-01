using System;
using System.Collections.Generic;

namespace StockLibrary
{
    public enum EVALUATION_ITEM
    {
        // SMA / EMA의 경우 위의 AVG_SAMPLEING이랑 같이 수정되어야 한다.
        SMA_5,
        SMA_10,
        SMA_20,
        SMA_50,
        SMA_100,
        SMA_200,

        EMA_5,
        EMA_10,
        EMA_20,
        EMA_50,
        EMA_100,
        EMA_200,

        RAINBOW_21,         // 그물망
        RAINBOW_25,
        RAINBOW_30,
        RAINBOW_35,
        RAINBOW_40,
        RAINBOW_45,
        RAINBOW_50,
        RAINBOW_55,
        RAINBOW_60,

        BOLLINGER,
        MACD,

        RSI,
        STOCHASTIC,
        STOCHASTIC_RSI,

        ADX,
        WILLIAMS,
        CCI,
        ATR,
        ROC,
        ULTIMATE_OSCIL,
        BULL_BEAR,

        MAX,

        SMA_START = SMA_5,
        EMA_START = EMA_5,
        AVG_END = EMA_200 + 1, // 평균지수와 기술지수로 나뉨.
    }

    // 이거 enum 이 아니고 0~5단계.
    public enum STOCK_EVALUATION
    {
        Buy,
        Normal,
        Sell,

        MAX,
    };

    //가격 평가
    public class Evaluation: ICloneable
    {
        private readonly STOCK_EVALUATION[] eval_ = new STOCK_EVALUATION[(int) EVALUATION_ITEM.MAX];     // 평가 아이템. 

        public Object Clone()
        {
            Evaluation clone = new Evaluation();
            this.eval_.CopyTo(clone.eval_, 0);
            return clone;
        }

        public STOCK_EVALUATION[] getItems()
        {
            return this.eval_;
        }

        private readonly int[] avgEvalCount_ = new int[(int) STOCK_EVALUATION.MAX];          // 평균지수(EMA / SMA) 평가들 취합
        private readonly int[] techEvalCount_ = new int[(int) STOCK_EVALUATION.MAX];         // 기술 평가들 취합
        private void counting()
        {
            for (int index = 0; index < (int) EVALUATION_ITEM.AVG_END; ++index) {
                STOCK_EVALUATION eval = this.eval_[index];
                this.avgEvalCount_[(int) eval]++;
            }
            for (int index = (int) EVALUATION_ITEM.AVG_END; index < (int) EVALUATION_ITEM.MAX; ++index) {
                STOCK_EVALUATION eval = this.eval_[index];
                this.techEvalCount_[(int) eval]++;
            }
        }

        public STOCK_EVALUATION evalAvg()
        {
            STOCK_EVALUATION result = STOCK_EVALUATION.Normal;
            int nowEvalIdx = 0;

            for (int index = 0; index < (int) STOCK_EVALUATION.MAX; ++index) {
                if (nowEvalIdx < this.avgEvalCount_[index]) {
                    nowEvalIdx = this.avgEvalCount_[index];
                    result = (STOCK_EVALUATION) index;
                }
            }

            return result;
        }

        public STOCK_EVALUATION evalTech()
        {
            STOCK_EVALUATION result = STOCK_EVALUATION.Normal;
            int nowEvalIdx = 0;

            for (int index = 0; index < (int) STOCK_EVALUATION.MAX; ++index) {
                if (nowEvalIdx < this.techEvalCount_[index]) {
                    nowEvalIdx = this.techEvalCount_[index];
                    result = (STOCK_EVALUATION) index;
                }
            }

            return result;
        }

        public STOCK_EVALUATION analysis(List<CandleData> priceTable, int timeIdx = 0)
        {
            this.analysisProcess(priceTable, timeIdx);
            this.counting();

            const int AVG_EVAL = 0;
            const int TECH_EVAL = 1;
            STOCK_EVALUATION[] eval = new STOCK_EVALUATION[2];
            eval[AVG_EVAL] = this.evalAvg();
            eval[TECH_EVAL] = this.evalTech();

            if (eval[AVG_EVAL] == eval[TECH_EVAL]) {
                if (eval[AVG_EVAL] < STOCK_EVALUATION.Normal) {
                    return STOCK_EVALUATION.Sell;
                }
                else if (eval[AVG_EVAL] > STOCK_EVALUATION.Normal) {
                    return STOCK_EVALUATION.Buy;
                }
            }
            else {
                if (eval[AVG_EVAL] < STOCK_EVALUATION.Normal || eval[TECH_EVAL] < STOCK_EVALUATION.Normal) {
                    return STOCK_EVALUATION.Sell;
                }
                else if (eval[AVG_EVAL] > STOCK_EVALUATION.Normal || eval[TECH_EVAL] > STOCK_EVALUATION.Normal) {
                    return STOCK_EVALUATION.Buy;
                }
            }

            return STOCK_EVALUATION.Normal;
        }

        private void analysisProcess(List<CandleData> priceTable, int timeIdx)
        {
            // 평가 내릴 수 없음. (과거 데이터가 2개라도 있어야함)
            const int MIN_COUNT = 20;
            if (priceTable.Count < MIN_COUNT) {
                return;
            }
            if (priceTable.Count - MIN_COUNT <= timeIdx) {
                return;
            }
            this.avgAnalysis(priceTable, timeIdx);
            this.bollingerAnalysis(priceTable, timeIdx);
            this.macdAnalysis(priceTable, timeIdx);
            this.rsi14Analysis(priceTable, timeIdx);
            this.stochasticAnalysis(priceTable, timeIdx);
            this.stochasticRsiAnalysis(priceTable, timeIdx);
            this.adx14Analysis(priceTable, timeIdx);
            this.williams14Analysis(priceTable, timeIdx);
            this.cciAnalysis(priceTable, timeIdx);
            this.atrAnalysis(priceTable, timeIdx);
            this.ultimateOsiliAnalysis(priceTable, timeIdx);
            this.rocAnalysis(priceTable, timeIdx);
            this.bullBearAnalysis(priceTable, timeIdx);
        }

        private void avgAnalysis(List<CandleData> priceTable, int timeIdx)
        {
            // 주식 차트 분석 119 page 참고해야함.
            // 지금은 kr.investing.com 의 것을 참고로 구현 되어 있음.
            // 현재값과 평균값 비교로 판단중...
            const int STAND_VAL = 3;

            double nowPrice = priceTable[timeIdx].price_;
            double percent = nowPrice * STAND_VAL / 100;

            //   매수 < -3% 중립 +3% < 매도
            for (int index = 0; index < 2; ++index) {
                for (int avgIdx = 0; avgIdx < (int) AVG_SAMPLEING.AVG_MAX; ++avgIdx) {
                    int valIdx = 0;
                    if (index == 0) {
                        valIdx = (int) EVALUATION_ITEM.SMA_START + avgIdx;
                    }
                    else {
                        valIdx = (int) EVALUATION_ITEM.EMA_START + avgIdx;
                    }
                    double calc = priceTable[timeIdx].calc_[valIdx];

                    //현재값 10000원 일때... 10000 - 300 = 9700 / 10300 기준으로 잡고.
                    //calc 가 밑에 있으면 매수
                    //calc 가 위에 있으면 매도임.
                    if (calc < (nowPrice - percent)) {
                        this.eval_[valIdx] = STOCK_EVALUATION.Buy;
                    }
                    else if ((nowPrice + percent) < calc) {
                        this.eval_[valIdx] = STOCK_EVALUATION.Sell;
                    }
                    else {
                        this.eval_[valIdx] = STOCK_EVALUATION.Normal;
                    }
                }
            }
        }

        private void bollingerAnalysis(List<CandleData> priceTable, int timeIdx)
        {
            int yesterTimeIdx = timeIdx + 1;
            // 주식차트 분석 256page 참고
            double nowPrice = priceTable[timeIdx].price_;
            double beforePrice = priceTable[yesterTimeIdx].price_;

            double upper = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.SMA_BOLLINGER_UP];
            double center = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.SMA_BOLLINGER_CENTER];
            double lower = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.SMA_BOLLINGER_DOWN];

            double nowBandWith = upper - lower;

            double oldUpper = priceTable[yesterTimeIdx].calc_[(int) EVALUATION_DATA.SMA_BOLLINGER_UP];
            double oldLower = priceTable[yesterTimeIdx].calc_[(int) EVALUATION_DATA.SMA_BOLLINGER_DOWN];

            double oldBandWith = oldUpper - oldLower;

            // 어제랑 비교로 밴드폭이 확대 되었으면
            if ((oldBandWith * PublicVar.bollingerBandGrowUp) < nowBandWith) {
                if (upper < nowPrice) {
                    this.eval_[(int) EVALUATION_ITEM.BOLLINGER] = STOCK_EVALUATION.Buy;
                    return;
                }
                if (UtilLibrary.Util.isRange(beforePrice, center, nowPrice)) {
                    this.eval_[(int) EVALUATION_ITEM.BOLLINGER] = STOCK_EVALUATION.Buy;
                    return;
                }
            }
            // 밴드 폭이 줄어 들면
            else if ((oldBandWith * PublicVar.bollingerBandGrowDown) > nowBandWith) {
                if (upper < nowPrice) {
                    this.eval_[(int) EVALUATION_ITEM.BOLLINGER] = STOCK_EVALUATION.Buy;
                    return;
                }
                if (nowPrice < lower) {
                    this.eval_[(int) EVALUATION_ITEM.BOLLINGER] = STOCK_EVALUATION.Sell;
                    return;
                }
            }

            // 중앙선 상승 돌파 시 처리
            if (beforePrice < center
                && center < nowPrice) {
                this.eval_[(int) EVALUATION_ITEM.BOLLINGER] = STOCK_EVALUATION.Buy;
                return;
            }

            // 중앙선을 하락 돌파시
            if (center < beforePrice
                && nowPrice < center) {
                this.eval_[(int) EVALUATION_ITEM.BOLLINGER] = STOCK_EVALUATION.Sell;
                return;
            }

            if (nowPrice < lower) {
                this.eval_[(int) EVALUATION_ITEM.BOLLINGER] = STOCK_EVALUATION.Buy;
                return;
            }

            if (nowPrice > upper) {
                this.eval_[(int) EVALUATION_ITEM.BOLLINGER] = STOCK_EVALUATION.Sell;
                return;
            }

            this.eval_[(int) EVALUATION_ITEM.BOLLINGER] = STOCK_EVALUATION.Normal;
        }

        private void macdAnalysis(List<CandleData> priceTable, int timeIdx)
        {
            int yesterTimeIdx = timeIdx + 1;
            //주식 차트 분석 191 page 참고
            double macdOscilNow = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.MACD_OSCIL];
            double macdOscilBefore = priceTable[yesterTimeIdx].calc_[(int) EVALUATION_DATA.MACD_OSCIL];
            double macdOscilBefore2 = priceTable[yesterTimeIdx + 1].calc_[(int) EVALUATION_DATA.MACD_OSCIL];

            // macd 가 signal을 상향돌파 했으면
            if (macdOscilBefore < 0 && 0 < macdOscilNow) {
                this.eval_[(int) EVALUATION_ITEM.MACD] = STOCK_EVALUATION.Buy;
                return;
            } // macd 가 signal을 하향돌파 했으면
            if (macdOscilBefore > 0 && 0 > macdOscilNow) {
                this.eval_[(int) EVALUATION_ITEM.MACD] = STOCK_EVALUATION.Sell;
                return;
            }

            if (macdOscilBefore2 > 0
                && macdOscilBefore2 < macdOscilBefore
                && macdOscilBefore < macdOscilNow) {
                this.eval_[(int) EVALUATION_ITEM.MACD] = STOCK_EVALUATION.Buy;
                return;
            }
            if (macdOscilBefore2 > macdOscilBefore
                && macdOscilBefore > macdOscilNow
                && macdOscilNow > 0) {
                this.eval_[(int) EVALUATION_ITEM.MACD] = STOCK_EVALUATION.Normal;
                return;
            }
            if (macdOscilBefore2 < 0
                && macdOscilBefore2 > macdOscilBefore
                && macdOscilBefore > macdOscilNow) {
                this.eval_[(int) EVALUATION_ITEM.MACD] = STOCK_EVALUATION.Sell;
                return;
            }
            // 음.....
            if (macdOscilBefore2 < macdOscilBefore
                && macdOscilBefore < macdOscilNow
                && macdOscilNow < 0) {
                this.eval_[(int) EVALUATION_ITEM.MACD] = STOCK_EVALUATION.Normal;
                return;
            }

            this.eval_[(int) EVALUATION_ITEM.MACD] = STOCK_EVALUATION.Normal;
            return;
        }

        private void rsi14Analysis(List<CandleData> priceTable, int timeIdx)
        {
            int yesterTimeIdx = timeIdx + 1;
            // 주식 차트 분석 291 page 참고
            double rsiNow = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.RSI];
            double rsiBefore = priceTable[yesterTimeIdx].calc_[(int) EVALUATION_DATA.RSI];
            const double OVER_BOUGHT = 70.0f;
            const double OVER_SOLD = 30.0f;

            // rsi 가 70을 하양 돌파 하면 매도
            if (rsiBefore >= OVER_BOUGHT && OVER_BOUGHT > rsiNow) {
                this.eval_[(int) EVALUATION_ITEM.RSI] = STOCK_EVALUATION.Sell;
                return;
            }

            // rsi 가 30을 상향 돌파시 매수
            if (rsiBefore <= OVER_SOLD && OVER_SOLD < rsiNow) {
                this.eval_[(int) EVALUATION_ITEM.RSI] = STOCK_EVALUATION.Buy;
                return;
            }

            if (rsiBefore >= OVER_SOLD && OVER_SOLD > rsiNow) {
                this.eval_[(int) EVALUATION_ITEM.RSI] = STOCK_EVALUATION.Sell;
            }
            else if (rsiBefore <= OVER_BOUGHT && OVER_BOUGHT < rsiNow) {
                this.eval_[(int) EVALUATION_ITEM.RSI] = STOCK_EVALUATION.Buy;
            }

            this.eval_[(int) EVALUATION_ITEM.RSI] = STOCK_EVALUATION.Normal;
        }

        private void stochasticAnalysis(List<CandleData> priceTable, int timeIdx)
        {
            int yesterTimeIdx = timeIdx + 1;
            //http://fox.gemizip.com/goodi/search_08_s.htm

            double stochKNow = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.STOCHASTIC_K];
            double stochDNow = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.STOCHASTIC_D];
            double stochKBefore = priceTable[yesterTimeIdx].calc_[(int) EVALUATION_DATA.STOCHASTIC_K];
            double stochDBefore = priceTable[yesterTimeIdx].calc_[(int) EVALUATION_DATA.STOCHASTIC_D];

            const double OVER_BOUGHT = 80.0f;
            const double OVER_SOLD = 20.0f;

            if ((stochKNow < OVER_BOUGHT && OVER_BOUGHT < stochKBefore)
                || (stochDNow < OVER_BOUGHT && OVER_BOUGHT < stochDBefore)) {
                this.eval_[(int) EVALUATION_ITEM.STOCHASTIC] = STOCK_EVALUATION.Sell;
                return;
            }

            if ((stochKBefore < OVER_SOLD && OVER_SOLD < stochKNow)
              || (stochDBefore < OVER_SOLD && OVER_SOLD < stochDNow)) {
                this.eval_[(int) EVALUATION_ITEM.STOCHASTIC] = STOCK_EVALUATION.Buy;
                return;
            }

            // K가 D를 골든 크로스
            if (stochDNow < stochKNow
                && stochDBefore > stochKBefore) {
                this.eval_[(int) EVALUATION_ITEM.STOCHASTIC] = STOCK_EVALUATION.Buy;
                return;
            }

            // D가 K를 크로스
            if (stochDNow > stochKNow
                && stochDBefore < stochKBefore) {
                this.eval_[(int) EVALUATION_ITEM.STOCHASTIC] = STOCK_EVALUATION.Sell;
                return;
            }

            this.eval_[(int) EVALUATION_ITEM.STOCHASTIC] = STOCK_EVALUATION.Normal;
        }

        private void stochasticRsiAnalysis(List<CandleData> priceTable, int timeIdx)
        {
            //http://fox.gemizip.com/goodi/search_08_v.htm

            double stochRsi = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.STOCHASTIC_RSI_K];

            const double BUY_AREA = 0.25f;
            const double SELL_AREA = 0.75f;

            if (stochRsi <= BUY_AREA) {
                this.eval_[(int) EVALUATION_ITEM.STOCHASTIC_RSI] = STOCK_EVALUATION.Buy;
                return;
            }
            if (stochRsi >= SELL_AREA) {
                this.eval_[(int) EVALUATION_ITEM.STOCHASTIC_RSI] = STOCK_EVALUATION.Sell;
                return;
            }

            this.eval_[(int) EVALUATION_ITEM.STOCHASTIC_RSI] = STOCK_EVALUATION.Normal;
        }

        private void adx14Analysis(List<CandleData> priceTable, int timeIdx)
        {
            int yesterTimeIdx = timeIdx + 1;
            // 주식 차트 분석 295 page 참고
            double diPlus = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.DI_PLUS];
            double diMinus = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.DI_MINUS];

            double oldDiPlus = priceTable[yesterTimeIdx].calc_[(int) EVALUATION_DATA.DI_PLUS];
            double oldDiMinus = priceTable[yesterTimeIdx].calc_[(int) EVALUATION_DATA.DI_MINUS];

            // +di 가 -di를 상향 돌파
            if (diMinus < diPlus
                && oldDiMinus > oldDiPlus) {
                this.eval_[(int) EVALUATION_ITEM.ADX] = STOCK_EVALUATION.Buy;
                return;
            }
            // -di 가 +di를 상향 돌파
            if (diMinus > diPlus
                && oldDiMinus < oldDiPlus) {
                this.eval_[(int) EVALUATION_ITEM.ADX] = STOCK_EVALUATION.Sell;
                return;
            }

            //추가로 여기도 참고
            //http://paxnet.moneta.co.kr/WWW/html/lecture/power/ChartClass/class43.htm
            if (diPlus > diMinus) {
                double nowWidth = diPlus - diMinus;
                double oldWidth = oldDiPlus - oldDiMinus;

                //괴리율이 커지면 매수
                if (oldWidth < nowWidth) {
                    this.eval_[(int) EVALUATION_ITEM.ADX] = STOCK_EVALUATION.Buy;
                    return;
                }
            }
            if (diPlus < diMinus) {
                double nowWidth = diPlus - diMinus;
                double oldWidth = oldDiPlus - oldDiMinus;

                //괴리율이 커지면 매도
                if (oldWidth < nowWidth) {
                    this.eval_[(int) EVALUATION_ITEM.ADX] = STOCK_EVALUATION.Sell;
                    return;
                }
            }

            const double ADX_VALUE = 20.0f;
            const double ADXUP_VALUE = 40.0f;
            double adx = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.ADX];
            double oldAdx = priceTable[yesterTimeIdx].calc_[(int) EVALUATION_DATA.ADX];
            if (adx < ADX_VALUE
                && oldAdx < adx) {
                this.eval_[(int) EVALUATION_ITEM.ADX] = STOCK_EVALUATION.Buy;
                return;
            }

            if (adx < ADXUP_VALUE
                && oldAdx > ADXUP_VALUE) {
                this.eval_[(int) EVALUATION_ITEM.ADX] = STOCK_EVALUATION.Sell;
                return;
            }
            this.eval_[(int) EVALUATION_ITEM.ADX] = STOCK_EVALUATION.Normal;
        }

        private void williams14Analysis(List<CandleData> priceTable, int timeIdx)
        {
            //http://traderk10.tistory.com/121 이쪽 설명 참고

            double williamR = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.WILLIAMS];

            const double OVER_BOUGHT = -25.0f;
            const double OVER_SOLD = -75.0f;

            if (williamR <= OVER_SOLD) {
                this.eval_[(int) EVALUATION_ITEM.WILLIAMS] = STOCK_EVALUATION.Sell;
            }
            else if (williamR >= OVER_BOUGHT) {
                this.eval_[(int) EVALUATION_ITEM.WILLIAMS] = STOCK_EVALUATION.Buy;
            }
            else {
                this.eval_[(int) EVALUATION_ITEM.WILLIAMS] = STOCK_EVALUATION.Normal;
            }
        }

        private void cciAnalysis(List<CandleData> priceTable, int timeIdx)
        {
            int yesterTimeIdx = timeIdx + 1;
            // 주식 차트 분석 289 page 참고
            double cciNow = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.CCI];
            double cciBefore = priceTable[yesterTimeIdx].calc_[(int) EVALUATION_DATA.CCI];
            const double OVER_BOUGHT = 100.0f;
            const double OVER_SOLD = -100.0f;

            if (cciBefore >= OVER_BOUGHT && OVER_BOUGHT > cciNow) {
                this.eval_[(int) EVALUATION_ITEM.CCI] = STOCK_EVALUATION.Sell;
            }
            else if (cciBefore <= OVER_SOLD && OVER_SOLD < cciNow) {
                this.eval_[(int) EVALUATION_ITEM.CCI] = STOCK_EVALUATION.Buy;
            }
            else {
                if (cciBefore >= OVER_SOLD && OVER_SOLD > cciNow) {
                    this.eval_[(int) EVALUATION_ITEM.CCI] = STOCK_EVALUATION.Sell;
                }
                else if (cciBefore <= OVER_BOUGHT && OVER_BOUGHT < cciNow) {
                    this.eval_[(int) EVALUATION_ITEM.CCI] = STOCK_EVALUATION.Buy;
                }
                else {
                    this.eval_[(int) EVALUATION_ITEM.CCI] = STOCK_EVALUATION.Normal;
                }
            }
        }

        private void atrAnalysis(List<CandleData> priceTable, int timeIdx)
        {
            //이 지수가 높은건 변동성이 높은것, 즉 리스크 부담으로 봐야 할듯.
            //http://www.daishin.co.kr/ctx_kr/sc_educenter/sg_online_edu/svc_sys_trading/technicalSchool/analysis07_11.html
            double atr = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.ATR];
            double nowPrice = priceTable[timeIdx].price_;

            this.eval_[(int) EVALUATION_ITEM.ATR] = STOCK_EVALUATION.Normal;
        }

        private void ultimateOsiliAnalysis(List<CandleData> priceTable, int timeIdx)
        {
            //얼티밋 오실레이터 분석 참고
            //https://www.instaforex.com/kr/forex_indicators.php?ind=oscillator

            double ulitmate = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.ULTIMATE_OSCIL];
            const double OVER_BOUGHT = 70.0f;
            const double OVER_SELL = 30.0f;

            if (ulitmate <= OVER_SELL) {
                this.eval_[(int) EVALUATION_ITEM.ULTIMATE_OSCIL] = STOCK_EVALUATION.Buy;
                return;
            }
            if (ulitmate >= OVER_BOUGHT) {
                this.eval_[(int) EVALUATION_ITEM.ULTIMATE_OSCIL] = STOCK_EVALUATION.Sell;
                return;
            }
            this.eval_[(int) EVALUATION_ITEM.ULTIMATE_OSCIL] = STOCK_EVALUATION.Normal;
        }

        private void rocAnalysis(List<CandleData> priceTable, int timeIdx)
        {
            double roc = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.ROC];
            const double OVER_BOUGHT = 10.0f;
            const double OVER_SOLD = -10.0f;

            if (roc > OVER_BOUGHT) {
                this.eval_[(int) EVALUATION_ITEM.ROC] = STOCK_EVALUATION.Sell;
            }
            else if (roc < OVER_SOLD) {
                this.eval_[(int) EVALUATION_ITEM.ROC] = STOCK_EVALUATION.Buy;
            }
            else {
                this.eval_[(int) EVALUATION_ITEM.ROC] = STOCK_EVALUATION.Normal;
            }
        }

        private void bullBearAnalysis(List<CandleData> priceTable, int timeIdx)
        {
            int yesterTimeIdx = timeIdx + 1;
            //ElderRayPower 분석이라 해야 맞음
            //http://www.hi-ib.com/systemtrade/st02090804view05.jsp
            //https://www.instaforex.com/kr/forex_technical_indicators/elder_rays

            double bear = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.BEAR_POWER];  //강세 오실레이터
            double bull = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.BULL_POWER];  //약세 오실레이터

            double oldBear = priceTable[yesterTimeIdx].calc_[(int) EVALUATION_DATA.BEAR_POWER];  //강세 오실레이터
            double oldBull = priceTable[yesterTimeIdx].calc_[(int) EVALUATION_DATA.BULL_POWER];  //약세 오실레이터

            double ema = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.EMA_20];
            // ema 가 상승 추세
            if (ema > 0) {
                if (oldBull < bull) {
                    this.eval_[(int) EVALUATION_ITEM.BULL_BEAR] = STOCK_EVALUATION.Buy;
                    return;
                }
                if (oldBear < bear) {
                    if (oldBull < bull) {
                        this.eval_[(int) EVALUATION_ITEM.BULL_BEAR] = STOCK_EVALUATION.Buy;
                        return;
                    }
                }
            }

            if (ema < 0) {
                if (oldBear > bear && 0 < bear) {
                    this.eval_[(int) EVALUATION_ITEM.BULL_BEAR] = STOCK_EVALUATION.Sell;
                    return;
                }
                if (oldBear > bear) {
                    this.eval_[(int) EVALUATION_ITEM.BULL_BEAR] = STOCK_EVALUATION.Sell;
                    return;
                }
            }

            this.eval_[(int) EVALUATION_ITEM.BULL_BEAR] = STOCK_EVALUATION.Normal;
        }
    };
}
