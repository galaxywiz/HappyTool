using StockLibrary;
using System.Collections.Generic;
using System.Linq;

namespace HappyTool
{
    public class StockTradeModuleFilter: TradeModuleFilter
    {
    
        public override IEnumerable<BackTestRecoder> doOrderBy(IEnumerable<BackTestRecoder> srcList)
        {
            if (srcList == null) {
                return null;
            }
            const int LIMIT = 100;
            var query = (from recode in srcList
                         orderby
                                 recode.deviation_ ascending,          // 표준편차가 적고
                                 recode.expectedWinRate_ descending,      // 기대값이 높고
                                 recode.avgProfit_ descending,          // 평균이익이 높고
                                 recode.tradeCount_ descending,
                                 recode.isUpperTrade() descending
                         select recode).Take(LIMIT);

            return query;
        }

        public override bool doFilter(BackTestRecoder recode, double lostCut)
        {
            if (recode.tradeCount_ == 0) {
                return false;
            }

            // 신뢰 가능한 백테스팅 숫자
            if (recode.tradeCount_ < PublicVar.allowTradeCount) {
                return false;
            }

            // 승률
            if (recode.expectedWinRate_ < PublicVar.allowTradeWinRate) {
                return false;
            }

            if ((recode.avgProfit_ - recode.deviation_) < 0) {
                return false;
            }

            //if (recode.expectedWinRate_ > 0.63    // 승률
            //    && (recode.avgProfit_ - recode.deviation_) >= feeTax                 // 기대값
            //    && recode.avgProfitRate_ > PublicVar.profitRate                      // 마지막엔 양수로 가야..
            //    && recode.minProfit_ > lostCut                // 최소이익이 lostCut보다 커야 함
            //    && (recode.avgProfit_ - recode.deviation_) > 0) {               // 최대 이익은 min보다 커야 하고
            //    return true;
            //}

            return true;
        }
    }

}
