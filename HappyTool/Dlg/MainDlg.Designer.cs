using HappyTool.Util;

namespace HappyTool
{
    partial class HappyTool
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HappyTool));
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.button_Stock = new System.Windows.Forms.Button();
            this.Button_quit = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.axKHOpenAPI = new AxKHOpenAPILib.AxKHOpenAPI();
            this.button_Future = new System.Windows.Forms.Button();
            this.button_Crypto = new System.Windows.Forms.Button();
            this.ListView_Log = new System.Windows.Forms.ListView();
            this.groupBox3.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.axKHOpenAPI)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.ListView_Log);
            this.groupBox3.Location = new System.Drawing.Point(12, 86);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox3.Size = new System.Drawing.Size(627, 272);
            this.groupBox3.TabIndex = 6;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Log";
            // 
            // Button_Stock
            // 
            this.button_Stock.Location = new System.Drawing.Point(16, 24);
            this.button_Stock.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.button_Stock.Name = "Button_Stock";
            this.button_Stock.Size = new System.Drawing.Size(88, 22);
            this.button_Stock.TabIndex = 1;
            this.button_Stock.Text = "주식 봇";
            this.button_Stock.UseVisualStyleBackColor = true;
            this.button_Stock.Click += new System.EventHandler(this.Button_start_Click);
            // 
            // Button_quit
            // 
            this.Button_quit.Location = new System.Drawing.Point(518, 24);
            this.Button_quit.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Button_quit.Name = "Button_quit";
            this.Button_quit.Size = new System.Drawing.Size(88, 22);
            this.Button_quit.TabIndex = 2;
            this.Button_quit.Text = "종료";
            this.Button_quit.UseVisualStyleBackColor = true;
            this.Button_quit.Click += new System.EventHandler(this.Button_quit_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.axKHOpenAPI);
            this.groupBox1.Controls.Add(this.Button_quit);
            this.groupBox1.Controls.Add(this.button_Crypto);
            this.groupBox1.Controls.Add(this.button_Future);
            this.groupBox1.Controls.Add(this.button_Stock);
            this.groupBox1.Location = new System.Drawing.Point(12, 13);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox1.Size = new System.Drawing.Size(627, 65);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Control";
            // 
            // axKHOpenAPI
            // 
            this.axKHOpenAPI.Enabled = true;
            this.axKHOpenAPI.Location = new System.Drawing.Point(110, 23);
            this.axKHOpenAPI.Name = "axKHOpenAPI";
            this.axKHOpenAPI.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axKHOpenAPI.OcxState")));
            this.axKHOpenAPI.Size = new System.Drawing.Size(56, 23);
            this.axKHOpenAPI.TabIndex = 11;
            this.axKHOpenAPI.Visible = false;
            // 
            // button_Future
            // 
            this.button_Future.Location = new System.Drawing.Point(184, 23);
            this.button_Future.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.button_Future.Name = "button_Future";
            this.button_Future.Size = new System.Drawing.Size(88, 22);
            this.button_Future.TabIndex = 1;
            this.button_Future.Text = "선물 봇";
            this.button_Future.UseVisualStyleBackColor = true;
            this.button_Future.Click += new System.EventHandler(this.Button_start_Click);
            // 
            // button_Crypto
            // 
            this.button_Crypto.Location = new System.Drawing.Point(341, 24);
            this.button_Crypto.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.button_Crypto.Name = "button_Crypto";
            this.button_Crypto.Size = new System.Drawing.Size(88, 22);
            this.button_Crypto.TabIndex = 1;
            this.button_Crypto.Text = "암호화폐 봇";
            this.button_Crypto.UseVisualStyleBackColor = true;
            this.button_Crypto.Click += new System.EventHandler(this.Button_start_Click);
            // 
            // quickListView_Log
            // 
            this.ListView_Log.Location = new System.Drawing.Point(4, 23);
            this.ListView_Log.Name = "quickListView_Log";
            this.ListView_Log.Size = new System.Drawing.Size(621, 245);
            this.ListView_Log.TabIndex = 0;
            this.ListView_Log.UseCompatibleStateImageBehavior = false;
            // 
            // HappyTool
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(649, 368);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "HappyTool";
            this.Opacity = 0.92D;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "HappyTool";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.HappyTool_FormClosing);
            this.Shown += new System.EventHandler(this.HappyTool_Shown);
            this.groupBox3.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.axKHOpenAPI)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button button_Stock;
        private System.Windows.Forms.Button Button_quit;
        private System.Windows.Forms.GroupBox groupBox1;
        private AxKHOpenAPILib.AxKHOpenAPI axKHOpenAPI;
        private System.Windows.Forms.Button button_Crypto;
        private System.Windows.Forms.Button button_Future;
        private System.Windows.Forms.ListView ListView_Log;
        //private AxKHOpenAPILib.AxKHOpenAPI axKHOpenAPI;
    }
}

