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
    class StockCalcViewer
    {
        private QuickDataGridView dataGridView_;

        string stockCode_ = "";
        StockData getStockData()
        {
            return ControlGet.getInstance.stockBot().getStockDataCode(stockCode_);
        }

        public StockCalcViewer(StockChartDlg dlg, string code)
        {
            stockCode_ = code;
            dataGridView_ = dlg.DataGridView_StockInfo;
            this.setup();
        }

        string[] columnName_ = { "시간", "시가", "고가", "저가", "종가" };

        private void initScreen()
        {
            int i = 0;

            dataGridView_.ColumnCount = columnName_.Length + Enum.GetValues(typeof(EVALUATION_DATA)).Length;
            foreach (string name in columnName_) {
                if (name == columnName_[0]) {
                    dataGridView_.Columns[i].DefaultCellStyle.Format = "yyyy-MM-dd hh:mm:ss";
                }
                else {
                    dataGridView_.Columns[i].DefaultCellStyle.Format = "##,###";
                }
                dataGridView_.Columns[i].Name = name;
                ++i;
            }

            foreach (string name in Enum.GetNames(typeof(EVALUATION_DATA))) {
                dataGridView_.Columns[i].DefaultCellStyle.Format = "##,###0.##";
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
            }
            else {
                this.initScreen();
            }
        }


        private void printInfo()
        {
            dataGridView_.Rows.Clear();
            List<CandleData> priceTable = stockData_.priceTable();
            if (priceTable == null) {
                return;
            }

            int index = 0;
            int maxCount = PublicVar.avgMax[(int) AVG_SAMPLEING.AVG_MAX - 1];
            foreach (CandleData priceData in priceTable) {

                ArrayList columnObject = new ArrayList();
                columnObject.Add(priceData.date_);
                columnObject.Add(priceData.startPrice_);
                columnObject.Add(priceData.highPrice_);
                columnObject.Add(priceData.lowPrice_);
                columnObject.Add(priceData.price_);

                int colIdx = 0;
                foreach (string name in Enum.GetNames(typeof(EVALUATION_DATA))) {
                    if (name.CompareTo("SMA_START") == 0) {
                        continue;
                    }
                    if (name.CompareTo("EMA_START") == 0) {
                        continue;
                    }
                    if (name.CompareTo("AVG_END") == 0) {
                        continue;
                    }
                    if (name.CompareTo("MAX") == 0) {
                        break;
                    }
                    columnObject.Add(priceData.calc_[colIdx++]);
                }

                dataGridView_.Rows.Add(columnObject.ToArray());

                index++;
                if (index > maxCount) {
                    break;
                }
            }

            dataGridView_.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
        }
        StockData stockData_;
        public void print()
        {
            StockData stockData = this.getStockData();
            if (stockData == null) {
                return;
            }

            stockData_ = (StockData) stockData.Clone();

            if (dataGridView_.InvokeRequired) {
                dataGridView_.BeginInvoke(new Action(() => this.printInfo()));
            }
            else {
                this.printInfo();
            }
        }
    }
}
