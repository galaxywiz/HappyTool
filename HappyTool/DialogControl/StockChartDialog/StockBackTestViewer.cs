using HappyTool.Dlg;
using HappyTool.Stock;
using StockLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using UtilLibrary;

namespace HappyTool.DlgControl
{
    class StockStrategyModuleViewer
    {
        private QuickDataGridView dataGridView_;

        string stockCode_ = "";
        StockData getStockData()
        {
            return ControlGet.getInstance.stockBot().getStockDataCode(stockCode_);
        }

        public StockStrategyModuleViewer(StockChartDlg dlg, string code)
        {
            stockCode_ = code;
            dataGridView_ = dlg.DataGridView_StrategyModule;
            this.setup();
        }

        string[] columnName_ = { "구매 시간", "구매시 가격", "갯수", "판매 시간", "판매시 가격", "이익금", "이익율" };

        private void initScreen()
        {
            int i = 0;

            dataGridView_.ColumnCount = columnName_.Length;
            foreach (string name in columnName_) {
                if (name == columnName_[0]) {
                    dataGridView_.Columns[i].DefaultCellStyle.Format = "yyyy-MM-dd hh:mm:ss";
                } else {
                    dataGridView_.Columns[i].DefaultCellStyle.Format = "##,###";
                }
                dataGridView_.Columns[i].Name = name;
                ++i;
            }

            dataGridView_.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
            dataGridView_.ReadOnly = true;
        }

        public void setup()
        {
            if (dataGridView_.InvokeRequired) {
                dataGridView_.BeginInvoke(new Action(() => this.initScreen()));
            } else {
                this.initScreen();
            }
        }

        private void printInfo()
        {
            StockData stockData = this.getStockData();
            if (stockData == null) {
                return;
            }
            BackTestRecoder history = stockData.tradeModule();
            if (history == null) {
                return;
            }
            dataGridView_.Rows.Clear();

            List<TradeRecord> historyList = history.getRecordData();
            foreach (TradeRecord data in historyList) {
                ArrayList columnObject = new ArrayList();
                columnObject.Add(data.buyDate_.ToString());
                columnObject.Add(data.buyPrice_.ToString("#,##0"));
                columnObject.Add(data.buyCount_.ToString("#,##0"));

                columnObject.Add(data.sellDate_.ToString());
                columnObject.Add(data.sellPrice_.ToString("#,##0"));
                columnObject.Add(data.totalProfit().ToString("#,##0"));
                columnObject.Add((data.totalProfitRate() * 100).ToString("#,##0.##"));

                dataGridView_.Rows.Add(columnObject.ToArray());
            }

            dataGridView_.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
        }

        public void print()
        {
            if (dataGridView_.InvokeRequired) {
                dataGridView_.BeginInvoke(new Action(() => this.printInfo()));
            } else {
                this.printInfo();
            }
        }
    }
}
