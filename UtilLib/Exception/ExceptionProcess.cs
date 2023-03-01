using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace UtilLibrary
{
    public class ExceptionProcess: SingleTon<ExceptionProcess>
    {
        string corePath_;
        public ExceptionProcess()
        {
            this.setCorePath("core");
        }
        public void setCorePath(string path)
        {
            this.corePath_ = path;
        }

        public void remainStackFile(Exception error)
        {
            DirectoryInfo di = new DirectoryInfo(this.corePath_);
            if (di.Exists == false) {
                di.Create();
            }
            TextWriter stackFile = null;
            string fileName = this.corePath_ + string.Format("\\core_{0}.txt", DateTime.Now.ToString("yyyyMMddHHmmss"));

            stackFile = new StreamWriter(fileName);
            stackFile.WriteLine("module : " + error.Source);
            stackFile.WriteLine("message : " + error.Message);
            stackFile.WriteLine("*************** stack trace ***************");
            stackFile.WriteLine(error.StackTrace.ToString());

            stackFile.Flush();
            stackFile.Close();
        }

        private int dumpFileCount()
        {
            int count = 0;
            if (Directory.Exists(this.corePath_)) {
                string[] fileEntries = Directory.GetFiles(this.corePath_);
                foreach (string file in fileEntries) {
                    count++;
                }
            }
            return count;
        }
        const int MAX_CORE_FILE = 5;

        private void dumpFileShake()
        {
            if (Directory.Exists(this.corePath_)) {

                var dumpFiles = new Dictionary<long, string>();

                string[] fileEntries = Directory.GetFiles(this.corePath_);
                foreach (string file in fileEntries) {
                    FileInfo info = new FileInfo(file);
                    if (string.Compare(info.Extension, ".dmp") != 0) {
                        continue;
                    }
                    dumpFiles.Add(info.CreationTime.Ticks, file);
                }

                int index = 0;
                int indexMax = dumpFiles.Count;
                foreach (var item in dumpFiles.OrderBy(i => i.Key)) {
                    string file = item.Value;
                    File.Delete(file);
                    ++index;
                    if (index > indexMax) {
                        break;
                    }
                }
            }
        }

        public void remainDumpFile()
        {
            int pid = Process.GetCurrentProcess().Id;

            DirectoryInfo di = new DirectoryInfo(this.corePath_);
            if (di.Exists == false) {
                di.Create();
            }
            if (this.dumpFileCount() > MAX_CORE_FILE) {
                this.dumpFileShake();
            }

            MinidumpWriter.MakeDump(this.corePath_ + string.Format("\\core_{0}.dmp", DateTime.Now.ToString("yyyyMMddHHmmss")), pid);
        }

        public void sendStackMail(string address, Exception error)
        {
            Mailer mail = new Mailer();

            mail.setToMailAddr(address);
            string title = "[에러] " + DateTime.Now.ToString("yyyy년 MM월 dd일 - HH:mm:ss.f") + "주식봇 다운";
            mail.setSubject(title);

            string errorStack = "module : " + error.Source + "\n";
            errorStack = errorStack + "message : " + error.Message + "\n";
            errorStack = errorStack + "stack trace\n" + error.StackTrace.ToString();

            mail.setBody(errorStack);
            mail.send();
        }
    }
}
