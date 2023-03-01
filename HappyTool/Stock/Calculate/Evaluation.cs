using HappyTool.Util;
using System;
using System.Collections.Generic;

namespace HappyTool.Stock.Calculate
{
    enum EVALUATION_ITEM
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
    enum STOCK_EVALUATION
    {
        적극매도,
        매도,
        중립,
        매수,
        적극매수,

        MAX,
    };

    // 평가 가중치
    class EvalWeightAverage
    {
        public int[] weightAvg_ = new int[(int) EVALUATION_ITEM.MAX];          // 평균지수 평가들 취합
        public EvalWeightAverage()
        {
            // 1로 초기화
            for (int index = 0; index < weightAvg_.Length; ++index) {
                weightAvg_[index] = 1;
            }
        }

        public string log()
        {
            string ret = "*********************\n";
            EVALUATION_ITEM item = EVALUATION_ITEM.SMA_START;
            for (int idx = 0; idx < (int)EVALUATION_ITEM.MAX; ++ idx) {
                ret += string.Format("{0} 의 가중치 {1}\n", item.ToString(), weightAvg_[idx]);
                item++;
            }
            return ret;
        }
    }

    //가격 평가
    class Evaluation : ICloneable
    {
        private STOCK_EVALUATION[] eval_ = new STOCK_EVALUATION[(int) EVALUATION_ITEM.MAX];     // 평가 아이템. 

        public Object Clone()
        {
            Evaluation clone = new Evaluation();
            eval_.CopyTo(clone.eval_, 0);
            return clone;
        }

        public STOCK_EVALUATION[] getItems()
        {
            return eval_;
        }

        private int[] avgEvalCount_ = new int[(int) STOCK_EVALUATION.MAX];          // 평균지수(EMA / SMA) 평가들 취합
        private int[] techEvalCount_ = new int[(int) STOCK_EVALUATION.MAX];         // 기술 평가들 취합
        private void counting()
        {
            for (int index = 0; index < (int) EVALUATION_ITEM.AVG_END; ++index) {
                STOCK_EVALUATION eval = eval_[index];
                avgEvalCount_[(int) eval]++;
            }
            for (int index = (int) EVALUATION_ITEM.AVG_END; index < (int) EVALUATION_ITEM.MAX; ++index) {
                STOCK_EVALUATION eval = eval_[index];
                techEvalCount_[(int) eval]++;
            }
        }

        public STOCK_EVALUATION evalAvg()
        {
            STOCK_EVALUATION result = STOCK_EVALUATION.중립;
            int nowEvalIdx = 0;

            for (int index = 0; index < (int) STOCK_EVALUATION.MAX; ++index) {
                if (nowEvalIdx < avgEvalCount_[index]) {
                    nowEvalIdx = avgEvalCount_[index];
                    result = (STOCK_EVALUATION) index;
                }
            }

            return result;
        }

        public STOCK_EVALUATION evalTech()
        {
            STOCK_EVALUATION result = STOCK_EVALUATION.중립;
            int nowEvalIdx = 0;

            for (int index = 0; index < (int) STOCK_EVALUATION.MAX; ++index) {
                if (nowEvalIdx < techEvalCount_[index]) {
                    nowEvalIdx = techEvalCount_[index];
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
                if (eval[AVG_EVAL] == STOCK_EVALUATION.매도) {
                    return STOCK_EVALUATION.매도;
                }
                else if (eval[AVG_EVAL] == STOCK_EVALUATION.매수) {
                    return STOCK_EVALUATION.매수;
                }
            }
            else {
                if (eval[AVG_EVAL] == STOCK_EVALUATION.매도 || eval[TECH_EVAL] == STOCK_EVALUATION.매도) {
                    return STOCK_EVALUATION.매도;
                }
                else if (eval[AVG_EVAL] == STOCK_EVALUATION.매수 || eval[TECH_EVAL] == STOCK_EVALUATION.매수) {
                    return STOCK_EVALUATION.매수;
                }
            }

            return STOCK_EVALUATION.중립;
        }

        public int analysisPoint(List<CandleData> priceTable, int timeIdx, EvalWeightAverage evalWeightAverage)
        {
            this.analysisProcess(priceTable, timeIdx);

            int point = 0;
            for (int index = 0; index < (int) EVALUATION_ITEM.MAX; ++index) {
                int temp = (int) eval_[index];
                if (temp < (int) STOCK_EVALUATION.중립) {
                    // 매도이면 point를 차감
                    point -= (int) eval_[index] * evalWeightAverage.weightAvg_[index];
                } else if (temp > (int) STOCK_EVALUATION.중립) {
                    point += (int) eval_[index] * evalWeightAverage.weightAvg_[index];
                }
            }

            return point;
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
                        eval_[valIdx] = STOCK_EVALUATION.매수;
                    }
                    else if ((nowPrice + percent) < calc) {
                        eval_[valIdx] = STOCK_EVALUATION.매도;
                    }
                    else {
                        eval_[valIdx] = STOCK_EVALUATION.중립;
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

            double upper = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.BOLLINGER_UPPER];
            double center = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.BOLLINGER_CENTER];
            double lower = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.BOLLINGER_LOWER];

            double nowBandWith = upper - lower;

            double oldUpper = priceTable[yesterTimeIdx].calc_[(int) EVALUATION_DATA.BOLLINGER_UPPER];
            double oldLower = priceTable[yesterTimeIdx].calc_[(int) EVALUATION_DATA.BOLLINGER_LOWER];

            double oldBandWith = oldUpper - oldLower;

            // 어제랑 비교로 밴드폭이 확대 되었으면
            if ((oldBandWith * PublicVar.bollingerBandGrowUp) < nowBandWith) {
                if (upper < nowPrice) {
                    eval_[(int) EVALUATION_ITEM.BOLLINGER] = STOCK_EVALUATION.매수;
                    return;
                }
                if (MyUtil.isRange(beforePrice, center, nowPrice)) {
                    eval_[(int) EVALUATION_ITEM.BOLLINGER] = STOCK_EVALUATION.매수;
                    return;
                }
            }
            // 밴드 폭이 줄어 들면
            else if ((oldBandWith * PublicVar.bollingerBandGrowDown) > nowBandWith) {
                if (upper < nowPrice) {
                    eval_[(int) EVALUATION_ITEM.BOLLINGER] = STOCK_EVALUATION.매수;
                    return;
                }
                if (nowPrice < lower) {
                    eval_[(int) EVALUATION_ITEM.BOLLINGER] = STOCK_EVALUATION.매도;
                    return;
                }
            }

            // 중앙선 상승 돌파 시 처리
            if (beforePrice < center 
                && center < nowPrice) { 
                eval_[(int) EVALUATION_ITEM.BOLLINGER] = STOCK_EVALUATION.매수;
                return;
            }

            // 중앙선을 하락 돌파시
            if (center < beforePrice 
                && nowPrice < center) {
                eval_[(int) EVALUATION_ITEM.BOLLINGER] = STOCK_EVALUATION.매도;
                return;
            }

            if (nowPrice < lower) {
                eval_[(int) EVALUATION_ITEM.BOLLINGER] = STOCK_EVALUATION.적극매수;
                return;
            }

            if (nowPrice > upper) {
                eval_[(int) EVALUATION_ITEM.BOLLINGER] = STOCK_EVALUATION.적극매도;
                return;
            }

            eval_[(int) EVALUATION_ITEM.BOLLINGER] = STOCK_EVALUATION.중립;
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
                eval_[(int) EVALUATION_ITEM.MACD] = STOCK_EVALUATION.매수;
                return;
            } // macd 가 signal을 하향돌파 했으면
            if (macdOscilBefore > 0 && 0 > macdOscilNow) {
                eval_[(int) EVALUATION_ITEM.MACD] = STOCK_EVALUATION.매도;
                return;
            }

            if (macdOscilBefore2 > 0
                && macdOscilBefore2 < macdOscilBefore
                && macdOscilBefore < macdOscilNow) {
                eval_[(int) EVALUATION_ITEM.MACD] = STOCK_EVALUATION.매수;
                return;
            }
            if (macdOscilBefore2 > macdOscilBefore
                && macdOscilBefore > macdOscilNow
                && macdOscilNow > 0) {
                eval_[(int) EVALUATION_ITEM.MACD] = STOCK_EVALUATION.중립;
                return;
            }
            if (macdOscilBefore2 < 0
                && macdOscilBefore2 > macdOscilBefore
                && macdOscilBefore > macdOscilNow) {
                eval_[(int) EVALUATION_ITEM.MACD] = STOCK_EVALUATION.매도;
                return;
            }
            // 음.....
            if (macdOscilBefore2 < macdOscilBefore
                && macdOscilBefore < macdOscilNow
                && macdOscilNow < 0) {
                eval_[(int) EVALUATION_ITEM.MACD] = STOCK_EVALUATION.중립;
                return;
            }

            eval_[(int) EVALUATION_ITEM.MACD] = STOCK_EVALUATION.중립;
            return;
        }

        private void rsi14Analysis(List<CandleData> priceTable, int timeIdx)
        {
            int yesterTimeIdx = timeIdx + 1;
            // 주식 차트 분석 291 page 참고
            double rsiNow = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.RSI];
            double rsiBefore = priceTable[yesterTimeIdx].calc_[(int) EVALUATION_DATA.RSI];
            const double OVER_BOUGHT = 70.0f;
            const double CENTER = 50.0f;
            const double OVER_SOLD = 30.0f;

            // rsi 가 70을 하양 돌파 하면 매도
            if (rsiBefore >= OVER_BOUGHT && OVER_BOUGHT > rsiNow) {
                eval_[(int) EVALUATION_ITEM.RSI] = STOCK_EVALUATION.적극매도;
                return;
            }

            // rsi 가 30을 상향 돌파시 매수
            if (rsiBefore <= OVER_SOLD && OVER_SOLD < rsiNow) {
                eval_[(int) EVALUATION_ITEM.RSI] = STOCK_EVALUATION.적극매수;
                return;
            }

            if (rsiBefore >= OVER_SOLD && OVER_SOLD > rsiNow) {
                eval_[(int) EVALUATION_ITEM.RSI] = STOCK_EVALUATION.매도;
            }
            else if (rsiBefore <= OVER_BOUGHT && OVER_BOUGHT < rsiNow) {
                eval_[(int) EVALUATION_ITEM.RSI] = STOCK_EVALUATION.매수;
            }

            //if (CENTER < rsiNow) {
            //    item_[(int) EVALUATION_ITEM.RSI] = STOCK_EVALUATION.매수;
            //    return;
            //}
            //if (rsiNow < CENTER) {
            //    item_[(int) EVALUATION_ITEM.RSI] = STOCK_EVALUATION.매도;
            //    return;
            //}
            eval_[(int) EVALUATION_ITEM.RSI] = STOCK_EVALUATION.중립;
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
                eval_[(int) EVALUATION_ITEM.STOCHASTIC] = STOCK_EVALUATION.매도;
                return;
            }

            if ((stochKBefore < OVER_SOLD && OVER_SOLD < stochKNow)
              || (stochDBefore < OVER_SOLD && OVER_SOLD < stochDNow)) {
                eval_[(int) EVALUATION_ITEM.STOCHASTIC] = STOCK_EVALUATION.매수;
                return;
            }

            // K가 D를 골든 크로스
            if (stochDNow < stochKNow
                && stochDBefore > stochKBefore) {
                eval_[(int) EVALUATION_ITEM.STOCHASTIC] = STOCK_EVALUATION.적극매수;
                return;
            }

            // D가 K를 크로스
            if (stochDNow > stochKNow
                && stochDBefore < stochKBefore) {
                eval_[(int) EVALUATION_ITEM.STOCHASTIC] = STOCK_EVALUATION.적극매도;
                return;
            }

            eval_[(int) EVALUATION_ITEM.STOCHASTIC] = STOCK_EVALUATION.중립;
        }

        private void stochasticRsiAnalysis(List<CandleData> priceTable, int timeIdx)
        {
            //http://fox.gemizip.com/goodi/search_08_v.htm

            double stochRsi = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.STOCHASTIC_RSI];

            const double BUY_AREA = 0.25f;
            const double SELL_AREA = 0.75f;

            if (stochRsi <= BUY_AREA) {
                eval_[(int) EVALUATION_ITEM.STOCHASTIC_RSI] = STOCK_EVALUATION.매수;
                return;
            }
            if (stochRsi >= SELL_AREA) {
                eval_[(int) EVALUATION_ITEM.STOCHASTIC_RSI] = STOCK_EVALUATION.매도;
                return;
            }

            eval_[(int) EVALUATION_ITEM.STOCHASTIC_RSI] = STOCK_EVALUATION.중립;
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
                eval_[(int) EVALUATION_ITEM.ADX] = STOCK_EVALUATION.적극매수;
                return;
            }
            // -di 가 +di를 상향 돌파
            if (diMinus > diPlus
                && oldDiMinus < oldDiPlus) {
                eval_[(int) EVALUATION_ITEM.ADX] = STOCK_EVALUATION.적극매도;
                return;
            }

            //추가로 여기도 참고
            //http://paxnet.moneta.co.kr/WWW/html/lecture/power/ChartClass/class43.htm
            if (diPlus > diMinus) {
                double nowWidth = diPlus - diMinus;
                double oldWidth = oldDiPlus - oldDiMinus;

                //괴리율이 커지면 매수
                if (oldWidth < nowWidth) {
                    eval_[(int) EVALUATION_ITEM.ADX] = STOCK_EVALUATION.매수;
                    return;
                }
            }
            if (diPlus < diMinus) {
                double nowWidth = diPlus - diMinus;
                double oldWidth = oldDiPlus - oldDiMinus;

                //괴리율이 커지면 매도
                if (oldWidth < nowWidth) {
                    eval_[(int) EVALUATION_ITEM.ADX] = STOCK_EVALUATION.매도;
                    return;
                }
            }

            const double ADX_VALUE = 20.0f;
            const double ADXUP_VALUE = 40.0f;
            double adx = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.ADX];
            double oldAdx = priceTable[yesterTimeIdx].calc_[(int) EVALUATION_DATA.ADX];
            if (adx < ADX_VALUE
                && oldAdx < adx) {
                eval_[(int) EVALUATION_ITEM.ADX] = STOCK_EVALUATION.매수;
                return;
            }

            if (adx < ADXUP_VALUE
                && oldAdx > ADXUP_VALUE) {
                eval_[(int) EVALUATION_ITEM.ADX] = STOCK_EVALUATION.매도;
                return;
            }
            eval_[(int) EVALUATION_ITEM.ADX] = STOCK_EVALUATION.중립;
        }

        private void williams14Analysis(List<CandleData> priceTable, int timeIdx)
        {
            //http://traderk10.tistory.com/121 이쪽 설명 참고

            double williamR = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.WILLIAMS];

            const double OVER_BOUGHT = -25.0f;
            const double OVER_SOLD = -75.0f;

            if (williamR <= OVER_SOLD) {
                eval_[(int) EVALUATION_ITEM.WILLIAMS] = STOCK_EVALUATION.매도;
            }
            else if (williamR >= OVER_BOUGHT) {
                eval_[(int) EVALUATION_ITEM.WILLIAMS] = STOCK_EVALUATION.매수;
            }
            else {
                eval_[(int) EVALUATION_ITEM.WILLIAMS] = STOCK_EVALUATION.중립;
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
                eval_[(int) EVALUATION_ITEM.CCI] = STOCK_EVALUATION.적극매도;
            }
            else if (cciBefore <= OVER_SOLD && OVER_SOLD < cciNow) {
                eval_[(int) EVALUATION_ITEM.CCI] = STOCK_EVALUATION.적극매수;
            }
            else {
                if (cciBefore >= OVER_SOLD && OVER_SOLD > cciNow) {
                    eval_[(int) EVALUATION_ITEM.CCI] = STOCK_EVALUATION.매도;
                }
                else if (cciBefore <= OVER_BOUGHT && OVER_BOUGHT < cciNow) {
                    eval_[(int) EVALUATION_ITEM.CCI] = STOCK_EVALUATION.매수;
                }
                else {
                    eval_[(int) EVALUATION_ITEM.CCI] = STOCK_EVALUATION.중립;
                }
            }
        }

        private void atrAnalysis(List<CandleData> priceTable, int timeIdx)
        {
            //이 지수가 높은건 변동성이 높은것, 즉 리스크 부담으로 봐야 할듯.
            //http://www.daishin.co.kr/ctx_kr/sc_educenter/sg_online_edu/svc_sys_trading/technicalSchool/analysis07_11.html
            double atr = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.ATR];
            double nowPrice = priceTable[timeIdx].price_;

            eval_[(int) EVALUATION_ITEM.ATR] = STOCK_EVALUATION.중립;
        }

        private void ultimateOsiliAnalysis(List<CandleData> priceTable, int timeIdx)
        {
            //얼티밋 오실레이터 분석 참고
            //https://www.instaforex.com/kr/forex_indicators.php?ind=oscillator

            double ulitmate = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.ULTIMATE_OSCIL];
            const double OVER_BOUGHT = 70.0f;
            const double OVER_SELL = 30.0f;

            if (ulitmate <= OVER_SELL) {
                eval_[(int) EVALUATION_ITEM.ULTIMATE_OSCIL] = STOCK_EVALUATION.매수;
                return;
            }
            if (ulitmate >= OVER_BOUGHT) {
                eval_[(int) EVALUATION_ITEM.ULTIMATE_OSCIL] = STOCK_EVALUATION.매도;
                return;
            }
            eval_[(int) EVALUATION_ITEM.ULTIMATE_OSCIL] = STOCK_EVALUATION.중립;
        }

        private void rocAnalysis(List<CandleData> priceTable, int timeIdx)
        {
            double roc = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.ROC];
            const double OVER_BOUGHT = 10.0f;
            const double OVER_SOLD = -10.0f;

            if (roc > OVER_BOUGHT) {
                eval_[(int) EVALUATION_ITEM.ROC] = STOCK_EVALUATION.매도;
            }
            else if (roc < OVER_SOLD) {
                eval_[(int) EVALUATION_ITEM.ROC] = STOCK_EVALUATION.매수;
            }
            else {
                eval_[(int) EVALUATION_ITEM.ROC] = STOCK_EVALUATION.중립;
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
                    eval_[(int) EVALUATION_ITEM.BULL_BEAR] = STOCK_EVALUATION.매수;
                    return;
                }
                if (oldBear < bear) {
                    if (oldBull < bull) {
                        eval_[(int) EVALUATION_ITEM.BULL_BEAR] = STOCK_EVALUATION.매수;
                        return;
                    }
                }
            }

            if (ema < 0) {
                if (oldBear > bear && 0 < bear) {
                    eval_[(int) EVALUATION_ITEM.BULL_BEAR] = STOCK_EVALUATION.매도;
                    return;
                }
                if (oldBear > bear) {
                    eval_[(int) EVALUATION_ITEM.BULL_BEAR] = STOCK_EVALUATION.매도;
                    return;
                }
            }

            eval_[(int) EVALUATION_ITEM.BULL_BEAR] = STOCK_EVALUATION.중립;
        }
    };
}
