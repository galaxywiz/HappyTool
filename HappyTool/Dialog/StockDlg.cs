using HappyTool.DialogControl.StockDialog;
using HappyTool.DlgControl;
using HappyTool.Stock;
using HappyTool.Util;
using System;
using System.Windows.Forms;
using UtilLibrary;
using StockLibrary;
using System.Threading;

namespace HappyTool.Dlg
{
    public partial class StockDlg :Form
    {
        public StockBot bot_ = null;
        public StockDlg()
        {
            InitializeComponent();
        }

        private void StockDlg_Shown(object sender, EventArgs e)
        {
            bot_ = ControlGet.getInstance.stockBot();
            StockDlgInfo.getInstance.setup();
            Progresser.getInstance.setup(progressBar);

            SimulateRecoderInfo backTest = SimulateRecoderInfo.getInstance;
            backTest.setup();

            this.setTimer();
        }

        void setTimer()
        {
            timer_clock.Interval = 1000; // 1초
            timer_clock.Start();
        }

        public QuickDataGridView dataGridView_Stock()
        {
            return DataGridView_StockPool;
        }

        public void updateAccountMoney(double money)
        {
            if (Label_money.InvokeRequired) {
                Label_money.BeginInvoke(new Action(() => Label_money.Text = money.ToString("#,##")));
            } else {
                Label_money.Text = money.ToString("#,###");
            }
            if (bot_ != null) {
                bot_.accountMoney_ = money;
            }
        }

        public Label userId()
        {
            return Label_id;
        }

        public Label account()
        {
            return Label_account;
        }

        public Label money()
        {
            return Label_money;
        }

        public void setTitle(string title)
        {
            if (this.InvokeRequired) {
                this.BeginInvoke(new Action(() => this.Text = title));
            } else {
                this.Text = title;
            }
        }

        public void printStatus(string status)
        {
            if (statusStrip_Stock.InvokeRequired) {
                statusStrip_Stock.BeginInvoke(new Action(() => toolStripStatus_stockInfo.Text = status));
            } else {
                toolStripStatus_stockInfo.Text = status;
            }
        }

        enum TAB_NAME
        {
            주식목록,            
            HTS수익이력,
            시뮬레이션,
            믿음,
        }

        private void tabControl_stockDlg_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selIdx = this.tabControl_stockDlg.SelectedIndex;
            var dlgInfo = StockDlgInfo.getInstance;

            switch (selIdx) {
                case (int) TAB_NAME.주식목록:
                    this.bot_.updatePoolView();
                    break;
                case (int) TAB_NAME.HTS수익이력:
                    dlgInfo.tradeRecoderView_.update();
                    dlgInfo.tradeHistoryView_.update();
                    break;
                case (int) TAB_NAME.시뮬레이션:
                    break;  
                case (int) TAB_NAME.믿음:
                    dlgInfo.printConfigInfo();
                    break;
            }
        }

        private void DataGridView_StockPool_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            StockPoolViewer.getInstance.cellClick(sender, e);
        }

        private void DataGridView_history_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            SimulateRecoderInfo.getInstance.cellClick(sender, e);
        }

        public void Button_quit_Click(object sender, EventArgs e)
        {
            var telegram = bot_.telegram_;
            telegram.sendMessage("행복의 도구 종료");
            while (true) {
                if (telegram.messageCount() == 0) {
                    break;
                }
                // 메시지 다 보낼때까지 대기
                Thread.Sleep(1000);
            }
            Program.happyTool_.quit();
        }

        //----------------------------------------------------------------------------------
        // 다이얼 로그 버튼들

        private void button_reload_Click(object sender, EventArgs e)
        {
            bot_.removeBuyStock();
            bot_.requestMyAccountInfo();
        }

        private void button_force_trade_Click(object sender, EventArgs e)
        {
            bot_.tradeForPayoff();
            bot_.tradeForBuy();
        }

        private void button_menual_all_sell_Click미미(object sender, EventArgs e)
        {
            var bot = ControlGet.getInstance.stockBot();
            StockDataEach eachDo = (string code, StockData stockData) => {
                if (stockData.isBuyedItem()) {
                    bot.payOff(code, "일괄 강제 청산");
                }
            };

            if ((MessageBox.Show("모두 청산", "강제 매도 확인", MessageBoxButtons.YesNo) == DialogResult.Yes)) {
                bot.doStocks(eachDo);
            }
        }

        private void button_menual_sell_Click(object sender, EventArgs e)
        {
            StockDlgInfo.getInstance.sellForce(sender, e);
        }

        private void timer_clock_Tick(object sender, EventArgs e)
        {
            string log = string.Format("{0}\n", DateTime.Now.ToLongTimeString());
            log += string.Format("현재 보유주 이익\n{0:##,###0} 원\n", this.bot_.nowTotalProfit_);
            log += string.Format("금일 이익\n{0:##,###0} 원\n", this.bot_.todayTotalProfit_);
            //log += this.bot_.logForTradeWinRate();
            this.label_timer.Text = log;

            var dlgInfo = StockDlgInfo.getInstance;
            dlgInfo.printCpuMemInfo();
        }

        private void dateTimePicker_end_ValueChanged(object sender, EventArgs e)
        {

        }

        private void dateTimePicker_start_ValueChanged(object sender, EventArgs e)
        {

        }
    }
}
