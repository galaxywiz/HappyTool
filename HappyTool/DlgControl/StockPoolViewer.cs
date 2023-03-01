using HappyTool.Dlg;
using HappyTool.DlgControl;
using HappyTool.Stock;
using HappyTool.Stock.Calculate;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Telegram.Bot.Args;

//http://blog.naver.com/PostView.nhn?blogId=dolmoon&logNo=60050778826
namespace HappyTool.Util
{
    class StockPoolViewer :SingleTon<StockPoolViewer>
    {
        private QuickDataGridView dataGridView_;
        private Dictionary<int, int> viewData_;                        // 보여지는 가격 데이터
        private readonly PRICE_TYPE priceType_ = PublicVar.initType;   // 재갱신 여부 판단용

        StockPoolViewer()
        {
            dataGridView_ = Program.happyTool_.stockDlg_.dataGridView_Stock();
            viewData_ = new Dictionary<int, int>();
            this.setup();
        }
        enum ColumnName
        {
            구분, 평가, 종목코드, 종목명, 현재가, 백테스팅_이익율, 백테스팅_시도, 보유수량, 주당_매입가, 총_매입가, 이익, 이익율, MAX
        };

        private void initScreen()
        {
            int i = 0;

            dataGridView_.ColumnCount = (int) ColumnName.MAX;
            foreach (ColumnName name in Enum.GetValues(typeof(ColumnName))) {
                if (name == ColumnName.MAX) {
                    break;
                }
                dataGridView_.Columns[i++].Name = name.ToString();
            }
            dataGridView_.ReadOnly = true;
        }

        public void setup()
        {
            if (dataGridView_.InvokeRequired) {
                dataGridView_.BeginInvoke(new Action(() => this.initScreen()));
            } else {
                this.initScreen();
            }
            dataGridView_.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
        }

        //---- 각 데이터가 달라야 갱신해주도록 한다.
        private void setViewData(Dictionary<int, StockData> stockPool)
        {
            viewData_.Clear();
            foreach (KeyValuePair<int, StockData> keyValue in stockPool) {
                int code = keyValue.Key;
                StockData stockData = keyValue.Value;

                viewData_.Add(code, stockData.nowPrice(priceType_));
            }
        }

        private bool needSyncViewData(Dictionary<int, StockData> stockPool)
        {
            if (stockPool.Count != viewData_.Count) {
                return true;
            }
            foreach (KeyValuePair<int, StockData> keyValue in stockPool) {
                int code = keyValue.Key;
                StockData stockData = keyValue.Value;

                if (viewData_[code] != stockData.nowPrice(priceType_)) {
                    return true;
                }
            }
            return false;
        }

        private void printList(Dictionary<int, StockData> stockPool)
        {
            dataGridView_.Rows.Clear();
            PRICE_TYPE priceType = StockBot.getInstance.priceType_;
            foreach (KeyValuePair<int, StockData> keyValue in stockPool) {
                int code = keyValue.Key;
                StockData orgData = keyValue.Value;

                StockData stockData = (StockData) orgData.Clone();
                //정렬을 위해...
                string column = "2_감시";
                if (stockData.isBuyedStock()) {
                    column = "1_보유";
                }

                string codeString = stockData.codeString();
                string name = stockData.name_;
                string nowPrice = stockData.nowPrice(priceType_).ToString("#,##0");

                string buyCount = "";
                string buyPrice = "";
                string totalBuyPrice = "";
                string profitPrice = "";
                string profitPriceRate = "";

                if (stockData.isBuyedStock()) {
                    BuyedStockData buyedStockData = (BuyedStockData) stockData;
                    buyCount = buyedStockData.buyCount_.ToString("#,##0");
                    buyPrice = buyedStockData.buyPrice_.ToString("#,##0");
                    totalBuyPrice = buyedStockData.totalBuyPrice().ToString("#,##0");
                    profitPrice = buyedStockData.profitPrice(priceType).ToString("#,##0");
                    profitPriceRate = buyedStockData.profitPriceRate(priceType).ToString("#,##0");
                }
                string eval = stockData.evalTotal(priceType_).ToString();
                BackTestHistory history = stockData.backTestHistory(priceType_);
                string testProfitRate = history.profitRate_.ToString("#,#.#");
                string testCount = history.tradeCount_.ToString();

                string[] row = new string[] { column, eval, codeString, name, nowPrice, testProfitRate, testCount, buyCount, buyPrice, totalBuyPrice, profitPrice, profitPriceRate };
                dataGridView_.Rows.Add(row);
                stockData = null;
            }

            dataGridView_.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
            int maxRow = Math.Max(dataGridView_.RowCount - 1, 0);
            for (int rowIdx = 0; rowIdx < maxRow; ++rowIdx) {
                double rate = 0.0f;
                if (double.TryParse(dataGridView_.Rows[rowIdx].Cells[(int) ColumnName.백테스팅_이익율].Value.ToString(), out rate) == false) {
                    continue;
                }
                if (rate > 0) {
                    dataGridView_.Rows[rowIdx].Cells[(int) ColumnName.백테스팅_이익율].Style.BackColor = Color.HotPink;
                } else {
                    dataGridView_.Rows[rowIdx].Cells[(int) ColumnName.백테스팅_이익율].Style.BackColor = Color.DeepSkyBlue;
                }
            }

            //첫열을 기준으로 정렬을 시켜서 구입한 주식을 위로 올린다.
            dataGridView_.Sort(dataGridView_.Columns[(int) ColumnName.구분], System.ComponentModel.ListSortDirection.Ascending);
        }

        private void consolePrintList(Dictionary<int, StockData> stockPool)
        {
            PRICE_TYPE priceType = StockBot.getInstance.priceType_;
            Logger.getInstance.consolePrint("=============================================================");
            foreach (KeyValuePair<int, StockData> keyValue in stockPool) {
                int code = keyValue.Key;
                StockData orgData = keyValue.Value;

                StockData stockData = (StockData) orgData.Clone();
                //정렬을 위해...
                string column = "2_감시";
                if (stockData.isBuyedStock()) {
                    column = "1_보유";
                }

                string codeString = stockData.codeString();
                string name = stockData.name_;
                string nowPrice = stockData.nowPrice(priceType_).ToString();

                string buyCount = "";
                string buyPrice = "";
                string totalBuyPrice = "";
                string profitPrice = "";
                string profitPriceRate = "";

                if (stockData.isBuyedStock()) {
                    BuyedStockData buyedStockData = (BuyedStockData) stockData;
                    buyCount = buyedStockData.buyCount_.ToString();
                    buyPrice = buyedStockData.buyPrice_.ToString();
                    totalBuyPrice = buyedStockData.totalBuyPrice().ToString();
                    profitPrice = buyedStockData.profitPrice(priceType).ToString();
                    profitPriceRate = buyedStockData.profitPriceRate(priceType).ToString();
                }

                Logger.getInstance.consolePrint("{0} | {1} | {2} | {3} | {4} | {5} | {6} | {7} | {8}",
                    column, codeString, name, nowPrice, buyCount, buyPrice, totalBuyPrice, profitPrice, profitPriceRate);

                stockData = null;
            }
        }

        // 주식 정보를 출력합니다.
        public void print(Dictionary<int, StockData> stockPool)
        {
            if (!this.needSyncViewData(stockPool)) {
                return;
            }
            this.setViewData(stockPool);

            if (dataGridView_.InvokeRequired) {
                dataGridView_.BeginInvoke(new Action(() => this.printList(stockPool)));
            } else {
                this.printList(stockPool);
            }
        }

        // 셀을 선택했을때 처리
        public void cellClick(Object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0
                || e.RowIndex >= (dataGridView_.RowCount - 2)) {
                return;
            }
            int stockCode = int.Parse(dataGridView_.Rows[e.RowIndex].Cells[(int) ColumnName.종목코드].Value.ToString());

            StockData stockData = StockBot.getInstance.stockData(stockCode);
            if (stockData == null) {
                return;
            }

            StockChartDlg chartDlg = new StockChartDlg();
            chartDlg.setStockCode(stockData.code_);
            chartDlg.Show();

            ////StockInfomationDlg infoDlg = new StockInfomationDlg();
            ////infoDlg.setStockData(stockData);
            ////infoDlg.Show();
        }
    }
}
