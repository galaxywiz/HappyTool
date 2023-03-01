using HappyTool.Dlg;
using HappyTool.Stock;
using StockLibrary;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using UtilLibrary;

namespace HappyTool.DlgControl
{
    enum StockDrawLine
    {
        EMA,
        볼린져,
        ENVELOPE,
        추세선,
    }

    class StockPriceChart
    {
        protected Chart chartStock_;

        protected Font xlabelFont_ = UtilLibrary.Util.getFont(9.0F, FontStyle.Regular);

        protected string stockCode_ = "";
        protected StockData getStockData()
        {
            return ControlGet.getInstance.stockBot().getStockDataCode(stockCode_);
        }

        PRICE_TYPE botPriceType_;
        public StockPriceChart(string code)
        {
            stockCode_ = code;
            botPriceType_ = ControlGet.getInstance.stockBot().priceType_;
        }

        public virtual void setChartControl(StockChartDlg dlg)
        {
            chartStock_ = dlg.Chart_Stock;
            chartStock_.ChartAreas[0].AxisX.LabelStyle.Font = xlabelFont_;
            chartStock_.Titles.Add("기본 차트");
            chartStock_.GetToolTipText += this.chartStockToolTipText;
        }

        protected void chartStockToolTipText(object sender, ToolTipEventArgs e)
        {
            // Check selected chart element and set tooltip text for it
            switch (e.HitTestResult.ChartElementType) {
                case ChartElementType.DataPoint:
                    var dataPoint = e.HitTestResult.Series.Points[e.HitTestResult.PointIndex];
                    e.Text = string.Format("X:\t{0}\nY:\t{1:#,##0.#}", dataPoint.AxisLabel, dataPoint.YValues[0]);
                    break;
            }
        }

        protected double minPrice_ = 0, maxPrice_ = 0;
        protected StockData stockData_;

        void drawChart()
        {
            StockData stockData = this.getStockData();
            if (stockData == null) {
                return;
            }
            if (stockData.priceTable() == null) {
                return;
            }

            stockData_ = (StockData) stockData.Clone();

            this.drawStockBaseChart();
            this.setAxisXY();
        }

        public void drawStockChart()
        {
            if (chartStock_.InvokeRequired) {
                chartStock_.BeginInvoke(new Action(() => this.drawChart()));
            }
            else {
                this.drawChart();
            }
        }

        public virtual void removeStockGraph()
        {
            chartStock_.Series.Clear();
        }

        public void changedTrackValue(Object sender, EventArgs e)
        {
            this.drawStockChart();     //다시 그리기
        }

        //----------------------------------------------------------------------------
        // 차트 그리기
        protected virtual void drawStockBaseChart()
        {
            List<CandleData> priceTable = stockData_.priceTable();
            if (priceTable == null) {
                return;
            }

            minPrice_ = 0; maxPrice_ = 0;
            chartStock_.Series.Clear();

            int count = stockData_.priceDataCount();

            // 먼저 캔들 그리고
            this.drawCandleChart();
        }

        private void setAxisXY()
        {
            // x축 값을 설정하지 않으면 에러남... ;;;
            DateTimeIntervalType type = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Auto;
            string lableFmt = "yy/MM/dd";
            switch (botPriceType_) {
                case PRICE_TYPE.DAY:
                case PRICE_TYPE.WEEK:
                    type = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Days;
                    lableFmt = "yy/MM/dd";
                    break;
                case PRICE_TYPE.MIN_2:
                case PRICE_TYPE.MIN_3:
                    type = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Hours;
                    lableFmt = "yy/MM/dd:hh";
                    break;
                case PRICE_TYPE.MIN_1:
                    type = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Minutes;
                    lableFmt = "yy/MM/dd:hh/mm";
                    break;
            }
            chartStock_.ChartAreas[0].AxisX.ScrollBar.Enabled = true;
            chartStock_.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
            var min = chartStock_.ChartAreas[0].AxisX.ScaleView.ViewMinimum;
            var max = chartStock_.ChartAreas[0].AxisX.ScaleView.ViewMaximum;

            chartStock_.ChartAreas[0].AxisX.IsLabelAutoFit = false;
            chartStock_.ChartAreas[0].AxisX.LabelStyle.Angle = 0;

            chartStock_.ChartAreas[0].AxisX.LabelStyle.IntervalType = type;
            chartStock_.ChartAreas[0].AxisX.LabelStyle.IntervalOffsetType = type;
            chartStock_.ChartAreas[0].AxisX.LabelStyle.Format = lableFmt;
            chartStock_.ChartAreas[0].AxisY.LabelStyle.Interval = 10;
            chartStock_.ChartAreas[0].AxisY.IsLogarithmic = true;

            chartStock_.ChartAreas[0].AxisY.LabelStyle.Interval = (maxPrice_ - minPrice_) / 15;
            chartStock_.ChartAreas[0].AxisY.Minimum = minPrice_;
            chartStock_.ChartAreas[0].AxisY.Maximum = maxPrice_;
            chartStock_.ChartAreas[0].AxisY.LabelStyle.Format = "{0:#,##0}";
        }

        protected void addBasicCharDataPool(ref Dictionary<DateTime, double[]> chartDataPool, CandleData priceData)
        {
            // 캔들 주식 값의 y축 데이터는 고가, 저가, 시가, 종가 순임
            double[] value = { priceData.highPrice_, priceData.lowPrice_, priceData.startPrice_, priceData.price_ };
            chartDataPool.Add(priceData.date_, value);

            if (minPrice_ == 0) {
                minPrice_ = priceData.lowPrice_;
                maxPrice_ = priceData.highPrice_;
            }
            double lowPrice = priceData.lowPrice_;
            double highPrice = priceData.highPrice_;

            minPrice_ = Math.Min(minPrice_, lowPrice);
            maxPrice_ = Math.Max(maxPrice_, highPrice);
        }

        protected void addCharDataPool(ref Dictionary<DateTime, double> chartDataPool, CandleData priceData, double value)
        {
            if (value < 10) {
                return;
            }
            chartDataPool.Add(priceData.date_, value);
            this.setMinMaxPrice(value);
        }

        protected virtual void setMinMaxPrice(double value)
        {
            if (minPrice_ == 0) {
                minPrice_ = maxPrice_ = value;
            }
            if (value < 100.0f) {
                return;
            }
            minPrice_ = Math.Min(minPrice_, value - (value * 10 / 100));
            maxPrice_ = Math.Max(maxPrice_, value + (value * 10 / 100));
        }

        protected const string basicChartName_ = "가격선";
        // 기본 캔들 차트 그림
        private void drawCandleChart()
        {
            List<CandleData> priceTable = stockData_.priceTable();

            Series chart = chartStock_.Series.Add(basicChartName_);

            chart.ChartType = SeriesChartType.Candlestick;
            chart["PriceUpColor"] = "LightPink";
            chart["PriceDownColor"] = "DeepSkyBlue";
            chart.BorderWidth = 1;

            Dictionary<DateTime, double[]> chartDataPool = new Dictionary<DateTime, double[]>();

            foreach (var priceData in priceTable) {
                this.addBasicCharDataPool(ref chartDataPool, priceData);
            }

            foreach (KeyValuePair<DateTime, double[]> chartData in chartDataPool) {
                int i = 0;
                chart.Points.AddXY(chartData.Key, chartData.Value[i++], chartData.Value[i++], chartData.Value[i++], chartData.Value[i++]);
            }
        }

        // 축에 수평인 선을 긋는다
        protected void drawStripeLine(Axis axis, double val, Color color)
        {
            StripLine line = new StripLine();
            line.Interval = 0;
            line.StripWidth = 1;
            line.IntervalOffset = val;
            line.BackColor = color;
            line.ToolTip = val.ToString();

            axis.StripLines.Add(line);
        }

        protected void addSpritLine(int price)
        {
            chartStock_.ChartAreas[0].AxisY.StripLines.Add(
            new StripLine() {
                BorderColor = Color.Red,
                IntervalOffset = price,
                Text = "추세선"
            });
        }

        //----------------------------------------------------------------------------
        // 이하 기술적 가격 계산에 대한 그리기
        protected void drawSimpleAvg()
        {
            List<CandleData> priceTable = stockData_.priceTable();

            for (AVG_SAMPLEING avgIdx = AVG_SAMPLEING.AVG_5; avgIdx < AVG_SAMPLEING.AVG_MAX; ++avgIdx) {
                Series chart = chartStock_.Series.Add("단순 " + avgIdx.ToString());
                chart.ChartType = SeriesChartType.Line;
                chart.BorderDashStyle = ChartDashStyle.Dot;

                Dictionary<DateTime, double> chartDataPool = new Dictionary<DateTime, double>();

                foreach (var priceData in priceTable) {
                    double avg = priceData.calc_[(int) EVALUATION_DATA.SMA_5 + (int) avgIdx];
                    this.addCharDataPool(ref chartDataPool, priceData, avg);
                }

                foreach (KeyValuePair<DateTime, double> chartData in chartDataPool) {
                    chart.Points.AddXY(chartData.Key, chartData.Value);
                }
            }
        }
    }

    class BasicStockChart : StockPriceChart
    {
        public BasicStockChart(string code) : base(code)
        {
        }

        public override void setChartControl(StockChartDlg dlg)
        {
            base.setChartControl(dlg);
        }

        public override void removeStockGraph()
        {
            base.removeStockGraph();
        }

        protected override void drawStockBaseChart()
        {
            base.drawStockBaseChart();
        }
    }
}
