using NetLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HappyMonitor
{
    public class MonitorServer: TCP_Server
    {
        public void sendHeartBeat()
        {
            this.onSendDataToAll(new HeartBeatPacket());
        }

        protected override void logAccept(User user)
        {
            var log = string.Format("* 클라이언트 접속 : {0} ", user.socket_.RemoteEndPoint);
            Program.mainDlg_.sendMessage(log);
        }

        protected override void logClose(User user)
        {
            var log = string.Format("* {0} 서버 접속 해제", user.serverNmae_);
            Program.mainDlg_.sendMessage(log);
        }
        
        public override void processPacket(User user)
        {
            base.processPacket(user);

            var viewer = Program.mainDlg_.monitorViewer_;
            viewer.update();
        }

        public double todayProfitSum()
        {
            double sumProfit = 0.0f;
            doUserEach eachDo = (User user) => {
                string [] tokens = user.serverNmae_.Split(':');
                if (tokens.Length < 2) {
                    return;
                }
                //5 로 시작하는게 실제 계좌
                if (tokens[1].Trim().StartsWith("5")) { 
                    sumProfit += user.todayProfit_;
                }
            };
            this.doUsers(eachDo);

            return sumProfit;
        }
    }
}
