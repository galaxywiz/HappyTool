using NetLibrary;
using StockLibrary;
using System.Threading;
using UtilLibrary;

namespace HappyFuture.NetClient
{
    public class FutureMonitorClient : TCP_Client
    {
        Thread thread_;
        bool active_ = false;
        public FutureMonitorClient()
        {
            this.start();
        }
        ~FutureMonitorClient()
        {
            this.stop();
        }

        public void stop()
        {
            active_ = false;
        }

        public void start()
        {
            if (active_) {
                return;
            }
            thread_ = new Thread(process);
            thread_.Start();
        }
                
        void process()
        {
            active_ = true;

            while (active_) {
                Thread.Sleep(1000 * 10);
                if (this.isConnected() == false) {
                    if (this.onConnect(PublicVar.monitorIp_, PublicVar.monitorPort_) == false) {
                        if (Program.happyFuture_.futureDlg_.checkBox_doTrade.Checked == false) {
                            Logger.getInstance.print(KiwoomCode.Log.주식봇, "시뮬레이션 할 거이므로 행복의 모니터 접속 종료");
                            break;
                        }
                        Logger.getInstance.print(KiwoomCode.Log.에러, "행복의 모니터[{0}]로 연결 시도", PublicVar.monitorIp_);
                        continue;
                    }
                }
               
            }
            this.onClose();
        }

        public void requestTrade(StockData stockData, Bot bot)
        {
            var requestPacket = string.Format("{0},{1},{2},{3:##.##}",
                "TRADE_REQUEST",
                stockData.code_,
                bot.priceTypeMin(),
                bot.accountMoney_);

            this.onSendString(requestPacket);
        }
    }
}
