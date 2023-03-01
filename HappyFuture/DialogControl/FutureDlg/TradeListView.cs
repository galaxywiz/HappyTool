using StockLibrary;
using StockLibrary.StrategyManager.ProfitSafer;
using System;
using System.Windows.Forms;
using UtilLibrary;

namespace HappyFuture.DialogControl.FutureDlg
{
    class TradeListView
    {
        readonly ListView listView_;           // 매매중인 주식
        readonly ControlPrint totalProfit_;
        readonly ControlPrint totalBuyCount_;
        readonly ControlPrint totalPureProfit_;

        public TradeListView()
        {
            var futureDlg = Program.happyFuture_.futureDlg_;
            this.listView_ = futureDlg.listView_tradeStock;

            this.totalProfit_ = new ControlPrint(futureDlg.label_totalProfit);
            this.totalBuyCount_ = new ControlPrint(futureDlg.label_totalCount);
            this.totalPureProfit_ = new ControlPrint(futureDlg.label_totalPureProfit);
        }

        //-------------------------------------------------------------//
        // 구입한 주식 리스트
        public void setup()
        {
            this.listView_.BeginUpdate();
            this.listView_.View = View.Details;
            this.listView_.CheckBoxes = true;

            this.listView_.Columns.Add("선물명");
            this.listView_.Columns.Add("선물코드");
            this.listView_.Columns.Add("포지션");

            this.listView_.Columns.Add("구입가격");
            this.listView_.Columns.Add("현재가격");
            this.listView_.Columns.Add("틱차이");

            this.listView_.Columns.Add("채결수량");
            this.listView_.Columns.Add("보유시간(분)");
            this.listView_.Columns.Add("1주당 최저이익($)");
            this.listView_.Columns.Add("1주당 최고이익($)");

            this.listView_.Columns.Add("1주당 기대수익($)");
            this.listView_.Columns.Add("이익금(1주당)($)");
            this.listView_.Columns.Add("순이익금 ($)");
            for (int i = 0; i < this.listView_.Columns.Count; ++i) {
                this.listView_.Columns[i].Width = -2;
            }

            this.listView_.EndUpdate();
        }

        void updateItems()
        {
            this.listView_.Items.Clear();

            double totalProfit = 0;
            int sumBuyCount = 0;
            FutureBot bot = ControlGet.getInstance.futureBot();
            StockDataEach eachDo = (string code, StockData stockData) => {
                if (stockData.isBuyedItem() == false) {
                    return;
                }
                FutureData futureData = (FutureData) stockData;
                if (futureData.nowPrice() <= 0.0f) {
                    return;
                }
                string date = futureData.nowDateTime().ToString();
                string fmt = "##,###0.#####";
                ListViewItem lvi = new ListViewItem(futureData.name_);
                lvi.SubItems.Add(futureData.code_);
                lvi.SubItems.Add(futureData.position_.ToString());

                lvi.SubItems.Add(futureData.buyPrice_.ToString(fmt));
                lvi.SubItems.Add(futureData.nowPrice().ToString(fmt));
                var tickSub = futureData.buyPrice_ - futureData.nowPrice();
                lvi.SubItems.Add(tickSub.ToString(fmt));

                lvi.SubItems.Add(futureData.buyCount_.ToString(fmt));
                lvi.SubItems.Add(futureData.positionHaveMin().ToString());

                lvi.SubItems.Add(futureData.minOneProfit_.ToString(fmt));
                lvi.SubItems.Add(futureData.maxProfit_.ToString(fmt));

                if (futureData.tradeModule() != null) {
                    lvi.SubItems.Add(futureData.tradeModule().avgProfit_.ToString(fmt));
                }
                else {
                    lvi.SubItems.Add("");
                }
                sumBuyCount += futureData.buyCount_;
                var profit = futureData.nowProfit();
                if ((int) tickSub != 0) {
                    if ((int) profit == 0) {
                        string log = string.Format("*** {0} 이익 계산 이상\n", futureData.name_);
                        log += string.Format("-> 포지션 {0}, 갯수 {1}\n", futureData.position_, futureData.buyCount_);
                        log += string.Format("-> 틱 차이 {0}\n", tickSub);
                        log += string.Format("-> 구입가 {0}, 현재가 {1}\n", futureData.buyPrice_, futureData.nowPrice());
                        log += string.Format("-> tickValue {0}, tickStep {1}\n", futureData.tickValue_, futureData.tickSize_);

                        Logger.getInstance.print(KiwoomCode.Log.에러, log);
                        bot.telegram_.sendMessage(log);

                        bot.engine_.requestFutureInfo(futureData.code_);
                    }
                }
                totalProfit += profit;

                var pureProfit = profit - (futureData.buyCount_ * PublicFutureVar.pureFeeTex);
                lvi.SubItems.Add(string.Format("{0}({1})", profit.ToString(fmt), futureData.nowOneProfit().ToString(fmt)));
                lvi.SubItems.Add(pureProfit.ToString(fmt));
                {
                    this.listView_.BeginUpdate();

                    this.listView_.Items.Add(lvi);
                    this.listView_.EnsureVisible(this.listView_.Items.Count - 1);

                    for (int i = 0; i < this.listView_.Columns.Count; ++i) {
                        this.listView_.Columns[i].Width = -2;
                    }
                    this.listView_.EndUpdate();
                }
            };
            bot.doStocks(eachDo);
            this.setTotalProfit(totalProfit, sumBuyCount);
        }

        void setTotalProfit(double totalProfit, int sumBuyCount)
        {
            double pureProfit = totalProfit - (sumBuyCount * PublicFutureVar.pureFeeTex);
            FutureBot bot = ControlGet.getInstance.futureBot();
            bot.nowBuyedProfit_ = pureProfit;

            this.totalProfit_.print(string.Format("{0:#,###0.#####} $", totalProfit));
            this.totalBuyCount_.print(string.Format("{0} 개", sumBuyCount));
            this.totalPureProfit_.print(string.Format("{0:#,###0.#####} $", pureProfit));
        }

        public void update()
        {
            if (this.listView_.InvokeRequired) {
                this.listView_.BeginInvoke(new Action(() => this.updateItems()));
            }
            else {
                this.updateItems();
            }
        }

        public void forcePayOff(object sender, EventArgs e)
        {
            if (this.listView_.CheckedItems.Count == 0) {
                return;
            }

            FutureBot bot = ControlGet.getInstance.futureBot();

            string message = "";
            foreach (ListViewItem item in this.listView_.CheckedItems) {
                message += string.Format("{0}({1})\n", item.Text, item.SubItems[2].Text);
            }
            message += string.Format("정말 청산 할까요?");
            if ((MessageBox.Show(message, "강제 청산 확인", MessageBoxButtons.YesNo) == DialogResult.Yes)) {
                foreach (ListViewItem item in this.listView_.CheckedItems) {
                    string code = item.SubItems[1].Text;
                    FutureData futureData = bot.getStockDataCode(code) as FutureData;
                    if (futureData == null) {
                        continue;
                    }
                    if (futureData.orderNumber_.Length > 0) {
                        message = string.Format("이미 {0}으로 주문이 들어가 있습니다.", futureData.orderNumber_);
                        Logger.getInstance.print(KiwoomCode.Log.주식봇, message);
                        return;
                    }
                    futureData.payOffCode_ = PAY_OFF_CODE.forcePayOff;
                    bot.payOff(code, "강제 청산 버튼 누름");
                }
            }
        }
    }

}
