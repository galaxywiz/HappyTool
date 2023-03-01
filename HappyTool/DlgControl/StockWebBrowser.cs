using HappyTool.Dlg;
using System.Windows.Forms;

namespace HappyTool.DlgControl
{
    class StockWebBrowser
    {
        WebBrowser webBrowser_ = null;
        string code_ = null;
        public StockWebBrowser(StockChartDlg dlg, string code)
        {
            webBrowser_ = dlg.chart_WebBrowser();
            code_ = code;
            webBrowser_.ScriptErrorsSuppressed = true;

            this.webBrowsing();
        }

        private void webBrowsing()
        {
            // url 형식
            /*http://m.stock.naver.com/item/main.nhn#/stocks/%06d/total */
            //네이버 url 의 # 때문에 웹브라우저에서 안먹힘...
            // 다음을 써야 할듯
            //https://stock.kakao.com/m/stocks/KOREA-A040160

            string url = string.Format("https://stock.kakao.com/m/stocks/KOREA-A{0}", code_);
       //     url = string.Format("https://www.google.com/finance?q=KRX%3A{0}", stockCodeString);
            webBrowser_.Navigate(url);
        }
    }
}
