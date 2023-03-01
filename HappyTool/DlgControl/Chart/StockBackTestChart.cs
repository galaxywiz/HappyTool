using HappyTool.Dlg;
using HappyTool.Stock;
using HappyTool.Stock.TradeModules;
using HappyTool.Stock.Calculate;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms.DataVisualization.Charting;

namespace HappyTool.DlgControl {
    class StockTradeModuleChart :StockPriceChart {
        public StockTradeModuleChart(int code) : base(code)
        {

        }

        public override void setChartControl(StockChartDlg dlg)
        {
            chartStock_ = dlg.chart_TradeModule();
            chartStock_.ChartAreas[0].AxisX.LabelStyle.Font = xlabelFont_;
            chartStock_.Titles.Add("백테스팅");
            chartStock_.GetToolTipText += this.chartStockToolTipText;
            trackBar_ = dlg.trackBarStock();
        }

        delegate void drawingTradingPoint(bool isBuy);
        protected override void drawStockGraph()
        {
            List<CandleData> priceTable = stockData_.priceTable(priceType_);
            int xMax = System.Math.Min(limitXCount_, priceTable.Count - 1);
            limitXCount_ = xMax;

            //기본을 먼저 그림
            base.drawStockGraph();

            TradeModule test = StockBot.getInstance.tradeModule();
            test.setTradeModule(stockCode_, priceType_);
            if (test != null) {
                if (test.useBnf_) {
                    base.drawBnfLine();
                }
                if (test.useBollinger_) {
                    base.drawBollinger();
                }
                if (test.useEma_) {
                    base.drawExpAvg();
                }
            }
            test = null;

            BackTestHistory history = stockData_.backTestHistory(priceType_);
            List<BackTestHistoryData> list = history.getHistoryData();
            if (list.Count == 0) {
                return;
            }

            drawingTradingPoint drawing = (isBuy) => {
                Series chart;
                if (isBuy) {
                    chart = chartStock_.Series.Add("매수시점");
                    chart.Color = Color.Red;
                } else {
                    chart = chartStock_.Series.Add("매도시점");
                    chart.Color = Color.Blue;
                }
                chart.ChartType = SeriesChartType.Point;
                chart.BorderWidth = 3;
                
                Dictionary<string, double> chartDataPool = new Dictionary<string, double>();

                for (int dateIdx = xMax; dateIdx >= 0; --dateIdx) {
                    CandleData priceData = priceTable[dateIdx];
                    var date = priceData.date_;
                    BackTestHistoryData write = null;
                    foreach (BackTestHistoryData data in list) {
                        if (data.date_ == date) {
                            if (isBuy) {
                                if (data.isBuy_) { write = data; }
                            } else {
                                if (data.isBuy_ == false) { write = data; }
                            }
                            break;
                        }
                    }
                    int price = 0;
                    if (write != null) {
                        price = write.price_;
                    }

                    this.addCharDataPool(ref chartDataPool, priceData, price);
                }

                // 산거 표시를 위해 value 위와래로 10 pt 정도 점을 찍어주자
                foreach (KeyValuePair<string, double> chartData in chartDataPool) {
                    chart.Points.AddXY(chartData.Key, chartData.Value);
                }

                foreach (BackTestHistoryData data in list) {
                    Axis yAxis = null;
                    if (isBuy) {
                        yAxis = chartStock_.ChartAreas[0].AxisY;

                    } else {
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
