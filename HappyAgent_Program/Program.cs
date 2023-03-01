using System;
using System.Threading;
using System.Windows.Forms;

namespace HappyAgent_Program
{
    class Program
    {
        static void Main(string[] args)
        {
            bool isNew = true;
            Mutex mutex = new Mutex(true, "행복의 감시자 중복방지", out isNew);
            if (isNew) {
                Application.Run(new AgentForm());
            }
        }
    }
}
