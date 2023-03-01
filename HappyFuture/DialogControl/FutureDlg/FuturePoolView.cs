using HappyFuture.Dialog;
using StockLibrary;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using UtilLibrary;

namespace HappyFuture.DialogControl.FutureDlg
{
    class FuturePoolViewer: QuickDataViewCtl
    {
        internal enum COLUMNS_NAME
        {
            거래, 종목명, 종목코드, 만기일, 포지션, 현재가, 업데이트, 거래시간, 신호, 거래량, 틱가치, 위탁증거금, 매수가능, 매도가능, 연속승리, 전략매니져,
        };

        void updateStockData(ref DataTable dt, StockData stockData)
        {
            FutureBot bot = ControlGet.getInstance.futureBot();
            FutureData futureData = stockData as FutureData;
            var canTrade = futureData.canTrade_;

            var name = futureData.name_;
            var codeString = futureData.code_;
            var endDay = futureData.endDays_;

            string position = futureData.position_.ToString();
            var nowPrice = futureData.realNowPrice();
            var updateTime = futureData.realRecvied_;
            var tradingTime = futureData.tradingTime_;
            var tradingSignal = futureData.tradingSignal(bot);
            var signal = (tradingSignal == TRADING_STATUS.매도) ? "(↓)하락" : (tradingSignal == TRADING_STATUS.매수) ? "(↑)상승" : "(-)중립";
            var volume = futureData.nowVolume();
            var tickValue = futureData.tickValue_;
            var trustMargin = futureData.margineMoney_.trustMargin_;

            var canBuy = futureData.canBuyCount_;
            var canSell = futureData.canSellCount_;
            var continueWin = futureData.statisticsCount_.continueWin_;
            var manager = futureData.fundManagement_ != null ? futureData.fundManagement_.name() : "NULL";

            var recode = futureData.tradeModule() as FutureBackTestRecoder;
            if (recode == null) {
                dt.Rows.Add(canTrade, name, codeString, endDay, position, nowPrice, updateTime, tradingTime, signal, volume, tickValue, trustMargin, canBuy, canSell, continueWin, manager);
                return;
            }
        }

        protected override void decorationView()
        {
            if (this.dataGridView_.Columns.Count == 0) {
                return;
            }
            foreach (DataGridViewColumn column in this.dataGridView_.Columns) {
                if (column.HeaderText == COLUMNS_NAME.거래.ToString()) {
                    continue;
                }
                string fmt = "#,###0.####";
                if (column.HeaderText == COLUMNS_NAME.업데이트.ToString()
                    || column.HeaderText == COLUMNS_NAME.거래시간.ToString()) {
                    fmt = "tt hh:mm:ss";
                }
                column.DefaultCellStyle.Format = fmt;
            }

            this.dataGridView_.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
            int maxRow = Math.Max(this.dataGridView_.RowCount - 1, 0);

            var bot = ControlGet.getInstance.futureBot();
            DateTime now = DateTime.Now;
            int min = bot.priceTypeMin();

            for (int rowIdx = 0; rowIdx < maxRow; ++rowIdx) {
                // 매수 / 매도는 다른 색으로
                string text = this.dataGridView_.Rows[rowIdx].Cells[(int) COLUMNS_NAME.포지션].Value.ToString();
                string code = this.dataGridView_.Rows[rowIdx].Cells[(int) COLUMNS_NAME.종목코드].Value.ToString();
                FutureData futureData = bot.getStockDataCode(code) as FutureData;
                if (futureData == null) {
                    continue;
                }
              
                Color color = Color.Empty;
                if (futureData.realTimeRecived_) {
                    color = Color.Azure;
                }

                // 목표값에 따른 컬럼 색 변경
                if (text != TRADING_STATUS.모니터닝.ToString()) {
                    if (futureData.payOffReadyCount_ > 0) {
                        color = Color.IndianRed;
                    }
                    else if (futureData.isExpectedRange()) {
                        color = Color.LightGoldenrodYellow;
                    }
                    else {
                        var pureProfit = futureData.nowOneProfit() - PublicFutureVar.pureFeeTex;
                        if (pureProfit > 0.0f) {
                            // 수익 구간
                            color = Color.Pink;
                        }
                        else {
                            // 손해 구간
                            color = Color.SkyBlue;
                        }
                    }
                } else {
                    var futureDataDate = futureData.realNowDateTime();
                    if (futureDataDate.AddMinutes(min) < now) {
                        // 아직 업데이트 안됨
                        color = Color.DarkSeaGreen;
                    }
                }
                // 임시 거래 금지
                if (futureData.isTradeBanTime()) {
                    color = Color.DarkBlue;
                }

                this.dataGridView_.Rows[rowIdx].DefaultCellStyle.BackColor = color;
            }
            this.dataGridView_.Columns[(int) COLUMNS_NAME.종목명].Frozen = true;
            this.dataGridView_.Columns[(int) COLUMNS_NAME.종목명].DividerWidth = 3; // 구분선 폭 설정
        }

        public override DataTable makeNewTable()
        {
            DataTable dt = new DataTable();
            foreach (COLUMNS_NAME name in Enum.GetValues(typeof(COLUMNS_NAME))) {
                if (
                  name == COLUMNS_NAME.포지션
                 || name == COLUMNS_NAME.신호
                 || name == COLUMNS_NAME.종목명
                 || name == COLUMNS_NAME.종목코드
                 || name == COLUMNS_NAME.전략매니져) {
                    dt.Columns.Add(name.ToString(), typeof(string));
                } else if (name == COLUMNS_NAME.업데이트
                       || name == COLUMNS_NAME.거래시간) {
                    dt.Columns.Add(name.ToString(), typeof(DateTime));
                } else if (name == COLUMNS_NAME.거래) {
                    dt.Columns.Add(name.ToString(), typeof(bool));
                } else {
                    dt.Columns.Add(name.ToString(), typeof(double));
                }
            }

            FutureBot bot = ControlGet.getInstance.futureBot();

            StockDataEach eachDo = (string code, StockData stockData) => {
                this.updateStockData(ref dt, stockData);
            };
            bot.doStocks(eachDo);
            return dt;
        }

        public override void update()
        {
            var dt = this.makeNewTable();
            if (this.dataGridView_ == null) {
                return;
            }
            if (this.dataGridView_.InvokeRequired) {
                this.dataGridView_.BeginInvoke(new Action(() => this.printList(dt)));
            }
            else {
                this.printList(dt);
            }
        }

        // 셀을 선택했을때 처리
        public void cellMouseUp(Object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0
                || e.RowIndex >= (this.dataGridView_.RowCount - 1)) {
                return;
            }
            bool selecetCheck = e.ColumnIndex == 0 ? true : false;

            DataGridViewCell currentCell = (sender as DataGridView).CurrentCell;
            if (currentCell == null) {
                return;
            }

            string stockCode = this.dataGridView_.Rows[e.RowIndex].Cells[(int) COLUMNS_NAME.종목코드].Value.ToString();
            StockData stockData = ControlGet.getInstance.futureBot().getStockDataCode(stockCode);
            if (stockData == null) {
                return;
            }
            
            // var dlgInfo = FutureDlgInfo.getInstance;
            switch (e.Button) {
                // 왼쪽이면 그래프창 띄우기
                case MouseButtons.Left:
                if (selecetCheck) {
                    var canTrade = stockData.canTrade_;
                    stockData.canTrade_ = !canTrade;
                    this.update();
                } else {
                    FutureDataInfoDlg chartDlg = new FutureDataInfoDlg();
                    chartDlg.setStockCode(stockData.code_);
                    chartDlg.Show();
                }
                break;

                //dlgInfo.addChart(stockData.code_, "차트 생성");
                //break;

                //case MouseButtons.Right:
                //dlgInfo.removeChart(stockData.code_, "차트 종료");
                //break;
            }
        }
    }
}
