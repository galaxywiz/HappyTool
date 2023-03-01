using HappyTool.Util;

namespace HappyTool.Dlg
{
    partial class StockChartDlg
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend2 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea3 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend3 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series3 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.label_bnfPercent = new System.Windows.Forms.Label();
            this.trackBar_bnf = new System.Windows.Forms.TrackBar();
            this.graph_line_check = new System.Windows.Forms.CheckBox();
            this.graph_bnf_check = new System.Windows.Forms.CheckBox();
            this.graph_boll_check = new System.Windows.Forms.CheckBox();
            this.graph_ema_check = new System.Windows.Forms.CheckBox();
            this.TrackBar_stock = new System.Windows.Forms.TrackBar();
            this.Chart_MACD = new QuickChart();
            this.Chart_Stock = new QuickChart();
            this.RadioButton_stock_15min = new System.Windows.Forms.RadioButton();
            this.RadioButton_stock_1hour = new System.Windows.Forms.RadioButton();
            this.RadioButton_stock_4hour = new System.Windows.Forms.RadioButton();
            this.RadioButton_stock_week = new System.Windows.Forms.RadioButton();
            this.RadioButton_stock_day = new System.Windows.Forms.RadioButton();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.WebBrowser_Stock = new System.Windows.Forms.WebBrowser();
            this.tabControl_stock = new System.Windows.Forms.TabControl();
            this.tabPage_chart = new System.Windows.Forms.TabPage();
            this.tabPage_TradeModule = new System.Windows.Forms.TabPage();
            this.DataGridView_TradeModule = new QuickDataGridView();
            this.Chart_BackTest = new QuickChart();
            this.tabPage_eval = new System.Windows.Forms.TabPage();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.DataGridView_Evaluation = new QuickDataGridView();
            this.DataGridView_StockInfo = new QuickDataGridView();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar_bnf)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TrackBar_stock)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Chart_MACD)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Chart_Stock)).BeginInit();
            this.groupBox5.SuspendLayout();
            this.tabControl_stock.SuspendLayout();
            this.tabPage_chart.SuspendLayout();
            this.tabPage_TradeModule.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DataGridView_TradeModule)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Chart_BackTest)).BeginInit();
            this.tabPage_eval.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DataGridView_Evaluation)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.DataGridView_StockInfo)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.label_bnfPercent);
            this.groupBox4.Controls.Add(this.trackBar_bnf);
            this.groupBox4.Controls.Add(this.graph_line_check);
            this.groupBox4.Controls.Add(this.graph_bnf_check);
            this.groupBox4.Controls.Add(this.graph_boll_check);
            this.groupBox4.Controls.Add(this.graph_ema_check);
            this.groupBox4.Controls.Add(this.Chart_MACD);
            this.groupBox4.Controls.Add(this.Chart_Stock);
            this.groupBox4.Location = new System.Drawing.Point(6, 7);
            this.groupBox4.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox4.Size = new System.Drawing.Size(1069, 849);
            this.groupBox4.TabIndex = 8;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "그래프";
            // 
            // label_bnfPercent
            // 
            this.label_bnfPercent.AutoSize = true;
            this.label_bnfPercent.Location = new System.Drawing.Point(129, 60);
            this.label_bnfPercent.Name = "label_bnfPercent";
            this.label_bnfPercent.Size = new System.Drawing.Size(0, 12);
            this.label_bnfPercent.TabIndex = 14;
            // 
            // trackBar_bnf
            // 
            this.trackBar_bnf.Location = new System.Drawing.Point(249, 12);
            this.trackBar_bnf.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.trackBar_bnf.Name = "trackBar_bnf";
            this.trackBar_bnf.Size = new System.Drawing.Size(547, 45);
            this.trackBar_bnf.TabIndex = 13;
            this.trackBar_bnf.TickStyle = System.Windows.Forms.TickStyle.Both;
            this.trackBar_bnf.Scroll += new System.EventHandler(this.trackBar_bnf_Scroll);
            // 
            // graph_line_check
            // 
            this.graph_line_check.AutoSize = true;
            this.graph_line_check.Location = new System.Drawing.Point(63, 60);
            this.graph_line_check.Name = "graph_line_check";
            this.graph_line_check.Size = new System.Drawing.Size(60, 16);
            this.graph_line_check.TabIndex = 12;
            this.graph_line_check.Text = "추세선";
            this.graph_line_check.UseVisualStyleBackColor = true;
            this.graph_line_check.CheckedChanged += new System.EventHandler(this.graph_line_check_CheckedChanged);
            // 
            // graph_bnf_check
            // 
            this.graph_bnf_check.AutoSize = true;
            this.graph_bnf_check.Location = new System.Drawing.Point(129, 30);
            this.graph_bnf_check.Name = "graph_bnf_check";
            this.graph_bnf_check.Size = new System.Drawing.Size(48, 16);
            this.graph_bnf_check.TabIndex = 11;
            this.graph_bnf_check.Text = "BNF";
            this.graph_bnf_check.UseVisualStyleBackColor = true;
            this.graph_bnf_check.CheckedChanged += new System.EventHandler(this.graph_bnf_check_CheckedChanged);
            // 
            // graph_boll_check
            // 
            this.graph_boll_check.AutoSize = true;
            this.graph_boll_check.Location = new System.Drawing.Point(63, 30);
            this.graph_boll_check.Name = "graph_boll_check";
            this.graph_boll_check.Size = new System.Drawing.Size(60, 16);
            this.graph_boll_check.TabIndex = 10;
            this.graph_boll_check.Text = "볼린져";
            this.graph_boll_check.UseVisualStyleBackColor = true;
            this.graph_boll_check.CheckedChanged += new System.EventHandler(this.graph_boll_check_CheckedChanged);
            // 
            // graph_ema_check
            // 
            this.graph_ema_check.AutoSize = true;
            this.graph_ema_check.Location = new System.Drawing.Point(8, 30);
            this.graph_ema_check.Name = "graph_ema_check";
            this.graph_ema_check.Size = new System.Drawing.Size(51, 16);
            this.graph_ema_check.TabIndex = 9;
            this.graph_ema_check.Text = "EMA";
            this.graph_ema_check.UseVisualStyleBackColor = true;
            this.graph_ema_check.CheckedChanged += new System.EventHandler(this.graph_ema_check_CheckedChanged);
            // 
            // TrackBar_stock
            // 
            this.TrackBar_stock.Location = new System.Drawing.Point(307, -3);
            this.TrackBar_stock.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.TrackBar_stock.Name = "TrackBar_stock";
            this.TrackBar_stock.Size = new System.Drawing.Size(784, 45);
            this.TrackBar_stock.TabIndex = 7;
            this.TrackBar_stock.TickStyle = System.Windows.Forms.TickStyle.Both;
            this.TrackBar_stock.Scroll += new System.EventHandler(this.TrackBar_stock_Scroll);
            // 
            // Chart_MACD
            // 
            chartArea1.Name = "ChartArea1";
            this.Chart_MACD.ChartAreas.Add(chartArea1);
            legend1.Name = "Legend1";
            this.Chart_MACD.Legends.Add(legend1);
            this.Chart_MACD.Location = new System.Drawing.Point(8, 625);
            this.Chart_MACD.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Chart_MACD.Name = "Chart_MACD";
            series1.ChartArea = "ChartArea1";
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            this.Chart_MACD.Series.Add(series1);
            this.Chart_MACD.Size = new System.Drawing.Size(1055, 216);
            this.Chart_MACD.TabIndex = 4;
            this.Chart_MACD.Text = "stock chart";
            // 
            // Chart_Stock
            // 
            chartArea2.Name = "ChartArea1";
            this.Chart_Stock.ChartAreas.Add(chartArea2);
            legend2.Name = "Legend1";
            this.Chart_Stock.Legends.Add(legend2);
            this.Chart_Stock.Location = new System.Drawing.Point(6, 111);
            this.Chart_Stock.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Chart_Stock.Name = "Chart_Stock";
            series2.ChartArea = "ChartArea1";
            series2.Legend = "Legend1";
            series2.Name = "Series1";
            this.Chart_Stock.Series.Add(series2);
            this.Chart_Stock.Size = new System.Drawing.Size(1057, 479);
            this.Chart_Stock.TabIndex = 2;
            this.Chart_Stock.Text = "stock chart";
            // 
            // RadioButton_stock_15min
            // 
            this.RadioButton_stock_15min.AutoSize = true;
            this.RadioButton_stock_15min.Location = new System.Drawing.Point(15, 13);
            this.RadioButton_stock_15min.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.RadioButton_stock_15min.Name = "RadioButton_stock_15min";
            this.RadioButton_stock_15min.Size = new System.Drawing.Size(47, 16);
            this.RadioButton_stock_15min.TabIndex = 5;
            this.RadioButton_stock_15min.TabStop = true;
            this.RadioButton_stock_15min.Text = "15분";
            this.RadioButton_stock_15min.UseVisualStyleBackColor = true;
            this.RadioButton_stock_15min.CheckedChanged += new System.EventHandler(this.RadioButton_stock_15min_CheckedChanged);
            // 
            // RadioButton_stock_1hour
            // 
            this.RadioButton_stock_1hour.AutoSize = true;
            this.RadioButton_stock_1hour.Location = new System.Drawing.Point(75, 13);
            this.RadioButton_stock_1hour.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.RadioButton_stock_1hour.Name = "RadioButton_stock_1hour";
            this.RadioButton_stock_1hour.Size = new System.Drawing.Size(53, 16);
            this.RadioButton_stock_1hour.TabIndex = 5;
            this.RadioButton_stock_1hour.TabStop = true;
            this.RadioButton_stock_1hour.Text = "1시간";
            this.RadioButton_stock_1hour.UseVisualStyleBackColor = true;
            this.RadioButton_stock_1hour.CheckedChanged += new System.EventHandler(this.RadioButton_stock_1hour_CheckedChanged);
            // 
            // RadioButton_stock_4hour
            // 
            this.RadioButton_stock_4hour.AutoSize = true;
            this.RadioButton_stock_4hour.Location = new System.Drawing.Point(134, 13);
            this.RadioButton_stock_4hour.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.RadioButton_stock_4hour.Name = "RadioButton_stock_4hour";
            this.RadioButton_stock_4hour.Size = new System.Drawing.Size(53, 16);
            this.RadioButton_stock_4hour.TabIndex = 5;
            this.RadioButton_stock_4hour.TabStop = true;
            this.RadioButton_stock_4hour.Text = "4시간";
            this.RadioButton_stock_4hour.UseVisualStyleBackColor = true;
            this.RadioButton_stock_4hour.CheckedChanged += new System.EventHandler(this.RadioButton_stock_4hour_CheckedChanged);
            // 
            // RadioButton_stock_week
            // 
            this.RadioButton_stock_week.AutoSize = true;
            this.RadioButton_stock_week.Location = new System.Drawing.Point(248, 13);
            this.RadioButton_stock_week.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.RadioButton_stock_week.Name = "RadioButton_stock_week";
            this.RadioButton_stock_week.Size = new System.Drawing.Size(53, 16);
            this.RadioButton_stock_week.TabIndex = 5;
            this.RadioButton_stock_week.TabStop = true;
            this.RadioButton_stock_week.Text = "Week";
            this.RadioButton_stock_week.UseVisualStyleBackColor = true;
            this.RadioButton_stock_week.CheckedChanged += new System.EventHandler(this.RadioButton_stock_week_CheckedChanged);
            // 
            // RadioButton_stock_day
            // 
            this.RadioButton_stock_day.AutoSize = true;
            this.RadioButton_stock_day.Location = new System.Drawing.Point(197, 13);
            this.RadioButton_stock_day.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.RadioButton_stock_day.Name = "RadioButton_stock_day";
            this.RadioButton_stock_day.Size = new System.Drawing.Size(45, 16);
            this.RadioButton_stock_day.TabIndex = 5;
            this.RadioButton_stock_day.TabStop = true;
            this.RadioButton_stock_day.Text = "Day";
            this.RadioButton_stock_day.UseVisualStyleBackColor = true;
            this.RadioButton_stock_day.CheckedChanged += new System.EventHandler(this.RadioButton_stock_day_CheckedChanged);
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.WebBrowser_Stock);
            this.groupBox5.Location = new System.Drawing.Point(1098, 13);
            this.groupBox5.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox5.Size = new System.Drawing.Size(452, 924);
            this.groupBox5.TabIndex = 10;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "기술 참고";
            // 
            // WebBrowser_Stock
            // 
            this.WebBrowser_Stock.Location = new System.Drawing.Point(6, 15);
            this.WebBrowser_Stock.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.WebBrowser_Stock.MinimumSize = new System.Drawing.Size(20, 25);
            this.WebBrowser_Stock.Name = "WebBrowser_Stock";
            this.WebBrowser_Stock.Size = new System.Drawing.Size(440, 901);
            this.WebBrowser_Stock.TabIndex = 1;
            this.WebBrowser_Stock.Url = new System.Uri("http://ㅡ", System.UriKind.Absolute);
            // 
            // tabControl_stock
            // 
            this.tabControl_stock.Controls.Add(this.tabPage_chart);
            this.tabControl_stock.Controls.Add(this.tabPage_TradeModule);
            this.tabControl_stock.Controls.Add(this.tabPage_eval);
            this.tabControl_stock.Location = new System.Drawing.Point(12, 36);
            this.tabControl_stock.Name = "tabControl_stock";
            this.tabControl_stock.SelectedIndex = 0;
            this.tabControl_stock.Size = new System.Drawing.Size(1086, 889);
            this.tabControl_stock.TabIndex = 11;
            this.tabControl_stock.SelectedIndexChanged += new System.EventHandler(this.tabControl_stock_SelectedIndexChanged);
            // 
            // tabPage_chart
            // 
            this.tabPage_chart.Controls.Add(this.groupBox4);
            this.tabPage_chart.Location = new System.Drawing.Point(4, 22);
            this.tabPage_chart.Name = "tabPage_chart";
            this.tabPage_chart.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_chart.Size = new System.Drawing.Size(1078, 863);
            this.tabPage_chart.TabIndex = 0;
            this.tabPage_chart.Text = "차트";
            this.tabPage_chart.UseVisualStyleBackColor = true;
            // 
            // tabPage_TradeModule
            // 
            this.tabPage_TradeModule.Controls.Add(this.DataGridView_TradeModule);
            this.tabPage_TradeModule.Controls.Add(this.Chart_BackTest);
            this.tabPage_TradeModule.Location = new System.Drawing.Point(4, 22);
            this.tabPage_TradeModule.Name = "tabPage_TradeModule";
            this.tabPage_TradeModule.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_TradeModule.Size = new System.Drawing.Size(1078, 863);
            this.tabPage_TradeModule.TabIndex = 2;
            this.tabPage_TradeModule.Text = "백테스트";
            this.tabPage_TradeModule.UseVisualStyleBackColor = true;
            // 
            // DataGridView_TradeModule
            // 
            this.DataGridView_TradeModule.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DataGridView_TradeModule.Location = new System.Drawing.Point(6, 582);
            this.DataGridView_TradeModule.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.DataGridView_TradeModule.Name = "DataGridView_TradeModule";
            this.DataGridView_TradeModule.ReadOnly = true;
            this.DataGridView_TradeModule.RowTemplate.Height = 23;
            this.DataGridView_TradeModule.Size = new System.Drawing.Size(1066, 274);
            this.DataGridView_TradeModule.TabIndex = 8;
            // 
            // Chart_TradeModule
            // 
            chartArea3.Name = "ChartArea1";
            this.Chart_BackTest.ChartAreas.Add(chartArea3);
            legend3.Name = "Legend1";
            this.Chart_BackTest.Legends.Add(legend3);
            this.Chart_BackTest.Location = new System.Drawing.Point(6, 17);
            this.Chart_BackTest.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Chart_BackTest.Name = "Chart_TradeModule";
            series3.ChartArea = "ChartArea1";
            series3.Legend = "Legend1";
            series3.Name = "Series1";
            this.Chart_BackTest.Series.Add(series3);
            this.Chart_BackTest.Size = new System.Drawing.Size(1066, 542);
            this.Chart_BackTest.TabIndex = 3;
            this.Chart_BackTest.Text = "stock chart";
            // 
            // tabPage_eval
            // 
            this.tabPage_eval.Controls.Add(this.groupBox2);
            this.tabPage_eval.Location = new System.Drawing.Point(4, 22);
            this.tabPage_eval.Name = "tabPage_eval";
            this.tabPage_eval.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_eval.Size = new System.Drawing.Size(1078, 863);
            this.tabPage_eval.TabIndex = 1;
            this.tabPage_eval.Text = "평가";
            this.tabPage_eval.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.DataGridView_Evaluation);
            this.groupBox2.Controls.Add(this.DataGridView_StockInfo);
            this.groupBox2.Location = new System.Drawing.Point(6, 7);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox2.Size = new System.Drawing.Size(1069, 885);
            this.groupBox2.TabIndex = 7;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Stock infomation";
            // 
            // DataGridView_Evaluation
            // 
            this.DataGridView_Evaluation.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DataGridView_Evaluation.Location = new System.Drawing.Point(6, 25);
            this.DataGridView_Evaluation.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.DataGridView_Evaluation.Name = "DataGridView_Evaluation";
            this.DataGridView_Evaluation.ReadOnly = true;
            this.DataGridView_Evaluation.RowTemplate.Height = 23;
            this.DataGridView_Evaluation.Size = new System.Drawing.Size(1057, 201);
            this.DataGridView_Evaluation.TabIndex = 7;
            // 
            // DataGridView_StockInfo
            // 
            this.DataGridView_StockInfo.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DataGridView_StockInfo.Location = new System.Drawing.Point(6, 234);
            this.DataGridView_StockInfo.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.DataGridView_StockInfo.Name = "DataGridView_StockInfo";
            this.DataGridView_StockInfo.ReadOnly = true;
            this.DataGridView_StockInfo.RowTemplate.Height = 23;
            this.DataGridView_StockInfo.Size = new System.Drawing.Size(1057, 615);
            this.DataGridView_StockInfo.TabIndex = 5;
            // 
            // StockChartDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1563, 943);
            this.Controls.Add(this.tabControl_stock);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.RadioButton_stock_week);
            this.Controls.Add(this.RadioButton_stock_day);
            this.Controls.Add(this.RadioButton_stock_4hour);
            this.Controls.Add(this.RadioButton_stock_1hour);
            this.Controls.Add(this.TrackBar_stock);
            this.Controls.Add(this.RadioButton_stock_15min);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "StockChartDlg";
            this.Text = "StockChart";
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar_bnf)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TrackBar_stock)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Chart_MACD)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Chart_Stock)).EndInit();
            this.groupBox5.ResumeLayout(false);
            this.tabControl_stock.ResumeLayout(false);
            this.tabPage_chart.ResumeLayout(false);
            this.tabPage_TradeModule.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.DataGridView_TradeModule)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Chart_BackTest)).EndInit();
            this.tabPage_eval.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.DataGridView_Evaluation)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.DataGridView_StockInfo)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.CheckBox graph_line_check;
        private System.Windows.Forms.CheckBox graph_bnf_check;
        private System.Windows.Forms.CheckBox graph_boll_check;
        private System.Windows.Forms.CheckBox graph_ema_check;
        private System.Windows.Forms.TrackBar TrackBar_stock;
        private System.Windows.Forms.DataVisualization.Charting.Chart Chart_MACD;

        private System.Windows.Forms.RadioButton RadioButton_stock_15min;
        private System.Windows.Forms.RadioButton RadioButton_stock_1hour;
        private System.Windows.Forms.RadioButton RadioButton_stock_4hour;
        private System.Windows.Forms.RadioButton RadioButton_stock_day;
        private System.Windows.Forms.RadioButton RadioButton_stock_week;

        private System.Windows.Forms.DataVisualization.Charting.Chart Chart_Stock;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.WebBrowser WebBrowser_Stock;
        private System.Windows.Forms.TrackBar trackBar_bnf;
        private System.Windows.Forms.Label label_bnfPercent;
        private System.Windows.Forms.TabControl tabControl_stock;
        private System.Windows.Forms.TabPage tabPage_chart;
        private System.Windows.Forms.TabPage tabPage_TradeModule;
        private System.Windows.Forms.TabPage tabPage_eval;
        private System.Windows.Forms.GroupBox groupBox2;
        private QuickDataGridView DataGridView_Evaluation;
        private QuickDataGridView DataGridView_StockInfo;
        private QuickDataGridView DataGridView_TradeModule;
        private System.Windows.Forms.DataVisualization.Charting.Chart Chart_BackTest;
    }
}