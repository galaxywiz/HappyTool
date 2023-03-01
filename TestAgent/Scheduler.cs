using NetLibrary;
using System;
using System.Diagnostics;
using System.Threading;
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
        XmlDocument configXml_ = null;

        public Scheduler()
        {
            configXml_ = new XmlDocument();
        }

        public bool setup()
        {
            string fileName = "D:";//System.IO.Directory.GetCurrentDirectory(); // = Application.StartupPath;
            fileName += "\\StockData.xml";
            try
            {
                configXml_.Load(fileName);
            }
            catch (Exception e)
            {
                configXml_ = null;
                return false;
            }
            return true;
        }

        // 설정값 가지고 오기
        private string getConfig(string attribute, string node)
        {
            if (configXml_ == null)
            {
                return "";
            }
            return configXml_.SelectSingleNode("/HappyAgent/" + attribute + "/@" + node).Value;
        }

        //행복의 도구와 모듈 통신
        AgentClient client_ = null;
        private void tryConnected()
        {
            if (client_ == null)
            {
                string ip = this.getConfig("Network", "IP");
                int port = int.Parse(this.getConfig("Network", "port"));
                client_ = new AgentClient(NetUtil.localIp(), port);
            }
            if (client_.connected() == false)
            {
                client_.connect();
                Thread.Sleep(1000 * 5); // 연결하는 쓰레드가 일할때까지 기다림.
            }
        }

        // 행복의 도구 시작
        private string runPath_ = "";
        private bool runHappyTool()
        {
            if (runPath_.Length == 0)
            {
                runPath_ = this.getConfig("RunInfo", "Path");
            }
            Process.Start(runPath_);
            return true;
        }

        // 주식 시장 여는 평일인지 체크
        private bool isStockMarketDay()
        {
            if (Clock.isTodayWeekDay() == false)
            {
                return false;
            }

            return true;
        }

        // 주식 시장 개장 시각 체크
        private long startTick_ = 0;
        private bool isStockMarketStartTime()
        {
            if (startTick_ == 0)
            {
                string startTime = this.getConfig("RunTime", "StartTime");
                startTick_ = Clock.todayTimeToTick(startTime);
            }
            DateTime now = DateTime.Now;

            if (startTick_ < now.Ticks)
            {
                return true;
            }

            return false;
        }

        private long endTick_ = 0;
        private bool isStockMarketEndTime()
        {
            if (endTick_ == 0)
            {
                string endTime = this.getConfig("RunTime", "EndTime");
                endTick_ = Clock.todayTimeToTick(endTime);
            }
            DateTime now = DateTime.Now;

            if (endTick_ < now.Ticks)
            {
                return true;
            }

            return false;
        }

        //-------------------------------------------------------------//
        // 1초마다 실행
        public void run()
        {
            if (configXml_ == null)
            {
                return;
            }

            if (this.isStockMarketDay() == false)
            {
                return;
            }

            this.tryConnected();
            if (client_.heartBeat() == false)
            {
                Process[] processList = Process.GetProcessesByName("HappyTool");
                foreach(Process process in processList) {
                    process.Kill();
                }

                if (this.isStockMarketEndTime() == false) {
                    this.runHappyTool();
                    // 실행하고 10초 기다림.
                    Thread.Sleep(1000 * 10);
                }

                return;
            }

            // 실행되어있으면
            // 시작할 시간?
            if (this.isStockMarketStartTime())
            {
                if (this.isStockMarketEndTime() == false) {
                    client_.toolStart();
                }
            }
            // 종료할 시간?
            if (this.isStockMarketEndTime())
            {
                client_.toolQuit();
            }
        }
    }
}
