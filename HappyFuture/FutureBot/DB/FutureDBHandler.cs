using StockLibrary;
using StockLibrary.DB;
using StockLibrary.StrategyForTrade;
using System;
using System.Data;
using UtilLibrary;

namespace HappyFuture.DB
{
    abstract class FutureSqliteDB: BaseSqliteDB
    {
        public FutureSqliteDB(string dbFile) : base(dbFile)
        {
        }

        public abstract bool select(ref FutureData futureData);
        public abstract void update(FutureData futureData);

        public abstract void delete(FutureData futureData);
    }

    class FutureTradeModulDB: TradeModuleDB
    {
        public FutureTradeModulDB(string dbFile) : base(dbFile)
        {
        }
        public override BackTestRecoder newBackTestRecoder()
        {
            return new FutureBackTestRecoder();
        }
    }

    //------------------------------------------------------------
    class FutureDBHandler
    {
        internal FuturePriceDB priceDB_;
        internal FuturePriceDB dayPriceDB_;
        FutureTradeModulDB moduleDB_;
        FutureTradeRecodeDB tradeDB_;
        TradeModuleHistoryDB tradeHistoryDB_;
        FutureFundPoolDB fundPoolDB_;
        WinRateDB winRateDB_;
        readonly string dbFile_;

        public FutureDBHandler(string dbFile)
        {
            this.dbFile_ = dbFile;

            this.priceDB_ = new FuturePriceDB(dbFile);
            this.moduleDB_ = new FutureTradeModulDB("TradeModules_" + dbFile);
            this.tradeDB_ = new FutureTradeRecodeDB("Trade_" + dbFile);
            this.tradeHistoryDB_ = new TradeModuleHistoryDB("History_" + dbFile);
            this.winRateDB_ = new WinRateDB("WinRate_" + dbFile);
        }

        ~FutureDBHandler()
        {
            this.priceDB_ = null;
            this.moduleDB_ = null;
            this.tradeDB_ = null;
            this.tradeHistoryDB_ = null;
            this.winRateDB_ = null;
        }

        public int queryWaitCount()
        {
            int sum = 0;

            sum += this.priceDB_.queryWaitCount();
            sum += this.moduleDB_.queryWaitCount();
            sum += this.tradeDB_.queryWaitCount();
            sum += this.tradeHistoryDB_.queryWaitCount();

            return sum;
        }

        //------------------------------------------------------------
        // 가격표 저장
        public bool selectFuturePrice(ref FutureData futureData)
        {
            return this.priceDB_.select(ref futureData);
        }
        public void updateFuturePrice(FutureData futureData, int min)
        {
            this.priceDB_.update(futureData, min);
        }
        public void vaccumPriceDB()
        {
            this.priceDB_.vaccum();
        }

        //------------------------------------------------------------
        // 모듈 저장
        public void selectTradeModule(ref FutureData futureData)
        {
            StockData stockData = futureData;
            this.moduleDB_.select(ref stockData);
        }
        public void updateTradeModule(FutureData futureData)
        {
            this.moduleDB_.update(futureData);
            this.updateTradeHistory(futureData);
        }

        // 시뮬로 찾은 매매 모듈
        public void selectFundPool(ref FutureData futureData)
        {
            this.fundPoolDB_.select(ref futureData);
        }
        public void updateFundPool(FutureData futureData)
        {
            this.fundPoolDB_.update(futureData);
        }
        public void deleteFundPool(FutureData futureData)
        {
            this.fundPoolDB_.delete(futureData);
        }

        public void selectUniversalFundPool()
        {
            this.fundPoolDB_ = null;
            this.fundPoolDB_ = new FutureFundPoolDB("StrategyPool_" + this.dbFile_);
            this.fundPoolDB_.selectUniversal();
        }
        public void updateUniversalFundPool()
        {
            this.fundPoolDB_.updateUniversal();
        }

        //------------------------------------------------------------
        // 매매 기록 저장
        public bool selectTradeRecode(DateTime startTime, DateTime endTime, out DataTable dt)
        {
            return this.tradeDB_.select(startTime, endTime, out dt);
        }
        public bool selectTradeRecode(DateTime startTime, DateTime endTime, out string fileName)
        {
            return this.tradeDB_.select(startTime, endTime, out fileName);
        }
        public bool selectTradedModule()
        {
            return this.tradeDB_.selectTradeModule();
        }
        public void updateTradeRecode(FutureData futureData, string why)
        {
            this.tradeDB_.update(futureData, why);
        }

        //------------------------------------------------------------
        // 백테스팅용 매매 모듈 저장 (매 순간 저장)
        public void updateTradeHistory(FutureData futureData)
        {
            this.tradeHistoryDB_.update(futureData);
        }
        public void updateTradeModuleList(StockTradeModuleList modules)
        {
            this.tradeHistoryDB_.updates(modules);
        }
        public void selectTradeModuleList(ref StockTradeModuleList modules, DateTime startTime)
        {
            this.tradeHistoryDB_.select(ref modules, startTime);
        }
        public void deleteAllTradeModule(string code)
        {
            this.tradeHistoryDB_.deleteAll(code);
        }

        //------------------------------------------------------------
        // 승율 전체 기록용
        public void updateWinRate(FutureData futureData)
        {
            this.winRateDB_.update(futureData);
        }
        public void selectWinRate(ref FutureData futureData)
        {
            var stockData = futureData as StockData;
            this.winRateDB_.select(ref stockData);
        }

        //------------------------------------------------------------
    }
}
