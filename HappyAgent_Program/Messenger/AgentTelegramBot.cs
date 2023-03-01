using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UtilLibrary;

namespace HappyAgent_Program
{
    public class ScreenCopy
    {
        // 사용예: ScreenCopy.Copy("test.png");
        // 
        public static void Copy(string outputFilename)
        {
            try {
                // 주화면의 크기 정보 읽기
                Rectangle rect = Screen.PrimaryScreen.Bounds;
                // 2nd screen = Screen.AllScreens[1]

                // 픽셀 포맷 정보 얻기 (Optional)
                int bitsPerPixel = Screen.PrimaryScreen.BitsPerPixel;
                PixelFormat pixelFormat = PixelFormat.Format32bppArgb;
                if (bitsPerPixel <= 16) {
                    pixelFormat = PixelFormat.Format16bppRgb565;
                }
                if (bitsPerPixel == 24) {
                    pixelFormat = PixelFormat.Format24bppRgb;
                }

                // 화면 크기만큼의 Bitmap 생성
                Bitmap bmp = new Bitmap(rect.Width, rect.Height, pixelFormat);

                // Bitmap 이미지 변경을 위해 Graphics 객체 생성
                using (Graphics gr = Graphics.FromImage(bmp)) {
                    // 화면을 그대로 카피해서 Bitmap 메모리에 저장
                    gr.CopyFromScreen(rect.Left, rect.Top, 0, 0, rect.Size);
                }

                // Bitmap 데이타를 파일로 저장
                bmp.Save(outputFilename, ImageFormat.Jpeg);
                bmp.Dispose();
            } catch(Exception e) {

            }
        }
    }

    class AgentTelegramBot : TelegramBot
    {
        public AgentTelegramBot(string api, long id) : base(api, id)
        {
        }

        void sendHelp()
        {
            string help = string.Format("*** 호출 메뉴는 아래와 같습니다. ***\n");
            help += string.Format("확인 / 상태 : 현재 상황 \n");
            help += string.Format("옵션 : 현재 옵션 상황\n");
            help += string.Format("중지 [appName] : 해당 어플 중지\n");
            help += string.Format("실행 [appName] : 해당 어플 실행시키기\n\n");

            help += string.Format("[현재 서버 시간 [{0}]]\n", DateTime.Now);
            var scheduler = HappyAgent.getInstance.sheduler_;
            if (scheduler != null) {
                help += string.Format("{0}", scheduler.logAppList());
            }
            this.sendMessage(help);
        }

        void optionList()
        {
            var scheduler = HappyAgent.getInstance.sheduler_;
            if (scheduler != null) {
                this.sendMessage(scheduler.logAppOptList());
            }
        }

        void stopApp(string message)
        {
            string[] order = message.Split(' ');
            if (order.Length < 2) {
                return;
            }
            string app = order[1];
            var scheduler = HappyAgent.getInstance.sheduler_;
            if (scheduler == null) {
                return;
            }

            string log = string.Empty;
            if (scheduler.stopApp(app)) {
                log = string.Format("{0} 프로그램 정지", app);
            } else {
                log = string.Format("{0} 프로그램 정지 실패함", app);
            }
            this.sendMessage(log);
        }

        void runApp(string message)
        {
            string[] order = message.Split(' ');
            if (order.Length < 2) {
                return;
            }
            string app = order[1];
            var scheduler = HappyAgent.getInstance.sheduler_;
            if (scheduler == null) {
                return;
            }

            string log = string.Empty;
            if (scheduler.runApp(app)) {
                log = string.Format("{0} 프로그램 시작", app);
            }
            else {
                log = string.Format("{0} 프로그램 시작 실패함", app);
            }
            this.sendMessage(log);
        }

        void capture()
        {
            string image = "Capture.jpg";
            ScreenCopy.Copy(image);
            this.sendPhoto(image, string.Format("현재시각: [{0}]", DateTime.Now));
        }

        public override void doOrder(string message)
        {
            if (message.StartsWith("옵션")) {
                this.optionList();
            }
            if (message.StartsWith("중지")) {
                this.stopApp(message);
            }
            if (message.StartsWith("실행")) {
                this.runApp(message);
            }
            if (message.StartsWith("확인") || message.StartsWith("상태")) {
                this.capture();
            }

            this.sendHelp();
        }
    }
}
