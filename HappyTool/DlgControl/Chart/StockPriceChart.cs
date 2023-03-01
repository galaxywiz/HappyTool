using HappyTool.Dlg;
using HappyTool.Stock;
using HappyTool.Stock.Calculate;
using HappyTool.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace HappyTool.DlgControl
{
    enum StockDrawLine
    {
        EMA,
        BOLL,
        BNF,
        LINE,
    }

    class StockPriceChart 
    {
        protected Chart chartStock_;
        protected int limitXCount_ = 150;
        protected TrackBar trackBar_;

        protected PRICE_TYPE priceType_;
        protected Font xlabelFont_ = MyUtil.getFont(9.0F, FontStyle.Regular);

        protected int stockCode_ = 0;
        protected StockData getStockData()
        {
            return StockBot.getInstance.stockData(stockCode_);
        }

        public StockPriceChart(int code)
        {
            stockCode_ = code;
        }

        public virtual void setChartControl(StockChartDlg dlg)
        {
            chartStock_ = dlg.chart_Stock();
            chartStock_.ChartAreas[0].AxisX.LabelStyle.Font = xlabelFont_;
            chartStock_.Titles.Add("기본 차트");
            chartStock_.GetToolTipText += this.chartStockToolTipText;
            trackBar_ = dlg.trackBarStock();
        }

        protected void chartStockToolTipText(object sender, ToolTipEventArgs e)
        {
            // Check selected chart element and set tooltip text for it
            switch (e.HitTestResult.ChartElementType) {
            case ChartElementType.DataPoint:
                var dataPoint = e.HitTestResult.Series.Points[e.HitTestResult.PointIndex];
                e.Text = string.Format("X:\t{0}\nY:\t{1:#,##.#}", dataPoint.AxisLabel, dataPoint.YValues[0]);
                break;
            }
        }

        public virtual void setPriceType(PRICE_TYPE type)
        {
            StockData stockData = this.getStockData();
            if (stockData == null) {
                return;
            }
            List<CandleData> priceTable = stockData.priceTable(type);
            if (priceTable == null) {
                return;
            }

            trackBar_.Maximum = priceTable.Count;
            trackBar_.Minimum = 5;
            trackBar_.Value = (priceTable.Count - 5) / 2;

            priceType_ = type;
            this.drawStock();     //다시 그리기
        }

        protected double minPrice_ = 0, maxPrice_ = 0;
        protected StockData stockData_;

        protected void drawStock()
        {
            StockData stockData = this.getStockData();
            if (stockData == null) {
                return;
            }
            if (stockData.priceTable(priceType_) == null) {
                return;
            }

            stockData_ = (StockData) stockData.Clone();      

            this.drawStockGraph();
            this.setAxisXY();
        }

        public virtual void removeStockGraph()
        {
            chartStock_.Series.Clear();
        }

        public void changedTrackValue(Object sender, EventArgs e)
        {
            limitXCount_ = trackBar_.Value;
            this.drawStock();     //다시 그리기
        }

        //----------------------------------------------------------------------------
        // 차트 그리기
        protected virtual void drawStockGraph()
        {
            List<CandleData> priceTable = stockData_.priceTable(priceType_);
            if (priceTable == null) {
                return;
            }
            
            minPrice_ = 0; maxPrice_ = 0;
            chartStock_.Series.Clear();

            int count = stockData_.priceTable(priceType_).Count;
            trackBar_.Maximum = count - 1;

            // 먼저 캔들 그리고
            this.drawCandleChart();
        }

        private void setAxisXY()
        {
            // x축 값을 설정하지 않으면 에러남... ;;;
            DateTimeIntervalType type = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Auto;
            string lableFmt = "yy/MM/dd";
            switch (priceType_)
            {
                case PRICE_TYPE.DAY:
                case PRICE_TYPE.WEEK:
                    type = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Days;
                    lableFmt = "yy/MM/dd";
                    break;
                case PRICE_TYPE.HOUR1:
                case PRICE_TYPE.HOUR4:
                    type = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Hours;
                    lableFmt = "yy/MM/dd:hh";
                    break;
                case PRICE_TYPE.MIN15:
                    type = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Minutes;
                    lableFmt = "yy/MM/dd:hh/mm";
                    break;
            }
            chartStock_.ChartAreas[0].AxisX.IsLabelAutoFit = false;
            chartStock_.ChartAreas[0].AxisX.LabelStyle.Angle = 90;

            chartStock_.ChartAreas[0].AxisX.LabelStyle.IntervalType = type;
            chartStock_.ChartAreas[0].AxisX.LabelStyle.IntervalOffsetType = type;
            chartStock_.ChartAreas[0].AxisX.LabelStyle.Format = lableFmt;
            chartStock_.ChartAreas[0].AxisY.LabelStyle.Interval = 5;

            chartStock_.ChartAreas[0].AxisY.LabelStyle.Interval = (maxPrice_ - minPrice_) / 15;
            chartStock_.ChartAreas[0].AxisY.Minimum = minPrice_;
            chartStock_.ChartAreas[0].AxisY.Maximum = maxPrice_;
            chartStock_.ChartAreas[0].AxisY.LabelStyle.Format = "{0:#,##0}";
        }

        protected void addBasicCharDataPool(ref Dictionary<string, int[]> chartDataPool, CandleData priceData)
        {
            System.DateTime dt = priceData.date_;
            // 캔들 주식 값의 y축 데이터는 고가, 저가, 시가, 종가 순임
            int[] value = { priceData.highPrice_, priceData.lowPrice_, priceData.startPrice_, priceData.price_ };

            switch (priceType_)
            {
                case PRICE_TYPE.WEEK:
                case PRICE_TYPE.DAY:
                    chartDataPool.Add(dt.ToString("yy/MM/dd"), value);
                    break;
                case PRICE_TYPE.HOUR4:
                case PRICE_TYPE.HOUR1:
                    chartDataPool.Add(dt.ToString("yy/MM/dd:hh"), value);
                    break;
                case PRICE_TYPE.MIN15:
                    chartDataPool.Add(dt.ToString("yy/MM/dd:hh/mm"), value);
                    break;
            }

            if (minPrice_ == 0)
            {
                minPrice_ = priceData.lowPrice_;
                maxPrice_ = priceData.highPrice_;
            }
            double lowPrice = priceData.lowPrice_;
            double highPrice = priceData.highPrice_;

            minPrice_ = Math.Min(minPrice_, lowPrice);
            maxPrice_ = Math.Max(maxPrice_, highPrice);
        }

        protected void addCharDataPool(ref Dictionary<string, double> chartDataPool, CandleData priceData, double value)
        {
            System.DateTime dt = priceData.date_;

            switch (priceType_)
            {
                case PRICE_TYPE.WEEK:
                case PRICE_TYPE.DAY:
                    chartDataPool.Add(dt.ToString("yy/MM/dd"), value);
                    break;
                case PRICE_TYPE.HOUR4:
                case PRICE_TYPE.HOUR1:
                    chartDataPool.Add(dt.ToString("yy/MM/dd:hh"), value);
                    break;
                case PRICE_TYPE.MIN15:
                    chartDataPool.Add(dt.ToString("yy/MM/dd:hh/mm"), value);
                    break;
            }

            this.setMinMaxPrice(value);
        }

        protected virtual void setMinMaxPrice(double value)
        {
            if (minPrice_ == 0)
            {
                minPrice_ = maxPrice_ = value;
            }
            if (value < 100.0f)
            {
                return;
            }
            minPrice_ = Math.Min(minPrice_, value - (value * 10 / 100));
            maxPrice_ = Math.Max(maxPrice_, value + (value * 10 / 100));
        }

        protected const string basicChartName_ = "가격선";
        // 기본 캔들 차트 그림
        private void drawCandleChart()
        {
            List<CandleData> priceTable = stockData_.priceTable(priceType_);
            int xMax = priceTable.Count;
            if (xMax < 0) {
                return;
            }
            xMax = Math.Min(limitXCount_, priceTable.Count - 1);

            Series chart = chartStock_.Series.Add(basicChartName_);

            chart.ChartType = SeriesChartType.Candlestick;
            chart["PriceUpColor"] = "LightPink";
            chart["PriceDownColor"] = "DeepSkyBlue";
            chart.BorderWidth = 1;

            Dictionary<string, int[]> chartDataPool = new Dictionary<string, int[]>();

            for (int dateIdx = xMax; dateIdx >= 0; --dateIdx) {
                CandleData priceData = priceTable[dateIdx];
                this.addBasicCharDataPool(ref chartDataPool, priceData);
            }

            foreach (KeyValuePair<string, int[]> chartData in chartDataPool) {
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

        //----------------------------------------------------------------------------
        // 이하 기술적 가격 계산에 대한 그리기
        protected void drawSimpleAvg()
        {
            List<CandleData> priceTable = stockData_.priceTable(priceType_);
            int xMax = Math.Min(limitXCount_, priceTable.Count);

            for (AVG_SAMPLEING avgIdx = AVG_SAMPLEING.AVG_5; avgIdx < AVG_SAMPLEING.AVG_MAX; ++avgIdx) {
                Series chart = chartStock_.Series.Add("단순 " + avgIdx.ToString());
                chart.ChartType = SeriesChartType.Line;
                chart.BorderDashStyle = ChartDashStyle.Dot;

                Dictionary<string, double> chartDataPool = new Dictionary<string, double>();

                for (int dateIdx = xMax; dateIdx >= 0; --dateIdx) {
                    CandleData priceData = priceTable[dateIdx];
                    double avg = priceTable[dateIdx].calc_[(int) EVALUATION_DATA.SMA_START + (int) avgIdx];
                    this.addCharDataPool(ref chartDataPool, priceData, avg);
                }

                foreach (KeyValuePair<string, double> chartData in chartDataPool) {
                    chart.Points.AddXY(chartData.Key, chartData.Value);
                }
            }
        }

        protected void drawExpAvg()
        {
            List<CandleData> priceTable = stockData_.priceTable(priceType_);
            int xMax = Math.Min(limitXCount_, priceTable.Count - 1);

            for (AVG_SAMPLEING avgIdx = AVG_SAMPLEING.AVG_5; avgIdx < AVG_SAMPLEING.AVG_MAX; ++avgIdx) {
                Series chart = chartStock_.Series.Add("지수 " + avgIdx.ToString());
                chart.ChartType = SeriesChartType.Line;
                chart.BorderDashStyle = ChartDashStyle.DashDot;

                Dictionary<string, double> chartDataPool = new Dictionary<string, double>();

                for (int dateIdx = xMax; dateIdx >= 0; --dateIdx) {
                    CandleData priceData = priceTable[dateIdx];
                    double avg = priceTable[dateIdx].calc_[(int) EVALUATION_DATA.EMA_START + (int) avgIdx];
                    this.addCharDataPool(ref chartDataPool, priceData, avg);
                }

                foreach (KeyValuePair<string, double> chartData in chartDataPool) {
                    chart.Points.AddXY(chartData.Key, chartData.Value);
                }
            }
        }

        protected void drawBnfLine()
        {
            List<CandleData> priceTable = stockData_.priceTable(priceType_);
            int xMax = Math.Min(limitXCount_, priceTable.Count - 1);

            for (BNG_LINE bnfIdx = BNG_LINE.BNF_UPPER; bnfIdx < BNG_LINE.BNF_MAX; ++bnfIdx)
            {
                Series chart = chartStock_.Series.Add(bnfIdx.ToString());
                chart.ChartType = SeriesChartType.Line;
                chart.BorderDashStyle = ChartDashStyle.Solid;

                Dictionary<string, double> chartDataPool = new Dictionary<string, double>();

                for (int dateIdx = xMax; dateIdx >= 0; --dateIdx)
                {
                    CandleData priceData = priceTable[dateIdx];
                    double bnf = priceTable[dateIdx].calc_[(int)EVALUATION_DATA.BNF_UPPER + (int)bnfIdx];
                    this.addCharDataPool(ref chartDataPool, priceData, bnf);
                }

                foreach (KeyValuePair<string, double> chartData in chartDataPool)
                {
                    chart.Points.AddXY(chartData.Key, chartData.Value);
                }
            }
        }

        delegate void drawingBollinger(bool upper);
        protected void drawBollinger()
        {
            List<CandleData> priceTable = stockData_.priceTable(priceType_);
            int xMax = Math.Min(limitXCount_, priceTable.Count - 1);

            drawingBollinger drawing = (upper) => {
                Series chart;
                if (upper) {
                    chart = chartStock_.Series.Add("볼린져 상한");
                } else {
                    chart = chartStock_.Series.Add("볼린져 하한");
                }
                chart.ChartType = SeriesChartType.Line;
                chart.BorderWidth = 2;
                chart.BorderDashStyle = ChartDashStyle.Dash;

                Dictionary<string, double> chartDataPool = new Dictionary<string, double>();

                for (int dateIdx = xMax; dateIdx >= 0; --dateIdx) {
                    CandleData priceData = priceTable[dateIdx];
                    double price = priceTable[dateIdx].calc_[(int) EVALUATION_DATA.BOLLINGER_LOWER];
                    if (upper) {
                        price = priceTable[dateIdx].calc_[(int) EVALUATION_DATA.BOLLINGER_UPPER];
                    }
                    this.addCharDataPool(ref chartDataPool, priceData, price);
                }

                foreach (KeyValuePair<string, double> chartData in chartDataPool) {
                    chart.Points.AddXY(chartData.Key, chartData.Value);
                }
            };

            drawing(true);      // 볼린저 upper_
            drawing(false);     // 볼린저 lower_
        }
    }

    class BasicStockChart : StockPriceChart
    {

        public BasicStockChart(int code) : base(code)
        {
        }

        public override void setChartControl(StockChartDlg dlg)
        {
            base.setChartControl(dlg);

            chartMacd_ = dlg.chart_Macd();
            chartMacd_.ChartAreas[0].AxisX.LabelStyle.Font = xlabelFont_;
            chartMacd_.Titles.Add("MACD 전용");
        }

        public void setBnfPercent(double percent)
        {
            StockData stockData = this.getStockData();
            if (stockData != null)
            {
                stockData.setBnfPercent(priceType_, percent);
                this.drawStock();
            }
        }

        public override void removeStockGraph()
        {
            base.removeStockGraph();
            chartMacd_.Series.Clear();
        }

        bool drawEma_ = false;
        bool drawBoll_ = false;
        bool drawBnf_ = false;
        bool drawLine_ = false;

        protected override void drawStockGraph()
        {
            base.drawStockGraph();

            if (drawEma_) this.drawExpAvg();
            if (drawBoll_) this.drawBollinger();
            if (drawBnf_) this.drawBnfLine();

            {
                minMacd_ = 0; maxMacd_ = 0;
                chartMacd_.Series.Clear();
                this.drawMacd();

                chartMacd_.ChartAreas[0].AxisY.Minimum = minMacd_;
                chartMacd_.ChartAreas[0].AxisY.Maximum = maxMacd_;
                chartMacd_.ChartAreas[0].AxisY.LabelStyle.Format = "#.#";
            }
        }

        public void setDrawLine(StockDrawLine type, bool draw)
        {
            switch (type)
            {
                case StockDrawLine.EMA:
                    drawEma_ = draw;
                    break;
                case StockDrawLine.BOLL:
                    drawBoll_ = draw;
                    break;
                case StockDrawLine.BNF:
                    drawBnf_ = draw;
                    break;
                case StockDrawLine.LINE:
                    drawLine_ = draw;
                    break;
            }
            this.drawStock();
        }

        //----------------------------------------------------------------------------//
        // MACD 차트 그리기
        private Chart chartMacd_;
        private double minMacd_ = 0, maxMacd_ = 0;

        enum MACD_TYPE
        {
            MACD,
            SIGNAL,
            OSCIL,
        }
        protected override void setMinMaxPrice(double value)
        {
            minMacd_ = Math.Min(minMacd_, value);
            maxMacd_ = Math.Max(maxMacd_, value);
        }

        delegate void drawingMACD(MACD_TYPE macdType);
        private void drawMacd()
        {
            List<CandleData> priceTable = stockData_.priceTable(priceType_);
            int xMax = Math.Min(limitXCount_, priceTable.Count - 1);

            maxMacd_ = minMacd_ = priceTable[xMax].calc_[(int)EVALUATION_DATA.MACD];

            drawingMACD drawing = (macdType) => {
                Series chart = chartMacd_.Series.Add(macdType.ToString());
                switch (macdType)
                {
                    case MACD_TYPE.MACD:
                    case MACD_TYPE.SIGNAL:
                        chart.ChartType = SeriesChartType.Line;
                        chart.BorderWidth = 1;
                        chart.BorderDashStyle = ChartDashStyle.Dash;
                        break;
                    default:
                        return;
                }

                Dictionary<string, double> chartDataPool = new Dictionary<string, double>();
                for (int dateIdx = xMax; dateIdx >= 0; --dateIdx)
                {
                    CandleData priceData = priceTable[dateIdx];
                    double price = 0.0f;
                    switch (macdType)
                    {
                        case MACD_TYPE.MACD: price = priceTable[dateIdx].calc_[(int)EVALUATION_DATA.MACD]; break;
                        case MACD_TYPE.SIGNAL: price = priceTable[dateIdx].calc_[(int)EVALUATION_DATA.MACD_SIGNAL]; break;
                        default:
                            return;
                    }
                    this.addCharDataPool(ref chartDataPool, priceData, price);
                }

                foreach (KeyValuePair<string, double> chartData in chartDataPool)
                {
                    chart.Points.AddXY(chartData.Key, chartData.Value);
                }
            };

            drawingMACD oscilDraw = (macdType) => {
                Series chart = chartMacd_.Series.Add(macdType.ToString());
                chart.ChartType = SeriesChartType.Stock;
                chart.BorderWidth = 2;

                Dictionary<string, double> chartDataPool = new Dictionary<string, double>();

                int index = 0;
                for (int dateIdx = xMax; dateIdx >= 0; --dateIdx)
                {
                    CandleData priceData = priceTable[dateIdx];
                    double price = priceTable[dateIdx].calc_[(int)EVALUATION_DATA.MACD_OSCIL];

                    this.addCharDataPool(ref chartDataPool, priceData, price);
                }

                index = 0;
                double oldValue = 0.0f;
                foreach (KeyValuePair<string, double> chartData in chartDataPool)
                {
                    int i = 0;
                    double[] value = { chartData.Value, 0, 0, chartData.Value };
                    chart.Points.AddXY(chartData.Key, value[i++], value[i++], value[i++], value[i++]);
                    if (value[0] <= 0)
                    {
                        chart.Points[index].Color = Color.Blue;
                    }
                    else
                    {
                        chart.Points[index].Color = Color.Red;
                    }
                    oldValue = chartData.Value;
                    index++;
                }
            };

            drawing(MACD_TYPE.MACD);
            drawing(MACD_TYPE.SIGNAL);
            oscilDraw(MACD_TYPE.OSCIL);
        }
    }
}
