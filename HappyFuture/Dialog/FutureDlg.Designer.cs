using UtilLibrary;

namespace HappyFuture
{
    partial class FutureDlg
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FutureDlg));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Legend legend2 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Legend legend3 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series3 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.Group3 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.comboBox_priceMin = new System.Windows.Forms.ComboBox();
            this.checkBox_reverse = new System.Windows.Forms.CheckBox();
            this.checkBox_doTrade = new System.Windows.Forms.CheckBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label_serachItem = new System.Windows.Forms.Label();
            this.label_nextUpdate = new System.Windows.Forms.Label();
            this.button_findFutureModule = new System.Windows.Forms.Button();
            this.button_allPayOff = new System.Windows.Forms.Button();
            this.button_manual_sell = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label_memInfo = new System.Windows.Forms.Label();
            this.label_cpuInfo = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.button_reload = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.Label_account = new System.Windows.Forms.Label();
            this.Label_id = new System.Windows.Forms.Label();
            this.label_totalPureProfit = new System.Windows.Forms.Label();
            this.label_totalProfit = new System.Windows.Forms.Label();
            this.label_totalCount = new System.Windows.Forms.Label();
            this.Label_money = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.Button_quit = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.statusStrip_Future = new System.Windows.Forms.StatusStrip();
            this.toolStripStatus_stockInfo = new System.Windows.Forms.ToolStripStatusLabel();
            this.timer_clock = new System.Windows.Forms.Timer(this.components);
            this.tabControl_futureDlg = new System.Windows.Forms.TabControl();
            this.tabPage_Watching = new System.Windows.Forms.TabPage();
            this.label_timer = new System.Windows.Forms.Label();
            this.listView_OrderView = new System.Windows.Forms.ListView();
            this.DataGridView_FuturePool = new UtilLibrary.QuickDataGridView();
            this.listView_tradeStock = new System.Windows.Forms.ListView();
            this.tabPage_OrderView = new System.Windows.Forms.TabPage();
            this.button_exportExcel = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.dateTimePicker_OrderEnd = new System.Windows.Forms.DateTimePicker();
            this.dateTimePicker_OrderStart = new System.Windows.Forms.DateTimePicker();
            this.DataGridView_OrderView = new UtilLibrary.QuickDataGridView();
            this.tabPage_TradeRecode = new System.Windows.Forms.TabPage();
            this.Chart_tradeHistory = new UtilLibrary.QuickChart();
            this.DataGridView_tradeHistory = new UtilLibrary.QuickDataGridView();
            this.label8 = new System.Windows.Forms.Label();
            this.dateTimePicker_MoneyEnd = new System.Windows.Forms.DateTimePicker();
            this.dateTimePicker_MoneyStart = new System.Windows.Forms.DateTimePicker();
            this.DataGridView_tradeRecoder = new UtilLibrary.QuickDataGridView();
            this.tabPage_backTestSimul = new System.Windows.Forms.TabPage();
            this.button_backTestDBSimul = new System.Windows.Forms.Button();
            this.button_totalEachSimul = new System.Windows.Forms.Button();
            this.textBox_testAccount = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.backTestLog_TextBox = new System.Windows.Forms.RichTextBox();
            this.label_backTestInfo = new System.Windows.Forms.Label();
            this.progressBar_backTest = new System.Windows.Forms.ProgressBar();
            this.button_TestCancel = new System.Windows.Forms.Button();
            this.button_moduleClear = new System.Windows.Forms.Button();
            this.button_backTestTotalTradeSimul = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.dateTimePicker_backTestEnd = new System.Windows.Forms.DateTimePicker();
            this.dateTimePicker_backTestStart = new System.Windows.Forms.DateTimePicker();
            this.Chart_TotalPrice = new UtilLibrary.QuickChart();
            this.DataGridView_backTestResult = new UtilLibrary.QuickDataGridView();
            this.Chart_backTestAccountResult = new UtilLibrary.QuickChart();
            this.tabPage_Beleve = new System.Windows.Forms.TabPage();
            this.button_resetWinRate = new System.Windows.Forms.Button();
            this.button_reloadDB = new System.Windows.Forms.Button();
            this.button_configReload = new System.Windows.Forms.Button();
            this.lable_configInfo = new System.Windows.Forms.Label();
            this.progressBar_engine = new System.Windows.Forms.ProgressBar();
            this.Group3.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.statusStrip_Future.SuspendLayout();
            this.tabControl_futureDlg.SuspendLayout();
            this.tabPage_Watching.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DataGridView_FuturePool)).BeginInit();
            this.tabPage_OrderView.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DataGridView_OrderView)).BeginInit();
            this.tabPage_TradeRecode.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Chart_tradeHistory)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.DataGridView_tradeHistory)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.DataGridView_tradeRecoder)).BeginInit();
            this.tabPage_backTestSimul.SuspendLayout();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Chart_TotalPrice)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.DataGridView_backTestResult)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Chart_backTestAccountResult)).BeginInit();
            this.tabPage_Beleve.SuspendLayout();
            this.SuspendLayout();
            // 
            // Group3
            // 
            this.Group3.Controls.Add(this.groupBox3);
            this.Group3.Controls.Add(this.groupBox2);
            this.Group3.Controls.Add(this.groupBox1);
            this.Group3.Controls.Add(this.Button_quit);
            this.Group3.Location = new System.Drawing.Point(12, 8);
            this.Group3.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Group3.Name = "Group3";
            this.Group3.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Group3.Size = new System.Drawing.Size(194, 721);
            this.Group3.TabIndex = 18;
            this.Group3.TabStop = false;
            this.Group3.Text = "컨트롤창";
            // 
            // groupBox3
            // 
            this.groupBox3.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("groupBox3.BackgroundImage")));
            this.groupBox3.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.groupBox3.Controls.Add(this.comboBox_priceMin);
            this.groupBox3.Controls.Add(this.checkBox_reverse);
            this.groupBox3.Controls.Add(this.checkBox_doTrade);
            this.groupBox3.Controls.Add(this.pictureBox1);
            this.groupBox3.Controls.Add(this.label_serachItem);
            this.groupBox3.Controls.Add(this.label_nextUpdate);
            this.groupBox3.Controls.Add(this.button_findFutureModule);
            this.groupBox3.Controls.Add(this.button_allPayOff);
            this.groupBox3.Controls.Add(this.button_manual_sell);
            this.groupBox3.Location = new System.Drawing.Point(6, 273);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox3.Size = new System.Drawing.Size(180, 336);
            this.groupBox3.TabIndex = 1;
            this.groupBox3.TabStop = false;
            // 
            // comboBox_priceMin
            // 
            this.comboBox_priceMin.FormattingEnabled = true;
            this.comboBox_priceMin.Location = new System.Drawing.Point(7, 51);
            this.comboBox_priceMin.Name = "comboBox_priceMin";
            this.comboBox_priceMin.Size = new System.Drawing.Size(163, 28);
            this.comboBox_priceMin.TabIndex = 38;
            this.comboBox_priceMin.SelectedIndexChanged += new System.EventHandler(this.comboBox_priceMin_SelectedIndexChanged);
            // 
            // checkBox_reverse
            // 
            this.checkBox_reverse.AutoSize = true;
            this.checkBox_reverse.BackColor = System.Drawing.Color.Transparent;
            this.checkBox_reverse.ForeColor = System.Drawing.Color.Lime;
            this.checkBox_reverse.Location = new System.Drawing.Point(108, 211);
            this.checkBox_reverse.Name = "checkBox_reverse";
            this.checkBox_reverse.Size = new System.Drawing.Size(76, 24);
            this.checkBox_reverse.TabIndex = 37;
            this.checkBox_reverse.Text = "역주문";
            this.checkBox_reverse.UseVisualStyleBackColor = false;
            this.checkBox_reverse.CheckedChanged += new System.EventHandler(this.CheckBox_reverse_CheckedChanged);
            // 
            // checkBox_doTrade
            // 
            this.checkBox_doTrade.AutoSize = true;
            this.checkBox_doTrade.BackColor = System.Drawing.Color.Transparent;
            this.checkBox_doTrade.ForeColor = System.Drawing.Color.Lime;
            this.checkBox_doTrade.Location = new System.Drawing.Point(108, 246);
            this.checkBox_doTrade.Name = "checkBox_doTrade";
            this.checkBox_doTrade.Size = new System.Drawing.Size(61, 24);
            this.checkBox_doTrade.TabIndex = 37;
            this.checkBox_doTrade.Text = "실전";
            this.checkBox_doTrade.UseVisualStyleBackColor = false;
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.pictureBox1.Image = global::HappyFuture.Properties.Resources.mario2;
            this.pictureBox1.Location = new System.Drawing.Point(6, 212);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(96, 117);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 5;
            this.pictureBox1.TabStop = false;
            // 
            // label_serachItem
            // 
            this.label_serachItem.AutoSize = true;
            this.label_serachItem.BackColor = System.Drawing.Color.Transparent;
            this.label_serachItem.ForeColor = System.Drawing.Color.Lime;
            this.label_serachItem.Location = new System.Drawing.Point(5, 144);
            this.label_serachItem.Name = "label_serachItem";
            this.label_serachItem.Size = new System.Drawing.Size(54, 20);
            this.label_serachItem.TabIndex = 25;
            this.label_serachItem.Text = "찾는거";
            // 
            // label_nextUpdate
            // 
            this.label_nextUpdate.AutoSize = true;
            this.label_nextUpdate.BackColor = System.Drawing.Color.Transparent;
            this.label_nextUpdate.ForeColor = System.Drawing.Color.Lime;
            this.label_nextUpdate.Location = new System.Drawing.Point(5, 20);
            this.label_nextUpdate.Name = "label_nextUpdate";
            this.label_nextUpdate.Size = new System.Drawing.Size(174, 20);
            this.label_nextUpdate.TabIndex = 24;
            this.label_nextUpdate.Text = "다음 분봉 갱신 시간까지";
            // 
            // button_findFutureModule
            // 
            this.button_findFutureModule.Location = new System.Drawing.Point(7, 175);
            this.button_findFutureModule.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.button_findFutureModule.Name = "button_findFutureModule";
            this.button_findFutureModule.Size = new System.Drawing.Size(165, 28);
            this.button_findFutureModule.TabIndex = 3;
            this.button_findFutureModule.Text = "각 종목 전략 찾기";
            this.button_findFutureModule.UseVisualStyleBackColor = true;
            this.button_findFutureModule.Click += new System.EventHandler(this.button_findFutureModule_Click);
            // 
            // button_allPayOff
            // 
            this.button_allPayOff.Location = new System.Drawing.Point(6, 116);
            this.button_allPayOff.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.button_allPayOff.Name = "button_allPayOff";
            this.button_allPayOff.Size = new System.Drawing.Size(165, 24);
            this.button_allPayOff.TabIndex = 2;
            this.button_allPayOff.Text = "모두 청산";
            this.button_allPayOff.UseVisualStyleBackColor = true;
            this.button_allPayOff.Click += new System.EventHandler(this.button_allPayOff_Click);
            // 
            // button_manual_sell
            // 
            this.button_manual_sell.Location = new System.Drawing.Point(7, 82);
            this.button_manual_sell.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.button_manual_sell.Name = "button_manual_sell";
            this.button_manual_sell.Size = new System.Drawing.Size(163, 26);
            this.button_manual_sell.TabIndex = 1;
            this.button_manual_sell.Text = "수동 청산";
            this.button_manual_sell.UseVisualStyleBackColor = true;
            this.button_manual_sell.Click += new System.EventHandler(this.button_manual_sell_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.BackColor = System.Drawing.Color.Black;
            this.groupBox2.Controls.Add(this.label_memInfo);
            this.groupBox2.Controls.Add(this.label_cpuInfo);
            this.groupBox2.ForeColor = System.Drawing.Color.Lime;
            this.groupBox2.Location = new System.Drawing.Point(6, 616);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(180, 89);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "시스템 자원";
            // 
            // label_memInfo
            // 
            this.label_memInfo.AutoSize = true;
            this.label_memInfo.ForeColor = System.Drawing.Color.Lime;
            this.label_memInfo.Location = new System.Drawing.Point(16, 47);
            this.label_memInfo.Name = "label_memInfo";
            this.label_memInfo.Size = new System.Drawing.Size(59, 20);
            this.label_memInfo.TabIndex = 29;
            this.label_memInfo.Text = "          ";
            // 
            // label_cpuInfo
            // 
            this.label_cpuInfo.AutoSize = true;
            this.label_cpuInfo.ForeColor = System.Drawing.Color.Lime;
            this.label_cpuInfo.Location = new System.Drawing.Point(16, 20);
            this.label_cpuInfo.Name = "label_cpuInfo";
            this.label_cpuInfo.Size = new System.Drawing.Size(59, 20);
            this.label_cpuInfo.TabIndex = 28;
            this.label_cpuInfo.Text = "          ";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.button_reload);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label12);
            this.groupBox1.Controls.Add(this.label11);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.Label_account);
            this.groupBox1.Controls.Add(this.Label_id);
            this.groupBox1.Controls.Add(this.label_totalPureProfit);
            this.groupBox1.Controls.Add(this.label_totalProfit);
            this.groupBox1.Controls.Add(this.label_totalCount);
            this.groupBox1.Controls.Add(this.Label_money);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(6, 61);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.groupBox1.Size = new System.Drawing.Size(180, 187);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Account";
            // 
            // button_reload
            // 
            this.button_reload.Location = new System.Drawing.Point(8, 152);
            this.button_reload.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.button_reload.Name = "button_reload";
            this.button_reload.Size = new System.Drawing.Size(163, 24);
            this.button_reload.TabIndex = 0;
            this.button_reload.Text = "계좌 리로드";
            this.button_reload.UseVisualStyleBackColor = true;
            this.button_reload.Click += new System.EventHandler(this.button_reload_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(16, 26);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(24, 20);
            this.label3.TabIndex = 8;
            this.label3.Text = "ID";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(16, 130);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(54, 20);
            this.label12.TabIndex = 24;
            this.label12.Text = "순이익";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(16, 110);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(74, 20);
            this.label11.TabIndex = 24;
            this.label11.Text = "전체 이익";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(16, 90);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(74, 20);
            this.label4.TabIndex = 24;
            this.label4.Text = "보유 갯수";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(74, 20);
            this.label2.TabIndex = 7;
            this.label2.Text = "계좌 번호";
            // 
            // Label_account
            // 
            this.Label_account.AutoSize = true;
            this.Label_account.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.Label_account.Location = new System.Drawing.Point(81, 46);
            this.Label_account.Name = "Label_account";
            this.Label_account.Size = new System.Drawing.Size(61, 22);
            this.Label_account.TabIndex = 5;
            this.Label_account.Text = "          ";
            // 
            // Label_id
            // 
            this.Label_id.AutoSize = true;
            this.Label_id.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.Label_id.Location = new System.Drawing.Point(81, 24);
            this.Label_id.Name = "Label_id";
            this.Label_id.Size = new System.Drawing.Size(61, 22);
            this.Label_id.TabIndex = 5;
            this.Label_id.Text = "          ";
            // 
            // label_totalPureProfit
            // 
            this.label_totalPureProfit.AutoSize = true;
            this.label_totalPureProfit.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label_totalPureProfit.Location = new System.Drawing.Point(81, 128);
            this.label_totalPureProfit.Name = "label_totalPureProfit";
            this.label_totalPureProfit.Size = new System.Drawing.Size(61, 22);
            this.label_totalPureProfit.TabIndex = 4;
            this.label_totalPureProfit.Text = "          ";
            // 
            // label_totalProfit
            // 
            this.label_totalProfit.AutoSize = true;
            this.label_totalProfit.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label_totalProfit.Location = new System.Drawing.Point(81, 108);
            this.label_totalProfit.Name = "label_totalProfit";
            this.label_totalProfit.Size = new System.Drawing.Size(61, 22);
            this.label_totalProfit.TabIndex = 4;
            this.label_totalProfit.Text = "          ";
            // 
            // label_totalCount
            // 
            this.label_totalCount.AutoSize = true;
            this.label_totalCount.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label_totalCount.Location = new System.Drawing.Point(81, 88);
            this.label_totalCount.Name = "label_totalCount";
            this.label_totalCount.Size = new System.Drawing.Size(61, 22);
            this.label_totalCount.TabIndex = 4;
            this.label_totalCount.Text = "          ";
            // 
            // Label_money
            // 
            this.Label_money.AutoSize = true;
            this.Label_money.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.Label_money.Location = new System.Drawing.Point(81, 66);
            this.Label_money.Name = "Label_money";
            this.Label_money.Size = new System.Drawing.Size(61, 22);
            this.Label_money.TabIndex = 4;
            this.Label_money.Text = "          ";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 69);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(54, 20);
            this.label1.TabIndex = 3;
            this.label1.Text = "예수금";
            // 
            // Button_quit
            // 
            this.Button_quit.Location = new System.Drawing.Point(14, 24);
            this.Button_quit.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.Button_quit.Name = "Button_quit";
            this.Button_quit.Size = new System.Drawing.Size(163, 27);
            this.Button_quit.TabIndex = 0;
            this.Button_quit.Text = "종료";
            this.Button_quit.UseVisualStyleBackColor = true;
            this.Button_quit.Click += new System.EventHandler(this.Button_quit_Click);
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(6, 660);
            this.progressBar.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(1019, 21);
            this.progressBar.TabIndex = 26;
            // 
            // statusStrip_Future
            // 
            this.statusStrip_Future.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.statusStrip_Future.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatus_stockInfo});
            this.statusStrip_Future.Location = new System.Drawing.Point(0, 735);
            this.statusStrip_Future.Name = "statusStrip_Future";
            this.statusStrip_Future.Size = new System.Drawing.Size(1491, 26);
            this.statusStrip_Future.TabIndex = 19;
            this.statusStrip_Future.Text = "statusStrip";
            // 
            // toolStripStatus_stockInfo
            // 
            this.toolStripStatus_stockInfo.Name = "toolStripStatus_stockInfo";
            this.toolStripStatus_stockInfo.Size = new System.Drawing.Size(74, 20);
            this.toolStripStatus_stockInfo.Text = "보조 상황";
            // 
            // timer_clock
            // 
            this.timer_clock.Tick += new System.EventHandler(this.timer_clock_Tick);
            // 
            // tabControl_futureDlg
            // 
            this.tabControl_futureDlg.Controls.Add(this.tabPage_Watching);
            this.tabControl_futureDlg.Controls.Add(this.tabPage_OrderView);
            this.tabControl_futureDlg.Controls.Add(this.tabPage_TradeRecode);
            this.tabControl_futureDlg.Controls.Add(this.tabPage_backTestSimul);
            this.tabControl_futureDlg.Controls.Add(this.tabPage_Beleve);
            this.tabControl_futureDlg.Location = new System.Drawing.Point(214, 8);
            this.tabControl_futureDlg.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabControl_futureDlg.Name = "tabControl_futureDlg";
            this.tabControl_futureDlg.SelectedIndex = 0;
            this.tabControl_futureDlg.Size = new System.Drawing.Size(1265, 721);
            this.tabControl_futureDlg.TabIndex = 0;
            this.tabControl_futureDlg.SelectedIndexChanged += new System.EventHandler(this.tabControl_futureDlg_SelectedIndexChanged);
            // 
            // tabPage_Watching
            // 
            this.tabPage_Watching.Controls.Add(this.label_timer);
            this.tabPage_Watching.Controls.Add(this.listView_OrderView);
            this.tabPage_Watching.Controls.Add(this.progressBar);
            this.tabPage_Watching.Controls.Add(this.DataGridView_FuturePool);
            this.tabPage_Watching.Controls.Add(this.listView_tradeStock);
            this.tabPage_Watching.Location = new System.Drawing.Point(4, 29);
            this.tabPage_Watching.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabPage_Watching.Name = "tabPage_Watching";
            this.tabPage_Watching.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabPage_Watching.Size = new System.Drawing.Size(1257, 688);
            this.tabPage_Watching.TabIndex = 0;
            this.tabPage_Watching.Text = "감시중인 종목들";
            this.tabPage_Watching.UseVisualStyleBackColor = true;
            // 
            // label_timer
            // 
            this.label_timer.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label_timer.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label_timer.ForeColor = System.Drawing.Color.LimeGreen;
            this.label_timer.Image = global::HappyFuture.Properties.Resources.DarkArchon;
            this.label_timer.Location = new System.Drawing.Point(1029, 531);
            this.label_timer.Name = "label_timer";
            this.label_timer.Size = new System.Drawing.Size(222, 150);
            this.label_timer.TabIndex = 22;
            this.label_timer.Text = "현재 시각";
            this.label_timer.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // listView_OrderView
            // 
            this.listView_OrderView.HideSelection = false;
            this.listView_OrderView.Location = new System.Drawing.Point(6, 535);
            this.listView_OrderView.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.listView_OrderView.Name = "listView_OrderView";
            this.listView_OrderView.Size = new System.Drawing.Size(1019, 119);
            this.listView_OrderView.TabIndex = 27;
            this.listView_OrderView.UseCompatibleStateImageBehavior = false;
            // 
            // DataGridView_FuturePool
            // 
            dataGridViewCellStyle1.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.DataGridView_FuturePool.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.DataGridView_FuturePool.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.DataGridView_FuturePool.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DataGridView_FuturePool.Location = new System.Drawing.Point(6, 9);
            this.DataGridView_FuturePool.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.DataGridView_FuturePool.Name = "DataGridView_FuturePool";
            this.DataGridView_FuturePool.ReadOnly = true;
            this.DataGridView_FuturePool.RowHeadersWidth = 62;
            this.DataGridView_FuturePool.RowTemplate.Height = 23;
            this.DataGridView_FuturePool.Size = new System.Drawing.Size(1245, 316);
            this.DataGridView_FuturePool.TabIndex = 23;
            this.DataGridView_FuturePool.CellMouseUp += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.DataGridView_FuturePool_CellMouseUp);
            // 
            // listView_tradeStock
            // 
            this.listView_tradeStock.HideSelection = false;
            this.listView_tradeStock.Location = new System.Drawing.Point(6, 334);
            this.listView_tradeStock.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.listView_tradeStock.Name = "listView_tradeStock";
            this.listView_tradeStock.Size = new System.Drawing.Size(1245, 193);
            this.listView_tradeStock.TabIndex = 15;
            this.listView_tradeStock.UseCompatibleStateImageBehavior = false;
            // 
            // tabPage_OrderView
            // 
            this.tabPage_OrderView.Controls.Add(this.button_exportExcel);
            this.tabPage_OrderView.Controls.Add(this.label6);
            this.tabPage_OrderView.Controls.Add(this.dateTimePicker_OrderEnd);
            this.tabPage_OrderView.Controls.Add(this.dateTimePicker_OrderStart);
            this.tabPage_OrderView.Controls.Add(this.DataGridView_OrderView);
            this.tabPage_OrderView.Location = new System.Drawing.Point(4, 29);
            this.tabPage_OrderView.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabPage_OrderView.Name = "tabPage_OrderView";
            this.tabPage_OrderView.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabPage_OrderView.Size = new System.Drawing.Size(1257, 688);
            this.tabPage_OrderView.TabIndex = 1;
            this.tabPage_OrderView.Text = "봇 주문 이력";
            this.tabPage_OrderView.UseVisualStyleBackColor = true;
            // 
            // button_exportExcel
            // 
            this.button_exportExcel.Location = new System.Drawing.Point(456, 6);
            this.button_exportExcel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.button_exportExcel.Name = "button_exportExcel";
            this.button_exportExcel.Size = new System.Drawing.Size(163, 24);
            this.button_exportExcel.TabIndex = 25;
            this.button_exportExcel.Text = "엑셀로 추출";
            this.button_exportExcel.UseVisualStyleBackColor = true;
            this.button_exportExcel.Click += new System.EventHandler(this.button_exportExcel_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(212, 11);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(20, 20);
            this.label6.TabIndex = 20;
            this.label6.Text = "~";
            // 
            // dateTimePicker_OrderEnd
            // 
            this.dateTimePicker_OrderEnd.Location = new System.Drawing.Point(233, 7);
            this.dateTimePicker_OrderEnd.Name = "dateTimePicker_OrderEnd";
            this.dateTimePicker_OrderEnd.Size = new System.Drawing.Size(200, 27);
            this.dateTimePicker_OrderEnd.TabIndex = 19;
            this.dateTimePicker_OrderEnd.ValueChanged += new System.EventHandler(this.dateTimePicker_OrderEnd_ValueChanged);
            // 
            // dateTimePicker_OrderStart
            // 
            this.dateTimePicker_OrderStart.Location = new System.Drawing.Point(6, 7);
            this.dateTimePicker_OrderStart.Name = "dateTimePicker_OrderStart";
            this.dateTimePicker_OrderStart.Size = new System.Drawing.Size(200, 27);
            this.dateTimePicker_OrderStart.TabIndex = 18;
            this.dateTimePicker_OrderStart.ValueChanged += new System.EventHandler(this.dateTimePicker_OrderStart_ValueChanged);
            // 
            // DataGridView_OrderView
            // 
            this.DataGridView_OrderView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DataGridView_OrderView.Location = new System.Drawing.Point(6, 36);
            this.DataGridView_OrderView.Margin = new System.Windows.Forms.Padding(2);
            this.DataGridView_OrderView.Name = "DataGridView_OrderView";
            this.DataGridView_OrderView.RowHeadersWidth = 62;
            this.DataGridView_OrderView.RowTemplate.Height = 30;
            this.DataGridView_OrderView.Size = new System.Drawing.Size(1246, 645);
            this.DataGridView_OrderView.TabIndex = 17;
            // 
            // tabPage_TradeRecode
            // 
            this.tabPage_TradeRecode.Controls.Add(this.Chart_tradeHistory);
            this.tabPage_TradeRecode.Controls.Add(this.DataGridView_tradeHistory);
            this.tabPage_TradeRecode.Controls.Add(this.label8);
            this.tabPage_TradeRecode.Controls.Add(this.dateTimePicker_MoneyEnd);
            this.tabPage_TradeRecode.Controls.Add(this.dateTimePicker_MoneyStart);
            this.tabPage_TradeRecode.Controls.Add(this.DataGridView_tradeRecoder);
            this.tabPage_TradeRecode.Location = new System.Drawing.Point(4, 29);
            this.tabPage_TradeRecode.Name = "tabPage_TradeRecode";
            this.tabPage_TradeRecode.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_TradeRecode.Size = new System.Drawing.Size(1257, 688);
            this.tabPage_TradeRecode.TabIndex = 2;
            this.tabPage_TradeRecode.Text = "HTS 거래 이력";
            this.tabPage_TradeRecode.UseVisualStyleBackColor = true;
            // 
            // Chart_tradeHistory
            // 
            legend1.Name = "Legend1";
            this.Chart_tradeHistory.Legends.Add(legend1);
            this.Chart_tradeHistory.Location = new System.Drawing.Point(7, 56);
            this.Chart_tradeHistory.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Chart_tradeHistory.Name = "Chart_tradeHistory";
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            this.Chart_tradeHistory.Series.Add(series1);
            this.Chart_tradeHistory.Size = new System.Drawing.Size(1253, 320);
            this.Chart_tradeHistory.TabIndex = 16;
            this.Chart_tradeHistory.Text = "quickChart_profit";
            // 
            // DataGridView_tradeHistory
            // 
            this.DataGridView_tradeHistory.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DataGridView_tradeHistory.Location = new System.Drawing.Point(6, 383);
            this.DataGridView_tradeHistory.Name = "DataGridView_tradeHistory";
            this.DataGridView_tradeHistory.RowHeadersWidth = 62;
            this.DataGridView_tradeHistory.RowTemplate.Height = 23;
            this.DataGridView_tradeHistory.Size = new System.Drawing.Size(612, 300);
            this.DataGridView_tradeHistory.TabIndex = 5;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(213, 12);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(20, 20);
            this.label8.TabIndex = 3;
            this.label8.Text = "~";
            // 
            // dateTimePicker_MoneyEnd
            // 
            this.dateTimePicker_MoneyEnd.Location = new System.Drawing.Point(234, 8);
            this.dateTimePicker_MoneyEnd.Name = "dateTimePicker_MoneyEnd";
            this.dateTimePicker_MoneyEnd.Size = new System.Drawing.Size(200, 27);
            this.dateTimePicker_MoneyEnd.TabIndex = 2;
            this.dateTimePicker_MoneyEnd.ValueChanged += new System.EventHandler(this.dateTimePicker_end_ValueChanged);
            // 
            // dateTimePicker_MoneyStart
            // 
            this.dateTimePicker_MoneyStart.Location = new System.Drawing.Point(7, 8);
            this.dateTimePicker_MoneyStart.Name = "dateTimePicker_MoneyStart";
            this.dateTimePicker_MoneyStart.Size = new System.Drawing.Size(200, 27);
            this.dateTimePicker_MoneyStart.TabIndex = 1;
            this.dateTimePicker_MoneyStart.ValueChanged += new System.EventHandler(this.dateTimePicker_start_ValueChanged);
            // 
            // DataGridView_tradeRecoder
            // 
            this.DataGridView_tradeRecoder.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DataGridView_tradeRecoder.Location = new System.Drawing.Point(625, 383);
            this.DataGridView_tradeRecoder.Name = "DataGridView_tradeRecoder";
            this.DataGridView_tradeRecoder.RowHeadersWidth = 62;
            this.DataGridView_tradeRecoder.RowTemplate.Height = 23;
            this.DataGridView_tradeRecoder.Size = new System.Drawing.Size(638, 300);
            this.DataGridView_tradeRecoder.TabIndex = 0;
            // 
            // tabPage_backTestSimul
            // 
            this.tabPage_backTestSimul.Controls.Add(this.button_backTestDBSimul);
            this.tabPage_backTestSimul.Controls.Add(this.button_totalEachSimul);
            this.tabPage_backTestSimul.Controls.Add(this.textBox_testAccount);
            this.tabPage_backTestSimul.Controls.Add(this.label9);
            this.tabPage_backTestSimul.Controls.Add(this.groupBox4);
            this.tabPage_backTestSimul.Controls.Add(this.label_backTestInfo);
            this.tabPage_backTestSimul.Controls.Add(this.progressBar_backTest);
            this.tabPage_backTestSimul.Controls.Add(this.button_TestCancel);
            this.tabPage_backTestSimul.Controls.Add(this.button_moduleClear);
            this.tabPage_backTestSimul.Controls.Add(this.button_backTestTotalTradeSimul);
            this.tabPage_backTestSimul.Controls.Add(this.label7);
            this.tabPage_backTestSimul.Controls.Add(this.dateTimePicker_backTestEnd);
            this.tabPage_backTestSimul.Controls.Add(this.dateTimePicker_backTestStart);
            this.tabPage_backTestSimul.Controls.Add(this.Chart_TotalPrice);
            this.tabPage_backTestSimul.Controls.Add(this.DataGridView_backTestResult);
            this.tabPage_backTestSimul.Controls.Add(this.Chart_backTestAccountResult);
            this.tabPage_backTestSimul.Location = new System.Drawing.Point(4, 29);
            this.tabPage_backTestSimul.Name = "tabPage_backTestSimul";
            this.tabPage_backTestSimul.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_backTestSimul.Size = new System.Drawing.Size(1257, 688);
            this.tabPage_backTestSimul.TabIndex = 3;
            this.tabPage_backTestSimul.Text = "시뮬레이션";
            this.tabPage_backTestSimul.UseVisualStyleBackColor = true;
            // 
            // button_backTestDBSimul
            // 
            this.button_backTestDBSimul.Location = new System.Drawing.Point(918, 11);
            this.button_backTestDBSimul.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.button_backTestDBSimul.Name = "button_backTestDBSimul";
            this.button_backTestDBSimul.Size = new System.Drawing.Size(107, 25);
            this.button_backTestDBSimul.TabIndex = 37;
            this.button_backTestDBSimul.Text = "찾은 모듈만";
            this.button_backTestDBSimul.UseVisualStyleBackColor = true;
            this.button_backTestDBSimul.Click += new System.EventHandler(this.button_backTestDBSimul_Click);
            // 
            // button_totalEachSimul
            // 
            this.button_totalEachSimul.Location = new System.Drawing.Point(1144, 11);
            this.button_totalEachSimul.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.button_totalEachSimul.Name = "button_totalEachSimul";
            this.button_totalEachSimul.Size = new System.Drawing.Size(107, 25);
            this.button_totalEachSimul.TabIndex = 36;
            this.button_totalEachSimul.Text = "각 종목 테스트";
            this.button_totalEachSimul.UseVisualStyleBackColor = true;
            this.button_totalEachSimul.Click += new System.EventHandler(this.button_totalEachSimul_Click);
            // 
            // textBox_testAccount
            // 
            this.textBox_testAccount.Location = new System.Drawing.Point(918, 41);
            this.textBox_testAccount.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.textBox_testAccount.Name = "textBox_testAccount";
            this.textBox_testAccount.Size = new System.Drawing.Size(107, 27);
            this.textBox_testAccount.TabIndex = 32;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.label9.Location = new System.Drawing.Point(862, 44);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(54, 20);
            this.label9.TabIndex = 33;
            this.label9.Text = "예수금";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.backTestLog_TextBox);
            this.groupBox4.Location = new System.Drawing.Point(6, 548);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(1245, 142);
            this.groupBox4.TabIndex = 34;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "로그창";
            // 
            // backTestLog_TextBox
            // 
            this.backTestLog_TextBox.Location = new System.Drawing.Point(6, 20);
            this.backTestLog_TextBox.Name = "backTestLog_TextBox";
            this.backTestLog_TextBox.ReadOnly = true;
            this.backTestLog_TextBox.Size = new System.Drawing.Size(1233, 108);
            this.backTestLog_TextBox.TabIndex = 32;
            this.backTestLog_TextBox.Text = "";
            // 
            // label_backTestInfo
            // 
            this.label_backTestInfo.AutoSize = true;
            this.label_backTestInfo.Location = new System.Drawing.Point(395, 11);
            this.label_backTestInfo.Name = "label_backTestInfo";
            this.label_backTestInfo.Size = new System.Drawing.Size(82, 20);
            this.label_backTestInfo.TabIndex = 30;
            this.label_backTestInfo.Text = "현재시간 : ";
            // 
            // progressBar_backTest
            // 
            this.progressBar_backTest.Location = new System.Drawing.Point(7, 35);
            this.progressBar_backTest.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.progressBar_backTest.Name = "progressBar_backTest";
            this.progressBar_backTest.Size = new System.Drawing.Size(849, 29);
            this.progressBar_backTest.TabIndex = 29;
            // 
            // button_TestCancel
            // 
            this.button_TestCancel.Location = new System.Drawing.Point(1144, 40);
            this.button_TestCancel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.button_TestCancel.Name = "button_TestCancel";
            this.button_TestCancel.Size = new System.Drawing.Size(107, 27);
            this.button_TestCancel.TabIndex = 22;
            this.button_TestCancel.Text = "테스트중지";
            this.button_TestCancel.UseVisualStyleBackColor = true;
            this.button_TestCancel.Click += new System.EventHandler(this.button_TestCancel_Click);
            // 
            // button_moduleClear
            // 
            this.button_moduleClear.Location = new System.Drawing.Point(1031, 40);
            this.button_moduleClear.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.button_moduleClear.Name = "button_moduleClear";
            this.button_moduleClear.Size = new System.Drawing.Size(107, 27);
            this.button_moduleClear.TabIndex = 22;
            this.button_moduleClear.Text = "모듈clear";
            this.button_moduleClear.UseVisualStyleBackColor = true;
            this.button_moduleClear.Click += new System.EventHandler(this.button_moduleClear_Click);
            // 
            // button_backTestTotalTradeSimul
            // 
            this.button_backTestTotalTradeSimul.Location = new System.Drawing.Point(1031, 11);
            this.button_backTestTotalTradeSimul.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.button_backTestTotalTradeSimul.Name = "button_backTestTotalTradeSimul";
            this.button_backTestTotalTradeSimul.Size = new System.Drawing.Size(107, 25);
            this.button_backTestTotalTradeSimul.TabIndex = 22;
            this.button_backTestTotalTradeSimul.Text = "종합 테스트";
            this.button_backTestTotalTradeSimul.UseVisualStyleBackColor = true;
            this.button_backTestTotalTradeSimul.Click += new System.EventHandler(this.button_totalTradeSimul_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(190, 11);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(20, 20);
            this.label7.TabIndex = 20;
            this.label7.Text = "~";
            // 
            // dateTimePicker_backTestEnd
            // 
            this.dateTimePicker_backTestEnd.Location = new System.Drawing.Point(211, 7);
            this.dateTimePicker_backTestEnd.Name = "dateTimePicker_backTestEnd";
            this.dateTimePicker_backTestEnd.Size = new System.Drawing.Size(178, 27);
            this.dateTimePicker_backTestEnd.TabIndex = 19;
            // 
            // dateTimePicker_backTestStart
            // 
            this.dateTimePicker_backTestStart.Location = new System.Drawing.Point(7, 6);
            this.dateTimePicker_backTestStart.Name = "dateTimePicker_backTestStart";
            this.dateTimePicker_backTestStart.Size = new System.Drawing.Size(177, 27);
            this.dateTimePicker_backTestStart.TabIndex = 18;
            // 
            // Chart_TotalPrice
            // 
            legend2.Name = "Legend1";
            this.Chart_TotalPrice.Legends.Add(legend2);
            this.Chart_TotalPrice.Location = new System.Drawing.Point(865, 72);
            this.Chart_TotalPrice.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Chart_TotalPrice.Name = "Chart_TotalPrice";
            series2.Legend = "Legend1";
            series2.Name = "Series1";
            this.Chart_TotalPrice.Series.Add(series2);
            this.Chart_TotalPrice.Size = new System.Drawing.Size(386, 309);
            this.Chart_TotalPrice.TabIndex = 35;
            this.Chart_TotalPrice.Text = "quickChart_profit";
            // 
            // DataGridView_backTestResult
            // 
            this.DataGridView_backTestResult.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DataGridView_backTestResult.Location = new System.Drawing.Point(7, 388);
            this.DataGridView_backTestResult.Name = "DataGridView_backTestResult";
            this.DataGridView_backTestResult.RowHeadersWidth = 62;
            this.DataGridView_backTestResult.RowTemplate.Height = 23;
            this.DataGridView_backTestResult.Size = new System.Drawing.Size(1244, 154);
            this.DataGridView_backTestResult.TabIndex = 21;
            // 
            // Chart_backTestAccountResult
            // 
            this.Chart_backTestAccountResult.BackColor = System.Drawing.Color.Transparent;
            legend3.Name = "Legend1";
            this.Chart_backTestAccountResult.Legends.Add(legend3);
            this.Chart_backTestAccountResult.Location = new System.Drawing.Point(7, 72);
            this.Chart_backTestAccountResult.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Chart_backTestAccountResult.Name = "Chart_backTestAccountResult";
            series3.Legend = "Legend1";
            series3.Name = "Series1";
            this.Chart_backTestAccountResult.Series.Add(series3);
            this.Chart_backTestAccountResult.Size = new System.Drawing.Size(823, 309);
            this.Chart_backTestAccountResult.TabIndex = 17;
            this.Chart_backTestAccountResult.Text = "quickChart_profit";
            // 
            // tabPage_Beleve
            // 
            this.tabPage_Beleve.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("tabPage_Beleve.BackgroundImage")));
            this.tabPage_Beleve.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.tabPage_Beleve.Controls.Add(this.button_resetWinRate);
            this.tabPage_Beleve.Controls.Add(this.button_reloadDB);
            this.tabPage_Beleve.Controls.Add(this.button_configReload);
            this.tabPage_Beleve.Controls.Add(this.lable_configInfo);
            this.tabPage_Beleve.Location = new System.Drawing.Point(4, 29);
            this.tabPage_Beleve.Name = "tabPage_Beleve";
            this.tabPage_Beleve.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_Beleve.Size = new System.Drawing.Size(1257, 688);
            this.tabPage_Beleve.TabIndex = 4;
            this.tabPage_Beleve.Text = "믿음";
            this.tabPage_Beleve.UseVisualStyleBackColor = true;
            // 
            // button_resetWinRate
            // 
            this.button_resetWinRate.Location = new System.Drawing.Point(1092, 54);
            this.button_resetWinRate.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.button_resetWinRate.Name = "button_resetWinRate";
            this.button_resetWinRate.Size = new System.Drawing.Size(133, 24);
            this.button_resetWinRate.TabIndex = 25;
            this.button_resetWinRate.Text = "승율 초기화";
            this.button_resetWinRate.UseVisualStyleBackColor = true;
            this.button_resetWinRate.Click += new System.EventHandler(this.Button_resetWinRate_Click);
            // 
            // button_reloadDB
            // 
            this.button_reloadDB.Location = new System.Drawing.Point(1092, 17);
            this.button_reloadDB.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.button_reloadDB.Name = "button_reloadDB";
            this.button_reloadDB.Size = new System.Drawing.Size(133, 24);
            this.button_reloadDB.TabIndex = 25;
            this.button_reloadDB.Text = "모듈 db 리로드";
            this.button_reloadDB.UseVisualStyleBackColor = true;
            this.button_reloadDB.Click += new System.EventHandler(this.Button_reloadDB_Click);
            // 
            // button_configReload
            // 
            this.button_configReload.Location = new System.Drawing.Point(933, 17);
            this.button_configReload.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.button_configReload.Name = "button_configReload";
            this.button_configReload.Size = new System.Drawing.Size(137, 24);
            this.button_configReload.TabIndex = 25;
            this.button_configReload.Text = "설정 리로드";
            this.button_configReload.UseVisualStyleBackColor = true;
            this.button_configReload.Click += new System.EventHandler(this.Button_configReload_Click);
            // 
            // lable_configInfo
            // 
            this.lable_configInfo.AutoSize = true;
            this.lable_configInfo.BackColor = System.Drawing.Color.Transparent;
            this.lable_configInfo.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lable_configInfo.ForeColor = System.Drawing.Color.DeepSkyBlue;
            this.lable_configInfo.Location = new System.Drawing.Point(24, 26);
            this.lable_configInfo.Name = "lable_configInfo";
            this.lable_configInfo.Size = new System.Drawing.Size(84, 23);
            this.lable_configInfo.TabIndex = 25;
            this.lable_configInfo.Text = "설정 정보";
            // 
            // progressBar_engine
            // 
            this.progressBar_engine.Location = new System.Drawing.Point(797, 739);
            this.progressBar_engine.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.progressBar_engine.Name = "progressBar_engine";
            this.progressBar_engine.Size = new System.Drawing.Size(682, 22);
            this.progressBar_engine.TabIndex = 28;
            // 
            // FutureDlg
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1491, 761);
            this.ControlBox = false;
            this.Controls.Add(this.progressBar_engine);
            this.Controls.Add(this.Group3);
            this.Controls.Add(this.statusStrip_Future);
            this.Controls.Add(this.tabControl_futureDlg);
            this.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FutureDlg";
            this.Text = "FutureDlg";
            this.Shown += new System.EventHandler(this.FutureDlg_Shown);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.FutureDlg_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.FutureDlg_DragEnter);
            this.Group3.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.statusStrip_Future.ResumeLayout(false);
            this.statusStrip_Future.PerformLayout();
            this.tabControl_futureDlg.ResumeLayout(false);
            this.tabPage_Watching.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.DataGridView_FuturePool)).EndInit();
            this.tabPage_OrderView.ResumeLayout(false);
            this.tabPage_OrderView.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DataGridView_OrderView)).EndInit();
            this.tabPage_TradeRecode.ResumeLayout(false);
            this.tabPage_TradeRecode.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Chart_tradeHistory)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.DataGridView_tradeHistory)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.DataGridView_tradeRecoder)).EndInit();
            this.tabPage_backTestSimul.ResumeLayout(false);
            this.tabPage_backTestSimul.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.Chart_TotalPrice)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.DataGridView_backTestResult)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Chart_backTestAccountResult)).EndInit();
            this.tabPage_Beleve.ResumeLayout(false);
            this.tabPage_Beleve.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.GroupBox Group3;
        private System.Windows.Forms.GroupBox groupBox3;
        internal System.Windows.Forms.Button button_manual_sell;
        private System.Windows.Forms.GroupBox groupBox1;
        internal System.Windows.Forms.Button button_reload;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        internal System.Windows.Forms.StatusStrip statusStrip_Future;
        internal System.Windows.Forms.ToolStripStatusLabel toolStripStatus_stockInfo;
        private System.Windows.Forms.Timer timer_clock;
        private System.Windows.Forms.Label label_timer;
        internal System.Windows.Forms.ListView listView_tradeStock;
        private System.Windows.Forms.TabPage tabPage_OrderView;
        internal System.Windows.Forms.Label Label_id;
        internal System.Windows.Forms.Label Label_money;
        internal System.Windows.Forms.Button Button_quit;
        internal System.Windows.Forms.TabControl tabControl_futureDlg;
        internal System.Windows.Forms.TabPage tabPage_Watching;
        internal UtilLibrary.QuickDataGridView DataGridView_FuturePool;
        internal UtilLibrary.QuickDataGridView DataGridView_OrderView;
        internal System.Windows.Forms.ProgressBar progressBar;
        internal System.Windows.Forms.Label label4;
        private System.Windows.Forms.TabPage tabPage_TradeRecode;
        internal System.Windows.Forms.Button button_findFutureModule;
        internal UtilLibrary.QuickDataGridView DataGridView_tradeRecoder;
        private System.Windows.Forms.Label label8;
        internal System.Windows.Forms.DateTimePicker dateTimePicker_MoneyEnd;
        internal System.Windows.Forms.DateTimePicker dateTimePicker_MoneyStart;
        internal QuickChart Chart_tradeHistory;
        internal QuickDataGridView DataGridView_tradeHistory;
        internal System.Windows.Forms.Label label_nextUpdate;
        internal System.Windows.Forms.Label label12;
        internal System.Windows.Forms.Label label11;
        internal System.Windows.Forms.Label label_totalPureProfit;
        internal System.Windows.Forms.Label label_totalProfit;
        internal System.Windows.Forms.Label label_totalCount;
        private System.Windows.Forms.Label label6;
        internal System.Windows.Forms.DateTimePicker dateTimePicker_OrderEnd;
        internal System.Windows.Forms.DateTimePicker dateTimePicker_OrderStart;
        internal System.Windows.Forms.ListView listView_OrderView;
        internal System.Windows.Forms.Label label_cpuInfo;
        internal System.Windows.Forms.Label label_memInfo;
        private System.Windows.Forms.GroupBox groupBox2;
        internal System.Windows.Forms.Label label_serachItem;
        internal System.Windows.Forms.Button button_exportExcel;
        internal System.Windows.Forms.ProgressBar progressBar_engine;
        private System.Windows.Forms.TabPage tabPage_backTestSimul;
        internal QuickDataGridView DataGridView_backTestResult;
        private System.Windows.Forms.Label label7;
        internal System.Windows.Forms.DateTimePicker dateTimePicker_backTestEnd;
        internal System.Windows.Forms.DateTimePicker dateTimePicker_backTestStart;
        internal QuickChart Chart_backTestAccountResult;
        internal System.Windows.Forms.Button button_backTestTotalTradeSimul;
        internal System.Windows.Forms.ProgressBar progressBar_backTest;
        internal System.Windows.Forms.Label label_backTestInfo;
        private System.Windows.Forms.TabPage tabPage_Beleve;
        private System.Windows.Forms.GroupBox groupBox4;
        internal System.Windows.Forms.Label lable_configInfo;
        private System.Windows.Forms.PictureBox pictureBox1;
        internal System.Windows.Forms.Button button_TestCancel;
        internal System.Windows.Forms.Button button_allPayOff;
        internal System.Windows.Forms.CheckBox checkBox_doTrade;
        internal System.Windows.Forms.TextBox textBox_testAccount;
        internal System.Windows.Forms.Label label9;
        internal QuickChart Chart_TotalPrice;
        internal System.Windows.Forms.Button button_moduleClear;
        internal System.Windows.Forms.Button button_configReload;
        internal System.Windows.Forms.Button button_reloadDB;
        internal System.Windows.Forms.CheckBox checkBox_reverse;
        internal System.Windows.Forms.Button button_resetWinRate;
        internal System.Windows.Forms.Label Label_account;
        internal System.Windows.Forms.ComboBox comboBox_priceMin;
        internal System.Windows.Forms.RichTextBox backTestLog_TextBox;
        internal System.Windows.Forms.Button button_totalEachSimul;
        internal System.Windows.Forms.Button button_backTestDBSimul;
    }
}