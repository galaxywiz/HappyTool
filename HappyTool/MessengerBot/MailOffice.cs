using HappyTool.Stock;
using StockLibrary;
using System;
using UtilLibrary;

namespace HappyTool.MessengerBot
{
    class MailOffice: SingleTon<MailOffice>
    {
        // 메일 보내기
        string mailTag_ = "[주식봇] ";
        public void sendWatchingStock()
        {
            Mailer mail = new Mailer();
            StockBot bot = ControlGet.getInstance.stockBot();

            mail.setToMailAddr(PublicVar.notifyMail);
            string title = mailTag_ + DateTime.Now.ToString("yyyy년 MM월 dd일") + "감시 리스트";
            mail.setSubject(title);

            string body = string.Format("{0,-16} {1,-10:D6} {2,-10} {3,-10} {4,-10} x {5,-10} = {6,-10}\n\r",
                "주식명", "주식코드", "현재가", "구입여부", "구입갯수", "구입가격", "총 소비비용");
            body += "=======================================================================\n\r";

            StockDataEach eachDo = (string code, StockData stockData) => {
                if (stockData.isBuyedItem()) {
                    KStockData kStockData = (KStockData) stockData;
                    body += string.Format("{0,-16} {1,-10:D6} {2,-10} {3,-10} {4,-10} x {5,-10} = {6,-10}\n\r",
                        kStockData.name_, kStockData.code_, kStockData.nowPrice(), "YES", kStockData.buyCount_, kStockData.buyPrice_, kStockData.totalBuyPrice());
                } else {
                    body += string.Format("{0,-16} {1,-10:D6} {2,-10} {3,-10}\n\r",
                        stockData.name_, stockData.code_, stockData.nowPrice(), "NO");
                }
            };
            bot.doStocks(eachDo);

            mail.setBody(body);
            mail.send();
        }

        public void sendBuyStock(StockData stockData, string bodyLog)
        {
            Mailer mail = new Mailer();
            StockBot bot = ControlGet.getInstance.stockBot();

            mail.setToMailAddr(PublicVar.notifyMail);
            string title = mailTag_ + DateTime.Now.ToString("yyyy년 MM월 dd일 - HH:mm:ss.f") + "주식 매수";
            mail.setSubject(title);
            mail.setBody(bodyLog);
            mail.send();
        }

        public void sendSellStock(KStockData kStockData, string body)
        {
            Mailer mail = new Mailer();
            StockBot bot = ControlGet.getInstance.stockBot();

            mail.setToMailAddr(PublicVar.notifyMail);
            string title = mailTag_ + DateTime.Now.ToString("yyyy년 MM월 dd일 - HH:mm:ss.f") + "주식 매도";
            mail.setSubject(title);

            mail.setBody(body);
            mail.send();
        }

        public void sendLog()
        {
            Mailer mail = new Mailer();

            mail.setToMailAddr(PublicVar.notifyMail);
            string title = mailTag_ + DateTime.Now.ToString("yyyy년 MM월 dd일 - HH:mm:ss.f") + "주식 결과";
            mail.setSubject(title);
            mail.setAttachFile(Logger.getInstance.getFileName());

            string body = "오늘의 로그 첨부";

            mail.setBody(body);
            mail.send();
        }

        public void sendMessage(string message)
        {
            Mailer mail = new Mailer();
            StockBot bot = ControlGet.getInstance.stockBot();

            mail.setToMailAddr(PublicVar.notifyMail);
            string title = mailTag_ + DateTime.Now.ToString("yyyy년 MM월 dd일 - HH:mm:ss.f") + "주식봇 메시지";
            mail.setSubject(title);
            mail.setBody(message);
            mail.send();
        }
    }
}
