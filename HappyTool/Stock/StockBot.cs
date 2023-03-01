using System;
using System.Collections.Generic;
using System.Threading;
using System.Collections;
using HappyTool.Util;
using HappyTool.Stock.Calculate;
using HappyTool.Stock.TradeModules;
using HappyTool.Messanger;
using HappyTool.DlgControl;

namespace HappyTool.Stock
{
    //---------------------------------------------------------------------
    // 주식 봇 
    class StockBot :SingleTon<StockBot>
    {
        StockBotState botState_ = new StockBotState();
        public int accountMoney_ { get; set; }
        public PRICE_TYPE priceType_ = PublicVar.initType;

        public void start()
        {
            botState_.start();
            MessengerBot bot = MessengerBot.getInstance;
        }

        public void process()
        {
            Thread.Sleep(1);
            botState_.process();
        }

        // 외부 (체결등이 왔을때 강제로 계좌 정보를 다시 읽도록 한다)
        public void reloadAccountInfo()
        {
            StockEngine.getInstance.addOrder(new AccountStockStatement());
            StockEngine.getInstance.addOrder(new AccountMoneyStatement());
        }

        //---------------------------------------------------------------------
        // 주식 봇이 갖고 있는 주식들에 대한처리
        private object lockObject_;
        Dictionary<int, StockData> stockPool_;
        TradeModule tradeModule_ = null;

        // 시간 갱신에 대해서
        enum UPDATE_TYPE {
            MIN15,
            HOUR1,
            HOUR4,
            MAX
        }
        DateTime[] nextUpdateTime_ = new DateTime[(int) UPDATE_TYPE.MAX];       // 픽 보내는 시간

        //백테스팅 시뮬결과 용
        public BackTestHistory[] backTestHistory_ = new BackTestHistory[(int) PRICE_TYPE.MAX];

        StockBot()
        {
            lockObject_ = new object();
            stockPool_ = new Dictionary<int, StockData>();
            accountMoney_ = 0;
            this.setTradeModule(new BollengerCCITradeModule());

            for (int i = 0; i < (int) UPDATE_TYPE.MAX; ++i) {
                nextUpdateTime_[i] = new DateTime(2018, 06, 19, 0, 0, 0);
            }

            for (int i = 0; i < (int) PRICE_TYPE.MAX; ++i) {
                backTestHistory_[i] = new BackTestHistory();
            }
        }

        // 주식 추가
        public void addStockData(StockData stockData)
        {
            int code = stockData.code_;

            lock (lockObject_) {
                StockData oldData = this.stockData(code);
                if (oldData != null) {
                    stockPool_.Remove(code);
                }
                stockPool_.Add(code, stockData);
            }
        }

        // 주식 가지고 오기
        public StockData stockData(int code)
        {
            lock (lockObject_) {
                StockData stockData = null;
                if (stockPool_.TryGetValue(code, out stockData)) {
                    return stockData;
                }
                return null;
            }
        }

        private int countBuyedStock()
        {
            int count = 0;
            lock (lockObject_) {
                foreach (KeyValuePair<int, StockData> keyValue in stockPool_) {
                    int code = keyValue.Key;
                    StockData stockData = keyValue.Value;

                    if (stockData.isBuyedStock()) {
                        count++;
                    }
                    if (stockData.alreadyOrder_) {
                        count++;
                    }
                }
            }
            return count;
        }

        internal void loadStockHistory(object priceType)
        {
            throw new NotImplementedException();
        }

        //--------------------------------------------------------
        // 사고 파는 처리
        const int MAX_BUYED_STOCK_COUNT = 5;
        const int NONE_COUNT = 0;
        private bool calculateBuyCount(StockData stockData, out int tradingCount, out int tradingPrice)
        {
            tradingCount = NONE_COUNT;
            tradingPrice = NONE_COUNT;

            int count = MAX_BUYED_STOCK_COUNT - this.countBuyedStock();
            if (count <= 0) {
                return false;
            }

            int nowPrice = stockData.nowPrice(priceType_);
            if (nowPrice == 0) {
                return false;
            }
            int buyable = accountMoney_ / count;

            if (buyable < nowPrice) {
                return false;
            }
            tradingCount = buyable / nowPrice;
            tradingCount -= 5;          // 시장가 구입이므로 넉넉하게 설정한다
            if (tradingCount < 0) {
                return false;
            }
            tradingPrice = nowPrice;

            return true;
        }

        private void addAccountMoney(int money)
        {
            int accountMoney = StockBot.getInstance.accountMoney_;
            accountMoney = accountMoney + money;

            StockBot.getInstance.accountMoney_ = accountMoney;
            Program.happyTool_.stockDlg_.updateAccountMoney(accountMoney);
        }

        private void subAccountMoney(int money)
        {
            this.addAccountMoney(-money);
        }

        public void buyStock()
        {
            if (accountMoney_ < 0) {
                return;
            }
            lock (lockObject_) {
                foreach (KeyValuePair<int, StockData> keyValue in stockPool_) {
                    int code = keyValue.Key;
                    StockData stockData = keyValue.Value;
                    if (stockData.alreadyOrder_) {
                        continue;
                    }
                    if (stockData.isBuyedStock()) {
                        continue;
                    }
                    if (stockData.isBuyTime(priceType_) == false) {
                        continue;
                    }

                    int tradingCount, tradingPrice;
                    if (this.calculateBuyCount(stockData, out tradingCount, out tradingPrice) == false) {
                        continue;
                    }

                    // 실제 구입주문 넣자.
                    int totalMoney = tradingCount * tradingPrice;
                    this.subAccountMoney(totalMoney);
                    StockEngine.getInstance.addOrder(new BuyStock(code, tradingCount, tradingPrice));
                    Logger.getInstance.print(KiwoomCode.Log.주식봇, "{0}:{1} 주식 주문", stockData.name_, stockData.codeString());

                    stockData.alreadyOrder_ = true;
                    this.sendBuyStock(stockData, tradingCount, tradingPrice);
                }
            }
        }

        public void sellStock()
        {
            lock (lockObject_) {
                foreach (KeyValuePair<int, StockData> keyValue in stockPool_) {
                    int code = keyValue.Key;
                    StockData stockData = keyValue.Value;

                    if (stockData.alreadyOrder_) {
                        continue;
                    }

                    if (stockData.isBuyedStock() == false) {
                        continue;
                    }
                    BuyedStockData buyedStockData = (BuyedStockData) stockData;

                    if (buyedStockData.alreadyOrder_) {
                        continue;
                    }

                    if (buyedStockData.isSellTime(priceType_) == false) {
                        continue;
                    }

                    // 실제 판매 주문 넣기
                    StockEngine.getInstance.addOrder(new SellStock(buyedStockData));
                    int price = buyedStockData.nowPrice(priceType_);
                    int tradingCount = buyedStockData.buyCount_;
                    int totalMoney = price * tradingCount;
                    this.addAccountMoney(totalMoney);

                    Logger.getInstance.print(KiwoomCode.Log.주식봇, "{0}:{1} 주식 판매", buyedStockData.name_, buyedStockData.codeString());
                    buyedStockData.alreadyOrder_ = true;

                    this.sendSellStock(buyedStockData);
                }
            }
        }

        public void requestStockData(PRICE_TYPE type, int code)
        {
            StockEngine engine = StockEngine.getInstance;
            switch (type) {
                case PRICE_TYPE.MIN15:
                engine.addOrder(new HistoryThe15MinStock(code));
                break;
                case PRICE_TYPE.HOUR1:
                engine.addOrder(new HistoryThe1HourStock(code));
                break;
                case PRICE_TYPE.HOUR4:
                engine.addOrder(new HistoryThe4HourStock(code));
                break;
                case PRICE_TYPE.DAY:
                engine.addOrder(new HistoryTheDaysStock(code));
                break;
                case PRICE_TYPE.WEEK:
                engine.addOrder(new HistoryTheWeeksStock(code));
                break;
            }
        }
        //--------------------------------------------------------
        // 주식 데이터를 리로딩할 준비를 한다.
        public void requestUpdateStockPriceData(PRICE_TYPE priceType)
        {
            lock (lockObject_) {
                foreach (KeyValuePair<int, StockData> keyValue in stockPool_) {
                    StockData stockData = keyValue.Value;
                    stockData.clearPrice(priceType);
                }
            }
        }
        // 주식풀의 모든 주식의 재갱신을 요구한다
        public void loadStocksPriceData()
        {
            lock (lockObject_) {
                foreach (KeyValuePair<int, StockData> keyValue in stockPool_) {
                    StockData stockData = keyValue.Value;
                    stockData.loadPriceData();
                }
            }
        }

        public bool checkFullLoadedStock(PRICE_TYPE priceType)
        {
            bool fullLoaded = true;
            lock (lockObject_) {
                foreach (KeyValuePair<int, StockData> keyValue in stockPool_) {
                    StockData stockData = keyValue.Value;
                    if (stockData.priceDataCount(priceType) == 0) {
                        fullLoaded = false;
                        break;
                    }
                }
            }

            if (fullLoaded) {
                string log = this.resultAllStockBackTest(priceType);
                MessengerBot.getInstance.sendMessage(log);
                BackTestInfo.getInstance.update(log);

                this.getNowRecommandStock(priceType);
                this.drawStockView();
            }

            return fullLoaded;
        }        

        PRICE_TYPE checkStockUpdateTime()
        {
            DateTime now = DateTime.Now;
            if (now.Hour < 9 || now.Hour > 16) {
                //return PRICE_TYPE.DAY;
            }
            //@@@
            if ((now.Minute % 15) == 0) {
                if (nextUpdateTime_[(int) UPDATE_TYPE.MIN15] < now) {
                    nextUpdateTime_[(int) UPDATE_TYPE.MIN15] = now.AddMinutes(15);
                    return PRICE_TYPE.MIN15;
                }
            }

            if (now.Minute == 0) {
                if (nextUpdateTime_[(int) UPDATE_TYPE.HOUR1] < now) {
                    nextUpdateTime_[(int) UPDATE_TYPE.HOUR1] = now.AddHours(1);
                    return PRICE_TYPE.HOUR1;
                }
            }

            if (now.Minute == 0 && (now.Hour == 9 || now.Hour == 13 || now.Hour == 15)) {
                if (nextUpdateTime_[(int) UPDATE_TYPE.HOUR4] < now) {
                    nextUpdateTime_[(int) UPDATE_TYPE.HOUR4] = now.AddHours(3);
                    return PRICE_TYPE.HOUR4;
                }
            }
            // 일단위 이상은 굳이 로딩할 필요 없음.
            return PRICE_TYPE.MAX;
        }

        // 현재 살만한 주식이 있는지 pick을 준다.
        PRICE_TYPE loadingType_ = PRICE_TYPE.MAX;
        public void watchingForPick()
        {
            // 주식 로딩처리
            this.loadStocksPriceData();
            if (loadingType_ != PRICE_TYPE.MAX) {
                // 모두 재갱신 됬는지 확인한다.
                if (this.checkFullLoadedStock(loadingType_)) {
                    loadingType_ = PRICE_TYPE.MAX;
                }
                return;
            }

            // 재갱신할 타임인가?
            loadingType_ = this.checkStockUpdateTime();
            if (loadingType_ != PRICE_TYPE.MAX) {
                requestUpdateStockPriceData(loadingType_);
                return;
            }
            
            // 분봉 데이터 업데이트가 되었다면, 주식 사고 파는 로직 넣는다.
            //   this.buyStock();
            //   this.sellStock();

            return;
        }

        public void getNowRecommandStock(PRICE_TYPE priceType = PRICE_TYPE.MIN15)
        {
            bool init = false;
            bool init2 = false;
            string log = "";

            lock (lockObject_) {
                foreach (KeyValuePair<int, StockData> keyValue in stockPool_) {
                    StockData stockData = keyValue.Value;
                    if (stockData.isBuyTime(priceType)) {
                        if (init == false) {
                            init = true;
                            log = string.Format("* buy [{0}] 봉 기준.\n", priceType);
                        }
                        string url = string.Format("https://stock.kakao.com/m/stocks/KOREA-A{0}", stockData.codeString());
                        log += string.Format("-> {0}:{1} 가격 {2:#,#}\n  [{3}]\n",
                            stockData.name_, stockData.codeString(), stockData.nowPrice(priceType), url);
                        continue;
                    }
                }

                foreach (KeyValuePair<int, StockData> keyValue in stockPool_) {
                    StockData stockData = keyValue.Value;
                    if (stockData.isSellTime(priceType)) {
                        if (init2 == false) {
                            init2 = true;
                            log = string.Format("\n* sell [{0}] 봉 기준.\n", priceType);
                        }
                        string url = string.Format("https://stock.kakao.com/m/stocks/KOREA-A{0}", stockData.codeString());
                        log += string.Format(">$ {0}:{1} 가격 {2:#,#}\n  [{3}]\n", stockData.name_, stockData.codeString(), stockData.nowPrice(priceType_), url);
                        continue;
                    }
                }
            }
            if (init || init2) {
                MessengerBot.getInstance.sendMessage(log);
            } else {
                MessengerBot.getInstance.sendMessage(string.Format("{0} 봉 기준으로는 지금 추천할 주식이 없어요 ㅠㅠ", priceType));
            }
        }

        // 일데이터 이외의 다른 데이터들도 미리 로딩한다.
        private void getAllStockData()
        {
            lock (lockObject_) {
                foreach (KeyValuePair<int, StockData> keyValue in stockPool_) {
                    int code = keyValue.Key;
                    StockEngine.getInstance.addOrder(new HistoryThe15MinStock(code));
                    StockEngine.getInstance.addOrder(new HistoryThe1HourStock(code));
                    StockEngine.getInstance.addOrder(new HistoryThe4HourStock(code));
                    StockEngine.getInstance.addOrder(new HistoryTheWeeksStock(code));
                }
            }
        }
        //--------------------------------------------------------
        // 메일 보내기
        string mailTag_ = "[주식봇] ";
        private void sendWatchingStock()
        {
            Mailer mail = new Mailer();

            mail.setToMailAddr(PublicVar.notifyMail);
            string title = mailTag_ + DateTime.Now.ToString("yyyy년 MM월 dd일") + "감시 리스트";
            mail.setSubject(title);

            string body = string.Format("{0,-16} {1,-10:D6} {2,-10} {3,-10} {4,-10} x {5,-10} = {6,-10}\n\r",
                "주식명", "주식코드", "현재가", "구입여부", "구입갯수", "구입가격", "총 소비비용");
            body += "=======================================================================\n\r";
            lock (lockObject_) {
                foreach (KeyValuePair<int, StockData> keyValue in stockPool_) {
                    int code = keyValue.Key;
                    StockData stockData = keyValue.Value;

                    if (stockData.isBuyedStock()) {
                        BuyedStockData buyedStockData = (BuyedStockData) stockData;
                        body += string.Format("{0,-16} {1,-10:D6} {2,-10} {3,-10} {4,-10} x {5,-10} = {6,-10}\n\r",
                            buyedStockData.name_, buyedStockData.code_, buyedStockData.nowPrice(priceType_), "YES", buyedStockData.buyCount_, buyedStockData.buyPrice_, buyedStockData.totalBuyPrice());
                    } else {
                        body += string.Format("{0,-16} {1,-10:D6} {2,-10} {3,-10}\n\r",
                            stockData.name_, stockData.code_, stockData.nowPrice(priceType_), "NO");
                    }
                }
            }
            mail.setBody(body);
            mail.send();
        }

        private void sendBuyStock(StockData stockData, int buyCount, int buyPrice)
        {
            Mailer mail = new Mailer();

            mail.setToMailAddr(PublicVar.notifyMail);
            string title = mailTag_ + DateTime.Now.ToString("yyyy년 MM월 dd일 - HH:mm:ss.f") + "주식 매수";
            mail.setSubject(title);

            string body = string.Format("{0,-16} {1,-10:D6} {2,-10} {3,-10} {4,-10} x {5,-10} = {6,-10}\n\r",
                "주식명", "주식코드", "현재가", "구입여부", "구입갯수", "구입가격", "총 소비비용");
            body += "=======================================================================\n\r";
            body += string.Format("{0,-16} {1,-10:D6} {2,-10} {3,-10} {4,-10} x {5,-10} = {6,-10}\n\r",
                stockData.name_, stockData.code_, stockData.nowPrice(priceType_), "YES", buyCount, buyPrice, buyCount * buyPrice);

            mail.setBody(body);
            mail.send();
        }

        private void sendSellStock(BuyedStockData buyedStockData)
        {
            Mailer mail = new Mailer();

            mail.setToMailAddr(PublicVar.notifyMail);
            string title = mailTag_ + DateTime.Now.ToString("yyyy년 MM월 dd일 - HH:mm:ss.f") + "주식 매도";
            mail.setSubject(title);

            string body = string.Format("{0,-16} {1,-10:D6} {2,-10} {3,-10} {4,-10} x {5,-10} = {6,-10}\n\r",
                "주식명", "주식코드", "현재가", "구입여부", "구입갯수", "구입가격", "총 소비비용");
            body += "=======================================================================\n\r";
            body += string.Format("{0,-16} {1,-10:D6} {2,-10} {3,-10} {4,-10} x {5,-10} = {6,-10}\n\r",
                buyedStockData.name_, buyedStockData.code_, buyedStockData.nowPrice(priceType_), "YES", buyedStockData.buyCount_, buyedStockData.buyPrice_, buyedStockData.totalBuyPrice());

            mail.setBody(body);
            mail.send();
        }

        public void sendLog()
        {
            Mailer mail = new Mailer();

            mail.setToMailAddr(PublicVar.notifyMail);
            string title = mailTag_ + DateTime.Now.ToString("yyyy년 MM월 dd일 - HH:mm:ss.f") + "주식 결과";
            mail.setSubject(title);
            mail.setAttachFile(Logger.getInstance.getFileName());

            string body = "오늘의 로그 첨부";

            mail.setBody(body);
            mail.send();
        }

        //--------------------------------------------------------
        // 다이얼로그에 그리기 함수
        public void drawStockView()
        {
            //@@@ 락문제 있음....
            lock (lockObject_) {
                StockPoolViewer.getInstance.print(stockPool_);
            }
        }

        //--------------------------------------------------------
        // 트레이딩 모듈 설정
        public string treadModuleName()
        {
            if (tradeModule_ != null) {
                return tradeModule_.GetType().Name;
            }
            return "";
        }

        public TradeModule tradeModule()
        {
            Type type = tradeModule_.GetType();
            TradeModule obj = (TradeModule) Activator.CreateInstance(type);
            return obj; 
        }

        public void setTradeModule(TradeModule module)
        {
            if (tradeModule_ == null) {
                tradeModule_ = module;
                return;
            }
            tradeModule_ = null;
            tradeModule_ = module;
            string log = this.resultAllStockBackTest(priceType_);
            BackTestInfo.getInstance.update(log);

            log = this.simulateBackTest(priceType_);
            BackTestInfo.getInstance.update(log);
        }

        //로드한 주식풀을 이용해서 전체 백테스팅을 해본다.
        string simulateBackTest(PRICE_TYPE priceType)
        {           
            const int LIMIT_TABLE_COUNT = 120;
            int lastTime = 0;
            
            lock (lockObject_) {
                foreach (KeyValuePair<int, StockData> keyValue in stockPool_) {
                    StockData stockData = keyValue.Value;
                    List<CandleData> priceTable = stockData.priceTable(priceType);
                    if (priceTable == null) {
                        continue;
                    }
                    // 120개 이상은 있어야 함.
                    if (priceTable.Count < LIMIT_TABLE_COUNT) {
                        continue;
                    }
                    lastTime = Math.Max(priceTable.Count -1, lastTime);                   
                }
            }
            if (lastTime < LIMIT_TABLE_COUNT) {
                return string.Format("{0} 시간대로는 백테스팅 할 데이터가 없음", priceType);
            }

            int backTestCount = 0;
            int backTestSuccess = 0;

            const int BUY_COUNT = 3;
            const int INIT_MONEY = 10000000;
            int money = INIT_MONEY;
            int[] buyStockCode = new int[BUY_COUNT];
            int[] buyPrice = new int[BUY_COUNT];
            int[] buyCount = new int[BUY_COUNT];

            for(int i = 0; i < BUY_COUNT; ++i) {
                buyStockCode[i] = 0;
                buyPrice[i] = 0;
                buyCount[i] = 0;
            }

            TradeModule tradeModule = this.tradeModule();
            int priceIdx = (int) priceType;
            if (backTestHistory_[priceIdx] != null) {
                backTestHistory_[priceIdx] = null;
                backTestHistory_[priceIdx] = new BackTestHistory();
            }

            DateTime tradeStartTime = DateTime.Now;

            lock (lockObject_) {
                for (int timeIdx = lastTime; timeIdx >= 0; --timeIdx) {
                    foreach (KeyValuePair<int, StockData> keyValue in stockPool_) {
                        int code = keyValue.Key;
                        StockData stockData = keyValue.Value;
                        List<CandleData> priceTable = stockData.priceTable(priceType);
                        if (priceTable == null) {
                            continue;
                        }

                        int price = priceTable[timeIdx].price_;
                        DateTime dateTime = priceTable[timeIdx].date_;
                        tradeModule.setTradeModule(code, priceType);
                        // 파는걸 먼저 처리함.
                        for (int i = 0; i < BUY_COUNT; ++i) {
                            if (buyStockCode[i] == code) {
                                double nowProfit = (double) (price - buyPrice[i]) / (double) price;
                                if (tradeModule.sell(timeIdx, nowProfit)) {
                                    money = money + (buyCount[i] * price);
                                    buyStockCode[i] = 0;
                                    backTestHistory_[priceIdx].addSell(code, timeIdx, dateTime, price, buyCount[i]);

                                    backTestCount++;
                                    if (nowProfit > 0.0f) {
                                        backTestSuccess++;
                                    }
                                    break;
                                }
                            }
                        }

                        int buyIdx = -1;
                        for (int i = 0; i < BUY_COUNT; ++i) {
                            if (buyStockCode[i] == 0) {
                                buyIdx = i;
                                break;
                            }
                        }
                        int remainBuy = 0;
                        for (int i = 0; i < BUY_COUNT; ++i) {
                            if (buyStockCode[i] == 0) {
                                remainBuy++;
                            }
                        }

                        // 살자리가 있으면
                        if (buyIdx != -1) {
                            if (tradeModule.buy(timeIdx)) {
                                buyPrice[buyIdx] = price;
                                buyCount[buyIdx] = (int) ((money / BUY_COUNT) / buyPrice[buyIdx]);
                                buyStockCode[buyIdx] = code;
                                backTestHistory_[priceIdx].addBuy(code, timeIdx, dateTime, price, buyCount[buyIdx]);
                                backTestCount++;

                                if (dateTime < tradeStartTime) {
                                    tradeStartTime = dateTime;
                                }
                            }
                        }
                    }
                }
            }

            // 마지막인 오늘은 현금화 한다고 생각하자
            for (int i = 0; i < BUY_COUNT; ++i) {
                if (buyStockCode[i] != 0) {
                    const int NOW_DAY_IDX = 0;   // 배열상 0번에 오늘 가격 데이터가 들어가 있음.       
                    StockData stockData = this.stockData(buyStockCode[i]);
                    List<CandleData> priceTable = stockData.priceTable(priceType);
                    if (priceTable == null) {
                        continue;
                    }
                    int price = priceTable[NOW_DAY_IDX].price_;
                    DateTime dateTime = priceTable[NOW_DAY_IDX].date_;

                    money = money + (buyCount[i] * price);
                    backTestHistory_[priceIdx].addSell(buyStockCode[i], NOW_DAY_IDX, dateTime, price, buyCount[i]);
                    buyStockCode[i] = 0;

                    backTestCount++;
                    double nowProfit = (double) (price - buyPrice[i]) / (double) price;
                    if (nowProfit > 0.0f) {
                        backTestSuccess++;
                    }
                }
            }
            backTestHistory_[priceIdx].calcEval();

            double profitRate = ((double) (money - INIT_MONEY) / (double) money) * 100;
            string log = string.Format("{0}, {1}분봉 매매 백테스팅\n {2} 부터 지금까지, 총{3} 의 매매 트레이딩\n 자산 {4} -> {5}, 이익율 {6}",
                tradeModule_.GetType().Name, priceType, tradeStartTime, backTestCount, INIT_MONEY, money, profitRate);

            tradeModule = null;

            return log;
        }

        // 백테스팅 테스트 함수
        string resultAllStockBackTest(PRICE_TYPE priceType)
        {
            int totalProfit = 0;
            int totalTestCount = 0;
            int backTestCount = 0;
            int backTestSuccess = 0;
            DateTime firstTradeDate = DateTime.Now;

            string strategy = this.treadModuleName();

            lock (lockObject_) {
                foreach (KeyValuePair<int, StockData> keyValue in stockPool_) {
                    int code = keyValue.Key;
                    StockData stockData = keyValue.Value;
                    stockData.doBackTesting(priceType);
              
                    BackTestHistory history = stockData.backTestHistory(priceType);
                    string testProfitRate = history.profitRate_.ToString("#,#.#");
                    string testCount = history.tradeCount_.ToString();
                    if (history.tradeCount_ == 0) {
                        continue;
                    }

                    double profit = stockData.backTestProfit(priceType);
                    backTestCount++;
                    totalTestCount += history.tradeCount_;
                    totalProfit += history.totalProfit_;

                    if (stockData.backTestFirstTradeDate(priceType) < firstTradeDate) {
                        firstTradeDate = stockData.backTestFirstTradeDate(priceType);
                    }
                    if (stockData.backTestProfit(priceType) > 0.0f) {
                        backTestSuccess++;
                    }
                }
            }
            int initMoney = backTestCount * PublicVar.TradeModuleInitMoney;
            double avgProfitRate = (double) (totalProfit - initMoney) / (double) initMoney * 100.0f;
            double rate = (double) backTestSuccess / (double) backTestCount * 100;

            string result = string.Format("[{0}]으로, {1}전략 백테스팅 결과.\n * 주식{2}개 매매 백테스팅.\n * {3}로부터 {4}번 시도.\n * 결과 : 금액 {5:#,#}원 -> {6:#,#}원 => 이익율 {7:#.##}%\n     성공율 {8:#.##}%",
                priceType, strategy, backTestCount, firstTradeDate, totalTestCount, initMoney, totalProfit, avgProfitRate, rate);

            return result;

            //XXX
            // 이거 테스팅 경우수가 너무 많음...4^24 ;;;
            // 60만건 테스팅 처리 하는데 i7급 cpu 로 5시간 걸림
            //findBestTestPoint(priceType_);
        }

        // 백테스팅 테스트 함수
        public double pointTradeModuleEvaluation(PRICE_TYPE priceType, EvalWeightAverage weightAvg)
        {
            int totalProfit = 0;
            int totalTestCount = 0;
            int count = 0;

            foreach (KeyValuePair<int, StockData> keyValue in stockPool_) {
                int code = keyValue.Key;
                StockData stockData = keyValue.Value;

                stockData.setEvalWeightAverage(weightAvg);
                stockData.doBackTesting(priceType);
                BackTestHistory history = stockData.backTestHistory(priceType);
                string testProfitRate = history.profitRate_.ToString("#,#.#");
                string testCount = history.tradeCount_.ToString();
                if (history.tradeCount_ != 0) {
                    count++;
                    totalTestCount += history.tradeCount_;
                    totalProfit += history.totalProfit_;
                }
            }

            int initMoney = count * PublicVar.TradeModuleInitMoney;
            double avgProfitRate = (double) (totalProfit - initMoney) / (double) initMoney * 100.0f;
            return avgProfitRate;
        }

        public void findBestTestPoint(PRICE_TYPE type)
        {
            System.Diagnostics.Process.GetCurrentProcess().PriorityClass = System.Diagnostics.ProcessPriorityClass.RealTime;
            EvalWeightAverage bestWeightAverage = null;
            double bestRate = 0.0f;
            const int LOOP_COUNT = 5;
            int count = 0;
            // 일단 무식하게 값 셋팅
            for (int idx1 = 1; idx1 < LOOP_COUNT; ++idx1)
            for (int idx2 = 1; idx2 < LOOP_COUNT; ++idx2)
            for (int idx3 = 1; idx3 < LOOP_COUNT; ++idx3)
            for (int idx4 = 1; idx4 < LOOP_COUNT; ++idx4)
            for (int idx5 = 1; idx5 < LOOP_COUNT; ++idx5)
            for (int idx6 = 1; idx6 < LOOP_COUNT; ++idx6)
            for (int idx7 = 1; idx7 < LOOP_COUNT; ++idx7)
            for (int idx8 = 1; idx8 < LOOP_COUNT; ++idx8)
            for (int idx9 = 1; idx9 < LOOP_COUNT; ++idx9)
            for (int idx10 = 1; idx10 < LOOP_COUNT; ++idx10)
            for (int idx11 = 1; idx11 < LOOP_COUNT; ++idx11)
            for (int idx12 = 1; idx12 < LOOP_COUNT; ++idx12)
            for (int idx13 = 1; idx13 < LOOP_COUNT; ++idx13)
            for (int idx14 = 1; idx14 < LOOP_COUNT; ++idx14)
            for (int idx15 = 1; idx15 < LOOP_COUNT; ++idx15)
            for (int idx16 = 1; idx16 < LOOP_COUNT; ++idx16)
            for (int idx17 = 1; idx17 < LOOP_COUNT; ++idx17)
            for (int idx18 = 1; idx18 < LOOP_COUNT; ++idx18)
            for (int idx19 = 1; idx19 < LOOP_COUNT; ++idx19)
            for (int idx20 = 1; idx20 < LOOP_COUNT; ++idx20)
            for (int idx21 = 1; idx21 < LOOP_COUNT; ++idx21)
            for (int idx22 = 1; idx22 < LOOP_COUNT; ++idx22)
            for (int idx23 = 1; idx23 < LOOP_COUNT; ++idx23)
            for (int idx24 = 1; idx24 < LOOP_COUNT; ++idx24) { 
                EvalWeightAverage weightAverage = new EvalWeightAverage();
                weightAverage.weightAvg_[(int) EVALUATION_ITEM.SMA_5] = idx1;
                weightAverage.weightAvg_[(int) EVALUATION_ITEM.SMA_10] = idx2;
                weightAverage.weightAvg_[(int) EVALUATION_ITEM.SMA_20] = idx3;
                weightAverage.weightAvg_[(int) EVALUATION_ITEM.SMA_50] = idx4;
                weightAverage.weightAvg_[(int) EVALUATION_ITEM.SMA_100] = idx5;
                weightAverage.weightAvg_[(int) EVALUATION_ITEM.SMA_200] = idx6;

                weightAverage.weightAvg_[(int) EVALUATION_ITEM.EMA_5] = idx7;
                weightAverage.weightAvg_[(int) EVALUATION_ITEM.EMA_10] = idx8;
                weightAverage.weightAvg_[(int) EVALUATION_ITEM.EMA_20] = idx9;
                weightAverage.weightAvg_[(int) EVALUATION_ITEM.EMA_50] = idx10;
                weightAverage.weightAvg_[(int) EVALUATION_ITEM.EMA_100] = idx11;
                weightAverage.weightAvg_[(int) EVALUATION_ITEM.EMA_200] = idx12;

                weightAverage.weightAvg_[(int) EVALUATION_ITEM.BOLLINGER] = idx13;
                weightAverage.weightAvg_[(int) EVALUATION_ITEM.MACD] = idx14;

                weightAverage.weightAvg_[(int) EVALUATION_ITEM.RSI] = idx15;
                weightAverage.weightAvg_[(int) EVALUATION_ITEM.STOCHASTIC] = idx16;
                weightAverage.weightAvg_[(int) EVALUATION_ITEM.STOCHASTIC_RSI] = idx17;

                weightAverage.weightAvg_[(int) EVALUATION_ITEM.ADX] = idx18;
                weightAverage.weightAvg_[(int) EVALUATION_ITEM.WILLIAMS] = idx19;
                weightAverage.weightAvg_[(int) EVALUATION_ITEM.CCI] = idx20;
                weightAverage.weightAvg_[(int) EVALUATION_ITEM.ATR] = idx21;
                weightAverage.weightAvg_[(int) EVALUATION_ITEM.ROC] = idx22;
                weightAverage.weightAvg_[(int) EVALUATION_ITEM.ULTIMATE_OSCIL] = idx23;
                weightAverage.weightAvg_[(int) EVALUATION_ITEM.BULL_BEAR] = idx24;
                
                count++;
                if (idx13 > 1 || idx14 > 1 || idx15 > 1 || idx16 > 1 || idx17 > 1 || idx18 > 1 || idx19 > 1) {
                    if ((count % 10000) == 0) {
                          Logger.getInstance.consolePrint("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}",
                              idx13, idx14, idx15, idx16, idx17, idx18, idx19, idx20, idx21, idx22, idx23, idx24);
                    }
                }
                double rate = this.pointTradeModuleEvaluation(type, weightAverage);
                if (bestRate < rate) {
                    bestRate = rate;
                    bestWeightAverage = weightAverage;
                    string ret = string.Format("[{0}]으로, 백테스팅 최적화 찾기.\n * 주식{1}개 매매 백테스팅중. 최대이익율 {2:#.##}%\n 새 최적화값", type.ToString(), count, bestRate);
                    ret += bestWeightAverage.log();
                    MessengerBot.getInstance.sendMessage(ret);
                }
            }

            string result = string.Format("[{0}]으로, 백테스팅 최적화 찾기.\n * 주식{1}개 매매 백테스팅. 최대이익율 {2:#.##}%\n", type.ToString(), count, bestRate);
            result += bestWeightAverage.log();
            MessengerBot.getInstance.sendMessage(result);
            System.Diagnostics.Process.GetCurrentProcess().PriorityClass = System.Diagnostics.ProcessPriorityClass.Normal;
        }
    }
}
