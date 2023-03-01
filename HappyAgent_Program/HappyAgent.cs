using HappyAgent;
using System;
using System.Threading;
using System.Windows.Forms;
using UtilLibrary;

namespace HappyAgent_Program
{
    class HappyAgent: SingleTon<HappyAgent>
    {
        Thread mainThread_ = null;
        bool shutdown_ = true;
        internal Scheduler sheduler_ = null;

        public HappyAgent()
        {
            sheduler_ = new Scheduler();
        }

        public void OnStart()
        {
            if (shutdown_ == true) {
                sheduler_.setup();
                System.Console.WriteLine("프로그램 실행");
                shutdown_ = false;
                mainThread_ = new Thread(new ThreadStart(run));
                mainThread_.Start();
            }
        }

        public void OnStop()
        {
            System.Console.WriteLine("프로그램 정지");
            shutdown_ = true;
            sheduler_.sendShutdownLog();

            Application.ExitThread();
            Environment.Exit(0);
        }

        // 실제 서비스 작업 함수
        private void run()
        {
            TimeWatch updateWatch_ = new TimeWatch(10);

            while (shutdown_ == false) {
                Thread.Sleep(1000);
                if (updateWatch_.isTimeOver()) {
                    updateWatch_.reset();
                    sheduler_.run();
                }
            }
            this.OnStop();
        }
    }
}
