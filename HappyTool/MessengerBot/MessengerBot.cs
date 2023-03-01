using HappyTool.DialogControl.StockDialog;
using HappyTool.Stock;
using StockLibrary;
using System;
using UtilLibrary;

//출처: http://deafjwhong.tistory.com/52?category=675699

//@happy_tool_bot 으로 텔레그램 통지기능 구현
namespace HappyTool.Messanger
{
    class StockTelegramBot :TelegramBot
    {
        public StockTelegramBot(string api, long id) : base(api, id)
        {

        }

        void sendHelp()
        {
            string help = string.Format("*** 호출 메뉴는 아래와 같습니다. ***\n");
            help += string.Format("확인 : 현재 상황 캡쳐해서 보기\n");
            help += string.Format("추천 : 지금 추천할 종목\n");
            help += string.Format("보유 : 지금 보유중인 것들\n");
            help += string.Format("/종료 : 오늘 여기까지\n");
            help += string.Format("강제매도 [코드] : 해당 코드를 강제 매도\n\n");
            help += string.Format("[현재 서버 시간 [{0}]]", DateTime.Now);
            this.sendMessage(help);
        }

        void getBuyedItemList()
        {
            StockBot bot = ControlGet.getInstance.stockBot();
            string log = "*** 코드 | 이름 | 현재 이익금 *** \n";
            double total = 0.0f;
            StockDataEach eachDo = (string code, StockData stockData) => {
                if (stockData.isBuyedItem() == false) {
                    return;
                }
                log += string.Format("{0} | {1} | {2:#,###0.##}\n",
                    stockData.code_, stockData.name_, stockData.nowProfit());
                total += stockData.nowProfit();
            };
            bot.doStocks(eachDo);
            log += string.Format("총 이익금 : {0:#,###0} 원", total);

            this.sendMessage(log);
        }

        void forcePayOff(string message)
        {
            StockBot bot = ControlGet.getInstance.stockBot();
            string[] token = message.Split(' ');
            if (token.Length < 1) {
                this.sendMessage("잘못된 입력입니다. 원 메시지[{0}]", message);
                return;
            }
            string code = token[1];
            StockData stockData = bot.getStockDataCode(code);
            if (stockData == null) {
                this.sendMessage("코드가 잘못 입력되었습니다. 코드 :[{0}]", code);
                return;
            }
            if (stockData.isBuyedItem() == false) {
                this.sendMessage("입력한 코드 :[{0}]는 지금 보유중이 아닙니다.", code);
                return;
            }
            bot.payOff(code, "강제 청산");
            this.sendMessage("{0}/{1} 강제 청산 접수했습니다.", stockData.name_, stockData.code_);
        }

        public override void doOrder(string message)
        {
            // eco 서버, 여기서 메시지 받았을때 switch 분기를 하면 됨.
            if (message.StartsWith("추천")) {
                this.sendMessage("오늘의 추천 주식");
                this.sendRecommandStocks();
            }
            else if (message.StartsWith("확인")) {
                string caputure = StockDlgInfo.getInstance.captureForm();
                this.sendPhoto(caputure, string.Format("{0} 시각 상황", DateTime.Now));
            }
            else if (message.StartsWith("보유")) {
                this.getBuyedItemList();
            }
            else if (message.StartsWith("/종료")) {
                Program.happyTool_.quit();
            }
            else if (message.StartsWith("강제매도")) {
                this.forcePayOff(message);
            }
            else {
                this.sendHelp();
            }
        }

        int sendCount_ = 0;
        public void sendRecommandStocks()
        {
            bool init = false;
            bool init2 = false;
            string log = "";
            StockBot stockBot = ControlGet.getInstance.stockBot();

            const int MAX_IDX = 10;
            int index = 0;
            StockDataEach eachDo = (string code, StockData stockData) => {
                if (index++ > MAX_IDX) {
                    return;
                }
                if (stockData.isBuyTime(stockBot)) {
                    if (init == false) {
                        init = true;
                        log = string.Format("* buy [{0}] 봉\n", stockBot.priceTypeMin());
                    }
                    string url = string.Format("https://stock.kakao.com/m/stocks/KOREA-A{0}", stockData.code_);
                    log += string.Format(" buy -> {0}:{1} 가격 {2:#,#}\n  [{3}]\n",
                        stockData.name_, stockData.code_, stockData.nowPrice(), url);
                }
            };
            stockBot.doStocks(eachDo);

            index = 0;
            eachDo = (string code, StockData stockData) => {
                if (index++ > MAX_IDX) {
                    return;
                }
                if (stockData.isSellTime(stockBot, stockData.nowPrice())) {
                    if (init2 == false) {
                        init2 = true;
                        log += string.Format("\n* sell [{0}] 봉\n", stockBot.priceTypeMin());
                    }
                    string url = string.Format("https://stock.kakao.com/m/stocks/KOREA-A{0}", stockData.code_);
                    log += string.Format(" sell -> {0}:{1} 가격 {2:#,#}\n  [{3}]\n", stockData.name_, stockData.code_, stockData.nowPrice(), url);
                }
            };
            stockBot.doStocks(eachDo);

            if (log.Length > 0) {
                this.sendMessage(log);
            } else {
                if ((sendCount_ % 10) == 0) {
                    this.sendMessage("현재 {0}분간 추천할 주식 없음", ControlGet.getInstance.stockBot().priceTypeMin());
                }
            }

            sendCount_++;
           
        }
    }
}