using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HappyAgent_Program
{
    public partial class AgentForm : Form
    {
        HappyAgent happyAgent_ = new HappyAgent();

        public AgentForm()
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
            this.Visible = true;
            this.notifyIcon1.Visible = true;
            notifyIcon1.ContextMenuStrip = contextMenuStrip1;
        }

        private void 종료ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            happyAgent_.OnStop();
            Application.Exit();
        }

        private void AgentForm_Shown(object sender, EventArgs e)
        {
            happyAgent_.OnStart();
        }
    }
}
