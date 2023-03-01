using HappyTool.Stock;
using StockLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HappyTool.DialogControl.StockDialog
{
    class OrderListView
    {
        readonly ListView listView_;           // 주문걸린 것들
        public OrderListView()
        {
            var stockDlg = Program.happyTool_.stockDlg_;
            this.listView_ = stockDlg.listView_orderStocks;
            this.setup();
        }

        enum COLUMNS_NAME
        {
            주식명,
            주문시간,
            구입가격,
            현재가격,
            채결수량,
            채결금액,
            한주당_이익금,
            이익금,
            현재_손익율
        }
        //-------------------------------------------------------------//
        // 구입한 주식 리스트
        void setup()
        {
            this.listView_.BeginUpdate();
            this.listView_.View = View.Details;
            this.listView_.CheckBoxes = true;

            foreach (COLUMNS_NAME name in Enum.GetValues(typeof(COLUMNS_NAME))) {
                this.listView_.Columns.Add(name.ToString());
            }

            for (int i = 0; i < this.listView_.Columns.Count; ++i) {
                this.listView_.Columns[i].Width = -2;
            }

            this.listView_.EndUpdate();
        }

        public void clearList()
        {
            if (this.listView_.InvokeRequired) {
                this.listView_.BeginInvoke(new Action(() => this.listView_.Items.Clear()));
            }
            else {
                this.listView_.Items.Clear();
            }
        }

        void addItem(ListViewItem lvi)
        {
            this.listView_.BeginUpdate();

            this.listView_.Items.Add(lvi);
            this.listView_.EnsureVisible(this.listView_.Items.Count - 1);

            for (int i = 0; i < this.listView_.Columns.Count; ++i) {
                this.listView_.Columns[i].Width = -2;
            }
            this.listView_.EndUpdate();
        }

        void add(ListViewItem lvi)
        {
            if (this.listView_.InvokeRequired) {
                this.listView_.BeginInvoke(new Action(() => this.addItem(lvi)));
            }
            else {
                this.addItem(lvi);
            }
        }

        void clear()
        {
            if (this.listView_.InvokeRequired) {
                this.listView_.BeginInvoke(new Action(() => listView_.Items.Clear()));
            }
            else {
                listView_.Items.Clear();
            }
        }

        public void updateScreen()
        {
            this.clear();
            StockBot bot = ControlGet.getInstance.stockBot();
            StockDataEach eachDo = (string code, StockData stockData) => {
                if (stockData.isBuyedItem() == false) {
                    return;
                }
                string fmt = "##,###0";
                string date = DateTime.Now.ToString();
                KStockData kStockData = (KStockData) stockData;

                ListViewItem lvi = new ListViewItem(kStockData.name_);
                lvi.SubItems.Add(kStockData.buyTime_.ToString());

                lvi.SubItems.Add(kStockData.buyPrice_.ToString(fmt));
                lvi.SubItems.Add(kStockData.nowPrice().ToString(fmt));
                lvi.SubItems.Add(kStockData.buyCount_.ToString(fmt));
                lvi.SubItems.Add(kStockData.totalBuyPrice().ToString(fmt));

                lvi.SubItems.Add(kStockData.nowOneProfit().ToString(fmt));
                lvi.SubItems.Add(kStockData.nowProfit().ToString(fmt));
                lvi.SubItems.Add((kStockData.nowProfitRate() * 100).ToString("##0.##"));

                this.add(lvi);
            };
            bot.doStocks(eachDo);
        }

        public void sellForce()
        {
            if (listView_.CheckedItems.Count == 0) {
                return;
            }

            StockBot bot = ControlGet.getInstance.stockBot();

            string message = "";
            foreach (ListViewItem item in listView_.CheckedItems) {
                message += string.Format("{0}({1})\n", item.Text, item.SubItems[2].Text);
            }
            message += string.Format("정말 매도 할까요?");
            if ((MessageBox.Show(message, "강제 매도 확인", MessageBoxButtons.YesNo) == DialogResult.Yes)) {
                foreach (ListViewItem item in listView_.CheckedItems) {
                    string code = item.SubItems[2].Text;
                    bot.payOff(code, "강제 청산");
                }
            }
        }
    }
}
