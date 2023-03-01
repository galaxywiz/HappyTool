using HappyFuture.Dialog;
using HappyFuture.FundManage;
using StockLibrary;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using UtilLibrary;

namespace HappyFuture.DialogControl.FutureDlg
{
    class FutureDlgInfo: SingleTon<FutureDlgInfo>
    {
        internal FuturePoolViewer futurePoolViewer_ = new FuturePoolViewer();
        internal TradeListView tradeListView_ = new TradeListView();
        internal OrderListView orderListView_ = new OrderListView();
        readonly ControlPrint nextUpdateTime_;
        readonly ControlPrint cpuInfo_;
        readonly ControlPrint memInfo_;
        readonly ControlPrint tradeSearchInfo_;
        readonly ControlPrint configInfo_;
        internal ComboBox priceTypeCombo_;

        internal TodayTradeRecoderView tradeRecoderView_ = new TodayTradeRecoderView();
        internal TradeHistoryView tradeHistoryView_ = new TradeHistoryView();

        internal OrderRecoderView orderRecoderView = new OrderRecoderView();
        readonly ControlCapture capture_;

        FutureDlgInfo()
        {
            var futureDlg = Program.happyFuture_.futureDlg_;

            this.nextUpdateTime_ = new ControlPrint(futureDlg.label_nextUpdate);
            this.cpuInfo_ = new ControlPrint(futureDlg.label_cpuInfo);
            this.memInfo_ = new ControlPrint(futureDlg.label_memInfo);
            this.tradeSearchInfo_ = new ControlPrint(futureDlg.label_serachItem);
            this.configInfo_ = new ControlPrint(futureDlg.lable_configInfo);
            this.capture_ = new ControlCapture(futureDlg);
            
            this.priceTypeCombo_ = futureDlg.comboBox_priceMin;
        }

        public void setDlgColor()
        {
            var futureDlg = Program.happyFuture_.futureDlg_;
            if (PublicVar.reverseOrder) {
                futureDlg.BackColor = Color.MistyRose;
            }
            else {
                futureDlg.BackColor = Color.White;
            }
        }

        public void setup()
        {
            this.tradeListView_.setup();
            this.orderListView_.setup();

            this.setDlgColor();

            var futureDlg = Program.happyFuture_.futureDlg_;
            this.futurePoolViewer_.setup(futureDlg.DataGridView_FuturePool);
            //거래 이력
            this.tradeRecoderView_.setup(futureDlg.DataGridView_tradeRecoder);
            this.orderRecoderView.setup(futureDlg.DataGridView_OrderView);
            this.tradeHistoryView_.setup(futureDlg.DataGridView_tradeHistory);

            this.setPriceTypeCombo();
        }

        void setPriceTypeCombo()
        {
            foreach (PRICE_TYPE priceType in Enum.GetValues(typeof(PRICE_TYPE))) {
                int min = 0;
                switch (priceType) {
                    case PRICE_TYPE.MIN_1:
                    min = PublicVar.priceType_min1;
                    break;
                    case PRICE_TYPE.MIN_2:
                    min = PublicVar.priceType_min2;
                    break;
                    case PRICE_TYPE.MIN_3:
                    min = PublicVar.priceType_min3;
                    break;
                }
                var title = string.Format("{0} 분봉", min);
                priceTypeCombo_.Items.Add(title);
                priceTypeCombo_.SelectedIndex = (int) PublicVar.initType;
            }
        }

        internal void setPriceTypeCombo(EventArgs e)
        {
            FutureBot bot = ControlGet.getInstance.futureBot();
            bot.setPriceMinType((PRICE_TYPE) priceTypeCombo_.SelectedIndex);
            var futureState = bot.botState_ as FutureBotState;
            futureState.initState();
        }

        public void printNextUpdateTime(TimeWatch remain)
        {
            var remainSec = remain.remainSec();
            this.nextUpdateTime_.print(string.Format("다음 업데이트 {0}분 {1}초", remainSec / 60, remainSec % 60));
        }

        public void printCpuMemInfo()
        {
            FutureBot bot = ControlGet.getInstance.futureBot();
            string log = string.Format("CPU : {0:##0.#####} %", bot.usesCpu());
            this.cpuInfo_.print(log);
            log = string.Format("MEM : {0:##,###} MB", bot.usesMem());
            this.memInfo_.print(log);
        }

        public void printConfigInfo()
        {
            string log = string.Empty;
            var bot = ControlGet.getInstance.futureBot();
            var fundManage = bot.fundManagement_ as FutureFundManagement;
            log += string.Format("원칙 1: 절대로 돈을 잃지 마라 (Never lose money).\n");
            log += string.Format("원칙 2: 절대 1번 원칙을 잊지 말아라(Never forget rule No.1)\n");
            log += string.Format("\n=================================================\n");
            log += string.Format("* 매매 전략 모드: {0} \n", PublicVar.fundManageStrategy);
            var fund = bot.fundManagement_;
            log += string.Format(" - 메인 전략: {0}\n", fund.name());
            log += string.Format(" - 진입 전략: {0}\n", fund.strategyModule_.name());

            if (PublicFutureVar.safeTrade) {
                log += string.Format("1일 목표 : {0} %\n", PublicFutureVar.safeProfitTodayTargetRate * 100);
                log += string.Format("1일 shutdown : {0} %\n", PublicFutureVar.safeLostTodayTargetRate * 100);
                log += string.Format("1일 목표 금액: {0:##,###} $\n", PublicFutureVar.safeTargetMoney);
            }
            log += string.Format("분봉 : {0} 분봉 사용 step\n", bot.priceTypeMin());
            log += string.Format("손절배율 = 증거금 * {0} step\n", fundManage.lostCutTime_);
            log += string.Format("손익배율 = 증거금 * {0} step\n", fundManage.targetProfitTime_);
            log += string.Format("올인원 전략: {0}\n", PublicVar.allInOneStrategy);
            log += string.Format("모든 모듈 사용: {0}\n", PublicVar.allTradeModuleUse);

            log += string.Format("매매 엔진 모드: {0} \n", bot.stockModuleSearcher_.defaultMode_);

            log += string.Format("매매 허가 조건: {0} \n", PublicVar.allowTradeCount);
            log += string.Format("매매 승율 조건: {0} \n", PublicVar.allowTradeWinRate);
            log += string.Format("매매 평균 이익: {0} \n", PublicFutureVar.feeTax);

            log += string.Format("즉시 매매 모드: {0} \n", PublicVar.immediatelyOrder ? "YES" : "NO");
            log += string.Format("위탁증거금 대비 예수금 비율: {0} \n", PublicFutureVar.allowTrustMarginTime);
            log += string.Format("expectMax 기대값 {0:##.##}\n", PublicVar.expectTimeMax);
            log += string.Format("expectMin 기대값 {0:##.##}\n", PublicVar.expectTimeMin);
            log += string.Format("일일 매매 : {0}\n", PublicFutureVar.tradePeriod);
            log += string.Format("승리시 내 몫%: {0}%\n", PublicFutureVar.myShareRate);
            this.configInfo_.print(log);
        }

        public void printTradeSerachItem()
        {
            FutureBot bot = ControlGet.getInstance.futureBot();
            string name = bot.serachingStockName();
            string log;
            if (name == string.Empty) {
                if (bot.nowStockMarketTime()) {
                    log = string.Format("쉬는중...");
                }
                else {
                    log = string.Format("마켓이 안열려 있음");
                }
            }
            else {
                log = string.Format("{0} 를 찾는중", name);
            }
            this.tradeSearchInfo_.print(log);
        }

        public void updateBuyPoolView()
        {
            this.tradeListView_.update();
        }

        public void forcePayOff(object sender, EventArgs e)
        {
            this.tradeListView_.forcePayOff(sender, e);
        }

        //--------------------------------------------------------------------------//
        // 화면 캡쳐
        const string futureDlgImg_ = "futureDlg.png";

        public string captureFormImgName()
        {
            string path = Application.StartupPath + "\\capture\\" + futureDlgImg_;
            return path;
        }

        public void caputreForm()
        {
            this.capture_.formCapture(futureDlgImg_);
        }
    }
}
