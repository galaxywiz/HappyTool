using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using UtilLibrary.Data;

namespace HappyFuture.Officer
{
    // 컬럼 

    class LogForGoogleSheet
    {
        protected GoogleSheet googleSheet_ = null;
        public bool active_ = false;
        protected Thread thread_;
        readonly Queue<IList<IList<Object>>> pool_ = new Queue<IList<IList<object>>>();

        public LogForGoogleSheet(string sheetId, string sheetName)
        {
            string jsonPath = "config/client_secret_com.json";
            System.IO.FileInfo fi = new System.IO.FileInfo(jsonPath);
            if (fi.Exists) {
                this.active_ = true;
            }
            else {
                MessageBox.Show("구글시트 인증키가 없습니다.");
            }

            string appName = "HappyFuture";
            this.googleSheet_ = new GoogleSheet(jsonPath, appName, sheetId, sheetName);

            this.thread_ = new Thread(() => this.run());
            this.thread_.Start();
        }

        readonly object lock_ = new object();
        protected void run()
        {
            while (this.active_) {
                Thread.Sleep(1000 * 3);
                lock (this.lock_) {
                    if (this.pool_.Count == 0) {
                        continue;
                    }
                    var recodes = this.pool_.Dequeue();
                    if (this.googleSheet_.update(recodes) == false) {
                        this.addLog(recodes);
                    }
                }
            }
        }

        protected void addLog(IList<IList<Object>> recodes)
        {
            lock (this.lock_) {
                this.pool_.Enqueue(recodes);
            }
        }
    }

    class TradeLogForGoogleSheet: LogForGoogleSheet
    {
        public TradeLogForGoogleSheet(string sheetId, string sheetName) : base(sheetId, sheetName)
        {

        }

        public void addTradeLog(FutureData futureData, string why)
        {
            var recodes = this.genRecode(futureData, why);
            this.addLog(recodes);
        }

        IList<IList<Object>> genRecode(FutureData futureData, string why)
        {
            List<IList<Object>> objNewRecords = new List<IList<Object>>();
            IList<Object> obj = new List<Object>();

            //https://docs.google.com/spreadsheets/d/1VOCbWUPhCAab5yDsceBusgR2FdIHx32SohS1vJREXFA/edit#gid=568220719
            //여기 순서대로
            obj.Add(futureData.nowDateTime());                          //매매시간
            obj.Add(futureData.name_);                                  //이름
            obj.Add(futureData.code_);                                  //코드
            obj.Add(futureData.position_.ToString());                   //포지션
            obj.Add(futureData.buyPrice_);                              //매수가격
            obj.Add(futureData.nowPrice());                             //매도가격
            obj.Add(futureData.nowPrice() - futureData.buyPrice_);      //tick차이
            obj.Add(futureData.buyCount_);                              //매수갯수
            obj.Add(futureData.positionHaveMin());                      //보유시간
            obj.Add(futureData.nowOneProfit());                         //1주당이익
            obj.Add(futureData.nowProfit());                            //총이익
            obj.Add(why);                                               //청산이유
            obj.Add(futureData.payOffCode_);
            obj.Add(futureData.minOneProfit_);                             //보유중최저가
            obj.Add(futureData.maxProfit_);                             //보유중최대가
            var trade = futureData.tradeModule();
            if (trade != null) {
                obj.Add(trade.avgProfit_);                              //기대이익
                obj.Add(trade.expectedWinRate_);                        //예상승률
                obj.Add(futureData.logExpectedRange());                 //예측가격범위
                obj.Add(trade.buyTradeModule_.getName());               //매수모듈
                obj.Add(trade.sellTradeModule_.getName());              //매도도듈
            }

            objNewRecords.Add(obj);
            return objNewRecords;
        }
    }

    class TestBotLogForGoogleSheet: LogForGoogleSheet
    {
        public TestBotLogForGoogleSheet(string sheetId, string sheetName) : base(sheetId, sheetName)
        {
        }

        public void addLog(IList<Object> obj)
        {
            List<IList<Object>> objNewRecords = new List<IList<Object>>();
            objNewRecords.Add(obj);
            this.addLog(objNewRecords);
        }
    }
}
