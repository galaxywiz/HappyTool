using StockLibrary;
using StockLibrary.StrategyManager;
using StockLibrary.StrategyManager.StrategyModuler;
using StockLibrary.StrategyManager.ProfitSafer;
using System;
using System.Text;
using UtilLibrary;

namespace HappyTool.FundManagement
{
    public class StockFundManagement: StrategyManagement
    {
        public StockFundManagement(Bot bot)
        {
            bot_ = bot;
            var profit = new Profit_TargetRate();
            profit.targetRate_ = PublicVar.profitRate;
            var lost = new LostCut_TargettRate();
            lost.targetRate_ = PublicVar.loseCutRate;

            this.profitSafer_.addProfitLost(profit);
            this.profitSafer_.addProfitLost(lost);
        }

       
        public bool calcTradeCount(KStockData kStockData, out int tradingCount)
        {
            tradingCount = 0;
            var account = (int) bot_.accountMoney_;
            if (account < PublicVar.stockTradeMoney) {
                return false;
            }

            var nowPrice = kStockData.nowPrice();
            tradingCount = (PublicVar.stockTradeMoney / (int) nowPrice);
            if (tradingCount <= 0) {
                return false;
            }

            return true;
        }

        // 주문 처리
        public override void orderBuy(StockData stockData, int tradingCount, double buyPrice)
        {
            KStockData kStockData = (KStockData) stockData;
            if (this.bot_.test_ == false) {
                string log = string.Format("↑[{0}:{1}]의 매수 신호가 감지.\n[{2}][{3:##,###0.####} 원]\n", kStockData.name_, kStockData.code_, kStockData.nowDateTime(), kStockData.nowPrice());
                log += kStockData.logCandleInfo(kStockData.nowDateTime());
                var telegram = bot_.telegram_;
                if (telegram != null) {
                    telegram.sendMessage(log);
                }
            }

            bot_.buyStock(kStockData.code_, tradingCount, buyPrice);
        }

        void orderSell(StockData stockData)
        {
            this.orderSell(stockData, stockData.buyCount_, stockData.nowPrice());
        }

        public override void orderSell(StockData stockData, int tradingCount, double buyPrice)
        {
            KStockData kStockData = (KStockData) stockData;
            bot_.sellStock(kStockData.code_, kStockData.buyCount_, kStockData.nowPrice());
        }

        public override void tradeRealCandle(StockData stockData)
        {
            return;
        }

        // --- 강제 포지션
        public override void forcedBuy(string code)
        {
            KStockData kStockData = bot_.getStockDataCode(code) as KStockData;
            if (kStockData == null) {
                return;
            }
            if (kStockData.isBuyedItem() == false) {
                int count = 0;
                kStockData.wasBuyed_ = false;
                if (this.calcTradeCount(kStockData, out count)) {
                    this.orderBuy(kStockData, count, kStockData.nowPrice());
                }
            }
        }

        public override void forcedSell(string code)
        {
            KStockData kStockData = bot_.getStockDataCode(code) as KStockData;
            if (kStockData == null) {
                return;
            }
            if (kStockData.isBuyedItem()) {
                this.orderSell(kStockData);
                return;
            }
        }

        bool signalPayOff(KStockData kStockData)
        {
            if (strategyModule_ == null) {
                return false;
            }

            switch (kStockData.position_) {
                case TRADING_STATUS.매수:
                    if (strategyModule_.buyPayOffAtCompleteCandle(kStockData)) {
                        return true;
                    }
                    break;
            }

            return false;
        }

        bool canTrade(KStockData kStockData)
        {
            if (kStockData.priceDataCount() <= 0) {
                return false;
            }
            return true;
        }

        public override void trade(StockData stockData)
        {
            var kStockData = stockData as KStockData;
            if (kStockData == null) {
                return;
            }

            try {
                if (this.canTrade(kStockData) == false) {
                    return;
                }

                switch (kStockData.position_) {
                    case TRADING_STATUS.모니터닝:
                        this.processMonitor(kStockData);
                        break;

                    // 청산일경우
                    default:
                        string why = string.Empty;
                        if (this.processPayOff(kStockData, out why)) {
                            bot_.payOff(kStockData.code_, why);
                        }

                        break;
                }
            }
            catch (Exception e) {
                string log = string.Format("{0} 트레이딩 에러: {1} / {2}", kStockData.name_, e.Message, e.StackTrace);
                Logger.getInstance.print(KiwoomCode.Log.에러, log);
                bot_.telegram_.sendMessage(log);
            }
        }

        bool checkNewBuyAble(StockData stockData)
        {
            var kStockData = stockData as KStockData;
            if (kStockData == null) {
                return false;
            } 
            if (kStockData.isBuyedItem()) {
                return false;
            }
            if (kStockData.wasBuyed_) {
                return false;
            }

            return true;
        }

        protected override void processMonitor(StockData stockData)
        {
            var kStockData = stockData as KStockData;
            if (this.checkNewBuyAble(kStockData) == false) {
                return;
            }

            // 진입 전략 체크
            var ret = strategyModule_.checkEntryStrategy(kStockData);
            if (ret == TRADING_STATUS.모니터닝) {
                return;
            }

            double tradePrice = kStockData.nowPrice();
            if (PublicVar.immediatelyOrder) {
                const double IMMEDIATELY_ORDER = -1.0f;
                tradePrice = IMMEDIATELY_ORDER;
            }

            int maxCount = 0;
            switch (ret) {
                case TRADING_STATUS.매수:
                    if (this.calcTradeCount(kStockData, out maxCount)) {
                        var tradingCount = maxCount;
                        if (tradingCount > 0) {
                            this.orderBuy(stockData, tradingCount, tradePrice);
                        }
                    }
                    break;
            }
        }

        protected override bool processPayOff(StockData stockData, out string why)
        {
            var kStockData = stockData as KStockData;

            why = string.Empty;
            StringBuilder why2 = new StringBuilder();
            // 디버그 분석용..
            var code = kStockData.code_;
            var buyTime = kStockData.lastChejanDate_;
            var nowTime = kStockData.nowDateTime();
            var position = kStockData.position_;
            var profit = kStockData.nowProfit();
            var haveMin = kStockData.positionHaveMin();

            // 청산 신호가 나왔는가?
            if (this.signalPayOff(kStockData)) {
                why = string.Format("{0} 의 청산 신호 감지\n", kStockData.name_);
                kStockData.payOffCode_ = PAY_OFF_CODE.payoffSignal;
                return true;
            }

            // 안전장치 가드
            if (this.checkProfitSafer(kStockData, out why2)) {
                why = why2.ToString();
                return true;
            }

            return false;
        }
    }  
}
