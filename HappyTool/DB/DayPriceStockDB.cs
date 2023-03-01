using HappyTool.Stock;
using StockLibrary;
using StockLibrary.DB;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilLibrary;

namespace HappyTool.DB
{
    class DayPriceStockDB: BaseSqliteDB
    {
        public DayPriceStockDB(string dbFile) : base(dbFile)
        {
        }

        protected override string tableName()
        {
            return "";
        }

        protected override void createTable()
        {
        }
        string baseTable_ = "DayPriceTable_";

        //---------------------------------------------------------------------------
        // 시뮬레이션 기능을 위한것들
        public void getCodeList(ref List<string> codePool)
        {

            string sql = string.Format("SELECT name FROM sqlite_master WHERE type = 'table' and name like '{0}%';", baseTable_);
            var dt = sqlDB_.readDataTableQuery(sql);
            if (dt.Rows.Count == 0) {
                return;
            }

            foreach (DataRow row in dt.Rows) {
                string tableName = (string) row[0];
                var code = tableName.Replace(baseTable_, "");
                codePool.Add(code);
            }
        }

        public bool selectForSimule(List<string> codePool, DateTime startTime, ref Dictionary<string, StockData> pool)
        {
            string sql = "";

            try {
                foreach (string code in codePool) {
                    sql = string.Format("SELECT REPLACE(candleTime,'.','-'), close, start, high, low, vol FROM {0}{1} WHERE candleTime > '{2}' order by candleTime desc;",
                        baseTable_, code, startTime.ToString("yyyy.MM.dd"));

                    var dt = sqlDB_.readDataTableQuery(sql);
                    if (dt.Rows.Count == 0) {
                        continue;
                    }

                    StockData stockData = new StockData(code, code);
                    foreach (DataRow row in dt.Rows) {
                        string orgDate = row[0] + " 15:30:00";
                        DateTime date = DateTime.Parse(orgDate);
                        string dateStr = date.ToString("yyyyMMddHHmmss");
                        double price = (int) row[1];
                        double start = (int) row[2];
                        double high = (int) row[3];
                        double low = (int) row[4];
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
                Logger.getInstance.print(KiwoomCode.Log.에러, "{0} 1일 데이터 로딩 실패 {1} 시간 부터 로딩이 안됨. {2} / {3}", sql, startTime, e.Message, e.StackTrace);
                return false;
            }
            return true;
        }

    }
}
