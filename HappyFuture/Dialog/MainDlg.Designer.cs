using AxKFOpenAPILib;

namespace HappyFuture
{
    partial class HappyFutureDlg
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HappyFutureDlg));
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.ListView_Log = new System.Windows.Forms.ListView();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.comboBox_account = new System.Windows.Forms.ComboBox();
            this.Button_Quit = new System.Windows.Forms.Button();
            this.axKFOpenAPI = new AxKFOpenAPILib.AxKFOpenAPI();
            this.Button_Test = new System.Windows.Forms.Button();
            this.button_passwd = new System.Windows.Forms.Button();
            this.Button_reLogin = new System.Windows.Forms.Button();
            this.Button_Future = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.axKFOpenAPI)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.ListView_Log);
            this.groupBox2.Location = new System.Drawing.Point(12, 73);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox2.Size = new System.Drawing.Size(976, 285);
            this.groupBox2.TabIndex = 8;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Log";
            // 
            // ListView_Log
            // 
            this.ListView_Log.BackColor = System.Drawing.Color.Black;
            this.ListView_Log.ForeColor = System.Drawing.Color.Lime;
            this.ListView_Log.HideSelection = false;
            this.ListView_Log.Location = new System.Drawing.Point(4, 23);
            this.ListView_Log.Name = "ListView_Log";
            this.ListView_Log.Size = new System.Drawing.Size(966, 255);
            this.ListView_Log.TabIndex = 0;
            this.ListView_Log.UseCompatibleStateImageBehavior = false;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.comboBox_account);
            this.groupBox1.Controls.Add(this.Button_Quit);
            this.groupBox1.Controls.Add(this.axKFOpenAPI);
            this.groupBox1.Controls.Add(this.Button_Test);
            this.groupBox1.Controls.Add(this.button_passwd);
            this.groupBox1.Controls.Add(this.Button_reLogin);
            this.groupBox1.Controls.Add(this.Button_Future);
            this.groupBox1.Location = new System.Drawing.Point(12, 13);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox1.Size = new System.Drawing.Size(970, 52);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Control";
            // 
            // comboBox_account
            // 
            this.comboBox_account.FormattingEnabled = true;
            this.comboBox_account.Location = new System.Drawing.Point(337, 20);
            this.comboBox_account.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.comboBox_account.Name = "comboBox_account";
            this.comboBox_account.Size = new System.Drawing.Size(165, 20);
            this.comboBox_account.TabIndex = 26;
            this.comboBox_account.SelectedIndexChanged += new System.EventHandler(this.ComboBox_account_SelectedIndexChanged);
            // 
            // Button_Quit
            // 
            this.Button_Quit.Location = new System.Drawing.Point(876, 18);
            this.Button_Quit.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Button_Quit.Name = "Button_Quit";
            this.Button_Quit.Size = new System.Drawing.Size(88, 22);
            this.Button_Quit.TabIndex = 5;
            this.Button_Quit.Text = "종료";
            this.Button_Quit.UseVisualStyleBackColor = true;
            this.Button_Quit.Click += new System.EventHandler(this.Button_Quit_Click);
            // 
            // axKFOpenAPI
            // 
            this.axKFOpenAPI.Enabled = true;
            this.axKFOpenAPI.Location = new System.Drawing.Point(0, 29);
            this.axKFOpenAPI.Name = "axKFOpenAPI";
            this.axKFOpenAPI.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axKFOpenAPI.OcxState")));
            this.axKFOpenAPI.Size = new System.Drawing.Size(51, 36);
            this.axKFOpenAPI.TabIndex = 3;
            this.axKFOpenAPI.Visible = false;
            // 
            // Button_Test
            // 
            this.Button_Test.Location = new System.Drawing.Point(16, 18);
            this.Button_Test.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Button_Test.Name = "Button_Test";
            this.Button_Test.Size = new System.Drawing.Size(88, 22);
            this.Button_Test.TabIndex = 0;
            this.Button_Test.Text = "테스팅전용";
            this.Button_Test.UseVisualStyleBackColor = true;
            this.Button_Test.Click += new System.EventHandler(this.Button_Test_Click);
            // 
            // button_passwd
            // 
            this.button_passwd.Location = new System.Drawing.Point(729, 18);
            this.button_passwd.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.button_passwd.Name = "button_passwd";
            this.button_passwd.Size = new System.Drawing.Size(88, 22);
            this.button_passwd.TabIndex = 0;
            this.button_passwd.Text = "비밀번호";
            this.button_passwd.UseVisualStyleBackColor = true;
            this.button_passwd.Click += new System.EventHandler(this.Button_passwd_Click);
            // 
            // Button_reLogin
            // 
            this.Button_reLogin.Location = new System.Drawing.Point(635, 18);
            this.Button_reLogin.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Button_reLogin.Name = "Button_reLogin";
            this.Button_reLogin.Size = new System.Drawing.Size(88, 22);
            this.Button_reLogin.TabIndex = 0;
            this.Button_reLogin.Text = "다시 로그인";
            this.Button_reLogin.UseVisualStyleBackColor = true;
            this.Button_reLogin.Click += new System.EventHandler(this.Button_reLogin_Click);
            // 
            // Button_Future
            // 
            this.Button_Future.Location = new System.Drawing.Point(135, 18);
            this.Button_Future.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Button_Future.Name = "Button_Future";
            this.Button_Future.Size = new System.Drawing.Size(88, 22);
            this.Button_Future.TabIndex = 0;
            this.Button_Future.Text = "해외 선물 봇";
            this.Button_Future.UseVisualStyleBackColor = true;
            this.Button_Future.Click += new System.EventHandler(this.button_Future_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(275, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 12);
            this.label1.TabIndex = 27;
            this.label1.Text = "선택 계좌";
            // 
            // HappyFutureDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 369);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "HappyFutureDlg";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.HappyFutureDlg_FormClosing);
            this.Shown += new System.EventHandler(this.HappyFutureDlg_Shown);
            this.groupBox2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.axKFOpenAPI)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        internal System.Windows.Forms.ListView ListView_Log;
        internal System.Windows.Forms.Button Button_Future;
        internal AxKFOpenAPI axKFOpenAPI;
        internal System.Windows.Forms.Button Button_Quit;
        internal System.Windows.Forms.Button Button_Test;
        internal System.Windows.Forms.Button Button_reLogin;
        internal System.Windows.Forms.Button button_passwd;
        internal System.Windows.Forms.ComboBox comboBox_account;
        private System.Windows.Forms.Label label1;
    }
}

