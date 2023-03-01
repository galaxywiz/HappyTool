using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UtilLibrary;

namespace NetLibrary
{
    // 어렵게 안하기
    public class Packet
    {
        protected const char PACKET_SPLIT = '\x01';
        public string packetString_;
        public Packet()
        {
            packetString_ = "";
        }

        public Packet(byte[] packetBuffer)
        {
            this.toPacket(packetBuffer);
        }

        public void toPacket(byte[] packetBuffer)
        {
            packetString_ = Encoding.UTF8.GetString(packetBuffer);
        }

        public byte[] toBytes()
        {
            if (packetString_.Length == 0) {
                return null;
            }

            // 문자열을 utf8 형식의 바이트로 변환한다.
            return Encoding.UTF8.GetBytes(packetString_);
        }

        protected void encodeHeader(PACKET_NAME header)
        {
            packetString_ = header.ToString();
        }

        public void encodeData(string data)
        {
            if (packetString_.Length == 0) {
                packetString_ = data;
            }
            packetString_ = string.Format("{0}{1}{2}", packetString_, PACKET_SPLIT, data);
        }

        public string[] decodeData()
        {
            return packetString_.Split(PACKET_SPLIT);
        }

        public virtual void encodePacket() { }
        public virtual void decodePacket() { }

        protected double parseDouble(string s)
        {
            if (s == "") {
                return 0;
            }
            return double.Parse(s);
        }
        protected int parseInt(string s)
        {
            if (s == "") {
                return 0;
            }
            return int.Parse(s);
        }
        protected bool parseBool(string s)
        {
            if (s == "") {
                return false;
            }
            return bool.Parse(s);
        }
    }

    public enum PACKET_NAME
    {
        REPORT,
        HEART_BEAT,
        WORKING,
        NOW_WORKING,
        CLOSE,
    }

    public class ReportPacket: Packet
    {
        public string serverName_;
        public double account_ = 0;         // 예수금
        public int winCount_ = 0;           // 승리횟수
        public int tradingCount_ = 0;       // 거래횟수
        public double todayProfit_ = 0;     // 금일 이익금
        public string strategyName_ = "";    // 전략 이름
        public string stateName_ = "";        // 봇 상태
        public bool nowWorking_ = false;        // 현재 일하는 중?

        public bool isError_ = false;

        public ReportPacket(string packetBuffer)
        {
            packetString_ = packetBuffer;
            this.decodePacket();
        }

        public ReportPacket(string serverName, double account, int win, int trading, double profit, string strategy, string state, bool working)
        {
            this.encodeHeader(PACKET_NAME.REPORT);
            serverName_ = serverName;
            account_ = account;
            winCount_ = win;
            tradingCount_ = trading;
            todayProfit_ = profit;
            strategyName_ = strategy;
            stateName_ = state;
            nowWorking_ = working;
            this.encodePacket();
        }

        public override void encodePacket()
        {
            this.encodeData(serverName_);
            this.encodeData(account_.ToString("##.##"));
            this.encodeData(winCount_.ToString());
            this.encodeData(tradingCount_.ToString());
            this.encodeData(todayProfit_.ToString("##.##"));
            this.encodeData(strategyName_);
            this.encodeData(stateName_);
            this.encodeData(nowWorking_.ToString());
        }

        public override void decodePacket()
        {
            string[] data = this.decodeData();
            int idx = 1;
            try {
                serverName_ = data[idx++];
                account_ = this.parseDouble(data[idx++]);
                winCount_ = this.parseInt(data[idx++]);
                tradingCount_ = this.parseInt(data[idx++]);
                todayProfit_ = this.parseDouble(data[idx++]);
                strategyName_ = data[idx++];
                stateName_ = data[idx++];
                nowWorking_ = this.parseBool(data[idx++]);
            }
            catch (Exception e) {
                isError_ = true;
                Logger.getInstance.print(KiwoomCode.Log.에러, "report packet {0} 에러 {1}", e.Message, e.StackTrace);
            }
        }
    }

    public class HeartBeatPacket: Packet
    {
        public HeartBeatPacket()
        {
            this.encodeHeader(PACKET_NAME.HEART_BEAT);
        }
        public override void decodePacket()
        {
        }
    }

    public class ClosePacket: Packet
    {
        public ClosePacket()
        {
            this.encodeHeader(PACKET_NAME.CLOSE);
        }
        public override void decodePacket()
        {
        }
    }
}
