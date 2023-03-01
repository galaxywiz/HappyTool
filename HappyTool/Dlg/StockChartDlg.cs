using HappyTool.DlgControl;
using HappyTool.Stock;
using HappyTool.Stock.Calculate;
using HappyTool.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace HappyTool.Dlg
{
    public partial class StockChartDlg : Form
    {
        BasicStockChart stockChart_;
        StockWebBrowser browser_;

        StockCalcViewer stockCalcViewer_;
        StockEvaluationViewer stockEvaluationViewer_;

        StockTradeModuleViewer TradeModuleViewer_;
        StockTradeModuleChart TradeModuleChart_;

        PRICE_TYPE priceType_;

        public StockChartDlg()
        {
            InitializeComponent();
        }

        int stockCode_ = 0;
        StockData getStockData()
        {
            return StockBot.getInstance.stockData(stockCode_);
        }

        internal void setStockCode(int code)
        {
            stockCode_ = code;
            StockData stockData = this.getStockData();

            stockChart_ = new BasicStockChart(stockCode_);
            stockChart_.setChartControl(this);

            browser_ = new StockWebBrowser(this, stockData.codeString());
            stockCalcViewer_ = new StockCalcViewer(this, stockCode_);
            stockEvaluationViewer_ = new StockEvaluationViewer(this, stockCode_);
            TradeModuleViewer_ = new StockTradeModuleViewer(this, stockCode_);

            TradeModuleChart_ = new StockTradeModuleChart(stockCode_);
            TradeModuleChart_.setChartControl(this);

            graph_bnf_check.Checked = true;
            graph_boll_check.Checked = true;

            priceType_ = PublicVar.initType;
            stockChart_.setPriceType(priceType_); 
            switch(priceType_) {
                case PRICE_TYPE.DAY: RadioButton_stock_day.Checked = true; break;
                case PRICE_TYPE.WEEK: RadioButton_stock_week.Checked = true; break;
                case PRICE_TYPE.HOUR1: RadioButton_stock_1hour.Checked = true; break;
                case PRICE_TYPE.HOUR4: RadioButton_stock_4hour.Checked = true; break;
                case PRICE_TYPE.MIN15: RadioButton_stock_15min.Checked = true; break;
            }

            TrackBar_stock.Value = (TrackBar_stock.Maximum - TrackBar_stock.Minimum) / 2;

            trackBar_bnf.Minimum = (int) PublicVar.bnfMin;
            trackBar_bnf.Maximum = (int) PublicVar.bnfMax;            
            trackBar_bnf.Value = (int) stockData.bnfPercent();
            setBnfPercentLabel(stockData.bnfPercent());

            stockCalcViewer_.setup();
            stockEvaluationViewer_.setup();
            TradeModuleViewer_.setup();

            this.setTitle();
        }

        void setTitle()
        {
            StockData stockData = this.getStockData();
            this.Text = String.Format("주식[{0}], 종목코드[{1}], 데이터 종류[{2}]", stockData.name_, stockData.codeString(), priceType_.ToString());
        }

        // 주식 그래프의 단위 선택(틱 / 분/ 일)
        private void RadioButton_stock_tick_CheckedChanged(Object sender, EventArgs e)
        {
            MessageBox.Show("준비중");
            //StockChart.getInstance.setUnit(PRICE_TYPE.TICK);
            //StockInfoViewer.getInstance.setUnit(PRICE_TYPE.TICK);
        }

        // 컨트롤 뷰들
        public Chart chart_Stock()
        {
            return Chart_Stock;
        }

        public Chart chart_Macd()
        {
            return Chart_MACD;
        }

        public Chart chart_TradeModule()
        {
            return Chart_BackTest;
        }

        public WebBrowser chart_WebBrowser()
        {
            return WebBrowser_Stock;
        }

        public QuickDataGridView dataGridView_StockInfo()
        {
            return DataGridView_StockInfo;
        }

        public QuickDataGridView dataGridView_Evaluation()
        {
            return DataGridView_Evaluation;
        }

        public QuickDataGridView dataGridView_TradeModule()
        {
            return DataGridView_TradeModule;
        }

        // 다이얼 로그 버튼들
        private void changeRadio(PRICE_TYPE priceType)
        {
            StockData stockData = this.getStockData();

            List<CandleData> priceData = stockData.priceTable(priceType);
            if (priceData == null)
            {
                StockBot bot = StockBot.getInstance;
                bot.requestStockData(priceType, stockData.code_);
            }

            priceType_ = priceType;
            stockChart_.setPriceType(priceType_);
            stockCalcViewer_.setPriceType(priceType_);
            stockEvaluationViewer_.setPriceType(priceType_);
            TradeModuleViewer_.setPriceType(priceType_);
            TradeModuleChart_.setPriceType(priceType_);

            this.setTitle();
        }

        private void RadioButton_stock_15min_CheckedChanged(object sender, EventArgs e)
        {
            this.changeRadio(PRICE_TYPE.MIN15);
        }
        private void RadioButton_stock_1hour_CheckedChanged(object sender, EventArgs e)
        {
            this.changeRadio(PRICE_TYPE.HOUR1);            
        }
        private void RadioButton_stock_4hour_CheckedChanged(object sender, EventArgs e)
        {
            this.changeRadio(PRICE_TYPE.HOUR4);
        }
        private void RadioButton_stock_day_CheckedChanged(Object sender, EventArgs e)
        {
            this.changeRadio(PRICE_TYPE.DAY);
        }
        private void RadioButton_stock_week_CheckedChanged(object sender, EventArgs e)
        {
            this.changeRadio(PRICE_TYPE.WEEK);
        }

        public TrackBar trackBarStock()
        {
            return TrackBar_stock;
        }

        private void TrackBar_stock_Scroll(object sender, EventArgs e)
        {
            if (stockChart_ != null)
            {
                stockChart_.changedTrackValue(sender, e);
            }
        }
        private void setBnfPercentLabel(double value)
        {
            this.label_bnfPercent.Text = String.Format("{0:0.##} % 이격", value);
        }

        private void trackBar_bnf_Scroll(object sender, EventArgs e)
        {
            if (stockChart_ != null)
            {
                double value = Convert.ToDouble(trackBar_bnf.Value);
                stockChart_.setBnfPercent(value);
                this.setBnfPercentLabel(value);
            }
        }

        private bool graphEma()
        {
            return graph_ema_check.Checked;
        }

        private bool graphBoll()
        {
            return graph_boll_check.Checked;
        }

        private bool graphBnf()
        {
            return graph_bnf_check.Checked;
        }

        private bool graphLine()
        {
            return graph_line_check.Checked;
        }

        private void graph_ema_check_CheckedChanged(Object sender, EventArgs e)
        {
            if (stockChart_ != null)
            {
                stockChart_.setDrawLine(StockDrawLine.EMA, this.graphEma());
            }
        }

        private void graph_boll_check_CheckedChanged(Object sender, EventArgs e)
        {
            if (stockChart_ != null)
            {
                stockChart_.setDrawLine(StockDrawLine.BOLL, this.graphBoll());
            }
        }

        private void graph_bnf_check_CheckedChanged(Object sender, EventArgs e)
        {
            if (stockChart_ != null)
            {
                stockChart_.setDrawLine(StockDrawLine.BNF, this.graphBnf());
            }
        }

        private void graph_line_check_CheckedChanged(Object sender, EventArgs e)
        {
            if (stockChart_ != null)
            {
                stockChart_.setDrawLine(StockDrawLine.LINE, this.graphLine());
            }
        }

        private void tabControl_stock_SelectedIndexChanged(object sender, EventArgs e)
        {
            const int VALUE_TABLE = 1;
            if (tabControl_stock.SelectedIndex == VALUE_TABLE)
            {
                stockCalcViewer_.setPriceType(priceType_);
                stockCalcViewer_.print();
                stockEvaluationViewer_.setup();
            }
        }

    }
}
