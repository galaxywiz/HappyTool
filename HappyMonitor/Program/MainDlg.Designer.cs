namespace HappyMonitor
{
    partial class MainDlg
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainDlg));
            this.quickDataGridView_monitor = new UtilLibrary.QuickDataGridView();
            this.label1 = new System.Windows.Forms.Label();
            this.monthCalendar1 = new System.Windows.Forms.MonthCalendar();
            this.ListView_Log = new System.Windows.Forms.ListView();
            ((System.ComponentModel.ISupportInitialize)(this.quickDataGridView_monitor)).BeginInit();
            this.SuspendLayout();
            // 
            // quickDataGridView_monitor
            // 
            this.quickDataGridView_monitor.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.quickDataGridView_monitor.Location = new System.Drawing.Point(12, 41);
            this.quickDataGridView_monitor.Name = "quickDataGridView_monitor";
            this.quickDataGridView_monitor.RowTemplate.Height = 23;
            this.quickDataGridView_monitor.Size = new System.Drawing.Size(1180, 264);
            this.quickDataGridView_monitor.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(20, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(121, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "행복의 도구들 모니터";
            // 
            // monthCalendar1
            // 
            this.monthCalendar1.Location = new System.Drawing.Point(972, 317);
            this.monthCalendar1.Name = "monthCalendar1";
            this.monthCalendar1.TabIndex = 2;
            // 
            // ListView_Log
            // 
            this.ListView_Log.BackColor = System.Drawing.Color.Black;
            this.ListView_Log.ForeColor = System.Drawing.Color.Lime;
            this.ListView_Log.HideSelection = false;
            this.ListView_Log.Location = new System.Drawing.Point(12, 317);
            this.ListView_Log.Name = "ListView_Log";
            this.ListView_Log.Size = new System.Drawing.Size(948, 162);
            this.ListView_Log.TabIndex = 5;
            this.ListView_Log.UseCompatibleStateImageBehavior = false;
            // 
            // MainDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1204, 489);
            this.Controls.Add(this.ListView_Log);
            this.Controls.Add(this.monthCalendar1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.quickDataGridView_monitor);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainDlg";
            this.Text = "Form1";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainDlg_FormClosed);
            this.Shown += new System.EventHandler(this.MainDlg_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.quickDataGridView_monitor)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal UtilLibrary.QuickDataGridView quickDataGridView_monitor;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.MonthCalendar monthCalendar1;
        internal System.Windows.Forms.ListView ListView_Log;
    }
}

