using StockLibrary.StrategyManager;
using System;

namespace StockLibrary
{
    // 매 결과를 저장 시킨 다음에, 이걸 활용하자.
    // 12시간 마다 갱신하지 말고 매 시간마다 이 정보를 이용해서 재 테스팅을 함.
    // 만약 rsq 나, win rate 가 범위를 벗어나면 다시 찾도록 로직을 수정하자

    public class TradeStrategyTestResult
    {
        public double lostCut_ { get; set; }
        public double targetProfit_ { get; set; }
        public StrategyManagement fundManage_ { get; }
        public int daysForAchieveGoalRate_ = 0;
        public int days_ = 0;
        public double totalAccount_;        // 결과 예수금
        public TradeStrategyTestResult(StrategyManagement fund, double lostCut, double targetProfit)
        {
            this.fundManage_ = fund;
            this.lostCut_ = lostCut;
            this.targetProfit_ = targetProfit;
        }

        public TradeStrategyTestRecoder machineRecoder_ = null;
        public double totalProfit()
        {
            if (this.machineRecoder_ == null) {
                return double.MinValue;
            }
            return this.machineRecoder_.grossEarnings_;
        }
        public int tradeCount()
        {
            if (this.machineRecoder_ == null) {
                return 0;
            }

            return this.machineRecoder_.tradingCount();
        }
        public int itemCount()
        {
            if (this.machineRecoder_ == null) {
                return 0;
            }

            return this.machineRecoder_.itemCount_;
        }
        public double winRate()
        {
            if (this.machineRecoder_ == null) {
                return 0;
            }
            return this.machineRecoder_.winRate_;
        }

        public double avgProfit()
        {
            if (this.machineRecoder_ == null) {
                return 0;
            }
            return this.machineRecoder_.avgProfit_;
        }

        public double deviation()
        {
            if (this.machineRecoder_ == null) {
                return 0;
            }
            return this.machineRecoder_.deviation_;
        }

        double rsq_ = double.MinValue;
        public double rsq()
        {
            if (this.machineRecoder_ == null) {
                return double.MinValue;
            }
            if (this.rsq_ == double.MinValue) {
                this.rsq_ = this.machineRecoder_.rsq_;
            }
            return this.rsq_;
        }

        public double predictedProfit(int min)
        {
            if (this.machineRecoder_ == null) {
                return double.MinValue;
            }
            return this.machineRecoder_.predictedProfit(min);
        }

        //종합 평가
        public double overallAssessment(int min)
        {
            if (this.machineRecoder_ == null) {
                return Int64.MinValue;
            }
            double maxDeviation = 5000;
            // 승률 * rsq * 전체 이익 * (5000-표준편차)
            var over = this.winRate()
                * this.rsq()
                * (this.totalProfit() / 1000) 
             /*   * (this.totalAccount_ / 1000) */
                * (maxDeviation - this.deviation())
                * (this.predictedProfit(min) / 1000)
                * (this.daysForAchieveGoalRate_ + 1)
                ;
            return over;
        }
    }
}
