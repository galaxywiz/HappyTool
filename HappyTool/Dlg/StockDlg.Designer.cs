using HappyTool.Util;

namespace HappyTool.Dlg
{
    partial class StockDlg
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
            if (disposing && (components != null)) {
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend2 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.Label_account = new System.Windows.Forms.Label();
            this.Label_id = new System.Windows.Forms.Label();
            this.Label_money = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.Button_quit = new System.Windows.Forms.Button();
            this.DataGridView_StockPool = new QuickDataGridView();
            this.comboBox_BackTest = new System.Windows.Forms.ComboBox();
            this.radioButton_week = new System.Windows.Forms.RadioButton();
            this.radioButton_day = new System.Windows.Forms.RadioButton();
            this.radioButton_4hour = new System.Windows.Forms.RadioButton();
            this.quickChart_profit = new QuickChart();
            this.radioButton_1hour = new System.Windows.Forms.RadioButton();
            this.radioButton_15min = new System.Windows.Forms.RadioButton();
            this.quickChart_winRate = new QuickChart();
            this.richTextBox_BackTestResult = new System.Windows.Forms.RichTextBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage_Watching = new System.Windows.Forms.TabPage();
            this.tabPage_Report = new System.Windows.Forms.TabPage();
            this.Group3 = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DataGridView_StockPool)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.quickChart_profit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.quickChart_winRate)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabPage_Watching.SuspendLayout();
            this.tabPage_Report.SuspendLayout();
            this.Group3.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.Label_account);
            this.groupBox1.Controls.Add(this.Label_id);
            this.groupBox1.Controls.Add(this.Label_money);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(6, 19);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox1.Size = new System.Drawing.Size(352, 69);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Account";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(16, 21);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(16, 12);
            this.label3.TabIndex = 8;
            this.label3.Text = "ID";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(178, 21);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(57, 12);
            this.label2.TabIndex = 7;
            this.label2.Text = "계좌 번호";
            // 
            // Label_account
            // 
            this.Label_account.AutoSize = true;
            this.Label_account.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.Label_account.Location = new System.Drawing.Point(260, 21);
            this.Label_account.Name = "Label_account";
            this.Label_account.Size = new System.Drawing.Size(47, 14);
            this.Label_account.TabIndex = 6;
            this.Label_account.Text = "          ";
            // 
            // Label_id
            // 
            this.Label_id.AutoSize = true;
            this.Label_id.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.Label_id.Location = new System.Drawing.Point(98, 19);
            this.Label_id.Name = "Label_id";
            this.Label_id.Size = new System.Drawing.Size(47, 14);
            this.Label_id.TabIndex = 5;
            this.Label_id.Text = "          ";
            // 
            // Label_money
            // 
            this.Label_money.AutoSize = true;
            this.Label_money.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.Label_money.Location = new System.Drawing.Point(98, 44);
            this.Label_money.Name = "Label_money";
            this.Label_money.Size = new System.Drawing.Size(47, 14);
            this.Label_money.TabIndex = 4;
            this.Label_money.Text = "          ";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 44);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(69, 12);
            this.label1.TabIndex = 3;
            this.label1.Text = "계좌 가용액";
            // 
            // Button_quit
            // 
            this.Button_quit.Location = new System.Drawing.Point(783, 21);
            this.Button_quit.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Button_quit.Name = "Button_quit";
            this.Button_quit.Size = new System.Drawing.Size(75, 61);
            this.Button_quit.TabIndex = 2;
            this.Button_quit.Text = "종료";
            this.Button_quit.UseVisualStyleBackColor = true;
            this.Button_quit.Click += new System.EventHandler(this.Button_quit_Click);
            // 
            // DataGridView_StockPool
            // 
            dataGridViewCellStyle1.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.DataGridView_StockPool.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.DataGridView_StockPool.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.DataGridView_StockPool.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DataGridView_StockPool.Location = new System.Drawing.Point(6, 7);
            this.DataGridView_StockPool.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.DataGridView_StockPool.Name = "DataGridView_StockPool";
            this.DataGridView_StockPool.ReadOnly = true;
            this.DataGridView_StockPool.RowTemplate.Height = 23;
            this.DataGridView_StockPool.Size = new System.Drawing.Size(848, 419);
            this.DataGridView_StockPool.TabIndex = 4;
            this.DataGridView_StockPool.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridView_StockPool_CellContentClick);
            // 
            // comboBox_BackTest
            // 
            this.comboBox_BackTest.FormattingEnabled = true;
            this.comboBox_BackTest.Location = new System.Drawing.Point(466, 60);
            this.comboBox_BackTest.Name = "comboBox_BackTest";
            this.comboBox_BackTest.Size = new System.Drawing.Size(175, 20);
            this.comboBox_BackTest.TabIndex = 3;
            this.comboBox_BackTest.SelectedIndexChanged += new System.EventHandler(this.comboBox_BackTest_SelectedIndexChanged);
            // 
            // radioButton_week
            // 
            this.radioButton_week.AutoSize = true;
            this.radioButton_week.Location = new System.Drawing.Point(606, 29);
            this.radioButton_week.Name = "radioButton_week";
            this.radioButton_week.Size = new System.Drawing.Size(35, 16);
            this.radioButton_week.TabIndex = 1;
            this.radioButton_week.TabStop = true;
            this.radioButton_week.Text = "주";
            this.radioButton_week.UseVisualStyleBackColor = true;
            this.radioButton_week.CheckedChanged += new System.EventHandler(this.radioButton_week_CheckedChanged);
            // 
            // radioButton_day
            // 
            this.radioButton_day.AutoSize = true;
            this.radioButton_day.Location = new System.Drawing.Point(565, 29);
            this.radioButton_day.Name = "radioButton_day";
            this.radioButton_day.Size = new System.Drawing.Size(35, 16);
            this.radioButton_day.TabIndex = 1;
            this.radioButton_day.TabStop = true;
            this.radioButton_day.Text = "일";
            this.radioButton_day.UseVisualStyleBackColor = true;
            this.radioButton_day.CheckedChanged += new System.EventHandler(this.radioButton_day_CheckedChanged);
            // 
            // radioButton_4hour
            // 
            this.radioButton_4hour.AutoSize = true;
            this.radioButton_4hour.Location = new System.Drawing.Point(506, 29);
            this.radioButton_4hour.Name = "radioButton_4hour";
            this.radioButton_4hour.Size = new System.Drawing.Size(53, 16);
            this.radioButton_4hour.TabIndex = 1;
            this.radioButton_4hour.TabStop = true;
            this.radioButton_4hour.Text = "4시간";
            this.radioButton_4hour.UseVisualStyleBackColor = true;
            this.radioButton_4hour.CheckedChanged += new System.EventHandler(this.radioButton_4hour_CheckedChanged);
            // 
            // quickChart_profit
            // 
            chartArea1.Name = "ChartArea1";
            this.quickChart_profit.ChartAreas.Add(chartArea1);
            legend1.Name = "Legend1";
            this.quickChart_profit.Legends.Add(legend1);
            this.quickChart_profit.Location = new System.Drawing.Point(6, 23);
            this.quickChart_profit.Name = "quickChart_profit";
            series1.ChartArea = "ChartArea1";
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            this.quickChart_profit.Series.Add(series1);
            this.quickChart_profit.Size = new System.Drawing.Size(415, 241);
            this.quickChart_profit.TabIndex = 0;
            this.quickChart_profit.Text = "quickChart_profit";
            // 
            // radioButton_1hour
            // 
            this.radioButton_1hour.AutoSize = true;
            this.radioButton_1hour.Location = new System.Drawing.Point(447, 29);
            this.radioButton_1hour.Name = "radioButton_1hour";
            this.radioButton_1hour.Size = new System.Drawing.Size(53, 16);
            this.radioButton_1hour.TabIndex = 1;
            this.radioButton_1hour.TabStop = true;
            this.radioButton_1hour.Text = "1시간";
            this.radioButton_1hour.UseVisualStyleBackColor = true;
            this.radioButton_1hour.CheckedChanged += new System.EventHandler(this.radioButton_1hour_CheckedChanged);
            // 
            // radioButton_15min
            // 
            this.radioButton_15min.AutoSize = true;
            this.radioButton_15min.Location = new System.Drawing.Point(394, 29);
            this.radioButton_15min.Name = "radioButton_15min";
            this.radioButton_15min.Size = new System.Drawing.Size(47, 16);
            this.radioButton_15min.TabIndex = 1;
            this.radioButton_15min.TabStop = true;
            this.radioButton_15min.Text = "15분";
            this.radioButton_15min.UseVisualStyleBackColor = true;
            this.radioButton_15min.CheckedChanged += new System.EventHandler(this.radioButton1_CheckedChanged);
            // 
            // quickChart_winRate
            // 
            chartArea2.Name = "ChartArea1";
            this.quickChart_winRate.ChartAreas.Add(chartArea2);
            legend2.Name = "Legend1";
            this.quickChart_winRate.Legends.Add(legend2);
            this.quickChart_winRate.Location = new System.Drawing.Point(427, 23);
            this.quickChart_winRate.Name = "quickChart_winRate";
            series2.ChartArea = "ChartArea1";
            series2.Legend = "Legend1";
            series2.Name = "Series1";
            this.quickChart_winRate.Series.Add(series2);
            this.quickChart_winRate.Size = new System.Drawing.Size(415, 241);
            this.quickChart_winRate.TabIndex = 0;
            this.quickChart_winRate.Text = "quickChart_winRate";
            // 
            // richTextBox_BackTestResult
            // 
            this.richTextBox_BackTestResult.Location = new System.Drawing.Point(6, 270);
            this.richTextBox_BackTestResult.Name = "richTextBox_BackTestResult";
            this.richTextBox_BackTestResult.Size = new System.Drawing.Size(848, 157);
            this.richTextBox_BackTestResult.TabIndex = 13;
            this.richTextBox_BackTestResult.Text = "";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage_Watching);
            this.tabControl1.Controls.Add(this.tabPage_Report);
            this.tabControl1.Location = new System.Drawing.Point(12, 114);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(868, 459);
            this.tabControl1.TabIndex = 14;
            // 
            // tabPage_Watching
            // 
            this.tabPage_Watching.Controls.Add(this.DataGridView_StockPool);
            this.tabPage_Watching.Location = new System.Drawing.Point(4, 22);
            this.tabPage_Watching.Name = "tabPage_Watching";
            this.tabPage_Watching.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_Watching.Size = new System.Drawing.Size(860, 433);
            this.tabPage_Watching.TabIndex = 0;
            this.tabPage_Watching.Text = "감시중인 종목들";
            this.tabPage_Watching.UseVisualStyleBackColor = true;
            // 
            // tabPage_Report
            // 
            this.tabPage_Report.Controls.Add(this.quickChart_winRate);
            this.tabPage_Report.Controls.Add(this.quickChart_profit);
            this.tabPage_Report.Controls.Add(this.richTextBox_BackTestResult);
            this.tabPage_Report.Location = new System.Drawing.Point(4, 22);
            this.tabPage_Report.Name = "tabPage_Report";
            this.tabPage_Report.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_Report.Size = new System.Drawing.Size(860, 433);
            this.tabPage_Report.TabIndex = 1;
            this.tabPage_Report.Text = "백테스트 결과";
            this.tabPage_Report.UseVisualStyleBackColor = true;
            // 
            // Group3
            // 
            this.Group3.Controls.Add(this.label4);
            this.Group3.Controls.Add(this.groupBox1);
            this.Group3.Controls.Add(this.radioButton_15min);
            this.Group3.Controls.Add(this.comboBox_BackTest);
            this.Group3.Controls.Add(this.radioButton_1hour);
            this.Group3.Controls.Add(this.radioButton_week);
            this.Group3.Controls.Add(this.Button_quit);
            this.Group3.Controls.Add(this.radioButton_4hour);
            this.Group3.Controls.Add(this.radioButton_day);
            this.Group3.Location = new System.Drawing.Point(12, 12);
            this.Group3.Name = "Group3";
            this.Group3.Size = new System.Drawing.Size(864, 96);
            this.Group3.TabIndex = 15;
            this.Group3.TabStop = false;
            this.Group3.Text = "컨트롤창";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(394, 63);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 12);
            this.label4.TabIndex = 4;
            this.label4.Text = "매매전략";
            // 
            // StockDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(886, 575);
            this.ControlBox = false;
            this.Controls.Add(this.Group3);
            this.Controls.Add(this.tabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "StockDlg";
            this.Text = "주식 봇 for 키움증권 ";
            this.Shown += new System.EventHandler(this.StockDlg_Shown);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DataGridView_StockPool)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.quickChart_profit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.quickChart_winRate)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabPage_Watching.ResumeLayout(false);
            this.tabPage_Report.ResumeLayout(false);
            this.Group3.ResumeLayout(false);
            this.Group3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private QuickDataGridView DataGridView_StockPool;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label Label_account;
        private System.Windows.Forms.Label Label_id;
        private System.Windows.Forms.Label Label_money;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button Button_quit;
        private System.Windows.Forms.RadioButton radioButton_week;
        private System.Windows.Forms.RadioButton radioButton_day;
        private System.Windows.Forms.RadioButton radioButton_4hour;
        private System.Windows.Forms.RadioButton radioButton_1hour;
        private System.Windows.Forms.RadioButton radioButton_15min;
        internal QuickChart quickChart_profit;
        internal QuickChart quickChart_winRate;
        internal System.Windows.Forms.ComboBox comboBox_BackTest;
        internal System.Windows.Forms.RichTextBox richTextBox_BackTestResult;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage_Watching;
        private System.Windows.Forms.TabPage tabPage_Report;
        private System.Windows.Forms.GroupBox Group3;
        private System.Windows.Forms.Label label4;
    }
}