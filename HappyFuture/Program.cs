using System;
using System.Threading;
using System.Windows.Forms;
using UtilLibrary;

namespace HappyFuture
{
    static class Program
    {
        public static HappyFutureDlg happyFuture_ = null;

        [STAThread]

        static void Main()
        {
            // 프로그램에서 못잡는 에러를 catch
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(unhandledException);

            bool isNew = true;
            Mutex mutex = new Mutex(true, "행복의 선물 중복방지", out isNew);

            if (isNew) {
                try {
                    ErrorModeCtl.setNoErrorDlg();
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    happyFuture_ = new HappyFutureDlg();
                    happyFuture_.StartPosition = FormStartPosition.Manual;
                    happyFuture_.Location = new System.Drawing.Point(100, 0);
                    Application.Run(happyFuture_);
                    mutex.ReleaseMutex();
                }
                catch (AccessViolationException execption) {
                    Logger.getInstance.consolePrint("[프로그램] {1}\n{2}\n{3}", execption.Message, execption.StackTrace, execption.InnerException);
                    Logger.getInstance.print(KiwoomCode.Log.에러, "[프로그램] {1}\n{2}\n{3}", execption.Message, execption.StackTrace, execption.InnerException);
                }
            }
        }

        private static void unhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            var bot = ControlGet.getInstance.futureBot();
            try {
                AccessViolationException execption = (AccessViolationException) args.ExceptionObject;
                Logger.getInstance.consolePrint("[프로그램] {1}\n{2}\n{3}", execption.Message, execption.StackTrace, execption.InnerException);
                Logger.getInstance.print(KiwoomCode.Log.에러, "[프로그램] {1}\n{2}\n{3}", execption.Message, execption.StackTrace, execption.InnerException);

                ExceptionProcess exceptionProcess = ExceptionProcess.getInstance;
                exceptionProcess.remainStackFile(execption);
                exceptionProcess.sendStackMail("galaxy_wiz@naver.com", execption);
                exceptionProcess.remainDumpFile();
            } catch (Exception e) {
                Logger.getInstance.print(KiwoomCode.Log.에러, "{0} / {1}", e.Message, e.StackTrace);
            }
            //    Environment.Exit(0);
        }
    }
}
