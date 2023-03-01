using HappyFuture.Dialog;
using StockLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using UtilLibrary;

namespace HappyFuture.DialogControl
{
    class FutureCalcViewer
    {
        private readonly QuickDataGridView dataGridView_;
        readonly string stockCode_ = "";
        StockData getStockData()
        {
            return ControlGet.getInstance.futureBot().getStockDataCode(this.stockCode_);
        }

        public FutureCalcViewer(FutureDataInfoDlg dlg, string code)
        {
            this.stockCode_ = code;
            this.dataGridView_ = dlg.DataGridView_FutureInfo;
            this.setup();
        }

        readonly string[] columnName_ = { "시간", "시가", "고가", "저가", "종가" };

        private void initScreen()
        {
            int i = 0;

            this.dataGridView_.ColumnCount = this.columnName_.Length + Enum.GetValues(typeof(EVALUATION_DATA)).Length;
            foreach (string name in this.columnName_) {
                if (name == this.columnName_[0]) {
                    this.dataGridView_.Columns[i].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm";
                }
                else {
                    this.dataGridView_.Columns[i].DefaultCellStyle.Format = "##,###0.#####0";
                }
                this.dataGridView_.Columns[i].Name = name;
                ++i;
            }

            foreach (string name in Enum.GetNames(typeof(EVALUATION_DATA))) {
                this.dataGridView_.Columns[i].DefaultCellStyle.Format = "##,###0.#####0";
                this.dataGridView_.Columns[i].Name = name;
                ++i;
            }
            this.dataGridView_.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
            this.dataGridView_.ReadOnly = true;
        }

        public void setup()
        {
            if (this.dataGridView_.InvokeRequired) {
                this.dataGridView_.BeginInvoke(new Action(() => this.initScreen()));
            }
            else {
                this.initScreen();
            }
        }

        private void printInfo()
        {
            this.dataGridView_.Rows.Clear();
            List<CandleData> priceTable = this.stockData_.priceTable();
            if (priceTable == null) {
                return;
            }

            foreach (CandleData priceData in priceTable) {

                ArrayList columnObject = new ArrayList();
                columnObject.Add(priceData.date_);
                columnObject.Add(priceData.startPrice_);
                columnObject.Add(priceData.highPrice_);
                columnObject.Add(priceData.lowPrice_);
                columnObject.Add(priceData.price_);

                int colIdx = 0;
                foreach (string name in Enum.GetNames(typeof(EVALUATION_DATA))) {
                    columnObject.Add(priceData.calc_[colIdx++]);
                }

                this.dataGridView_.Rows.Add(columnObject.ToArray());
            }

            this.dataGridView_.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
        }
        StockData stockData_;
        public void print()
        {
            StockData stockData = this.getStockData();
            if (stockData == null) {
                return;
            }

            this.stockData_ = (StockData) stockData.Clone();

            if (this.dataGridView_.InvokeRequired) {
                this.dataGridView_.BeginInvoke(new Action(() => this.printInfo()));
            }
            else {
                this.printInfo();
            }
        }
    }
}
