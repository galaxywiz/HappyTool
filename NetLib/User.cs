using System;
using System.Net.Sockets;
using UtilLibrary;

namespace NetLibrary
{
    public class User
    {
        const int BUF_SIZE = 4096 * 10;

        public byte[] buffer_;
        public Socket socket_;
        public readonly int bufferSize_;
        protected string name_;

        public User()
        {
            bufferSize_ = BUF_SIZE;
            buffer_ = new byte[bufferSize_];
        }

        public void setName(string name)
        {
            name_ = name;
        }

        public void clearBuffer()
        {
            Array.Clear(buffer_, 0, bufferSize_);
        }

        //==============================================================//

        public string serverNmae_ = "";      // 서버 이름
        public double account_ = 0;         // 예수금
        public int winCount_ = 0;           // 승리횟수
        public int tradingCount_ = 0;       // 거래횟수
        public double todayProfit_ = 0;     // 금일 이익금
        public string strategyName_ = "";    // 전략 이름
        public string stateName_ = "";      // 봇 상태

        public bool nowWorking_ = false;        // 현재 일하는 중?
        public bool nowConnected_ = false;      // 모니터링과 연결중?

        public DateTime lastHeartBeat_ = DateTime.MinValue;

        public void process(NetCore netCore)
        {
            Packet packet = new Packet(buffer_);

            // 0x01 기준으로 짜른다.
            // tokens[0] - 보낸 사람 IP
            // tokens[1] - 보낸 메세지
            string[] packetTokens = packet.decodeData();
            if (packetTokens.Length == 0) {
                netCore.onClose(this);
            }

            // header 에 따른 처리
            string header = packetTokens[0];
            PACKET_NAME head = (PACKET_NAME) System.Enum.Parse(typeof(PACKET_NAME), header);
            switch (head) {
                case PACKET_NAME.REPORT:
                this.packetReport(packet);
                break;

                case PACKET_NAME.WORKING:
                this.packetWoring(packet);
                break;

                case PACKET_NAME.NOW_WORKING:
                this.packetNotWorking(packet);
                break;

                case PACKET_NAME.CLOSE:
                netCore.removeUser(this);
                break;
            }

            lastHeartBeat_ = DateTime.Now;
            //Logger.getInstance.print(KiwoomCode.Log.주식봇, string.Format("[받음]{0}: {1}", socket_.RemoteEndPoint, header));
        }

        void packetReport(Packet packet)
        {
            var report = new ReportPacket(packet.packetString_);
            if (report.isError_) {
                return;
            }

            serverNmae_ = report.serverName_;
            account_ = report.account_;
            winCount_ = report.winCount_;
            tradingCount_ = report.tradingCount_;
            todayProfit_ = report.todayProfit_;
            strategyName_ = report.strategyName_;
            stateName_ = report.stateName_;
            nowWorking_ = report.nowWorking_;
        }

        void packetWoring(Packet packet)
        {
            this.nowWorking_ = true;
        }

        void packetNotWorking(Packet packet)
        {
            this.nowWorking_ = false;
        }
    }
}
