using System;
using System.Collections.Generic;
using System.Data;
using UtilLibrary;

namespace StockLibrary.DB
{
    public class WinRateDB: StockDataSqliteDB
    {
        public WinRateDB(string dbFile) : base(dbFile)
        {
        }

        protected override string tableName()
        {
            return string.Format("WIN_RATE_TABLE");
        }

        protected override void createTable()
        {
            // 코드, buy모듈, sell모듈, rank (이익율 많은 순서)
            string column = "code TEXT, nowWinCount INT, nowLoseCount INT, totalWinCount INT, totalTradeCount INT, startDate DateTime";
            string sql = string.Format("CREATE TABLE {0} ({1})", this.tableName(), column);
            this.sqlDB_.executeQuery(sql);
        }

        public override bool select(ref StockData stockData)
        {
            try {
                if (this.checkTable() == false) {
                    return false;
                }
                var sql = string.Format("SELECT nowWinCount, nowLoseCount, totalWinCount, totalTradeCount, startDate FROM {0} WHERE code = '{1}'", this.tableName(), stockData.code_);

                var dt = this.sqlDB_.readDataTableQuery(sql);
                if (dt.Rows.Count == 0) {
                    return false;
                }

                stockData.tradeModuleList_ = new List<BackTestRecoder>();

                foreach (DataRow row in dt.Rows) {
                    stockData.statisticsCount_.continueWin_ = int.Parse(row[0].ToString());
                    stockData.statisticsCount_.continueLose_ = int.Parse(row[1].ToString());
                    stockData.statisticsCount_.totalWin_ = int.Parse(row[2].ToString());
                    stockData.statisticsCount_.totalTrade_ = int.Parse(row[3].ToString());
                    stockData.statisticsCount_.startDate_ = (DateTime) row[4];
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
                foreach (var recode in recodeList) {
                    if (recode.buyTradeModule_ == null || recode.sellTradeModule_ == null) {
                        continue;
                    }
                    var sql = string.Format("INSERT INTO {0} ", this.tableName());
                    sql += " ('code', 'nowWinCount', 'nowLoseCount', 'totalWinCount', 'totalTradeCount', 'startDate') ";
                    sql += string.Format(" VALUES ('{0}', {1}, {2}, {3}, {4}, '{5}')",
                        stockData.code_, 
                        stockData.statisticsCount_.continueWin_,
                        stockData.statisticsCount_.continueLose_,
                        stockData.statisticsCount_.totalWin_,
                        stockData.statisticsCount_.totalTrade_,
                        stockData.statisticsCount_.startDate_.ToString(dateTimeFmt_));
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
