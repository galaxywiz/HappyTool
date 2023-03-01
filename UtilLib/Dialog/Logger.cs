using KiwoomCode;
using System;
using System.IO;
using System.Windows.Forms;

namespace UtilLibrary
{
    public class Logger: SingleTon<Logger>
    {
        private ListView listView_ = null;
        private TextWriter logFile_ = null;
        private string fileName_ = "";
        public bool logActive_;

        public void close()
        {
            this.logActive_ = false;
            this.logFile_.Close();
        }

        public void setup(ListView listView)
        {
            this.listView_ = listView;

            this.listView_.BeginUpdate();
            this.listView_.View = View.Details;

            this.listView_.Columns.Add("로그 종류");
            this.listView_.Columns.Add("시간");
            this.listView_.Columns.Add("메시지");

            this.listView_.Columns[0].Width = -2;
            this.listView_.Columns[1].Width = 150;
            this.listView_.Columns[2].Width = 1000;

            this.listView_.EndUpdate();

            string logPath = Application.StartupPath + "\\log\\";
            DirectoryInfo di = new DirectoryInfo(logPath);
            if (di.Exists == false) {
                di.Create();
            }

            this.fileName_ = logPath + string.Format("log_{0}.txt", DateTime.Now.ToString("yyyyMMddHHmmss"));
            this.logFile_ = new StreamWriter(this.fileName_);
            this.logActive_ = true;
        }

        public string getFileName()
        {
            return this.fileName_;
        }

        void fileWrite(Log type, string log)
        {
            try {
                string date = DateTime.Now.ToString();
                this.logFile_.WriteLine(string.Format("{0},{1},{2}", date, type.ToString(), log));
                this.logFile_.Flush();
            }
            catch (Exception) {

            }
        }

        void printLog(Log type, string log)
        {
            string date = DateTime.Now.ToString();

            ListViewItem lvi = new ListViewItem(type.ToString());
            lvi.SubItems.Add(date);
            lvi.SubItems.Add(log);
            this.listView_.BeginUpdate();

            this.listView_.Items.Add(lvi);
            this.listView_.EnsureVisible(this.listView_.Items.Count - 1);

            this.listView_.EndUpdate();

        }

        // 로그를 출력합니다.
        public void print(Log type, string format, params Object[] args)
        {
            if (this.logActive_ == false) {
                this.consolePrint(format, args);
                return;
            }
            // API조회 로그가 너무 많아서 분석에 방해됨.
            switch (type) {
                case Log.API조회:
                this.consolePrint(format, args);
                return;
                case Log.백테스팅:
                case Log.백테스팅_csv:
                return;
            }

            string message = String.Format(format, args);
            this.fileWrite(type, message);

            if (this.listView_.InvokeRequired) {
                this.listView_.BeginInvoke(new Action(() => this.printLog(type, message)));
            }
            else {
                this.printLog(type, message);
            }
        }

        public void consolePrint(string format, params Object[] args)
        {
            string message = String.Format(format, args);
            Console.WriteLine("{0}\t{1}", DateTime.Now.ToString(), message);
        }
    }
}