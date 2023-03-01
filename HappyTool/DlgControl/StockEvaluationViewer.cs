using HappyTool.Dlg;
using HappyTool.Stock;
using HappyTool.Stock.Calculate;
using HappyTool.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace HappyTool.DlgControl
{
    class StockEvaluationViewer
    {
        private QuickDataGridView dataGridView_;
        StockData stockData_;
        int stockCode_;
        PRICE_TYPE priceType_;

        public StockEvaluationViewer(StockChartDlg dlg, int code)
        {
            stockCode_ = code;            
            dataGridView_ = dlg.dataGridView_Evaluation();
            this.setup();
        }

        string[] columnName_ = { "시간", "종합평가" };
        private void initScreen()
        {
            int i = 0;

            dataGridView_.ColumnCount = columnName_.Length + (int) EVALUATION_ITEM.MAX;
            foreach (string name in columnName_) {
                dataGridView_.Columns[i].DefaultCellStyle.Format = "##.##";
                dataGridView_.Columns[i].Name = name;
                ++i;
            }

            foreach (string name in Enum.GetNames(typeof(EVALUATION_ITEM))) {
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

                dataGridView_.Columns[i].DefaultCellStyle.Format = "##.##";
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
        public void setPriceType(PRICE_TYPE type)
        {
            priceType_ = type;
            print();
        }

        private bool printEvaluation(PRICE_TYPE type)
        {
            List<CandleData> priceTable = stockData_.priceTable(type);
            if (priceTable == null) {
                return false;
            }

            Evaluation stockAnalysis = new Evaluation();
            STOCK_EVALUATION evalTotal = stockAnalysis.analysis(priceTable);
            STOCK_EVALUATION[] items = stockAnalysis.getItems();

            ArrayList columnObject = new ArrayList();
            columnObject.Add(type.ToString());
            columnObject.Add(evalTotal.ToString());

            int i = 0;
            foreach (string name in Enum.GetNames(typeof(EVALUATION_ITEM))) {
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
                columnObject.Add(items[i++].ToString());
            }

            dataGridView_.Rows.Add(columnObject.ToArray());

            return true;
        }

        private void printInfo()
        {
            dataGridView_.Rows.Clear();

            if (this.printEvaluation(PRICE_TYPE.DAY) == false) {
                return;
            }
            if (this.printEvaluation(PRICE_TYPE.HOUR1) == false) {
                return;
            }
            int count = dataGridView_.RowCount;

            for (int rowIdx = 0; rowIdx < 2; ++rowIdx) {
                for (int colIdx = 1; colIdx < dataGridView_.ColumnCount; colIdx++) {

                    string col = dataGridView_.Rows[rowIdx].Cells[colIdx].Value.ToString();
                    if (col.CompareTo(STOCK_EVALUATION.매수.ToString()) == 0) {
                        dataGridView_.Rows[rowIdx].Cells[colIdx].Style.BackColor = Color.HotPink;
                    }
                    else if (col.CompareTo(STOCK_EVALUATION.매도.ToString()) == 0) {
                        dataGridView_.Rows[rowIdx].Cells[colIdx].Style.BackColor = Color.DeepSkyBlue;
                    }
                }
            }

            dataGridView_.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
        }

        StockData getStockData()
        {
            return StockBot.getInstance.stockData(stockCode_);
        }

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
