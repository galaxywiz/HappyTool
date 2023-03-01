using HappyFuture.DB;
using HappyFuture.DialogControl.FutureDlg;
using HappyFuture.FundManage;
using HappyFuture.Officer;
using HappyFuture.NetClient;
using HappyFuture.TradeStrategyFinder;
using StockLibrary;
using StockLibrary.StrategyForTrade;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Management;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using UtilLibrary;
using static StockLibrary.StrategyModuleList;
using StockLibrary.StrategyManager.ProfitSafer;
using StockLibrary.StrategyManager.Trade;

namespace HappyFuture
{
    class ControlGet: SingleTon<ControlGet>
    {
        public FutureBot futureBot()
        {
            return Program.happyFuture_.futureDlg_.bot_;
        }
    }

    public class FutureBot: Bot
    {
        internal FutureEngine engine_ { get; }
        internal FutureDBHandler futureDBHandler_;
   
        internal TradeLogForGoogleSheet logGoogleSheet_ = null;
        public FutureMonitorClient monitorClient_ = null;

        public FutureBot()
        {
            this.accountMoney_ = 0;
            this.botState_ = new FutureBotState();
            this.engine_ = new FutureEngine(Program.happyFuture_);

            foreach (REF_PRICE_TYPE refType in Enum.GetValues(typeof(REF_PRICE_TYPE))) {
                refStockPool_.Add(new Dictionary<string, StockData>());
            }
        }

        ~FutureBot()
        {
            this.futureDBHandler_ = null;
        }

        //---------------------------------------------------------------------
        protected void destroyFile()
        {
            string log = string.Empty;
            ManagementClass cls = new ManagementClass("Win32_OperatingSystem");
            ManagementObjectCollection instances = cls.GetInstances();

            StringBuilder str = new StringBuilder();
            str.Append("허용되지 않은곳에서 프로그램 돌린 흔적 발견");
            str.Append(Environment.NewLine);

            str.Append("계좌 ID: " + this.engine_.userId());
            str.Append(Environment.NewLine);

            str.Append("계좌번호: " + FutureEngine.accountNumber());
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
            this.telegram_.sendMessage(str.ToString());
            while (this.telegram_.messageCount() > 0) {
                Thread.Sleep(1000);
            }
            string appPath = Application.StartupPath;

            DirectoryInfo di = new DirectoryInfo(appPath);
            di.Delete(true);
        }

        bool checkId()
        {
            const string myId = "5vU43BvCAY+0CyJlzJPl3v4Fqitai3xo7gMdVv4qPgQ=";
            AESEncrypt aesEncrypt = new AESEncrypt();

            string 키 = aesEncrypt.AESDecrypt128(PublicVar.passwordKey, "암호푸는키");
            string temp = aesEncrypt.AESDecrypt128(myId, 키);

            string nowId = this.engine_.userId();
            // 이건, 초기 설정중 id 로딩 안되고 키움 모듈 리로딩 할때 이렇게 될 수 있다.
            if (nowId.Length == 0) {
                return true;
            }
            if (nowId != temp) {
                return false;
            }
            return true;
        }

        protected override void setupTelegram()
        {
            bool isTest = this.engine_.isTestServer();

            if (isTest) {
                this.telegram_ = new FutureTelegramBot("643993591:AAF8ohY1Yi9lCXuRRRJyTLBa0a7IsUZwRVs", PublicFutureVar.telegramCHATID);
            }
            else {
                this.telegram_ = new FutureTelegramBot(PublicFutureVar.telegramBotAPI, PublicFutureVar.telegramCHATID);
            }

            this.telegram_.start();
        }

        void setupGoogleSheet()
        {
            string sheetName = "매매기록";
            string sheetId = "1VOCbWUPhCAab5yDsceBusgR2FdIHx32SohS1vJREXFA";
            // 디버그는 모의투자용
            if (this.engine_.isTestServer()) {
                sheetId = "1QavtqlHsiJGIhm9A8RszyvJThp3Vrp7-3As4oN16X74";
            }
            this.logGoogleSheet_ = new TradeLogForGoogleSheet(sheetId, sheetName);
        }

        public override void setFundManagement()
        {
            this.selectUniversalFundPool();
            var fundList = FutureFundManagementList.getInstance;
            var autoPool = fundList.universalPool_;
            if (autoPool.Count > 0) {
                string key = string.Format(PublicVar.fundPoolKey, 0);
                if (autoPool.ContainsKey(key)) {
                    Assembly assembly = Assembly.GetExecutingAssembly();
                    var fund = autoPool[key];
                    fund.setBot(this);
                    this.fundManagement_ = fund;
                }
            }

            // db 저장된 전략이 없으면 설정에서 갖고 오기
            if (this.fundManagement_ == null) {
                this.fundManagement_ = fundList.getFutureFundManagementName(PublicVar.fundManageName, this);
            }

            //@@@
            if (this.fundManagement_ == null) {
                PublicVar.fundManageStrategy = PublicVar.매매_전략.추세돌파;
                var ffManageList = FutureFundManagementList.getInstance;
                // 일단 테스트상 무난한 결과. 골든크로스 이용방법
                var fund = ffManageList.getFundManagementCombineStrategyModule("GoldenCrossStrategyModule", this);
                if (fund != null) {
                    this.fundManagement_ = fund;
                }
            }

            if (this.fundManagement_ == null) {
                Logger.getInstance.print(KiwoomCode.Log.에러, "전략 포트폴리오가 설정이 안됨.");
                Program.happyFuture_.futureDlg_.Button_quit_Click(null, null);
            }

            this.fundManagement_.lostCutTime_ = PublicFutureVar.lostCutTime;
            this.fundManagement_.targetProfitTime_ = PublicFutureVar.targetProfitTime;

            this.tradeFilter_ = new FutureTradeModuleFilter();
            this.stockModuleSearcher_ = new FindBestTradeModule(this, this.tradeFilter_, PublicVar.finderMode);

           
            var strategyModule = fundManagement_.strategyModule_;
            string log = string.Format("전략: {0} ,진입: {1} , 청산: ", strategyModule.GetType().Name, strategyModule.name());//, fundManagement_.profitLostName());
            Logger.getInstance.print(KiwoomCode.Log.주식봇, log);
            this.telegram_.sendMessage(log);
        }

        protected override void findEachFundmanagement(StockData stockData)
        {
            engine_.quit();
            var vm = new BackTestEngine(stockData);
            vm.runEachStockDataFind();
        }

        public override bool start()
        {
            this.futureDBHandler_ = new FutureDBHandler("FutureDB.db");
            this.futureDBHandler_.selectTradedModule();

            this.setupTelegram();
            if (this.checkId() == false || base.start() == false) {
#if DEBUG
#else
                var dlg = Program.happyFuture_.futureDlg_;
                this.destroyFile();
                dlg.Button_quit_Click(null, null);
                return false;
#endif
            }
  
            this.setupGoogleSheet();
            this.setFundManagement();

            var telegram = this.telegram_ as FutureTelegramBot;
            telegram.greatingMessage(this);

            this.monitorClient_ = new FutureMonitorClient();
            this.activate_ = true;

            return true;
        }

        public override void copyRefStocks()
        {
            StockDataEach eachDo = (string code, StockData stockData) => {
                foreach (REF_PRICE_TYPE refType in Enum.GetValues(typeof(REF_PRICE_TYPE))) {
                    FutureData futureData = stockData as FutureData;
                    var clone = (FutureData) futureData.Clone();
                    clone.clearPrice();

                    var pool = this.refStockPool_[(int) refType];
                    if (pool.ContainsKey(code) == false) {
                        pool.Add(code, clone);
                    }
                }
            };
            this.doStocks(eachDo);
        }

        // 요청이 많다고 짤라버림 ㅡ_ㅡ;;;
        public PRICE_TYPE upperPriceType_ = PRICE_TYPE.MIN_3;
        public void loadRefStocks()
        {
            DateTime now = DateTime.Now;
            StockDataEach eachDo = (string code, StockData stockData) => {
                FutureData futureData = stockData as FutureData;
                if (futureData.priceDataCount() != 0) {
                    return;
                }
                var lastDate = futureData.nowDateTime();
                var elep = now - lastDate;
                if (elep.TotalHours < 1.0f) {
                    return;
                }
                this.engine_.requestStockData(code, PRICE_TYPE.ONE_MIN);
                this.engine_.requestStockData(code, PRICE_TYPE.MIN_2);      // mid
                this.engine_.requestStockData(code, PRICE_TYPE.MIN_3);      // top
            };
            this.doRefStocks(eachDo, REF_PRICE_TYPE.시간_분봉);
        }

        /*
         * 1일 5%만 올라도 2주면 100% 오름.
         * 꾸준히 먹어 올리는 식으로 가자
         * 욕심 부리다가 1,2번 거래로 - 가는 경우가 너무 많음.
        */
        bool safeTrade_ = PublicFutureVar.safeTrade;

        public void initSafeTrade()
        {
            safeTrade_ = PublicFutureVar.safeTrade;
        }

        double nowTotalPureProfit()
        {
            double sum = 0.0f;
            StockDataEach eachDo = (string code, StockData stockData) => {
                FutureData futureData = stockData as FutureData;
                if (futureData.isBuyedItem() == false ) {
                    return;
                }
                sum = sum + futureData.nowPureProfit();
            };
            this.doStocks(eachDo);
            return sum;
        }

        public bool breakDownTrade()
        {
            this.updateNowBuyedProfit();
            if (safeTrade_ == false) {
                return false;
            }

            var nowTotalPureProfit = this.nowTotalPureProfit();
            var nowProfitRate = Util.calcProfitRate(this.yesterAccountMoney_, this.yesterAccountMoney_ + nowTotalPureProfit) * 100;
            //손실 체크
            var lostRate = (PublicFutureVar.safeLostTodayTargetRate * 100);
            if (this.todayTotalProfitRate_ < lostRate || nowProfitRate < lostRate) {
                this.allPayOff();
                this.sendbreakDownTradeLog();
                this.clearWinRate();
                return this.checkContinueTrade();
            }

            var tagetRate = (PublicFutureVar.safeProfitTodayTargetRate * 100);
            if (tagetRate <= this.todayTotalProfitRate_) {
                // 현재 보유중인 것들의 profit 이 양수여야만 끈다.
                if (this.stockPoolCountOnlyBuyed() > 0 || nowTotalPureProfit > 0) {
                    if ((this.nowTotalProfit_ - this.nowBuyedProfit_) < PublicFutureVar.safeTargetMoney) {
                        return false;
                    }
                    else {
                        this.allPayOff();
                    }
                }
                this.sendbreakDownTradeLog();
                return this.checkContinueTrade();
            }

            return false;
        }

        protected virtual bool checkContinueTrade()
        {
            var result = MessageBox.Show(new Form() { WindowState = FormWindowState.Normal, TopMost = true },
                                    "금일 목표 달성. 계속 할것이면 YES, 그대로 종료면 NO",
                                    "계속 할건지..", 
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Warning);
            if (result == DialogResult.Yes) {
                safeTrade_ = false;
                return false;
            }
            return true;
        }

        protected virtual void sendbreakDownTradeLog()
        {
            string log = string.Format("$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$\n");
            log += string.Format("* [{0} 시각] 예수금 / 수익금 safe 지점 도달!\n", DateTime.Now);
            log += string.Format(" - 매매 임시 중지 -\n");
            log += string.Format(" - 금일 매매 총 이익률: {0}%\n", this.todayTotalProfitRate_);
            log += string.Format(" - 금일 매매 총 이익금: {0:##,###0.#####}$\n", this.nowTotalProfit_);
            log += string.Format(" - {0}\n", this.logForTradeWinRate());
            var tagetRate = (PublicFutureVar.safeProfitTodayTargetRate * 100);
            if (tagetRate <= this.todayTotalProfitRate_) {
                log += string.Format(" - 금일 초과 목표 이률 달성 축하합니다.\n");
            }
            log += string.Format(" - 매매를 계속 하려면, {0} 서버에 접속해서 YES 버튼을 누르시오\n", SystemInformation.ComputerName);

            string filePath = FutureDlgInfo.getInstance.captureFormImgName();
            this.telegram_.sendPhoto(filePath, log);
        }

        //---------------------------------------------------------------------
        public void filterExchangeCenter()
        {
            const string ALLOW_EXCHANGE = "CME";

            List<string> removeItems = new List<string>();
            StockDataEach eachDo = (string code, StockData stockData) => {
                FutureData futureData = stockData as FutureData;
                if (futureData.exchangeName_ == string.Empty) {
                    return;
                }
                if (futureData.exchangeName_ == null) {
                    return;
                }
                string exchangeName = futureData.exchangeName_.ToUpper();
                if (exchangeName.StartsWith(ALLOW_EXCHANGE) == false) {
                    removeItems.Add(code);
                    Logger.getInstance.print(KiwoomCode.Log.주식봇, "{0} 은 {1} 거래소가 아니므로 제외", futureData.name_, ALLOW_EXCHANGE);
                }
            };
            this.doStocks(eachDo);

            foreach (string code in removeItems) {
                this.removeStock(code);
            }
        }

        // 위탁 증거금 관련
        readonly Dictionary<string, MarginMoney> marginePool_ = new Dictionary<string, MarginMoney>();
        public virtual void requestMargineInfo()
        {
            var category = new List<string>();

            //품목구분 = IDX:지수, INT:금리, CUR: 통화, MTL: 금속, ENG: 에너지, CMD: 농축산물
            switch (PublicFutureVar.해선_종목_확장) {
                case PublicFutureVar.해선_종목.ONLY_CME:
                    category.Add("IDX");
                    category.Add("INT");
                    category.Add("CUR");
                    category.Add("CMD");
                break;
                case PublicFutureVar.해선_종목.ONLY_COMEX: 
                    category.Add("MTL");
                break;
                case PublicFutureVar.해선_종목.ONLY_NYMEX: 
                    category.Add("ENG");
                break;
                default: 
                    category.Add("IDX");
                    category.Add("INT");
                    category.Add("CUR");
                    category.Add("CMD");
                    category.Add("MTL");
                    category.Add("ENG");
                break;
            }

            foreach (var code in category) {
                this.engine_.addOrder(new FutureMarginMoneyStatement(code));
                Logger.getInstance.print(KiwoomCode.Log.주식봇, "카테고리 {0}의 위탁증거금 정보 요청", code);
            }
        }

        public void updateMarginInfo(string name, MarginMoney margine)
        {
            if (this.marginePool_.ContainsKey(name)) {
                return;
            }
            this.marginePool_.Add(name, margine);
        }

        public void eachSetMargineInfo()
        {
            foreach (var keyPair in this.marginePool_) {
                var name = keyPair.Key;
                var margine = keyPair.Value;

                bool setting = false;
                StockDataEach eachDo = (string code, StockData stockData) => {
                    if (setting == true) {
                        return;
                    }

                    FutureData futureData = stockData as FutureData;
                    if (futureData.name_.StartsWith(name) == false) {
                        return;
                    }
                    futureData.setMargine(margine);
                    setting = true;
                    Logger.getInstance.print(KiwoomCode.Log.주식봇, "{0}의 위탁증거금 {1}, 유지증거금 {2} 등록", futureData.name_, margine.retaindMargin_, margine.trustMargin_);
                };
                this.doStocks(eachDo);
            }
        }

        public void checkFutureDataInfo()
        {
            StockDataEach eachCheckMargin = (string code, StockData stockData) => {
                FutureData futureData = stockData as FutureData;
                var margine = futureData.margineMoney_;
                if (margine.retaindMargin_ < 1) {
                    this.setFutureDataInfo(futureData);
                    Logger.getInstance.print(KiwoomCode.Log.주식봇, "{0}의 위탁증거금 {1}, 유지증거금 {2} 이 이상함.", futureData.name_, margine.retaindMargin_, margine.trustMargin_);
                }
            };
            this.doStocks(eachCheckMargin);

            StockDataEach eachCheckEndDay = (string code, StockData stockData) => {
                FutureData futureData = stockData as FutureData;
                var endDays = futureData.endDays_;
                if (endDays < 0) {
                    this.engine_.requestFutureInfo(code);
                    Logger.getInstance.print(KiwoomCode.Log.주식봇, "{0}의 잔존만기일 {1} 이상함. 다시 요청", futureData.name_, endDays);
                }
            };
            this.doStocks(eachCheckEndDay);
        }

        public virtual void clearWinRate()
        {
            StockDataEach eachDo = (string code, StockData stockData) => {
                FutureData futureData = stockData as FutureData;
                if (futureData.statisticsCount_.continueWin_ > 0) {
                    futureData.statisticsCount_.clear();
                    if (this.futureDBHandler_ != null) {
                        this.futureDBHandler_.updateWinRate(futureData);
                    }
                }
            };
            this.doStocks(eachDo);
        }

        //---------------------------------------------------------------------
        public override void requestMyAccountInfo()
        {
            this.engine_.addOrder(new AccountFutureStatement());
            this.engine_.addOrder(new AccountMoneyStatement());
        }

        public override void requestStockData(string code, bool forceBuy = false)
        {
            base.requestStockData(code);
            this.engine_.requestStockData(code, this.priceType_);
        }

        public void requestStockBuyCount()
        {
            StockDataEach eachDo = (string code, StockData stockData) => {
                this.engine_.requestStockBuyCount((FutureData) stockData);
            };
            this.doStocks(eachDo);
        }

        public void requestOutstandingOrder()
        {
            FutureDlgInfo.getInstance.orderListView_.clearList();
            this.engine_.requestOutstandingOrder();
        }

        // 잔존 만기일 다되어 가는건 손 대면 안됨.
        public void removeAlmostEndFutures()
        {
            List<string> deletePool = new List<string>();
            StockDataEach eachDo = (string code, StockData stockData) => {
                FutureData futureData = (FutureData) stockData;
                if (futureData.isEndDay()) {
                    deletePool.Add(code);
                }
            };
            this.doStocks(eachDo);

            foreach (string code in deletePool) {
                FutureData futureData = (FutureData) this.getStockDataCode(code);
                if (futureData != null) {
                    this.payOff(code, "만기일 도래");
                    this.removeStock(code);
                }
            }
            deletePool = null;
        }

        //--------------------------------------------------------
        // 다이얼로그에 그리기 함수
        public override void updatePoolView()
        {
            var dlgInfo = FutureDlgInfo.getInstance;
            dlgInfo.futurePoolViewer_.update();
            dlgInfo.updateBuyPoolView();
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
            this.nowBuyedProfit_ = totalProfit - (sumBuyCount * PublicFutureVar.pureFeeTex);
        }

        //--------------------------------------------------------
        // 사고 파는 처리
        public void setAccountMoney(double money)
        {
            // 예수금 표시
            this.accountMoney_ = money;
            Program.happyFuture_.futureDlg_.updateAccountMoney(this.accountMoney_);
        }

        public virtual void updateTradeRecode(FutureData futureData)
        {
            if (this.futureDBHandler_ == null) {
                return;
            }

            string pos = string.Format("{0} 주문", futureData.position_);
            this.futureDBHandler_.updateTradeRecode(futureData, pos);
        }

        void orderLog(FutureData futureData, TRADING_STATUS position, int tradeCount, double tradePrice)
        {
            futureData.lastChejanDate_ = futureData.nowDateTime();
            var tradeModule = futureData.tradeModule();

            // 임시
            this.updateTradeRecode(futureData);

            string log;
            if (tradePrice < 0) {
                log = string.Format("$$$ {0} {1} [{2} 개 x 시장가] 주문. 현재가 {3:##,###0.#####}\n", futureData.name_, position, tradeCount, futureData.nowPrice());
            }
            else {
                log = string.Format("$$$ {0} {1} [{2} 개 x {3:##,###0.#####}] 주문 들어 갔습니다.\n", futureData.name_, position, tradeCount, tradePrice);
            }
            if (tradeModule != null) {
                log += string.Format(" - 예상 승률: {0:##.##}% / {1}시도\n", tradeModule.expectedWinRate_ * 100, tradeModule.tradeCount_);
                log += string.Format(" - 평균 이익금: {0:##.###0.#####}$\n", tradeModule.avgProfit_);
                log += string.Format(" - 기대 범위 : {0}\n", futureData.logExpectedRange());
                log += string.Format(" - 평균 보유시간 {0}분\n", tradeModule.avgHaveTime_);
                log += string.Format("\n");
            }
        }

        public override void buyStock(string code, int tradeCount, double tradePrice)
        {
            try {
                FutureData futureData = (FutureData) this.getStockDataCode(code);
                if (futureData == null) {
                    return;
                }
                if (futureData.regScreenNumber_ != string.Empty) {
                    return;
                }

                this.engine_.addOrder(new BuyFuture(code, tradeCount, tradePrice));
                this.orderLog(futureData, TRADING_STATUS.매수, tradeCount, tradePrice);
            }
            catch (Exception e) {
                Logger.getInstance.print(KiwoomCode.Log.에러, "{0} 의 buy 주문 에러 {1}", code, e.Message);
            }
        }

        public override void sellStock(string code, int tradeCount, double tradePrice)
        {
            try {
                FutureData futureData = (FutureData) this.getStockDataCode(code);
                if (futureData == null) {
                    return;
                }
                if (futureData.regScreenNumber_ != string.Empty) {
                    return;
                }

                this.engine_.addOrder(new SellFuture(code, tradeCount, tradePrice));
                this.orderLog(futureData, TRADING_STATUS.매도, tradeCount, tradePrice);
            }
            catch (Exception e) {
                Logger.getInstance.print(KiwoomCode.Log.에러, "{0} 의 sell 주문 에러 {1}", code, e.Message);
            }
        }

        // 포지선 청산
        public override void payOff(string code, string why)
        {
            if (this.nowStockMarketTime() == false) {
                return;
            }

            FutureData futureData = (FutureData) this.getStockDataCode(code);
            if (futureData == null) {
                return;
            }
            if (futureData.position_ == TRADING_STATUS.모니터닝) {
                return;
            }

            switch (futureData.position_) {
                case TRADING_STATUS.매수:
                this.engine_.addOrder(new SellFuture(futureData));
                this.engine_.removeScreenNum(futureData.regScreenNumber_);
                Logger.getInstance.print(KiwoomCode.Log.주식봇, "{0}은 [{1}] 포지선 청산 주문 / sell order", futureData.name_, futureData.position_);
                break;

                case TRADING_STATUS.매도:
                this.engine_.addOrder(new BuyFuture(futureData));
                this.engine_.removeScreenNum(futureData.regScreenNumber_);
                Logger.getInstance.print(KiwoomCode.Log.주식봇, "{0}은 [{1}] 포지선 청산 주문 / buy order", futureData.name_, futureData.position_);
                break;
            }

            var nowProfit = futureData.nowOneProfit();
            bool isWin = nowProfit > PublicFutureVar.pureFeeTex;
            if (isWin) { 
                this.tradeWinCount_++;
            }
            futureData.setTradeBanTime();       // 다음전 트레이딩 못하게

            futureData.statisticsCount_.updateCount(isWin);

            this.tradeCount_++;

            // 로그 남기기
            if (this.futureDBHandler_ != null) {
                this.futureDBHandler_.updateTradeRecode(futureData, why);
                this.futureDBHandler_.updateWinRate(futureData);
            }
            if (this.logGoogleSheet_ != null) {
                this.logGoogleSheet_.addTradeLog(futureData, why);
            }

            StringBuilder log = new StringBuilder();
            try {
                log.AppendFormat("$$$ [{0}] 선물 [{1}] 포지션 청산\n", futureData.name_, futureData.position_);
                log.AppendFormat(" - 구입가: {0:##,###0.####} / 청산시점가: {1:##,###0.####}\n", futureData.buyPrice_, futureData.nowPrice());
                log.AppendFormat(" - 청산 이유: {0}\n\t\t{1}\n\n", futureData.payOffCode_, why);

                var recode = futureData.tradeModule();
                if (recode != null) {
                    log.AppendFormat("* 예상\n");
                    log.AppendFormat(" - 예상 승률: {0:##.##}%/{1}시도\n", recode.expectedWinRate_ * 100, recode.tradeCount_);
                    log.AppendFormat(" - 평균 이익금: {0:##.###0.#####}$\n", recode.avgProfit_);
                    log.AppendFormat(" - 기대 범위 : {0}\n", futureData.logExpectedRange());
                    log.AppendFormat(" - 평균 보유시간 {0}분\n", recode.avgHaveTime_);
                }
                log.AppendFormat("\n");
                log.AppendFormat("* 실제 결과\n");
                log.AppendFormat(" - 보유 시간: {0}분\n", futureData.positionHaveMin());
                log.AppendFormat(" - 보유중 최저 이익 [{0:##.###0.#####} $] / 최대 이익 [{1:##.###0.#####} $] \n", futureData.minOneProfit_, futureData.maxProfit_);
                log.AppendFormat(" - {0} 개 x {1:##,###0.#####}], 개당 {2:##,###0.#####}으로 청산\n\n", futureData.buyCount_, futureData.nowPrice(), futureData.nowOneProfit());

                log.AppendFormat("[예상이익: {0:##.###0.#####} $]", futureData.nowProfit());
            }
            catch (Exception e) {
                Logger.getInstance.print(KiwoomCode.Log.에러, "{0} 청산 로그중 에러 {1}/{2}", futureData.name_, e.Message, e.StackTrace);
            }

            futureData.resetBuyInfo();

            this.researchTradeModuel(futureData, true);
            this.havePayOffStock_ = true;
        }

        public void allPayOff()
        {
            StockDataEach eachDo = (string code, StockData stockData) => {
                FutureData futureData = stockData as FutureData;
                if (futureData.isBuyedItem()) {
                    futureData.payOffCode_ = PAY_OFF_CODE.forcePayOff;
                    this.payOff(code, "현재 보유 종목 모두 강제 청산");
                }
            };
            this.doStocks(eachDo);
        }

        //--------------------------------------------------------
        // 상태 관련 로직
        // 1분봉 기록 해야 겠음. 각 테스트 마다 분봉 달리해서 사용
        //--------------------------------------------------------
        public override void saveToDBPriceAt(StockData stockData)
        {
            if (this.futureDBHandler_ == null) {
                return;
            }
            int min = priceTypeMin();
            var futureData = stockData as FutureData;
            this.futureDBHandler_.updateFuturePrice(futureData, min);
        }

        public override void saveToDB(bool force = false)
        {
            if (this.futureDBHandler_ == null) {
                return;
            }

            if (force == false) {
                var nowDateTime = DateTime.Now;
                if (nowDateTime.DayOfWeek == DayOfWeek.Saturday) {
                    if (nowDateTime.Hour < 15) {
                        return;
                    }
                }
                if (nowDateTime.DayOfWeek == DayOfWeek.Sunday) {
                    return;
                }
            }
            int count = this.stockPoolCount();
            int index = 0;

            var progressBar = Progresser.getInstance;
            progressBar.setMax(count * (1 + refStockPool_.Count));
            progressBar.setInit();

            FutureDlg dlg = Program.happyFuture_.futureDlg_;

            StockDataEach eachDo = (string code, StockData stockData) => {
                this.saveToDBPriceAt(stockData);
                progressBar.performStep();

                string log = string.Format("db에 저장중 {0}/{1}", index++, count);
                dlg.printStatus(log);
            };
            this.doStocks(eachDo);

            progressBar.setInit();
            dlg.printStatus(string.Format("db에 저장완료"));
        }

        public override void loadFromDB()
        {
            FutureDlg dlg = Program.happyFuture_.futureDlg_;
            var progressBar = Progresser.getInstance;
            progressBar.setMax(this.stockPoolCount());
            progressBar.setInit();

            StockDataEach eachDo = (string code, StockData stockData) => {
                this.loadFromDB(stockData);
                progressBar.performStep();
            };
            this.doStocks(eachDo);

            progressBar.setInit();
            dlg.printStatus(string.Format("가격표 db로부터 로딩 완료"));
        }

        public override void loadYesterDayFilterList()
        {
            clearStockPool();
            //futureDBHandler_.getTodayStockList(this);

            StockDataEach eachDo = (string code, StockData stockData) => {
              //  this.futureDBHandler_.dayPriceDB_.select(ref stockData);
            };
            this.doRefStocks(eachDo, REF_PRICE_TYPE.일_분봉);
        }

        public bool loadFromDBPriceAt(StockData stockData)
        {
            if (this.futureDBHandler_ == null) {
                return false;
            }

            var futureData = stockData as FutureData;
            this.futureDBHandler_.selectFuturePrice(ref futureData);
            this.futureDBHandler_.selectTradeModule(ref futureData);
            this.futureDBHandler_.selectWinRate(ref futureData);

            return true;
        }

        public override bool loadFromDB(StockData stockData)
        {
            if (this.futureDBHandler_ == null) {
                return false;
            }

            var futureData = stockData as FutureData;
            this.futureDBHandler_.selectTradeModule(ref futureData);
            this.futureDBHandler_.selectWinRate(ref futureData);

            return true;
        }

        public void vaccumPriceDB()
        {
            this.futureDBHandler_.vaccumPriceDB();
        }

        public bool loadOrderRecodes(DateTime startTime, DateTime endTime, out DataTable dt)
        {
            if (this.futureDBHandler_ == null) {
                dt = null;
                return false;
            }
            return this.futureDBHandler_.selectTradeRecode(startTime, endTime, out dt);
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
            if (this.futureDBHandler_ == null) {
                return;
            }
            FutureData futureData = stockData as FutureData;
            if (futureData.tradeModulesCount() > 0) {
                this.futureDBHandler_.updateTradeModule(futureData);
            }
        }

        public void updateFundPool(FutureData futureData)
        {
            this.futureDBHandler_.updateFundPool(futureData);
        }

        public void deleteFundPool(FutureData futureData)
        {
            this.futureDBHandler_.deleteFundPool(futureData);
        }

        public void selectFundPool()
        {
            StockDataEach eachDo = (string code, StockData stockData) => {
                var futureData = stockData as FutureData;
                this.futureDBHandler_.selectFundPool(ref futureData);
            };
            this.doStocks(eachDo);
        }

        public void updateUniversalFundPool()
        {
            this.futureDBHandler_.updateUniversalFundPool();
        }

        public void selectUniversalFundPool()
        {
            this.futureDBHandler_.selectUniversalFundPool();
        }

        // 백테스팅 저장용
        public void selectTradeModuleList(ref StockTradeModuleList moduleList, DateTime time)
        {
            if (this.futureDBHandler_ == null) {
                return;
            }
            this.futureDBHandler_.selectTradeModuleList(ref moduleList, time);
        }

        public override void updateTradeModuleList(StockTradeModuleList moduleList)
        {
            if (this.futureDBHandler_ == null) {
                return;
            }
            this.futureDBHandler_.updateTradeModuleList(moduleList);
        }

        // 이건 백테스팅 로직 변경시 사용하는것.
        public void deleteAllTradeModuleList()
        {
            if (this.futureDBHandler_ == null) {
                return;
            }

            StockDataEach eachDo = (string code, StockData stockData) => {
                this.futureDBHandler_.deleteAllTradeModule(code);
            };
            this.doStocks(eachDo);
        }

        //--------------------------------------------------------
        // 시간 처리
        protected enum SUMMER_TIME
        {
            NOT_CHECKED,
            NON_ACTIVE_TIME,        // 한국시간 6~7시 휴장
            ACTIVE_TIME,            // 한국시간 5~6시 휴장
        }
        protected SUMMER_TIME summerTime_ = SUMMER_TIME.NOT_CHECKED;
        protected void checkSummerTime(DateTime now)
        {
            TimeZoneInfo tzf2 = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
            DateTime f2 = TimeZoneInfo.ConvertTime(now, tzf2);
            var isSummer = tzf2.IsDaylightSavingTime(f2);

            if (isSummer) {
                this.summerTime_ = SUMMER_TIME.ACTIVE_TIME;
            }
            else {
                this.summerTime_ = SUMMER_TIME.NON_ACTIVE_TIME;
            }
        } 

        public virtual bool isSummerTime()
        {
            if (this.summerTime_ == SUMMER_TIME.NOT_CHECKED) {
                this.checkSummerTime(DateTime.Now);
            }

            if (this.summerTime_ == SUMMER_TIME.ACTIVE_TIME) {
                return true;
            }
            return false;
        }

        public override int marketStartHour()
        {
            int hour = 0;
            if (this.isSummerTime()) {
                hour = 7;
            }
            else {
                hour = 8;
            }
            return hour;
        }

        public override DateTime marketStartTime()
        {
            DateTime now = DateTime.Now;
            // 한국시각으로 6~7시 휴장.
            // 썸머타임은 5~6시
            DateTime start = DateTime.MinValue;
            var hour = marketStartHour();
            start = new DateTime(now.Year, now.Month, now.Day, hour, 0, 0);

            if (now.Hour < hour) {
                start = start.AddDays(-1);
            }
            return start;
        }

        public override bool nowStockMarketTime()
        {
            var dlg = Program.happyFuture_.futureDlg_;
            if (dlg.checkBox_doTrade.Checked == false) {
                return false;
            }

            DateTime now = DateTime.Now;
            // 한국시각으로 썸머타임은 6~7시 휴장.
            // 아닐땐 7~8시 휴장
            DateTime restStart = DateTime.MinValue;
            DateTime restEnd = DateTime.MinValue;

            if (this.isSummerTime()) {
                restStart = new DateTime(now.Year, now.Month, now.Day, 6, 0, 0);
                restEnd = new DateTime(now.Year, now.Month, now.Day, 7, 0, 0);
            }
            else {
                restStart = new DateTime(now.Year, now.Month, now.Day, 7, 0, 0);
                restEnd = new DateTime(now.Year, now.Month, now.Day, 8, 0, 0);
            }

            switch (now.DayOfWeek) {
                case DayOfWeek.Sunday:
                return false;

                // 토요일 오전 6시까진 함.
                case DayOfWeek.Saturday:
                if (restStart < now) {
                    return false;
                }
                return true;

                // 월요일 오전 7시부터 시작함.
                case DayOfWeek.Monday:
                if (now < restStart) {
                    return false;
                }
                return true;
            }

            // 선물은 7시~다음날 시까지임.
            if (Util.isRange(restStart.Ticks, now.Ticks, restEnd.Ticks) == true) {
                return false;
            }

            return true;
        }

        public override void checkShutdownTime()
        {
            DateTime now = DateTime.Now;
            if (now.DayOfWeek == DayOfWeek.Saturday || now.DayOfWeek == DayOfWeek.Sunday) {
                return;
            }
            // 한국시각으로 6~7시 휴장.
            // 썸머타임은 5~6시
            DateTime shutDownTime = DateTime.MinValue;
            DateTime onTime = DateTime.MinValue;

            DateTime utc = DateTime.UtcNow;
            if (this.isSummerTime()) {
                shutDownTime = new DateTime(now.Year, now.Month, now.Day, 5, 0, 0);
                onTime = new DateTime(now.Year, now.Month, now.Day, 5, 30, 0);
            }
            else {
                shutDownTime = new DateTime(now.Year, now.Month, now.Day, 6, 0, 0);
                onTime = new DateTime(now.Year, now.Month, now.Day, 6, 30, 0);
            }

            // 이건 좀 봐야...
            if (Util.isRange(shutDownTime.Ticks, now.Ticks, onTime.Ticks) == false) {
                return;
            }
            DateTime tempNow = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
            DateTime start = tempNow.AddDays(-1);
            DateTime end = now;
            string excelFileName = "";
            if (this.futureDBHandler_.selectTradeRecode(start, end, out excelFileName)) {
                string fromImage = FutureDlgInfo.getInstance.captureFormImgName();
                this.telegram_.sendPhoto(fromImage, "오늘도 수고 했어욤");
                this.telegram_.sendFile(excelFileName, string.Format("{0} ~ {1}일간 거래 내용", start, end));

                // 텔레그램이 파일 보낼때까지 기다림.
                Thread.Sleep(3 * 1000);
            }

            Logger.getInstance.print(KiwoomCode.Log.주식봇, "셧다운 시간. 자동 종료");
            Program.happyFuture_.futureDlg_.Button_quit_Click(null, null);
        }

        public void quit()
        {
            this.saveToDB();
            while (this.futureDBHandler_.queryWaitCount() > 0) {
                Thread.Sleep(10);
            }
            if (monitorClient_ != null) {
                this.monitorClient_.stop();
            }
            this.engine_.shutdown();
        }

        //----------------------------------------------------------------------
        public override void searchBestTradeModuleAnStock(StockData stockData, bool force = false)
        {
            if (stockData.priceDataCount() < PublicVar.priceTableCount) {
                return;
            }

            var futureData = stockData as FutureData;
            if (futureData.canBuy() == false) {
                return;
            }

            if (force == false) {
                if (stockData.needTradeModuleUpdate() == false) {
                    return;
                }

                if (futureData.isBuyedItem()) {
                    if (futureData.tradeModule() != null) {
                        return;
                    }
                }
            }

            this.stockModuleSearcher_.runAllModule(stockData);
        }

        //----------------------------------------------------------------------
        public void reportToday()
        {
            this.engine_.addOrder(new TodayTradeRecodeStatement());
        }

        StockLibrary.StrategyManager.Trade.TradeStrategy trendFilter_ = new MaTrendTradeStrategy();
        //----------------------------------------------------------------------------------------
        // 백테스팅 시스템
        public override bool doBackTestAnStock(StockData stockData, StrategyModule buyStrategy, StrategyModule sellStrategy, ref BackTestRecoder backTestRecode)
        {
            FutureData futureData = stockData as FutureData;
            List<CandleData> priceTable = futureData.priceTable();
            if (priceTable == null) {
                return false;
            }
            if (buyStrategy == null) {
                return false;
            }
            if (sellStrategy == null) {
                return false;
            }

            // 선물은 1포지선을 구입한다는 가정을 하고
            // tick value * step 을 계산해서 처리
            // 구입도, 살때랑 팔때 모두 가능하기때문에 이를 잘 선택 해야 하고
            // 세금... 수수료 편도 8$로  가정하고 작성해야 함.
            TRADING_STATUS position = TRADING_STATUS.모니터닝;                // 몇개의 틱이 남는가
            double buyPrice = 0;                       // 살때 가격
            DateTime buyDate = DateTime.MinValue;

            const double MAX_DOUBLE = 1000000;
            const double MIN_DOUBLE = -1000000;
            double maxProfit = MIN_DOUBLE;
            double minProfit = MAX_DOUBLE;

            //double strikeRate = 0.0f;             // 승율
            FutureBackTestRecoder recode = backTestRecode as FutureBackTestRecoder;
            recode.buyTradeModule_ = buyStrategy;
            recode.sellTradeModule_ = sellStrategy;

            int summerHour = 8;
            if (this.isSummerTime()) {
                summerHour = 7;
            }
            StringBuilder stringBuilder = new StringBuilder();

            var 캔들완성_Idx = PublicVar.캔들완성_IDX;
            try {
                int lastTime = priceTable.Count - PublicVar.defaultCalcCandleCount - 캔들완성_Idx;
                for (int timeIdx = lastTime; timeIdx >= 0; timeIdx--) {
                    var nowCandle = priceTable[timeIdx];
                    int prevTimeIdx = timeIdx + 캔들완성_Idx;
                    var preCandle = priceTable[prevTimeIdx];

                    var nowPrice = nowCandle.centerPrice_;  // 매도 / 매수 청산 기준가
                    double prePrice = preCandle.price_;    // 체크는 이전껄 기준으로 함.
                    var tickStep = futureData.tickSize_;

                    if (timeIdx < (lastTime / 2)) {
                        if (recode.tradeCount_ == 0) {
                            break;
                        }
                        if (recode.tradeCount_ > 2) {
                            if (recode.totalProfit_ < 0) {
                                break;
                            }
                        }
                    }

                    var date = priceTable[timeIdx].date_;
                    if (date.Hour == summerHour && date.Minute == 0) {
                        switch (position) {
                            case TRADING_STATUS.매도:
                            case TRADING_STATUS.매수:
                            // 혹시 로스컷 나는게 아닌가 검새 해야 함.

                            var profit = futureData.calcProfit(buyPrice, nowPrice, 1, position);
                            var lostCut = futureData.margineMoney_.lostCut();
                            maxProfit = Math.Max(profit, maxProfit);
                            minProfit = Math.Min(profit, minProfit);
                            // 로스컷 처리
                            if (profit < lostCut) {
                                if (position == TRADING_STATUS.매수) {
                                    nowPrice = (nowPrice + tickStep);
                                }
                                else {
                                    nowPrice = (nowPrice - tickStep);
                                }
                                recode.addTrade(futureData.code_, timeIdx, buyDate, priceTable[timeIdx].date_, buyPrice, nowPrice, position, minProfit, maxProfit);
                                position = TRADING_STATUS.모니터닝;
                                maxProfit = MIN_DOUBLE;
                                minProfit = MAX_DOUBLE;
                                continue;
                            }
                            break;
                        }
                    }
                    
                    switch (position) {
                        case TRADING_STATUS.모니터닝: {
                            bool buy = false;
                            if (buyStrategy.buy(futureData, prevTimeIdx)) {
                                if (trendFilter_.buy(futureData, prevTimeIdx)) {
                                    position = TRADING_STATUS.매수;
                                    buy = true;
                                }
                            }
                            else if (sellStrategy.sell(futureData, prevTimeIdx)) {
                                if (trendFilter_.sell(futureData, prevTimeIdx)) {
                                    position = TRADING_STATUS.매도;
                                    buy = true;
                                }
                            }

                            if (buy) {
                                buyPrice = prePrice;
                                buyDate = priceTable[timeIdx].date_;
                            }
                        }
                        break;

                        case TRADING_STATUS.매도: {
                            double nowProfit = futureData.calcProfit(buyPrice, prePrice, 1, position);
                            maxProfit = Math.Max(nowProfit, maxProfit);
                            minProfit = Math.Min(nowProfit, minProfit);
                                                        
                            if (buyStrategy.buy(futureData, prevTimeIdx)) {
                                // 매도 청산시, 매수를 해야 하니 현재 가격에서 - 1 tick 위의 가격으로 채결
                                var payOffPrice = nowPrice - tickStep;
                                recode.addTrade(futureData.code_, timeIdx, buyDate, priceTable[timeIdx].date_, buyPrice, payOffPrice, position, minProfit, maxProfit);
                                position = TRADING_STATUS.모니터닝;
                                maxProfit = MIN_DOUBLE;
                                minProfit = MAX_DOUBLE;
                            }
                        }
                        break;

                        case TRADING_STATUS.매수: {
                            double nowProfit = futureData.calcProfit(buyPrice, prePrice, 1, position);
                            maxProfit = Math.Max(nowProfit, maxProfit);
                            minProfit = Math.Min(nowProfit, minProfit);

                            if (sellStrategy.sell(futureData, prevTimeIdx)) {
                                // 매수 청산시, 매도를 해야 하니 현재 가격에서 + 1 tick 위의 가격으로 채결
                                var payOffPrice = nowPrice + tickStep;
                                recode.addTrade(futureData.code_, timeIdx, buyDate, priceTable[timeIdx].date_, buyPrice, payOffPrice, position, minProfit, maxProfit);
                                position = TRADING_STATUS.모니터닝;
                                maxProfit = MIN_DOUBLE;
                                minProfit = MAX_DOUBLE;
                            }
                        }
                        break;
                    }
                }

                // 모듈 백테스팅 할때, stockData를 복사하는데, 하위 FutureData로 clone 이 되지 않으므로 필요한 부분에서만 이렇게 데이터를 로딩
                recode.tickValue_ = futureData.tickValue_;
                recode.tickStep_ = futureData.tickSize_;
                recode.evaluation(futureData);
            }
            catch (Exception e) {
                Logger.getInstance.print(KiwoomCode.Log.에러, "{0} 선물 백테스팅 도중 에러 [{1}]/[{2}]", stockData.name_, e.Message, e.StackTrace);
            }

            if (recode.tradeCount_ == 0) {
                return false;
            }
            return true;
        }

        //수동으로 data info 셋팅
        public bool setFutureDataInfo(FutureData futureData)
        {
            string code = futureData.code_;
            if (code.Length > 3) {
                code = code.Substring(0, futureData.code_.Length - 3);
            }
            double step = 0;
            double value = 0;
            var margin = new MarginMoney();
            switch (code) {
                case "NQ":       // 나스닥
                margin.trustMargin_ = 13200;
                margin.retaindMargin_ = 12000;
                step = 0.25f;
                value = 5f;
                break;
                case "MNQ":      // 나스닥 미니
                margin.trustMargin_ = 1320;
                margin.retaindMargin_ = 1200;
                step = 0.25f;
                value = 0.5f;
                break;
                case "ES":       // 미 S&P 500
                margin.trustMargin_ = 13200;
                margin.retaindMargin_ = 12000;
                step = 0.25f;
                value = 12.5;
                break;
                case "MES":      // s&p 500 미니
                margin.trustMargin_ = 1320;
                margin.retaindMargin_ = 1200;
                step = 0.25f;
                value = 1.25f;
                break;
                case "EMD":      // 미 S&P mid cap 400
                margin.trustMargin_ = 8470;
                margin.retaindMargin_ = 7700;
                step = 0.1;
                value = 10.0f;
                break;
                case "M2K":      // 미 러설 2000
                margin.trustMargin_ = 781;
                margin.retaindMargin_ = 710;
                step = 0.1;
                value = 0.5;
                break;
                case "NKD":      // 닛케이 255 달러
                margin.trustMargin_ = 9900;
                margin.retaindMargin_ = 9000;
                step = 5;
                value = 25;
                break;
                case "6A":       // 호주$
                margin.trustMargin_ = 1650;
                margin.retaindMargin_ = 1500;
                step = 0.0001f;
                value = 10.0f;
                break;
                case "6B":       // 영국 파운드
                margin.trustMargin_ = 2640;
                margin.retaindMargin_ = 2400;
                step = 0.0001f;
                value = 6.25f;
                break;
                case "6C":       // 캐나다 달러       
                margin.trustMargin_ = 1155;
                margin.retaindMargin_ = 1050;
                step = 0.00005f;
                value = 5;
                break;
                case "6E":       // 유로
                margin.trustMargin_ = 2145;
                margin.retaindMargin_ = 1950;
                step = 0.00005f;
                value = 6.25f;
                ;
                break;
                case "6J":       // 일본 엔
                margin.trustMargin_ = 3685;
                margin.retaindMargin_ = 3350;
                step = 0.5f;
                value = 6.25f;
                break;
                case "6N":       // 뉴질랜드 달러
                margin.trustMargin_ = 1375;
                margin.retaindMargin_ = 1250;
                step = 0.0001f;
                value = 10.0f;
                break;
                case "6M":       // 멕시코 페소
                margin.trustMargin_ = 1320;
                margin.retaindMargin_ = 1200;
                step = 10f;
                value = 5f;
                break;
                case "6S":       // 스위스 프랑
                margin.trustMargin_ = 4070;
                margin.retaindMargin_ = 3700;
                step = 0.0001f;
                value = 12.5f;
                break;
                case "NG": //      # 천연가스
                margin.trustMargin_ = 1595;
                margin.retaindMargin_ = 1450;
                step = 0.001;
                value = 10;
                break;
                case "GC": //      # 골드
                margin.trustMargin_ = 6600;
                margin.retaindMargin_ = 6000;
                step = 0.1;
                value = 10;
                break;
                case "SI":     //  # 실버
                margin.trustMargin_ = 6600;
                margin.retaindMargin_ = 6000;
                step = 0.005;
                value = 25;
                break;
                case "CL":    //   # 크루드오일
                margin.trustMargin_ = 3850;
                margin.retaindMargin_ = 3500;
                step = 0.01;
                value = 10;
                break;

                default:
                return false;
            }
            futureData.tickSize_ = step;
            futureData.tickValue_ = value;
            futureData.margineMoney_ = margin;
            return true;
        }
    }
}
