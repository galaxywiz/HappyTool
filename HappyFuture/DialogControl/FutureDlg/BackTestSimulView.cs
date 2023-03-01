using StockLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using UtilLibrary;
using static StockLibrary.TradeStrategyTestRecoder;

namespace HappyFuture.DialogControl.FutureDlg
{
    class BackTestSimulView: QuickDataViewCtl
    {
        DateTimePicker startDate_;
        DateTimePicker endDate_;
        QuickChart chartAccount_;
        QuickChart chartEachTotal_;

        enum CHART_ACCOUNT_AREA
        {
            이익금추이,
        }
        enum CHART_TOTAL_AREA
        {
            종목당_이익금
        }

        public override void setup(QuickDataGridView dataView)
        {
            base.setup(dataView);

            var dlg = Program.happyFuture_.futureDlg_;
            this.startDate_ = dlg.dateTimePicker_backTestStart;

            var day = -30;
            if (this.startDate_.InvokeRequired) {
                this.startDate_.BeginInvoke(new Action(() => this.startDate_.Value = DateTime.Now.AddDays(day)));
            }
            else {
                this.startDate_.Value = DateTime.Now.AddDays(day);
            }

            this.endDate_ = dlg.dateTimePicker_backTestEnd;
            if (this.endDate_.InvokeRequired) {
                this.endDate_.BeginInvoke(new Action(() => this.endDate_.Value = DateTime.Now));
            }
            else {
                this.endDate_.Value = DateTime.Now;
            }

            this.chartAccount_ = dlg.Chart_backTestAccountResult;
            this.chartEachTotal_ = dlg.Chart_TotalPrice;
            this.setChart();
        }

        void initChart()
        {
            this.chartAccount_.ChartAreas.Clear();
            foreach (CHART_ACCOUNT_AREA area in Enum.GetValues(typeof(CHART_ACCOUNT_AREA))) {
                this.chartAccount_.ChartAreas.Add(area.ToString());
            }

            this.chartEachTotal_.ChartAreas.Clear();
            foreach (CHART_TOTAL_AREA area in Enum.GetValues(typeof(CHART_TOTAL_AREA))) {
                this.chartEachTotal_.ChartAreas.Add(area.ToString());
            }

        }
        void setChart()
        {
            if (this.chartAccount_.InvokeRequired) {
                this.chartAccount_.BeginInvoke(new Action(() => this.initChart()));
            }
            else {
                this.initChart();
            }
        }

        void setChartSeries()
        {
            try {
                if (this.dataTable_.Rows.Count == 0) {
                    return;
                }
                this.chartAccount_.DataSource = this.dataTable_;

                var chartProfit = this.chartAccount_.Series.Add(CHART_ACCOUNT_AREA.이익금추이.ToString());
                chartProfit.ChartType = SeriesChartType.Line;
                chartProfit.XValueMember = BACKTEST_RECODE_TABLE_COLUMN.청산시점.ToString();
                chartProfit.YValueMembers = BACKTEST_RECODE_TABLE_COLUMN.누적수익금.ToString();
                chartProfit.ChartArea = CHART_ACCOUNT_AREA.이익금추이.ToString();
                chartProfit.XValueType = ChartValueType.DateTime;

                this.chartAccount_.DataBind();

                // 각각의 수익금 차트
                var recodeEach = new Dictionary<string, double>();
                int maxRow = Math.Max(this.dataGridView_.RowCount - 1, 0);
                for (int rowIdx = 0; rowIdx < maxRow; ++rowIdx) {
                    string code = this.dataGridView_.Rows[rowIdx].Cells[(int) BACKTEST_RECODE_TABLE_COLUMN.코드].Value.ToString();
                    if (code.Length == 0) {
                        continue;
                    }
                    if (recodeEach.ContainsKey(code)) {
                        continue;
                    }
                    double sum = 0.0f;
                    for (int rowIdx2 = 0; rowIdx2 < maxRow; ++rowIdx2) {
                        string code2 = this.dataGridView_.Rows[rowIdx2].Cells[(int) BACKTEST_RECODE_TABLE_COLUMN.코드].Value.ToString();
                        if (code2 != code) {
                            continue;
                        }
                        double profit;
                        if (double.TryParse(this.dataGridView_.Rows[rowIdx2].Cells[(int) BACKTEST_RECODE_TABLE_COLUMN.이익].Value.ToString(), out profit)) {
                            sum += profit;
                        }
                    }
                    recodeEach[code] = sum;
                }

                var eachProfit = this.chartEachTotal_.Series.Add(CHART_TOTAL_AREA.종목당_이익금.ToString());
                eachProfit.ChartType = SeriesChartType.Column;
                eachProfit.Points.DataBindXY(recodeEach.Keys, recodeEach.Values);
            }
            catch (Exception e) {
                Logger.getInstance.print(KiwoomCode.Log.에러, "차트 그리다가 에러 {0} / {1}", e.Message, e.StackTrace);
            }
        }

        void setChartAreas()
        {
            if (this.dataTable_.Rows.Count == 0) {
                return;
            }

            DateTimeIntervalType type = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Auto;
            type = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Auto;

            foreach (CHART_ACCOUNT_AREA area in Enum.GetValues(typeof(CHART_ACCOUNT_AREA))) {
                var chart = this.chartAccount_.ChartAreas[area.ToString()];
                chart.AxisX.LabelStyle.IntervalType = type;
                chart.AxisX.LabelStyle.IntervalOffsetType = type;
                chart.AxisX.LabelStyle.Format = "yy/MM/dd\nHH:mm";
                chart.AxisY.LabelStyle.Format = "{0:##,###0.#####} $";
            }
            var chartLegends = this.chartAccount_.Legends[0];
            chartLegends.Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Top;
            chartLegends.Alignment = System.Drawing.StringAlignment.Center;

            foreach (CHART_TOTAL_AREA area in Enum.GetValues(typeof(CHART_TOTAL_AREA))) {
                var chart = this.chartEachTotal_.ChartAreas[area.ToString()];
                chart.AxisY.LabelStyle.Format = "{0:##,###0.#####} $";
            }

            chartLegends = this.chartEachTotal_.Legends[0];
            chartLegends.Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Top;
            chartLegends.Alignment = System.Drawing.StringAlignment.Center;

        }

        void drawChart()
        {
            this.chartAccount_.Series.Clear();
            this.chartEachTotal_.Series.Clear();
            this.setChartSeries();
            this.setChartAreas();
        }

        //---------------------------------------------------------------//
        // 데이터 뷰

        protected override void decorationView()
        {
            foreach (DataGridViewColumn column in this.dataGridView_.Columns) {
                if (column.HeaderText == BACKTEST_RECODE_TABLE_COLUMN.코드.ToString()
                    || column.HeaderText == BACKTEST_RECODE_TABLE_COLUMN.포지션.ToString()                  
                    || column.HeaderText == BACKTEST_RECODE_TABLE_COLUMN.보유시간.ToString()
                    || column.HeaderText == BACKTEST_RECODE_TABLE_COLUMN.매수갯수.ToString()
                      || column.HeaderText == BACKTEST_RECODE_TABLE_COLUMN.매수모듈.ToString()
                       || column.HeaderText == BACKTEST_RECODE_TABLE_COLUMN.매도모듈.ToString()
                    ) {
                } else if (
                      column.HeaderText == BACKTEST_RECODE_TABLE_COLUMN.매수시점.ToString()
                    || column.HeaderText == BACKTEST_RECODE_TABLE_COLUMN.청산시점.ToString()) {
                    column.DefaultCellStyle.Format = "yyyy-MM-dd HH:mm:ss";
                }
                else {
                    column.DefaultCellStyle.Format = "#,###0.####";
                }

            }
            int maxRow = Math.Max(this.dataGridView_.RowCount - 1, 0);
            for (int rowIdx = 0; rowIdx < maxRow; ++rowIdx) {
                // 매수 / 매도는 다른 색으로
                var minProfit = double.Parse(this.dataGridView_.Rows[rowIdx].Cells[(int) BACKTEST_RECODE_TABLE_COLUMN.최소1주이익].Value.ToString());
                var oneProfit = double.Parse(this.dataGridView_.Rows[rowIdx].Cells[(int) BACKTEST_RECODE_TABLE_COLUMN.청산시1주이익].Value.ToString());
                Color color = Color.SkyBlue;
                if (oneProfit - PublicFutureVar.pureFeeTex > 0) {
                    color = Color.Pink;
                }
                this.dataGridView_.Rows[rowIdx].DefaultCellStyle.BackColor = color;
            }
            this.dataGridView_.FirstDisplayedScrollingRowIndex = this.dataGridView_.Rows.Count - 1;
        }

        public override void print(DataTable dt)
        {
            this.dataTable_ = dt;

            base.print(dt);

            if (this.chartAccount_.InvokeRequired) {
                this.chartAccount_.BeginInvoke(new Action(() => this.drawChart()));
            }
            else {
                this.drawChart();
            }
        }

        public override void update()
        {
            if (this.startDate_ != null && this.endDate_ != null) {
            }
        }
    }
}
