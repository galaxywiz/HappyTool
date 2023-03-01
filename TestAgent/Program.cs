using HappyAgent;
using System.Threading;

namespace TestAgent
{
    class Program
    {
        static Thread mainThread_ = null;
        static bool shutdown_ = false;
        static private Scheduler sheduler_ = null;

        static void Main(string[] args)
        {
            mainThread_ = new Thread(new ThreadStart(run));
            mainThread_.Start();
        }

        // 실제 서비스 작업 함수
        static private void run()
        {
            sheduler_ = new Scheduler();
            if (sheduler_.setup() == false)
            {
                return;
            }
            const int ONE_SECOND = 1000;
            while (shutdown_ == false) {
                Thread.Sleep(ONE_SECOND);
                sheduler_.run();
            }
        }
    }
}
