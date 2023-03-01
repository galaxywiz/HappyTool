using HappyFuture.Dialog;
using StockLibrary;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using UtilLibrary;

namespace HappyFuture.DialogControl
{
    class FuturePriceChart
    {
        protected QuickChart stockChart_;
        Label lableInfo_;
        protected string stockCode_ = "";

        protected StockData getStockData()
        {
            return ControlGet.getInstance.futureBot().getStockDataCode(this.stockCode_);
        }

        readonly PRICE_TYPE botPriceType_;
        public FuturePriceChart(string code)
        {
            this.stockCode_ = code;
            this.botPriceType_ = ControlGet.getInstance.futureBot().priceType_;
        }

        ComboBox priceComboBox_;

        public virtual void setup(FutureDataInfoDlg dlg)
        {
            lableInfo_ = dlg.label_chartInfo;
            this.stockChart_ = dlg.Chart_Future;

            foreach (CHART_AREA area in Enum.GetValues(typeof(CHART_AREA))) {
                this.stockChart_.ChartAreas.Add(area.ToString());
            }
            this.stockChart_.GetToolTipText += this.chartStockToolTipText;

            this.priceComboBox_ = dlg.comboBox_priceType;
            this.setupCombo();
        }

        void setupCombo()
        {
            string[] data = { "기본 분봉", "1분봉", "15 분봉", "60 분봉" };
            // 각 콤보박스에 데이타를 초기화
            this.priceComboBox_.Items.AddRange(data);

            // 처음 선택값 지정. 첫째 아이템 선택
            this.priceComboBox_.SelectedIndex = 0;
        }

        void drawChart(int index)
        {
            try {
                var bot = ControlGet.getInstance.futureBot();

                StockData stockData = null;
                switch (index) {
                    case 0:
                    stockData = this.getStockData();
                    break;
                    case 1:
                    stockData = bot.getRefStockDataCode(this.stockCode_, REF_PRICE_TYPE.기준_분봉);
                    break;
                    case 2:
                    stockData = bot.getRefStockDataCode(this.stockCode_, REF_PRICE_TYPE.중간_분봉);
                    break;
                    case 3:
                    stockData = bot.getRefStockDataCode(this.stockCode_, REF_PRICE_TYPE.시간_분봉);
                    break;
                }

                if (stockData == null) {
                    return;
                }
                if (stockData.priceTable() == null) {
                    return;
                }

                this.stockData_ = (StockData) stockData.Clone();
                this.removeStockGraph();
                this.setChartSeries();
                this.setChartArea();

                this.drawPayTimeLine();
                this.writePayTimeInfo();
            }
            catch (Exception e) {
                Logger.getInstance.print(KiwoomCode.Log.에러, "그래프 출력 에러, {0}/{1}", e.Message, e.StackTrace);
            }
        }
               
        const int PRINT_CANDLE_COUNT = 50;
        const string CHART_TIME_FMT = "yy/MM/dd\nHH:mm";

        bool getViewMaxMinValue(DateTime start, DateTime end, out double max, out double min)
        {
            max = double.MinValue;
            min = double.MaxValue;

            var table = stockData_.priceTable();
            FutureData futureData = stockData_ as FutureData;
            foreach (var candle in table) {
                if (start <= candle.date_ && candle.date_ <= end) {
                    if (candle.highPrice_ > max) {
                        max = candle.highPrice_;
                     //   max = Math.Max(max, candle.calc_[(int) EVALUATION_DATA.EMA_50]);
                        max = max + (futureData.tickSize_ * 10);
                    }
                    if (candle.lowPrice_ < min) {
                        min = candle.lowPrice_;
                      //  min = Math.Min(min, candle.calc_[(int) EVALUATION_DATA.EMA_50]);
                        min = min - (futureData.tickSize_ * 10);
                    }
                }
            }
            if (max < min) {
                return false;
            }
            return true;
        }

        internal void changeAxisView(object sender, ViewEventArgs e)
        {
            try {
                if (sender.Equals(this.stockChart_) == false) {
                    return;
                }
                DateTime start = DateTime.FromOADate(e.Axis.ScaleView.ViewMinimum);
                DateTime end = DateTime.FromOADate(e.Axis.ScaleView.ViewMaximum);

                double min, max;
                if (this.getViewMaxMinValue(start, end, out max, out min)) {
                    var chartArea = this.stockChart_.ChartAreas[CHART_AREA.가격차트.ToString()];
                    chartArea.AxisY.Maximum = max;
                    chartArea.AxisY.Minimum = min;
                }
            } catch (Exception ex) {
                Logger.getInstance.print(KiwoomCode.Log.에러, "그래프 줌 출력 에러, {0}/{1}", ex.Message, ex.StackTrace);
            }
        }

        internal void selectComboPriceType(EventArgs e)
        {
            this.print(this.priceComboBox_.SelectedIndex);
        }

        protected void chartStockToolTipText(object sender, ToolTipEventArgs e)
        {
            // Check selected chart element and set tooltip text for it
            switch (e.HitTestResult.ChartElementType) {
                case ChartElementType.DataPoint:
                var dataPoint = e.HitTestResult.Series.Points[e.HitTestResult.PointIndex];
                DateTime date = DateTime.FromOADate(dataPoint.XValue);
                var text = this.stockData_.logCandleInfo(date);
                e.Text = text;
                break;
            }
        }

        protected StockData stockData_;
        enum CHART_AREA
        {
            가격차트,
        }

        public virtual void removeStockGraph()
        {
            this.stockChart_.Series.Clear();
           
            var chartArea = this.stockChart_.ChartAreas[CHART_AREA.가격차트.ToString()];
            var axis = chartArea.AxisX;
            axis.StripLines.Clear();

            axis = chartArea.AxisY;
            axis.StripLines.Clear();
        }

        DataTable dataTable_;

        enum SERIES_NAME
        {
            가격표,
            // 여기는 StockData의 PRICE_DATA_TABLE_COLUMN 과 이름을 같이 할것
            //볼린저_상단,
            //볼린저_중앙,
            //볼린저_하단,
            //EMA_3,
            //EMA_20,
            //EMA_50,
            //PRICE_CHANNEL_UPPER,
            //PRICE_CHANNEL_LOWER,
        }

        void drawAverageLine(Axis axis, double avg, Color color, string text)
        {
            StripLine stripLine = new StripLine();
            stripLine.Interval = 0;

            stripLine.BorderDashStyle = ChartDashStyle.DashDot;
            stripLine.BorderColor = color;
            stripLine.BorderWidth = 1;
            
            stripLine.IntervalOffset = avg;
            stripLine.BackColor = Color.Transparent;
            stripLine.ToolTip = text;
            stripLine.Text = text;

            axis.StripLines.Add(stripLine);
        }
        
        void drawPayTimeLine()
        {
            var chartArea = this.stockChart_.ChartAreas[CHART_AREA.가격차트.ToString()];

            var time = this.stockData_.nowDateTime();
            var price = this.stockData_.nowPrice();
            Color color = Color.DarkBlue;
            
            this.drawAverageLine(chartArea.AxisX, time.ToOADate(), color, time.ToString(CHART_TIME_FMT));
            this.drawAverageLine(chartArea.AxisY, price, color, price.ToString("#,###0.######") + " $");

            var buyTime = this.stockData_.lastChejanDate_;
            if (buyTime == DateTime.MinValue) {
                return;
            }

            time = this.stockData_.lastChejanDate_;
            price = this.stockData_.buyPrice_;
            color = Color.DarkRed;
            this.drawAverageLine(chartArea.AxisX, time.ToOADate(), color, time.ToString(CHART_TIME_FMT));
            this.drawAverageLine(chartArea.AxisY, price, color, price.ToString("#,###0.######") + " $");
        }

        public void writePayTimeInfo()
        {
            var time = this.stockData_.nowDateTime();
            var log = string.Format("*** 현재 정보 ***\n");
            log += stockData_.logCandleInfo(time);
            var buyTime = this.stockData_.lastChejanDate_;
            try {
                if (buyTime == DateTime.MinValue) {
                    return;
                }
                log += string.Format("\n*** 구입시 정보 ***\n");
                log += stockData_.logCandleInfo(stockData_.lastChejanDate_);
                log += string.Format("\n*** 현재 이익 정보 ***\n");
                log += stockData_.logProfitInfo();
            }
            finally {
                if (lableInfo_.InvokeRequired) {
                    lableInfo_.BeginInvoke(new Action(() => lableInfo_.Text = log));
                }
                else {
                    lableInfo_.Text = log;
                }
            }
        }

        void drawTechLine(Series priceChart, string lineName, Color color)
        {
            priceChart = this.stockChart_.Series.Add(lineName);
            priceChart.XValueMember = StockData.PRICE_DATA_TABLE_COLUMN.시간.ToString();

            priceChart.ChartArea = CHART_AREA.가격차트.ToString();
            priceChart.XValueType = ChartValueType.DateTime;

            priceChart.ChartType = SeriesChartType.Line;
            priceChart.Color = color;            
            priceChart.YValueMembers = lineName;
        }

        void setChartSeries()
        {
            this.dataTable_ = this.stockData_.getChartPriceTable();
            if (this.dataTable_.Rows.Count == 0) {
                return;
            }
            try {
                Series priceChart;
                this.stockChart_.DataSource = this.dataTable_;
                {
                    priceChart = this.stockChart_.Series.Add(SERIES_NAME.가격표.ToString());
                    priceChart.XValueMember = StockData.PRICE_DATA_TABLE_COLUMN.시간.ToString();

                    priceChart.ChartArea = CHART_AREA.가격차트.ToString();
                    priceChart.XValueType = ChartValueType.DateTime;

                    priceChart.ChartType = SeriesChartType.Candlestick;
                    string yValue = string.Format("{0},{1},{2},{3}", StockData.PRICE_DATA_TABLE_COLUMN.저가, StockData.PRICE_DATA_TABLE_COLUMN.고가, StockData.PRICE_DATA_TABLE_COLUMN.시가, StockData.PRICE_DATA_TABLE_COLUMN.종가);
                    priceChart.YValueMembers = yValue;
                    priceChart["PriceUpColor"] = "IndianRed";
                    priceChart["PriceDownColor"] = "RoyalBlue";             
                }

              //  this.drawTechLine(priceChart, SERIES_NAME.볼린저_상단.ToString(), Color.Crimson);
              ////  this.drawTechLine(priceChart, SERIES_NAME.볼린저_중앙.ToString(), Color.Green);
              //  this.drawTechLine(priceChart, SERIES_NAME.볼린저_하단.ToString(), Color.Indigo);
              //  this.drawTechLine(priceChart, SERIES_NAME.EMA_3.ToString(), Color.OliveDrab);
              //  this.drawTechLine(priceChart, SERIES_NAME.EMA_20.ToString(), Color.Red);
              //  this.drawTechLine(priceChart, SERIES_NAME.PRICE_CHANNEL_UPPER.ToString(), Color.Red);
              //  this.drawTechLine(priceChart, SERIES_NAME.PRICE_CHANNEL_LOWER.ToString(), Color.Blue);

                this.stockChart_.DataBind();
                DataManipulator myDataManip = this.stockChart_.DataManipulator;
                // Remove weekends.
                foreach (SERIES_NAME seriesName in Enum.GetValues(typeof(SERIES_NAME))) {
                    myDataManip.Filter(DateRangeType.DayOfWeek, "0", seriesName.ToString());
                }
                this.stockChart_.Invalidate();
            } catch (Exception e) {
                Logger.getInstance.print(KiwoomCode.Log.에러, "{0} / {1}", e.Message, e.StackTrace);
            }
        }

        void setChartArea()
        {
            if (this.dataTable_.Rows.Count == 0) {
                return;
            }

            DateTimeIntervalType type = DateTimeIntervalType.Auto;
            type = DateTimeIntervalType.Minutes;

            foreach (CHART_AREA area in Enum.GetValues(typeof(CHART_AREA))) {
                var chartAreas = this.stockChart_.ChartAreas[area.ToString()];
                chartAreas.AxisX.LabelStyle.IntervalType = type;
                chartAreas.AxisX.LabelStyle.IntervalOffsetType = type;
                chartAreas.AxisX.LabelStyle.Format = CHART_TIME_FMT;
                chartAreas.AxisX.LabelAutoFitStyle = LabelAutoFitStyles.WordWrap;
                chartAreas.AxisX.MajorGrid.Enabled = false;  
            }
            var chartArea = this.stockChart_.ChartAreas[CHART_AREA.가격차트.ToString()];
            var nowPrice = stockData_.nowPrice();
            if (nowPrice < 10) {
                chartArea.AxisY.LabelStyle.Format = "{0:#0.#####0} $";
            } else {
                chartArea.AxisY.LabelStyle.Format = "{0:#,###0.##0} $";
            }
            chartArea.AxisY.MajorGrid.Enabled = false;

            DateTime start = (DateTime) dataTable_.Rows[PRINT_CANDLE_COUNT].ItemArray[0];
            DateTime end = (DateTime) dataTable_.Rows[0].ItemArray[0];
            double min, max;
            if (this.getViewMaxMinValue(start, end, out max, out min)) {
                chartArea.AxisY.Maximum = max;
                chartArea.AxisY.Minimum = min;
            }            

            if (PRINT_CANDLE_COUNT < dataTable_.Rows.Count) {
                var startTime = (DateTime) dataTable_.Rows[PRINT_CANDLE_COUNT].ItemArray[0];
                var endTime = (DateTime) dataTable_.Rows[0].ItemArray[0];
                chartArea.AxisX.ScaleView.Zoom(startTime.ToOADate(), endTime.ToOADate());
            }
            chartArea.BackColor = Color.LightGoldenrodYellow;
        }

        public void print(int index = 0)
        {
            if (this.stockChart_.InvokeRequired) {
                this.stockChart_.BeginInvoke(new Action(() => this.drawChart(index)));
            }
            else {
                this.drawChart(index);
            }
        }
    }
}
