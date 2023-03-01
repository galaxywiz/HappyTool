using System;
using System.Collections.Generic;
using System.Data;
using UtilLibrary;
using static StockLibrary.StrategyModuleList;

namespace StockLibrary
{
    public class BestTradeModuleDB: BaseSqliteDB
    {
        public BestTradeModuleDB(string dbFile) : base(dbFile)
        {
        }

        protected override string tableName()
        {
            return string.Format("BEST_TRADE_MODULES");
        }

        protected override void createTable()
        {
            string column = "모듈이름 TEXT  PRIMARY KEY, rank INT, 갱신시간 DATETIME, 모듈들 TEXT";
            string sql = string.Format("CREATE TABLE {0} ({1})", this.tableName(), column);
            this.sqlDB_.executeQuery(sql);
        }

        public bool select(Bot bot, int LIMIT_ROW = 1000)
        {
            try {
                if (this.checkTable() == false) {
                    return false;
                }
                // 너무 많으면 탐색에 시간이 좀 많이 걸림
                var sql = string.Format("SELECT 모듈이름, 갱신시간 FROM {0} ORDER BY 갱신시간 DESC LIMIT {1}", this.tableName(), LIMIT_ROW);

                var dt = this.sqlDB_.readDataTableQuery(sql);
                if (dt.Rows.Count == 0) {
                    return false;
                }
                var moduleList = StrategyModuleList.getInstance;
                moduleList.clearPool();
                DateTime updateTime = DateTime.MinValue;
                foreach (DataRow row in dt.Rows) {
                    string moduleName = row[0].ToString();
                    CombineStrategyModule module = moduleList.getGenPoolAtName(moduleName);
                    if (module == null) {
                        //module = StrategyModuleList.getInstance.parseModule(moduleName);
                        //if (module == null) {
                        //    Logger.getInstance.print(KiwoomCode.Log.에러, "{0} 은 삭제된 전략이 있어 생성되지 않은 모듈입니다.", moduleName);
                        continue;
                        //}
                    }
                    moduleList.addModule(module);

                    DateTime date = (DateTime) row[1];
                    if (date > updateTime) {
                        updateTime = date;
                    }
                }
                Logger.getInstance.print(KiwoomCode.Log.주식봇, "{0} 개의 매매 모듈 로딩", moduleList.getDefaultPool().Count);
                ///      bot.moduleUpdateTime_ = updateTime;
            }
            catch (Exception e) {
                Logger.getInstance.print(KiwoomCode.Log.에러, "sqlite {0} 로딩 실패. / {1} / {2}", this.tableName(), e.Message, e.StackTrace);
                return false;
            }
            return true;
        }

        public void update()
        {
            try {
                if (this.checkTable() == false) {
                    // 테이블이 없으면 만든다
                    this.createTable();
                }
                //    this.delete();
                DateTime now = DateTime.Now;
                var pool = StrategyModuleList.getInstance.getDefaultPool();
                List<string> sqls = new List<string>();
                int index = 0;
                foreach (var keyValue in pool) {
                    var module = keyValue.Value;
                    if (module.activeModule() == false) {
                        //        Logger.getInstance.print(KiwoomCode.Log.에러, "{0} 모듈은 내부 전략이 제거되어 삭제 시킴.", module.getName());
                        continue;
                    }
                    var sql = string.Format("INSERT OR REPLACE INTO {0} ", this.tableName());
                    sql += " ('모듈이름', 'rank', '갱신시간', '모듈들') ";
                    sql += string.Format(" VALUES ('{0}', '{1}', '{2}', '{3}')",
                        module.getName(),
                        index++,
                        now.ToString(dateTimeFmt_),
                        module.getBuyStrategyForCSV());
                    sqls.Add(sql);

                    Logger.getInstance.print(KiwoomCode.Log.주식봇, "best 모듈 추가: {0};", sql);
                }

                this.sqlDB_.executeQueryList(sqls);
            }
            catch (Exception e) {
                Logger.getInstance.print(KiwoomCode.Log.에러, "sqlite {0} 입력 실패. {1} / {2}", this.tableName(), e.Message, e.StackTrace);
            }
        }

        public void delete()
        {
            string sql = string.Format("DELETE FROM {0}", this.tableName());
            this.sqlDB_.executeQuery(sql);
        }
    }
}
