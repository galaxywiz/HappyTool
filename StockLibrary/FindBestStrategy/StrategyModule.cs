using System;
using System.Collections.Generic;
using System.Linq;
using UtilLibrary;

namespace StockLibrary
{
    public class StrategyModuleList: SingleTon<StrategyModuleList>
    {
        public static CombineStrategyModule strategyModule(string name)
        {
            CombineStrategyModule module = null;
            foreach (KeyValuePair<string, CombineStrategyModule> temp in StrategyModuleList.getInstance.getDefaultPool()) {
                CombineStrategyModule val = temp.Value;
                if (val.getName() == name) {
                    module = val;
                    break;
                }
            }
            return module;
        }

        readonly Dictionary<string, CombineStrategyModule> tradedModulePool_ = new Dictionary<string, CombineStrategyModule>();   // 이미 매매를 해봤던 풀
        readonly Dictionary<string, CombineStrategyModule> defaultPool_ = new Dictionary<string, CombineStrategyModule>();       // 이미 테스트로 어느정도 검증이 된 풀
        readonly List<CombineStrategyModule> genPool_ = new List<CombineStrategyModule>();                            // 모든 조합을 찾을때 쓰는 풀.

        string first_ = "";
        StrategyModuleList()
        {
            this.genAutoModule();
            this.initDefaultModule();
        }

        public string[] getDefaultModules()
        {
            // 2019-7-21 일 테스트 결과
            string[] sample = {
                "gen[ADX_DITechTradeModlue][ParabolicSARTechTradeModlue]_2",
                "gen[CandleTechTradeModlue][ParabolicSARTechTradeModlue]_2",
                "gen[CandleTechTradeModlue][WilliamsTechTradeModlue]_2",
                "gen[CandleTechTradeModlue][CCITechTradeModlue][StochastictTechTradeModlue]_3",
                "gen[UltimateTechTradeModlue][WilliamsTechTradeModlue]_2",
                "gen[RSITechTradeModlue][WilliamsTechTradeModlue]_2",
                "gen[StochastictTechTradeModlue][WilliamsTechTradeModlue]_2",
                "gen[RSITechTradeModlue][StochastictTechTradeModlue]_2",
                "gen[CCITechTradeModlue][StochastictTechTradeModlue][WilliamsTechTradeModlue]_3",
                "gen[StochastictRSITechTradeModlue][StochastictTechTradeModlue]_2",
                "gen[CCITechTradeModlue][RSITechTradeModlue][WilliamsTechTradeModlue]_3",
                "gen[CandleTechTradeModlue][RSITechTradeModlue]_2",
                "gen[ParabolicSARTechTradeModlue][StochastictRSITechTradeModlue]_2",
                "gen[CCITechTradeModlue][RSITechTradeModlue][StochastictRSITechTradeModlue]_3",
                "gen[CCITechTradeModlue][UltimateTechTradeModlue]_2",
                "gen[CandleTechTradeModlue][CCITechTradeModlue]_2",
                "gen[CandleTechTradeModlue][ParabolicSARTechTradeModlue][WilliamsTechTradeModlue]_3",
                "gen[CCITechTradeModlue][StochastictRSITechTradeModlue]_2",
                "gen[ADX_DITechTradeModlue][CandleTechTradeModlue]_2",
                "gen[RSITechTradeModlue][UltimateTechTradeModlue]_2",
                "gen[CandleTechTradeModlue][StochastictTechTradeModlue]_2",
                "gen[CandleTechTradeModlue][RSITechTradeModlue][WilliamsTechTradeModlue]_3",
                "gen[CCITechTradeModlue][ParabolicSARTechTradeModlue][WilliamsTechTradeModlue]_3",
                "gen[CCITechTradeModlue][ParabolicSARTechTradeModlue]_2",
                "gen[CCITechTradeModlue][WilliamsTechTradeModlue]_2",
                "gen[RSITechTradeModlue][StochastictRSITechTradeModlue]_2",
                "gen[CandleTechTradeModlue][UltimateTechTradeModlue][WilliamsTechTradeModlue]_3",
                "gen[CCITechTradeModlue][ParabolicSARTechTradeModlue][StochastictRSITechTradeModlue]_3",
                "gen[RSITechTradeModlue][StochastictTechTradeModlue][WilliamsTechTradeModlue]_3",
                "gen[StochastictRSITechTradeModlue][WilliamsTechTradeModlue]_2",
                "gen[StochastictTechTradeModlue][UltimateTechTradeModlue]_2",
                "gen[CCITechTradeModlue][StochastictRSITechTradeModlue][StochastictTechTradeModlue]_3",
                "gen[ADX_DITechTradeModlue][CCITechTradeModlue][WilliamsTechTradeModlue]_3",
                "gen[CandleTechTradeModlue][UltimateTechTradeModlue]_2",
                "gen[ADX_DITechTradeModlue][StochastictRSITechTradeModlue]_2",
                "gen[ParabolicSARTechTradeModlue][StochastictRSITechTradeModlue][WilliamsTechTradeModlue]_3",
                "gen[ADX_DITechTradeModlue][WilliamsTechTradeModlue]_2",
                "gen[CCITechTradeModlue][StochastictRSITechTradeModlue][UltimateTechTradeModlue]_3",
                "gen[StochastictRSITechTradeModlue][UltimateTechTradeModlue]_2",
                "gen[CCITechTradeModlue][RSITechTradeModlue][StochastictTechTradeModlue]_3",
                "gen[ADX_DITechTradeModlue][CCITechTradeModlue]_2",
                "gen[RSITechTradeModlue][StochastictRSITechTradeModlue][WilliamsTechTradeModlue]_3",
                "gen[ParabolicSARTechTradeModlue][RSITechTradeModlue]_2",
                "gen[ADX_DITechTradeModlue][CCITechTradeModlue][ParabolicSARTechTradeModlue]_3",
                "gen[CCITechTradeModlue][ParabolicSARTechTradeModlue][RSITechTradeModlue]_3",
                "gen[CandleTechTradeModlue][StochastictRSITechTradeModlue]_2",
                "gen[CCITechTradeModlue][StochastictRSITechTradeModlue][WilliamsTechTradeModlue]_3",
                "gen[StochastictRSITechTradeModlue][StochastictTechTradeModlue][UltimateTechTradeModlue]_3",
                "gen[CCITechTradeModlue][RSITechTradeModlue]_2",
                "gen[StochastictRSITechTradeModlue][StochastictTechTradeModlue][WilliamsTechTradeModlue]_3",
                "gen[CandleTechTradeModlue][CCITechTradeModlue][UltimateTechTradeModlue]_3",
                "gen[StochastictRSITechTradeModlue][UltimateTechTradeModlue][WilliamsTechTradeModlue]_3",
                "gen[RSITechTradeModlue][UltimateTechTradeModlue][WilliamsTechTradeModlue]_3",
                "gen[StochastictTechTradeModlue][UltimateTechTradeModlue][WilliamsTechTradeModlue]_3",
                "gen[CandleTechTradeModlue][MADoubleTechTradeModlue]_2",
                "gen[CCITechTradeModlue][StochastictTechTradeModlue][UltimateTechTradeModlue]_3",
                "gen[CCITechTradeModlue][RSITechTradeModlue][UltimateTechTradeModlue]_3",
                "gen[ADX_DITechTradeModlue][ParabolicSARTechTradeModlue][StochastictRSITechTradeModlue]_3",
                "gen[MADoubleTechTradeModlue][ParabolicSARTechTradeModlue]_2",
                "gen[ADX_DITechTradeModlue][UltimateTechTradeModlue]_2",
                "gen[RSITechTradeModlue][StochastictRSITechTradeModlue][StochastictTechTradeModlue]_3",
                "gen[CandleTechTradeModlue][CCITechTradeModlue][WilliamsTechTradeModlue]_3",
                "gen[ParabolicSARTechTradeModlue][UltimateTechTradeModlue]_2",
                "gen[CandleTechTradeModlue][ParabolicSARTechTradeModlue][StochastictRSITechTradeModlue]_3",
                "gen[ADX_DITechTradeModlue][StochastictRSITechTradeModlue][StochastictTechTradeModlue]_3",
                "gen[CCITechTradeModlue][StochastictTechTradeModlue]_2",
                "gen[ADX_DITechTradeModlue][CandleTechTradeModlue][CCITechTradeModlue]_3",
                "gen[ParabolicSARTechTradeModlue][WilliamsTechTradeModlue]_2",
                "gen[ADX_DITechTradeModlue][CCITechTradeModlue][StochastictRSITechTradeModlue]_3",
                "gen[CandleTechTradeModlue][StochastictRSITechTradeModlue][UltimateTechTradeModlue]_3",
                "gen[CandleTechTradeModlue][StochastictTechTradeModlue][WilliamsTechTradeModlue]_3",
                "gen[CandleTechTradeModlue][StochastictRSITechTradeModlue][WilliamsTechTradeModlue]_3",
                "gen[ADX_DITechTradeModlue][CandleTechTradeModlue][WilliamsTechTradeModlue]_3",
                "gen[ADX_DITechTradeModlue][StochastictTechTradeModlue]_2",
                "gen[CandleTechTradeModlue][CCITechTradeModlue][RSITechTradeModlue]_3",
                "gen[CandleTechTradeModlue][StochastictRSITechTradeModlue][StochastictTechTradeModlue]_3",
                "gen[CandleTechTradeModlue][ParabolicSARTechTradeModlue][RSITechTradeModlue]_3",
                "gen[CandleTechTradeModlue][RSITechTradeModlue][StochastictTechTradeModlue]_3",
                };
            return sample;
        }

        public void initDefaultModule()
        {
            // 없으면 genPool 에서 랜덤으로 뽑는다
            string[] sample = this.getDefaultModules();
            foreach (var moduleName in sample) {
                var module = this.getGenPoolAtName(moduleName);
                if (module == null) {
                    continue;
                }
                this.addModule(module);
            }
            Logger.getInstance.print(KiwoomCode.Log.주식봇, "기본 모듈 {0} 개 로딩", this.defaultPool_.Count);
        }

        public Dictionary<string, CombineStrategyModule> getTradedModulePool()
        {
            return this.tradedModulePool_;
        }

        public Dictionary<string, CombineStrategyModule> getDefaultPool()
        {
            return this.defaultPool_;
        }

        public List<CombineStrategyModule> getGenPool()
        {
            return this.genPool_;
        }

        public CombineStrategyModule getGenPoolAtName(string name)
        {
            foreach (var module in this.genPool_) {
                if (module.getName().CompareTo(name) == 0) {
                    return module;
                }
            }
            return null;
        }

        public CombineStrategyModule parseModule(string name)
        {
            if (this.getGenPoolAtName(name) == null) {
                return null;
            }

            CombineStrategyModule module = new CombineStrategyModule(name);
            //module.name_ = string.Format("gen[{0}][{1}]_{2}", name[0], name[1], i);
            name = name.Replace("gen", "");
            name = name.Replace("[", "");
            name = name.Replace("]", "/");
            string[] world = name.Split('_');
            if (world.Count() < 2) {
                Logger.getInstance.print(KiwoomCode.Log.에러, "{0} 의 모듈은 존재 하지 않습니다.", name);
                return null;
            }
            string[] moduleNames = world[0].Split('/');
            if (moduleNames.Count() < 2) {
                Logger.getInstance.print(KiwoomCode.Log.에러, "{0} 의 모듈은 존재 하지 않습니다.", name);
                return null;
            }
            int value = int.Parse(world[1]);
            module.setBuyTotalValue(value);
            module.setSellTotalValue(value);
            
            List<TechTradeModlue> list = TechTradeModlueList.getInstance.list_;
            foreach (string n in moduleNames) {
                if (n.Length == 0) {
                    continue;
                }
                foreach (TechTradeModlue ts in list) {
                    if (ts.GetType().Name.CompareTo(n) == 0) {
                        module.addBuyStrategy(ts, 1);
                        module.addSellStrategy(ts, 1);
                    }
                }
            }
            return module;
        }

        public void removeModule(CombineStrategyModule module)
        {
            if (this.defaultPool_.ContainsKey(module.getName())) {
                this.defaultPool_.Remove(module.getName());
            }
        }

        public void addModule(CombineStrategyModule module)
        {
            if (this.defaultPool_.ContainsKey(module.getName())) {
                return;
            }

            this.defaultPool_[module.getName()] = module;
            if (this.first_.Length == 0) {
                this.first_ = module.getName();
            }
        }

        void addGenModule(CombineStrategyModule module)
        {
            if (this.genPool_.Contains(module)) {
                return;
            }
            this.genPool_.Add(module);
        }

        public void clearPool()
        {
            this.defaultPool_.Clear();
            this.first_ = "";
        }

        void genAutoModule()
        {
            List<TechTradeModlue> list = TechTradeModlueList.getInstance.list_;
            foreach (TechTradeModlue strategy1 in list) {
                foreach (TechTradeModlue strategy2 in list) {
                    if (strategy1 == strategy2) {
                        continue;
                    }
                    for (int i = 2; i > 1; --i) {
                        CombineStrategyModule module = new CombineStrategyModule("gen");
                        module.addBuyStrategy(strategy1, 1);
                        module.addSellStrategy(strategy1, 1);
                        module.addBuyStrategy(strategy2, 1);
                        module.addSellStrategy(strategy2, 1);
                        string[] name = { strategy1.GetType().Name, strategy2.GetType().Name };
                        Array.Sort(name, StringComparer.InvariantCulture);

                        module.setBuyTotalValue(i);
                        module.setSellTotalValue(i);
                        module.name_ = string.Format("gen[{0}][{1}]_{2}", name[0], name[1], i);

                        this.addGenModule(module);
                    }
                }
            }
            if (CodeModule.해선_밴드전략과_조합) {
                foreach (TechTradeModlue strategy1 in list) {
                    foreach (TechTradeModlue strategy2 in list) {
                        if (strategy1 == strategy2) {
                            continue;
                        }
                        foreach (TechTradeModlue strategy3 in list) {
                            if (strategy1 == strategy3) {
                                continue;
                            }
                            if (strategy2 == strategy3) {
                                continue;
                            }
                            for (int i = 3; i > 2; --i) {
                                CombineStrategyModule module = new CombineStrategyModule("gen");
                                module.addBuyStrategy(strategy1, 1);
                                module.addSellStrategy(strategy1, 1);
                                module.addBuyStrategy(strategy2, 1);
                                module.addSellStrategy(strategy2, 1);
                                module.addBuyStrategy(strategy3, 1);
                                module.addSellStrategy(strategy3, 1);
                                module.setBuyTotalValue(i);
                                module.setSellTotalValue(i);
                                string[] name = { strategy1.GetType().Name, strategy2.GetType().Name, strategy3.GetType().Name };
                                Array.Sort(name, StringComparer.InvariantCulture);

                                module.name_ = string.Format("gen[{0}][{1}][{2}]_{3}", name[0], name[1], name[2], i);
                                this.addGenModule(module);
                            }
                        }
                    }
                }
            }
            else {
                foreach (TechTradeModlue strategy1 in list) {
                    CombineStrategyModule module = new CombineStrategyModule("gen");
                    module.addBuyStrategy(strategy1, 1);
                    module.addSellStrategy(strategy1, 1);
                    module.setBuyTotalValue(1);
                    module.setSellTotalValue(1);
                    module.name_ = string.Format("gen[{0}]", strategy1.GetType().Name);
                    this.addGenModule(module);
                }
            }
        }

        public class StrategyModule
        {
            public virtual string getName()
            {
                return this.GetType().Name;
            }

            //------------------------------------------------------------------------//
            // 여기서 사고 파는 전략을 수립하자.
            public virtual bool buy(StockData stockData, int timeIdx)
            {
                return true;
            }

            public virtual bool sell(StockData stockData, int timeIdx)
            {
                return true;
            }
        }

        public class CombineStrategyModule: StrategyModule
        {
            readonly List<TechTradeModlue> buyStrategy_ = new List<TechTradeModlue>();
            readonly List<int> buyValue_ = new List<int>();
            int buyTotalValue_;
            readonly List<TechTradeModlue> sellStrategy_ = new List<TechTradeModlue>();
            readonly List<int> sellValue_ = new List<int>();
            int sellTotalValue_;

            public string name_;
            public CombineStrategyModule(string name)
            {
                this.name_ = name;
            }

            public override string getName()
            {
                return this.name_;
            }

            public void setBuyTotalValue(int value)
            {
                this.buyTotalValue_ = value;
            }

            public int buyTotalValue()
            {
                return this.buyTotalValue_;
            }

            public void setSellTotalValue(int value)
            {
                this.sellTotalValue_ = value;
            }

            public int sellTotalValue()
            {
                return this.sellTotalValue_;
            }

            public void addBuyStrategy(TechTradeModlue strategy, int value)
            {
                if (this.buyStrategy_.Contains(strategy)) {
                    return;
                }
                this.buyStrategy_.Add(strategy);
                this.buyValue_.Add(value);
            }

            public void addSellStrategy(TechTradeModlue strategy, int value)
            {
                if (this.sellStrategy_.Contains(strategy)) {
                    return;
                }
                this.sellStrategy_.Add(strategy);
                this.sellValue_.Add(value);
            }

            public string getBuyStrategyForCSV()
            {
                string csv = "";
                for (int i = 0; i < this.buyStrategy_.Count; ++i) {
                    csv += string.Format("{0}, {1}, ", this.buyStrategy_[i].GetType().Name, this.buyValue_[i]);
                }
                return csv;
            }

            public string getSellStrategyForCSV()
            {
                string csv = "";
                for (int i = 0; i < this.sellStrategy_.Count; ++i) {
                    csv += string.Format("{0}, {1}, ", this.sellStrategy_[i].GetType().Name, this.sellValue_[i]);
                }
                return csv;
            }

            public bool activeModule()
            {
                return this.buyStrategy_.Count != 0 ? true : false;
            }

            // 여기서 사고 파는 전략을 수립하자.
            public override bool buy(StockData stockData, int timeIdx)
            {
                if (stockData == null) {
                    return false;
                }

                foreach (var strategy in this.buyStrategy_) {
                    if (strategy.buy(stockData, timeIdx) == false) {
                        return false;
                    }
                }
                return true;
            }

            public override bool sell(StockData stockData, int timeIdx)
            {
                if (stockData == null) {
                    return false;
                }

                foreach (var strategy in this.sellStrategy_) {
                    if (strategy.sell(stockData, timeIdx) == false) {
                        return false;
                    }
                }
                return true;
            }
        }
    }
}