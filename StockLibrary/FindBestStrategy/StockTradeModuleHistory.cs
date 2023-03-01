using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace StockLibrary.StrategyForTrade
{
    public class StockTradeModuleList
    {
        public string code_;
        public StockTradeModuleList(string code)
        {
            this.code_ = code;
        }

        public ConcurrentDictionary<DateTime, BackTestRecoder> tradePool_ = new ConcurrentDictionary<DateTime, BackTestRecoder>();

        public void addAt(DateTime dateTime, BackTestRecoder tradeModule)
        {
            if (this.tradePool_.ContainsKey(dateTime)) {
                return;
            }
            this.tradePool_[dateTime] = tradeModule;
        }

        public BackTestRecoder historyAt(DateTime dateTime)
        {
            if (this.tradePool_.ContainsKey(dateTime)) {
                return this.tradePool_[dateTime];
            }
            return null;
        }

        public int count()
        {
            return this.tradePool_.Count;
        }
    }


    public class StockTradeModuleHistory
    {
        // code
        public Dictionary<string, StockTradeModuleList> historyPool_ = new Dictionary<string, StockTradeModuleList>();
        public bool updateTradeModule(ref StockData stockData)
        {
            StockTradeModuleList moduleList = this.getModuleList(stockData.code_);
            // 전에 찾은게 있으면 그걸 취함.
            if (moduleList == null) {
                return false;
            }
            var tradeModule = moduleList.historyAt(stockData.nowDateTime());
            if (tradeModule == null) {
                return false;
            }

            var list = stockData.tradeModuleList();
            if (list.Count == 0) {
                list.Add(tradeModule);
            }
            else {
                list[0] = tradeModule;
            }
            return true;
        }

        public void setTradeModule(StockData stockData)
        {
            // 결과 저장
            var stockTradeModule = stockData.tradeModule();
            var newTradeModule = new BackTestRecoder();

            // stockTradeModule 가 null 이어도 넣어줘야 함. 안그럼 계속 찾음
            if (stockTradeModule != null) {
                newTradeModule.buyTradeModule_ = stockTradeModule.buyTradeModule_;
                newTradeModule.sellTradeModule_ = stockTradeModule.sellTradeModule_;
            }
            StockTradeModuleList moduleList = this.getModuleList(stockData.code_);
            if (moduleList == null) {
                moduleList = new StockTradeModuleList(stockData.code_);
                this.addTradeModuleList(moduleList);
            }

            moduleList.addAt(stockData.nowDateTime(), newTradeModule);
        }

        public StockTradeModuleList getModuleList(string code)
        {
            StockTradeModuleList moduleList = null;
            if (this.historyPool_.TryGetValue(code, out moduleList) == false) {
                return null;
            }
            return moduleList;
        }

        public void addTradeModuleList(StockTradeModuleList moduleList)
        {
            var code = moduleList.code_;
            if (this.historyPool_.ContainsKey(code) == false) {
                this.historyPool_.Add(code, moduleList);
            }
        }

        public void updateDB(Bot bot)
        {
            foreach (var keyValue in this.historyPool_) {
                bot.updateTradeModuleList(keyValue.Value);
            }
        }
    }
}
