using HappyTool.Stock;
using StockLibrary;
using StockLibrary.DB;
using System;
using System.Collections.Generic;
using System.Data;
using UtilLibrary;

namespace HappyTool
{
    class StockDB
    {
        TradeModuleDB moduleDB_;
        TargetStockDB targetDB_;
        internal StockDataPriceDB dayPriceDB_;
        TodayTargetStockDB yesterDayFilterDB_;

        public StockDB(string dbFile)
        {
            moduleDB_ = new TradeModuleDB(dbFile);
            targetDB_ = new TargetStockDB(dbFile);
            dayPriceDB_ = new StockDataPriceDB("KoreaStockData.db");
            yesterDayFilterDB_ = new TodayTargetStockDB(PublicVar.yesterDayFilterDB);
        }
        //------------------------------------------------------------
        // 가격표 저장 로직
        public bool selectStockPrice(ref StockData futureData)
        {
            return dayPriceDB_.select(ref futureData);
        }

        public void updateStockPrice(StockData futureData)
        {
            dayPriceDB_.update(futureData);
        }
        public void getYesterDayFilterkList(StockBot bot)
        {
            yesterDayFilterDB_.getStockList(bot);
        }
        //------------------------------------------------------------
        // 모듈 저장
        public void selectTradeModule(ref StockData stockData)
        {
            if (stockData.tradeModuleUpdateTime_ == DateTime.MinValue) {
                moduleDB_.select(ref stockData);
            }
        }

        public void updateTradeModule(StockData stockData)
        {
            moduleDB_.update(stockData);
        }

        public bool selectTodayStock(StockBot bot)
        {
            return targetDB_.select(bot);
        }

        public void updateTodayStock(StockBot bot)
        {
            targetDB_.update(bot);
        }
    }

    public class TargetStockDB: StockDataSqliteDB
    {
        public TargetStockDB(string dbFile) : base(dbFile)
        {
            Logger.getInstance.print(KiwoomCode.Log.주식봇, "오늘의 리스트 db {0}", dbFile);
        }

        protected override string tableName()
        {
            return string.Format("TARGET_STOCKS");
        }

        protected override void createTable()
        {
            // 코드, buy모듈, sell모듈, rank (이익율 많은 순서)
            string column = "code TEXT, name TEXT, searchDate datetime";
            string sql = string.Format("CREATE TABLE {0} ({1})", this.tableName(), column);
            sqlDB_.executeQuery(sql);
        }
        public DateTime today()
        {
            var now = DateTime.Now;
            return new DateTime(now.Year, now.Month, now.Day);
        }

        public override bool select(ref StockData StockData)
        {
            throw new NotImplementedException();
        }

        public bool select(StockBot bot)
        {
            try {
                if (this.checkTable() == false) {
                    return false;
                }
                var sql = string.Format("SELECT code, name FROM {0} WHERE searchDate = '{1}'", this.tableName(), this.today().ToString(dateTimeFmt_));

                var dt = sqlDB_.readDataTableQuery(sql);
                if (dt.Rows.Count == 0) {
                    return false;
                }

                foreach (DataRow row in dt.Rows) {
                    string code = row[0].ToString();
                    string name = row[1].ToString();
                    KStockData kStockData = new KStockData(code, name);
                    bot.addStockData(kStockData);
                }
                return true;
            }
            catch (Exception e) {
                Logger.getInstance.print(KiwoomCode.Log.에러, "sqlite {0} 로딩 실패. {1} / {2}", this.tableName(), e.Message, e.StackTrace);
                return false;
            }
        }

        public override void update(StockData stockData)
        {
            return;
        }

        public void update(StockBot bot)
        {
            try {
                if (this.checkTable() == false) {
                    // 테이블이 없으면 만든다
                    this.createTable();
                }
                var today = this.today().ToString(dateTimeFmt_);
                List<string> sqls = new List<string>();
                StockDataEach eachDo = (string code, StockData stockData) => {
                    var sql = string.Format("INSERT OR REPLACE INTO {0} ", this.tableName());
                    sql += " ('code', 'name', 'searchDate') ";
                    sql += string.Format(" VALUES ('{0}', '{1}', '{2}')", stockData.code_, stockData.name_, today);
                    sqls.Add(sql);
                };

                bot.doStocks(eachDo);
                sqlDB_.executeQueryList(sqls);
            }
            catch (Exception e) {
                Logger.getInstance.print(KiwoomCode.Log.에러, "sqlite {0} 입력 실패. {1} / {2}", this.tableName(), e.Message, e.StackTrace);
            }
        }

        public override void delete(StockData stockData)
        {
            string sql = string.Format("DELETE FROM {0} WHERE code = '{1}'", this.tableName(), stockData.code_);
            sqlDB_.executeQuery(sql);
        }
    }

    class TodayTargetStockDB: StockDataSqliteDB
    {
        public TodayTargetStockDB(string dbFile) : base(dbFile)
        {
        }
        protected override string tableName()
        {
            return "TODAY_STOCK_LIST";
        }

        protected override void createTable()
        {
        }

        public int getStockList(Bot bot)
        {
            int count = 0;
            try {
                var today = DateTime.Now;

                switch (today.DayOfWeek) {
                    case DayOfWeek.Sunday:
                        today = today.AddDays(-2);
                        break;
                    case DayOfWeek.Monday:
                        today = today.AddDays(-3);
                        break;

                    default:
                        today = today.AddDays(-1);
                        break;
                }
                var strToday = today.ToString("yyyy-MM-dd");
                var logs = string.Format("### [{0}] 의 추천 주식들 ###\n", strToday);

                string sql = string.Format("SELECT name, code, strategyAction FROM TODAY_STOCK_LIST WHERE candleTime = '{0}' and strategyAction = 'BUY' and vol > {1} and {2} < closePrice and closePrice < {3};",
                    strToday, PublicVar.limitVolumes, PublicVar.limitLowStockPrice, PublicVar.limitHighStockPrice);
                var dt = sqlDB_.readDataTableQuery(sql);
                if (dt == null) {
                    return 0;
                }
                if (dt.Rows.Count == 0) {
                    return count;
                }
                count = dt.Rows.Count;
                foreach (DataRow row in dt.Rows) {
                    string name = (string) row[0];
                    string code = (string) row[1];
                    string action = (string) row[2];

                    if (action.CompareTo("BUY") == 0) {
                        var stockData = new KStockData(code, name);
                        bot.addStockData(stockData);
                        logs += string.Format("- [{0}] {1} \n", name, code);
                    }
                }
                bot.copyRefStocks();
                bot.telegram_.sendMessage(logs);
            }
            catch (Exception e) {
                Logger.getInstance.print(KiwoomCode.Log.에러, "sqlite 로딩 실패. {0} / {1}", e.Message, e.StackTrace);
                return count;
            }
            return count;
        }
        public override bool select(ref StockData kStockData)
        {
            return false;
        }

        public override void update(StockData kStockData)
        {
        }

        public override void delete(StockData StockData)
        {
        }
    }

    class StockDataPriceDB: StockDataSqliteDB
    {
        public StockDataPriceDB(string dbFile) : base(dbFile)
        {
        }

        KStockData kStockData_;
        protected override string tableName()
        {
            return string.Format("day_price_{0}", kStockData_.code_);
        }

        protected override void createTable()
        {
            string column = "candleTime datetime NOT NULL PRIMARY KEY, start int, high int, low int, close int, vol int";
            string sql = string.Format("CREATE TABLE {0} ({1})", this.tableName(), column);
            sqlDB_.executeQuery(sql);
        }

        //public int getStockList(Bot bot)
        //{
        //    int count = 0;
        //    try {
        //        string sql = "SELECT name FROM sqlite_master WHERE type IN('table', 'view') AND name NOT LIKE 'sqlite_%' UNION ALL SELECT name FROM sqlite_temp_master WHERE type IN('table', 'view') ORDER BY 1;";
        //        var dt = sqlDB_.readDataTableQuery(sql);
        //        if (dt.Rows.Count == 0) {
        //            return count;
        //        }
        //        count = dt.Rows.Count;
        //        foreach (DataRow row in dt.Rows) {
        //            string code = (string) row[0];
        //            code = code.Replace("day_price_", "");
        //            var stockData = new KStockData(code, code);
        //            bot.addStockData(stockData);
        //        }
        //        bot.copyRefStocks();
        //    }
        //    catch (Exception e) {
        //        Logger.getInstance.print(KiwoomCode.Log.에러, "sqlite 로딩 실패. {0} / {1}", e.Message, e.StackTrace);
        //        return count;
        //    }
        //    return count;
        //}

        public override bool select(ref StockData kStockData)
        {
            kStockData_ = kStockData as KStockData;
            try {
                if (this.checkTable() == false) {
                    return false;
                }
                const int LIMIT_ROW = 600;
                var sql = string.Format("SELECT candleTime, start, high, low, close, vol FROM {0} ORDER BY candleTime DESC LIMIT {1}", this.tableName(), LIMIT_ROW);

                var dt = sqlDB_.readDataTableQuery(sql);
                if (dt == null) {
                    Logger.getInstance.print(KiwoomCode.Log.에러, "{0} sqlite {1} 로딩 실패. 날짜 파싱이 안된듯", kStockData.name_, this.tableName());
                    return false;
                }
                if (dt.Rows.Count == 0) {
                    return false;
                }

                foreach (DataRow row in dt.Rows) {
                    DateTime date = (DateTime) row[0];
                    string dateStr = date.ToString("yyyyMMddHHmmss");
                    double start = double.Parse(row[1].ToString());
                    double high = double.Parse(row[2].ToString());
                    double low = double.Parse(row[3].ToString());
                    double price = double.Parse(row[4].ToString());

                    UInt64 volumn = UInt64.Parse(row[5].ToString());

                    CandleData priceData = new CandleData(dateStr, price, start, high, low, volumn);
                    priceData.dbSaved_ = true;
                    kStockData.updatePrice(priceData);
                }
                var bot = ControlGet.getInstance.stockBot();
                kStockData.updatePriceTable(bot);
            }
            catch (Exception e) {
                Logger.getInstance.print(KiwoomCode.Log.에러, "{0} sqlite {1} 로딩 실패. {2} / {3}", kStockData.name_, this.tableName(), e.Message, e.StackTrace);
                return false;
            }
            return true;
        }

        public override void update(StockData kStockData)
        {
            kStockData_ = kStockData as KStockData;
            try {
                if (this.checkTable() == false) {
                    // 테이블이 없으면 만든다
                    this.createTable();
                }
                List<string> sqls = new List<string>();
                // var dt = futureData.getPriceTable();
                // this.saveDataTable(dt, this.tableName(futureData));
                var priceTable = kStockData_.priceTable();
                foreach (var candle in priceTable) {
                    if (candle.dbSaved_ == true) {
                        continue;
                    }
                    var sql = string.Format("INSERT OR REPLACE INTO {0} ", this.tableName());
                    sql += " ('시간', '종가', '시가', '고가', '저가', '거래량') ";
                    sql += string.Format(" VALUES ('{0}', {1}, {2}, {3}, {4}, {5})",
                        candle.date_.ToString(dateTimeFmt_),
                        candle.price_,
                        candle.startPrice_,
                        candle.highPrice_,
                        candle.lowPrice_,
                        candle.volume_);
                    sqls.Add(sql);
                }
                kStockData_.setPriceTableDBSaved();
                sqlDB_.executeQueryList(sqls);
            }
            catch (Exception e) {
                Logger.getInstance.print(KiwoomCode.Log.에러, "{0} sqlite {1} 입력 실패. {2} / {3}", kStockData.name_, this.tableName(), e.Message, e.StackTrace);
            }
        }

        public override void delete(StockData kStockData)
        {
            string sql = string.Format("DELETE FROM {0}", this.tableName());
            sqlDB_.executeQuery(sql);
        }

        //---------------------------------------------------------------------------
        // 시뮬레이션 기능을 위한것들
        public void getTableList(ref List<string> list)
        {
            string sql = string.Format("SELECT name FROM sqlite_master WHERE type = 'table' and name like '%PRICE_TABLE_%';");
            var dt = sqlDB_.readDataTableQuery(sql);
            if (dt.Rows.Count == 0) {
                return;
            }

            foreach (DataRow row in dt.Rows) {
                string tableName = (string) row[0];
                if (tableName.Contains("Upper")) {
                    continue;
                }
                tableName = tableName.Replace("PRICE_TABLE_", "");
                list.Add(tableName);
            }
        }

        public bool selectForSimule(List<string> codePool, DateTime startTime, ref Dictionary<string, StockData> pool)
        {
            try {
                foreach (string code in codePool) {
                    var sql = string.Format("SELECT 시간, 종가, 시가, 고가, 저가, 거래량 FROM PRICE_TABLE_{0} WHERE 시간 > '{1}' order by 시간 desc;",
                        code, startTime.ToString("yyyy-MM-dd hh:mm:ss"));

                    var dt = sqlDB_.readDataTableQuery(sql);
                    if (dt.Rows.Count == 0) {
                        continue;
                    }

                    StockData stockData = new StockData(code, code);
                    foreach (DataRow row in dt.Rows) {
                        DateTime date = (DateTime) row[0];
                        string dateStr = date.ToString("yyyyMMddHHmmss");
                        double price = (double) row[1];
                        double start = (double) row[2];
                        double high = (double) row[3];
                        double low = (double) row[4];
                        UInt64 volumn = UInt64.Parse(row[5].ToString());

                        CandleData priceData = new CandleData(dateStr, price, start, high, low, volumn);
                        //@@@ 저장할때 시간봉이 겹쳐서 임시 땜빵
                        // 2018-11-5일 이후 데이터는 괜찮을듯
                        if (date.Minute == 0) {
                            var count = stockData.priceDataCount();
                            if (count == 0) {
                                continue;
                                ;
                            }
                            var old = stockData.prevCandle();
                            if (UtilLibrary.Util.calcProfitRate(old.price_, price) > 10) {
                                price = old.price_;
                                start = old.startPrice_;
                                high = old.highPrice_;
                                low = old.lowPrice_;
                                volumn = old.volume_;
                                priceData = new CandleData(dateStr, price, start, high, low, volumn);
                            }
                        }
                        priceData.dbSaved_ = true;
                        stockData.updatePrice(priceData);
                    }
                    var bot = ControlGet.getInstance.stockBot();
                    stockData.updatePriceTable(bot);
                    pool.Add(code, stockData);
                }
            }
            catch (Exception e) {
                Logger.getInstance.print(KiwoomCode.Log.에러, "sqlite 타임머신 기능 실패 {0} 시간 부터 로딩이 안됨. {1} / {2}", startTime, e.Message, e.StackTrace);
                return false;
            }
            return true;
        }

    }
}
