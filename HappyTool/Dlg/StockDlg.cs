using HappyTool.DlgControl;
using HappyTool.Stock;
using HappyTool.Stock.Calculate;
using HappyTool.Util;
using System;
using System.Windows.Forms;

namespace HappyTool.Dlg
{
    public partial class StockDlg :Form
    {
        public StockDlg()
        {
            InitializeComponent();
        }

        private void StockDlg_Shown(object sender, EventArgs e)
        {
            PRICE_TYPE priceType = StockBot.getInstance.priceType_;
            switch (priceType) {
                case PRICE_TYPE.DAY: radioButton_day.Checked = true; break;
                case PRICE_TYPE.WEEK: radioButton_week.Checked = true; break;
                case PRICE_TYPE.HOUR1: radioButton_1hour.Checked = true; break;
                case PRICE_TYPE.HOUR4: radioButton_4hour.Checked = true; break;
                case PRICE_TYPE.MIN15: radioButton_15min.Checked = true; break;
            }
            BackTestInfo backTest = BackTestInfo.getInstance;
            backTest.setup();
        }

        public QuickDataGridView dataGridView_Stock()
        {
            return DataGridView_StockPool;
        }

        public void updateAccountMoney(int money)
        {
            if (Label_money.InvokeRequired) {
                Label_money.BeginInvoke(new Action(() => Label_money.Text = money.ToString("#,##")));
            } else {
                Label_money.Text = money.ToString("#,##");
            }
            StockBot.getInstance.accountMoney_ = money;
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

        private void DataGridView_StockPool_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            StockPoolViewer.getInstance.cellClick(sender, e);
        }

        private void Button_quit_Click(object sender, EventArgs e)
        {
            Program.happyTool_.quit();
        }

        // 다이얼 로그 버튼들
        private void changeRadio(PRICE_TYPE priceType)
        {
            StockBot.getInstance.priceType_ = priceType;
            StockBot.getInstance.drawStockView();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            this.changeRadio(PRICE_TYPE.MIN15);
        }

        private void radioButton_1hour_CheckedChanged(object sender, EventArgs e)
        {
            this.changeRadio(PRICE_TYPE.HOUR1);
        }

        private void radioButton_4hour_CheckedChanged(object sender, EventArgs e)
        {
            this.changeRadio(PRICE_TYPE.HOUR4);
        }

        private void radioButton_day_CheckedChanged(object sender, EventArgs e)
        {
            this.changeRadio(PRICE_TYPE.DAY);
        }

        private void radioButton_week_CheckedChanged(object sender, EventArgs e)
        {
            this.changeRadio(PRICE_TYPE.WEEK);
        }

        private void comboBox_BackTest_SelectedIndexChanged(object sender, EventArgs e)
        {
            int sel = comboBox_BackTest.SelectedIndex;
            BackTestInfo.getInstance.changeTradeModule(sel);
        }

    }
}
