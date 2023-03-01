using StockLibrary.StrategyManager.StrategyModuler;
using StockLibrary.StrategyManager.ProfitSafer;
using System;
using System.Collections.Generic;
using System.Text;

namespace StockLibrary.StrategyManager
{
    public abstract class StrategyManagement: ICloneable
    {
        protected Bot bot_;
        public double lostCutTime_;
        public double targetProfitTime_;
        public int rankIdx_ = 0;
        public StrategyModule strategyModule_;
        public ProfitSaferStrategy profitSafer_ = new ProfitSaferStrategy();
        public virtual object Clone()
        {
            throw new NotImplementedException();
        }

        public abstract void trade(StockData stockData);
        public abstract void tradeRealCandle(StockData stockData);

        protected abstract void processMonitor(StockData stockData);
        protected abstract bool processPayOff(StockData stockData, out string why);
        
        public abstract void forcedBuy(string code);
        public abstract void forcedSell(string code);

        public abstract void orderBuy(StockData stockData, int tradingCount, double buyPrice);
        public abstract void orderSell(StockData stockData, int tradingCount, double buyPrice);

        public void setStrategyModule(StrategyModule strategyModule)
        {
            strategyModule_ = strategyModule;
        }

        public void setBot(Bot bot)
        {
            this.bot_ = bot;
        }

        public string name_ = "";
        public string name()
        {
            if (this.strategyModule_ == null) {
                name_ = this.GetType().Name;
                return name_;
            }
            var fundName = this.strategyModule_.GetType().Name;
            name_ = fundName;
            return this.name_;
        }

        // 진입 조건 체크
        public TRADING_STATUS checkEntryStrategy(StockData stockData)
        {
            if (strategyModule_.haveNetConnect_) {
                return TRADING_STATUS.모니터닝;
            }
            return strategyModule_.checkEntryStrategy(stockData);
        }

        public bool checkProfitSafer(StockData stockData, out StringBuilder why)
        {
            if (profitSafer_ == null) {
                why = null;
                return false;
            }
            return profitSafer_.checkProfitSafer(stockData, out why);
        }
    }
}
