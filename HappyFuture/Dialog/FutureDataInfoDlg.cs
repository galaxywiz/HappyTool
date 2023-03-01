using HappyFuture.DialogControl;
using StockLibrary;
using System;
using System.Threading;
using System.Windows.Forms;
using UtilLibrary;

namespace HappyFuture.Dialog
{
    public partial class FutureDataInfoDlg: Form
    {
        FuturePriceChart stockChart_;
        FutureCalcViewer stockCalcViewer_;
        FutureBackTestViewer backTestViewer_;

        public FutureDataInfoDlg()
        {
            this.InitializeComponent();
        }

        string stockCode_ = "";
        StockData getStockData()
        {
            return ControlGet.getInstance.futureBot().getStockDataCode(this.stockCode_);
        }

        public void redrawChart()
        {
            this.stockChart_.print();
        }

        internal void setStockCode(string code)
        {
            this.stockCode_ = code;
            StockData stockData = this.getStockData();
            this.capture_ = new ControlCapture(this);

            this.stockChart_ = new FuturePriceChart(this.stockCode_);
            this.stockChart_.setup(this);
            this.stockChart_.print();

            this.stockCalcViewer_ = new FutureCalcViewer(this, this.stockCode_);
            this.stockCalcViewer_.setup();

            this.backTestViewer_ = new FutureBackTestViewer(this, this.stockCode_);

            this.setTitle();
        }

        ControlCapture capture_;
        //--------------------------------------------------------------------------//
        // 화면 캡쳐
      
        public void captureMessage(string message)
        {
            this.stockChart_.print();

            StockData stockData = this.getStockData();
            string path = Application.StartupPath + "\\capture\\";
            string fileName = string.Format("chart_{0}_{1}.png", stockData.code_, stockData.nowDateTime().ToString("yyyyMMddHHmm"));
            this.capture_.formCapture(fileName);

         //   Thread.Sleep(500);
            var bot = ControlGet.getInstance.futureBot();
            bot.telegram_.sendPhoto(path + fileName, message);
        }

        void setTitle()
        {
            FutureData futureData = this.getStockData() as FutureData;
            this.Text = string.Format("선물[{0}], 종목코드[{1}], 포지션[{2}]", futureData.name_, futureData.code_, futureData.position_);
        }

        enum TAB_COLUMN
        {
            차트,
            백테스팅,
            표,
        }

        private void tabControl_future_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (this.tabControl_future.SelectedIndex) {
                case (int) TAB_COLUMN.차트:
                this.stockChart_.print();
                break;

                case (int) TAB_COLUMN.백테스팅:
                this.backTestViewer_.update();
                break;

                case (int) TAB_COLUMN.표:
                this.stockCalcViewer_.print();
                break;
            }
        }

        private void listView_Recoder_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.backTestViewer_.selectRecodeIndex(e);
        }

        private void comboBox_priceType_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.stockChart_.selectComboPriceType(e);
        }

        private void FutureInfoDlg_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) {
                this.Close();
            }
        }

        private void Chart_Future_AxisViewChanged(object sender, System.Windows.Forms.DataVisualization.Charting.ViewEventArgs e)
        {
            this.stockChart_.changeAxisView(sender, e);
        }
    }
}
