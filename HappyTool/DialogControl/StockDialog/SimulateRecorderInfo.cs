using HappyTool.Dlg;
using HappyTool.Stock;
using StockLibrary;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using UtilLibrary;
using static StockLibrary.StrategyModuleList;

namespace HappyTool.DlgControl
{
    class SimulateRecoderInfo :SingleTon<SimulateRecoderInfo>
    {
        QuickChart profitChart_;            // 이익율 차트
        RichTextBox infoWindow_;

        SimulateHistoryView recoderView_;

        SimulateRecoderInfo()
        {
            StockDlg stockDlg = Program.happyTool_.stockDlg_;
            profitChart_ = stockDlg.quickChart_SimulateProfit;
            infoWindow_ = stockDlg.richTextBox_SimulateResult;

            recoderView_ = new SimulateHistoryView();
        }

        public void setup()
        {
            infoWindow_.Text = "시뮬레이팅 결과 출력창";
        }

        void print(string text)
        {
            infoWindow_.Text += "\n===============================================\n";
            infoWindow_.Text += text;
            infoWindow_.SelectionStart = infoWindow_.Text.Length;
            infoWindow_.ScrollToCaret();
        }

        public void printToLogWindow(string text)
        {
            if (infoWindow_.InvokeRequired) {
                infoWindow_.BeginInvoke(new Action(() => this.print(text)));
            } else {
                this.print(text);
            }
        }

        void drawTradeRecoderChart(SimulateBackTestRecoder recoder)
        {
            // 차트 그리기
            StockBot bot = ControlGet.getInstance.stockBot();
            int priceIdx = (int) bot.priceType_;
            if (recoder == null) {
                return;
            }
            List<TradeRecord> profitRecoderList = recoder.getRecordData();
            if (profitRecoderList.Count == 0) {
                return;
            }

            profitChart_.Series.Clear();
            Series chart = profitChart_.Series.Add("자산 추세");

            chart.ChartType = SeriesChartType.Line;
            chart.BorderWidth = 1;

            int BUY_COUNT = PublicVar.simulateBuyCount;
            int INIT_MONEY = PublicVar.simulateInitMoney;
            int money = INIT_MONEY;

            SortedDictionary<DateTime, double> chartDataPool = new SortedDictionary<DateTime, double>();
            double tradeMoney = money;
            double minMoney = money;
            double maxMoney = 0;
            foreach (TradeRecord data in profitRecoderList) {
                double profit = data.totalProfit();
                tradeMoney = tradeMoney + profit;
                chartDataPool[data.buyDate_] = tradeMoney;

                minMoney = Math.Min(tradeMoney, minMoney);
                maxMoney = Math.Max(tradeMoney, maxMoney);
            }

            //           profitChart_.ChartAreas[0].AxisY.Minimum = (double) minMoney + ((double) minMoney * 0.2f);
            //           profitChart_.ChartAreas[0].AxisY.Maximum = (double) maxMoney + ((double) maxMoney * 0.2f);
            profitChart_.ChartAreas[0].AxisY.LabelStyle.Format = "{0:#,##0}";

            foreach (KeyValuePair<DateTime, double> chartData in chartDataPool) {
                chart.Points.AddXY(chartData.Key, chartData.Value);
            }
        }

        void resetChart()
        {
            profitChart_.Series.Clear();
        }

        void resetDrawTradeHistoryChart()
        {
            if (profitChart_.InvokeRequired) {
                profitChart_.BeginInvoke(new Action(() => this.resetChart()));
            } else {
                this.resetChart();
            }
        }

        void printChart(SimulateBackTestRecoder recoder)
        {
            if (profitChart_.InvokeRequired) {
                profitChart_.BeginInvoke(new Action(() => this.drawTradeRecoderChart(recoder)));
            } else {
                this.drawTradeRecoderChart(recoder);
            }
        }

        public void updateChart(SimulateBackTestRecoder recoder)
        {
            this.resetDrawTradeHistoryChart();
            this.printChart(recoder);

            recoderView_.resetDataGridView();
            recoderView_.setBackTestHistory(recoder);
            recoderView_.print();
        }

        internal void cellClick(object sender, DataGridViewCellEventArgs e)
        {
            recoderView_.cellClick(sender, e);
        }
    }
}
