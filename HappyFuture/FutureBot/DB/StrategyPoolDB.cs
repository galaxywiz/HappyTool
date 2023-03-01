using HappyFuture.FundManage;
using StockLibrary;
using StockLibrary.StrategyManager;
using System;
using System.Collections.Generic;
using System.Data;
using UtilLibrary;

namespace HappyFuture.DB
{
    class FutureFundPoolDB: FutureSqliteDB
    {
        public FutureFundPoolDB(string dbFile) : base(dbFile)
        {
        }

        protected override string tableName()
        {
            return string.Format("FUND_POOL_TABLE");
        }

        protected override void createTable()
        {
            // 코드, buy모듈, sell모듈, rank (이익율 많은 순서)
            string column = "code TEXT, entrie TEXT, entrieAtt TEXT, better TEXT, lostCut FLOAT, targetProfit FLOAT, updateTime DATETIME, rank INT";
            string sql = string.Format("CREATE TABLE {0} ({1})", this.tableName(), column);
            this.sqlDB_.executeQuery(sql);
        }
        const string TREND = "TREND";
        const string REV = "REV";
        const string UNIVERSAL_CODE = "UNIVERSAL";
        const string SELECT_SQL = "SELECT entrie, entrieAtt, better, lostCut, targetProfit FROM {0} WHERE code = '{1}' order by rank ASC";
        List<FutureFundManagement> select(string sql)
        {
            try {
                if (this.checkTable() == false) {
                    return null;
                }

                var dt = this.sqlDB_.readDataTableQuery(sql);
                if (dt == null) {
                    return null;
                }
                if (dt.Rows.Count == 0) {
                    return null;
                }

                List<FutureFundManagement> ret = new List<FutureFundManagement>();
                var fundList = FutureFundManagementList.getInstance;
                foreach (DataRow row in dt.Rows) {
                    int i = 0;
                    string entrieName = row[i++].ToString();
                    string entrieAttr = row[i++].ToString();
                    string bettorName = row[i++].ToString();
                    double lostCut = (double) row[i++];
                    double targetProfit = (double) row[i++];

                    // 갱신 날짜만 로딩
                    var bot = ControlGet.getInstance.futureBot();
                    var fund = fundList.getFundManagementCombineStrategyModule(entrieName, bot);
                    if (fund == null) {
                        continue;
                    }
                    var strategy = fund.strategyModule_;    
                    strategy.parseDBString(entrieAttr);

                    var assetBettor = AssetBettorList.getInstance.getAssetBettor(bettorName);
                    strategy.setBettor(assetBettor);

                    fund.lostCutTime_ = lostCut;
                    fund.targetProfitTime_ = targetProfit;
                    ret.Add(fund);
                }
                return ret;
            }
            catch (Exception e) {
                Logger.getInstance.print(KiwoomCode.Log.에러, "{0} sqlite 로딩 실패. {1} / {2}", this.tableName(), e.Message, e.StackTrace);
                return null;
            }
        }

        public override bool select(ref FutureData futureData)
        {
            try {
                var sql = string.Format(SELECT_SQL, this.tableName(), futureData.code_);
                var list = this.select(sql);
                if (list == null || list.Count == 0) {
                    return false;
                }

                futureData.fundManagement_ = list[0];                
                return true;
            }
            catch (Exception e) {
                Logger.getInstance.print(KiwoomCode.Log.에러, "{0} sqlite {1} 로딩 실패. {2} / {3}", futureData.name_, this.tableName(), e.Message, e.StackTrace);
                return false;
            }
        }
    
        public bool selectUniversal()
        {
            try {
                var sql = string.Format(SELECT_SQL, this.tableName(), UNIVERSAL_CODE);
                var list = this.select(sql);
                if (list == null || list.Count == 0) {
                    return false;
                }
                var autoPool = FutureFundManagementList.getInstance.universalPool_;
                autoPool.Clear();

                int index = 0;
                foreach (var fund in list) {
                    var key = string.Format(PublicVar.fundPoolKey, index++);
                    fund.name_ = key;
                    autoPool.Add(key, fund);
                }

                return true;
            }
            catch (Exception e) {
                Logger.getInstance.print(KiwoomCode.Log.에러, "{0} sqlite {1} 로딩 실패. {2} / {3}", UNIVERSAL_CODE, this.tableName(), e.Message, e.StackTrace);
                return false;
            }
        }

        string update(StrategyManagement fund, string code, int rank = 0)
        {
            string sql = "";
            try {
                DateTime now = DateTime.Now;                
                var fundName = fund.name();
                var strategyModule = fund.strategyModule_;

                var entrieAttr = strategyModule.dbString();
                var bettor = strategyModule.getBettorName();
                var lostCut = fund.lostCutTime_;
                var targetProfit = fund.targetProfitTime_;
                var nowDate = now.ToString(dateTimeFmt_);

                sql = string.Format("INSERT INTO {0} ", this.tableName());
                sql += " ('code', 'entrie', 'entrieAtt', 'better', 'lostCut', 'targetProfit', 'updateTime', 'rank') ";
                sql += string.Format(" VALUES ('{0}', '{1}', '{2}', '{3}', {4}, {5}, '{6}', {7})",
                    code, strategyModule.GetType().Name, entrieAttr, bettor, lostCut, targetProfit, nowDate, rank);

            }
            catch (Exception e) {
                Logger.getInstance.print(KiwoomCode.Log.에러, "{0} sqlite {1} 입력 실패. {2} / {3}", code, this.tableName(), e.Message, e.StackTrace);
            }
            return sql;
        }

        void update(List<StrategyManagement> fundPool, string code)
        {
            const int LIMIT = 50;

            try {
                List<string> sqls = new List<string>();
                DateTime now = DateTime.Now;
                for (int index = 0; index < fundPool.Count; ++index) {
                    var combinFund = fundPool[index] as FutureFundManagement;
                    var sql = this.update(combinFund, code, index);
                    if (sql.Length == 0) {
                        continue;
                    }
                    sqls.Add(sql);
                    if (index > LIMIT) {
                        break;
                    }
                }

                this.sqlDB_.updateQueryList(sqls);
            }
            catch (Exception e) {
                Logger.getInstance.print(KiwoomCode.Log.에러, "{0} sqlite {1} 입력 실패. {2} / {3}", code, this.tableName(), e.Message, e.StackTrace);
            }
        }

        public override void update(FutureData futureData)
        {
            if (this.checkTable() == false) {
                // 테이블이 없으면 만든다
                this.createTable();
            }
            else {
                // 재갱신
                this.delete(futureData);
            }
            var autoPool = futureData.fundList_;
            if (autoPool.Count == 0) {
                return;
            }

            var list = new List<StrategyManagement>(autoPool.Values);
            this.update(list, futureData.regularCode());
        }

        public void updateUniversal()
        {
            if (this.checkTable() == false) {
                // 테이블이 없으면 만든다
                this.createTable();
            }
            else {
                // 재갱신
                this.delete(UNIVERSAL_CODE);
            }

            var autoPool = FutureFundManagementList.getInstance.universalPool_;
            if (autoPool.Count == 0) {
                return;
            }

            var list = new List<StrategyManagement>(autoPool.Values);
            this.update(list, UNIVERSAL_CODE);
        }

        void delete(string code)
        {
            string sql = string.Format("DELETE FROM {0} WHERE code = '{1}'", this.tableName(), code);
            this.sqlDB_.executeQuery(sql);
        }

        public override void delete(FutureData futureData)
        {
            this.delete(futureData.regularCode());
        }
    }
}
