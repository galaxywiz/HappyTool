using HappyAgent_Program;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using UtilLibrary;

namespace HappyAgent
{
    class Scheduler
    {
        /*  1. 클라이언트에 ping을 쏘고
            2. 연결이 끊어지거나, 답이 없으면, kill 명령어를 쓴다
            3. 3초뒤 다시 프로그램 올리고
            4. 5초뒤 프로그램 실행, 커넥 확인

            이 과정이 5회 이상이면 off 시킴
        */
        AgentTelegramBot telegram_ = null;
        List<WatchProgram> lists_ = new List<WatchProgram>();

        public Scheduler()
        {
            Microsoft.Win32.SystemEvents.SessionEnded += new Microsoft.Win32.SessionEndedEventHandler(SystemEvents_SessionEnded);

            this.listUp();
        }

        void SystemEvents_SessionEnded(object sender, Microsoft.Win32.SessionEndedEventArgs e)
        {
            string log = string.Format("### 서버 종료 감지 ###\n");
            log += getServerInfo();
            telegram_.sendMessage(log);

            Process.Start("shutdown.exe", "-a"); 
        }

        public void sendShutdownLog()
        {
            var log = string.Format("행복의 감시자가 종료 됩니다.");
            telegram_.sendMessage(log);
            while (telegram_.messageCount() > 0) {
                Thread.Sleep(1000);
            }
        }

        void listUp()
        {
            var dt = CsvParser.getDataTableFromCsv(".\\happyList.csv");
            foreach (DataRow row in dt.Rows) {
                var program = (string) row[0];
                var path = (string) row[1];
                var flagStr = (string) row[2];
                bool flag = flagStr.ToLower().Contains("true") ? true : false;

                WatchProgram watch = null;
                if (program.ToLower().Contains("future")) {
                    watch = new FutureWatchProgram(program, path, flag);
                }
                else if (program.ToLower().Contains("tool")) {
                    watch = new StockWatchProgram(program, path, flag);
                }
                else {
                    continue;
                }
                lists_.Add(watch);
            }
        }

        string getServerInfo()
        {
            string log = string.Format(" - 현재 시각 : {0}\n", DateTime.Now);
            log += string.Format(" - 서버 이름 : {0}\n", SystemInformation.ComputerName);
            log += string.Format(" - 서버 주소 : {0}\n", UtilLibrary.Util.getHostIP());
            return log;
        }

        public void setup()
        {
            string api = "719654159:AAHZK3EmBTa5wVJ699GRcdDMwda6bdxKnCU";
            long id = 508897948;
            telegram_ = new AgentTelegramBot(api, id);
            telegram_.start();

            string log = string.Format("$ 행복의 감시자 시작 $\n");
            log += getServerInfo();
            telegram_.sendMessage(log);
        }

        //-------------------------------------------------------------//
        // 1분 마다 실행
        public void run()
        {
            foreach (var program in lists_) {
                if (program.active_ == false) {
                    continue;
                }

                if (program.isRunningTime() == false) {
                    continue;
                }

                if (program.isRunning() == false) {
                    string log;
                    if (program.haveNewPatch()) {
                        program.patch();
                        program.run();
                        log = string.Format("* [{0}] 의 새 버젼 발견하여 패치", program.appName_);
                        telegram_.sendMessage(log);
                    }
                    else {
                        log = string.Format("* [{0}] 이 활동하는 시간인데 off 입니다.\n", program.appName_);
                        log += string.Format(" - 프로그램 재가동 해야 합니다.\n");
                        log += string.Format(" - 서버 이름 : {0}\n", SystemInformation.ComputerName);
                        telegram_.sendMessage(log);
                        program.run();
                        program.offFlag_ = true;
                    }
                }
                else {
                    if (program.offFlag_) {
                        string log = string.Format("* [{0}] 이 재가동 된 것을 확인 했습니다.\n", program.appName_);
                        log += string.Format(" - 서버 이름 : {0}\n", SystemInformation.ComputerName);
                        telegram_.sendMessage(log);
                        program.offFlag_ = false;
                    }
                }
            }
            Thread.Sleep(1000 * 60);
        }

        WatchProgram getWatch(string appName)
        {
            appName = appName.ToLower();
            foreach (var program in lists_) {
                string watchApp = program.appName_.ToLower();
                if (watchApp == appName) {
                    return program;
                }
            }
            return null;
        }

        public string logAppOptList()
        {
            string ret = "";
            foreach (var program in lists_) {
                ret += string.Format("- [{0}], 활성: {1}\n", program.appName_, program.active_);
            }
            return ret;
        }

        public string logAppList()
        {
            string ret = "";
            foreach (var program in lists_) {
                var act = program.isRunning();
                if (act) {
                    ret += string.Format("- [{0}] 일하는중\n", program.appName_);
                } else {
                    ret += string.Format("- [{0}] Off 상태\n", program.appName_);
                }
            }
            return ret;
        }

        public bool stopApp(string appName)
        {
            var program = this.getWatch(appName);
            if (program == null) {
                return false;
            }
            program.active_ = false;
            program.kill();
            return true;
        }

        public bool runApp(string appName)
        {
            var program = this.getWatch(appName);
            if (program == null) {
                return false;
            }
            program.active_ = true;
            program.run();
            return true;
        }
    }
}
