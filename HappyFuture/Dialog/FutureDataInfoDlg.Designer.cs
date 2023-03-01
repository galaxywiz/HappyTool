namespace HappyFuture.Dialog
{
    partial class FutureDataInfoDlg
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
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Legend legend2 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FutureDataInfoDlg));
            this.tabControl_future = new System.Windows.Forms.TabControl();
            this.tabPage_chart = new System.Windows.Forms.TabPage();
            this.label_chartInfo = new System.Windows.Forms.Label();
            this.comboBox_priceType = new System.Windows.Forms.ComboBox();
            this.Chart_Future = new UtilLibrary.QuickChart();
            this.tabPage_recode = new System.Windows.Forms.TabPage();
            this.label_moduleName = new System.Windows.Forms.Label();
            this.listView_Recoder = new System.Windows.Forms.ListView();
            this.Chart_BackTest = new UtilLibrary.QuickChart();
            this.DataGridView_BackTest = new UtilLibrary.QuickDataGridView();
            this.tabPage_eval = new System.Windows.Forms.TabPage();
            this.DataGridView_FutureInfo = new UtilLibrary.QuickDataGridView();
            this.tabControl_future.SuspendLayout();
            this.tabPage_chart.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Chart_Future)).BeginInit();
            this.tabPage_recode.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Chart_BackTest)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.DataGridView_BackTest)).BeginInit();
            this.tabPage_eval.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DataGridView_FutureInfo)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl_future
            // 
            this.tabControl_future.Controls.Add(this.tabPage_chart);
            this.tabControl_future.Controls.Add(this.tabPage_recode);
            this.tabControl_future.Controls.Add(this.tabPage_eval);
            this.tabControl_future.Location = new System.Drawing.Point(12, 12);
            this.tabControl_future.Name = "tabControl_future";
            this.tabControl_future.SelectedIndex = 0;
            this.tabControl_future.Size = new System.Drawing.Size(1336, 709);
            this.tabControl_future.TabIndex = 12;
            this.tabControl_future.SelectedIndexChanged += new System.EventHandler(this.tabControl_future_SelectedIndexChanged);
            // 
            // tabPage_chart
            // 
            this.tabPage_chart.Controls.Add(this.label_chartInfo);
            this.tabPage_chart.Controls.Add(this.comboBox_priceType);
            this.tabPage_chart.Controls.Add(this.Chart_Future);
            this.tabPage_chart.Location = new System.Drawing.Point(4, 22);
            this.tabPage_chart.Name = "tabPage_chart";
            this.tabPage_chart.Padding = new System.Windows.Forms.Padding(3, 3, 3, 3);
            this.tabPage_chart.Size = new System.Drawing.Size(1328, 683);
            this.tabPage_chart.TabIndex = 0;
            this.tabPage_chart.Text = "차트";
            this.tabPage_chart.UseVisualStyleBackColor = true;
            // 
            // label_chartInfo
            // 
            this.label_chartInfo.AutoSize = true;
            this.label_chartInfo.Location = new System.Drawing.Point(11, 39);
            this.label_chartInfo.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_chartInfo.Name = "label_chartInfo";
            this.label_chartInfo.Size = new System.Drawing.Size(0, 12);
            this.label_chartInfo.TabIndex = 4;
            // 
            // comboBox_priceType
            // 
            this.comboBox_priceType.FormattingEnabled = true;
            this.comboBox_priceType.Location = new System.Drawing.Point(6, 5);
            this.comboBox_priceType.Name = "comboBox_priceType";
            this.comboBox_priceType.Size = new System.Drawing.Size(192, 20);
            this.comboBox_priceType.TabIndex = 3;
            this.comboBox_priceType.SelectedIndexChanged += new System.EventHandler(this.comboBox_priceType_SelectedIndexChanged);
            // 
            // Chart_Future
            // 
            legend1.Alignment = System.Drawing.StringAlignment.Center;
            legend1.Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Top;
            legend1.Name = "Legend1";
            this.Chart_Future.Legends.Add(legend1);
            this.Chart_Future.Location = new System.Drawing.Point(202, 7);
            this.Chart_Future.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Chart_Future.Name = "Chart_Future";
            this.Chart_Future.Size = new System.Drawing.Size(1111, 675);
            this.Chart_Future.TabIndex = 2;
            this.Chart_Future.Text = "stock chart";
            this.Chart_Future.AxisViewChanged += new System.EventHandler<System.Windows.Forms.DataVisualization.Charting.ViewEventArgs>(this.Chart_Future_AxisViewChanged);
            // 
            // tabPage_recode
            // 
            this.tabPage_recode.Controls.Add(this.label_moduleName);
            this.tabPage_recode.Controls.Add(this.listView_Recoder);
            this.tabPage_recode.Controls.Add(this.Chart_BackTest);
            this.tabPage_recode.Controls.Add(this.DataGridView_BackTest);
            this.tabPage_recode.Location = new System.Drawing.Point(4, 22);
            this.tabPage_recode.Name = "tabPage_recode";
            this.tabPage_recode.Padding = new System.Windows.Forms.Padding(3, 3, 3, 3);
            this.tabPage_recode.Size = new System.Drawing.Size(1328, 683);
            this.tabPage_recode.TabIndex = 2;
            this.tabPage_recode.Text = "백테스팅 기록";
            this.tabPage_recode.UseVisualStyleBackColor = true;
            // 
            // label_moduleName
            // 
            this.label_moduleName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label_moduleName.Location = new System.Drawing.Point(127, 8);
            this.label_moduleName.Name = "label_moduleName";
            this.label_moduleName.Size = new System.Drawing.Size(1195, 38);
            this.label_moduleName.TabIndex = 9;
            this.label_moduleName.Text = "label1";
            // 
            // listView_Recoder
            // 
            this.listView_Recoder.HideSelection = false;
            this.listView_Recoder.Location = new System.Drawing.Point(0, 0);
            this.listView_Recoder.Name = "listView_Recoder";
            this.listView_Recoder.Size = new System.Drawing.Size(121, 677);
            this.listView_Recoder.TabIndex = 8;
            this.listView_Recoder.UseCompatibleStateImageBehavior = false;
            this.listView_Recoder.SelectedIndexChanged += new System.EventHandler(this.listView_Recoder_SelectedIndexChanged);
            // 
            // Chart_BackTest
            // 
            legend2.Name = "Legend1";
            this.Chart_BackTest.Legends.Add(legend2);
            this.Chart_BackTest.Location = new System.Drawing.Point(127, 50);
            this.Chart_BackTest.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Chart_BackTest.Name = "Chart_BackTest";
            this.Chart_BackTest.Size = new System.Drawing.Size(1195, 433);
            this.Chart_BackTest.TabIndex = 7;
            this.Chart_BackTest.Text = "backTestChart";
            // 
            // DataGridView_BackTest
            // 
            this.DataGridView_BackTest.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DataGridView_BackTest.Location = new System.Drawing.Point(127, 491);
            this.DataGridView_BackTest.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.DataGridView_BackTest.Name = "DataGridView_BackTest";
            this.DataGridView_BackTest.ReadOnly = true;
            this.DataGridView_BackTest.RowHeadersWidth = 62;
            this.DataGridView_BackTest.RowTemplate.Height = 23;
            this.DataGridView_BackTest.Size = new System.Drawing.Size(1195, 186);
            this.DataGridView_BackTest.TabIndex = 6;
            // 
            // tabPage_eval
            // 
            this.tabPage_eval.Controls.Add(this.DataGridView_FutureInfo);
            this.tabPage_eval.Location = new System.Drawing.Point(4, 22);
            this.tabPage_eval.Name = "tabPage_eval";
            this.tabPage_eval.Padding = new System.Windows.Forms.Padding(3, 3, 3, 3);
            this.tabPage_eval.Size = new System.Drawing.Size(1328, 683);
            this.tabPage_eval.TabIndex = 1;
            this.tabPage_eval.Text = "평가";
            this.tabPage_eval.UseVisualStyleBackColor = true;
            // 
            // DataGridView_FutureInfo
            // 
            this.DataGridView_FutureInfo.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DataGridView_FutureInfo.Location = new System.Drawing.Point(3, 7);
            this.DataGridView_FutureInfo.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.DataGridView_FutureInfo.Name = "DataGridView_FutureInfo";
            this.DataGridView_FutureInfo.ReadOnly = true;
            this.DataGridView_FutureInfo.RowHeadersWidth = 62;
            this.DataGridView_FutureInfo.RowTemplate.Height = 23;
            this.DataGridView_FutureInfo.Size = new System.Drawing.Size(1319, 672);
            this.DataGridView_FutureInfo.TabIndex = 5;
            // 
            // FutureDataInfoDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1366, 731);
            this.Controls.Add(this.tabControl_future);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FutureDataInfoDlg";
            this.Text = "FutureInfo";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FutureInfoDlg_KeyDown);
            this.tabControl_future.ResumeLayout(false);
            this.tabPage_chart.ResumeLayout(false);
            this.tabPage_chart.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Chart_Future)).EndInit();
            this.tabPage_recode.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.Chart_BackTest)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.DataGridView_BackTest)).EndInit();
            this.tabPage_eval.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.DataGridView_FutureInfo)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl_future;
        private System.Windows.Forms.TabPage tabPage_chart;
        internal UtilLibrary.QuickChart Chart_Future;
        private System.Windows.Forms.TabPage tabPage_eval;
        internal UtilLibrary.QuickDataGridView DataGridView_FutureInfo;
        private System.Windows.Forms.TabPage tabPage_recode;
        internal UtilLibrary.QuickDataGridView DataGridView_BackTest;
        internal UtilLibrary.QuickChart Chart_BackTest;
        internal System.Windows.Forms.ListView listView_Recoder;
        internal System.Windows.Forms.Label label_moduleName;
        internal System.Windows.Forms.ComboBox comboBox_priceType;
        internal System.Windows.Forms.Label label_chartInfo;
    }
}