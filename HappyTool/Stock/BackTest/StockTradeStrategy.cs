using HappyTool.Stock.Calculate;
using HappyTool.Util;
using System;
using System.Collections.Generic;

namespace HappyTool.Stock.TradeModules {
    class StockTradeStrategy {
        PRICE_TYPE priceType_;
        int stockCode_;
        TradeModule test_;

        public StockTradeStrategy(int stockCode, PRICE_TYPE priceType)
        {
            stockCode_ = stockCode;
            priceType_ = priceType;
        }

        public StockTradeStrategy(TradeModule test)
        {
            test_ = test;
            stockCode_ = test.stockCode_;
            priceType_ = test.priceType_;
        }

        protected StockData getStockData()
        {
            return StockBot.getInstance.stockData(stockCode_);
        }

        protected List<CandleData> priceTable()
        {
            StockData stockData = this.getStockData();
            return stockData.priceTable(priceType_);
        }

        public void setPriceType(PRICE_TYPE priceType)
        {
            priceType_ = priceType;
        }

        protected STOCK_EVALUATION evalTotal(int timeIdx) {
            List<CandleData> priceTable = this.priceTable();
            if (priceTable == null) {
                return STOCK_EVALUATION.중립;
            }
            Evaluation stockAnalysis = new Evaluation();
            if (test_.evalWeightAverage_ == null) {
                STOCK_EVALUATION evalTotal = stockAnalysis.analysis(priceTable, timeIdx);
                return evalTotal;
            }

            int point = stockAnalysis.analysisPoint(priceTable, timeIdx, test_.evalWeightAverage_);
            if ((int) STOCK_EVALUATION.중립 < point) {
                return STOCK_EVALUATION.매수;
            }
            if (point < (int) STOCK_EVALUATION.중립) {
                return STOCK_EVALUATION.매도;
            }
            return STOCK_EVALUATION.중립;
        }

        const int NOW_DAY_IDX = 0;   // 배열상 0번에 오늘 가격 데이터가 들어가 있음.

        // 개인적으로 사면 오를거 같은 주식을 추려보는 함수 (일단위로 본다)
        // ema50선 위에 가격이 있는가?
        protected bool checkEma50Upper(int time)
        {
            List<CandleData> priceTable = this.priceTable();
            if (priceTable == null) {
                return false;
            }
            int lastTime = priceTable.Count;
            time = Math.Min(time, lastTime);

            double nowPrice = (double) priceTable[time].price_;
            double emaPrice = priceTable[time].calc_[(int) EVALUATION_DATA.EMA_50];

            if (nowPrice < emaPrice) {
                return true;
            }
            return false;
        }

        protected bool isEmaUpper(int tick, EVALUATION_DATA type)
        {
            List<CandleData> priceTable = this.priceTable();
            if (priceTable == null) {
                return false;
            }
            if (priceTable.Count < tick) {
                return false;
            }
            double dayTime = priceTable[tick].calc_[(int) type];
            double toDay = priceTable[NOW_DAY_IDX].calc_[(int) type];

            if (dayTime < toDay) {
                return true;
            }
            return false;
        }

        //-----------------------------------------------------------//
        // 각 클래스 마다 매매 전략 구현
        //-----------------------------------------------------------//
        public virtual bool buy(int timeIdx)
        {
            // ema가 3개전과 비교해서 오르막인가?
            const int time = 3;
            if (this.isEmaUpper(time, EVALUATION_DATA.EMA_20) == false) {
                return false;
            }
            return true;
        }

        // 기본 전력. 3%로 컷
        public virtual bool sell(int timeIdx, double nowProfit)
        {
            double profit = PublicVar.profitRate;          // 주식을 사고 위아래로 3%이면 무족건 매매를 한다고 생각
            double loseProfit = PublicVar.losePorfitRate;
            // 가격이 오름 내림을 체크한다.
            if (nowProfit <= loseProfit) {
                // 손해율이 3% 이상 나면 팜
                return true;
            } else if (profit <= nowProfit) {
                // 이익율이 3% 달성
                return true;
            }
            return false;
        }
    }

    // 종합 평가 위주로 매매
    class EvaluationStockTradeStrategy: StockTradeStrategy {
        public EvaluationStockTradeStrategy(int stockCode, PRICE_TYPE priceType) : base(stockCode, priceType) { }
        public EvaluationStockTradeStrategy(TradeModule test) : base(test)
        {
        }

        public override bool buy(int timeIdx)
        {
            STOCK_EVALUATION eval = this.evalTotal(timeIdx);
            if (eval == STOCK_EVALUATION.매수 || eval == STOCK_EVALUATION.적극매수) {
                return true;
            }
            return false;
        }

        public override bool sell(int timeIdx, double nowProfit)
        {
            STOCK_EVALUATION eval = this.evalTotal(timeIdx);
            if (eval == STOCK_EVALUATION.매도 || eval == STOCK_EVALUATION.적극매도) {
                return true;
            }
            return false;
        }
    }

    // bnf 기준으로 가격이 밑에 있는가?
    class BnfBackStockTradeStrategy :StockTradeStrategy {
        public BnfBackStockTradeStrategy(int stockCode, PRICE_TYPE priceType) : base(stockCode, priceType) { }
        public BnfBackStockTradeStrategy(TradeModule test) :
            base(test)
        {
            test.useBnf_ = true;        // bnf 그래프 그려야 함.
        }

        // bnf 기준으로 가격이 밑을 크로스 상향 돌파 하는가?
        public override bool buy(int timeIdx)
        {
            List<CandleData> priceTable = this.priceTable();
            if (priceTable == null) {
                return false;
            }
            const int TERM = 1;
            int lastTime = priceTable.Count - PublicVar.bnfTerm - TERM;     // bnf 만들려면 25일 데이터가 필요함.
            timeIdx = Math.Min(timeIdx, lastTime);
            if (timeIdx < TERM) {
                return false;
            }

            double nowPrice = (double) priceTable[timeIdx].price_;
            double bnfPrice = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.BNF_LOWER];

            double yesterPrice = (double) priceTable[timeIdx + TERM].price_;
            double yesterBnfPrice = priceTable[timeIdx + TERM].calc_[(int) EVALUATION_DATA.BNF_LOWER];

            // 어제 bnf 하단에 있다가 오늘은 bnf 돌파함
            if (yesterPrice < yesterBnfPrice) {
                if (bnfPrice <= nowPrice) {
                    return true;
                }
            }
            return false;
        }

        public override bool sell(int timeIdx, double nowProfit)
        {
            List<CandleData> priceTable = this.priceTable();
            if (priceTable == null) {
                return false;
            }
            const int TERM = 1;
            int lastTime = priceTable.Count - PublicVar.bnfTerm - TERM;     // bnf 만들려면 25일 데이터가 필요함.
            timeIdx = Math.Min(timeIdx, lastTime);
            if (timeIdx < TERM) {
                return false;
            }

            double nowPrice = (double) priceTable[timeIdx].price_;
            double bnfPrice = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.BNF_UPPER];

            double yesterPrice = (double) priceTable[timeIdx + TERM].price_;
            double yesterBnfPrice = priceTable[timeIdx + TERM].calc_[(int) EVALUATION_DATA.BNF_UPPER];

            // 어제 bnf 상단에 있다가 오늘은 bnf 상단 밑으로 내려감
            if (yesterPrice > yesterBnfPrice) {
                if (bnfPrice >= nowPrice) {
                    return true;
                }
            }
            return false;
        }
    }

    class GoldenCrossStockTradeStrategy :StockTradeStrategy {
        public GoldenCrossStockTradeStrategy(int stockCode, PRICE_TYPE priceType) : base(stockCode, priceType) { }
        public GoldenCrossStockTradeStrategy(TradeModule test) :
            base(test)
        {
            test.useMacd_ = true;
        }

        // 골든 크로스 조건을 보고 산다, 20선이 5선을 돌파하면 골든 크로스임.
        public override bool buy(int timeIdx)
        {
            const int TERM = 3;
            List<CandleData> priceTable = this.priceTable();
            if (priceTable == null) {
                return false;
            }
            int lastTime = priceTable.Count;
            timeIdx = Math.Min(timeIdx, lastTime);

            if (timeIdx < TERM) {
                return false;
            }

            double nowPrice = (double) priceTable[timeIdx].price_;
            double ema20NowPrice = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.EMA_20];
            double ema5NowPrice = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.EMA_5];

            double ema20YesterPrice = priceTable[timeIdx + TERM].calc_[(int) EVALUATION_DATA.EMA_20];
            double ema5YesterPrice = priceTable[timeIdx + TERM].calc_[(int) EVALUATION_DATA.EMA_5];

            if (ema20YesterPrice < ema5YesterPrice) {
                if (ema20NowPrice > ema5YesterPrice) {
                    return true;
                }
            }
            return false;
        }

        // 20선이 5선 이하로 떨어지면 데드 크로스
        public override bool sell(int timeIdx, double nowProfit)
        {
            const int TERM = 3;
            List<CandleData> priceTable = this.priceTable();
            if (priceTable == null) {
                return false;
            }
            int lastTime = priceTable.Count;
            timeIdx = Math.Min(timeIdx, lastTime);

            if (timeIdx < TERM) {
                return false;
            }

            double nowPrice = (double) priceTable[timeIdx].price_;
            double ema20NowPrice = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.EMA_20];
            double ema5NowPrice = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.EMA_5];

            double ema20YesterPrice = priceTable[timeIdx + TERM].calc_[(int) EVALUATION_DATA.EMA_20];
            double ema5YesterPrice = priceTable[timeIdx + TERM].calc_[(int) EVALUATION_DATA.EMA_5];

            if (ema20YesterPrice > ema5YesterPrice) {
                if (ema20NowPrice < ema5YesterPrice) {
                    return true;
                }
            }
            return false;
        }
    }

    class BollingerStockTradeStrategy :StockTradeStrategy {
        public BollingerStockTradeStrategy(int stockCode, PRICE_TYPE priceType) : base(stockCode, priceType) { }
        public BollingerStockTradeStrategy(TradeModule test) :
            base(test)
        {
            test.useBollinger_ = true;
        }

        protected double bollingerLower(int timeIdx)
        {
            List<CandleData> priceTable = this.priceTable();
            if (priceTable == null) {
                return 0.0f;
            }
            return priceTable[timeIdx].calc_[(int) EVALUATION_DATA.BOLLINGER_LOWER];
        }
        protected double bollingerUpper(int timeIdx)
        {
            List<CandleData> priceTable = this.priceTable();
            if (priceTable == null) {
                return 0.0f;
            }
            return priceTable[timeIdx].calc_[(int) EVALUATION_DATA.BOLLINGER_UPPER];
        }

        protected double bollinger1Persent(int timeIdx)
        {
            List<CandleData> priceTable = this.priceTable();
            if (priceTable == null) {
                return 0.0f;
            }
            double bollUpPrice = this.bollingerUpper(timeIdx);
            double bollLowPrice = this.bollingerLower(timeIdx);

            double value = (bollUpPrice - bollLowPrice) / 100.0f;
            return value;
        }

        // 볼린저 하단 10% 근접하면 매수 
        public override bool buy(int timeIdx)
        {
            List<CandleData> priceTable = this.priceTable();
            if (priceTable == null) {
                return false;
            }
            int lastTime = priceTable.Count - PublicVar.bollingerTerm;     // 볼린져 벤드 만들려면 데이터가 필요함.
            timeIdx = Math.Min(timeIdx, lastTime);

            double nowPrice = (double) priceTable[timeIdx].price_;
            double tradePrice = this.bollingerLower(timeIdx);
            double val = this.bollinger1Persent(timeIdx) * 5;

            if (nowPrice < tradePrice + val) {
                return true;
            }
            return false;
        }

        // 가격이 볼린저 중단과 상단의 가운데 이상이면 판다.
        public override bool sell(int timeIdx, double nowProfit)
        {
            List<CandleData> priceTable = this.priceTable();
            if (priceTable == null) {
                return false;
            }
            int lastTime = priceTable.Count - PublicVar.bollingerTerm;     // 볼린져 벤드 만들려면 데이터가 필요함.
            timeIdx = Math.Min(timeIdx, lastTime);

            double nowPrice = (double) priceTable[timeIdx].price_;

            double tradePrice = this.bollingerLower(timeIdx);
            double val = this.bollinger1Persent(timeIdx) * 80;

            if (tradePrice + val < nowPrice) {
                return true;
            }
            return false;
        }
    }

    // 헥트 아저씨가 소개해준 volatility squeeze 전략. 볼린저 상단 돌파시 매수, 
    // 팔때 상단 터치 확인후, 중단쯤 떨어질때 팜.
    class BollingerStockTradeStrategyByHETC :BollingerStockTradeStrategy {
        public BollingerStockTradeStrategyByHETC(int stockCode, PRICE_TYPE priceType) : base(stockCode, priceType) { }
        public BollingerStockTradeStrategyByHETC(TradeModule test) :
            base(test)
        {
        }

        public override bool buy(int timeIdx)
        {
            List<CandleData> priceTable = this.priceTable();
            if (priceTable == null) {
                return false;
            }
            int lastTime = priceTable.Count - PublicVar.bollingerTerm - 1;     // 볼린져 벤드 만들려면 데이터가 필요함.
            timeIdx = Math.Min(timeIdx, lastTime);

            // 볼린저선 계산 전 데이터는 제외
            double yesterTradePrice = this.bollingerUpper(timeIdx + 1);
            if (yesterTradePrice < 10.0f) {
                return false;
            }
            double yesterPrice = (double) priceTable[timeIdx + 1].price_;

            double nowPrice = (double) priceTable[timeIdx].price_;
            double nowTradePrice = this.bollingerUpper(timeIdx);

            // 볼린저선 상향 돌파시
            if (yesterPrice < yesterTradePrice) {
                if (nowTradePrice < nowPrice) {
                    return true;
                }
            }

            double yesterLower = this.bollingerLower(timeIdx + 1);
            double nowLower = this.bollingerLower(timeIdx);
            if (yesterPrice < yesterLower) {
                if (nowPrice < nowLower) {
                    return true;
                }
            }
            return false;
        }

        bool sellFlag_ = false;
        public override bool sell(int timeIdx, double nowProfit)
        {
            List<CandleData> priceTable = this.priceTable();
            if (priceTable == null) {
                return false;
            }
            int lastTime = priceTable.Count - PublicVar.bollingerTerm;     // 볼린져 벤드 만들려면 데이터가 필요함.
            timeIdx = Math.Min(timeIdx, lastTime);

            double nowPrice = (double) priceTable[timeIdx].price_;
            double upperPrice = this.bollingerUpper(timeIdx);
            if (upperPrice < nowPrice) {
                return false;
            }

            double tradePrice = this.bollingerLower(timeIdx);
            double val = this.bollinger1Persent(timeIdx);

            if (sellFlag_ == false) {
                //볼린저 상단을 터치 해야 팔거임.
                if (MyUtil.isRange(tradePrice + (val * 80), nowPrice, tradePrice + (val * 100))) {
                    sellFlag_ = true;
                    return false;
                }
            }

            if (MyUtil.isRange(tradePrice + (val * 70), nowPrice, tradePrice + (val * 80))) {
                return true;
            }

            return false;
        }
    }

    // 볼린저밴드 폭을 기준으로 매매
    class BollingerStockTradeStrategyWithBandWidth :BollingerStockTradeStrategy {
        public BollingerStockTradeStrategyWithBandWidth(int stockCode, PRICE_TYPE priceType) : base(stockCode, priceType) { }
        public BollingerStockTradeStrategyWithBandWidth(TradeModule test) :
            base(test)
        {
        }

        // 볼린저 밴드 폭이 2배이상으로 넓어지면 구매한다.
        public override bool buy(int timeIdx)
        {
            List<CandleData> priceTable = this.priceTable();
            if (priceTable == null) {
                return false;
            }
            int lastTime = priceTable.Count - PublicVar.bollingerTerm - 1;     // 볼린져 벤드 만들려면 데이터가 필요함.
            timeIdx = Math.Min(timeIdx, lastTime);

            // 볼린저선 계산 전 데이터는 제외
            double yesterTradePrice = this.bollingerUpper(timeIdx + 1);
            if (yesterTradePrice < 10.0f) {
                return false;
            }
            double yesterWitdh = this.bollingerUpper(timeIdx + 1) - this.bollingerLower(timeIdx + 1);
            double todayWidth = this.bollingerUpper(timeIdx) - this.bollingerLower(timeIdx);

            if (yesterWitdh * 1.5f < todayWidth) {
                return true;
            }
            return false;
        }

        public override bool sell(int timeIdx, double nowProfit)
        {
            List<CandleData> priceTable = this.priceTable();
            if (priceTable == null) {
                return false;
            }
            int lastTime = priceTable.Count - PublicVar.bollingerTerm;     // 볼린져 벤드 만들려면 데이터가 필요함.
            timeIdx = Math.Min(timeIdx, lastTime);

            double nowPrice = (double) priceTable[timeIdx].price_;
            double upperPrice = this.bollingerUpper(timeIdx);
            if (upperPrice < nowPrice) {
                return false;
            }

            double tradePrice = this.bollingerLower(timeIdx);
            double val = this.bollinger1Persent(timeIdx);

            if (MyUtil.isRange(tradePrice + (val * 70), nowPrice, tradePrice + (val * 80))) {
                return true;
            }
            if (tradePrice + (val * 55) > nowPrice) {
                return true;
            }
            return false;
        }
    }

    // CCI 지표를 이용해서 매매
    class CCIStockTradeStrategy :StockTradeStrategy {
        public CCIStockTradeStrategy(int stockCode, PRICE_TYPE priceType) : base(stockCode, priceType) { }
        public CCIStockTradeStrategy(TradeModule test) :
            base(test)
        {
        }

        public override bool buy(int timeIdx)
        {
            List<CandleData> priceTable = this.priceTable();
            if (priceTable == null) {
                return false;
            }
            int lastTime = priceTable.Count - 2;
            timeIdx = Math.Min(timeIdx, lastTime);

            double yesterCCI = priceTable[timeIdx + 1].calc_[(int) EVALUATION_DATA.CCI];
            double nowCCI = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.CCI];

            //CCI가 -100을 상향 이탈시 매수
            const double STAND_VALUE = -100.0f;
            if (yesterCCI < STAND_VALUE) {
                if (nowCCI > STAND_VALUE) {
                    return true;
                }
            }
            return false;
        }

        public override bool sell(int timeIdx, double nowProfit)
        {
            List<CandleData> priceTable = this.priceTable();
            if (priceTable == null) {
                return false;
            }
            int lastTime = priceTable.Count - 2;
            timeIdx = Math.Min(timeIdx, lastTime);

            double yesterCCI = priceTable[timeIdx + 1].calc_[(int) EVALUATION_DATA.CCI];
            double nowCCI = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.CCI];

            //CCI가 +100을 하양 이탈시 매도
            const double STAND_VALUE = 100.0f;
            if (yesterCCI > STAND_VALUE) {
                if (nowCCI < STAND_VALUE) {
                    return true;
                }
            }
            return false;
        }
    }
}
    