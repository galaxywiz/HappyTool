using HappyFuture.FundManage;
using System;
using System.Reflection;

namespace HappyFuture.DB
{
    class FutureFundModuleDB: FutureSqliteDB
    {
        public FutureFundModuleDB(string dbFile) : base(dbFile)
        {
        }

        protected override string tableName()
        {
            return string.Format("FUND_TABLE");
        }

        protected override void createTable()
        {
            // 코드, buy모듈, sell모듈, rank (이익율 많은 순서)
            string column = "code TEXT, fundName TEXT, goalName TEXT, lostCut float, targetProfit float, updateTime datetime";
            string sql = string.Format("CREATE TABLE {0} ({1})", this.tableName(), column);
            this.sqlDB_.executeQuery(sql);
        }

        FutureFundManagement getFundManagement(string name)
        {
            if (name.Length == 0) {
                return null;
            }
            Assembly assembly = Assembly.GetExecutingAssembly();
            Type type = assembly.GetType("HappyFuture.FundManage." + name);
            if (type == null) {
                return null;
            }
            var bot = ControlGet.getInstance.futureBot();
            FutureFundManagement instance = Activator.CreateInstance(type, bot) as FutureFundManagement;
            return instance;
        }

        public override bool select(ref FutureData futureData)
        {
            return true;

            //if (PublicVar.eachFundManagement == false) {
            //    return true;
            //}
            //try {
            //    if (this.checkTable() == false) {
            //        return false;
            //    }
            //    var sql = string.Format("SELECT fundName, goalName, lostCut, targetProfit, updateTime FROM {0} WHERE code = '{1}'", this.tableName(), futureData.code_);

            //    var dt = this.sqlDB_.readDataTableQuery(sql);
            //    if (dt.Rows.Count == 0) {
            //        return false;
            //    }
            //    futureData.clearFundManagement();

            //    foreach (DataRow row in dt.Rows) {
            //        string fundName = row[0].ToString();
            //        string goalName = row[1].ToString();
            //        double lostCut = (double) row[2];
            //        double targetProfit = (double) row[3];
            //        DateTime date = (DateTime) row[4];

            //        futureData.fundUpdateTime_ = date;
            //        var fund = this.getFundManagement(fundName);
            //        if (fund == null) {
            //            continue;
            //        }
            //        var goal = this.getFutureGoalPrice(goalName);
            //        if (goal == null) {
            //            continue;
            //        }
            //        fund.goalPrice_ = goal;

            //        futureData.fundManagement_ = fund;
            //        futureData.lostCutTime_ = lostCut;
            //        futureData.targetProfitTime_ = targetProfit;
            //    }
            //    return true;
            //}
            //catch (Exception e) {
            //    Logger.getInstance.print(KiwoomCode.Log.에러, "{0} sqlite {1} 로딩 실패. {2} / {3}", futureData.name_, this.tableName(), e.Message, e.StackTrace);
            //    return false;
            //}
        }

        public override void update(FutureData futureData)
        {
            //try {
            //    if (this.checkTable() == false) {
            //        // 테이블이 없으면 만든다
            //        this.createTable();
            //    }
            //    this.delete(futureData);

            //    string sql = "";

            //    var fund = futureData.fundManagement_;
            //    if (fund != null) {
            //        var goal = fund.goalPrice_;
            //        string goalName = "";
            //        if (goal != null) {
            //            goalName = goal.GetType().Name;
            //        }
            //        sql = string.Format("INSERT INTO {0} ", this.tableName());
            //        sql += " ('code', 'fundName', 'goalName', 'lostCut', 'targetProfit', 'updateTime') ";
            //        sql += string.Format(" VALUES ('{0}', '{1}', '{2}', {3}, {4}, '{5}')",
            //            futureData.code_, fund.GetType().Name, goalName, futureData.lostCutTime_, futureData.targetProfitTime_, futureData.fundUpdateTime_.ToString(dateTimeFmt_));
            //    }
            //    else {
            //        sql = string.Format("INSERT INTO {0} ", this.tableName());
            //        sql += " ('code', 'fundName', 'goalName', 'lostCut', 'targetProfit', 'updateTime') ";
            //        sql += string.Format(" VALUES ('{0}', '{1}', '{2}', {3}, {4}, '{5}')",
            //            futureData.code_, "", "", futureData.lostCutTime_, futureData.targetProfitTime_, futureData.fundUpdateTime_.ToString(dateTimeFmt_));
            //    }

            //    this.sqlDB_.updateQuery(sql);
            //}
            //catch (Exception e) {
            //    Logger.getInstance.print(KiwoomCode.Log.에러, "{0} sqlite {1} 입력 실패. {2} / {3}", futureData.name_, this.tableName(), e.Message, e.StackTrace);
            //}
        }

        public override void delete(FutureData futureData)
        {
            string sql = string.Format("DELETE FROM {0} WHERE code = '{1}'", this.tableName(), futureData.code_);
            this.sqlDB_.executeQuery(sql);
        }
    }

}
