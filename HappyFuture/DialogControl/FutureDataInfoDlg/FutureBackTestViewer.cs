using HappyFuture.Dialog;
using StockLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using UtilLibrary;

namespace HappyFuture.DialogControl
{
    class FutureBackTestViewer: QuickDataViewCtl
    {
        readonly FutureDataInfoDlg dlg_ = null;
        readonly string code_ = null;

        public FutureBackTestViewer(FutureDataInfoDlg dlg, string code)
        {
            this.dlg_ = dlg;
            this.code_ = code;
            this.setup(this.dlg_.DataGridView_BackTest);
        }

        ListView listView_ = null;
        QuickChart chart_ = null;
        ControlPrint moduleName_;

        enum CHART_AREA
        {
            손익금,
            매매시점,
        }

        public override void setup(QuickDataGridView dataView)
        {
            base.setup(dataView);

            this.chart_ = this.dlg_.Chart_BackTest;
            foreach (CHART_AREA area in Enum.GetValues(typeof(CHART_AREA))) {
                this.chart_.ChartAreas.Add(area.ToString());
            }

            this.listView_ = this.dlg_.listView_Recoder;
            this.setupList();
            this.updateBackTestList();

            this.moduleName_ = new ControlPrint(this.dlg_.label_moduleName);
        }

        //----------------------------------------------------------------------------------//
        // 백테스팅 결과들 리스트
        void setupList()
        {
            this.listView_.BeginUpdate();
            this.listView_.View = View.Details;
            this.listView_.CheckBoxes = false;

            this.listView_.Columns.Add("백테스트 리스트");
            for (int i = 0; i < this.listView_.Columns.Count; ++i) {
                this.listView_.Columns[i].Width = -2;
            }

            this.listView_.EndUpdate();
        }

        FutureBackTestRecoder viewRecode_;

        public List<BackTestRecoder> recoderList_ = null;
        void updateBackTestList()
        {
            this.listView_.Items.Clear();
            var bot = ControlGet.getInstance.futureBot();
            FutureData futureData = bot.getStockDataCode(this.code_) as FutureData;
            if (futureData.tradeModulesCount() == 0) {
                return;
            }
            this.recoderList_ = new List<BackTestRecoder>(futureData.tradeModuleList_);

            int idx = 0;
            foreach (var recode in this.recoderList_) {
                FutureBackTestRecoder fRecode = recode as FutureBackTestRecoder;
                ListViewItem lvi = new ListViewItem(string.Format("결과 {0}", idx++));
                {
                    this.listView_.BeginUpdate();

                    this.listView_.Items.Add(lvi);
                    this.listView_.EnsureVisible(this.listView_.Items.Count - 1);

                    for (int i = 0; i < this.listView_.Columns.Count; ++i) {
                        this.listView_.Columns[i].Width = -2;
                    }
                    this.listView_.EndUpdate();
                }
            }
        }

        internal void selectRecodeIndex(EventArgs e)
        {
            var getSelected = this.listView_.FocusedItem.Index;
            this.setRecode(getSelected);
            this.dataTable_ = this.makeNewTable();
            this.print();
            this.printModuleName();
        }
        //----------------------------------------------------------------------------------//
        //백테스팅 모듈 이름 출력
        void printModuleName()
        {
            string text = string.Format("매매 전략 없음");
            if (this.viewRecode_ != null) {
                text = string.Format("매매 전략 : buy[{0}]\n     sell[{1}]", this.viewRecode_.buyTradeModule_.getName(), this.viewRecode_.sellTradeModule_.getName());
            }
            this.moduleName_.print(text);
        }

        //----------------------------------------------------------------------------------//
        // 백테스팅 결과표 및 차트
        internal enum COLUMNS_NAME
        {
            매수시간, 매수가격, 포지션, 매도시간, 매도가격, 최소익, 최대익, 순손익, 손익율, 보유시간
        };

        protected override void decorationView()
        {
            this.dataTable_.DefaultView.Sort = string.Format("{0} DESC", COLUMNS_NAME.매도시간);
            this.dataGridView_.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);

            int maxRow = Math.Max(this.dataGridView_.RowCount - 1, 0);
            for (int rowIdx = 0; rowIdx < maxRow; ++rowIdx) {
                double rate = 0.0f;
                if (double.TryParse(this.dataGridView_.Rows[rowIdx].Cells[(int) COLUMNS_NAME.순손익].Value.ToString(), out rate) == false) {
                    continue;
                }
                if (rate > 0) {
                    this.dataGridView_.Rows[rowIdx].DefaultCellStyle.BackColor = Color.LightPink;
                }
                else {
                    this.dataGridView_.Rows[rowIdx].DefaultCellStyle.BackColor = Color.SkyBlue;
                }
            }
        }

        public override DataTable makeNewTable()
        {
            DataTable dt = new DataTable();
            // header
            foreach (COLUMNS_NAME name in Enum.GetValues(typeof(COLUMNS_NAME))) {
                if (name == COLUMNS_NAME.포지션) {
                    dt.Columns.Add(name.ToString(), typeof(string));
                }
                else if (name == COLUMNS_NAME.매수시간
                    || name == COLUMNS_NAME.매도시간) {
                    dt.Columns.Add(name.ToString(), typeof(DateTime));
                }
                else {
                    dt.Columns.Add(name.ToString(), typeof(double));
                }
            }

            if (this.viewRecode_ == null) {
                return dt;
            }
            var bot = ControlGet.getInstance.futureBot();
            FutureData futureData = bot.getStockDataCode(this.code_) as FutureData;
            foreach (var trade in this.viewRecode_.getRecordData()) {
                var fTread = trade as FutureTradeRecord;
                var buyDate = fTread.buyDate_;
                var buyPrice = fTread.buyPrice_;
                var buyCount = fTread.buyCount_;
                var position = fTread.position_;
                var sellDate = fTread.sellDate_;
                var sellPrice = fTread.sellPrice_;
                var minProfit_ = fTread.minProfit_;
                var maxProfit_ = fTread.maxProfit_;
                var profit = fTread.profit(futureData);
                var profitRate = fTread.profitRate();
                var haveTime = fTread.haveTime();
                dt.Rows.Add(buyDate, buyPrice, position, sellDate, sellPrice, minProfit_, maxProfit_, profit, profitRate, haveTime);
            }

            return dt;
        }

        enum SERIES_NAME
        {
            손익금,
            매수시점,
            매도시점
        }

        void setChartSeries()
        {
            if (this.dataTable_.Rows.Count == 0) {
                return;
            }
            this.chart_.DataSource = this.dataTable_;
            var chartProfit = this.chart_.Series.Add(SERIES_NAME.손익금.ToString());
            chartProfit.ChartType = SeriesChartType.Column;
            chartProfit.XValueMember = COLUMNS_NAME.매도시간.ToString();
            chartProfit.YValueMembers = COLUMNS_NAME.순손익.ToString();
            chartProfit.ChartArea = CHART_AREA.손익금.ToString();
            chartProfit.XValueType = ChartValueType.DateTime;

            var chartBuyDate = this.chart_.Series.Add(SERIES_NAME.매수시점.ToString());
            chartBuyDate.ChartType = SeriesChartType.Point;
            chartBuyDate.XValueMember = COLUMNS_NAME.매수시간.ToString();
            chartBuyDate.YValueMembers = COLUMNS_NAME.매수가격.ToString();
            chartBuyDate.ChartArea = CHART_AREA.매매시점.ToString();
            chartBuyDate.XValueType = ChartValueType.DateTime;

            var chartSellDate = this.chart_.Series.Add(SERIES_NAME.매도시점.ToString());
            chartSellDate.ChartType = SeriesChartType.Point;
            chartSellDate.XValueMember = COLUMNS_NAME.매도시간.ToString();
            chartSellDate.YValueMembers = COLUMNS_NAME.매도가격.ToString();
            chartSellDate.ChartArea = CHART_AREA.매매시점.ToString();
            chartSellDate.XValueType = ChartValueType.DateTime;
            this.chart_.DataBind();

            DataManipulator myDataManip = this.chart_.DataManipulator;
            // Remove weekends.
            foreach (SERIES_NAME seriesName in Enum.GetValues(typeof(SERIES_NAME))) {
                myDataManip.Filter(DateRangeType.DayOfWeek, "0,6", seriesName.ToString());
            }
            this.chart_.Invalidate();
        }

        void setChartAreas()
        {
            double minLevel = double.MaxValue;
            double maxLevel = double.MinValue;
            foreach (DataRow dr in this.dataTable_.Rows) {
                double min = dr.Field<double>(COLUMNS_NAME.매수가격.ToString());
                minLevel = Math.Min(minLevel, min);
                min = dr.Field<double>(COLUMNS_NAME.매도가격.ToString());
                minLevel = Math.Min(minLevel, min);

                double max = dr.Field<double>(COLUMNS_NAME.매수가격.ToString());
                maxLevel = Math.Max(maxLevel, max);
                max = dr.Field<double>(COLUMNS_NAME.매도가격.ToString());
                maxLevel = Math.Max(maxLevel, max);
            }

            DateTimeIntervalType type = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Auto;
            type = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Minutes;

            foreach (CHART_AREA area in Enum.GetValues(typeof(CHART_AREA))) {
                this.chart_.ChartAreas[area.ToString()].AxisX.LabelStyle.IntervalType = type;
                this.chart_.ChartAreas[area.ToString()].AxisX.LabelStyle.IntervalOffsetType = type;
                this.chart_.ChartAreas[area.ToString()].AxisX.LabelStyle.Format = "yy/MM/dd\nHH:mm";
            }

            this.chart_.ChartAreas[CHART_AREA.손익금.ToString()].AxisY.LabelStyle.Format = "{0:#,##0.#####} $";
            this.chart_.ChartAreas[CHART_AREA.매매시점.ToString()].AxisY.LabelStyle.Format = "{0:#,##0.#####} $";
            this.chart_.ChartAreas[CHART_AREA.매매시점.ToString()].AxisY.Maximum = maxLevel;
            this.chart_.ChartAreas[CHART_AREA.매매시점.ToString()].AxisY.Minimum = minLevel;
        }

        void setRecode(int index = -1)
        {
            var bot = ControlGet.getInstance.futureBot();
            FutureData futureData = bot.getStockDataCode(this.code_) as FutureData;

            if (index == -1) {
                this.viewRecode_ = futureData.tradeModule() as FutureBackTestRecoder;
            }
            else {
                if (this.recoderList_.Count < index) {
                    this.viewRecode_ = futureData.tradeModule() as FutureBackTestRecoder;
                }
                else {
                    this.viewRecode_ = this.recoderList_[index] as FutureBackTestRecoder;
                }
            }
        }

        public override void print(DataTable dt)
        {
            // 내용
            base.print(dt);

            this.chart_.Series.Clear();
            this.setChartSeries();
            this.setChartAreas();
        }

        public override void update()
        {
            this.setRecode();
            this.dataTable_ = this.makeNewTable();
            this.print();
            this.printModuleName();
        }
    }
}
