using HappyTool.Dlg;
using HappyTool.Stock;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms.DataVisualization.Charting;
using StockLibrary;
using static StockLibrary.StrategyModuleList;
using System;

namespace HappyTool.DlgControl
{
    class StockStrategyModuleChart :StockPriceChart
    {
        public StockStrategyModuleChart(string code) : base(code)
        {

        }

        public override void setChartControl(StockChartDlg dlg)
        {
            chartStock_ = dlg.Chart_BackTest;
            chartStock_.ChartAreas[0].AxisX.LabelStyle.Font = xlabelFont_;
            chartStock_.Titles.Add("백테스팅");
            chartStock_.GetToolTipText += this.chartStockToolTipText;
        }

        delegate void drawingTradingPoint(bool isBuy);
        protected override void drawStockBaseChart()
        {
            List<CandleData> priceTable = stockData_.priceTable();

            //기본을 먼저 그림
            base.drawStockBaseChart();

            StockBot stockBot = ControlGet.getInstance.stockBot();

            BackTestRecoder history = stockData_.tradeModule();
            if (history == null) {
                return;
            }
            List<TradeRecord> list = history.getRecordData();
            if (list == null || list.Count == 0) {
                return;
            }

            drawingTradingPoint drawing = (isBuy) =>
            {
                Series chart;
                if (isBuy) {
                    chart = chartStock_.Series.Add("매수시점");
                    chart.Color = Color.Red;
                }
                else {
                    chart = chartStock_.Series.Add("매도시점");
                    chart.Color = Color.Blue;
                }
                chart.ChartType = SeriesChartType.Point;
                chart.BorderWidth = 3;

                Dictionary<DateTime, double> chartDataPool = new Dictionary<DateTime, double>();

                foreach (var priceData in priceTable) {
                    var date = priceData.date_;
                    double price = 0;

                    foreach (TradeRecord data in list) {
                        if (data.buyDate_ == date) {
                            if (isBuy) {
                                if (data.sellPrice_ == 0) { price = data.buyPrice_; }
                            }
                            else {
                                if (data.sellPrice_ != 0) { price = data.sellPrice_; }
                            }
                            break;
                        }
                    }

                    this.addCharDataPool(ref chartDataPool, priceData, price);
                }

                // 산거 표시를 위해 value 위와래로 10 pt 정도 점을 찍어주자
                foreach (KeyValuePair<DateTime, double> chartData in chartDataPool) {
                    chart.Points.AddXY(chartData.Key, chartData.Value);
                }

                foreach (TradeRecord data in list) {
                    Axis yAxis = null;
                    if (isBuy) {
                        yAxis = chartStock_.ChartAreas[0].AxisY;

                    }
                    else {
                        yAxis = chartStock_.ChartAreas[0].AxisY;
                    }
                    //this.drawStripeLine(yAxis, data.date_.ToOADate(), chart.Color);
                }
            };

            drawing(true);      // 매수시점 그리기
            drawing(false);     // 매도시점 그리기

        }
    }
}
