using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Management;
using System.Net;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace UtilLibrary
{
    public enum GRAPHIC_CARD
    {
        NVIDIA,
        AMD,
        NONE,
    }

    public class Util
    {
        public static GRAPHIC_CARD getGraphicCard()
        {
            ManagementObjectSearcher searcher
                 = new ManagementObjectSearcher("SELECT * FROM Win32_DisplayConfiguration");

            string graphicsCard = string.Empty;
            foreach (ManagementObject mo in searcher.Get()) {
                foreach (PropertyData property in mo.Properties) {
                    if (property.Name == "Description") {
                        graphicsCard = property.Value.ToString();
                    }
                }
            }

            if (graphicsCard.Contains("NVIDIA")) {
                return GRAPHIC_CARD.NVIDIA;
            }
            if (graphicsCard.Contains("AMD")) {
                return GRAPHIC_CARD.AMD;
            }
            return GRAPHIC_CARD.NONE;
        }

        public static DateTime getCreateDate()
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
            return dtBuildDate;
        }

        public static double calcProfitRate(double origin, double now)
        {
            return (now - origin) / origin;
        }

        public static Font getFont(float fontSize, FontStyle style)
        {
            Font font = new Font("font", fontSize, style);
            return font;
        }

        public static long checkRange(long start, long now, long end)
        {
            return Math.Min(end, Math.Max(now, start));
        }

        public static int checkRange(int start, int now, int end)
        {
            return Math.Min(end, Math.Max(now, start));
        }

        public static bool isRange(long start, long now, long end)
        {
            return now == Util.checkRange(start, now, end);
        }

        public static double checkRange(double start, double now, double end)
        {
            return Math.Min(end, Math.Max(now, start));
        }

        public static bool isRange(double start, double now, double end)
        {
            return now == Util.checkRange(start, now, end);
        }

        public static string getMyIP()
        {
            IPHostEntry host = Dns.GetHostByName(Dns.GetHostName());
            string myip = host.AddressList[0].ToString();
            return myip;
        }

        public static string getHostIP()
        {
            string host = new WebClient().DownloadString("http://ipinfo.io/ip");
            host = host.Trim('\n');
            return host;
        }
    }

    public class TimeWatch
    {
        readonly Stopwatch stopWatch_ = new Stopwatch();
        int sec_;
        public TimeWatch(int sec)
        {
            this.setTimeSecond(sec);
            this.stopWatch_.Start();
        }
        public void setTimeSecond(int sec)
        {
            this.sec_ = sec;
        }

        public bool isTimeOver()
        {
            if (this.sec_ < (this.stopWatch_.ElapsedMilliseconds / 1000)) {
                this.stopWatch_.Stop();
                return true;
            }
            return false;
        }

        public bool isOver(int sec)
        {
            if (sec < (this.stopWatch_.ElapsedMilliseconds / 1000)) {
                this.stopWatch_.Stop();
                return true;
            }
            return false;
        }

        public int remainSec()
        {
            return this.sec_ - (int) (this.stopWatch_.ElapsedMilliseconds / 1000);
        }

        public void reset()
        {
            this.stopWatch_.Restart();
        }
    }

    public class Calendar
    {
        public Calendar() { }

        public static bool isTodayWeekDay()
        {
            DateTime now = DateTime.Now;

            switch (now.DayOfWeek) {
                case DayOfWeek.Saturday:
                case DayOfWeek.Sunday:
                return false;
            }
            return true;
        }

        public static bool isKoreanHolyday()
        {
            DateTime now = DateTime.Now;
            // 매년 공휴일은 빼주자
            DateTime[] holiDays = {
                new DateTime(now.Year, 1, 1), new DateTime(now.Year, 3, 1),
                new DateTime(now.Year, 5, 5), new DateTime(now.Year, 6, 6),
                new DateTime(now.Year, 8, 15), new DateTime(now.Year, 10, 3),
                new DateTime(now.Year, 10, 9), new DateTime(now.Year, 12, 25),
                new DateTime(now.Year, 12, 31),
            };
            foreach (DateTime date in holiDays) {
                if (date.ToString("yyyy-MM-dd") == now.ToString("yyyy-MM-dd")) {
                    return true;
                }
            }
            return false;
        }
    }

    public class ControlPrint
    {
        readonly Control control_;
        public ControlPrint(Control control)
        {
            this.control_ = control;
        }

        public void print(string text)
        {
            if (this.control_.InvokeRequired) {
                this.control_.BeginInvoke(new Action(() => this.control_.Text = text));
            }
            else {
                this.control_.Text = text;
            }
        }
    }

    public class ControlCapture
    {
        readonly Control control_;
        public ControlCapture(Control control)
        {
            this.control_ = control;
        }

        public bool formCapture(string fileName)
        {
            try {
                string path = Application.StartupPath + "\\capture";
                DirectoryInfo di = new DirectoryInfo(path);
                if (di.Exists == false) {
                    di.Create();
                }
                fileName = string.Format("{0}\\{1}.png", path, fileName.Replace(".png", ""));

                Bitmap bitmap = new Bitmap(this.control_.Width, this.control_.Height);
                this.control_.DrawToBitmap(bitmap, new Rectangle(0, 0, this.control_.Width, this.control_.Height));
                bitmap.Save(fileName, System.Drawing.Imaging.ImageFormat.Png);
                bitmap.Dispose();
                return true;
            }
            catch (Exception e) {
                Logger.getInstance.print(KiwoomCode.Log.에러, "{0} 캡쳐 하다가 에러, GDI+ 나 쓰레드 뭐시기일듯, {1}", fileName, e.Message);
                return false;
            }
        }

        public string captureForm(string fileName)
        {
            if (this.control_.InvokeRequired) {
                this.control_.BeginInvoke(new Action(() => this.formCapture(fileName)));
            }
            else {
                this.formCapture(fileName);
            }
            return fileName;
        }
    }

    public class QuickDataGridView: DataGridView
    {
        readonly ControlCapture capture_ = null;

        public QuickDataGridView()
        {
            this.capture_ = new ControlCapture(this);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();
            this.DoubleBuffered = true;
        }
        public string captureView(string fileName)
        {
            return this.capture_.captureForm(fileName);
        }
    }

    public class QuickListView: ListView
    {
        readonly ControlCapture capture_ = null;

        public QuickListView()
        {
            this.capture_ = new ControlCapture(this);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();
            this.DoubleBuffered = true;
        }
        public string captureView(string fileName)
        {
            return this.capture_.captureForm(fileName);
        }
    }

    public class QuickChart: Chart
    {
        readonly ControlCapture capture_ = null;

        public QuickChart()
        {
            this.capture_ = new ControlCapture(this);

            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();
            this.DoubleBuffered = true;
        }

        public string captureView(string fileName)
        {
            return this.capture_.captureForm(fileName);
        }
    }
}
