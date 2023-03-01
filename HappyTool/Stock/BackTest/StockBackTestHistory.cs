using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HappyTool.Stock {
    class BackTestHistoryData {
        public int stockCode_ { get; }
        public int timeIndex_ { get; }
        public System.DateTime date_ { get; }
        public int price_ { get; }
        public int buyCount_ { get; }
        public bool isBuy_ { get; }

        public BackTestHistoryData(int stockCode, int timeIndex, DateTime date, int price, int count, bool isBuy)
        {
            stockCode_ = stockCode;
            timeIndex_ = timeIndex;
            date_ = date;
            price_ = price;
            buyCount_ = count;
            isBuy_ = isBuy;
        }

        public int totalPrice()
        {
            return price_ * buyCount_;
        }
    }
    class BackTestProfitHistory
    {
        public int stockCode_;
        public DateTime date_;
        public int money_;

        public BackTestProfitHistory(int stockCode, DateTime date, int profit)
        {
            stockCode_ = stockCode;
            date_ = date;
            money_ = profit;
        }
    }

    class BackTestHistory {
        List<BackTestHistoryData> historyList_ { get; set; }
        public int tradeCount_ = 0;
        public List<BackTestProfitHistory> profitHistoryList_ { get; set; }

        public BackTestHistory()
        {
            historyList_ = new List<BackTestHistoryData>();
            profitHistoryList_ = new List<BackTestProfitHistory>();
        }

        public List<BackTestHistoryData> getHistoryData()
        {
            return historyList_;
        }

        public void addBuy(int stockCode, int timeIndex, DateTime date, int price, int count)
        {
            BackTestHistoryData data = new BackTestHistoryData(stockCode, timeIndex, date, price, count, true);
            historyList_.Add(data);
            tradeCount_++;
        }

        public void addSell(int stockCode, int timeIndex, DateTime date, int price, int count)
        {
            BackTestHistoryData data = new BackTestHistoryData(stockCode, timeIndex, date, price, count, false);
            historyList_.Add(data);
            tradeCount_++;
        }

        // 과거 이력 우선으로 소팅 한다. 그래야 매매를 순차 테스팅이 되지
        private void sortDesc()
        {
            historyList_.Sort(delegate (BackTestHistoryData A, BackTestHistoryData B) {
                if (A.timeIndex_ > B.timeIndex_) {
                    return -1;
                }
                if (A.timeIndex_ < B.timeIndex_) {
                    return 1;
                }
                return 0;
            });
        }
        public double profitRate_ { get; set; }
        public int totalProfit_ { get; set; }

        public void calcEval()
        {
            profitRate_ = 0.0f;
            this.sortDesc();
            if (historyList_.Count < 1) {
                return;
            }
            BackTestHistoryData first = historyList_[0];
            BackTestHistoryData last = historyList_[historyList_.Count - 1];

            int totalMoney = last.price_ * last.buyCount_;
            int initMoney = first.price_ * first.buyCount_;

            profitRate_ = ((double) (totalMoney - initMoney) / (double) totalMoney) * 100;
            totalProfit_ = totalMoney;

            foreach (BackTestHistoryData data in historyList_) {
                if (data.isBuy_) {
                    BackTestProfitHistory profitHistory = new BackTestProfitHistory(data.stockCode_, data.date_, data.totalPrice());
                    profitHistoryList_.Add(profitHistory);
                }
            }
        }

        // 첫 거래일을 가지고 온다.
        public DateTime firstTradeDate()
        {
            if (historyList_.Count == 0) {
                return DateTime.Now;
            }

            return historyList_[0].date_;
        }
    }
}
