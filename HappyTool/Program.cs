using HappyTool.Stock;
using HappyTool.Util;
using StockLibrary;
using System;
using System.Threading;
using System.Windows.Forms;
using UtilLibrary;

namespace HappyTool
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        public static HappyTool happyTool_ = null;

        [STAThread]
                
        static void Main()
        {
            // 프로그램에서 못잡는 에러를 catch
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(unhandledException);

            bool isNew = true;
            Mutex mutex = new Mutex(true, "중복방지", out isNew);

            if (isNew) {
                try {
                    ErrorModeCtl.setNoErrorDlg();
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    happyTool_ = new HappyTool();
                    Application.Run(happyTool_);
                }
                catch (AccessViolationException execption) {
                    Logger.getInstance.consolePrint("[프로그램] {1}\n{2}\n{3}", execption.Message, execption.StackTrace, execption.InnerException);
                    Logger.getInstance.print(KiwoomCode.Log.에러, "[프로그램] {1}\n{2}\n{3}", execption.Message, execption.StackTrace, execption.InnerException);
                }
            }
        }

        private static void unhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            var bot = ControlGet.getInstance.stockBot();
            if (bot == null) {
                return;
            }
            if (bot.allowDump_ == false) {
                return;
            }
            AccessViolationException execption = (AccessViolationException) args.ExceptionObject;
            Logger.getInstance.consolePrint("[프로그램] {1}\n{2}\n{3}", execption.Message, execption.StackTrace, execption.InnerException);
            Logger.getInstance.print(KiwoomCode.Log.에러, "[프로그램] {1}\n{2}\n{3}", execption.Message, execption.StackTrace, execption.InnerException);

            ExceptionProcess exceptionProcess = ExceptionProcess.getInstance;
            exceptionProcess.remainStackFile(execption);
            exceptionProcess.sendStackMail(PublicVar.notifyMail, execption);
            exceptionProcess.remainDumpFile();
        }
    }
}
