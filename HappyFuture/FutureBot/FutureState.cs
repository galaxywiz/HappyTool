using HappyFuture.DialogControl.FutureDlg;
using StockLibrary;
using System;
using System.Collections.Generic;
using System.Threading;

using UtilLibrary;

namespace HappyFuture
{
    class FutureState: Phase
    {
        protected List<string> watchingCodes_;
        public FutureState()
        {
            // 거래소 (CME / CBOT / NYMEX / COMEX 마다 돈 내므로 
            // 아래 상품 소개 사이트를 보고 잘 선택 할것.
            //https://fx.kiwoom.com/fxk.templateFrameSet.do
            switch (PublicFutureVar.해선_종목_확장) {
                case PublicFutureVar.해선_종목.ONLY_CME: {
                    string[] CODE = {
                 //       "NQ",       // 미국 나스닥
                        "MNQ",      // 미국 마이크로 나스닥
                 //       "ES",       // 미국 S&P 500
                        "MES",      // 미국 마이크로 sp500
                 //       "M2KM",     // 미국 러셀 2000
                 //       "NKD",      // 닛케이 255 달러
                //      금리는 좀....

                        "6A",       // 호주$
                        "6B",       // 영국 파운드
                        "6C",       // 캐나다 달러       
                        "6E",       // 유로
                        "6J",       // 일본 엔
                        "6N",       // 뉴질랜드 달러
                  //      "6M",       // 멕시코 페소
                        "6S",       // 스위스 프랑
                    };

                    watchingCodes_ = new List<string>(CODE);
                }
                break;
                case PublicFutureVar.해선_종목.ONLY_COMEX: {
                    string[] CODE = {
                        //COMEX
                            //    국채는 채권 공시 가격 표기법으로 로딩됨...
                        "ZN",       // 미 10년 국채             CME
                        "ZF",       // 미 5년 국채              CME
                        "ZT",       // 미 2년 국채              CME

                        "GC",       // 골드             CME
                        "QO",       // 골드 미니        CME
                        "SI",       // 은        // tick value 가 너무 큰데       CME
                        "HG",       // 구리           // tick value 가 너무 큰데 CME
                        "PL",       // 플레티늄     CME
                    };

                    watchingCodes_ = new List<string>(CODE);
                }
                break;

                case PublicFutureVar.해선_종목.ONLY_NYMEX: {
                    string[] CODE = {
                        //NYMEX
                        "BZ",       // 브랜트 오일        CME
                        "CL",       // 크루드 오일           CME
                        "HO",       // 가솔린                  CME
                        "NG",       // 천연가스                 CME
                    };

                    watchingCodes_ = new List<string>(CODE);
                }
                break;

                case PublicFutureVar.해선_종목.일반: {
                    string[] CODE = {
                        "NQ",       // 나스닥           CME
                        "ES",       // 미 S&P 500

                        "6A",       // 호주$
                        "6B",       // 영국 파운드
                        "6C",       // 캐나다 달러       
                        "6E",       // 유로
                        "6J",       // 일본 엔
                        "6N",       // 뉴질랜드 달러

                        "GC",       // 골드
                        "SI",       // 은        // tick value 가 너무 큰데
                        "HG",       // 구리           // tick value 가 너무 큰데
                        "PL",       // 플레티늄

                        "BZ",       // 브랜트 오일
                        "CL",       // 크루드 오일
                        "HO",       // 가솔린
                        "NG",       // 천연가스
                    };

                    watchingCodes_ = new List<string>(CODE);
                }
                break;             
            }
        }

        ~ FutureState()
        {
            watchingCodes_ = null;
        }
        protected bool watctingCode(string code)
        {
            foreach (var wathcing in watchingCodes_) {
                if (code == wathcing) {
                    return true;
                }
            }
            return false;
        }
    }

    class ListUpFuturePhase: FutureState
    {
        readonly FutureBot bot_ = ControlGet.getInstance.futureBot();
        public override void process()
        {
            bot_.clearStockPool();
            bot_.requestMargineInfo();

            FutureDlg dlg = Program.happyFuture_.futureDlg_;
            var checkBox = dlg.checkBox_doTrade;

            if (checkBox.Checked) {
                List<FutureItem> codeList = bot_.engine_.requestFutureList(watchingCodes_);

                foreach (var fItem in codeList) {
                    if (this.watctingCode(fItem.arti_code_)) {
                        FutureData futureData = new FutureData(fItem.code_, fItem.arti_hnm_);

                        bot_.addStockData(futureData);
                        bot_.engine_.requestFutureInfo(futureData.code_);
                        Thread.Sleep(100);

                        bot_.loadFromDB(futureData);
                    }
                }

                bot_.copyRefStocks();
            } else {
                var codeList = watchingCodes_;

                foreach (var code in codeList) {
                    FutureData futureData = new FutureData(code, code);

                    bot_.addStockData(futureData);
                    bot_.loadFromDBPriceAt(futureData);
                    bot_.setFutureDataInfo(futureData);
                }
            }
            this.nextStep_ = true;
        }
    }

    //---------------------------------------------------------------------
    // 리스트 업한 주식에서 실제 볼 주식들만 고르는 처리
    class SelectedPriceLoadFuturePhase: FutureState
    {
        readonly FutureBot bot_ = ControlGet.getInstance.futureBot();

        public SelectedPriceLoadFuturePhase()
        {
            this.bot_.requestMyAccountInfo();
         //   this.bot_.loadRefStocks();
            this.bot_.requestOutstandingOrder();
        }

        public override void process()
        {
            base.process();

            this.bot_.checkFullLoadedStocks();
            this.bot_.requestStockBuyCount();
            this.bot_.removeAlmostEndFutures();

            FutureDlg dlg = Program.happyFuture_.futureDlg_;
            dlg.printStatus(string.Format("Update [{0}], [{1}] data complete. ", DateTime.Now.ToLongTimeString(), this.bot_.priceTypeMin()));
            this.nextStep_ = true;
        }
    }

    class ReadyPrepareFuturePhase: FutureState
    {
        readonly FutureBot bot_ = ControlGet.getInstance.futureBot();

        public override void process()
        {
            this.bot_.yesterAccountMoney_ = this.bot_.accountMoney_;
            this.bot_.eachSetMargineInfo();
            this.bot_.filterExchangeCenter();

            FutureDlg dlg = Program.happyFuture_.futureDlg_;
            var checkBox = dlg.checkBox_doTrade;
            this.bot_.selectFundPool();
            this.bot_.initSafeTrade();
            
            var now = DateTime.Now;
            if (checkBox.Checked) {
                this.bot_.searchStockEachBestTradeModule();
                if (now.DayOfWeek == DayOfWeek.Monday) {
                    if (now.Hour < 12) {
                        this.bot_.clearWinRate();
                    }
                }
                this.nextStep_ = true;
            }
            else {
                if (checkBox.InvokeRequired) {
                    checkBox.BeginInvoke(new Action(() => checkBox.Enabled = false));
                } else {
                    checkBox.Enabled = false;
                }

                dlg.setTitle("테스팅 모드, 매매 재개 하려면 다시 재가동 해야 합니다.");
                switch (now.DayOfWeek) {
                    //case DayOfWeek.Saturday:
                    //if (now.Hour < 15) {
                    //    bot_.vaccumPriceDB();
                    //    dlg.button_findFutureModule_Click(null, null);
                    //}
                    //break;
                    //case DayOfWeek.Sunday:
                    //break;
                }
                this.bot_.updatePoolView();
                this.bot_.engine_.quit();
            }
        }
    }

    //---------------------------------------------------------------------
    // 리스트 업한 주식들을 관리 하고 팔고 / 사는 처리
    class WatchingFuturePhase: FutureState
    {
        readonly FutureBot bot_ = ControlGet.getInstance.futureBot();

        TimeWatch lifeWatch_ = null;
        TimeWatch tradeWatch_ = null;
        TimeWatch reflashBuyCountWatch_ = null;
        TimeWatch reflushWatch_ = null;
        public WatchingFuturePhase()
        {
            if (this.bot_.nowStockMarketTime()) {
                // 주문 expired 됬는지 체크
                FutureDlgInfo info = FutureDlgInfo.getInstance;
                info.orderListView_.checkExpired();

                // 분봉 갱신
                this.bot_.checkStocksTradeModule();
                try {
                    if (this.bot_.breakDownTrade()) {
                        Program.happyFuture_.futureDlg_.Button_quit_Click(null, null);
                        return;
                    }
                    this.bot_.checkFutureDataInfo();
                }
                finally {
                    this.bot_.saveToDB();
                    this.bot_.activeTrade();
                }
            } 
            this.bot_.updatePoolView();
            this.setWatch();
        }

        int delaySec_ = 0;
        const int ONE_MIN = 60;
        // 다음 분봉 갱신 시간에 맞춰 이 state의 시간을 정한다.
        void setWatch()
        {
            var now = DateTime.Now;
            int termMin = this.bot_.priceTypeMin();     // 너무 자주 하지 않도록 조절

            var temp = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);
            temp = temp.AddSeconds(delaySec_);
            var nextUpdateTime = temp.AddMinutes(termMin - (now.Minute % termMin));
            var el = nextUpdateTime - now;

            int spanSecond_ = (int) (el.TotalSeconds);
            if (spanSecond_ < 0) {
                Logger.getInstance.print(KiwoomCode.Log.에러, "다음 tick 계산에 에러가 발생. spanSecod : {0}", spanSecond_);
                spanSecond_ = termMin * ONE_MIN;
            }
            this.lifeWatch_ = new TimeWatch(spanSecond_ * 3);
            this.tradeWatch_ = new TimeWatch(termMin * ONE_MIN);
            this.reflashBuyCountWatch_ = new TimeWatch(termMin * ONE_MIN - delaySec_);
            this.reflushWatch_ = new TimeWatch(ONE_MIN);
        }

        void marketTime()
        {
            FutureDlgInfo dlg = FutureDlgInfo.getInstance;
            dlg.printNextUpdateTime(this.lifeWatch_);

            // 새 분봉 그릴 타이밍인지 체크
            if (this.lifeWatch_.isTimeOver()) {
                this.nextStep_ = true;
                return;
            }

            if (reflashBuyCountWatch_ != null) {
                if (this.reflashBuyCountWatch_.isTimeOver()) {
                    // 주문 expired 됬는지 체크
                    FutureDlgInfo info = FutureDlgInfo.getInstance;
                    info.orderListView_.checkExpired();

                    this.bot_.requestStockBuyCount();
                    this.bot_.requestOutstandingOrder();
                    this.reflashBuyCountWatch_ = null;
                }
            }

            if (this.tradeWatch_.isTimeOver()) {
                this.tradeWatch_.reset();
                reflashBuyCountWatch_ = new TimeWatch(this.bot_.priceTypeMin() * ONE_MIN - delaySec_);

                if (this.bot_.breakDownTrade()) {
                    Program.happyFuture_.futureDlg_.Button_quit_Click(null, null);
                    return;
                }
                this.bot_.updatePoolView();
            }

            if (this.reflushWatch_.isTimeOver()) {
                this.reflushWatch_.reset();

                this.bot_.saveToDB();

                // 트레이딩
                // 먼저 구입한것들 부터 먼저 처리
                this.bot_.tradeForPayoff(true);

                // payoff 즉 청산이 이루어졌으면 buyCount 를 다시 갖고 온다.
                if (bot_.havePayOffStock_) {
                    this.bot_.requestStockBuyCount();
                }
                this.bot_.checkFutureDataInfo();

                // 다시 매매 시도
                this.bot_.tradeForBuy(true);
                bot_.havePayOffStock_ = false;

                this.bot_.updatePoolView();
            }
        }

        void notMarketTime()
        {
         //   this.bot_.checkShutdownTime();
        }

        public override void process()
        {
            base.process();

            // 마켓 시장 시간대일때랑 아닐때 구분
            if (this.bot_.nowStockMarketTime()) {
                this.marketTime();
            }
            else {
                this.notMarketTime();
            }
        }
    }

    class FutureBotState: BotState
    {
        public override void start()
        {
            // 1. 계좌 리스트 업을 먼저 시킴
            this.phase_ = new ListUpFuturePhase();
        }

        public override void changePhase(Phase newPhase)
        {
            base.changePhase(newPhase);
            FutureDlg dlg = Program.happyFuture_.futureDlg_;

            string log = string.Format("현재 상태: [{0}]", this.nowPhaseName());
            dlg.setTitle(log);
        }

        bool initStateFlag_ = false;
        public void initState()
        {
            if (this.phase_ == null) {
                this.changePhase(new ListUpFuturePhase());
                return;
            }
            Type stateType = this.phase_.GetType();
            if (stateType == typeof(WatchingFuturePhase)) {
                this.changePhase(new ListUpFuturePhase());
                return;
            }

            initStateFlag_ = true;
        }

        bool searchingInit_ = true;
        public override void process()
        {
            try {
                var bot = ControlGet.getInstance.futureBot();
                if (bot.activate_ == false) {
                    return;
                }
                if (this.phase_ == null) {
                    return;
                }
                if (initStateFlag_) {
                    initStateFlag_ = false;
                    this.changePhase(new ListUpFuturePhase());
                }

                this.phase_.process();
                Type stateType = this.phase_.GetType();
                //상태 변화 시킴
                if (this.phase_.nextStep()) {
                    if (stateType == typeof(ListUpFuturePhase)) {
                        this.changePhase(new SelectedPriceLoadFuturePhase());
                        return;
                    }
                    if (stateType == typeof(SelectedPriceLoadFuturePhase)) {
                        // 4. 워칭 모드로 전환
                        if (this.searchingInit_) {
                            this.searchingInit_ = false;
                            this.changePhase(new ReadyPrepareFuturePhase());
                        }
                        else {
                            this.changePhase(new WatchingFuturePhase());
                        }
                        return;
                    }
                    if (stateType == typeof(ReadyPrepareFuturePhase)) {
                        this.changePhase(new WatchingFuturePhase());
                        return;
                    }
                    if (stateType == typeof(WatchingFuturePhase)) {
                        this.changePhase(new SelectedPriceLoadFuturePhase());
                        return;
                    }
                }
            }
            catch (Exception e) {
                Logger.getInstance.print(KiwoomCode.Log.에러, "{0} 에서 에러. {1}/{2}", this.phase_.GetType().Name, e.Message, e.StackTrace);
            }
        }
    }
}
