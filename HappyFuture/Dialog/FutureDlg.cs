using HappyFuture.DialogControl.FutureDlg;
using HappyFuture.TradeStrategyFinder;
using StockLibrary;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using UtilLibrary;

namespace HappyFuture
{
    public partial class FutureDlg: Form
    {
        internal FutureBot bot_ = null;

        public FutureDlg()
        {
            this.InitializeComponent();
            this.setBackTestStartDate(DateTime.Now.AddDays(-365));
            this.dateTimePicker_backTestEnd.Value = DateTime.Now;
        }

        public void setBackTestStartDate(DateTime dateTime)
        {
            var control = this.dateTimePicker_backTestStart;
            if (control.InvokeRequired) {
                control.BeginInvoke(new Action(() => control.Value = dateTime));
            }
            else {
                control.Value = dateTime;
            }
        }

        public void updateAccountMoney(double money)
        {
            if (this.bot_ == null) {
                return;
            }
            if (this.Label_money.InvokeRequired) {
                this.Label_money.BeginInvoke(new Action(() => this.Label_money.Text = money.ToString("#,###0.##") + " $"));
            }
            else {
                this.Label_money.Text = money.ToString("#,###0.##") + " $";
            }

            this.bot_.accountMoney_ = money;
        }

        public void setTitle(string title)
        {
            title = string.Format("행복의 선물 : {0}", title);
            if (PublicVar.reverseOrder) {
                title += "[친구매매 모드]";
            }
            title += this.bot_.logForTradeWinRate();
            title = title.Replace("\n", "");

            if (this.InvokeRequired) {
                this.BeginInvoke(new Action(() => this.Text = title));
            }
            else {
                this.Text = title;
            }
        }

        public void setAccountNumber(string accountMoney)
        {
            var lable = this.Label_account;
            if (lable.InvokeRequired) {
                lable.BeginInvoke(new Action(() => lable.Text = accountMoney));
            }
            else {
                lable.Text = accountMoney;
            }
        }

        public void setTabIndex(int index)
        {
            var tab = this.tabControl_futureDlg;
            if (tab.InvokeRequired) {
                tab.BeginInvoke(new Action(() => tab.SelectedIndex = index));
            }
            else {
                tab.SelectedIndex = index;
            }
        }

        public void printStatus(string status)
        {
            if (this.statusStrip_Future.InvokeRequired) {
                this.statusStrip_Future.BeginInvoke(new Action(() => this.toolStripStatus_stockInfo.Text = status));
            }
            else {
                this.toolStripStatus_stockInfo.Text = status;
            }
        }

        private void FutureDlg_Shown(object sender, EventArgs e)
        {
            FutureDlgInfo.getInstance.setup();
            Progresser.getInstance.setup(this.progressBar);
            this.bot_.engine_.setupProgressBar(this.progressBar_engine);
            if (this.bot_.test_) {
                this.checkBox_doTrade.Checked = false;
            }
            else {
                this.checkBox_doTrade.Checked = true;
            }
            this.checkBox_reverse.Checked = PublicVar.reverseOrder;
            this.setTimer();
        }

        void setTimer()
        {
            this.timer_clock.Interval = 1000; // 1초
            this.timer_clock.Start();
        }

        public void Button_quit_Click(object sender, EventArgs e)
        {
            if (this.bot_ != null) {
                this.bot_.quit();
            }

            Program.happyFuture_.quit();
        }

        public enum DLG_TAB_NAME
        {
            선물목록,
            주문이력,
            HTS수익이력,
            시뮬레이션,
            믿음,
        }

        private void tabControl_futureDlg_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selIdx = this.tabControl_futureDlg.SelectedIndex;
            var dlgInfo = FutureDlgInfo.getInstance;

            switch (selIdx) {
                case (int) DLG_TAB_NAME.선물목록:
                    this.bot_.updatePoolView();
                    break;
                case (int) DLG_TAB_NAME.주문이력:
                    dlgInfo.orderRecoderView.update();
                    break;
                case (int) DLG_TAB_NAME.HTS수익이력:
                    dlgInfo.tradeRecoderView_.update();
                    dlgInfo.tradeHistoryView_.update();
                    break;
                case (int) DLG_TAB_NAME.믿음:
                    dlgInfo.printConfigInfo();
                    break;
            }
        }

        private void DataGridView_FuturePool_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
        {
            FutureDlgInfo.getInstance.futurePoolViewer_.cellMouseUp(sender, e);
        }

        private void button_reload_Click(object sender, EventArgs e)
        {
            this.bot_.requestMyAccountInfo();
        }

        private void button_manual_sell_Click(object sender, EventArgs e)
        {
            FutureDlgInfo.getInstance.forcePayOff(sender, e);
        }

        readonly TimeWatch captureWatch_ = new TimeWatch(30);
        private void timer_clock_Tick(object sender, EventArgs e)
        {
            string time = string.Format("{0}\n", DateTime.Now.ToLongTimeString());
            time += string.Format("현황: {0:##,###0.#####}$ / {1:##.##}%\n", this.bot_.nowTotalProfit_, this.bot_.todayTotalProfitRate_);
            time += this.bot_.logForTradeWinRate();
            this.label_timer.Text = time;

            var dlgInfo = FutureDlgInfo.getInstance;
            dlgInfo.printCpuMemInfo();
            dlgInfo.printTradeSerachItem();

            if (this.captureWatch_.isTimeOver()) {
                dlgInfo.caputreForm();
                this.captureWatch_.reset();
            }
        }

        public void setButtonFindFutureModuleEnable(bool flag)
        {
            var button = button_findFutureModule;
            if (button.InvokeRequired) {
                button.BeginInvoke(new Action(() => button.Enabled = flag));
            }
            else {
                button.Enabled = flag;
            }
        }

        public void button_findFutureModule_Click(object sender, EventArgs e)
        {
            this.bot_.findEachFundmanagementModule();
            this.setButtonFindFutureModuleEnable(false);
        }

        //----------------------------------------------------------------------
        // HTS
        private void dateTimePicker_start_ValueChanged(object sender, EventArgs e)
        {
            var dlgInfo = FutureDlgInfo.getInstance;
            dlgInfo.tradeHistoryView_.update();
        }

        private void dateTimePicker_end_ValueChanged(object sender, EventArgs e)
        {
            var dlgInfo = FutureDlgInfo.getInstance;
            dlgInfo.tradeHistoryView_.update();
        }

        //----------------------------------------------------------------------
        // 매매 기록
        private void dateTimePicker_OrderStart_ValueChanged(object sender, EventArgs e)
        {
            var dlgInfo = FutureDlgInfo.getInstance;
            dlgInfo.orderRecoderView.update();
        }
                
        private void dateTimePicker_OrderEnd_ValueChanged(object sender, EventArgs e)
        {
            var dlgInfo = FutureDlgInfo.getInstance;
            dlgInfo.orderRecoderView.update();
        }

        private void button_exportExcel_Click(object sender, EventArgs e)
        {
            var dlgInfo = FutureDlgInfo.getInstance;
            dlgInfo.orderRecoderView.exportExcel();
        }

        //----------------------------------------------------------------------
        // 시뮬레이션

        BackTestEngine machine_ = null;
        private void button_totalTradeSimul_Click(object sender, EventArgs e)
        {
            this.machine_ = new BackTestEngine(this.dateTimePicker_backTestStart.Value, this.dateTimePicker_backTestEnd.Value);
            this.machine_.run();
        }

        private void button_backTestDBSimul_Click(object sender, EventArgs e)
        {
            this.machine_ = new BackTestEngine(this.dateTimePicker_backTestStart.Value, this.dateTimePicker_backTestEnd.Value);
            this.machine_.run(false);
        }

        List<BackTestEngine> machines_ = new List<BackTestEngine>();
        private void button_totalEachSimul_Click(object sender, EventArgs e)
        {
            StockDataEach eachDo = (string code, StockData stockData) => {
                var machine = new BackTestEngine(stockData);
                machines_.Add(machine);
                machine.run();
            };
            bot_.doStocks(eachDo);
        }


        private void button_TestCancel_Click(object sender, EventArgs e)
        {
            if (this.machine_ != null) {
                this.machine_.processingTest_ = false;
            }
            else {
                foreach (var vm in machines_) {
                    vm.processingTest_ = false;
                }
            }
        }

        private void button_allPayOff_Click(object sender, EventArgs e)
        {
            var bot = ControlGet.getInstance.futureBot();
            bot.allPayOff();
        }

        void setEnableButton(Control button, bool flag)
        {
            if (button.InvokeRequired) {
                button.BeginInvoke(new Action(() => button.Enabled = flag));
            }
            else {
                button.Enabled = flag;
            }
        }

        public void setEnableTestButtons(bool flag)
        {
            this.setEnableButton(this.button_backTestTotalTradeSimul, flag);
            this.setEnableButton(this.button_allPayOff, flag);
            this.setEnableButton(this.button_TestCancel, !flag);
            this.setEnableButton(this.checkBox_doTrade, flag);
            this.setEnableButton(this.button_moduleClear, flag);
            this.setEnableButton(this.button_backTestDBSimul, flag);
            this.setEnableButton(this.button_totalEachSimul, flag);
        }

        private void button_moduleClear_Click(object sender, EventArgs e)
        {
            var bot = ControlGet.getInstance.futureBot();
            bot.deleteAllTradeModuleList();
        }

        private void Button_configReload_Click(object sender, EventArgs e)
        {
            PublicVar.readConfig();
            PublicFutureVar.readConfig2();
            var dlgInfo = FutureDlgInfo.getInstance;
            dlgInfo.printConfigInfo();
        }

        private void Button_reloadDB_Click(object sender, EventArgs e)
        {
            var bot = ControlGet.getInstance.futureBot();
            bot.setFundManagement();
        }

        private void Button_resetWinRate_Click(object sender, EventArgs e)
        {
            var bot = ControlGet.getInstance.futureBot();
            bot.clearWinRate();
        }

        private void CheckBox_reverse_CheckedChanged(object sender, EventArgs e)
        {
            if (PublicVar.reverseOrder == false) {
                var result = MessageBox.Show(new Form() { WindowState = FormWindowState.Normal, TopMost = true },
                                      "정말로 주문 모드 반전 시킵니까? 기존 보유한 종목은 즉시 처분 됩니다.",
                                      "수동으로 반전?",
                                      MessageBoxButtons.YesNo,
                                      MessageBoxIcon.Warning);
                if (result == DialogResult.No) {
                    checkBox_reverse.Checked = PublicVar.reverseOrder;
                    return;
                }
            }
            var bot = ControlGet.getInstance.futureBot();
            bot.allPayOff();

            PublicVar.reverseOrder = checkBox_reverse.Checked;
            FutureDlgInfo.getInstance.setDlgColor();

            checkBox_reverse.Checked = PublicVar.reverseOrder;
        }

        private void comboBox_priceMin_SelectedIndexChanged(object sender, EventArgs e)
        {
            FutureDlgInfo.getInstance.setPriceTypeCombo(e);
        }

        private void FutureDlg_DragDrop(object sender, DragEventArgs e)
        {
            var files = (string[]) e.Data.GetData(DataFormats.FileDrop);
            var bot = ControlGet.getInstance.futureBot();

            foreach (string file in files) {
                bot.loadFromFile(file);
            }

            bot.saveToDB(true);
        }

        private void FutureDlg_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                e.Effect = DragDropEffects.Copy;
            }
        }

      
    }
}
