using StockLibrary.StrategyManager.ProfitSafer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using UtilLibrary.Data;

namespace StockLibrary
{
    public class BackTestRecode
    {
        public string code_;
        public string buyModule_;
        public string sellModule_;
        public double tradeModuelAvgProfit_;
        public TRADING_STATUS position_;
        public DateTime buyTime_;
        public double buyPrice_;
        public DateTime payoffTime_;
        public double payoffPrice_;
        public TimeSpan haveTime_;
        public double minProfit_;
        public double maxProfit_;
        public double oneProfit_;
        public int buyCount_;
        public double profit_;
        public int ticks_;
        public string whyPayOff_;
        public PAY_OFF_CODE payOffCode_;
        public double grossEarnings_;       // 누적수익금 (차트용)
        public double grossEarningsMinus_;  // 역매매시 수익금.
        public double rsq_;
        public double predictedProfit_;
        public string buyLog_;
        public string sellLog_;
    }

    public class TradeStrategyTestRecoder
    {
        public List<BackTestRecode> recodes_ = new List<BackTestRecode>();
        public DateTime startTime_;
        public DateTime endTime_;
        private int tradeCount_;
        public double winCount_;
        public double winRate_;

        public double grossEarnings_;       // 총 이익금
        public double grossEarningsMinus_;  // 역매매시 이익금
        public double saveMyMoney_;

        readonly List<int> profitPayOff_ = new List<int>();       // 이익 payoff code
        readonly List<int> lostPayOff_ = new List<int>();         // 손해 payoff code

        readonly Bot bot_;
        public TradeStrategyTestRecoder(Bot bot)
        {
            this.bot_ = bot;

            foreach (PAY_OFF_CODE code in Enum.GetValues(typeof(PAY_OFF_CODE))) {
                this.profitPayOff_.Add(0);
                this.lostPayOff_.Add(0);
            }
        }

        public void clear()
        {
            this.recodes_.Clear();
        }

        public void addRecode(BackTestRecode recode)
        {
            this.recodes_.Add(recode);
            if (recode.profit_ > PublicFutureVar.pureFeeTex) {
                this.profitPayOff_[(int) recode.payOffCode_]++;
            }
            else {
                this.lostPayOff_[(int) recode.payOffCode_]++;
            }
        }

        public double avgProfit_;
        public double varation_;
        public double deviation_;

        public double avgMinProfit_;
        public double avgMaxProfit_;
        public double minProfit_ = double.MaxValue;
        public double maxProfit_ = double.MinValue;

        public TimeSpan maxHaveTime_ = TimeSpan.MinValue;

        public int tradingCount()
        {
            return this.recodes_.Count;
        }

        public double calcTotalProfit()
        {
            double gross = 0;
            foreach (var recode in this.recodes_) {
                var oneProfit = recode.oneProfit_;
                var originProfit = oneProfit - PublicFutureVar.pureFeeTex;
                gross = this.grossEarnings_ + (originProfit * recode.buyCount_);
            }
            return gross;
        }

        public double minProfitRate_ = double.MaxValue;
        public double maxProfitRate_ = double.MinValue;
        public void calcTotal()
        {
            this.tradeCount_ = this.tradingCount();
            this.grossEarnings_ = 0;
            this.grossEarningsMinus_ = 0;
            this.winCount_ = 0;
            this.winRate_ = 0;

            this.startTime_ = DateTime.MaxValue;
            this.endTime_ = DateTime.MinValue;

            if (this.tradeCount_ == 0) {
                return;
            }

            double minProfit = 0;
            double maxProfit = 0;
            foreach (var recode in this.recodes_) {
                var oneProfit = recode.oneProfit_;
                var originProfit = oneProfit - PublicFutureVar.pureFeeTex;
                if (originProfit > 0) {
                    this.winCount_++;
                }
                this.grossEarnings_ = this.grossEarnings_ + (originProfit * recode.buyCount_);

                minProfit = minProfit + recode.minProfit_;
                maxProfit = maxProfit + recode.maxProfit_;

                this.grossEarningsMinus_ = this.grossEarningsMinus_ + ((-oneProfit - PublicFutureVar.pureFeeTex) * recode.buyCount_);

                this.minProfit_ = Math.Min(recode.minProfit_, this.minProfit_);
                this.maxProfit_ = Math.Max(recode.maxProfit_, this.maxProfit_);
                if (this.maxHaveTime_ < recode.haveTime_) {
                    this.maxHaveTime_ = recode.haveTime_;
                }
            }

            this.startTime_ = this.recodes_[0].buyTime_;
            var lastRecode = this.recodes_[this.recodes_.Count - 1];
            this.endTime_ = lastRecode.payoffTime_;
            lastRecode.grossEarnings_ = this.grossEarnings_;
            lastRecode.grossEarningsMinus_ = this.grossEarningsMinus_;

            this.winRate_ = this.winCount_ / this.tradeCount_;

            this.avgProfit_ = this.grossEarnings_ / this.tradeCount_;
            this.avgMinProfit_ = minProfit / this.tradeCount_;
            this.avgMaxProfit_ = maxProfit / this.tradeCount_;

            // 표준 편차 구하기
            double power = 0.0f;
            foreach (var trade in this.recodes_) {
                var dev = trade.oneProfit_ - this.avgProfit_;      // 편차 = 변량 - 평균
                power = power + (dev * dev);                       // 편차 제곱의 합
            }
            this.varation_ = power / this.tradeCount_;             // 분산
            this.deviation_ = Math.Sqrt(this.varation_);           // 표준편차

            this.calcRSQ();

            lastRecode.rsq_ = this.rsq_;
            lastRecode.predictedProfit_ = this.predictedProfit(PublicVar.fundUpdateHours * 60);

            this.evaluate();
        }

        public double rsq_ = double.MinValue;
        public double yIntercept_ = double.MinValue;
        public double slope_ = double.MinValue;

        void calcRSQ()
        {
            int count = this.recodes_.Count;
            if (count == 0) {
                return;
            }
            double[] index = new double[count];
            double[] accountMoney = new double[count];

            for (int i = 0; i < this.recodes_.Count; ++i) {
                var recode = this.recodes_[i];
                var time = recode.buyTime_ - this.startTime_;
                index[i] = time.TotalMinutes;
                accountMoney[i] = recode.grossEarnings_;
            }

            CalcLiner.LinearRegression(index, accountMoney, out this.rsq_, out this.yIntercept_, out this.slope_);
        }

        public double predictedProfit(int min)
        {
            int count = this.recodes_.Count;
            if (count == 0) {
                return double.MinValue;
            }
            if (this.startTime_ == DateTime.MinValue) {
                return double.MinValue;
            }

            var elepTime = this.endTime_ - this.startTime_;

            var predic = (this.slope_ * (elepTime.TotalMinutes + min)) + this.yIntercept_;
            predic = predic - this.recodes_[this.recodes_.Count - 1].grossEarnings_;
            return predic;
        }

        public double itemVaration_ = double.MinValue;          // 종목당 분산
        public double itemDeviation_ = double.MinValue;         // 종목당 표준편차
        public int itemCount_ = 0;
        void evaluate()
        {
            //code , profit
            var recodeEach = new Dictionary<string, double>();

            foreach (var codeRecode in this.recodes_) {
                var thisCode = codeRecode.code_;
                if (recodeEach.ContainsKey(thisCode)) {
                    continue;
                }

                double sum = 0.0f;
                foreach (var recode in this.recodes_) {
                    if (thisCode != recode.code_) {
                        continue;
                    }
                    sum = sum + recode.profit_;
                }
                recodeEach[thisCode] = sum;
            }
            this.itemCount_ = recodeEach.Count;
            if (this.itemCount_ == 0) {
                return;
            }
            // 각 종목간 통합 표준편차 / 분산
            double power = 0.0f;
            foreach (var keyPair in recodeEach) {
                var dev = keyPair.Value - this.avgProfit_;       // 편차 = 변량 - 평균
                power = power + (dev * dev);                     // 편차 제곱의 합
            }
            this.itemVaration_ = power / this.itemCount_;                // 분산
            this.itemDeviation_ = Math.Sqrt(this.itemVaration_);              // 표준편차
        }

        public enum BACKTEST_RECODE_TABLE_COLUMN
        {
            코드,

            포지션,
            매수시점,
            매수가격,
            청산시점,
            청산가격,
            보유시간,

            매수갯수,
            이익,
            RSQ,
            매수로그,
            매도로그,
            다음예상이익,
            누적수익금,
            청산코드,

            최소1주이익,
            최대1주이익,
            청산시1주이익,

            매수모듈,
            매도모듈,
            모듈평균이익,
        }
        public DataTable getRecodeDataTable(bool final)
        {
            DataTable dt = new DataTable();
            foreach (BACKTEST_RECODE_TABLE_COLUMN name in Enum.GetValues(typeof(BACKTEST_RECODE_TABLE_COLUMN))) {
                if (name == BACKTEST_RECODE_TABLE_COLUMN.코드
                    || name == BACKTEST_RECODE_TABLE_COLUMN.매수모듈
                    || name == BACKTEST_RECODE_TABLE_COLUMN.매도모듈
                    || name == BACKTEST_RECODE_TABLE_COLUMN.포지션
                    || name == BACKTEST_RECODE_TABLE_COLUMN.보유시간
                    || name == BACKTEST_RECODE_TABLE_COLUMN.청산코드
                    || name == BACKTEST_RECODE_TABLE_COLUMN.매수로그
                    || name == BACKTEST_RECODE_TABLE_COLUMN.매도로그) {
                    dt.Columns.Add(name.ToString(), typeof(string));
                }
                else if (name == BACKTEST_RECODE_TABLE_COLUMN.매수시점
                    || name == BACKTEST_RECODE_TABLE_COLUMN.청산시점) {
                    dt.Columns.Add(name.ToString(), typeof(DateTime));
                }
                else {
                    dt.Columns.Add(name.ToString(), typeof(double));
                }
            }

            foreach (var recode in recodes_) {
                dt.Rows.Add(
                    recode.code_,

                    recode.position_,
                    recode.buyTime_,
                    recode.buyPrice_,
                    recode.payoffTime_,
                    recode.payoffPrice_,
                    string.Format("{0:0#}일 {1:0#}시 {2:0#}분", recode.haveTime_.Days, recode.haveTime_.Hours, recode.haveTime_.Minutes),

                    recode.buyCount_,
                    recode.profit_,
                    recode.rsq_,
                    recode.buyLog_,
                    recode.sellLog_,
                    recode.predictedProfit_,
                    recode.grossEarnings_,
                    recode.payOffCode_.ToString(),

                    recode.minProfit_,
                    recode.maxProfit_,
                    recode.oneProfit_,

                    recode.buyModule_,
                    recode.sellModule_,
                    recode.tradeModuelAvgProfit_
                    ) ;
            }
            var fund = this.bot_.fundManagement_;
            dt.TableName = fund.GetType().Name;
            return dt;
        }

        public string getLog()
        {
            var recoder = this;

            var fund = this.bot_.fundManagement_;
            StringBuilder log = new StringBuilder();

            log.AppendFormat("* 전략 : {0} \n", fund.name());
            log.AppendFormat("[진입전략: {0}]\n", fund.strategyModule_.name());

            log.AppendFormat("- [거래 : (승리/총) {0}/{1}] / ", recoder.winCount_, recoder.tradeCount_);
            log.AppendFormat("[승율 : {0:##0.##}%] / ", recoder.winRate_ * 100);
            
            log.AppendFormat("[총 이익금 : {0:##,###0.#}$ / 예수금 : {1:##,###0.#}$ / 내몫 : {2:##,###0.#}$]\n", recoder.grossEarnings_, bot_.totalAccountMoney(), bot_.saveMyMoney_);
            log.AppendFormat("- [최대 거래 시간: {0:0#}일 {1:0#}시 {2:0#}분] \n", recoder.maxHaveTime_.Days, recoder.maxHaveTime_.Hours, recoder.maxHaveTime_.Minutes);            

            log.AppendFormat("- 거래 아이템 : {0}, 평균이익: {1:##,###0.#}$, 표준편차 : {2:##,###0.#####}\n", this.itemCount_, this.avgProfit_, this.itemDeviation_);

            log.AppendFormat("- 신뢰도 (rsq) : {0:##.##} | ", this.rsq_);
            log.AppendFormat("평균최소이익 : {0:##0.#},  평균 최대이익 : {1:##0.#} ", this.avgMinProfit_, this.avgMaxProfit_);
            log.AppendFormat("최소이익 : {0:##0.#}, 최대이익 : {1:##0.#}\n", this.minProfit_, this.maxProfit_);

            log.AppendFormat("- [{0} 시간후 예상 이익 : {1:##0.#}\n", PublicVar.fundUpdateHours, this.predictedProfit(PublicVar.fundUpdateHours * 60));
            log.AppendFormat("- [거래 시간 {0} ~ {1} => {2}\n", recoder.startTime_, recoder.endTime_, (recoder.endTime_ - recoder.startTime_));

            return log.ToString();
        }
    }
}
