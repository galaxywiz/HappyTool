using HappyTool.Util;
using UtilLibrary;

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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea3 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend3 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series3 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea4 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend4 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series4 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.Chart_Stock = new UtilLibrary.QuickChart();
            this.WebBrowser_Stock = new System.Windows.Forms.WebBrowser();
            this.tabControl_stock = new System.Windows.Forms.TabControl();
            this.tabPage_chart = new System.Windows.Forms.TabPage();
            this.tabPage_StrategyModule = new System.Windows.Forms.TabPage();
            this.richTextBox_backTestLog = new System.Windows.Forms.RichTextBox();
            this.DataGridView_StrategyModule = new UtilLibrary.QuickDataGridView();
            this.Chart_BackTest = new UtilLibrary.QuickChart();
            this.tabPage_eval = new System.Windows.Forms.TabPage();
            this.DataGridView_StockInfo = new UtilLibrary.QuickDataGridView();
            this.tabPage_web = new System.Windows.Forms.TabPage();
            ((System.ComponentModel.ISupportInitialize)(this.Chart_Stock)).BeginInit();
            this.tabControl_stock.SuspendLayout();
            this.tabPage_chart.SuspendLayout();
            this.tabPage_StrategyModule.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DataGridView_StrategyModule)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Chart_BackTest)).BeginInit();
            this.tabPage_eval.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DataGridView_StockInfo)).BeginInit();
            this.tabPage_web.SuspendLayout();
            this.SuspendLayout();
            // 
            // Chart_Stock
            // 
            chartArea3.Name = "ChartArea1";
            this.Chart_Stock.ChartAreas.Add(chartArea3);
            legend3.Name = "Legend1";
            this.Chart_Stock.Legends.Add(legend3);
            this.Chart_Stock.Location = new System.Drawing.Point(6, 4);
            this.Chart_Stock.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Chart_Stock.Name = "Chart_Stock";
            series3.ChartArea = "ChartArea1";
            series3.Legend = "Legend1";
            series3.Name = "Series1";
            this.Chart_Stock.Series.Add(series3);
            this.Chart_Stock.Size = new System.Drawing.Size(1063, 672);
            this.Chart_Stock.TabIndex = 2;
            this.Chart_Stock.Text = "stock chart";
            // 
            // WebBrowser_Stock
            // 
            this.WebBrowser_Stock.Location = new System.Drawing.Point(6, 7);
            this.WebBrowser_Stock.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.WebBrowser_Stock.MinimumSize = new System.Drawing.Size(20, 25);
            this.WebBrowser_Stock.Name = "WebBrowser_Stock";
            this.WebBrowser_Stock.Size = new System.Drawing.Size(1063, 672);
            this.WebBrowser_Stock.TabIndex = 1;
            this.WebBrowser_Stock.Url = new System.Uri("http://ㅡ", System.UriKind.Absolute);
            // 
            // tabControl_stock
            // 
            this.tabControl_stock.Controls.Add(this.tabPage_chart);
            this.tabControl_stock.Controls.Add(this.tabPage_StrategyModule);
            this.tabControl_stock.Controls.Add(this.tabPage_eval);
            this.tabControl_stock.Controls.Add(this.tabPage_web);
            this.tabControl_stock.Location = new System.Drawing.Point(12, 12);
            this.tabControl_stock.Name = "tabControl_stock";
            this.tabControl_stock.SelectedIndex = 0;
            this.tabControl_stock.Size = new System.Drawing.Size(1083, 709);
            this.tabControl_stock.TabIndex = 11;
            this.tabControl_stock.SelectedIndexChanged += new System.EventHandler(this.tabControl_stock_SelectedIndexChanged);
            // 
            // tabPage_chart
            // 
            this.tabPage_chart.Controls.Add(this.Chart_Stock);
            this.tabPage_chart.Location = new System.Drawing.Point(4, 22);
            this.tabPage_chart.Name = "tabPage_chart";
            this.tabPage_chart.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_chart.Size = new System.Drawing.Size(1075, 683);
            this.tabPage_chart.TabIndex = 0;
            this.tabPage_chart.Text = "차트";
            this.tabPage_chart.UseVisualStyleBackColor = true;
            // 
            // tabPage_StrategyModule
            // 
            this.tabPage_StrategyModule.Controls.Add(this.richTextBox_backTestLog);
            this.tabPage_StrategyModule.Controls.Add(this.DataGridView_StrategyModule);
            this.tabPage_StrategyModule.Controls.Add(this.Chart_BackTest);
            this.tabPage_StrategyModule.Location = new System.Drawing.Point(4, 22);
            this.tabPage_StrategyModule.Name = "tabPage_StrategyModule";
            this.tabPage_StrategyModule.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_StrategyModule.Size = new System.Drawing.Size(1075, 683);
            this.tabPage_StrategyModule.TabIndex = 2;
            this.tabPage_StrategyModule.Text = "백테스트";
            this.tabPage_StrategyModule.UseVisualStyleBackColor = true;
            // 
            // richTextBox_backTestLog
            // 
            this.richTextBox_backTestLog.Location = new System.Drawing.Point(697, 443);
            this.richTextBox_backTestLog.Name = "richTextBox_backTestLog";
            this.richTextBox_backTestLog.Size = new System.Drawing.Size(372, 233);
            this.richTextBox_backTestLog.TabIndex = 17;
            this.richTextBox_backTestLog.Text = "";
            // 
            // DataGridView_StrategyModule
            // 
            this.DataGridView_StrategyModule.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DataGridView_StrategyModule.Location = new System.Drawing.Point(3, 443);
            this.DataGridView_StrategyModule.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.DataGridView_StrategyModule.Name = "DataGridView_StrategyModule";
            this.DataGridView_StrategyModule.ReadOnly = true;
            this.DataGridView_StrategyModule.RowTemplate.Height = 23;
            this.DataGridView_StrategyModule.Size = new System.Drawing.Size(688, 233);
            this.DataGridView_StrategyModule.TabIndex = 8;
            // 
            // Chart_BackTest
            // 
            chartArea4.Name = "ChartArea1";
            this.Chart_BackTest.ChartAreas.Add(chartArea4);
            legend4.Name = "Legend1";
            this.Chart_BackTest.Legends.Add(legend4);
            this.Chart_BackTest.Location = new System.Drawing.Point(6, 17);
            this.Chart_BackTest.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Chart_BackTest.Name = "Chart_BackTest";
            series4.ChartArea = "ChartArea1";
            series4.Legend = "Legend1";
            series4.Name = "Series1";
            this.Chart_BackTest.Series.Add(series4);
            this.Chart_BackTest.Size = new System.Drawing.Size(1063, 419);
            this.Chart_BackTest.TabIndex = 3;
            this.Chart_BackTest.Text = "stock chart";
            // 
            // tabPage_eval
            // 
            this.tabPage_eval.Controls.Add(this.DataGridView_StockInfo);
            this.tabPage_eval.Location = new System.Drawing.Point(4, 22);
            this.tabPage_eval.Name = "tabPage_eval";
            this.tabPage_eval.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_eval.Size = new System.Drawing.Size(1075, 683);
            this.tabPage_eval.TabIndex = 1;
            this.tabPage_eval.Text = "평가";
            this.tabPage_eval.UseVisualStyleBackColor = true;
            // 
            // DataGridView_StockInfo
            // 
            this.DataGridView_StockInfo.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DataGridView_StockInfo.Location = new System.Drawing.Point(3, 7);
            this.DataGridView_StockInfo.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.DataGridView_StockInfo.Name = "DataGridView_StockInfo";
            this.DataGridView_StockInfo.ReadOnly = true;
            this.DataGridView_StockInfo.RowTemplate.Height = 23;
            this.DataGridView_StockInfo.Size = new System.Drawing.Size(1066, 672);
            this.DataGridView_StockInfo.TabIndex = 5;
            // 
            // tabPage_web
            // 
            this.tabPage_web.Controls.Add(this.WebBrowser_Stock);
            this.tabPage_web.Location = new System.Drawing.Point(4, 22);
            this.tabPage_web.Name = "tabPage_web";
            this.tabPage_web.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_web.Size = new System.Drawing.Size(1075, 683);
            this.tabPage_web.TabIndex = 3;
            this.tabPage_web.Text = "웹페이지 평가";
            this.tabPage_web.UseVisualStyleBackColor = true;
            // 
            // StockChartDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1097, 726);
            this.Controls.Add(this.tabControl_stock);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "StockChartDlg";
            this.Text = "StockChart";
            ((System.ComponentModel.ISupportInitialize)(this.Chart_Stock)).EndInit();
            this.tabControl_stock.ResumeLayout(false);
            this.tabPage_chart.ResumeLayout(false);
            this.tabPage_StrategyModule.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.DataGridView_StrategyModule)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Chart_BackTest)).EndInit();
            this.tabPage_eval.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.DataGridView_StockInfo)).EndInit();
            this.tabPage_web.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        internal System.Windows.Forms.WebBrowser WebBrowser_Stock;
        private System.Windows.Forms.TabControl tabControl_stock;
        private System.Windows.Forms.TabPage tabPage_chart;
        private System.Windows.Forms.TabPage tabPage_StrategyModule;
        private System.Windows.Forms.TabPage tabPage_eval;
        internal QuickDataGridView DataGridView_StockInfo;
        internal QuickDataGridView DataGridView_StrategyModule;
        internal System.Windows.Forms.RichTextBox richTextBox_backTestLog;
        internal QuickChart Chart_Stock;
        internal QuickChart Chart_BackTest;
        internal System.Windows.Forms.TabPage tabPage_web;
    }
}