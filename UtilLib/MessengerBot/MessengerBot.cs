using System;
using System.Collections.Generic;
using System.Threading;
using Telegram.Bot.Types.Enums;

//출처: http://deafjwhong.tistory.com/52?category=675699

//@happy_tool_bot 으로 텔레그램 통지기능 구현
namespace UtilLibrary
{
    class TelegramBot :SingleTon<TelegramBot> {
        const long CHAT_BOT_ID = 508897948;
        const string CHAT_BOT_API = "511370550:AAGdKo6WPjecx2pPVSSgdxdT22bYVHpXTJE";
        private Telegram.Bot.TelegramBotClient bot_ = new Telegram.Bot.TelegramBotClient(CHAT_BOT_API);

        public TelegramBot()
        {
            setTelegramEvent();
            telegramAPIAsync();
        }

        private async void telegramAPIAsync()
        {
            var me = await bot_.GetMeAsync();
            Logger.getInstance.print(KiwoomCode.Log.주식봇, "내 이름은 {0}", me.FirstName);
        }

        private void setTelegramEvent()
        {
            bot_.OnMessage += botOnMessage;
            bot_.StartReceiving();
            sendPoolThread_.Start();
        }

        private async void botOnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            var msg = e.Message;
            if (msg == null || msg.Type != MessageType.Text) {
                return;
            }

            this.doOrder(msg.Text);
            //this.sendTextMessage(msg.Text);
            //Logger.getInstance.print(KiwoomCode.Log.주식봇, "받은 메시지 : {0}", msg.Text);
        }
        Thread sendPoolThread_ = new Thread(run);
        List<string> messagePool_ = new List<string>();
        object lockObject_ = new object ();

        public void sendMessage(string message)
        {
            bool lockTaken = false;
            Monitor.Enter(lockObject_, ref lockTaken);
            try {
                messagePool_.Add(message);
            } finally {
                if (lockTaken) {
                    Monitor.Exit(lockObject_);
                }
            }
        }

        public string getMessage()
        {
            string message = "";
            bool lockTaken = false;
            Monitor.Enter(lockObject_, ref lockTaken);
            try {
                if (messagePool_.Count > 0) {
                    if (messagePool_[0] != null) {
                        message = messagePool_[0];
                        messagePool_.RemoveAt(0);
                    }
                }
            } finally {
                if (lockTaken) {
                    Monitor.Exit(lockObject_);
                }
            }
            return message;
        }

        // 실제 메시지 보내는곳
        public async void sendTextMessage(string message)
        {
            try {
                await bot_.SendTextMessageAsync(CHAT_BOT_ID, message);
            } catch (Exception e) {
                string log = string.Format("텔레그램 에러 : {0}\n, -> trace\n{1}\n=== origin message ===\n{2}", e.Message, e.StackTrace, message);
                MailOffice.getInstance.sendMessage(log);
            }
        }

        private void doOrder(string message)
        {
            // eco 서버, 여기서 메시지 받았을때 switch 분기를 하면 됨.
            if (message.StartsWith("/주식추천")) {
                this.sendMessage("오늘의 추천 주식");
                this.sendRecommandStocks();
            }

            if (message.StartsWith("/주식추가")) {
                string[] token = message.Split(' ');
                this.sendMessage("주식 추가 : " + token[1]);
            }
        }

        public void sendRecommandStocks(PRICE_TYPE priceType = PRICE_TYPE.MIN15)
        {
            bool init = false;
            bool init2 = false;
            string log = "";
            StockBot stockBot = StockBot.getInstance;
            StockDataEach eachDo = (int code, StockData stockData) => {
                if (stockData.isBuyTime(priceType)) {
                    if (init == false) {
                        init = true;
                    log = string.Format("* buy [{0}] 봉, [{1}]매매기법 기준.\n", priceType, StockBot.getInstance.buyStrategyModule().getName());
                    }
                    string url = string.Format("https://stock.kakao.com/m/stocks/KOREA-A{0}", stockData.codeString());
                    log += string.Format(" buy -> {0}:{1} 가격 {2:#,#}\n  [{3}]\n",
                        stockData.name_, stockData.codeString(), stockData.nowPrice(priceType), url);
                    return;
                }
            };
            stockBot.doStocks(eachDo);

            eachDo = (int code, StockData stockData) => {
                if (stockData.isSellTime(priceType, stockData.nowPrice(priceType))) {
                    if (init2 == false) {
                        init2 = true;
                        log = string.Format("\n* sell [{0}] 봉 [{1}] 매도기법 기준.\n", priceType, StockBot.getInstance.sellStrategyModule().getName());
                    }
                    string url = string.Format("https://stock.kakao.com/m/stocks/KOREA-A{0}", stockData.codeString());
                    log += string.Format(" sell -> {0}:{1} 가격 {2:#,#}\n  [{3}]\n", stockData.name_, stockData.codeString(), stockData.nowPrice(priceType), url);
                    return;
                }
            };
            stockBot.doStocks(eachDo);

            if (init || init2) {
                this.sendMessage(log);
            } else {
                this.sendMessage(string.Format("{0} 봉 기준으로는 지금 추천할 주식이 없어요 ㅠㅠ", priceType));
            }
        }

        static async void run()
        {
            TelegramBot messenger = TelegramBot.getInstance;
            while(true) {
                string msg = messenger.getMessage();
                if (msg.Length > 0) {
                    messenger.sendTextMessage(msg);
                }
                Thread.Sleep(3000);
            }
        }
    }
}