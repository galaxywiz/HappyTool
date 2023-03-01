using System;
using System.Reflection;
using System.Text;
using UtilLibrary;

namespace StockLibrary.StrategyManager.ProfitSafer
{
    public abstract class ProfitLost
    {
        // 이익금이 최대치를 달성했다가 stand만큼 빠져 있는 상태 인지 체크
        protected bool profitPullBack(StockData stockData, double stand)
        {
            // 1주당의 이익
            var nowProfit = stockData.nowOneProfit();
            if (nowProfit < PublicFutureVar.pureFeeTex) {
                return false;
            }

            // 강제 익절
            var maxProfit = stockData.maxProfit_;
            var limitProfit = UtilLibrary.Util.calcProfitRate(maxProfit, nowProfit);
            if (limitProfit <= stand) {
                return true;
            }
            return false;
        }

        public abstract bool targetHit(StockData stockData, out StringBuilder why);
    }
    public static class getProfitLost
    {
        public static ProfitLost getter(string name)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Type type = assembly.GetType("StockLibrary.StrategyManager.TargetGoal." + name);
            if (type == null) {
                return null;
            }
            ProfitLost instance = Activator.CreateInstance(type) as ProfitLost;
            return instance;
        }
    }

    public class Profit_TargetPrice: ProfitLost
    {
        public double targetPrice_ = double.MaxValue;
        // 1.6 이 황금비율로 여기서 반등을 치는 경우가 꽤 됨.
        public override bool targetHit(StockData stockData, out StringBuilder why)
        {
            why = new StringBuilder();

            var tradeModule = stockData.tradeModule();
            if (tradeModule == null) {
                return false;
            }

            var nowOneProfit = stockData.realOneProfit();
            // 500 < 400
            if (nowOneProfit < targetPrice_) {
                return false;
            }
            stockData.payOffCode_ = PAY_OFF_CODE.Profit_TargetPrice;

            why.AppendLine(stockData.whyPayOff_);
            why.AppendFormat("\n");
            why.AppendFormat("이후 1계약당 기대 수익금 [{0:##,###0.##} $] 가 설정값 [{1:##,###0.##} $] 돌파로 익절", nowOneProfit, targetPrice_);
            Logger.getInstance.print(KiwoomCode.Log.백테스팅, stockData.whyPayOff_);
            return true;
        }
    }

    public class LostCut_TargetPrice: ProfitLost
    {
        public double targetPrice_ = double.MinValue;
        public override bool targetHit(StockData stockData, out StringBuilder why)
        {
            why = new StringBuilder();

            var tradeModule = stockData.tradeModule();
            if (tradeModule == null) {
                return false;
            }

            var nowOneProfit = stockData.realOneProfit();
            if (nowOneProfit > 0) {
                return false;
            }
            // -250 < -300
            if (targetPrice_ < nowOneProfit) {
                return false;
            }

            stockData.payOffCode_ = PAY_OFF_CODE.LostCut_TargetPrice;

            why.AppendFormat("이후 1계약당 기대 수익금 [{0:##,###0.##} $] 가 손절매 설정값 [{1:##,###0.##} $] 이하로 떨어져서 청산", nowOneProfit, targetPrice_);
            Logger.getInstance.print(KiwoomCode.Log.백테스팅, stockData.whyPayOff_);
            return true;
        }
    }

    public class Profit_TargetRate: ProfitLost
    {
        public double targetRate_ = double.MaxValue;
        public override bool targetHit(StockData stockData, out StringBuilder why)
        {
            why = new StringBuilder();

            var tradeModule = stockData.tradeModule();
            if (tradeModule == null) {
                return false;
            }
            
            var nowOneProfit = stockData.realOneProfit();
            var profitRate = Util.calcProfitRate(stockData.buyPrice_, stockData.realNowPrice());
            // 400 < 300
            if (profitRate < targetRate_) {
                return false;
            }
            stockData.payOffCode_ = PAY_OFF_CODE.Profit_TargetRate;

            why.AppendFormat("{0} 이 1주당 평가액{1:##,###0.##} $인데,", stockData.name_, nowOneProfit);
            why.AppendFormat("이익율 {0:##}% 초과 하므로 익절", targetRate_ * 100);
            stockData.whyPayOff_ = why.ToString();
            Logger.getInstance.print(KiwoomCode.Log.백테스팅, stockData.whyPayOff_);
            return true;
        }
    }

    public class LostCut_TargettRate: ProfitLost
    {
        public double targetRate_ = double.MaxValue;
        public override bool targetHit(StockData stockData, out StringBuilder why)
        {
            why = new StringBuilder();
            var tradeModule = stockData.tradeModule();
            if (tradeModule == null) {
                return false;
            }

            var nowOneProfit = stockData.realOneProfit();
            var profitRate = Util.calcProfitRate(stockData.buyPrice_, stockData.realNowPrice());
            // -200 < -100
            if (targetRate_ < nowOneProfit) {
                return false;
            }
            // -최소값을 하락시 손절
            stockData.payOffCode_ = PAY_OFF_CODE.LostCut_TargettRate;

            why.AppendFormat("{0} 이 1주당 평가액{1:##,###0.##} $인데,", stockData.name_, nowOneProfit);
            why.AppendFormat("이익율 {0:##}% 미만 하므로 손절", targetRate_ * 100);
            stockData.whyPayOff_ = why.ToString();
            Logger.getInstance.print(KiwoomCode.Log.백테스팅, stockData.whyPayOff_);
            return true;
        }
    }
}
