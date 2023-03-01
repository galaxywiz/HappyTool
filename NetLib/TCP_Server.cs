using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UtilLibrary;

namespace NetLibrary
{
    public delegate void doUserEach(User user);

    public abstract class TCP_Server : NetCore
    {
        public TCP_Server()
        {
        }

        public void start(int port = 10900)
        {
            // 서버에서 클라이언트의 연결 요청을 대기하기 위해
            // 소켓을 열어둔다.
            IPEndPoint serverEP = new IPEndPoint(thisAddress_, port);
            mainSock_.Bind(serverEP);
            mainSock_.Listen(10);

            // 비동기적으로 클라이언트의 연결 요청을 받는다.
            mainSock_.BeginAccept(acceptCallback, null);
            Logger.getInstance.print(KiwoomCode.Log.주식봇, "{0}:{1} 로 서버 시작", thisAddress_, port);
        }

        List<User> userPool_ = new List<User>();
        object lock_ = new object();

        void addUser(User user)
        {
            lock (lock_) {
                userPool_.Add(user);
            }
        }
        public override void removeUser(User user)
        {
            lock (lock_) {
                userPool_.Remove(user);
            }
        }

        public void stop()
        {
            userPool_.Clear();
            mainSock_.Close();
        }

        public void doUsers(doUserEach each)
        {
            lock (lock_) {
                foreach(var user in userPool_) {
                    each(user);
                }
            }
        }

        void acceptCallback(IAsyncResult ar)
        {
            try {
                // 클라이언트의 연결 요청을 수락한다.
                Socket client = mainSock_.EndAccept(ar);

                // 또 다른 클라이언트의 연결을 대기한다.
                mainSock_.BeginAccept(acceptCallback, null);

                User user = new User();
                user.socket_ = client;

                // 연결된 클라이언트 리스트에 추가해준다.
                this.addUser(user);
                this.logAccept(user);
                // 텍스트박스에 클라이언트가 연결되었다고 써준다.
                Logger.getInstance.print(KiwoomCode.Log.주식봇, string.Format("클라이언트 (@ {0})가 연결되었습니다.", client.RemoteEndPoint));

                // 클라이언트의 데이터를 받는다.
                client.BeginReceive(user.buffer_, 0, user.bufferSize_, 0, onRecevie, user);
            } catch (Exception e) {
                Logger.getInstance.print(KiwoomCode.Log.에러, "{0}/{1}", e.Message, e.StackTrace);
            }
        }

        protected virtual void logAccept(User user) { }
        protected virtual void logClose(User user) { }

        public override void onClose(User user)
        {
            try {
                this.logClose(user);
                base.onClose(user);
                this.removeUser(user);
            } catch (Exception e) {
                Logger.getInstance.print(KiwoomCode.Log.에러, "{0}/{1}", e.Message, e.StackTrace);
            }
        }

        void onRecevie(IAsyncResult ar)
        {
            // BeginReceive에서 추가적으로 넘어온 데이터를 AsyncObject 형식으로 변환한다.
            User user = (User) ar.AsyncState;
            try {
                // 데이터 수신을 끝낸다.
                int received = user.socket_.EndReceive(ar);

                // 받은 데이터가 없으면(연결끊어짐) 끝낸다.
                if (received <= 0) {
                    this.onClose(user);
                    return;
                }

                this.processPacket(user);

                user.clearBuffer();
                // 수신 대기
                user.socket_.BeginReceive(user.buffer_, 0, user.bufferSize_, 0, onRecevie, user);
            } catch (Exception e) {
                if (user != null) {
                    this.onClose(user);
                }
                Logger.getInstance.print(KiwoomCode.Log.에러, "{0}/{1}", e.Message, e.StackTrace);
            }
        }
        public virtual void processPacket(User user)
        {
            user.process(this);
        }

        public void onSendData(User user, Packet packet)
        {
            // 서버가 대기중인지 확인한다.
            if (!mainSock_.IsBound) {
                Logger.getInstance.print(KiwoomCode.Log.에러, "서버가 실행되고 있지 않습니다!");
                return;
            }

            Socket socket = user.socket_;
            try {
                var bytes = packet.toBytes();
                if (bytes == null) {
                    return;
                }
                socket.Send(bytes);
            }
            catch {
                // 오류 발생하면 전송 취소하고 리스트에서 삭제한다.
                try {
                    socket.Dispose();
                }
                catch { }
                this.onClose(user);
            }
        }

        public void onSendDataToAll(Packet packet)
        {
            // 서버가 대기중인지 확인한다.
            if (!mainSock_.IsBound) {
                Logger.getInstance.print(KiwoomCode.Log.에러, "서버가 실행되고 있지 않습니다!");
                return;
            }

            // 연결된 모든 클라이언트에게 전송한다.
            for (int i = userPool_.Count - 1; i >= 0; i--) {
                User user = userPool_[i];
                this.onSendData(user, packet);
            }
        }
    }
}
