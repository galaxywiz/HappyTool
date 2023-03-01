using HappyFuture.DialogControl.FutureDlg;
using StockLibrary;
using System;
using UtilLibrary;

namespace HappyFuture.Officer
{
    class FutureTelegramBot: TelegramBot
    {
        public FutureTelegramBot(string api, long id) : base(api, id)
        {

        }

        void sendHelp()
        {
            string help = string.Format("*** 호출 메뉴는 아래와 같습니다. ***\n");
            help += string.Format("확인 /상황 : 현재 상황 캡쳐해서 보기\n");
            help += string.Format("수익 : 금일 HTS 수익 조회\n");
            help += string.Format("추천 : 지금 추천할 종목\n");
            help += string.Format("보유 : 지금 보유중인 것들\n");
            help += string.Format("추세 / 추이 : 1달간 수익율, 예수금 추이\n");            
            help += string.Format("강제매도 [코드] : 해당 코드를 강제 매도\n\n");

            FutureBot bot = ControlGet.getInstance.futureBot();
            help += string.Format("[금일 총 이익금 : {0:#,###0.#####} $]\n", bot.nowTotalProfit_);
            help += string.Format("[금일 전일대비 이익율 : {0:#,###0.#####} %]\n", bot.todayTotalProfitRate_);
            help += bot.logForTradeWinRate();
            help += string.Format("\n[서버 시간 [{0}]]", DateTime.Now);
            this.sendMessage(help);
        }

        void getBuyedItemList()
        {
            FutureBot bot = ControlGet.getInstance.futureBot();
            string log = "*** 코드 | 이름 | 포지션 | 1계약당 이익금($) | 계약수 *** \n";
            double total = 0.0f;
            StockDataEach eachDo = (string code, StockData stockData) => {
                if (stockData.isBuyedItem() == false) {
                    return;
                }
                FutureData futureData = stockData as FutureData;
                log += string.Format("{0} | {1} | {2} | {3:#,###0.#####} | {4}\n",
                    futureData.code_, futureData.name_, futureData.position_, futureData.nowOneProfit(), futureData.buyCount_);
                total += (futureData.nowProfit());
            };
            bot.doStocks(eachDo);
            log += string.Format("총 이익금 : {0:#,###0.#####} $\n", total);
            log += string.Format("금일 총 이익금 : {0:#,###0.#####} $\n", bot.nowTotalProfit_);
            log += string.Format("금일 총 이익율 : {0:#,###0.#####} %\n", bot.todayTotalProfitRate_);
            log += bot.logForTradeWinRate();
            this.sendMessage(log);
        }

        void forcePayOff(string message)
        {
            FutureBot bot = ControlGet.getInstance.futureBot();
            string[] token = message.Split(' ');
            if (token.Length < 1) {
                this.sendMessage("잘못된 입력입니다. 원 메시지[{0}]", message);
                return;
            }
            string code = token[1];
            FutureData futureData = bot.getStockDataCode(code) as FutureData;
            if (futureData == null) {
                this.sendMessage("코드가 잘못 입력되었습니다. 코드 :[{0}]", code);
                return;
            }
            if (futureData.isBuyedItem() == false) {
                this.sendMessage("입력한 코드 :[{0}]는 지금 보유중인 포지션이 아닙니다.", code);
                return;
            }
            bot.payOff(code, "텔레그램 강제 청산");
            this.sendMessage("{0}/{1} 강제 청산 접수했습니다.", futureData.name_, futureData.code_);
        }

        void send1MonthHistory()
        {
            var info = FutureDlgInfo.getInstance;
            info.tradeHistoryView_.update();
        }

        public override void doOrder(string message)
        {
            FutureDlg dlg = Program.happyFuture_.futureDlg_;
            var checkBox = dlg.checkBox_doTrade;
            if (checkBox.Checked == false) {
                return;
            }

            FutureBot bot = ControlGet.getInstance.futureBot();
            // eco 서버, 여기서 메시지 받았을때 switch 분기를 하면 됨.
            if (message.StartsWith("추천")) {
                if (this.sendRecommandFutures() == false) {
                    this.sendMessage("현재 {0} 시각기준 추천할 것이 없습니다", DateTime.Now);
                }
                return;
            }
            if (message.StartsWith("확인") || message.StartsWith("상황")) {
                string imageFile = FutureDlgInfo.getInstance.captureFormImgName();
                this.sendPhoto(imageFile, string.Format("{0} 시각 상황", DateTime.Now));
                return;
            }
            if (message.StartsWith("수익")) {
                bot.reportToday();
                return;
            }
            if (message.StartsWith("보유")) {
                this.getBuyedItemList();
                return;
            }
            if (message.StartsWith("추세") || message.StartsWith("추이")) {
                this.send1MonthHistory();
                return;
            }        
            if (message.StartsWith("강제매도")) {
                this.forcePayOff(message);
                return;
            }

            this.sendHelp();
        }

        public bool sendRecommandFutures()
        {
            bool init = false;
            bool init2 = false;
            string log = string.Format("현재 시각: {0} / UTC: {1} 기준\n", DateTime.Now, DateTime.UtcNow);
            FutureBot bot = ControlGet.getInstance.futureBot();
            StockDataEach eachDo = (string code, StockData stockData) => {
                if (stockData.isBuyTime(bot)) {
                    if (init == false) {
                        init = true;
                        log = string.Format("* 매수 추천 [{0}] 봉\n", bot.priceTypeMin());
                    }
                    log += string.Format(" 매수 -> {0} 가격 {1:#,###.####}\n",
                        stockData.name_, stockData.nowPrice());

                    FutureData futureData = (FutureData) stockData;
                    log += string.Format("{0}\n", futureData.exchangeUrl());
                }
            };
            bot.doStocks(eachDo);

            eachDo = (string code, StockData stockData) => {
                if (stockData.isSellTime(bot, stockData.nowPrice())) {
                    if (init2 == false) {
                        init2 = true;
                        log = string.Format("\n* 매도 추천 [{0}] 봉\n", bot.priceTypeMin());
                    }
                    log += string.Format(" 매도 -> {0} 가격 {1:#,###.####}\n",
                        stockData.name_, stockData.nowPrice());

                    FutureData futureData = (FutureData) stockData;
                    log += string.Format("{0}\n", futureData.exchangeUrl());
                }
            };
            bot.doStocks(eachDo);

            if (init || init2) {
                this.sendMessage(log);
                return true;
            }
            return false;
        }
        
        public void greatingMessage(FutureBot bot)
        {
            bool isTest = bot.engine_.isTestServer();
            string log = string.Format("### 행복의 선물 [{0}] 시작 ###\n", isTest ? "모의투자" : "실거래");
            log += string.Format(" - 현재 시각: {0}\n", DateTime.Now);
            log += string.Format(" - 사용 분봉: {0} 분\n", bot.priceTypeMin());
            var fund = bot.fundManagement_;
            log += string.Format(" - 메인 전략: {0}\n", fund.name());
            log += string.Format(" - 진입 전략: {0}\n", fund.strategyModule_.name());
            this.sendMessage(log);
        }
    }
}
