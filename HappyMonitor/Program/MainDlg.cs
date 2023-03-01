using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UtilLibrary;

namespace HappyMonitor
{
    public partial class MainDlg: Form
    {
        public MainDlg()
        {
            InitializeComponent();
        }

        private void MainDlg_Shown(object sender, EventArgs e)
        {
            this.setup();
        }

        internal MonitorServer server_ = new MonitorServer();
        internal MonitorViewer monitorViewer_ = new MonitorViewer();
        private void setup()
        {
            this.setTitle("행복의 모니터");

            // 컨트롤 넘겨줘서 셋팅하기
            Logger.getInstance.setup(this.ListView_Log);
            monitorViewer_.setup(this.quickDataGridView_monitor);
            capture_ = new ControlCapture(this);
            this.setupTelegram();

            server_.start();
        }

        private void setMainTitle(string stateName)
        {
            this.Text = stateName;
        }

        public void setTitle(string stateName)
        {
            if (this.InvokeRequired) {
                this.BeginInvoke(new Action(() => this.setMainTitle(stateName)));
            }
            else {
                this.setMainTitle(stateName);
            }
        }
        //--------------------------------------------------------------------------//
        // 화면 캡쳐
        const string futureDlgImg_ = "main.png";
        ControlCapture capture_;

        public string captureFormImgName()
        {
            string path = Application.StartupPath + "\\capture\\" + futureDlgImg_;
            return path;
        }

        public void caputreForm()
        {
            this.capture_.formCapture(futureDlgImg_);
        }

        public void sendMessage(string message)
        {
            telegram_.sendMessage(message);
        }

        MonitorTelegram telegram_;
        void setupTelegram()
        {
            string api = "933352356:AAEbCPM2nOpxm9ZOt1Ln9eCsQQaH3cqvZF8";
            long id = 508897948;
            telegram_ = new MonitorTelegram(api, id);
            telegram_.start();

            var log = string.Format("!!! [{0}] 행복의 모니터 시작 !!!", DateTime.Now);
            this.sendMessage(log);
        }

        private void MainDlg_FormClosed(object sender, FormClosedEventArgs e)
        {
            Logger.getInstance.close();
            server_.stop();
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
    }
}
