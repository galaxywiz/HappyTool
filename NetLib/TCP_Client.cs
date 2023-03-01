using System;
using System.Text;
using UtilLibrary;

namespace NetLibrary
{
    public class TCP_Client: NetCore
    {
        public TCP_Client()
        {
        }

        public bool isConnected()
        {
            if (!mainSock_.Connected) {
                return false;
            }
          
            return true;
        }

        User user_;
        string ip_;
        int port_;
        public bool onConnect(string ip, int port)
        {
            try {
                if (this.isConnected()) {
                    Logger.getInstance.print(KiwoomCode.Log.에러, "이미 연결되어 있습니다!");
                    return false;
                }

                ip_ = ip;
                port_ = port;

                mainSock_.Connect(ip_, port_);
                Logger.getInstance.print(KiwoomCode.Log.주식봇, "서버와 연결되었습니다.");

                // 연결 완료, 서버에서 데이터가 올 수 있으므로 수신 대기한다.
                user_ = new User();
                user_.socket_ = mainSock_;
                mainSock_.BeginReceive(user_.buffer_, 0, user_.bufferSize_, 0, onReceive, user_);
                return true;
            }
            catch {
                mainSock_.Close();
                this.makeSocket();
                Logger.getInstance.print(KiwoomCode.Log.에러, "소켓 연결 에러, 삭제하고 다시 소켓 생성");
            }            
            return false;
        }

        public void onClose()
        {
            try {
                this.onSendData(new ClosePacket());
                this.onClose(user_);
            }
            catch (Exception e) {
                Logger.getInstance.print(KiwoomCode.Log.에러, "패킷 종료 에러 / {0}/{1}", e.Message, e.StackTrace);
            }
        }

        void onReceive(IAsyncResult ar)
        {
            try {
                // BeginReceive에서 추가적으로 넘어온 데이터를 AsyncObject 형식으로 변환한다.
                User user = (User) ar.AsyncState;

                // 데이터 수신을 끝낸다.
                int received = user.socket_.EndReceive(ar);

                // 받은 데이터가 없으면(연결끊어짐) 끝낸다.
                if (received <= 0) {
                    user.socket_.Close();
                    return;
                }

                user.process(this);

                // 클라이언트에선 데이터를 전달해줄 필요가 없으므로 바로 수신 대기한다.
                // 데이터를 받은 후엔 다시 버퍼를 비워주고 같은 방법으로 수신을 대기한다.
                user.clearBuffer();

                // 수신 대기
                user.socket_.BeginReceive(user.buffer_, 0, user.bufferSize_, 0, onReceive, user);
            } catch (Exception e) {
                Logger.getInstance.print(KiwoomCode.Log.에러, "패킷 받기 에러 / {0}/{1}", e.Message, e.StackTrace);
            }
        }

        public void onSendData(Packet packet)
        {
            try {
                // 서버가 대기중인지 확인한다.
                if (!mainSock_.IsBound) {
                    Logger.getInstance.print(KiwoomCode.Log.에러, "서버 접속 대기!");
                    mainSock_.Close();
                    return;
                }

                if (this.isConnected() == false) {
                    Logger.getInstance.print(KiwoomCode.Log.에러, "서버가 실행되고 있지 않습니다!");
                }
                // 서버에 전송한다.
                mainSock_.Send(packet.toBytes());
            } catch (Exception e) {
                Logger.getInstance.print(KiwoomCode.Log.에러, "패킷 전송에 에러 / {0}/{1}", e.Message, e.StackTrace);
            }
        }

        public void onSendString(string packet)
        {
            try {
                // 서버가 대기중인지 확인한다.
                if (!mainSock_.IsBound) {
                    Logger.getInstance.print(KiwoomCode.Log.에러, "서버 접속 대기!");
                    mainSock_.Close();
                    return;
                }

                if (this.isConnected() == false) {
                    Logger.getInstance.print(KiwoomCode.Log.에러, "서버가 실행되고 있지 않습니다!");
                }
                // 서버에 전송한다.
                mainSock_.Send(Encoding.UTF8.GetBytes(packet));
            } catch (Exception e) {
                Logger.getInstance.print(KiwoomCode.Log.에러, "패킷 전송에 에러 / {0}/{1}", e.Message, e.StackTrace);
            }
        }
    }
}
