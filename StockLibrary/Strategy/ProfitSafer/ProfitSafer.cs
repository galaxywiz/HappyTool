using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockLibrary.StrategyManager.ProfitSafer
{
    public class ProfitSaferStrategy
    {
       
        // 청산 조건 체크
        public List<ProfitLost> profitLostList_ = new List<ProfitLost>();

        public string profitLostName()
        {
            var log = new StringBuilder();
            foreach (var profitLost in this.profitLostList_) {
                log.AppendFormat("{0}, ", profitLost.GetType().Name);
            }
            return log.ToString();
        }

        public void addProfitLost(ProfitLost pl)
        {
            // net 커넥은 저쪽에서 알아서 처리
            foreach (var profitLost in this.profitLostList_) {
                if (profitLost.GetType() == pl.GetType()) {
                    return;
                }
            }
            this.profitLostList_.Add(pl);
        }

        public void clearProfitLost()
        {
            profitLostList_.Clear();
        }

        public bool checkProfitSafer(StockData stockData, out StringBuilder why)
        {
            StringBuilder why2 = new StringBuilder();
            foreach (var profitLost in this.profitLostList_) {
                if (profitLost.targetHit(stockData, out why2)) {
                    why = why2;
                    return true;
                }
            }
            why = null;
            return false;
        }
    }

}
