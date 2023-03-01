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

namespace HappyTool.DlgControl {
    class StockCalcViewer {
        private QuickDataGridView dataGridView_;

        PRICE_TYPE priceType_;
        int stockCode_ = 0;
        StockData getStockData()
        {
            return StockBot.getInstance.stockData(stockCode_);
        }

        public StockCalcViewer(StockChartDlg dlg, int code)
        {
            stockCode_ = code;
            dataGridView_ = dlg.dataGridView_StockInfo();
            this.setup();
        }

        string[] columnName_ = { "시간", "시가", "고가", "저가", "종가" };

        private void initScreen()
        {
            int i = 0;

            dataGridView_.ColumnCount = columnName_.Length + (int) EVALUATION_DATA.MAX;
            foreach (string name in columnName_) {
                dataGridView_.Columns[i].DefaultCellStyle.Format = "##.##";
                dataGridView_.Columns[i].Name = name;
                ++i;
            }

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
            } else {
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
            dataGridView_.Rows.Clear();
            List<CandleData> priceTable = stockData_.priceTable(priceType_);
            if (priceTable == null) {
                return;
            }

            int index = 0;
            int maxCount = PublicVar.avgMax[(int) AVG_SAMPLEING.AVG_MAX - 1];
            foreach (CandleData priceData in priceTable) {
                string[] date = priceData.dateStr_.Split('#');

                ArrayList columnObject = new ArrayList();
                columnObject.Add(date[0]);
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
            } else {
                this.printInfo();
            }
        }
    }
}
