using System;
using System.Windows.Forms;
using HappyTool.Stock;
using HappyTool.Util;
using System.Windows.Forms.DataVisualization.Charting;
using HappyTool.DlgControl;
using HappyTool.Stock.Calculate;
using System.Threading;
using System.Globalization;
using HappyTool.Dlg;

namespace HappyTool
{
    public partial class HappyTool : Form
    {
        private bool started_;
        public bool loaded_ { get; set; }
        public StockDlg stockDlg_ = null;

        public HappyTool()
        {
            stockDlg_ = new StockDlg();

            InitializeComponent();
            loaded_ = false;
            started_ = false;
            // win form의 예외 처리 잡기
            Application.ThreadException += new ThreadExceptionEventHandler(threadException);

            // console의 예외 처리 잡기
            Thread.GetDomain().UnhandledException += new UnhandledExceptionEventHandler(unhandledException);
        }

        private void threadException(object sender, ThreadExceptionEventArgs e)
        {
            ExceptionProcess exceptionProcess = ExceptionProcess.getInstance;
            exceptionProcess.remainStackFile(e.Exception);
            exceptionProcess.sendStackMail(e.Exception);
            exceptionProcess.remainDumpFile();
        }

        private void unhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            AccessViolationException execption = (AccessViolationException) args.ExceptionObject;
            Logger.getInstance.consolePrint("[프로그램] {1}\n{2}\n{3}", execption.Message, execption.StackTrace, execption.InnerException);
            Logger.getInstance.print(KiwoomCode.Log.에러, "[프로그램] {1}\n{2}\n{3}", execption.Message, execption.StackTrace, execption.InnerException);

            ExceptionProcess exceptionProcess = ExceptionProcess.getInstance;
            exceptionProcess.remainStackFile(execption);
            exceptionProcess.sendStackMail(execption);
            exceptionProcess.remainDumpFile();
        }

        private void HappyTool_Shown(Object sender, EventArgs e)
        {
            this.setup();
            StockEngine.getInstance.start();
        }

        private int sizeX_, sizeY_;
        private void setup()
        {
            sizeX_ = this.Size.Width;
            sizeY_ = this.Size.Height;

            button_Stock.Enabled = false;
            button_Future.Enabled = false;
            button_Crypto.Enabled = false;

            this.setTitle("Welcome");

            // 컨트롤 넘겨줘서 셋팅하기
            Logger.getInstance.setup(ListView_Log);
            StockEngine.getInstance.setup(this);
        }

        private void setMainTitle(string stateName)
        {
            string strVersionText = System.Reflection.Assembly.GetExecutingAssembly().FullName
                                      .Split(',')[1]
                                      .Trim()
                                      .Split('=')[1];

            //2. Version Text의 세번째 값(Build Number)은 2000년 1월 1일부터
            //Build된 날짜까지의 총 일(Days) 수 이다.
            int intDays = Convert.ToInt32(strVersionText.Split('.')[2]);
            DateTime refDate = new DateTime(2000, 1, 1);
            DateTime dtBuildDate = refDate.AddDays(intDays);

            //3. Verion Text의 네번째 값(Revision NUmber)은 자정으로부터 Build된
            //시간까지의 지나간 초(Second) 값 이다.
            int intSeconds = Convert.ToInt32(strVersionText.Split('.')[3]);
            intSeconds = intSeconds * 2;
            dtBuildDate = dtBuildDate.AddSeconds(intSeconds);

            //4. 시차조정
            DaylightTime daylingTime = TimeZone.CurrentTimeZone.GetDaylightChanges(dtBuildDate.Year);
            if (TimeZone.IsDaylightSavingTime(dtBuildDate, daylingTime)) {
                dtBuildDate = dtBuildDate.Add(daylingTime.Delta);
            }

            this.Text = string.Format("행복의 도구 ; {0} ; Create {1} ; Version {2}", stateName, dtBuildDate.ToString(), strVersionText);
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

        private bool statable()
        {
            if (started_ == true) {
                return false;
            }
            if (loaded_ == false) {
                return false;
            }
            return true;
        }

        public void start()
        {
            if (this.statable() == false) {
                return;
            }
            button_Stock.Enabled = false;
          
            started_ = true;
        }

        private void Button_start_Click(object sender, EventArgs e)
        {
            if (StockEngine.getInstance.start()) {
                StockBot.getInstance.start();
            }
            stockDlg_.Show();
            this.start();
        }

        public void quit()
        {
            Logger.getInstance.close();
            StockEngine.getInstance.shutdown();
            StockEngine.getInstance.quit();
            Application.Exit();
        }

        private void Button_quit_Click(object sender, EventArgs e)
        {
            this.quit();
        }

        //--------------------------------------------------------------------------
        // 메인 컨트롤들 넘겨줌.

        public AxKHOpenAPILib.AxKHOpenAPI openApi()
        {
            return axKHOpenAPI;
        }

        public Button buttonStart()
        {
            return button_Stock;
        }
        
        private void HappyTool_FormClosing(Object sender, FormClosingEventArgs e)
        {
            Button_quit_Click(sender, e);
        }

        private void DataGridView_Stock_CellClick(Object sender, DataGridViewCellEventArgs e)
        {
            StockPoolViewer.getInstance.cellClick(sender, e);
        }
    }
}
