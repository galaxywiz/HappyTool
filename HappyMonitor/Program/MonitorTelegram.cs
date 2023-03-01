using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilLibrary;

namespace HappyMonitor
{
    class MonitorTelegram: TelegramBot
    {
        public MonitorTelegram(string api, long id) : base(api, id)
        {

        }

        void sendHelp()
        {
            string help = string.Format("*** 호출 메뉴는 아래와 같습니다. ***\n");
            help += string.Format("확인 /상황 : 현재 상황 캡쳐해서 보기\n");
            help += string.Format("수익 : 금일 HTS 수익 조회\n");

            help += string.Format("[금일 총 이익금 : {0:#,###0.##} $]\n", Program.mainDlg_.server_.todayProfitSum());
            help += string.Format("\n[시간 [{0}]]", DateTime.Now);
            this.sendMessage(help);
        }

        public override void doOrder(string message)
        {
            var dlg = Program.mainDlg_;
            dlg.caputreForm();

            // eco 서버, 여기서 메시지 받았을때 switch 분기를 하면 됨.
            if (message.StartsWith("확인") || message.StartsWith("상황")) {
                string imageFile = dlg.captureFormImgName();
                this.sendPhoto(imageFile, string.Format("{0} 시각 상황", DateTime.Now));
                return;
            }

            this.sendHelp();
        }
    }
}
