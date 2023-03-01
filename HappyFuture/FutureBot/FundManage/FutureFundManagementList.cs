using StockLibrary;
using StockLibrary.StrategyManager;
using StockLibrary.StrategyManager.StrategyModuler;
using StockLibrary.StrategyManager.ProfitSafer;
using System;
using System.Collections.Generic;
using System.Reflection;
using UtilLibrary;

namespace HappyFuture.FundManage
{
    class FutureFundManagementList: SingleTon<FutureFundManagementList>
    {
        public Dictionary<string, string> pool_ = new Dictionary<string, string>();
        public Dictionary<string, FutureFundManagement> universalPool_ = new Dictionary<string, FutureFundManagement>();

        public FutureFundManagementList()
        {
        //    this.pool_["AI전략"] = "ML_StrategyModule";
            this.pool_["MA0전략"] = "GoldenCorssStrategyModule_00";
            this.pool_["MA1전략"] = "GoldenCorssStrategyModule_01";
            this.pool_["MA2전략"] = "GoldenCorssStrategyModule_02";
            this.pool_["MA3전략"] = "GoldenCorssStrategyModule_03";
         //   this.pool_["MA4전략"] = "GoldenCorssStrategyModule_04";

            this.pool_["추세돌파01"] = "LarryRStrategyModule_Con01";
            this.pool_["추세돌파02"] = "LarryRStrategyModule_Con02";
            this.pool_["역추세돌파01"] = "LarryRStrategyModule_Con03";

            this.pool_["돈체인"] = "PriceChanelStrategyModule";

            //this.pool_["기본 전략"] = "BollingerFundManagement";
            //this.pool_["볼린저 터치 전략"] = "BollingerTouchFundManagement";
            //this.pool_["역기본 전략"] = "RevBollingerFundManagement";
            //this.pool_["역볼린저 터치 전략"] = "RevBollingerTouchFundManagement";
        }

        public FutureFundManagement getFutureFundManagementName(string name, Bot bot)
        {
            string className;
            this.pool_.TryGetValue(name, out className);
            var fund = getFundManagementCombineStrategyModule(className, bot);
            if (fund == null) {
                return null;
            }

            return fund;
        }

        public FutureFundManagement getFundManagementCombineStrategyModule(string entrieName, Bot bot)
        {
            var strategyModule = StrategyModuleGetter.getInstance.getStrategyModule(entrieName, bot);
            if (strategyModule == null) {
                return null;
            }

            var fundManagement = new FutureFundManagement(bot);
            fundManagement.setStrategyModule(strategyModule);
            
            return fundManagement;
        }
    }
}
