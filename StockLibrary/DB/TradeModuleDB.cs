using System;
using System.Collections.Generic;
using System.Data;
using UtilLibrary;

namespace StockLibrary.DB
{
    public class TradeModuleDB: StockDataSqliteDB
    {
        public TradeModuleDB(string dbFile) : base(dbFile)
        {
        }

        protected override string tableName()
        {
            return string.Format("MODULE_TABLE");
        }

        protected override void createTable()
        {
            // 코드, buy모듈, sell모듈, rank (이익율 많은 순서)
            string column = "code TEXT, buy TEXT, sell TEXT, rank INT, updateTime datetime";
            string sql = string.Format("CREATE TABLE {0} ({1})", this.tableName(), column);
            this.sqlDB_.executeQuery(sql);
        }

        public virtual BackTestRecoder newBackTestRecoder()
        {
            return new BackTestRecoder();
        }

        public override bool select(ref StockData stockData)
        {
            try {
                if (this.checkTable() == false) {
                    return false;
                }
                var sql = string.Format("SELECT buy, sell, rank, updateTime FROM {0} WHERE code = '{1}' AND rank < 51", this.tableName(), stockData.code_);

                var dt = this.sqlDB_.readDataTableQuery(sql);
                if (dt.Rows.Count == 0) {
                    return false;
                }

                stockData.tradeModuleList_ = new List<BackTestRecoder>();

                foreach (DataRow row in dt.Rows) {
                    BackTestRecoder recoder = this.newBackTestRecoder();
                    string buyModuelName = row[0].ToString();
                    string sellModuleName = row[1].ToString();
                    
                    if (StrategyModuleList.getInstance.getGenPoolAtName(buyModuelName) == null) {
                        continue;
                    }
                    if (StrategyModuleList.getInstance.getGenPoolAtName(sellModuleName) == null) {
                        continue;
                    }

                    recoder.buyTradeModule_ = StrategyModuleList.strategyModule(buyModuelName);
                    recoder.sellTradeModule_ = StrategyModuleList.strategyModule(sellModuleName);

                    stockData.tradeModuleList_.Add(recoder);

                    int index = int.Parse(row[2].ToString());
                    DateTime date = (DateTime) row[3];

                    // 0번이 직전에 거래 했던 모듈임.
                    if (index == 0) {
                        stockData.tradeModuleUpdateTime_ = date;
                    }
                }
                return true;
            }
            catch (Exception e) {
                Logger.getInstance.print(KiwoomCode.Log.에러, "{0} sqlite {1} 로딩 실패. {2} / {3}", stockData.name_, this.tableName(), e.Message, e.StackTrace);
                return false;
            }
        }

        public override void update(StockData stockData)
        {
            try {
                if (stockData.hasTradeModule() == false) {
                    return;
                }

                if (this.checkTable() == false) {
                    // 테이블이 없으면 만든다
                    this.createTable();
                }
                this.delete(stockData);

                var recodeList = stockData.tradeModuleList_;
                List<string> sqls = new List<string>();
                int index = 0;
                foreach (var recode in recodeList) {
                    if (recode.buyTradeModule_ == null || recode.sellTradeModule_ == null) {
                        continue;
                    }
                    var sql = string.Format("INSERT INTO {0} ", this.tableName());
                    sql += " ('code', 'buy', 'sell', 'rank', 'updateTime') ";
                    sql += string.Format(" VALUES ('{0}', '{1}', '{2}', {3}, '{4}')",
                        stockData.code_, recode.buyTradeModule_.getName(), recode.sellTradeModule_.getName(), index++, stockData.nowDateTime().ToString(dateTimeFmt_));
                    sqls.Add(sql);
                }

                this.sqlDB_.updateQueryList(sqls);
            }
            catch (Exception e) {
                Logger.getInstance.print(KiwoomCode.Log.에러, "{0} sqlite {1} 입력 실패. {2} / {3}", stockData.name_, this.tableName(), e.Message, e.StackTrace);
            }
        }

        public override void delete(StockData stockData)
        {
            string sql = string.Format("DELETE FROM {0} WHERE code = '{1}'", this.tableName(), stockData.code_);
            this.sqlDB_.executeQuery(sql);
        }
    }
}
