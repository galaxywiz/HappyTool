using NetLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UtilLibrary;

namespace HappyMonitor
{
    class MonitorViewer: QuickDataViewCtl
    {
        internal enum COLUMNS_NAME
        {
            서버이름, 상태, 일하는중, 업데이트시간, 예수금, 금일_이익금, 승리횟수, 거래횟수, 승율, 포트폴리오
        };

        void updateItem(ref DataTable dt, User user)
        {
            var monitor = user;

            var serverName = monitor.serverNmae_;
            var state = monitor.stateName_;
            var working = monitor.nowWorking_;
            var lastUpdate = monitor.lastHeartBeat_;
            var account = monitor.account_;
            var profit = monitor.todayProfit_;
            var winCount = monitor.winCount_;
            var tradingCount = monitor.tradingCount_;
            var winRate = winCount / (double) tradingCount;
            var strategy = monitor.strategyName_;

            dt.Rows.Add(serverName, state, working, lastUpdate, account, profit, winCount, tradingCount, winRate, strategy);
        }

        protected override void decorationView()
        {
            if (this.dataGridView_.Columns.Count == 0) {
                return;
            }
            foreach (DataGridViewColumn column in this.dataGridView_.Columns) {
                string fmt = "#,###0.####";
                if (column.HeaderText == COLUMNS_NAME.업데이트시간.ToString()) { 
                    fmt = "tt hh:mm";
                }
                column.DefaultCellStyle.Format = fmt;
            }

            this.dataGridView_.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
            int maxRow = Math.Max(this.dataGridView_.RowCount - 1, 0);

            var bot = Program.mainDlg_.server_;

            for (int rowIdx = 0; rowIdx < maxRow; ++rowIdx) {
                // 매수 / 매도는 다른 색으로
                string text = this.dataGridView_.Rows[rowIdx].Cells[(int) COLUMNS_NAME.금일_이익금].Value.ToString();
                double profit = 0.0f;
                if (double.TryParse(text, out profit) == false) {
                    continue;
                }
                
                Color color = Color.Empty;
                // 목표값에 따른 컬럼 색 변경
                if (profit > 0) {
                    color = Color.Pink;
                } else if (profit < 0) {
                    color = Color.SkyBlue;
                }
                
                this.dataGridView_.Rows[rowIdx].DefaultCellStyle.BackColor = color;
            }
            this.dataGridView_.Columns[(int) COLUMNS_NAME.서버이름].Frozen = true;
            this.dataGridView_.Columns[(int) COLUMNS_NAME.서버이름].DividerWidth = 3; // 구분선 폭 설정
        }

        public override DataTable makeNewTable()
        {
            DataTable dt = new DataTable();
            foreach (COLUMNS_NAME name in Enum.GetValues(typeof(COLUMNS_NAME))) {
                if (
                  name == COLUMNS_NAME.서버이름
                 || name == COLUMNS_NAME.포트폴리오
                 || name == COLUMNS_NAME.상태) { 
                    dt.Columns.Add(name.ToString(), typeof(string));
                }
                else if (name == COLUMNS_NAME.업데이트시간) { 
                    dt.Columns.Add(name.ToString(), typeof(DateTime));
                }
                else {
                    dt.Columns.Add(name.ToString(), typeof(double));
                }
            }

            var bot = Program.mainDlg_.server_;
            doUserEach eachDo = (User user) => {
                this.updateItem(ref dt, user);
            };
            bot.doUsers(eachDo);

            double sumProfit = bot.todayProfitSum();
            //dt.Rows.Add(serverName, state, working, lastUpdate, account, profit, winCount, tradingCount, winRate, strategy);
            dt.Rows.Add("총합", "", 0, DateTime.Now, 0, sumProfit);
            if (beforeProfit_ == double.MinValue) {
                beforeProfit_ = sumProfit;
                string log = string.Format("* 현재 행복들의 이익금 \n => {0:##,###.##} $", sumProfit);
                Program.mainDlg_.sendMessage(log);
            } else if (sumProfit != beforeProfit_) {
                beforeProfit_ = sumProfit;
                string log = string.Format("* 이익금 변동 알림\n => {0:##,###.##} $", sumProfit);
                Program.mainDlg_.sendMessage(log);
            }
            return dt;
        }
        double beforeProfit_ = double.MinValue;

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
    }
}
