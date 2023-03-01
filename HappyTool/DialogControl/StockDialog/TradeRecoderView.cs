using HappyTool.Stock;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using UtilLibrary;

namespace HappyTool.DialogControl.StockDialog
{
    class TodayTradeRecoderView: QuickDataViewCtl
    {
        internal enum COLUMNS_NAME
        {
            종목코드, 종목명, 보유수량, 평균단가, 손익금액, 손익율
        };

        protected override void decorationView()
        {
            this.dataGridView_.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
            int maxRow = Math.Max(this.dataGridView_.RowCount - 1, 0);
            for (int rowIdx = 0; rowIdx < maxRow; ++rowIdx) {
                double price = 0.0f;
                if (double.TryParse(this.dataGridView_.Rows[rowIdx].Cells[(int) COLUMNS_NAME.손익금액].Value.ToString(), out price) == false) {
                    continue;
                }
                if (price > 0) {
                    this.dataGridView_.Rows[rowIdx].DefaultCellStyle.BackColor = Color.LightPink;
                }
                else if (price < 0) {
                    this.dataGridView_.Rows[rowIdx].DefaultCellStyle.BackColor = Color.SkyBlue;
                }
            }
        }

        public override DataTable makeNewTable()
        {
            DataTable dt = new DataTable();
            foreach (COLUMNS_NAME name in Enum.GetValues(typeof(COLUMNS_NAME))) {
                if (name == COLUMNS_NAME.종목명) {
                    dt.Columns.Add(name.ToString(), typeof(string));
                    continue;
                } 
                dt.Columns.Add(name.ToString(), typeof(double));
            }
            return dt;
        }

        public override void print()
        {
            base.print();

            // 마지막 행이 중요...
            this.dataGridView_.FirstDisplayedScrollingRowIndex = this.dataGridView_.Rows.Count - 1;
        }

        public override void update()
        {
            var bot = ControlGet.getInstance.stockBot();
            var engine = bot.engine_;

            engine.addOrder(new AccountStockStatement());
        }
    }

    class TradeHistoryView: QuickDataViewCtl
    {
        internal enum COLUMNS_NAME
        {
            일자, 예수금_초, 예수금_말, 투자원금평잔, 평가손익, 수익율
        };

        DateTimePicker startDate_;
        DateTimePicker endDate_;
        QuickChart chart_;

        enum CHART_AREA
        {
            예수금,
        }

        public override void setup(QuickDataGridView dataView)
        {
            base.setup(dataView);

            var dlg = Program.happyTool_.stockDlg_;
            this.startDate_ = dlg.dateTimePicker_MoneyStart;
            this.startDate_.Value = DateTime.Now.AddDays(-30);

            this.endDate_ = dlg.dateTimePicker_MoneyEnd;
            DateTime now = DateTime.Now;
            int hour = now.Hour;
            int stand = 9;
            if (hour < stand) {
                now = now.AddHours(-(hour + 1));
            }
            this.endDate_.Value = now;

            this.chart_ = dlg.Chart_tradeHistory;
            foreach (CHART_AREA area in Enum.GetValues(typeof(CHART_AREA))) {
                this.chart_.ChartAreas.Add(area.ToString());
            }
        }

        protected override void decorationView()
        {
            this.dataGridView_.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
            int maxRow = Math.Max(this.dataGridView_.RowCount - 1, 0);
            for (int rowIdx = 0; rowIdx < maxRow; ++rowIdx) {
                double price = 0.0f;
                if (double.TryParse(this.dataGridView_.Rows[rowIdx].Cells[(int) COLUMNS_NAME.평가손익].Value.ToString(), out price) == false) {
                    continue;
                }
                if (price > 0) {
                    this.dataGridView_.Rows[rowIdx].DefaultCellStyle.BackColor = Color.LightPink;
                }
                else if (price < 0) {
                    this.dataGridView_.Rows[rowIdx].DefaultCellStyle.BackColor = Color.SkyBlue;
                }
            }
        }

        public override DataTable makeNewTable()
        {
            DataTable dt = new DataTable();
            foreach (COLUMNS_NAME name in Enum.GetValues(typeof(COLUMNS_NAME))) {
                if (name == COLUMNS_NAME.일자) {
                    dt.Columns.Add(name.ToString(), typeof(DateTime));
                }
                else {
                    dt.Columns.Add(name.ToString(), typeof(double));
                }
            }
            return dt;
        }

        void drawAverageLine(Axis axis, double avg, Color color)
        {
            StripLine stripeLine = new StripLine {
                Interval = 0,
                StripWidth = 2,
                IntervalOffset = avg,
                BackColor = color,
                ToolTip = avg.ToString()
            };

            axis.StripLines.Add(stripeLine);
        }

        void setChartSeries()
        {
            if (this.dataTable_.Rows.Count == 0) {
                return;
            }
            this.chart_.DataSource = this.dataTable_;
            Series chartPrepareMoney = this.chart_.Series.Add("예수금 추이");
            chartPrepareMoney.ChartType = SeriesChartType.Line;
            chartPrepareMoney.XValueMember = COLUMNS_NAME.일자.ToString();
            chartPrepareMoney.YValueMembers = COLUMNS_NAME.평가손익.ToString();
            chartPrepareMoney.ChartArea = CHART_AREA.예수금.ToString();
            chartPrepareMoney.XValueType = ChartValueType.DateTime;

            //  drawAverageLine(chart_.ChartAreas[CHART_AREA.이익율.ToString()].AxisX, 0, Color.ForestGreen);
        }

        void setChartAreas()
        {
            if (this.dataTable_.Rows.Count == 0) {
                return;
            }

            DateTimeIntervalType type = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Auto;
            type = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Days;

            foreach (CHART_AREA area in Enum.GetValues(typeof(CHART_AREA))) {
                this.chart_.ChartAreas[area.ToString()].AxisX.LabelStyle.IntervalType = type;
                this.chart_.ChartAreas[area.ToString()].AxisX.LabelStyle.IntervalOffsetType = type;
                this.chart_.ChartAreas[area.ToString()].AxisX.LabelStyle.Format = "yy/MM/dd";
            }

            this.chart_.ChartAreas[CHART_AREA.예수금.ToString()].AxisY.LabelStyle.Format = "{0:#,##0.##} $";
        }

        public string chartCapture()
        {
            return this.chart_.captureView("TradeHistory.png");
        }

        public override void print(DataTable dt)
        {
            base.print(dt);

            this.chart_.Series.Clear();
            this.setChartSeries();
            this.setChartAreas();

            if (dt.Rows.Count == 0) {
                return;
            }
            var bot = ControlGet.getInstance.stockBot();
            DataRow row = dt.Rows[0];
            //bot.todayTotalProfitRate_ = (double) row[(int) COLUMNS_NAME.손익율];
            //bot.todayTotalProfit_ = (double) row[(int) COLUMNS_NAME.일별손익금액];
        }

        public override void update()
        {
            var bot = ControlGet.getInstance.stockBot();
            var engine = bot.engine_;

            if (this.startDate_ != null && this.endDate_ != null) {
                TradeHistoryStatement statement = new TradeHistoryStatement {
                    dateStart_ = this.startDate_.Value,
                    dateEnd_ = this.endDate_.Value
                };

                engine.addOrder(statement);
            }
        }
    }
}
