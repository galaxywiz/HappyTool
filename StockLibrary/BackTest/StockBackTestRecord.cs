using System;
using System.Collections.Generic;
using static StockLibrary.StrategyModuleList;

namespace StockLibrary
{

    public class TradeRecord
    {
        public string stockCode_ { get; }
        public string stockName_ { get; }

        public int timeIndex_ { get; }
        public System.DateTime buyDate_ { get; }
        public System.DateTime sellDate_ { get; }
        public double buyPrice_ { get; }
        public double sellPrice_ { get; }
        public int buyCount_ { get; }
        public TimeSpan haveTime_ { get; }

        const double MAX_DOUBLE = 1000000;
        const double MIN_DOUBLE = -1000000;
        //보유중 최고이익, 최저 이익
        public double minProfit_ = MAX_DOUBLE;
        public double maxProfit_ = MIN_DOUBLE;

        // 실 매매용
        public TradeRecord(string stockName, string stockCode, DateTime startDate, DateTime endDate, double buyPrice, double sellPrice, int count)
        {
            this.stockName_ = stockName;
            this.stockCode_ = stockCode;
            this.buyDate_ = startDate;
            this.sellDate_ = endDate;
            this.buyPrice_ = buyPrice;
            this.sellPrice_ = sellPrice;
            this.buyCount_ = count;
            this.haveTime_ = endDate - startDate;
        }

        // 백테스트 용
        public TradeRecord(string stockCode, int timeIndex, DateTime startDate, DateTime endDate, double buyPrice, double sellPrice, int count)
        {
            this.stockCode_ = stockCode;
            this.timeIndex_ = timeIndex;
            this.buyDate_ = startDate;
            this.sellDate_ = endDate;
            this.buyPrice_ = buyPrice;
            this.sellPrice_ = sellPrice;
            this.buyCount_ = count;
            this.haveTime_ = endDate - startDate;
        }

        public int haveTime()
        {
            return (int) this.haveTime_.TotalMinutes;
        }

        public double totalBuyPrice()
        {
            return this.buyPrice_ * this.buyCount_;
        }

        public double totalSellPrice()
        {
            return this.sellPrice_ * this.buyCount_;
        }

        const double STAND = double.MinValue;
        public double profit_ = STAND;
        public virtual double totalProfit()
        {
            if (this.profit_ <= STAND) {
                this.profit_ = (this.sellPrice_ - this.buyPrice_) * this.buyCount_;
                this.profit_ = this.profit_ - (this.profit_ * PublicVar.tradeTax);
            }
            return this.profit_;
        }

        public double profitRate_ = STAND;
        public double totalProfitRate()
        {
            if (this.profitRate_ <= STAND) {
                double buyPrice = this.totalBuyPrice();
                double sellPrice = this.totalSellPrice();
                return UtilLibrary.Util.calcProfitRate(buyPrice, sellPrice);
            }
            return this.profitRate_;
        }
    }

    // 주식 매매 기록
    public class BackTestRecoder
    {
        public List<TradeRecord> recodeList_ { get; set; }
        public StrategyModule buyTradeModule_ = null;
        public StrategyModule sellTradeModule_ = null;

        public int tradeCount_ = 0;
        public int totalHaveTime_ = 0;
        public int avgHaveTime_ = 0;

        //이익율
        public double avgProfitRate_ { get; set; }      // 1거래당 평균 이익율   
        public double totalProfit_ { get; set; }        // 전체 거래에 대한 이익
        public double avgProfit_;           // 1거래당 평균이익

        //https://learnx.tistory.com/entry/%ED%86%B5%EA%B3%84%EC%9D%98-%EA%B8%B0%EC%B4%88%EC%9D%B8-%ED%8F%89%EA%B7%A0-%EB%B6%84%EC%82%B0-%ED%91%9C%EC%A4%80%ED%8E%B8%EC%B0%A8
        //통계 확율을 좀 적극적응로 넣자
        public double expectedWinRate_;       // 다음에도 승리 할지에 대한 기대값
        public double varation_ = 0.0f;          //분산
        public double deviation_ = 0.0f;       // 표준 편차 (이게 작으면 모든값이 평균 근처에 논다는뜻. 기대 값이 더 정확하지)

        //승률
        public int winCount_ = 0;
        public double winRate_ = 0.0f;

        //보유중 최고이익, 최저 이익
        public double minProfit_ = 0.0f;
        public double maxProfit_ = 0.0f;

        public double evalValue_ = 0.0f;      // 종합 평가
        // vud
        public BackTestRecoder()
        {
            this.recodeList_ = new List<TradeRecord>();
        }

        ~BackTestRecoder()
        {
            this.recodeList_.Clear();
        }

        public string getName()
        {
            if (buyTradeModule_ == null || sellTradeModule_ == null) {
                return "";
            }
            return buyTradeModule_.getName() + sellTradeModule_.getName();
        }

        public List<TradeRecord> getRecordData()
        {
            return this.recodeList_;
        }

        public void addTrade(string stockCode, int timeIndex, DateTime startDate, DateTime endDate, double buyPrice, double sellPrice, int count)
        {
            TradeRecord data = new TradeRecord(stockCode, timeIndex, startDate, endDate, buyPrice, sellPrice, count);
            this.recodeList_.Add(data);
        }

        // 과거 이력 우선으로 소팅 한다. 그래야 매매를 순차 테스팅이 되지
        protected void sortDesc()
        {
            this.recodeList_.Sort(delegate (TradeRecord A, TradeRecord B) {
                if (A == null || B == null) {
                    return 0;
                }
                if (A.timeIndex_ > B.timeIndex_) {
                    return -1;
                }
                if (A.timeIndex_ < B.timeIndex_) {
                    return 1;
                }
                return 0;
            });
        }

        // 첫 거래일을 가지고 온다.
        public DateTime firstTradeDate()
        {
            if (this.recodeList_.Count == 0) {
                return DateTime.MinValue;
            }

            return this.recodeList_[0].buyDate_;
        }

        // 상승 추세?
        public bool isUpperTrade()
        {
            if (this.recodeList_.Count == 0) {
                return false;
            }
            if (this.recodeList_.Count == 1) {
                return this.recodeList_[0].profit_ > 0.0f;
            }

            int idx = this.recodeList_.Count - 1;
            if (this.recodeList_[idx].profit_ < 0.0f) {
                return false;
            }

            int idx2 = this.recodeList_.Count - 2;
            return this.recodeList_[idx].profit_ > this.recodeList_[idx2].profit_;
        }

        protected void resetValue()
        {
            this.tradeCount_ = this.recodeList_.Count;

            this.avgProfitRate_ = 0.0f;
            this.totalProfit_ = 0.0f;
            this.avgProfit_ = 0.0f;

            this.winRate_ = 0.0f;
            this.winCount_ = 0;

            this.totalHaveTime_ = 0;
            this.avgHaveTime_ = 0;

            this.expectedWinRate_ = 0;
            this.deviation_ = 0;
            this.varation_ = 0;

            this.maxProfit_ = 0;
            this.minProfit_ = 0;

            this.evalValue_ = double.MinValue;
        }
    }

    public class StockBackTestRecoder: BackTestRecoder
    {
        public void calcEval()
        {
            this.resetValue();
            if (this.tradeCount_ < 1) {
                return;
            }

            double weight = (this.tradeCount_ * (this.tradeCount_ + 1)) / 2;        //sigma

            int index = 0;
            foreach (var recode in this.recodeList_) {
                index++;
                if (recode.totalProfitRate() > PublicVar.tradeTax) {
                    this.winCount_++;
                    this.expectedWinRate_ = this.expectedWinRate_ + (index / weight);
                }
                this.avgProfitRate_ += recode.totalProfitRate();
                this.totalProfit_ += recode.totalProfit();
                this.totalHaveTime_ += recode.haveTime();
            }
            this.avgProfitRate_ = this.avgProfitRate_ / this.tradeCount_;
            this.avgHaveTime_ = this.totalHaveTime_ / this.tradeCount_;
            this.avgProfit_ = this.totalProfit_ / this.tradeCount_;
            this.winRate_ = this.winCount_ / (double) this.tradeCount_;

            // 표준 편차 구하기
            double power = 0.0f;
            foreach (var trade in this.recodeList_) {
                var dev = trade.profit_ - this.avgProfit_;      // 편차 = 변량 - 평균
                power = power + (dev * dev);                // 편차 제곱의 합
            }
            this.varation_ = power / this.tradeCount_;                // 분산
            this.deviation_ = Math.Sqrt(this.varation_);              // 표준편차

            // 종합 평가
            this.evalValue_ = this.avgProfit_ - this.deviation_;
            // evalValue_ = (recode.totalProfit_ * recode.tradeCount_ * recode.winRate_) descending,

        }
    }

    public class SimulateBackTestRecoder: BackTestRecoder
    {
        public string log_ { get; set; }

        public void calcEval(double lastMoney, double initMoney)
        {
            this.avgProfitRate_ = 0.0f;
            this.sortDesc();
            if (this.recodeList_.Count < 1) {
                return;
            }
            this.tradeCount_ = this.recodeList_.Count;

            this.avgProfitRate_ = UtilLibrary.Util.calcProfitRate(initMoney, lastMoney);

            this.recodeList_.Sort(delegate (TradeRecord a, TradeRecord b) {
                return a.buyDate_.CompareTo(b.buyDate_);
            });
        }

        public double winRate()
        {
            int total = this.recodeList_.Count;
            if (total == 0) {
                return 0.0f;
            }
            return this.winCount_ / (double) total * 100.0f;
        }
    }
}
