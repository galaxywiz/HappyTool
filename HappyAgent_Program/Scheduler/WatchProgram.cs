using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UtilLibrary;

namespace HappyAgent_Program
{
    class WatchProgram
    {
        public string appName_ { get; }
        public bool active_;
        string runDir_;
        public bool offFlag_ = false;

        string newPatchDir_ = string.Empty;
        const string PATCH_DIR = "C:\\Patch";
        public WatchProgram(string appName, string path, bool active)
        {
            appName_ = appName;
            runDir_ = path;
            active_ = active;
            newPatchDir_ = string.Format("{0}\\{1}", PATCH_DIR, appName_);
        }

        public virtual bool isRunningTime()
        {
            return false;
        }

        public bool isRunning()
        {
            Process[] processList = Process.GetProcessesByName(this.appName_);
            if (processList.Length < 1) {
                return false;
            }
            return true;
        }

        public virtual void run()
        {
            string runPath = string.Format("{0}\\{1}.exe", runDir_, appName_);
            Process.Start(runPath);
        }

        public void kill()
        {
            string param = string.Format("- im {0}.exe", this.appName_);
            Process.Start("taskkill", param);
            Thread.Sleep(1000 * 10);
        }

        public bool haveNewPatch()
        {
            string path = PATCH_DIR;
            DirectoryInfo di = new DirectoryInfo(path);
            if (di.Exists == false) {
                di.Create();
            }

            di = new DirectoryInfo(newPatchDir_);
            if (di.Exists == false) {
                di.Create();
                return false;
            }

            string filePath = string.Format("{0}\\{1}.exe", path, appName_);
            if (System.IO.File.Exists(filePath)) {
                return true;
            }
            return false;
        }

        public void patch()
        {
            this.deleteFolder(runDir_);
            this.copyFolder(newPatchDir_, runDir_);
            this.deleteFolder(newPatchDir_);
        }

        // 파일 복사 / 삭제
        public void deleteFolder(string path)
        {
            DirectoryInfo di = new DirectoryInfo(path);
            di.Delete(false);
        }

        void copyFolder(string sourceFolder, string destFolder)
        {
            if (!Directory.Exists(destFolder)) {
                Directory.CreateDirectory(destFolder);
            }

            string[] files = Directory.GetFiles(sourceFolder);
            string[] folders = Directory.GetDirectories(sourceFolder);

            foreach (string file in files) {
                string name = Path.GetFileName(file);
                string dest = Path.Combine(destFolder, name);
                File.Copy(file, dest);
            }

            foreach (string folder in folders) {
                string name = Path.GetFileName(folder);
                string dest = Path.Combine(destFolder, name);
                copyFolder(folder, dest);
            }
        }
    }

    class FutureWatchProgram: WatchProgram
    {
        public FutureWatchProgram(string appName, string path, bool active) : base(appName, path, active)
        {
        }
        public bool isSummerTime()
        {
            DateTime now = DateTime.Now;
            TimeZoneInfo tzf2 = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
            DateTime f2 = TimeZoneInfo.ConvertTime(now, tzf2);
            var isSummer = tzf2.IsDaylightSavingTime(f2);
            return isSummer;
        }

        bool nowStockMarketTime()
        {
            DateTime now = DateTime.Now;
            // 한국시각으로 6~7시 휴장.
            // 썸머타임은 5~6시
            DateTime start = DateTime.Now;
            DateTime end = DateTime.Now;

            if (this.isSummerTime()) {
                start = new DateTime(now.Year, now.Month, now.Day, 6, 0, 0);
                end = new DateTime(now.Year, now.Month, now.Day, 7, 0, 0);
            }
            else {
                start = new DateTime(now.Year, now.Month, now.Day, 7, 0, 0);
                end = new DateTime(now.Year, now.Month, now.Day, 8, 0, 0);
            }

            switch (now.DayOfWeek) {
                case DayOfWeek.Sunday:
                    return false;

                // 토요일 오전 6시까진 함.
                case DayOfWeek.Saturday:
                    if (start < now) {
                        return false;
                    }
                    return true;

                // 월요일 오전 7시부터 시작함.
                case DayOfWeek.Monday:
                    if (now < start) {
                        return false;
                    }
                    return true;
            }

            // 선물은 7시~다음날 시까지임.
            if (Util.isRange(start.Ticks, now.Ticks, end.Ticks) == true) {
                return false;
            }

            return true;
        }
        public override void run()
        {
            
        }

        public override bool isRunningTime()
        {
            return nowStockMarketTime();
        }
    }

    class StockWatchProgram: WatchProgram
    {
        public StockWatchProgram(string appName, string path, bool active) : base(appName, path, active)
        {
        }

        public bool nowStockMarketTime()
        {
            if (Calendar.isTodayWeekDay() == false) {
                return false;
            }

            if (Calendar.isKoreanHolyday()) {
                return false;
            }

            DateTime now = DateTime.Now;
            DateTime start = new DateTime(now.Year, now.Month, now.Day, 9, 0, 0);
            DateTime end = new DateTime(now.Year, now.Month, now.Day, 15, 20, 0);

            if (UtilLibrary.Util.isRange(start.Ticks, now.Ticks, end.Ticks) == true) {
                return true;
            }
            return false;
        }

        public override bool isRunningTime()
        {
            return nowStockMarketTime();
        }
    }


}
