using HappyTool.DlgControl;
using HappyTool.Stock;
using StockLibrary;
using System;
using System.Windows.Forms;
using UtilLibrary;

namespace HappyTool.Dlg
{
    public partial class StockChartDlg : Form
    {
        BasicStockChart stockChart_;
        StockWebBrowser browser_;

        StockCalcViewer stockCalcViewer_;

        StockStrategyModuleViewer strategyModuleViewer_ = null;
        StockStrategyModuleChart strategyModuleChart_;

        public StockChartDlg()
        {
            InitializeComponent();
        }

        string stockCode_ = "";
        StockData getStockData()
        {
            return ControlGet.getInstance.stockBot().getStockDataCode(stockCode_);
        }

        internal void setStockCode(string code)
        {
            stockCode_ = code;
            StockData stockData = this.getStockData();
            if (stockData == null) {
                return;
            }
            stockChart_ = new BasicStockChart(stockCode_);
            stockChart_.setChartControl(this);

            stockCalcViewer_ = new StockCalcViewer(this, stockCode_);
            stockCalcViewer_.setup();

            strategyModuleChart_ = new StockStrategyModuleChart(stockCode_);
            strategyModuleChart_.setChartControl(this);

            strategyModuleViewer_ = new StockStrategyModuleViewer(this, stockCode_);
            strategyModuleViewer_.setup();

            stockChart_.drawStockChart();

            browser_ = new StockWebBrowser(this, stockData.code_);

            this.setTitle();
        }

        void setTitle()
        {
            StockData stockData = this.getStockData();
            this.Text = String.Format("주식[{0}], 종목코드[{1}]", stockData.name_, stockData.code_);
        }

        enum TAB_COLUMN {
            차트,
            백테스트,
            표,
            웹,
        }
        private void tabControl_stock_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch(tabControl_stock.SelectedIndex) {
                case (int)TAB_COLUMN.차트:
                    stockChart_.drawStockChart();
                    break;

                case (int) TAB_COLUMN.백테스트:
                    strategyModuleChart_.drawStockChart();
                    strategyModuleViewer_.print();
                    break;

                case (int) TAB_COLUMN.표:
                    stockCalcViewer_.print();
                    break;

                case (int) TAB_COLUMN.웹:
                    browser_.print();
                    break;
            }
        }

    }
}
