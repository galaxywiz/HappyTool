using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Windows.Forms;

namespace UtilLibrary
{
    public class SqliteDB
    {
        readonly string dataSource_ = null;
        readonly object lock_ = new object();
        readonly Thread thread_ = null;
        readonly Queue<string> sqlList_ = new Queue<string>();

        string pathFile_;
        public SqliteDB(string dbFile)
        {
            try {
                string path = "";
                if (dbFile.Contains("\\")) {
                    pathFile_ = dbFile;
                }
                else {
                    path = Application.StartupPath + "\\db\\";
                    pathFile_ = path + dbFile;
                }
                DirectoryInfo di = new DirectoryInfo(path);
                if (di.Exists == false) {
                    di.Create();
                    SQLiteConnection.CreateFile(pathFile_);
                }
                this.dataSource_ = string.Format(@"Data Source={0}", pathFile_);

                this.thread_ = new Thread(() => this.run());
                this.thread_.Priority = ThreadPriority.Highest;
                this.thread_.Start();
            } catch(Exception e) {
                Logger.getInstance.print(KiwoomCode.Log.주식봇, "{0} db 가 생성 안됨", dbFile);
            }
        }

        ~SqliteDB()
        {
            using (var conn = new SQLiteConnection(this.dataSource_)) {
                conn.Close();
            }
        }

        bool shutdown_ = false;
        public void stop()
        {
            this.shutdown_ = true;
        }

        void run()
        {
            while (this.shutdown_ == false) {
                Thread.Sleep(50);
                if (this.sqlList_.Count == 0) {
                    continue;
                }

                string sql = "";
                lock (this.lock_) {
                    sql = this.sqlList_.Dequeue();
                }

                if (sql.Length > 0) {
                    this.executeQuery(sql);
                }
            }
        }
        public int queryWaitCount()
        {
            return this.sqlList_.Count;
        }

        public void copyBackup()
        {
            DateTime now = DateTime.Now;
            var destFile = string.Format("%s.%d.%d.%d", pathFile_, now.Year, now.Month, now.Day);
            try {
                File.Copy(pathFile_, destFile);
            }catch (Exception e) {
                Logger.getInstance.print(KiwoomCode.Log.에러, "{0} db 복사중 에러 {1}/{2}", pathFile_, e.Message, e.StackTrace);
            }
        }

        public void vacuum()
        {
            this.executeQuery("vacuum;");
        }

        public void executeQuery(string sql)
        {
            try {
                using (var conn = new SQLiteConnection(this.dataSource_)) {
                    conn.Open();
                    lock (this.lock_) {
                        SQLiteCommand cmd = new SQLiteCommand(sql, conn);
                        cmd.ExecuteNonQuery();
                    }
                    conn.Close();
                }
            }
            catch (Exception e) {
                Logger.getInstance.print(KiwoomCode.Log.에러, "{0} 쿼리 실행중 에러 {1}/{2}", sql, e.Message, e.StackTrace);
            }
        }

        // update 전용 쿼리, queue에 담았다가 쏘도록 함.
        public void updateQuery(string sql)
        {
            if (sql.Length == 0) {
                return;
            }
            lock (this.lock_) {
                this.sqlList_.Enqueue(sql);
            }
        }

        public void updateQueryList(List<string> sqlList)
        {
            if (sqlList.Count == 0) {
                return;
            }

            string totalSql = "BEGIN TRANSACTION; ";
            foreach (var sql in sqlList) {
                totalSql = string.Format("{0}; {1}", totalSql, sql);
            }
            totalSql += "; COMMIT;";

            this.updateQuery(totalSql);
        }

        // 즉시 실행하는 쿼리
        public void executeQueryList(List<string> sqlList)
        {
            if (sqlList.Count == 0) {
                return;
            }

            string totalSql = "BEGIN TRANSACTION; ";
            foreach (var sql in sqlList) {
                totalSql = string.Format("{0}; {1}", totalSql, sql);
            }
            totalSql += "; COMMIT;";

            this.executeQuery(totalSql);
        }

        public DataTable readDataTableQuery(string sql)
        {
            DataTable dt = new DataTable();
            //SQLiteDataAdapter 클래스를 이용 비연결 모드로 데이타 읽기
            try {
                var adpt = new SQLiteDataAdapter(sql, this.dataSource_);                
                adpt.Fill(dt);
            } catch(Exception e) {
                return null;
            }
            return dt;
        }

        public delegate void ReadSQLite(SQLiteDataReader rdr);

        public void readQuery(string sql, ReadSQLite readSQLite)
        {
            using (var conn = new SQLiteConnection(this.dataSource_)) {
                conn.Open();

                //SQLiteDataReader를 이용하여 연결 모드로 데이타 읽기
                SQLiteCommand cmd = new SQLiteCommand(sql, conn);
                SQLiteDataReader rdr = cmd.ExecuteReader();
                readSQLite(rdr);

                rdr.Close();
            }
        }

        public void saveDataTable(DataTable dt, string tableName)
        {
            using (var conn = new SQLiteConnection(this.dataSource_)) {
                conn.Open();

                string sql = string.Format("SELECT * FROM {0}", tableName);
                var adapter = new SQLiteDataAdapter(sql, this.dataSource_);
                adapter.AcceptChangesDuringFill = false;
                SQLiteCommandBuilder builder = new SQLiteCommandBuilder(adapter);
                adapter.Update(dt);

                conn.Close();
            }
        }

        public bool checkTableName(string tableName)
        {
            var sql = string.Format("SELECT count(*) FROM sqlite_master WHERE Name = '{0}'", tableName);

            bool ret = false;
            ReadSQLite read = (SQLiteDataReader rdr) => {
                if (rdr.Read()) {
                    if (int.Parse(rdr[0].ToString()) == 1) {
                        ret = true;
                    }
                }
            };

            this.readQuery(sql, read);
            return ret;
        }
    }

    //----------------------------------------------------------------------
    // 기본 구조
    public abstract class BaseSqliteDB
    {
        protected const string dateTimeFmt_ = "yyyy-MM-dd HH:mm:ss";

        protected SqliteDB sqlDB_;
        public BaseSqliteDB(string dbFile)
        {
            this.sqlDB_ = new SqliteDB(dbFile);
        }

        ~BaseSqliteDB()
        {
            this.sqlDB_.stop();
        }

        protected abstract string tableName();
        protected abstract void createTable();
        protected bool checkTable()
        {
            return this.sqlDB_.checkTableName(this.tableName());
        }

        public int queryWaitCount()
        {
            return this.sqlDB_.queryWaitCount();
        }

        public void vaccum()
        {
            this.sqlDB_.copyBackup();
            this.sqlDB_.vacuum();
        }
        // select / update는 알아서 밑에서 알아서 구현
    }
}