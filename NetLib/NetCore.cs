using System.Net;
using System.Net.Sockets;

namespace NetLibrary
{
    public class NetCore
    {
        protected Socket mainSock_;
        protected IPAddress thisAddress_;

        public NetCore()
        {
            this.makeSocket();
            this.findMyIp();
        }

        protected void makeSocket()
        {
            mainSock_ = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
        }

        void findMyIp()
        {
            IPHostEntry he = Dns.GetHostEntry(Dns.GetHostName());

            // 처음으로 발견되는 ipv4 주소를 사용한다.
            foreach (IPAddress addr in he.AddressList) {
                if (addr.AddressFamily == AddressFamily.InterNetwork) {
                    thisAddress_ = addr;
                    break;
                }
            }

            // 주소가 없다면..
            if (thisAddress_ == null) {
                // 로컬호스트 주소를 사용한다.
                thisAddress_ = IPAddress.Loopback;
            }
        }

        public virtual void removeUser(User user) { }

        public virtual void onClose(User user)
        {
            if (user == null) {
                return;
            }
            if (user.socket_ != null) {
                user.socket_.Close();
            }
        }
    }
}
