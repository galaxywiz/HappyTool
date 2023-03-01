using HappyTool.Dlg;
using HappyTool.Stock;
using HappyTool.Stock.TradeModules;
using HappyTool.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace HappyTool.DlgControl
{
    class BackTestInfo :SingleTon<BackTestInfo>
    {
        QuickChart profitChart_;            // 이익율 차트
        QuickChart winRateChart_;           // 승률 차트
        RichTextBox infoWindow_;
        ComboBox backTestCombo_;

        BackTestInfo()
        {
            StockDlg stockDlg = Program.happyTool_.stockDlg_;
            profitChart_ = stockDlg.quickChart_profit;
            winRateChart_ = stockDlg.quickChart_winRate;

            infoWindow_ = stockDlg.richTextBox_BackTestResult;
            backTestCombo_ = stockDlg.comboBox_BackTest;
        }

        public void setup()
        {
            infoWindow_.Text = "백테스팅 결과 출력창";

            foreach (TRADE_MODULE_TYPE type in Enum.GetValues(typeof(TRADE_MODULE_TYPE))) {
                backTestCombo_.Items.Add(type.ToString());
            }
            backTestCombo_.SelectedIndex = 0;
        }

        public void changeTradeModule(int index)
        {
            StockBot bot = StockBot.getInstance;
            TRADE_MODULE_TYPE type = (TRADE_MODULE_TYPE) index;
            switch (type) {
                case TRADE_MODULE_TYPE.볼린저_CCI:
                bot.setTradeModule(new BollengerCCITradeModule());
                break;
                case TRADE_MODULE_TYPE.볼린저_HECT:
                break;
                case TRADE_MODULE_TYPE.기술_평가:
                bot.setTradeModule(new EvaluationTradeModule());
                break;
            }
        }

        void print(string text)
        {
            infoWindow_.Text += "\n==========================\n";
            infoWindow_.Text += text;
            infoWindow_.Select(infoWindow_.Text.Length, 0);
        }

        void printToLogWindow(string text)
        {
            if (infoWindow_.InvokeRequired) {
                infoWindow_.BeginInvoke(new Action(() => this.print(text)));
            } else {
                this.print(text);
            }
        }

        void drawHistory()
        {
            // 차트 그리기
            StockBot bot = StockBot.getInstance;
            int priceIdx = (int) bot.priceType_;
            BackTestHistory history = bot.backTestHistory_[priceIdx];
            List<BackTestProfitHistory> profitHistoryList = history.profitHistoryList_;
            if (profitHistoryList.Count == 0) {
                return;
            }
            profitChart_.Titles.Add("백테스팅 이익율 추이");

            Series chart = profitChart_.Series.Add("이익율");

            chart.ChartType = SeriesChartType.Line;
            chart.BorderWidth = 1;

            foreach(BackTestProfitHistory profitHistory in profitHistoryList) {
                chart.Points.AddXY(profitHistory.date_, profitHistory.money_);
            }          
        }

        public void update(string text)
        {
            this.printToLogWindow(text);
            this.drawHistory();
        }
    }
}
