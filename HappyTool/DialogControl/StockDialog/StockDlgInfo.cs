using HappyTool.Dlg;
using HappyTool.FundManagement;
using HappyTool.Stock;
using StockLibrary;
using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using UtilLibrary;

namespace HappyTool.DialogControl.StockDialog
{
    class StockDlgInfo :SingleTon<StockDlgInfo>
    {
        ControlPrint nextUpdateTime_;
        readonly ControlPrint configInfo_;
        readonly ControlPrint cpuInfo_;
        readonly ControlPrint memInfo_;
        OrderListView orderListView_;

        internal TodayTradeRecoderView tradeRecoderView_ = new TodayTradeRecoderView();
        internal TradeHistoryView tradeHistoryView_ = new TradeHistoryView();
        StockDlgInfo()
        {
            StockDlg stockDlg = Program.happyTool_.stockDlg_;
            orderListView_ = new OrderListView();
            nextUpdateTime_ = new ControlPrint(stockDlg.label_nextUpdate);
            this.configInfo_ = new ControlPrint(stockDlg.lable_configInfo);
            this.cpuInfo_ = new ControlPrint(stockDlg.label_cpuInfo);
            this.memInfo_ = new ControlPrint(stockDlg.label_memInfo);
        }

        public void setup()
        { 
            StockDlg stockDlg = Program.happyTool_.stockDlg_;
            this.tradeRecoderView_.setup(stockDlg.DataGridView_tradeRecoder);
            this.tradeHistoryView_.setup(stockDlg.DataGridView_tradeHistory);
        }

        public void setNextUpdateTime(TimeWatch remain)
        {
            var remainSec = remain.remainSec();
            this.nextUpdateTime_.print(string.Format("다음 업데이트 {0}분 {1}초", remainSec / 60, remainSec % 60));
        }

        //-------------------------------------------------------------//
        // 주식 정보를 출력. 1초에 1번 깜빡이게...넘 느린거 아닌가..
        public void updateBuyPoolView()
        {
            orderListView_.updateScreen();
        }
     
        public void sellForce(object sender, EventArgs e)
        {
            orderListView_.sellForce();
        }

        public void printConfigInfo()
        {
            string log = string.Empty;
            var bot = ControlGet.getInstance.stockBot();
            var fundManage = bot.fundManagement_ as StockFundManagement;
            log += string.Format("원칙 1: 절대로 돈을 잃지 마라 (Never lose money).\n");
            log += string.Format("원칙 2: 절대 1번 원칙을 잊지 말아라(Never forget rule No.1)\n");

            log += string.Format("현재 전략 : {0}\n", fundManage.name());
                        
            log += string.Format("매매 엔진 모드: {0} \n", bot.stockModuleSearcher_.defaultMode_);
            log += string.Format("매매 전략 모드: {0} \n", PublicVar.fundManageStrategy);

            log += string.Format("즉시 매매 모드: {0} \n", PublicVar.immediatelyOrder ? "YES" : "NO");

            log += string.Format("목표 이율: {0:##.##} %\n", PublicVar.profitRate * 100);
            log += string.Format("손절 이율: {0:##.##} %\n", PublicVar.loseCutRate * 100);

            this.configInfo_.print(log);
        }

        public void printCpuMemInfo()
        {
            var bot = ControlGet.getInstance.stockBot();
            string log = string.Format("CPU : {0:##0.#####} %", bot.usesCpu());
            this.cpuInfo_.print(log);
            log = string.Format("MEM : {0:##,###} MB", bot.usesMem());
            this.memInfo_.print(log);
        }

        //--------------------------------------------------------------------------//
        // 화면 캡쳐
        void makeCapture(string fileName)
        {
            try {
                var dlg = Program.happyTool_.stockDlg_;
                Bitmap bitmap = new Bitmap(dlg.Width, dlg.Height);
                dlg.DrawToBitmap(bitmap, new Rectangle(0, 0, dlg.Width, dlg.Height));
                bitmap.Save(fileName, System.Drawing.Imaging.ImageFormat.Jpeg);
                bitmap.Dispose();
            } catch(Exception e) {
                string log = string.Format("{0}\n{1}", e.Message, e.StackTrace);
                Logger.getInstance.print(KiwoomCode.Log.에러, log);
            }
        }

        void formCapture(string fileName)
        {
            var dlg = Program.happyTool_.stockDlg_;
            if (dlg.InvokeRequired) {
                dlg.BeginInvoke(new Action(() => this.makeCapture(fileName)));
            } else {
                this.makeCapture(fileName);
            }
            // 이미지 만들때까지 대기
            Thread.Sleep(100);
        }

        public string captureFormPath()
        {
            string path = Application.StartupPath + "\\capture";
            DirectoryInfo di = new DirectoryInfo(path);
            if (di.Exists == false) {
                di.Create();
            }

            return string.Format("{0}\\StockDlg.jpg", path);
        }

        public string captureForm()
        {
            try {
                string fileName = this.captureFormPath();
                formCapture(fileName);
                return fileName;
            }
            catch (Exception e) {
                string log = string.Format("{0}\n{1}", e.Message, e.StackTrace);
                Logger.getInstance.print(KiwoomCode.Log.에러, log);
                ControlGet.getInstance.stockBot().telegram_.sendMessage(log);
                return "";
            }
        }
    }
}
