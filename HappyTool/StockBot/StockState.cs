using HappyTool.DialogControl.StockDialog;
using HappyTool.Dlg;
using StockLibrary;
using System;
using UtilLibrary;

namespace HappyTool.Stock
{
    class StockPhase : Phase
    {
        protected StockBot bot_ = ControlGet.getInstance.stockBot();
    }

    // listup pre 
    // 일단위로 적합한거 불러오기
    // 저장된거 있으면 먼저 로딩, 없으면 생성

    class ListUpStockPhase: StockPhase
    {
        public ListUpStockPhase()
        {
            bot_.requestMyAccountInfo();
            bot_.loadYesterDayFilterList();
            if (bot_.stockPoolCount() < 10) {
                bot_.requestHighTradingStocks();
               // bot_.requestAgencyStocks();
            }
        }
        public override void process()
        {
            nextStep_ = true;
        }
    }

    //---------------------------------------------------------------------
    // 리스트 업한 주식에서 실제 볼 주식들만 고르는 처리
    class SelectedPriceLoadStockPhase: StockPhase
    {
        public SelectedPriceLoadStockPhase()
        {
        }

        public override void process()
        {
            if (bot_.checkFullLoadedStocks() == false) {
                return;
            }
            //bot_.trimStocks();
            bot_.updatePoolView();

            StockDlg dlg = Program.happyTool_.stockDlg_;
            dlg.printStatus(string.Format("Update [{0}], [{1}] data complete. ", DateTime.Now.ToLongTimeString(), bot_.priceTypeMin()));

            nextStep_ = true;
        }
    }

    //---------------------------------------------------------------------
    // 리스트 업한 주식들을 관리 하고 팔고 / 사는 처리
    class WatchingStockPhase : StockPhase
    {
        TimeWatch updateWatch_ = null;
        public WatchingStockPhase()
        {
            bot_.activeTrade();

            // 다 되었으면 매매 할 수있는지 체크
            bot_.tradeForPayoff();
            bot_.tradeForBuy();

            this.setWatch();
            bot_.updatePoolView();
        }

        int spanSecond_ = 0;
        const int ONE_MIN = 60;

        // 다음 분봉 갱신 시간에 맞춰 이 state의 시간을 정한다.
        void setWatch()
        {
            const int DELAY_SEC = 10;
            var now = DateTime.Now;
            int min = this.bot_.priceTypeMin();

            var temp = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, DELAY_SEC);
            var nextUpdateTime = temp.AddMinutes(min - (now.Minute % min));
            var el = nextUpdateTime - now;

            spanSecond_ = (int) (el.TotalSeconds);
            if (spanSecond_ < 0) {
                Logger.getInstance.print(KiwoomCode.Log.에러, "다음 tick 계산에 에러가 발생. spanSecod : {0}", spanSecond_);
                spanSecond_ = min * ONE_MIN;
            }
            this.updateWatch_ = new TimeWatch(spanSecond_);
        }

        TimeWatch mainDlgCaptureWatch_ = new TimeWatch(60);
        // 주식 시간대 처리
        void marketTime()
        {
            StockDlg dlg = Program.happyTool_.stockDlg_;
            StockDlgInfo.getInstance.setNextUpdateTime(updateWatch_);

            if (updateWatch_.isTimeOver()) {
                bot_.clearStockDataRecvedTime();
                nextStep_ = true;
                return;
            }

            if (mainDlgCaptureWatch_.isTimeOver()) {
                StockDlgInfo.getInstance.captureForm();
                mainDlgCaptureWatch_.reset();
            }
        }

        // 장외 시간때 처리
        void notMarketTime()
        {
            // 웹으로 데이터 수집....
            //bot_.getRealPriceFromWeb();

            // 결산 
            bot_.reportToday();

            // 자동 종료 
            bot_.checkShutdownTime();
        }

        public override void process()
        {
            base.process();

            // 마켓 시장 시간대일때랑 아닐때 구분
            if (bot_.nowStockMarketTime()) {
                this.marketTime();
            }
            else {
                this.notMarketTime();
            }
        }
    }

    public class StockBotState : BotState
    {
        StockBot bot_ = null;
        public StockBotState(StockBot bot)
        {
            bot_ = bot;
        }
        DateTime listUpdateTime_ = DateTime.Now;

        public override void start()
        {
            // 1. 계좌 리스트 업을 먼저 시킴
            phase_ = new ListUpStockPhase();
        }

        public override void changePhase(Phase newPhase)
        {
            base.changePhase(newPhase);

            StockDlg dlg = Program.happyTool_.stockDlg_;
            string log = string.Format("현재 상태 : [{0}]", phase_.GetType().Name);
            dlg.setTitle(log);
        }

        bool searchingInit_ = false;
        public override void process()
        {
            if (phase_ == null) {
                return;
            }

            phase_.process();
            //상태 변화 시킴
            
            if (phase_.nextStep()) {
                Type stateType = phase_.GetType();
                if (stateType == typeof(ListUpStockPhase)) {
                    this.changePhase(new SelectedPriceLoadStockPhase());
                    return;
                }
                if (stateType == typeof(SelectedPriceLoadStockPhase)) {
                    // 4. 워칭 모드로 전환
                    this.changePhase(new WatchingStockPhase());
                    if (searchingInit_ == false) {
                        searchingInit_ = true;
                        bot_.searchStockEachBestTradeModule();
                    }
                    return;
                }
                if (stateType == typeof(WatchingStockPhase)) {
                    this.changePhase(new SelectedPriceLoadStockPhase());
                    return;
                }
            }
        }
    }
}
