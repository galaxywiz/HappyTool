using NetLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HappyAgent
{
    class AgentClient : Client
    {
        private Thread thread_;
        private bool shutdown_;

        public AgentClient(string ip, int port) : base(ip, port)
        {
            shutdown_ = false;
            thread_ = new Thread(process);
            thread_.Start();
        }

        ~AgentClient()
        {
            thread_.Join();
        }

        public void process()
        {
            while (!shutdown_) {
                base.connect();
                if (base.connected()) {
                    break;
                }
                Thread.Sleep(1000);
            }

            while (!shutdown_) {
                Thread.Sleep(1);
                // 일단 대기...
            }
        }

        public bool toolStart()
        {
            if (this.connected() == false) {
                return false;
            }

            string message = PublicNetVar.toolStart;
            Packet sendPacket = new Packet();
            sendPacket.encodeData(System.Text.Encoding.Default.GetBytes(message));

            this.send(sendPacket);

            Packet recvPacket = new Packet();
            int len = this.recive(recvPacket);
            if (len < 1) {
                return false;
            }

            byte[] recvData = recvPacket.decodeData();
            string recvMessage = Encoding.Default.GetString(recvData, 0, len);

            if (string.Compare(message, recvMessage) != 0) {
                return false;
            }

            return true;
        }

        public bool toolQuit()
        {
            if (this.connected() == false) {
                return false;
            }

            string message = PublicNetVar.toolQuit;
            Packet sendPacket = new Packet();
            sendPacket.encodeData(System.Text.Encoding.Default.GetBytes(message));

            this.send(sendPacket);

            Packet recvPacket = new Packet();
            int len = this.recive(recvPacket);
            if (len < 1) {
                return false;
            }

            byte[] recvData = recvPacket.decodeData();
            string recvMessage = Encoding.Default.GetString(recvData, 0, len);

            if (string.Compare(message, recvMessage) != 0) {
                return false;
            }

            return true;
        }

        public override bool heartBeat()
        {
            if (this.connected() == false) {
                return false;
            }

            string message = PublicNetVar.heartBeat;
            Packet sendPacket = new Packet();
            sendPacket.encodeData(System.Text.Encoding.Default.GetBytes(message));

            this.send(sendPacket);

            Packet recvPacket = new Packet();
            int len = this.recive(recvPacket);
            if (len < 1) {
                return false;
            }

            byte[] recvData = recvPacket.decodeData();
            string recvMessage = Encoding.Default.GetString(recvData, 0, len);

            if (string.Compare(message, recvMessage) != 0) {
                return false;
            }
            return true;
        }
    }
}
