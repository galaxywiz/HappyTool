using StockLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilLibrary;

namespace HappyTool.FundManagement
{
    class StockFundManagementList: SingleTon<StockFundManagementList>
    {
        StockFundManagementList()
        {
        }

        public StockFundManagement getFundManagementCombineStrategyModule(string className, Bot bot)
        {
            var strategyModule = StockLibrary.StrategyManager.StrategyModuler.StrategyModuleGetter.getInstance.getStrategyModule(className, bot);
            if (strategyModule == null) {
                return null;
            }

            var fundManagement = new StockFundManagement(bot);
            fundManagement.setStrategyModule(strategyModule);
            return fundManagement;
        }
    }
}
