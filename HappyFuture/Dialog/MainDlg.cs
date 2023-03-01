using StockLibrary;
using System;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using UtilLibrary;

namespace HappyFuture
{
    public partial class HappyFutureDlg: Form
    {
        private bool started_;
        public bool loaded_ { get; set; }
        public FutureDlg futureDlg_ = null;

        public HappyFutureDlg()
        {
            this.futureDlg_ = new FutureDlg();
            futureDlg_.StartPosition = FormStartPosition.Manual;
            this.InitializeComponent();
            this.loaded_ = false;
            this.started_ = false;

            // win form의 예외 처리 잡기
            Application.ThreadException += new ThreadExceptionEventHandler(this.threadException);

            // console의 예외 처리 잡기
            Thread.GetDomain().UnhandledException += new UnhandledExceptionEventHandler(this.unhandledException);
        }

        private void threadException(object sender, ThreadExceptionEventArgs e)
        {
            var bot = ControlGet.getInstance.futureBot();
            ExceptionProcess exceptionProcess = ExceptionProcess.getInstance;
            exceptionProcess.remainStackFile(e.Exception);
            exceptionProcess.sendStackMail(PublicVar.notifyMail, e.Exception);
            exceptionProcess.remainDumpFile();
        }

        private void unhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            var bot = ControlGet.getInstance.futureBot();
            try {
                AccessViolationException execption = (AccessViolationException) args.ExceptionObject;
                Logger.getInstance.consolePrint("[프로그램] {1}\n{2}\n{3}", execption.Message, execption.StackTrace, execption.InnerException);
                Logger.getInstance.print(KiwoomCode.Log.에러, "[프로그램] {1}\n{2}\n{3}", execption.Message, execption.StackTrace, execption.InnerException);

                ExceptionProcess exceptionProcess = ExceptionProcess.getInstance;
                exceptionProcess.remainStackFile(execption);
                exceptionProcess.sendStackMail(PublicVar.notifyMail, execption);
                exceptionProcess.remainDumpFile();
            } catch( Exception e) {
                Logger.getInstance.print(KiwoomCode.Log.에러, "{0} / {1}", e.Message, e.StackTrace);
            }
        }

        private void setMainTitle(string stateName)
        {
            // 이거 util로 두지 말것
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

            this.Text = string.Format("행복의 선물 ; {0} ; Create {1} ; Version {2}", stateName, dtBuildDate.ToString(), strVersionText);
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
            if (this.started_ == true) {
                return false;
            }
            if (this.loaded_ == false) {
                return false;
            }
            return true;
        }

        public void start()
        {
            if (this.statable() == false) {
                return;
            }
            this.Button_Future.Enabled = false;

            this.started_ = true;
        }

        public void button_Future_Click(object sender, EventArgs e)
        {
            this.Button_Future.Enabled = false;
            this.Button_Test.Enabled = false;
            this.Button_Quit.Enabled = false;
            futureDlg_.Location = new System.Drawing.Point(100, this.Location.Y + this.Size.Height / 2);
            this.futureDlg_.Show();
            this.start();

            var bot = ControlGet.getInstance.futureBot();
            if (bot.engine_.start()) {
                bot.start();
            }
        }

        private void Button_Test_Click(object sender, EventArgs e)
        {
            var bot = ControlGet.getInstance.futureBot();
            bot.test_ = true;

            this.button_Future_Click(sender, e);
        }

        public void quit()
        {
            var bot = ControlGet.getInstance.futureBot();
            var telegram = bot.telegram_;
            if (telegram != null) {
                telegram.sendMessage("행복의 선물 종료");
                while (true) {
                    if (telegram.messageCount() == 0) {
                        break;
                    }
                    // 메시지 다 보낼때까지 대기
                    Thread.Sleep(1000);
                }
                Logger.getInstance.close();
                bot.engine_.shutdown();
            }
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }

        private void HappyFutureDlg_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.quit();
        }

        private void Button_Quit_Click(object sender, EventArgs e)
        {
            this.quit();
        }

        private void setup()
        {
            this.Button_Future.Enabled = false;

            this.setTitle("Welcome");

            // 컨트롤 넘겨줘서 셋팅하기
            Logger.getInstance.setup(this.ListView_Log);
            var bot = futureDlg_.bot_ = new FutureBot();
            FutureEngine engine = bot.engine_;
            engine.setup(bot, this.axKFOpenAPI);
            engine.start();
        }

        private void HappyFutureDlg_Shown(object sender, EventArgs e)
        {
            this.setup();
        }

        private void Button_reLogin_Click(object sender, EventArgs e)
        {
            var bot = ControlGet.getInstance.futureBot();
            FutureEngine engine = bot.engine_;
            if (engine.openLoginDlg() == false) {
                Logger.getInstance.print(KiwoomCode.Log.에러, "로그인창 열기 실패");
            }
        }

        private void Button_passwd_Click(object sender, EventArgs e)
        {
            var bot = ControlGet.getInstance.futureBot();
            FutureEngine engine = bot.engine_;
            engine.showAccountPw();

            MessageBox.Show("봇의 status 를 다시 초기화 합니다.");
            Logger.getInstance.print(KiwoomCode.Log.API조회, "로그인창 열기 성공");
            var futureState = bot.botState_ as FutureBotState;
            futureState.initState();
        }

        private void ComboBox_account_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox_account.SelectedIndex < 0) {
                return;
            }
            var accountNumber = comboBox_account.SelectedItem.ToString();
            var bot = ControlGet.getInstance.futureBot();
            bot.engine_.setAccountNumber(accountNumber);

            var dlg = Program.happyFuture_.futureDlg_;
            if (dlg != null) {
                dlg.setAccountNumber(accountNumber);
            }

            Logger.getInstance.print(KiwoomCode.Log.주식봇, "선물 [{0}] 계좌 선택", accountNumber);
        }

    }
}
