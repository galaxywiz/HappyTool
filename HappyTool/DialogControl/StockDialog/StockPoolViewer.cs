using HappyTool.Dlg;
using HappyTool.Stock;
using StockLibrary;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using UtilLibrary;

//http://blog.naver.com/PostView.nhn?blogId=dolmoon&logNo=60050778826
namespace HappyTool.Util
{
    class StockPoolViewer :SingleTon<StockPoolViewer>
    {
        private QuickDataGridView dataGridView_;

        StockPoolViewer()
        {
            dataGridView_ = Program.happyTool_.stockDlg_.dataGridView_Stock();
        }

        internal enum COLUMNS_NAME
        {
            종목명, 종목코드, 포지션, 현재가, 업데이트, 분봉수, 거래량, 분석수, 예측가격, 예측기준시간,
        };

        private void initScreen()
        {
            dataGridView_.ReadOnly = true;
            dataGridView_.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
        }

        public void setup()
        {
            if (dataGridView_.InvokeRequired) {
                dataGridView_.BeginInvoke(new Action(() => this.initScreen()));
            } else {
                this.initScreen();
            }
        }

        void updateStockData(ref DataTable dt, StockData stockData, bool refresh = true)
        {
            KStockData kStockData = stockData as KStockData;
            var name = kStockData.name_;
            var codeString = kStockData.code_;
            var position = kStockData.position_;
            var nowPrice = kStockData.nowPrice();
            var updateTime = kStockData.nowDateTime();
            var candleCount = kStockData.priceDataCount();
            var volume = kStockData.nowVolume();

            var recode = kStockData.tradeModule();
            if (recode == null) {
                dt.Rows.Add(name, codeString, position, nowPrice, updateTime, candleCount, volume);
            } else {
                var count = kStockData.tradeModulesCount();
                var testExpected = kStockData.logExpectedRange();
                var testDate = kStockData.tradeModuleUpdateTime_;

                dt.Rows.Add(name, codeString, position, nowPrice, updateTime, candleCount, volume, count, testExpected, testDate);
            }
        }

        void decorationView()
        {
            if (dataGridView_.Columns.Count == 0) {
                return;
            }
            foreach (DataGridViewColumn column in dataGridView_.Columns) {
                string fmt = "#,###0.##";

                if (column.HeaderText == COLUMNS_NAME.업데이트.ToString()
                || column.HeaderText == COLUMNS_NAME.예측기준시간.ToString()) {
                    fmt = "tt hh:mm";
                }
                column.DefaultCellStyle.Format = fmt;
            }

            dataGridView_.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
            int maxRow = Math.Max(dataGridView_.RowCount - 1, 0);

            var bot = ControlGet.getInstance.stockBot();
            for (int rowIdx = 0; rowIdx < maxRow; ++rowIdx) {
                // 포지션에 따른 행의 색 변경
                string text = dataGridView_.Rows[rowIdx].Cells[(int) COLUMNS_NAME.포지션].Value.ToString();
                if (text != TRADING_STATUS.모니터닝.ToString()) {
                    string code = dataGridView_.Rows[rowIdx].Cells[(int) COLUMNS_NAME.종목코드].Value.ToString();
                    KStockData stockData = bot.getStockDataCode(code) as KStockData;
                    if (stockData != null) {
                        Color color = Color.Empty;
                        if (stockData.sellOffReady_) {
                            color = Color.IndianRed;
                        }
                        else {
                            var pureProfit = stockData.nowPureProfit();
                            if (pureProfit > 0.0f) {
                                color = Color.Pink;
                            }
                            else {
                                color = Color.SkyBlue;
                            }
                        }
                        dataGridView_.Rows[rowIdx].DefaultCellStyle.BackColor = color;
                    }
                }
             }

            this.dataGridView_.Columns[(int) COLUMNS_NAME.종목명].Frozen = true;
            this.dataGridView_.Columns[(int) COLUMNS_NAME.종목명].DividerWidth = 3; // 구분선 폭 설정
        }

        DataTable makeNewTable()
        {
            DataTable dt = new DataTable();
            foreach (COLUMNS_NAME name in Enum.GetValues(typeof(COLUMNS_NAME))) {
                if (name == COLUMNS_NAME.포지션
                 || name == COLUMNS_NAME.종목명
                 || name == COLUMNS_NAME.종목코드
                 || name == COLUMNS_NAME.예측가격) {
                    dt.Columns.Add(name.ToString(), typeof(string));
                }
                else if (name == COLUMNS_NAME.업데이트
                    || name == COLUMNS_NAME.예측기준시간) {
                    dt.Columns.Add(name.ToString(), typeof(DateTime));
                }
                else {
                    dt.Columns.Add(name.ToString(), typeof(double));
                }
            }

            StockBot bot = ControlGet.getInstance.stockBot();
            StockDataEach eachDo = (string code, StockData stockData) => {
                this.updateStockData(ref dt, stockData, false);
            };
            bot.doStocks(eachDo);
            return dt;
        }

        private void printList()
        {
            //안에 뭐 수정하고 하면 느려짐. 그냥 새로 만드는게 더 빠름.
            var newData = this.makeNewTable();
            if (newData == null) {
                return;
            }

            dataGridView_.DataSource = null;
            dataGridView_.DataSource = newData;

            this.decorationView();
            dataGridView_.Update();
            dataGridView_.Refresh();
        }

        public void print()
        {
            if (dataGridView_.InvokeRequired) {
                dataGridView_.BeginInvoke(new Action(() => this.printList()));
            } else {
                this.printList();
            }
        }

        // 셀을 선택했을때 처리
        public void cellClick(Object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0
                || e.RowIndex >= (dataGridView_.RowCount - 2)) {
                return;
            }
            string stockCode = dataGridView_.Rows[e.RowIndex].Cells[(int) COLUMNS_NAME.종목코드].Value.ToString();

            StockData stockData = ControlGet.getInstance.stockBot().getStockDataCode(stockCode);
            if (stockData == null) {
                return;
            }

            StockChartDlg chartDlg = new StockChartDlg();
            chartDlg.setStockCode(stockData.code_);
            chartDlg.Show();

            this.decorationView();
        }
    }
}
