using StockLibrary.StrategyForTrade;
using System;
using System.Collections.Generic;
using System.Data;
using UtilLibrary;

namespace StockLibrary.DB
{
    //백테스팅용 
    // 매 분 매매 모듈을 저장해 뒀다가, 백테스팅 할때 분석할 수 있도록 하는 db
    public class TradeModuleHistoryDB: BotSqliteDB
    {
        public TradeModuleHistoryDB(string dbFile) : base(dbFile)
        {
        }

        protected string tableName(string code)
        {
            return string.Format("TRADE_MODULE_HISTORY_{0}", code);
        }

        protected void createTable(string code)
        {
            // 코드, buy모듈, sell모듈, rank (이익율 많은 순서)
            string column = "candleTime DATETIME PRIMARY KEY, buyModule TEXT, sellModule TEXT";
            string sql = string.Format("CREATE TABLE {0} ({1})", this.tableName(code), column);
            this.sqlDB_.executeQuery(sql);
        }

        bool checkTable(string code)
        {
            return this.sqlDB_.checkTableName(this.tableName(code));
        }

        public bool select(ref StockData stockData)
        {
            var code = stockData.code_;

            try {
                if (this.checkTable(code) == false) {
                    return false;
                }
                var sql = string.Format("SELECT candleTime, buyModule, sellModule FROM {0} WHERE candleTime = '{1}'",
                    this.tableName(code), stockData.nowDateTime());

                var dt = this.sqlDB_.readDataTableQuery(sql);
                if (dt.Rows.Count == 0) {
                    return false;
                }

                stockData.tradeModuleList_ = new List<BackTestRecoder>();

                foreach (DataRow row in dt.Rows) {
                    DateTime date = (DateTime) row[0];

                    BackTestRecoder recoder = new BackTestRecoder();
                    string buyModuelName = row[1].ToString();
                    string sellModuleName = row[2].ToString();

                    recoder.buyTradeModule_ = StrategyModuleList.strategyModule(buyModuelName);
                    recoder.sellTradeModule_ = StrategyModuleList.strategyModule(sellModuleName);
                    stockData.tradeModuleList_.Add(recoder);
                }
                return true;
            }
            catch (Exception e) {
                Logger.getInstance.print(KiwoomCode.Log.에러, "{0} sqlite {1} 로딩 실패. {2} / {3}", stockData.name_, this.tableName(code), e.Message, e.StackTrace);
                return false;
            }
        }

        public void update(StockData stockData)
        {
            var code = stockData.code_;
            try {
                var tradeModule = stockData.tradeModule();
                if (tradeModule == null) {
                    return;
                }

                if (this.checkTable(code) == false) {
                    // 테이블이 없으면 만든다
                    this.createTable(code);
                }

                var sql = string.Format("INSERT OR REPLACE INTO {0} ", this.tableName(code));
                sql += " ('candleTime', 'buyModule', 'sellModule') ";
                sql += string.Format(" VALUES ('{0}', '{1}', '{2}')",
                    stockData.nowDateTime().ToString(dateTimeFmt_), tradeModule.buyTradeModule_.getName(), tradeModule.sellTradeModule_.getName());

                this.sqlDB_.updateQuery(sql);
            }
            catch (Exception e) {
                Logger.getInstance.print(KiwoomCode.Log.에러, "{0} sqlite {1} 입력 실패. {2} / {3}", stockData.name_, this.tableName(code), e.Message, e.StackTrace);
            }
        }

        const string NULL_MODULE = "NULL";
        // 백테스팅용 db load / save 구조
        public bool select(ref StockTradeModuleList modules, DateTime startTime)
        {
            var code = modules.code_;
            try {
                if (this.checkTable(code) == false) {
                    return false;
                }
                var sql = string.Format("SELECT candleTime, buyModule, sellModule FROM {0} WHERE candleTime > '{1}'",
                    this.tableName(code), startTime);

                var dt = this.sqlDB_.readDataTableQuery(sql);
                if (dt.Rows.Count == 0) {
                    return false;
                }

                foreach (DataRow row in dt.Rows) {
                    DateTime date = (DateTime) row[0];

                    BackTestRecoder recoder = new BackTestRecoder();
                    string buyModuelName = row[1].ToString().Trim();
                    string sellModuleName = row[2].ToString().Trim();
                    if (buyModuelName != NULL_MODULE) {
                        recoder.buyTradeModule_ = StrategyModuleList.strategyModule(buyModuelName);
                    }
                    if (sellModuleName != NULL_MODULE) {
                        recoder.sellTradeModule_ = StrategyModuleList.strategyModule(sellModuleName);
                    }

                    modules.addAt(date, recoder);
                }                
                return true;
            }
            catch (Exception e) {
                Logger.getInstance.print(KiwoomCode.Log.에러, "{0} sqlite {1} 로딩 실패. {2} / {3}", "history load", this.tableName(code), e.Message, e.StackTrace);
                return false;
            }
        }

        public void updates(StockTradeModuleList modules)
        {
            var code = modules.code_;
            try {
                if (modules.count() == 0) {
                    return;
                }

                if (this.checkTable(code) == false) {
                    // 테이블이 없으면 만든다
                    this.createTable(code);                
                }

                List<string> sqls = new List<string>();
                var pool = modules.tradePool_;
                foreach (var keyPair in pool) {
                    var recoder = keyPair.Value;

                    string buyModule = NULL_MODULE;
                    string sellModule = NULL_MODULE;
                    if (recoder.buyTradeModule_ != null) {
                        buyModule = recoder.buyTradeModule_.getName();
                    }
                    if (recoder.sellTradeModule_ != null) {
                        sellModule = recoder.sellTradeModule_.getName();
                    }

                    var sql = string.Format("INSERT OR REPLACE INTO {0} ", this.tableName(code));
                    sql += " ('candleTime', 'buyModule', 'sellModule') ";
                    sql += string.Format(" VALUES ('{0}', '{1}', '{2}')",
                        keyPair.Key.ToString(dateTimeFmt_), buyModule, sellModule);
                    sqls.Add(sql);
                }
                this.sqlDB_.executeQueryList(sqls);
                Logger.getInstance.print(KiwoomCode.Log.주식봇, "{0} : {1} 개의 모듈 업데이트", code, sqls.Count);
            }
            catch (Exception e) {
                Logger.getInstance.print(KiwoomCode.Log.에러, "{0} sqlite {1} 입력 실패. {2} / {3}", "모듈저장", this.tableName(code), e.Message, e.StackTrace);
            }
        }

        public void delete(StockData stockData, DateTime dateTime)
        {
            var code = stockData.code_;
            string sql = string.Format("DELETE FROM {0} WHERE candleTime < '{1}' ", this.tableName(code), dateTime.ToString(dateTimeFmt_));
            this.sqlDB_.executeQuery(sql);
        }

        public void deleteAll(string code)
        {
            string sql = string.Format("DELETE FROM {0}", this.tableName(code));
            this.sqlDB_.executeQuery(sql);
        }
    }
}
