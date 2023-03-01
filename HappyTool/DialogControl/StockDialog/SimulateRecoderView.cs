using HappyTool.Dlg;
using HappyTool.Stock;
using StockLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using UtilLibrary;

namespace HappyTool.DlgControl
{
    class SimulateHistoryView
    {
        private QuickDataGridView dataGridView_;

        enum SIMULATE_COLUMN
        {
            매수일, 매도일, 주식명, 청산가, 갯수, 투자액, 청산금, 세금땐이익금, 순이익율, MAX
        }

        public SimulateHistoryView()
        {
            StockDlg stockDlg = Program.happyTool_.stockDlg_;
            dataGridView_ = stockDlg.DataGridView_history;
            this.setup();
        }

        private void initScreen()
        {
            int i = 0;
            dataGridView_.ColumnCount = (int) SIMULATE_COLUMN.MAX;
            foreach (SIMULATE_COLUMN name in Enum.GetValues(typeof(SIMULATE_COLUMN))) {
                if (name == SIMULATE_COLUMN.MAX) {
                    break;
                }
                dataGridView_.Columns[i++].Name = name.ToString();
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

        BackTestRecoder recoder_ = null;
        public void setBackTestHistory(BackTestRecoder recoder)
        {
            recoder_ = recoder;
        }

        void decorationView()
        {
            dataGridView_.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
            int maxRow = Math.Max(dataGridView_.RowCount - 1, 0);
            for (int rowIdx = 0; rowIdx < maxRow; ++rowIdx) {
                double rate = 0.0f;
                if (double.TryParse(dataGridView_.Rows[rowIdx].Cells[(int) SIMULATE_COLUMN.순이익율].Value.ToString(), out rate) == false) {
                    continue;
                }
                if (rate > 0) {
                    dataGridView_.Rows[rowIdx].Cells[(int) SIMULATE_COLUMN.순이익율].Style.BackColor = Color.HotPink;
                }
                else {
                    dataGridView_.Rows[rowIdx].Cells[(int) SIMULATE_COLUMN.순이익율].Style.BackColor = Color.DeepSkyBlue;
                }
            }
        }

        private void printInfo()
        {
            if (recoder_ == null) {
                return;
            }

            List<TradeRecord> profitRecoderList = recoder_.getRecordData();
            if (profitRecoderList.Count == 0) {
                return;
            }
            dataGridView_.Rows.Clear();

            foreach (TradeRecord data in profitRecoderList) {
                StockData stockData = ControlGet.getInstance.stockBot().getStockDataCode(data.stockCode_);
                ArrayList columnObject = new ArrayList();
                columnObject.Add(data.buyDate_.ToString());
                columnObject.Add(data.sellDate_.ToString());
                columnObject.Add(stockData.name_);
                columnObject.Add(data.sellPrice_.ToString("#,##0"));
                columnObject.Add(data.buyCount_.ToString("#,##0"));
                columnObject.Add(data.totalBuyPrice().ToString("#,##0"));
                columnObject.Add(data.totalSellPrice().ToString("#,##0"));
                columnObject.Add(data.totalProfit().ToString("#,##"));
                columnObject.Add((data.totalProfitRate() * 100).ToString("#,##0.##"));

                dataGridView_.Rows.Add(columnObject.ToArray());
            }

            this.decorationView();            
        }

        public void resetDataGridView()
        {
            if (dataGridView_.InvokeRequired) {
                dataGridView_.BeginInvoke(new Action(()=> dataGridView_.Rows.Clear()));
            } else {
                dataGridView_.Rows.Clear();
            }
        }

        public void print()
        {
            if (dataGridView_.InvokeRequired) {
                dataGridView_.BeginInvoke(new Action(() => this.printInfo()));
            } else {
                this.printInfo();
            }
        }

        internal void cellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0
                || e.RowIndex >= (dataGridView_.RowCount - 1)) {
                return;
            }
            string stockName = dataGridView_.Rows[e.RowIndex].Cells[(int) SIMULATE_COLUMN.주식명].Value.ToString();

            StockData stockData = ControlGet.getInstance.stockBot().getStockData(stockName);
            if (stockData == null) {
                return;
            }

            StockChartDlg chartDlg = new StockChartDlg();
            chartDlg.setStockCode(stockData.code_);
            chartDlg.Show();
        }
    }
}
