using StockLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using UtilLibrary;

namespace HappyFuture.DB
{
    class FuturePriceDB: FutureSqliteDB
    {
        public FuturePriceDB(string dbFile) : base(dbFile)
        {
        }

        FutureData futureData_;
        const string TABLE_NAME = "PRICE_TABLE_";
        protected string tableName(int min)
        {
            if (this.futureData_ == null) {
                return "NULL";
            }
            var code = futureData_.regularCode();

            var tableName = string.Format("{0}{1}_{2}", TABLE_NAME, min, code);
            return tableName;
        }

        protected override string tableName()
        {
            var bot = ControlGet.getInstance.futureBot();
            int min = bot.priceTypeMin();
            return tableName(min);
        }

        protected bool checkTable(int min)
        {
            return this.sqlDB_.checkTableName(this.tableName(min));
        }

        protected void createTable(int min)
        {
            var tableName = this.tableName(min);
            string column = "candleTime DATETIME NOT NULL PRIMARY KEY, start FLOAT, low FLOAT, high FLOAT, close FLOAT, volume UINT";
            string sql = string.Format("CREATE TABLE {0} ({1})", tableName, column);
            this.sqlDB_.executeQuery(sql);
            sql = string.Format("CREATE INDEX idx_{0} ON {1} (candleTime DESC)", tableName, tableName);
            this.sqlDB_.executeQuery(sql);
        }

        protected override void createTable()
        {
            var bot = ControlGet.getInstance.futureBot();
            int min = bot.priceTypeMin();
            this.createTable(min);
        }

        public bool select(ref FutureData futureData, int min)
        {
            this.futureData_ = futureData;
            try {
                if (this.checkTable() == false) {
                    return false;
                }

                var sql = string.Format("SELECT candleTime, start, low, high, close, volume FROM {0} ORDER BY candleTime DESC LIMIT {1}", this.tableName(min), PublicVar.priceTableCount);

                var dt = this.sqlDB_.readDataTableQuery(sql);
                if (dt.Rows.Count == 0) {
                    return false;
                }
                var futureBot = ControlGet.getInstance.futureBot();

                foreach (DataRow row in dt.Rows) {
                    DateTime date = (DateTime) row[0];
                    string dateStr = date.ToString("yyyyMMddHHmmss");
                    double start = (double) row[1];
                    double low = (double) row[2];
                    double high = (double) row[3];
                    double price = (double) row[4];
                    UInt64 volumn = UInt64.Parse(row[5].ToString());

                    CandleData priceData = new CandleData(dateStr, price, start, high, low, volumn);
                    priceData.dbSaved_ = true;
                    futureData.updatePrice(priceData);
                }
                futureData.updatePriceTable(futureBot);
            }
            catch (Exception e) {
                Logger.getInstance.print(KiwoomCode.Log.에러, "{0} sqlite {1} 로딩 실패. {2} / {3}", futureData.name_, this.tableName(min), e.Message, e.StackTrace);
                return false;
            }
            return true;
        }

        public override bool select(ref FutureData futureData)
        {
            var bot = ControlGet.getInstance.futureBot();
            int min = bot.priceTypeMin();
            return this.select(ref futureData, min);
        }

        string replaceSQL(CandleData candle, int min)
        {
            var tableName = this.tableName(min);
            
            var sql = string.Format("INSERT OR REPLACE INTO {0} ", tableName);
            sql += " ('candleTime', 'start', 'low', 'high', 'close','volume') ";
            sql += string.Format(" VALUES ('{0}', {1}, {2}, {3}, {4}, {5})",
                candle.date_.ToString(dateTimeFmt_),
                candle.startPrice_,
                candle.lowPrice_,
                candle.highPrice_,
                candle.price_,
                candle.volume_);
            return sql;
        }

        public void update(FutureData futureData, int min)
        {
            this.futureData_ = futureData;
            try {
                if (this.checkTable(min) == false) {
                    // 테이블이 없으면 만든다
                    this.createTable(min);
                }
                if (futureData.priceDataCount() == 0) {
                    return;
                }

                List<string> sqls = new List<string>();
                // 연구 하다가 포기..
                // var dt = futureData.getPriceTable();
                // this.saveDataTable(dt, this.tableName(futureData));
                var priceTable = futureData.priceTable();

                foreach (var candle in priceTable) {
                    if (candle.dbSaved_ || candle.makeCandle_) {
                        continue;
                    }
                    if ((candle.date_.Minute % min) != 0) {
                        Logger.getInstance.print(KiwoomCode.Log.에러, "{0} 에서 잘못된 캔들 데이터 {1}", futureData.name_, candle.date_);
                    }
                    var sql = this.replaceSQL(candle, min);
                    sqls.Add(sql);
                }
                futureData.setPriceTableDBSaved();
                this.sqlDB_.executeQueryList(sqls);
            }
            catch (Exception e) {
                Logger.getInstance.print(KiwoomCode.Log.에러, "{0} sqlite {1} 입력 실패. {2} / {3}", futureData.name_, this.tableName(), e.Message, e.StackTrace);
            }
        }

        public override void update(FutureData futureData)
        {
            var bot = ControlGet.getInstance.futureBot();
            int min = bot.priceTypeMin();
            this.update(futureData, min);
        }

        public override void delete(FutureData futureData)
        {
            string sql = string.Format("DELETE FROM {0}", this.tableName());
            this.sqlDB_.executeQuery(sql);
        }

        void deleteTimeLine(List<DateTime> delPool, int minType)
        {
            List<string> sqls = new List<string>();
            foreach(var dateTime in delPool) {
                var sql = string.Format("DELETE FROM {0} WHERE candleTime = '{1}'", this.tableName(minType), dateTime.ToString(dateTimeFmt_));
                sqls.Add(sql);
            }
            this.sqlDB_.executeQueryList(sqls);
        }

        //---------------------------------------------------------------------------
        // 시뮬레이션 기능을 위한것들
        public void getTableList(ref List<string> list)
        {
            string sql = string.Format("SELECT name FROM sqlite_master WHERE type = 'table' and name like '{0}%';", TABLE_NAME);
            var dt = this.sqlDB_.readDataTableQuery(sql);
            if (dt.Rows.Count == 0) {
                return;
            }
            var futureBot = ControlGet.getInstance.futureBot();

            foreach (DataRow row in dt.Rows) {
                string codeName = (string) row[0];
                var token = codeName.Split('_');
                if (token.Length == 4) {
                    codeName = token[3];
                    if (list.Contains(codeName) == false) {
                        list.Add(codeName);
                    } 
                }
            }
        }

        string makeSql(int minType, string code, DateTime startTime, DateTime endTime)
        {
            var sql = string.Format("SELECT candleTime, start, low, high, close, volume " +
               "FROM {0}{1}_{2} WHERE candleTime >= '{3}' AND candleTime <= '{4}' ORDER bY candleTime DESC;",
               TABLE_NAME, minType, code, startTime.ToString("yyyy-MM-dd"), endTime.AddDays(1).ToString("yyyy-MM-dd"));

            return sql;
        }

        public bool selectForSimule(List<string> codePool, DateTime startTime, DateTime endTime, int minType, ref Dictionary<string, StockData> pool)
        {
            var futureBot = ControlGet.getInstance.futureBot();

            foreach (string code in codePool) {
                if (pool.ContainsKey(code)) {
                    continue;
                }

                var delPool = new List<DateTime>();
                try {
                    bool directData = true;
                    var sql = this.makeSql(minType, code, startTime, endTime);
                    var dt = this.sqlDB_.readDataTableQuery(sql);
                    if (dt == null || dt.Rows.Count == 0) {
                        directData = false;
                        sql = this.makeSql(1, code, startTime, endTime);
                        dt = this.sqlDB_.readDataTableQuery(sql);
                        if (dt == null) {
                            continue;
                        }
                        if (dt.Rows.Count == 0) {
                            continue;
                        }
                    }
                    Logger.getInstance.print(KiwoomCode.Log.주식봇, "{0} 의 데이터 {1}개 로딩", code, dt.Rows.Count);
                    StockData stockData = new StockData(code, code);

                    if (directData) {
                        DateTime nextTime = DateTime.MinValue;
                        foreach (DataRow row in dt.Rows) {
                            DateTime date = (DateTime) row[0];

                            string dateStr = date.ToString("yyyyMMddHHmmss");
                            double start = (double) row[1];
                            double low = (double) row[2];

                            double high = (double) row[3];
                            double price = (double) row[4];
                            UInt64 volumn = UInt64.Parse(row[5].ToString());

                            var tempDate = CandleData.strToDateTime(dateStr);
                            if ((tempDate.Minute % minType) != 0) {
                                Logger.getInstance.print(KiwoomCode.Log.에러, "{0}에 잘못된 시간 데이터:[{1}] 이 추가 되어 있음.", this.tableName(minType), tempDate);
                                delPool.Add(tempDate);
                                continue;
                            }
                            CandleData priceData = new CandleData(dateStr, price, start, high, low, volumn);
                            stockData.updatePrice(priceData);
                            nextTime = date;
                        }
                        this.deleteTimeLine(delPool, minType);
                    }
                    else {
                        double start = 0, clow = 0, chigh = 0, price = 0;
                        UInt64 volumn;
                        string dateStr = "";
                        int i = 0;
                        foreach (DataRow row in dt.Rows) {
                            DateTime date = (DateTime) row[0];
                            if (i == 0) {
                                dateStr = date.ToString("yyyyMMddHHmmss");
                                clow = double.MaxValue;
                                chigh = double.MinValue;
                                price = (double) row[4];
                            }
                            start = (double) row[1];
                            var low = (double) row[2];
                            var high = (double) row[3];
                            volumn = UInt64.Parse(row[5].ToString());

                            clow = Math.Min(clow, low);
                            chigh = Math.Max(chigh, high);
                            if (++i >= minType) {
                                CandleData priceData = new CandleData(dateStr, price, start, chigh, clow, volumn);
                                stockData.updatePrice(priceData);
                                i = 0;
                            }
                        }
                    }
                    stockData.updatePriceTable(futureBot);
                    pool.Add(code, stockData);
                }
                catch (Exception e) {
                    Logger.getInstance.print(KiwoomCode.Log.에러, "sqlite 타임머신 기능 실패 code[{0}] time[{1} ~ {2}] candleTime 부터 로딩이 안됨. {3} / {4}", code, startTime, endTime, e.Message, e.StackTrace);
                    continue;
                }
            }

            return true;
        }
    }
}
