using StockLibrary;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HappyFuture
{
    class FutureTradeRecord: TradeRecord
    {
        public TRADING_STATUS position_ = TRADING_STATUS.모니터닝;

        // 실 매매용
        public FutureTradeRecord(string stockName, string stockCode, DateTime startDate, DateTime endDate, double buyPrice, double sellPrice, int count)
            : base(stockName, stockCode, startDate, endDate, buyPrice, sellPrice, count)
        {

        }
        // 백테스트 용
        public FutureTradeRecord(string stockCode, int timeIndex, DateTime startDate, DateTime endDate, double buyPrice, double sellPrice, int count)
            : base(stockCode, timeIndex, startDate, endDate, buyPrice, sellPrice, count)
        {
        }

        public FutureTradeRecord(string stockCode, int timeIndex, DateTime startDate, DateTime endDate, double buyPrice, double sellPrice, TRADING_STATUS position)
          : base(stockCode, timeIndex, startDate, endDate, buyPrice, sellPrice, 1)
        {
            this.position_ = position;
        }

        public double profit(FutureData futureData)
        {
            var profit = futureData.calcProfit(this.buyPrice_, this.sellPrice_, this.buyCount_, this.position_);
            profit -= PublicFutureVar.pureFeeTex;  // 수수료 세금도 
            this.profit_ = profit;
            return this.profit_;
        }

        public double profitRate()
        {
            double rate = 0.0f;
            switch (this.position_) {
                case TRADING_STATUS.매수:
                rate = ((this.sellPrice_ - this.buyPrice_) / this.buyPrice_) * this.buyCount_;
                break;
                case TRADING_STATUS.매도:
                rate = ((this.sellPrice_ - this.buyPrice_) / this.buyPrice_) * this.buyCount_ * -1;
                break;
            }
            this.profitRate_ = rate;
            return rate;
        }
    }

    internal class FutureBackTestRecoder: BackTestRecoder
    {
        public double tickValue_;
        public double tickStep_;

        // 테스트로는 1개를 사서 그걸로 판단한다.
        public void addTrade(string stockCode, int timeIndex, DateTime startDate, DateTime endDate, double buyPrice, double sellPrice, TRADING_STATUS position, double minProfit, double maxProfit)
        {
            FutureTradeRecord data = new FutureTradeRecord(stockCode, timeIndex, startDate, endDate, buyPrice, sellPrice, position);
            data.minProfit_ = minProfit;
            data.maxProfit_ = maxProfit;
            this.recodeList_.Add(data);
        }

        public void evaluation(FutureData futureData)
        {
            this.resetValue();
            if (this.tradeCount_ < 1) {
                return;
            }
            double weight = (this.tradeCount_ * (this.tradeCount_ + 1)) / 2;        //sigma

            if (this.tradeCount_ <= 0) {
                return;
            }

            this.minProfit_ = 100000000;
            this.maxProfit_ = -100000000;
            int index = 0;
            foreach (var trade in this.recodeList_) {
                var fTread = trade as FutureTradeRecord;
                index++;
                //buyCount 가 음수면 매도 포지션, 양수면 매수 포지션
                double profit = fTread.profit(futureData);
                this.totalProfit_ = this.totalProfit_ + profit;
                if (profit > PublicVar.tradeTax) {
                    this.winCount_++;
                    this.expectedWinRate_ = this.expectedWinRate_ + (index / weight);     // 기대값 가중치
                }
                double rate = fTread.profitRate();
                fTread.profitRate_ = rate;
                this.avgProfitRate_ = this.avgProfitRate_ + rate;

                this.totalHaveTime_ = this.totalHaveTime_ + fTread.haveTime();

                this.minProfit_ = Math.Min(trade.minProfit_, this.minProfit_);
                this.maxProfit_ = Math.Max(trade.maxProfit_, this.maxProfit_);
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
        }

        double minProfit()
        {
            if (this.tradeCount_ <= 0) {
                return double.MinValue;
            }

            var query = (from recode in this.recodeList_
                         select recode.profit_).Min();
            return query;
        }
    }

    public class FutureTradeModuleFilter: TradeModuleFilter
    {
        public FutureTradeModuleFilter()
        {
        }

        public override IEnumerable<BackTestRecoder> doOrderBy(IEnumerable<BackTestRecoder> srcList)
        {
            if (srcList == null) {
                return null;
            }
            if (srcList.Count() == 0) {
                return null;
            }

            var query = (from recode in srcList
                         orderby
                             recode.evalValue_ descending,         // 종합 평가 기준 큰 순서
                             recode.deviation_ ascending,          // 표준편차가 적고
                             recode.totalProfit_ descending,
                             recode.expectedWinRate_ descending,   // 기대승율이 높고
                             recode.tradeCount_ descending
                         select recode).Take(PublicVar.tradeModuleCount);
            return query;
        }

        public override bool doFilter(BackTestRecoder recode, double lostCut)
        {
            if (recode == null) {
                return false;
            }
            if (recode.tradeCount_ == 0) {
                return false;
            }

            // 에러 검사
            const double ERR_PROFIT = 5000;
            if (recode.maxProfit_ > ERR_PROFIT) {
                return false;
            }
            if (recode.minProfit_ < -ERR_PROFIT) {
                return false;
            }

            // 신뢰 가능한 백테스팅 숫자
            if (recode.tradeCount_ < PublicVar.allowTradeCount) {
                return false;
            }

            if (Math.Abs(recode.maxProfit_) <= Math.Abs(recode.minProfit_)) {
                return false;
            }

            // 승률
            if (recode.expectedWinRate_ < PublicVar.allowTradeWinRate) {
                return false;
            }

            // 평균이익이 100$는 넘어야..
            //if ((recode.avgProfit_ - recode.deviation_) < PublicFutureVar.feeTax) {
            //    return false;
            //}

            return true;
        }
    }
}
