using HappyTool.Util;
using UtilLibrary;

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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend2 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StockDlg));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.button_reload = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.Label_account = new System.Windows.Forms.Label();
            this.Label_id = new System.Windows.Forms.Label();
            this.Label_money = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.Button_quit = new System.Windows.Forms.Button();
            this.richTextBox_SimulateResult = new System.Windows.Forms.RichTextBox();
            this.tabControl_stockDlg = new System.Windows.Forms.TabControl();
            this.tabPage_Watching = new System.Windows.Forms.TabPage();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.listView_orderStocks = new System.Windows.Forms.ListView();
            this.DataGridView_StockPool = new UtilLibrary.QuickDataGridView();
            this.tabPage_hts = new System.Windows.Forms.TabPage();
            this.Chart_tradeHistory = new UtilLibrary.QuickChart();
            this.DataGridView_tradeHistory = new UtilLibrary.QuickDataGridView();
            this.label8 = new System.Windows.Forms.Label();
            this.dateTimePicker_MoneyEnd = new System.Windows.Forms.DateTimePicker();
            this.dateTimePicker_MoneyStart = new System.Windows.Forms.DateTimePicker();
            this.DataGridView_tradeRecoder = new UtilLibrary.QuickDataGridView();
            this.tabPage_Simulation = new System.Windows.Forms.TabPage();
            this.DataGridView_history = new UtilLibrary.QuickDataGridView();
            this.quickChart_SimulateProfit = new UtilLibrary.QuickChart();
            this.Group3 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label_nextUpdate = new System.Windows.Forms.Label();
            this.button_force_trade = new System.Windows.Forms.Button();
            this.button_manual_all_sell = new System.Windows.Forms.Button();
            this.button_manual_sell = new System.Windows.Forms.Button();
            this.statusStrip_Stock = new System.Windows.Forms.StatusStrip();
            this.toolStripStatus_stockInfo = new System.Windows.Forms.ToolStripStatusLabel();
            this.timer_clock = new System.Windows.Forms.Timer(this.components);
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label_memInfo = new System.Windows.Forms.Label();
            this.label_cpuInfo = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label_timer = new System.Windows.Forms.Label();
            this.tabPage_belive = new System.Windows.Forms.TabPage();
            this.lable_configInfo = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.tabControl_stockDlg.SuspendLayout();
            this.tabPage_Watching.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DataGridView_StockPool)).BeginInit();
            this.tabPage_hts.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Chart_tradeHistory)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.DataGridView_tradeHistory)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.DataGridView_tradeRecoder)).BeginInit();
            this.tabPage_Simulation.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DataGridView_history)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.quickChart_SimulateProfit)).BeginInit();
            this.Group3.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.statusStrip_Stock.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.tabPage_belive.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.button_reload);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.Label_account);
            this.groupBox1.Controls.Add(this.Label_id);
            this.groupBox1.Controls.Add(this.Label_money);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(6, 58);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.groupBox1.Size = new System.Drawing.Size(180, 154);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Account";
            // 
            // button_reload
            // 
            this.button_reload.Location = new System.Drawing.Point(8, 117);
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
            this.label3.Location = new System.Drawing.Point(16, 27);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(24, 20);
            this.label3.TabIndex = 8;
            this.label3.Text = "ID";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 54);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(74, 20);
            this.label2.TabIndex = 7;
            this.label2.Text = "계좌 번호";
            // 
            // Label_account
            // 
            this.Label_account.AutoSize = true;
            this.Label_account.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.Label_account.Location = new System.Drawing.Point(98, 54);
            this.Label_account.Name = "Label_account";
            this.Label_account.Size = new System.Drawing.Size(61, 22);
            this.Label_account.TabIndex = 6;
            this.Label_account.Text = "          ";
            // 
            // Label_id
            // 
            this.Label_id.AutoSize = true;
            this.Label_id.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.Label_id.Location = new System.Drawing.Point(98, 22);
            this.Label_id.Name = "Label_id";
            this.Label_id.Size = new System.Drawing.Size(61, 22);
            this.Label_id.TabIndex = 5;
            this.Label_id.Text = "          ";
            // 
            // Label_money
            // 
            this.Label_money.AutoSize = true;
            this.Label_money.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.Label_money.Location = new System.Drawing.Point(98, 84);
            this.Label_money.Name = "Label_money";
            this.Label_money.Size = new System.Drawing.Size(61, 22);
            this.Label_money.TabIndex = 4;
            this.Label_money.Text = "          ";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 84);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 20);
            this.label1.TabIndex = 3;
            this.label1.Text = "계좌 가용액";
            // 
            // Button_quit
            // 
            this.Button_quit.Location = new System.Drawing.Point(14, 24);
            this.Button_quit.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.Button_quit.Name = "Button_quit";
            this.Button_quit.Size = new System.Drawing.Size(163, 24);
            this.Button_quit.TabIndex = 2;
            this.Button_quit.Text = "종료";
            this.Button_quit.UseVisualStyleBackColor = true;
            this.Button_quit.Click += new System.EventHandler(this.Button_quit_Click);
            // 
            // richTextBox_SimulateResult
            // 
            this.richTextBox_SimulateResult.Location = new System.Drawing.Point(6, 493);
            this.richTextBox_SimulateResult.Name = "richTextBox_SimulateResult";
            this.richTextBox_SimulateResult.Size = new System.Drawing.Size(1107, 107);
            this.richTextBox_SimulateResult.TabIndex = 13;
            this.richTextBox_SimulateResult.Text = "";
            // 
            // tabControl_stockDlg
            // 
            this.tabControl_stockDlg.Controls.Add(this.tabPage_Watching);
            this.tabControl_stockDlg.Controls.Add(this.tabPage_hts);
            this.tabControl_stockDlg.Controls.Add(this.tabPage_Simulation);
            this.tabControl_stockDlg.Controls.Add(this.tabPage_belive);
            this.tabControl_stockDlg.Location = new System.Drawing.Point(212, 12);
            this.tabControl_stockDlg.Name = "tabControl_stockDlg";
            this.tabControl_stockDlg.SelectedIndex = 0;
            this.tabControl_stockDlg.Size = new System.Drawing.Size(1095, 637);
            this.tabControl_stockDlg.TabIndex = 14;
            this.tabControl_stockDlg.SelectedIndexChanged += new System.EventHandler(this.tabControl_stockDlg_SelectedIndexChanged);
            // 
            // tabPage_Watching
            // 
            this.tabPage_Watching.Controls.Add(this.label_timer);
            this.tabPage_Watching.Controls.Add(this.progressBar);
            this.tabPage_Watching.Controls.Add(this.listView_orderStocks);
            this.tabPage_Watching.Controls.Add(this.DataGridView_StockPool);
            this.tabPage_Watching.Location = new System.Drawing.Point(4, 29);
            this.tabPage_Watching.Name = "tabPage_Watching";
            this.tabPage_Watching.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_Watching.Size = new System.Drawing.Size(1087, 604);
            this.tabPage_Watching.TabIndex = 0;
            this.tabPage_Watching.Text = "감시중인 종목들";
            this.tabPage_Watching.UseVisualStyleBackColor = true;
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(6, 576);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(837, 27);
            this.progressBar.TabIndex = 23;
            // 
            // listView_tradeStock
            // 
            this.listView_orderStocks.HideSelection = false;
            this.listView_orderStocks.Location = new System.Drawing.Point(6, 413);
            this.listView_orderStocks.Name = "listView_tradeStock";
            this.listView_orderStocks.Size = new System.Drawing.Size(837, 160);
            this.listView_orderStocks.TabIndex = 15;
            this.listView_orderStocks.UseCompatibleStateImageBehavior = false;
            // 
            // DataGridView_StockPool
            // 
            dataGridViewCellStyle1.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.DataGridView_StockPool.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.DataGridView_StockPool.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.DataGridView_StockPool.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DataGridView_StockPool.Location = new System.Drawing.Point(6, 9);
            this.DataGridView_StockPool.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.DataGridView_StockPool.Name = "DataGridView_StockPool";
            this.DataGridView_StockPool.ReadOnly = true;
            this.DataGridView_StockPool.RowHeadersWidth = 62;
            this.DataGridView_StockPool.RowTemplate.Height = 23;
            this.DataGridView_StockPool.Size = new System.Drawing.Size(1072, 396);
            this.DataGridView_StockPool.TabIndex = 4;
            this.DataGridView_StockPool.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridView_StockPool_CellContentClick);
            // 
            // tabPage_hts
            // 
            this.tabPage_hts.Controls.Add(this.Chart_tradeHistory);
            this.tabPage_hts.Controls.Add(this.DataGridView_tradeHistory);
            this.tabPage_hts.Controls.Add(this.label8);
            this.tabPage_hts.Controls.Add(this.dateTimePicker_MoneyEnd);
            this.tabPage_hts.Controls.Add(this.dateTimePicker_MoneyStart);
            this.tabPage_hts.Controls.Add(this.DataGridView_tradeRecoder);
            this.tabPage_hts.Location = new System.Drawing.Point(4, 29);
            this.tabPage_hts.Name = "tabPage_hts";
            this.tabPage_hts.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_hts.Size = new System.Drawing.Size(1087, 604);
            this.tabPage_hts.TabIndex = 3;
            this.tabPage_hts.Text = "HTS 거래 이력";
            this.tabPage_hts.UseVisualStyleBackColor = true;
            // 
            // Chart_tradeHistory
            // 
            legend1.Name = "Legend1";
            this.Chart_tradeHistory.Legends.Add(legend1);
            this.Chart_tradeHistory.Location = new System.Drawing.Point(6, 35);
            this.Chart_tradeHistory.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Chart_tradeHistory.Name = "Chart_tradeHistory";
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            this.Chart_tradeHistory.Series.Add(series1);
            this.Chart_tradeHistory.Size = new System.Drawing.Size(1107, 338);
            this.Chart_tradeHistory.TabIndex = 22;
            this.Chart_tradeHistory.Text = "quickChart_profit";
            // 
            // DataGridView_tradeHistory
            // 
            this.DataGridView_tradeHistory.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DataGridView_tradeHistory.Location = new System.Drawing.Point(6, 386);
            this.DataGridView_tradeHistory.Name = "DataGridView_tradeHistory";
            this.DataGridView_tradeHistory.RowHeadersWidth = 62;
            this.DataGridView_tradeHistory.RowTemplate.Height = 23;
            this.DataGridView_tradeHistory.Size = new System.Drawing.Size(459, 204);
            this.DataGridView_tradeHistory.TabIndex = 21;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(212, 9);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(20, 20);
            this.label8.TabIndex = 20;
            this.label8.Text = "~";
            // 
            // dateTimePicker_MoneyEnd
            // 
            this.dateTimePicker_MoneyEnd.Location = new System.Drawing.Point(233, 5);
            this.dateTimePicker_MoneyEnd.Name = "dateTimePicker_MoneyEnd";
            this.dateTimePicker_MoneyEnd.Size = new System.Drawing.Size(200, 27);
            this.dateTimePicker_MoneyEnd.TabIndex = 19;
            // 
            // dateTimePicker_MoneyStart
            // 
            this.dateTimePicker_MoneyStart.Location = new System.Drawing.Point(6, 5);
            this.dateTimePicker_MoneyStart.Name = "dateTimePicker_MoneyStart";
            this.dateTimePicker_MoneyStart.Size = new System.Drawing.Size(200, 27);
            this.dateTimePicker_MoneyStart.TabIndex = 18;
            // 
            // DataGridView_tradeRecoder
            // 
            this.DataGridView_tradeRecoder.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DataGridView_tradeRecoder.Location = new System.Drawing.Point(471, 386);
            this.DataGridView_tradeRecoder.Name = "DataGridView_tradeRecoder";
            this.DataGridView_tradeRecoder.RowHeadersWidth = 62;
            this.DataGridView_tradeRecoder.RowTemplate.Height = 23;
            this.DataGridView_tradeRecoder.Size = new System.Drawing.Size(642, 204);
            this.DataGridView_tradeRecoder.TabIndex = 17;
            // 
            // tabPage_Simulation
            // 
            this.tabPage_Simulation.Controls.Add(this.DataGridView_history);
            this.tabPage_Simulation.Controls.Add(this.quickChart_SimulateProfit);
            this.tabPage_Simulation.Controls.Add(this.richTextBox_SimulateResult);
            this.tabPage_Simulation.Location = new System.Drawing.Point(4, 29);
            this.tabPage_Simulation.Name = "tabPage_Simulation";
            this.tabPage_Simulation.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_Simulation.Size = new System.Drawing.Size(1087, 604);
            this.tabPage_Simulation.TabIndex = 1;
            this.tabPage_Simulation.Text = "시뮬레이션";
            this.tabPage_Simulation.UseVisualStyleBackColor = true;
            // 
            // DataGridView_history
            // 
            this.DataGridView_history.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DataGridView_history.Location = new System.Drawing.Point(6, 329);
            this.DataGridView_history.Margin = new System.Windows.Forms.Padding(2);
            this.DataGridView_history.Name = "DataGridView_history";
            this.DataGridView_history.RowHeadersWidth = 62;
            this.DataGridView_history.RowTemplate.Height = 30;
            this.DataGridView_history.Size = new System.Drawing.Size(1107, 159);
            this.DataGridView_history.TabIndex = 14;
            this.DataGridView_history.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridView_history_CellContentClick);
            // 
            // quickChart_SimulateProfit
            // 
            chartArea1.Name = "ChartArea1";
            this.quickChart_SimulateProfit.ChartAreas.Add(chartArea1);
            legend2.Name = "Legend1";
            this.quickChart_SimulateProfit.Legends.Add(legend2);
            this.quickChart_SimulateProfit.Location = new System.Drawing.Point(6, 8);
            this.quickChart_SimulateProfit.Name = "quickChart_SimulateProfit";
            series2.ChartArea = "ChartArea1";
            series2.Legend = "Legend1";
            series2.Name = "Series1";
            this.quickChart_SimulateProfit.Series.Add(series2);
            this.quickChart_SimulateProfit.Size = new System.Drawing.Size(1107, 316);
            this.quickChart_SimulateProfit.TabIndex = 0;
            this.quickChart_SimulateProfit.Text = "quickChart_profit";
            // 
            // Group3
            // 
            this.Group3.Controls.Add(this.groupBox2);
            this.Group3.Controls.Add(this.groupBox3);
            this.Group3.Controls.Add(this.groupBox1);
            this.Group3.Controls.Add(this.Button_quit);
            this.Group3.Location = new System.Drawing.Point(12, 15);
            this.Group3.Name = "Group3";
            this.Group3.Size = new System.Drawing.Size(194, 627);
            this.Group3.TabIndex = 15;
            this.Group3.TabStop = false;
            this.Group3.Text = "컨트롤창";
            // 
            // groupBox3
            // 
            this.groupBox3.BackColor = System.Drawing.Color.Black;
            this.groupBox3.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.groupBox3.Controls.Add(this.pictureBox1);
            this.groupBox3.Controls.Add(this.label_nextUpdate);
            this.groupBox3.Controls.Add(this.button_force_trade);
            this.groupBox3.Controls.Add(this.button_manual_all_sell);
            this.groupBox3.Controls.Add(this.button_manual_sell);
            this.groupBox3.Location = new System.Drawing.Point(8, 220);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(180, 295);
            this.groupBox3.TabIndex = 10;
            this.groupBox3.TabStop = false;
            // 
            // label_nextUpdate
            // 
            this.label_nextUpdate.AutoSize = true;
            this.label_nextUpdate.BackColor = System.Drawing.Color.Transparent;
            this.label_nextUpdate.ForeColor = System.Drawing.Color.Lime;
            this.label_nextUpdate.Location = new System.Drawing.Point(6, 19);
            this.label_nextUpdate.Name = "label_nextUpdate";
            this.label_nextUpdate.Size = new System.Drawing.Size(109, 20);
            this.label_nextUpdate.TabIndex = 8;
            this.label_nextUpdate.Text = "다음 분봉 까지";
            // 
            // button_force_trade
            // 
            this.button_force_trade.Location = new System.Drawing.Point(8, 48);
            this.button_force_trade.Name = "button_force_trade";
            this.button_force_trade.Size = new System.Drawing.Size(162, 26);
            this.button_force_trade.TabIndex = 1;
            this.button_force_trade.Text = "강제 자동 매매";
            this.button_force_trade.UseVisualStyleBackColor = true;
            this.button_force_trade.Click += new System.EventHandler(this.button_force_trade_Click);
            // 
            // button_manual_all_sell
            // 
            this.button_manual_all_sell.Location = new System.Drawing.Point(8, 84);
            this.button_manual_all_sell.Name = "button_manual_all_sell";
            this.button_manual_all_sell.Size = new System.Drawing.Size(162, 28);
            this.button_manual_all_sell.TabIndex = 2;
            this.button_manual_all_sell.Text = "모두 청산";
            this.button_manual_all_sell.UseVisualStyleBackColor = true;
            this.button_manual_all_sell.Click += new System.EventHandler(this.button_menual_all_sell_Click미미);
            // 
            // button_manual_sell
            // 
            this.button_manual_sell.Location = new System.Drawing.Point(8, 123);
            this.button_manual_sell.Name = "button_manual_sell";
            this.button_manual_sell.Size = new System.Drawing.Size(162, 28);
            this.button_manual_sell.TabIndex = 3;
            this.button_manual_sell.Text = "수동 청산";
            this.button_manual_sell.UseVisualStyleBackColor = true;
            this.button_manual_sell.Click += new System.EventHandler(this.button_menual_sell_Click);
            // 
            // statusStrip_Stock
            // 
            this.statusStrip_Stock.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.statusStrip_Stock.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatus_stockInfo});
            this.statusStrip_Stock.Location = new System.Drawing.Point(0, 652);
            this.statusStrip_Stock.Name = "statusStrip_Stock";
            this.statusStrip_Stock.Size = new System.Drawing.Size(1306, 26);
            this.statusStrip_Stock.TabIndex = 16;
            this.statusStrip_Stock.Text = "statusStrip";
            // 
            // toolStripStatus_stockInfo
            // 
            this.toolStripStatus_stockInfo.Name = "toolStripStatus_stockInfo";
            this.toolStripStatus_stockInfo.Size = new System.Drawing.Size(152, 20);
            this.toolStripStatus_stockInfo.Text = "toolStripStatusLabel1";
            // 
            // timer_clock
            // 
            this.timer_clock.Tick += new System.EventHandler(this.timer_clock_Tick);
            // 
            // groupBox2
            // 
            this.groupBox2.BackColor = System.Drawing.Color.Black;
            this.groupBox2.Controls.Add(this.label_memInfo);
            this.groupBox2.Controls.Add(this.label_cpuInfo);
            this.groupBox2.ForeColor = System.Drawing.Color.Lime;
            this.groupBox2.Location = new System.Drawing.Point(8, 521);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(180, 88);
            this.groupBox2.TabIndex = 18;
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
            // pictureBox1
            // 
            this.pictureBox1.Image = global::HappyTool.Properties.Resources.mario1;
            this.pictureBox1.Location = new System.Drawing.Point(9, 163);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(161, 116);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 9;
            this.pictureBox1.TabStop = false;
            // 
            // label_timer
            // 
            this.label_timer.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label_timer.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.label_timer.Font = new System.Drawing.Font("맑은 고딕", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label_timer.ForeColor = System.Drawing.Color.Lime;
            this.label_timer.Image = global::HappyTool.Properties.Resources.DropShip;
            this.label_timer.Location = new System.Drawing.Point(853, 411);
            this.label_timer.Name = "label_timer";
            this.label_timer.Size = new System.Drawing.Size(225, 190);
            this.label_timer.TabIndex = 22;
            this.label_timer.Text = "현재 시각";
            this.label_timer.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tabPage_belive
            // 
            this.tabPage_belive.BackgroundImage = global::HappyTool.Properties.Resources._2018101724563044;
            this.tabPage_belive.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.tabPage_belive.Controls.Add(this.lable_configInfo);
            this.tabPage_belive.Location = new System.Drawing.Point(4, 29);
            this.tabPage_belive.Name = "tabPage_belive";
            this.tabPage_belive.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_belive.Size = new System.Drawing.Size(1087, 604);
            this.tabPage_belive.TabIndex = 2;
            this.tabPage_belive.Text = "옵션";
            this.tabPage_belive.UseVisualStyleBackColor = true;
            // 
            // lable_configInfo
            // 
            this.lable_configInfo.AutoSize = true;
            this.lable_configInfo.BackColor = System.Drawing.Color.Transparent;
            this.lable_configInfo.Font = new System.Drawing.Font("맑은 고딕", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lable_configInfo.ForeColor = System.Drawing.Color.Gold;
            this.lable_configInfo.Location = new System.Drawing.Point(20, 24);
            this.lable_configInfo.Name = "lable_configInfo";
            this.lable_configInfo.Size = new System.Drawing.Size(95, 25);
            this.lable_configInfo.TabIndex = 26;
            this.lable_configInfo.Text = "설정 정보";
            // 
            // StockDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(1306, 678);
            this.ControlBox = false;
            this.Controls.Add(this.statusStrip_Stock);
            this.Controls.Add(this.Group3);
            this.Controls.Add(this.tabControl_stockDlg);
            this.Font = new System.Drawing.Font("맑은 고딕", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "StockDlg";
            this.Text = "주식 봇 for 키움증권 ";
            this.Shown += new System.EventHandler(this.StockDlg_Shown);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tabControl_stockDlg.ResumeLayout(false);
            this.tabPage_Watching.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.DataGridView_StockPool)).EndInit();
            this.tabPage_hts.ResumeLayout(false);
            this.tabPage_hts.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Chart_tradeHistory)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.DataGridView_tradeHistory)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.DataGridView_tradeRecoder)).EndInit();
            this.tabPage_Simulation.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.DataGridView_history)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.quickChart_SimulateProfit)).EndInit();
            this.Group3.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.statusStrip_Stock.ResumeLayout(false);
            this.statusStrip_Stock.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.tabPage_belive.ResumeLayout(false);
            this.tabPage_belive.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

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
        internal System.Windows.Forms.RichTextBox richTextBox_SimulateResult;
        private System.Windows.Forms.TabControl tabControl_stockDlg;
        private System.Windows.Forms.TabPage tabPage_Watching;
        private System.Windows.Forms.TabPage tabPage_Simulation;
        private System.Windows.Forms.GroupBox Group3;
        internal QuickChart quickChart_SimulateProfit;
        internal QuickDataGridView DataGridView_history;
        private System.Windows.Forms.GroupBox groupBox3;
        internal System.Windows.Forms.Button button_manual_all_sell;
        internal System.Windows.Forms.Button button_manual_sell;
        internal System.Windows.Forms.StatusStrip statusStrip_Stock;
        internal System.Windows.Forms.Button button_reload;
        internal System.Windows.Forms.ListView listView_orderStocks;
        internal System.Windows.Forms.Button button_force_trade;
        internal System.Windows.Forms.ToolStripStatusLabel toolStripStatus_stockInfo;
        private System.Windows.Forms.Label label_timer;
        private System.Windows.Forms.Timer timer_clock;
        internal System.Windows.Forms.ProgressBar progressBar;
        internal System.Windows.Forms.Label label_nextUpdate;
        private System.Windows.Forms.TabPage tabPage_belive;
        private System.Windows.Forms.TabPage tabPage_hts;
        private System.Windows.Forms.PictureBox pictureBox1;
        internal QuickChart Chart_tradeHistory;
        internal QuickDataGridView DataGridView_tradeHistory;
        private System.Windows.Forms.Label label8;
        internal System.Windows.Forms.DateTimePicker dateTimePicker_MoneyEnd;
        internal System.Windows.Forms.DateTimePicker dateTimePicker_MoneyStart;
        internal QuickDataGridView DataGridView_tradeRecoder;
        internal System.Windows.Forms.Label lable_configInfo;
        private System.Windows.Forms.GroupBox groupBox2;
        internal System.Windows.Forms.Label label_memInfo;
        internal System.Windows.Forms.Label label_cpuInfo;
    }
}