using StockLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using UtilLibrary;

namespace HappyFuture.DB
{
    class FutureTradeRecodeDB: FutureSqliteDB
    {
        public FutureTradeRecodeDB(string dbFile) : base(dbFile)
        {
        }

        protected override string tableName()
        {
            return string.Format("TRADE_RECODE");
        }

        protected override void createTable()
        {
            string column = "name TEXT, code TEXT, position TEXT, buyTime datetime, haveTime int, buyPrice float, payOffPrice float, low float, high float, expendPrice float, expendRate float, testCount float, profit float, why Text,  buyModule TEXT, sellModule TEXT";
            string sql = string.Format("CREATE TABLE {0} ({1})", this.tableName(), column);
            this.sqlDB_.executeQuery(sql);
        }

        public override bool select(ref FutureData futureData)
        {
            return false;
        }

        public bool select(DateTime startTime, DateTime endTime, out DataTable dt)
        {
            string path = Application.StartupPath + "\\tradeRecode\\";
            DirectoryInfo di = new DirectoryInfo(path);
            if (di.Exists == false) {
                di.Create();
            }
            dt = null;

            try {
                if (this.checkTable() == false) {
                    return false;
                }
                var sql = string.Format("SELECT * FROM {0} WHERE '{1}' < buyTime AND buyTime < '{2}'",
                    this.tableName(), startTime.ToString(dateTimeFmt_), endTime.ToString(dateTimeFmt_));

                dt = this.sqlDB_.readDataTableQuery(sql);
                if (dt.Rows.Count == 0) {
                    return false;
                }

                return true;
            }
            catch (Exception e) {
                Logger.getInstance.print(KiwoomCode.Log.에러, "sqlite {0} 로딩 실패. {1} / {2}", this.tableName(), e.Message, e.StackTrace);
                return false;
            }
        }

        public bool select(DateTime startTime, DateTime endTime, out string fileName)
        {
            string path = Application.StartupPath + "\\tradeRecode\\";
            DirectoryInfo di = new DirectoryInfo(path);
            if (di.Exists == false) {
                di.Create();
            }
            fileName = string.Empty;

            try {
                if (this.checkTable() == false) {
                    return false;
                }
                var sql = string.Format("SELECT * FROM {0} WHERE '{1}' < buyTime AND buyTime < '{2}'",
                    this.tableName(), startTime.ToString(dateTimeFmt_), endTime.ToString(dateTimeFmt_));

                var dt = this.sqlDB_.readDataTableQuery(sql);
                if (dt.Rows.Count == 0) {
                    return false;
                }

                string fmt = "yyyy-MM-ddTHHmm";
                fileName = string.Format("{0}{1}-{2}", path, startTime.ToString(fmt), endTime.ToString(fmt));

                ExcelParser parser = new ExcelParser(fileName);
                parser.save(dt);
                return true;
            }
            catch (Exception e) {
                Logger.getInstance.print(KiwoomCode.Log.에러, "sqlite {0} 로딩 실패. {1} / {2}", this.tableName(), e.Message, e.StackTrace);
                return false;
            }
        }

        class ModuleRecode
        {
            public string moduleName_;
            public int count_ = 1;
        }

        public bool selectTradeModule()
        {
            try {
                if (this.checkTable() == false) {
                    return false;
                }
                var sql = string.Format("SELECT buyModule FROM {0}", this.tableName());

                var dt = this.sqlDB_.readDataTableQuery(sql);
                if (dt.Rows.Count == 0) {
                    return false;
                }

                Dictionary<string, ModuleRecode> temp = new Dictionary<string, ModuleRecode>();

                foreach (DataRow row in dt.Rows) {
                    string moduleName = (string) row[0];
                    ModuleRecode record;
                    if (temp.TryGetValue(moduleName, out record)) {
                        record.count_++;
                    }
                    else {
                        record = new ModuleRecode();
                        record.moduleName_ = moduleName;
                        temp[moduleName] = record;
                    }
                }

                var moduleList = StrategyModuleList.getInstance;
                var pool = moduleList.getTradedModulePool();
                var query = from module in temp
                            orderby module.Value.count_ descending
                            select module.Value.moduleName_;

                foreach (string moduleName in query) {
                    var module = moduleList.parseModule(moduleName);
                    if (module == null) {
                        continue;
                    }
                    // 젠을 할때기존에 검색됬던 모듈을 가장 앞에 둬서 우선순위를 준다
                    moduleList.getGenPool().Insert(0, module);
                    if (pool.Count <= PublicVar.quickPoolCount) {
                        pool[module.name_] = module;
                    }
                }

                return true;
            }
            catch (Exception e) {
                Logger.getInstance.print(KiwoomCode.Log.에러, "sqlite {0} 로딩 실패. {1} / {2}", this.tableName(), e.Message, e.StackTrace);
                return false;
            }
            finally {
                var moduleList = StrategyModuleList.getInstance;
                var pool = moduleList.getTradedModulePool();
                if (pool.Count < PublicVar.quickPoolCount) {
                    string[] sample = moduleList.getDefaultModules();
                    foreach (var moduleName in sample) {
                        var module = StrategyModuleList.getInstance.getGenPoolAtName(moduleName);
                        if (module == null) {
                            continue;
                        }
                        if (pool.ContainsKey(module.name_) == false) {
                            pool.Add(module.name_, module);
                            if (pool.Count > PublicVar.quickPoolCount) {
                                break;
                            }
                        }
                    }
                }
            }

        }

        public override void update(FutureData futureData)
        {
        }

        public void update(FutureData futureData, string why)
        {
            try {
                if (this.checkTable() == false) {
                    // 테이블이 없으면 만든다
                    this.createTable();
                }
                if (futureData == null) {
                    return;
                }
                var tradeModule = futureData.tradeModule();
                if (tradeModule == null) {
                    return;
                }
                var sql = string.Format("INSERT INTO {0} ", this.tableName());
                sql += "(name, code, position, buyTime, haveTime, buyPrice, payOffPrice, low, high, expendPrice, expendRate, testCount, profit, why, buyModule, sellModule) ";
                sql += string.Format(" VALUES ('{0}', '{1}', '{2}', '{3}', {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, '{13}', '{14}', '{15}')",
                    futureData.name_,                                           //0
                    futureData.code_,                                           //1
                    futureData.position_.ToString(),                            //2
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),               //3
                    futureData.positionHaveMin(),                               //4
                    futureData.buyPrice_,                                       //5
                    futureData.nowOneProfit(),                                  //6
                    futureData.minOneProfit_,                                      //7
                    futureData.maxProfit_,                                      //8
                    tradeModule.avgProfit_,                         //9
                    tradeModule.expectedWinRate_,                   //10
                    tradeModule.tradeCount_,                        //11
                    futureData.nowProfit(),                                     //12
                    why,                                                        //13
                    tradeModule.buyTradeModule_.getName(),          //14
                    tradeModule.sellTradeModule_.getName()          //15
                    );

                this.sqlDB_.updateQuery(sql);
            }
            catch (Exception e) {
                Logger.getInstance.print(KiwoomCode.Log.에러, "{0} sqlite {1} 입력 실패. {2} / {3}", futureData.name_, this.tableName(), e.Message, e.StackTrace);
            }
        }

        public override void delete(FutureData futureData)
        {
        }
    }
}
