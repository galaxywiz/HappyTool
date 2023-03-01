using HappyTool.Dlg;
using HappyTool.Stock;
using HappyTool.Stock.Calculate;
using HappyTool.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HappyTool.DlgControl
{
    class StockTradeModuleViewer
    {
        private QuickDataGridView dataGridView_;

        PRICE_TYPE priceType_;
        int stockCode_ = 0;
        StockData getStockData()
        {
            return StockBot.getInstance.stockData(stockCode_);
        }

        public StockTradeModuleViewer(StockChartDlg dlg, int code)
        {
            stockCode_ = code;
            dataGridView_ = dlg.dataGridView_TradeModule();
            this.setup();
        }

        string[] columnName_ = { "시간", "구매 여부", "가격", "갯수", "매매 금액"};

        private void initScreen()
        {
            int i = 0;

            dataGridView_.ColumnCount = columnName_.Length;
            foreach (string name in columnName_)
            {
                dataGridView_.Columns[i].DefaultCellStyle.Format = "##.##";
                dataGridView_.Columns[i].Name = name;
                ++i;
            }
      
            dataGridView_.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
            dataGridView_.ReadOnly = true;
        }

        public void setup()
        {
            if (dataGridView_.InvokeRequired)
            {
                dataGridView_.BeginInvoke(new Action(() => this.initScreen()));
            }
            else
            {
                this.initScreen();
            }
        }

        public void setPriceType(PRICE_TYPE type)
        {
            priceType_ = type;
            this.print();
        }

        private void printInfo()
        {
            StockData stockData = this.getStockData();
            if (stockData == null)
            {
                return;
            }
            BackTestHistory history = stockData.backTestHistory(priceType_);
            if (history == null)
            {
                return;
            }
            dataGridView_.Rows.Clear();

            List<BackTestHistoryData> historyList = history.getHistoryData();
            foreach (BackTestHistoryData data in historyList)
            {
                ArrayList columnObject = new ArrayList();
                columnObject.Add(data.date_.ToString());
                columnObject.Add(data.isBuy_ ? "매수" : "매도");
                columnObject.Add(data.price_.ToString("#,##0"));
                columnObject.Add(data.buyCount_.ToString("#,##0"));
                columnObject.Add(data.totalPrice().ToString("#,##0"));

                dataGridView_.Rows.Add(columnObject.ToArray());
            }

            dataGridView_.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
        }

        public void print()
        {
            if (dataGridView_.InvokeRequired)
            {
                dataGridView_.BeginInvoke(new Action(() => this.printInfo()));
            }
            else
            {
                this.printInfo();
            }
        }
    }
}
