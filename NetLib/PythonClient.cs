using System;
using System.Net.Sockets;
using System.Text;
using UtilLibrary;

namespace NetLibrary
{
    // 일종의 에코 서버
    public class PythonClient
    {
        string ip_ = "127.0.0.1";
        int port_ = 30000;
        int PACKET_SIZE = 1024;

        public PythonClient(string ip, int port)
        {
            ip_ = ip;
            port_ = port;
        }

        ~PythonClient()
        {
        }

        public bool ableRequest()
        {
            var tc = new TcpClient(ip_, port_);
            if (tc.Connected) {
                return false;
            }
            tc.Close();
            return true;
        }

        public string requestPacket(string packet)
        {
            string output = "";
            var tc = new TcpClient(ip_, port_);
            if (tc.Connected) {
                try {
                    NetworkStream stream = tc.GetStream();
                    byte[] buff = Encoding.ASCII.GetBytes(packet);
                    stream.Write(buff, 0, buff.Length);

                    byte[] outbuf = new byte[PACKET_SIZE];
                    int nbytes = stream.Read(outbuf, 0, outbuf.Length);
                    output = Encoding.ASCII.GetString(outbuf, 0, nbytes);

                    stream.Close();
                } catch (Exception e) {
                    Logger.getInstance.print(KiwoomCode.Log.에러, "{0}/{1}", e.Message, e.StackTrace);
                }
            }
            tc.Close();
            return output;
        }
    }
}
