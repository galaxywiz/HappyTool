using NetLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilLibrary;

namespace StockLibrary.StrategyManager.StrategyModuler
{
    public class ML_StrategyModule :StrategyModule
    {
        const int TERM = 2;
        PythonClient client_;

        // 파이썬과 연락할 클라이언트
        //protected Client_ ;
        public ML_StrategyModule(Bot bot) : base(bot)
        {
            //Client = new Client(ip)
            client_ = new PythonClient(PublicVar.aiIp_, PublicVar.aiPort_);
            this.haveNetConnect_ = true;           
        }

        ~ML_StrategyModule()
        {
            client_ = null;
        }

        //----------------------------------------------------------------------//

        string makePacket(StockData stockData)
        {
            var candle = stockData.nowCandle();
            if (candle == null) {
                return "NONE";
            }
            var date = stockData.nowDateTime();
            var dateStr = date.ToString("yyyyMMddHHmm");
            var close = candle.price_;
            var start = candle.startPrice_;
            var high = candle.highPrice_;
            var low = candle.lowPrice_;
            var vol = candle.volume_;

            var accountRate = bot_.accountRate();
            var backTest = this.bot_.test_ ? 1 : 0;
            var buyPosition = "HOLD";
            switch (stockData.position_) {
                case TRADING_STATUS.매수:
                buyPosition = "BUY";
                break;
                case TRADING_STATUS.매도:
                buyPosition = "SELL";
                break;
            }
                                     // head,co,min,date,close,     start,  high,      low,     vol,cnt,pos,accountrate,backtest
            var packet = string.Format("{0},{1},{2},{3},{4:##0.#####},{5:##0.#####},{6:##0.#####},{7:##0.#####},{8},{9},{10},{11:##0.#####},{12}",
                "TRADE_REQUEST",            //0
                stockData.regularCode(),    //1
                bot_.priceTypeMin(),        //2
                dateStr,                    //3
                close,                      //4
                start,                      //5
                high,                       //6
                low,                        //7
                vol,                        //8
                stockData.buyCount_,        //9
                buyPosition,                //10
                accountRate,                //11
                backTest);                  //12
            return packet;
        }

        string retPacket_;
        string[] retList_ = null;

        protected void requestAI(StockData stockData)
        {
            var packet = this.makePacket(stockData);
            var backTest = this.bot_.test_ ? 1 : 0;
            if (backTest == 0) {
                // 실제 파이썬이 떠있는지 확인한다.
                // 없을시, 강제로 실행
            }

            retPacket_ = client_.requestPacket(packet);
            retList_ = retPacket_.Split(',');
        }

        //----------------------------------------------------------------------//
        protected override bool buy(StockData stockData, int timeIdx = 1)
        {
            if (timeIdx == -1) {
                timeIdx = 캔들완성_IDX;
            }

            var priceTable = this.priceTable(stockData, TERM);
            if (priceTable == null) {
                return false;
            }

            stockData.aiPredicCount_ = 0;
            requestAI(stockData);

            switch (retList_[0]) {
                case "NONE":
                case "HOLD":
                case "SELL":
                return false;

                case "BUY":
                stockData.aiPredicCount_ = Int32.Parse(retList_[1]);
                return true;
            }
            return false;
        }

        protected override bool sell(StockData stockData, int timeIdx = 1)
        {
            if (timeIdx == -1) {
                timeIdx = 캔들완성_IDX;
            }

            var priceTable = this.priceTable(stockData, TERM);
            if (priceTable == null) {
                return false;
            }

            if (client_.ableRequest() == false) {
                return false;
            }

            stockData.aiPredicCount_ = 0;
            requestAI(stockData);

            switch (retList_[0]) {
                case "NONE":
                case "HOLD":
                case "BUY":
                return false;

                case "SELL":
                stockData.aiPredicCount_ = Int32.Parse(retList_[1]);
                return true;
            }
            return false;
        }
    }
}
