using HappyTool.Messanger;
using HappyTool.Stock;
using HappyTool.Stock.TradeModules;
using HappyTool.Stock.Calculate;
using HappyTool.Util;

using System;
using System.Collections.Generic;

namespace HappyTool {
    class StockData :ICloneable {
        // stockData 갱신 락
        public object dataLock_ { get; set; }

        //-------------------------------------------------------------
        const int MAX_STOCK_PRICE = 30000;
        const int NOW_TIME_IDX = 0;

        public int code_ { get; set; }

        public string codeString()
        {
            return String.Format("{0:D6}", code_);
        }

        public string name_ { get; }
        public virtual bool isBuyedStock()
        {
            return false;
        }
        public bool alreadyOrder_ { get; set; }

        public List<CandleData>[] priceTable_;      // 가격 테이블
        public BackTestHistory[] backTestHistory_;  // 백테스팅용

        public List<CandleData> priceTable(PRICE_TYPE type)
        {
            lock (dataLock_) {
                List<CandleData> priceTable = priceTable_[(int) type];
                if (priceTable.Count == 0) {
                    return null;
                }
                return priceTable;
            }
        }

        STOCK_EVALUATION[] evalTotal_;     //총 평가
        public STOCK_EVALUATION evalTotal(PRICE_TYPE type)
        {
            lock (dataLock_) {
                return evalTotal_[(int) type];
            }
        }

        //-------------------------------------------------------------
        //-------------------------------------------------------------
        // 함수
        public StockData(int code, string name)
        {
            code_ = code;
            name_ = name;

            priceTable_ = new List<CandleData>[(int) PRICE_TYPE.MAX];

            backTestHistory_ = new BackTestHistory[(int) PRICE_TYPE.MAX];
            evalTotal_ = new STOCK_EVALUATION[(int) PRICE_TYPE.MAX];

            for (int i = 0; i < (int) PRICE_TYPE.MAX; ++i) {
                priceTable_[i] = new List<CandleData>();
                backTestHistory_[i] = new BackTestHistory();
            }

            alreadyOrder_ = false;
            dataLock_ = new object();
        }

        //-------------------------------------------------------------
        // 복사 메소드
        protected void copyPriceDatas(StockData clone)
        {
            for (int type = 0; type < priceTable_.Length; ++type) {
                for (int index = 0; index < priceTable_[type].Count; ++index) {
                    clone.updatePrice((PRICE_TYPE) type, index, priceTable_[type][index]);
                }
            }

            evalTotal_.CopyTo(clone.evalTotal_, 0);
            backTestHistory_.CopyTo(clone.backTestHistory_, 0);
        }

        public virtual Object Clone()
        {
            StockData clone = new StockData(code_, name_);
            this.copyPriceDatas(clone);
            return clone;
        }

        public int priceDataCount(PRICE_TYPE priceType)
        {
            return priceTable_[(int) priceType].Count;
        }

        public int nowPrice(PRICE_TYPE type)
        {
            List<CandleData> priceTable = this.priceTable(type);
            if (priceTable == null) {
                return 0;
            }
            return priceTable[NOW_TIME_IDX].price_;
        }

        //-------------------------------------------------------------
        // 업데이트 시간 체크 (30초안에 데이터를 못받으면 다시 요청한다)
        public void loadPriceData()
        {
            foreach (PRICE_TYPE priceType in Enum.GetValues(typeof(PRICE_TYPE))) {
                if (priceType == PRICE_TYPE.MAX) {
                    continue;
                }
                int index = (int) priceType;
                int count = priceTable_[index].Count;
                if (count == 0) {
                    StockBot.getInstance.requestStockData(priceType, code_);
                    Logger.getInstance.print(KiwoomCode.Log.주식봇, "{0} 주식 {1}분봉 데이터 요청", name_, priceType);
                    return;
                }
            }
        }

        //-------------------------------------------------------------
        //가격표관련 처리
        public void clearPrice(PRICE_TYPE type)
        {
            priceTable_[(int) type].Clear();
        }
        public void updatePrice(PRICE_TYPE type, int index, CandleData priceData)
        {
            priceTable_[(int) type].Insert(index, priceData);
        }

        private double bnfPercent_ = PublicVar.bnfPercent;
        public void setBnfPercent(PRICE_TYPE type, double percent)
        {
            bnfPercent_ = percent;
            {
                List<CandleData> priceTable = this.priceTable(type);
                if (priceTable == null) {
                    return;
                }
                BNFCalculater bnfCalculater = new BNFCalculater();
                bnfCalculater.setBnfPercent(bnfPercent_);
                bnfCalculater.calculate(ref priceTable);
            }
        }
        public double bnfPercent()
        {
            return bnfPercent_;
        }

        public void calculatePrice(PRICE_TYPE type)
        {
            List<CandleData> priceTable = this.priceTable(type);
            if (priceTable == null) {
                return;
            }
            Calculater calculater;
            calculater = new AvgCalculater();
            calculater.calculate(ref priceTable);

            calculater = new BollingerCalculater();
            calculater.calculate(ref priceTable);

            calculater = new MacdCalculater();
            calculater.calculate(ref priceTable);

            {
                this.findBnfRate(type);
            }

            calculater = new RSI14Calculater();
            calculater.calculate(ref priceTable);

            calculater = new StochasticCalculater();
            calculater.calculate(ref priceTable);

            calculater = new ADXCalculater();
            calculater.calculate(ref priceTable);

            calculater = new WilliamsCalculater();
            calculater.calculate(ref priceTable);

            calculater = new CCICalculater();
            calculater.calculate(ref priceTable);

            calculater = new ATRCalculater();
            calculater.calculate(ref priceTable);

            calculater = new HighsLowsCalculater();
            calculater.calculate(ref priceTable);

            calculater = new UltimateCalculater();
            calculater.calculate(ref priceTable);

            calculater = new ROCCalculater();
            calculater.calculate(ref priceTable);

            calculater = new BullBearCalculater();
            calculater.calculate(ref priceTable);

            Evaluation stockAnalysis = new Evaluation();
            evalTotal_[(int) type] = stockAnalysis.analysis(priceTable);

            //   stockAnalysis_[(int)type].analysis(priceTable);
            /*
            int count = Math.Min(priceTable.Count - 1, PublicVar.avgMax[(int) AVG_SAMPLEING.AVG_MAX - 1]);
            for (int i = priceTable.Count - 1; i >= count ; --i) {
                priceTable.RemoveAt(i);
            }
            */
        }
        void findBnfRate(PRICE_TYPE type)
        {
            List<CandleData> priceTable = this.priceTable(type);
            if (priceTable == null) {
                return;
            }
            int lastAgo = priceTable.Count - PublicVar.bnfTerm;     // bnf 만들려면 25일 데이터가 필요함.

            List<double> percentList = new List<double>();
            for (double i = PublicVar.bnfMin; i < PublicVar.bnfMax; i += 0.1f) {
                percentList.Add(i);
            }

            int index = 0;
            int maxIndex = 0;
            int maxCount = 0;

            // bnf 하단선과 캔들봉이 많이 맞는곳을 찾기
            List<int> hitCount = new List<int>();
            foreach (double percent in percentList) {
                this.setBnfPercent(type, percent);

                int count = 0;
                for (int i = 0; i < lastAgo; ++i) {
                    double bnfMinPrice = priceTable[i].calc_[(int) EVALUATION_DATA.BNF_LOWER];
                    double bnfMaxPrice = priceTable[i].calc_[(int) EVALUATION_DATA.BNF_UPPER];

                    double minPrice = (double) priceTable[i].minPrice();
                    double maxPrice = (double) priceTable[i].maxPrice();
                    double centerPrice = priceTable[i].centerPrice_;

                    if (minPrice <= bnfMinPrice) {
                        // 하단과 중간선을 기준으로 10% 이하를 지날때를 counting
                        double temp = minPrice + ((centerPrice - minPrice) / 100.0f * 30.0f);
                        if (bnfMinPrice < temp) {
                            count++;
                        }
                    } else if (maxPrice >= bnfMaxPrice) {
                        double temp = maxPrice + ((centerPrice - maxPrice) / 100.0f * 30.0f);
                        if (bnfMaxPrice > temp) {
                            count++;
                        }
                    }
                }
                if (maxCount < count) {
                    maxIndex = index;
                    maxCount = count;
                }
                hitCount.Add(count);
                index++;
            }
            double optimizationValue = percentList[maxIndex];
            Logger.getInstance.print(KiwoomCode.Log.주식봇, "{0} 주식 {1}분봉, bnf 최적 {2:0.##}%", this.name_, type, optimizationValue);
            this.setBnfPercent(type, optimizationValue);
        }

        //---------------------------------------------------------------------------
        // 백테스팅 전략 관련
        public ref BackTestHistory backTestHistory(PRICE_TYPE type)
        {
            return ref backTestHistory_[(int) type];
        }

        public double backTestProfit(PRICE_TYPE type)
        {
            BackTestHistory history = this.backTestHistory(type);
            return history.profitRate_;
        }
        
        EvalWeightAverage evalWeightAverage_ = null;
        public void doBackTesting(PRICE_TYPE type)
        {
            List<CandleData> priceTable = this.priceTable(type);
            if (priceTable == null) {
                return;
            }
            if (backTestHistory_[(int) type] != null) {
                backTestHistory_[(int) type] = null;
                backTestHistory_[(int) type] = new BackTestHistory();
            }

            TradeModule module = StockBot.getInstance.tradeModule();
            module.setTradeModule(code_, type);
            module.setEvalWeightAverage(evalWeightAverage_);
            module.doTest();
            module = null;
        }

        internal void setEvalWeightAverage(EvalWeightAverage weightAvg)
        {
            if (evalWeightAverage_ != null) {
                evalWeightAverage_ = null;
            }
            evalWeightAverage_ = weightAvg;
        }

        public DateTime backTestFirstTradeDate(PRICE_TYPE type)
        {
            List<CandleData> priceTable = this.priceTable(type);
            if (priceTable == null) {
                return DateTime.Now;
            }
            BackTestHistory history = this.backTestHistory(type);
            return history.firstTradeDate();
        }

        //-------------------------------------------------------------
        // 가격 정상 판단 처리
        public virtual bool isWathing(PRICE_TYPE type)
        {
            //일이 강력 매수인 것을 상대로.
            // 최대 3만원이하 주식을 대상
            //if (this.nowPrice(PRICE_TYPE.HOUR1) > MAX_STOCK_PRICE) {
            //    return false;
            //}

            return true;
        }

        //-------------------------------------------------------------
        // 테스트로 볼린저로만 판단, 이후에 investing 처럼 수정..
        public bool isBuyTime(PRICE_TYPE priceType)
        {
            TradeModule module = StockBot.getInstance.tradeModule();
            module.setTradeModule(code_, priceType);
            if (module.buy(NOW_TIME_IDX) == false) {
                module = null;
                return false;
            }
            module = null;

            return true;
        }
        public virtual double profitPriceRate(PRICE_TYPE type)
        {
            return 0.0f;
        }

        public bool isSellTime(PRICE_TYPE priceType)
        {
            double nowProfit = this.profitPriceRate(priceType);

            TradeModule module = StockBot.getInstance.tradeModule();
            module.setTradeModule(code_, priceType);
            if (module.sell(NOW_TIME_IDX, nowProfit) == false) {
                module = null;
                return false;
            }
            module = null;

            return true;
        }
    }

    class BuyedStockData :StockData {
        public int buyCount_ { get; set; }
        public int buyPrice_ { get; set; }

        public BuyedStockData(int code, string name, int buyCount, int buyPrice) : base(code, name)
        {
            buyCount_ = buyCount;
            buyPrice_ = buyPrice;
        }

        // 산주식은 무족건 워칭.
        public override bool isWathing(PRICE_TYPE type)
        {
            return true;
        }

        public override Object Clone()
        {
            BuyedStockData clone = new BuyedStockData(code_, name_, buyCount_, buyPrice_);
            this.copyPriceDatas(clone);
            return clone;
        }

        public override Boolean isBuyedStock()
        {
            return true;
        }

        public int totalBuyPrice()
        {
            return buyCount_ * buyPrice_;
        }

        public int profitPrice(PRICE_TYPE type)
        {
            int nowValue = this.nowPrice(type) * buyCount_;
            int myValue = totalBuyPrice();
            return nowValue - myValue;
        }

        public override double profitPriceRate(PRICE_TYPE type)
        {
            int now = this.nowPrice(type);
            if (now == 0) {
                return 0;
            }
            double nowPrice = (double) now;
            double profitRate = (nowPrice - (double) buyPrice_) / nowPrice * 100;
            return profitRate;
        }
    }
}
