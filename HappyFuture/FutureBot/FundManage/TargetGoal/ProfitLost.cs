using StockLibrary;
using StockLibrary.StrategyManager;
using StockLibrary.StrategyManager.ProfitSafer;
using System.Collections.Generic;
using System.Text;
using UtilLibrary;

namespace HappyFuture.FundManage
{
    public class ProfitLostList: SingleTon<ProfitLostList>
    {
        public List<ProfitLost> commonProfitLostList_ = new List<ProfitLost>();
        public List<ProfitLost> profitLostList_ = new List<ProfitLost>();
        ProfitLostList()
        {
            //***************************************************************//
            // -15tick 시 손절 (안전 장치. 이건 꼭 넣을것!)
            this.commonProfitLostList_.Add(new LostCut_BaseTime());
            //***************************************************************//
            this.profitLostList_.Add(new Profit_BaseTime());                  // 30tick 초과시 익절
            
            this.profitLostList_.Add(new LostCut_LongTimeHaved());            // 보유 시간이 길면 강제 손절
            //     profitLostList_.Add(new StatisticMax());                      // 120$ 돌파 익절 (통계적 best 이익)
            //      profitLostList_.Add(new StatisticMin());                      // -80$ 돌파 손절
            this.profitLostList_.Add(new Profit_DetectPlugePrices());         // 150달러 초과시, 15%빠지면 익절
            
            this.profitLostList_.Add(new Profit_PullBack());                  // 최고점에서 15% 가격 하락시 익절
            this.profitLostList_.Add(new Profit_OffenseTrend());              // 20분봉, 10분봉 현재를 비교시 계속 - 로 떨어지면 손절
            this.profitLostList_.Add(new Profit_ProtectionTrend());           // 120달러 돌파 이후 이 다음이 곧 -가 될거 같으면 손절
            
            this.profitLostList_.Add(new LostCut_Trend());
            this.profitLostList_.Add(new Profit_Default());

            // 이쪽은 거의 폐기...
            //      this.commonProfitLostList_.Add(new LostCut_ExpectMin());          // 최대 기대값 -1.5배면 손절 (최소 안전장치)
            //      this.commonProfitLostList_.Add(new Profit_ExpectMax());                 // 최대 기대값 1.5배면 익절
            // profitLostList_.Add(new ExpectMinusMax());                      // 최대 기대값 -1.5배면 손절

            //this.profitLostList_.Add(new Profit_MinStandardDeviation());      // 예측 최소값 돌파시 익절
            //this.profitLostList_.Add(new LostCut_MinStandardDeviation());     // 백테스팅 예측 - 최소값 밖으로 떨어질시 손절

            //this.profitLostList_.Add(new Profit_AvgStandardDeviation());      // 백테스팅 평균값 초과시 익절
            //this.profitLostList_.Add(new LostCut_AvgStandardDeviation());     // 최대값이 평균찍고 최소 범위 밖 이탈시

            //this.profitLostList_.Add(new Profit_MaxStandardDeviation());      // 예측 최대값 돌파시 익절
            //this.profitLostList_.Add(new LostCut_MaxStandardDeviation());     // 백테스팅 예측 - 최대값 밖으로 떨어질시 손절
        }
    }

    public class Profit_Default: ProfitLost
    {
        public override bool targetHit(StockData stockData, out StringBuilder why)
        {
            why = new StringBuilder();
            bool payOff = false;
            var futureData = stockData as FutureData;
            var bot = ControlGet.getInstance.futureBot();
            switch (futureData.position_) {
                case TRADING_STATUS.매수:
                if (futureData.isSellTime(bot)) {
                    payOff = true;
                }
                break;

                case TRADING_STATUS.매도:
                if (futureData.isBuyTime(bot)) {
                    payOff = true;
                }
                break;
            }

            if (payOff) {
                why.AppendFormat("{0}은 [{1}] 포지선, {2} 개 x {3:##,###0.#####}], 개당 {4:##,###0.#####}으로 청산 신호 감지\n",
                    futureData.name_, futureData.position_, futureData.buyCount_, futureData.nowPrice(), futureData.nowOneProfit());
                futureData.payOffCode_ = PAY_OFF_CODE.defaultCheck;
            }
            return payOff;
        }
    }

    public class Profit_BaseTime: ProfitLost
    {
        public override bool targetHit(StockData stockData, out StringBuilder why)
        {
            why = new StringBuilder();

            var nowOneProfit = stockData.nowOneProfit();

            if (stockData.targetProfit() <= nowOneProfit) {
                stockData.payOffCode_ = PAY_OFF_CODE.Profit_BaseTime;

                why.AppendFormat("[{0}]의 이익금 {1:#,###.##}$, 목표치를[{2:##}] 초과 하므로 강제 청산", stockData.name_, nowOneProfit, stockData.targetProfit());
                Logger.getInstance.print(KiwoomCode.Log.백테스팅, stockData.whyPayOff_);
                stockData.whyPayOff_ = why.ToString();
                return true;
            }
            return false;
        }
    }

    public class Profit_DetectPlugePrices: ProfitLost
    {
        const int TICK_STEP = 22;
        public override bool targetHit(StockData stockData, out StringBuilder why)
        {
            why = new StringBuilder();
            var maxProfit = stockData.maxProfit_;

            var futureData = stockData as FutureData;
            //10틱이 기준값
            var belowStandardPrice = TICK_STEP * futureData.tickValue_;
            if (maxProfit < belowStandardPrice) {
                return false;
            }

            const double PULL_BACK_PER = 0.3f;     //30%가 빠지면 ㄱㄱ
            var detcectVal = maxProfit - (maxProfit * PULL_BACK_PER);

            var nowOneProfit = futureData.nowOneProfit();
            if (nowOneProfit < detcectVal) {
                futureData.payOffCode_ = PAY_OFF_CODE.Profit_DetectPlugePrices;
                why.AppendFormat("{0} 이 1주당 최고{1:##,###0.#####} $ 였는데,\n", futureData.name_, futureData.maxProfit_);
                why.AppendFormat("{0:##,###0.#####} $로 급락해버림.\n", nowOneProfit);
                stockData.whyPayOff_ = why.ToString();
                Logger.getInstance.print(KiwoomCode.Log.백테스팅, futureData.whyPayOff_);
            }
            return true;
        }
    }

    public class LostCut_BaseTime: ProfitLost
    {
        public override bool targetHit(StockData stockData, out StringBuilder why)
        {
            var nowOneProfit = stockData.nowOneProfit();
            var pureProfit = nowOneProfit - PublicFutureVar.pureFeeTex;

            why = new StringBuilder();
            if (nowOneProfit > 0) {
                return false;
            }

            if (pureProfit <= stockData.lostCutProfit()) {
                stockData.payOffCode_ = PAY_OFF_CODE.LostCut_BaseTime;

                why.AppendFormat("1계약당 강제 손절매[{0:##}] 구간에 진입하여 청산. 현재가 {1:##,###0.#####}$ ", stockData.lostCutProfit(), pureProfit);
                Logger.getInstance.print(KiwoomCode.Log.백테스팅, stockData.whyPayOff_);
                stockData.whyPayOff_ = why.ToString();
                return true;
            }
            return false;
        }
    }

    // 직전 분봉과 비교해서 급락했는가?
    public class LostCut_SuddenDrop: ProfitLost
    {
        public override bool targetHit(StockData stockData, out StringBuilder why)
        {
            var nowOneProfit = stockData.nowOneProfit();
            var pureProfit = nowOneProfit - PublicFutureVar.pureFeeTex;

            why = new StringBuilder();

            var nowCandle = stockData.nowCandle();
            var prevCandle = stockData.prevCandle();

            stockData.payOffCode_ = PAY_OFF_CODE.LostCut_BaseTime;

            if (nowCandle.height() / 2 > prevCandle.height()) {
                if (stockData.position_ == TRADING_STATUS.매수) {
                    if (nowCandle.centerPrice_ < prevCandle.lowPrice_) {
                        why.AppendFormat("급락 감지");
                        Logger.getInstance.print(KiwoomCode.Log.백테스팅, stockData.whyPayOff_);
                        stockData.whyPayOff_ = why.ToString();
                        return true;
                    }
                }
                else {
                    if (nowCandle.centerPrice_ > prevCandle.lowPrice_) {
                        why.AppendFormat("급락 감지");
                        Logger.getInstance.print(KiwoomCode.Log.백테스팅, stockData.whyPayOff_);
                        stockData.whyPayOff_ = why.ToString();
                        return true;
                    }
                }
            }

            return false;
        }
    }

    // 손절 추세 확인
    // 통계적으로 100 달러 못넘고, 계속 하락으로 -300 이하가 되면 이건 추세 잘못 집은거
    public class Profit_OffenseTrend: ProfitLost
    {
        const int STAND_MIN = 15;

        public override bool targetHit(StockData stockData, out StringBuilder why)
        {
            why = new StringBuilder();

            var bot = ControlGet.getInstance.futureBot();
            var min = bot.priceTypeMin();
            if (stockData.positionHaveMin() < (min * STAND_MIN)) {
                return false;
            }

            var futureData = stockData as FutureData;
            //10틱이 기준값
            var standardValue = 10 * futureData.tickValue_;

            if (futureData.nowOneProfit() > standardValue) {
                return false;
            }

            var priceTable = futureData.priceTable();
            var price = priceTable[STAND_MIN].price_;
            var firstProfit = futureData.calcProfit(futureData.buyPrice_, price, 1, futureData.position_);

            price = priceTable[STAND_MIN / 2].price_;
            var middleProfit = futureData.calcProfit(futureData.buyPrice_, price, 1, futureData.position_);

            var nowOneProfit = futureData.nowOneProfit();
            if (firstProfit > middleProfit) {
                if (middleProfit > nowOneProfit) {
                    futureData.payOffCode_ = PAY_OFF_CODE.Profit_OffenseTrend;

                    why.AppendFormat("{0} 이 하락 추세라서 강제 청산. 현재 이익 {1:##,###0.#####}$ ", futureData.name_, nowOneProfit);
                    futureData.whyPayOff_ = why.ToString();
                    Logger.getInstance.print(KiwoomCode.Log.백테스팅, futureData.whyPayOff_);
                    return true;
                }
            }

            return false;
        }
    }

    // 다음 예측값이 0 ~ 80 달러로 떨어질거 같으면 청산
    public class Profit_ProtectionTrend: ProfitLost
    {
        const int TICK_STEP = 20;
        public override bool targetHit(StockData stockData, out StringBuilder why)
        {
            why = new StringBuilder();

            FutureData futureData = stockData as FutureData;
            var maxOneProfit = futureData.maxProfit_;
            double STAND = TICK_STEP * futureData.tickValue_;

            // 점화 하려면 최대 값이 TICK_STEP tick를 돌파해야 함.
            if (maxOneProfit < STAND) {
                return false;
            }
            var nowOneProfit = futureData.nowOneProfit();

            var priceTable = futureData.priceTable();
            const int YESTER_IDX = 1;
            var yesterPrice = priceTable[YESTER_IDX].price_;
            var preProfit = futureData.calcProfit(futureData.buyPrice_, yesterPrice, 1, futureData.position_);
            var nextProfit = nowOneProfit + (nowOneProfit - preProfit);

            var MIN_STARD = (TICK_STEP / 2) * futureData.tickValue_;
            if (nextProfit > MIN_STARD) {
                return false;
            }
            futureData.payOffCode_ = PAY_OFF_CODE.Profit_ProtectionTrend;

            why.AppendFormat("{0} 이 다음 틱에 -가 될 수 있어서 미리 청산. 현재 이익 {1:##,###0.#####}$\n", futureData.name_, nowOneProfit);
            why.AppendFormat("-> 다음 예측 가격 {0:##,###0.#####}", nextProfit);
            futureData.whyPayOff_ = why.ToString();
            Logger.getInstance.print(KiwoomCode.Log.백테스팅, futureData.whyPayOff_);
            return true;
        }
    }

    public class LostCut_Trend: ProfitLost
    {
        const int PREV_CANDLE = 10;
        public override bool targetHit(StockData stockData, out StringBuilder why)
        {
            why = new StringBuilder();

            FutureData futureData = stockData as FutureData;
            var bot = ControlGet.getInstance.futureBot();
            if (futureData.positionHaveMin() < (PREV_CANDLE * bot.priceTypeMin())) {
                return false;
            }
            var priceTable = futureData.priceTable();
            var nowOneProfit = futureData.nowOneProfit();

            var prevPrice = priceTable[PREV_CANDLE].price_;
            var preProfit = futureData.calcProfit(futureData.buyPrice_, prevPrice, 1, futureData.position_);
            var nextProfit = nowOneProfit + (nowOneProfit - preProfit);

            if (nextProfit > 0) {
                return false;
            }
            futureData.payOffCode_ = PAY_OFF_CODE.LostCut_Trend;

            why.AppendFormat("{0} 이 다음 틱에 -가 될 수 있어서 미리 청산. 현재 이익 {1:##,###0.#####}$\n", futureData.name_, nowOneProfit);
            why.AppendFormat("-> 다음 예측 가격 {0:##,###0.#####}", nextProfit);
            futureData.whyPayOff_ = why.ToString();
            Logger.getInstance.print(KiwoomCode.Log.백테스팅, futureData.whyPayOff_);
            return true;
        }
    }

    public class LostCut_LongTimeHaved: ProfitLost
    {
        public override bool targetHit(StockData stockData, out StringBuilder why)
        {
            why = new StringBuilder();
            FutureData futureData = stockData as FutureData;

            var hasMin = futureData.positionHaveMin();
            const int LIMIT_MIN = 60 * 24;  // 1일하고 8시간
            if (hasMin < LIMIT_MIN) {
                return false;
            }
            var nowProfit = futureData.nowPrice();
            if (nowProfit > PublicFutureVar.pureFeeTex) {
                futureData.payOffCode_ = PAY_OFF_CODE.LostCut_LongTimeHaved;

                why.AppendFormat("{0} 의 보유시간[{1}]분 이 너무 길어서 청산. 현재 이익 {2:##,###0.#####}$\n", futureData.name_, futureData.positionHaveMin(), nowProfit);
                futureData.whyPayOff_ = why.ToString();
                Logger.getInstance.print(KiwoomCode.Log.백테스팅, futureData.whyPayOff_);
                return true;
            }

            return false;
        }
    }

    // 최대 이익에서 몇프로 빠지면 처리
    public class Profit_PullBack: ProfitLost
    {
        public override bool targetHit(StockData stockData, out StringBuilder why)
        {
            why = new StringBuilder();
            FutureData futureData = stockData as FutureData;

            var tradeModule = futureData.tradeModule();
            if (tradeModule == null) {
                return false;
            }
            var nowOneProfit = futureData.nowOneProfit();
            var maxProfit = futureData.maxProfit_;

            if (maxProfit == nowOneProfit) {
                return false;
            }

            const double PULL_BACK_PER = 0.4f;     //40%가 빠지면 ㄱㄱ
            var detcectVal = maxProfit - (maxProfit * PULL_BACK_PER);
            if (nowOneProfit < detcectVal) {
                futureData.payOffCode_ = PAY_OFF_CODE.Profit_PullBack;

                why.AppendLine(futureData.whyPayOff_);
                why.AppendFormat("\n");
                why.AppendFormat("이후 1계약당 기대 수익금 {0:##,###0.#####}$ 돌파 이후, 수익이 {1::##,###0.#####} $로 후퇴 해서 청산", futureData.maxProfit_, nowOneProfit);
                Logger.getInstance.print(KiwoomCode.Log.백테스팅, futureData.whyPayOff_);
                return true;
            }
            return false;
        }
    }
}
