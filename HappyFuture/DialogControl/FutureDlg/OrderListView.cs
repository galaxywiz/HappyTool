using StockLibrary;
using System;
using System.Windows.Forms;
using UtilLibrary;

namespace HappyFuture.DialogControl.FutureDlg
{
    class OrderListView
    {
        readonly ListView listView_;           // 주문걸린 것들
        public OrderListView()
        {
            var futureDlg = Program.happyFuture_.futureDlg_;
            this.listView_ = futureDlg.listView_OrderView;
        }

        enum COLUMNS_NAME
        {
            선물명,
            선물코드,
            주문시간,
            포지션,
            채결갯수,
            주문갯수,
            주문번호,
        }
        //-------------------------------------------------------------//
        // 구입한 주식 리스트
        public void setup()
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

        public void addInfo(FutureData futureData, string datetime, TRADING_STATUS buyInfo, string buyType, string orderCount, string orderNumber)
        {
            ListViewItem lvi = new ListViewItem(futureData.name_);
            lvi.SubItems.Add(futureData.code_);             // 1
            lvi.SubItems.Add(datetime);                     // 2
            lvi.SubItems.Add(buyInfo.ToString());           // 3
            lvi.SubItems.Add(buyType);                      // 4
            int count;
            if (int.TryParse(orderCount, out count)) {
                lvi.SubItems.Add(count.ToString());             // 5
            }
            else {
                lvi.SubItems.Add(orderCount);
            }

            int number;
            if (int.TryParse(orderNumber, out number)) {
                lvi.SubItems.Add(number.ToString());             // 6
            }
            else {
                lvi.SubItems.Add(orderNumber);
            }
            this.add(lvi);
        }

        void expire()
        {
            var bot = ControlGet.getInstance.futureBot();
            var telegram = bot.telegram_;
            var engine = bot.engine_;
            DateTime now = DateTime.Now;
            try {
                foreach (ListViewItem item in this.listView_.Items) {
                    string code = item.SubItems[(int) COLUMNS_NAME.선물코드].Text;
                    FutureData futureData = bot.getStockDataCode(code) as FutureData;
                    if (futureData == null) {
                        continue;
                    }
                    // 그사이 체결된거면 패스
                    if (futureData.isBuyedItem()) {
                        continue;
                    }

                    //DateTime buyTime = futureData.lastChejanDate_;
                    //11/16 08:11:22 이런 형식임
                    // var dateStr = item.SubItems[(int) COLUMNS_NAME.주문시간].Text;
                    // buyTime = DateTime.ParseExact(dateStr, "MM/dd hh:mm:ss", CultureInfo.InvariantCulture);

                    // 원래 5분 기다리는데, 백테스팅 할땐 이게 변수로 작용할 수 있으므로,
                    // 다음 분봉까지 체결안되면 바로 주문 취소.
                    //   const int EXPIRE_MIN = 5;
                    //             if (buyTime.AddMinutes(EXPIRE_MIN) < now) {
                    // 주문 취소
                    TradingStatement statement = null;
                    if (item.SubItems[(int) COLUMNS_NAME.포지션].Text.StartsWith(TRADING_STATUS.매수.ToString())) {
                        statement = new BuyCancleFuture(futureData);
                    }
                    else {
                        statement = new SellCancleFuture(futureData);
                    }
                    var orderCount = int.Parse(item.SubItems[(int) COLUMNS_NAME.주문갯수].Text);
                    statement.tradingCount_ = orderCount;

                    var orderNumber = item.SubItems[(int) COLUMNS_NAME.주문번호].Text;
                    Int64 on2;
                    if (Int64.TryParse(futureData.orderNumber_, out on2)) {
                        if (on2.ToString() != orderNumber) {
                            orderNumber = on2.ToString();
                        }
                    }
                    statement.orderNumber_ = orderNumber;
                    engine.addOrder(statement);

                    string log = string.Format("{0}[{1}] 항목, 캔들 갱신될동안 체결이 안됨. 주문 취소\n", futureData.name_, code);
                    log += string.Format("주문번호: {0}\n", orderNumber);
                    log += string.Format("원주문번호: {0}\n", futureData.orderNumber_);

                    Logger.getInstance.print(KiwoomCode.Log.주식봇, log);
                    telegram.sendMessage(log);
                }
                //       }
            }
            catch (Exception e) {
                string log = string.Format("체결 대기창 에러: {0}\n {1}", e.Message, e.StackTrace);
                Logger.getInstance.print(KiwoomCode.Log.에러, log);
                telegram.sendMessage(log);
            }
        }
        public void checkExpired()
        {
            if (this.listView_.InvokeRequired) {
                this.listView_.BeginInvoke(new Action(() => this.expire()));
            }
            else {
                this.expire();
            }
        }
    }
}
