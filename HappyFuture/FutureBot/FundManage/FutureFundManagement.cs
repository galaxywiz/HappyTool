using StockLibrary;
using StockLibrary.StrategyManager;
using StockLibrary.StrategyManager.ProfitSafer;
using System;
using System.Text;
using UtilLibrary;

namespace HappyFuture.FundManage
{
    // 공용 함수들..
    // 이걸 각 futureData 마다 따로 갖도록 하는게 어떨까
    public class FutureFundManagement: StrategyManagement
    {
        public FutureFundManagement(Bot bot)
        {
            this.bot_ = bot;
        }

        public override object Clone()
        {
            return new FutureFundManagement(this.bot_);
        }

        // 강제 청산 체크
        bool checkAlmostEndDay(FutureData futureData)
        {
            if (futureData.isEndDay()) {
                if (futureData.buyCount_ > 0) {
                    string log = string.Format("{0} 잔존만기로 무족건 청산", futureData.name_);
                    Logger.getInstance.print(KiwoomCode.Log.주식봇, log);
                    this.bot_.payOff(futureData.code_, log);
                    return true;
                }
            }
            if (futureData.isAlmostEndDay()) {
                return true;
            }
            return false;
        }

        bool canTradeCount(FutureData futureData, TRADING_STATUS position, out int tradingCount)
        {
            int canBuy;
            if (position == TRADING_STATUS.매도) {
                canBuy = futureData.canBuyCount_;
            }
            else {
                canBuy = futureData.canSellCount_;
            }

            if (canBuy == 0) {
                tradingCount = 0;
                return false;
            }

            tradingCount = canBuy;
            return true;
        }

        const int MICRO_NUMBER = 10;
        // 주문 처리
        public override void orderBuy(StockData stockData, int tradingCount, double buyPrice)
        {
            FutureData futureData = stockData as FutureData;
            if (this.bot_.test_ == false) {
                string log = string.Format("↑[{0}:{1}]의 매수 신호가 감지.\n[{2}][{3:##,###0.####} $]\n", futureData.name_, futureData.code_, futureData.nowDateTime(), futureData.nowPrice());
                log += futureData.logCandleInfo(futureData.nowDateTime());
                var telegram = bot_.telegram_;
                if (telegram != null) {
                    telegram.sendMessage(log);
                }
            }

            if (futureData.isMicroCode() && tradingCount >= MICRO_NUMBER) {
                var code = futureData.code_.Substring(1);
                var changeItem = bot_.getStockDataCode(code) as FutureData;
                if (changeItem != null) {
                    tradingCount = tradingCount / MICRO_NUMBER;
                    futureData = changeItem;
                }
            }

            futureData.resetTradeBanTime();
            this.bot_.buyStock(futureData.code_, tradingCount, buyPrice);
        }

        public override void orderSell(StockData stockData, int tradingCount, double buyPrice)
        {
            FutureData futureData = stockData as FutureData;
            if (this.bot_.test_ == false) {
                string log = string.Format("↓[{0}:{1}]의 매도 신호가 감지.\n[{2}][{3:##,###0.####} $]\n", futureData.name_, futureData.code_, futureData.nowDateTime(), futureData.nowPrice());
                log += futureData.logCandleInfo(futureData.nowDateTime());
                var telegram = bot_.telegram_;
                if (telegram != null) {
                    telegram.sendMessage(log);
                }
            }

            if (futureData.isMicroCode() && tradingCount >= MICRO_NUMBER) {
                var code = futureData.code_.Substring(1);
                var changeItem = bot_.getStockDataCode(code) as FutureData;
                if (changeItem != null) {
                    tradingCount = tradingCount / MICRO_NUMBER;
                    futureData = changeItem;
                }
            }

            futureData.resetTradeBanTime();
            this.bot_.sellStock(futureData.code_, tradingCount, buyPrice);
        }

        bool canTrade(FutureData futureData)
        {
            if (futureData.priceDataCount() < PublicVar.priceTableCount) {
                return false;
            }

            if (futureData.orderNumber_.Length > 0) {
                return false;
            }

            if (strategyModule_.useBackTestResult()) {
                if (futureData.isBuyedItem() == false) {
                    if (futureData.hasTradeModule() == false) {
                        this.bot_.researchTradeModuel(futureData);
                        if (futureData.tradeModulesCount() == 0) {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        bool checkTrade(FutureData futureData)
        {
            var nowDateTime = futureData.nowDateTime();
            if (futureData.tradingTime_ == nowDateTime) {
                return false;
            }
            if (this.canTrade(futureData) == false) {
                return false;
            }

            if (futureData.isBuyedItem() == false) {
                if (this.checkAlmostEndDay(futureData) == true) {
                    return false;
                }
            }

            // 백테스팅 시뮬을 하는거 이면 처리인데..
            if (strategyModule_.useBackTestResult()) {
                // 이거 기준이 2,3건 이라 통계상 무쓸모 전략임.
                if (futureData.isBuyedItem()) {
                    if (futureData.tradeModule() == null) {
                        futureData.setupTradeModule(this.bot_, false);
                    }

                    if (futureData.hasTradeModule() == false) {
                        this.bot_.researchTradeModuel(futureData);
                    }
                }
                else {
                    if (futureData.hasCorrectTradeModule(this.bot_) == false) {
                        return false;
                    }
                }
            }
            return true;
        }

        bool futureTradeEndTime(FutureData futureData)
        {
            DateTime now = futureData.nowDateTime();
            if (futureData.endDays_ < 3) {
                DateTime tradeEndTime = futureData.tradeEndTime_;
                if (tradeEndTime == DateTime.MinValue) {
                    return false;
                }

                const int HOUR_INVERTER = 1;
                DateTime startTime = tradeEndTime.AddHours(-HOUR_INVERTER);
                if (startTime > now) {
                    return true;
                }
            }
            return false;
        }

        bool checkNewBuyAble(FutureData futureData)
        {
            if (futureData.isBuyedItem()) {
                return false;
            }
            if (this.futureTradeEndTime(futureData)) {
                return false;
            }

            if (futureData.priceDataCount() < PublicVar.priceTableCount) {
                return false;
            }

            if (futureData.allowVolume() == false) {
                return false;
            }

            // 예수금이 위탁증거금 1.5배 되어야 거래 함. (너무 위험해)
            //var trustMargin = futureData.margineMoney_.trustMargin_;
            //if (Util.calcProfitRate(trustMargin, this.bot_.accountMoney_) < PublicFutureVar.allowTrustMarginTime) {
            //    Logger.getInstance.print(KiwoomCode.Log.주식봇, "{0} 은 안전 증거금이 없어서 거부", futureData.name_);
            //    return false;
            //}

            if (futureData.isTradeBanTime()) {
                return false;
            }

            if (strategyModule_.useBackTestResult()) {
                // 백테스팅 안한건 매매 안함
                if (futureData.hasTradeModule() == false) {
                    return false;
                }

                if (futureData.tradeModulesCount() == 0) {
                    return false;
                }

                if (futureData.expectedMin() < 0) {
                    return false;
                }
            }

            if (PublicFutureVar.tradePeriod == 매매기한.일간) {
                DateTime now = futureData.nowDateTime();
                // 2시에서 7시 사이는 매수 걸지 않는다. 대신 현재 것들은 장마감 시간에 청산
                if (Util.isRange(2, now.Hour, 7)) {
                    return false;
                }
            }
            return true;
        }
         
        // 장마감 1시간 전부턴 매수 금지, 적당히 이익 나면 무족건 매도
        // 주말 지나니 천연가스 1주가 갑자기 반대로 폭등해서 200만원어치 손해가 나버림;;; 
        bool isMarketCloseTime(FutureData futureData)
        {
            if (futureData.isBuyedItem() == false) {
                return false;
            }
            if (this.futureTradeEndTime(futureData)) {
                this.bot_.payOff(futureData.code_, "옵션 만기 시간이라 청산");
                return true;
            }
            DateTime now = futureData.nowDateTime();
            switch (PublicFutureVar.tradePeriod) {
                case 매매기한.일간:
                    // 섬머 타임 봐도 6,7시에 장이 마감됨, 5시에 청산
                    if (Util.isRange(5, now.Hour, 7)) {
                        this.bot_.payOff(futureData.code_, "장 마감시간 다가와서 강제 청산");
                        return true;
                    }
                    break;
                case 매매기한.주간:
                    if (now.DayOfWeek == DayOfWeek.Saturday) {
                        if (Util.isRange(5, now.Hour, 7)) {
                            this.bot_.payOff(futureData.code_, "장 마감시간 다가와서 강제 청산");
                            return true;
                        }
                    }
                    break;
                case 매매기한.무제한:
                    return false;
            }

            return false;
        }


        // 이게 계속 이득이 오르는 중인가?
        bool profitTrand(FutureData futureData)
        {
            var nowProfit = futureData.nowProfit();
            var prevProfit = futureData.prevProfit();

            if (prevProfit < nowProfit) {
                return true;
            }
            return false;
        }

        // 지금 즉시 청산 시켜야 하는가?
        bool immediatePayOff(StockData stockData)
        {
            FutureData futureData = stockData as FutureData;

            if (this.profitTrand(futureData) == false) {
                return true;
            }

            return false;
        }

        //------------------------------------------------------------------
        // 매매 관련
        // 강제 매수 포지션 (버튼이나 텔레그램으로 명령시)
        public override void forcedBuy(string code)
        {
            FutureData futureData = (FutureData) this.bot_.getStockDataCode(code);
            if (futureData == null) {
                return;
            }

            if (this.canTrade(futureData) == false) {
                return;
            }

            switch (futureData.position_) {
                case TRADING_STATUS.모니터닝:
                    int maxCount = 0;
                    double tradePrice = futureData.nowCandle().price_;
                    if (PublicVar.immediatelyOrder) {
                        const double IMMEDIATELY_ORDER = -1.0f;
                        tradePrice = IMMEDIATELY_ORDER;
                    }
                    if (this.canTradeCount(futureData, TRADING_STATUS.매수, out maxCount)) {
                        var tradingCount = strategyModule_.countTradeBuy(futureData, maxCount);
                        this.orderBuy(futureData, tradingCount, tradePrice);
                    }
                    break;

                // 청산일경우
                default:
                    string log = string.Format("{0}은 [{1}] 포지선 청산 in buy order", futureData.name_, futureData.position_);
                    //      Logger.getInstance.print(KiwoomCode.Log.백테스팅, log);
                    this.bot_.payOff(code, log);
                    break;
            }
        }

        // 강제 매도 포지션 (버튼이나 텔레그램으로 명령시)
        public override void forcedSell(string code)
        {
            FutureData futureData = (FutureData) this.bot_.getStockDataCode(code);
            if (futureData == null) {
                return;
            }

            if (this.canTrade(futureData) == false) {
                return;
            }

            switch (futureData.position_) {
                case TRADING_STATUS.모니터닝:
                    int maxCount = 0;
                    double tradePrice = futureData.nowCandle().price_;
                    if (PublicVar.immediatelyOrder) {
                        const double IMMEDIATELY_ORDER = -1.0f;
                        tradePrice = IMMEDIATELY_ORDER;
                    }
                    if (this.canTradeCount(futureData, TRADING_STATUS.매도, out maxCount)) {
                        var tradingCount = strategyModule_.countTradeSell(futureData, maxCount);
                        this.orderSell(futureData, tradingCount, tradePrice);
                    }
                    break;

                // 청산일경우
                default:
                    string log = string.Format("{0}은 [{1}] 포지선 청산 in sell order", futureData.name_, futureData.position_);
                    //     Logger.getInstance.print(KiwoomCode.Log.백테스팅, log);
                    this.bot_.payOff(code, log);
                    break;
            }
        }

        // 청산 신호 감지        
        bool signalPayOff(FutureData futureData)
        {
            if (strategyModule_ == null) {
                return false;
            }

            switch (futureData.position_) {
                case TRADING_STATUS.매수:
                    if (strategyModule_.buyPayOffAtCompleteCandle(futureData)) {
                        return true;
                    }
                    break;
                case TRADING_STATUS.매도:
                    if (strategyModule_.sellPayOffAtCompleteCandle(futureData)) {
                        return true;
                    }
                    break;
            }

            return false;
        }

        public override void tradeRealCandle(StockData stockData)
        {
            FutureData futureData = stockData as FutureData;
            if (futureData == null) {
                return;
            }
            switch (futureData.position_) {
                case TRADING_STATUS.매수:
                    if (strategyModule_.buyPayOffAtRealCandle(futureData)) {
                        this.bot_.payOff(futureData.code_, "긴급 break 시그널로 청산");
                        return;
                    }
                    break;
                case TRADING_STATUS.매도:
                    if (strategyModule_.sellPayOffAtRealCandle(futureData)) {
                        this.bot_.payOff(futureData.code_, "긴급 break 시그널로 청산");
                        return;
                    }
                    break;
            }
        }

        public override void trade(StockData stockData)
        {
            FutureData futureData = stockData as FutureData;
            if (futureData == null) {
                return;
            }
            
            try {
                if (this.isMarketCloseTime(futureData)) {
                    return;
                }
                if (this.checkTrade(futureData) == false) {
                    return;
                }
                switch (futureData.position_) {
                    case TRADING_STATUS.모니터닝:
                        this.processMonitor(futureData);
                        break;

                    // 청산일경우
                    default:
                        string why = string.Empty;
                        if (this.processPayOff(futureData, out why)) {
                            this.bot_.payOff(futureData.code_, why);
                        }
                        break;
                }
            }
            catch (Exception e) {
                string log = string.Format("{0} [{1}] 포지선, 트레이딩 에러: {2} / {3}", futureData.name_, futureData.position_, e.Message, e.StackTrace);
                Logger.getInstance.print(KiwoomCode.Log.에러, log);
                this.bot_.telegram_.sendMessage(log);
            }
            finally {
                if (Program.happyFuture_.futureDlg_.checkBox_doTrade.Enabled) {
                    Logger.getInstance.print(KiwoomCode.Log.주식봇, "[{0}] 종목 [{1} 시간] 매매 처리 완료", stockData.name_, stockData.nowDateTime());
                }
            }
        }

        //@@@ 이부분 리팩토링이 필요
        /*
         * 1. 진입 전략 체크
         * 2. 진입 조건에 따른 갯수 체크
         * 3. 청산 전략 체크
         * 
         */

        protected override void processMonitor(StockData stockData)
        {
            FutureData futureData = stockData as FutureData;
            if (this.checkNewBuyAble(futureData) == false) {
                return;
            }

            // 진입 전략 체크
            var ret = strategyModule_.checkEntryStrategy(stockData);
            if (ret == TRADING_STATUS.모니터닝) {
                return;
            }

            double tradePrice = futureData.nowPrice();
            if (PublicVar.immediatelyOrder) {
                const double IMMEDIATELY_ORDER = -1.0f;
                tradePrice = IMMEDIATELY_ORDER;
            }

            var nowDateTime = futureData.nowDateTime();
            int maxCount = 0;
            switch (ret) {
                case TRADING_STATUS.매수:
                    if (this.canTradeCount(futureData, ret, out maxCount)) {
                        var tradingCount = strategyModule_.countTradeBuy(stockData, maxCount);
                        if (tradingCount > 0) {
                            this.orderBuy(futureData, tradingCount, tradePrice);
                            futureData.tradingTime_ = nowDateTime;
                        }
                    }
                    break;
                case TRADING_STATUS.매도:
                    if (this.canTradeCount(futureData, ret, out maxCount)) {
                        var tradingCount = strategyModule_.countTradeSell(stockData, maxCount);
                        if (tradingCount > 0) {
                            this.orderSell(futureData, tradingCount, tradePrice);
                            futureData.tradingTime_ = nowDateTime;
                        }
                    }
                    break;
            }
        }

        protected override bool processPayOff(StockData stockData, out string why)
        {
            why = string.Empty;
            StringBuilder why2 = new StringBuilder();
            FutureData futureData = stockData as FutureData;
            // 디버그 분석용..
            var code = futureData.code_;
            var buyTime = futureData.lastChejanDate_;
            var nowTime = futureData.nowDateTime();
            var position = futureData.position_;
            var profit = futureData.nowProfit();
            var haveMin = futureData.positionHaveMin();
            if (futureData.payOffReadyCount_ > 0) {
                if (this.immediatePayOff(futureData)) {
                    return true;
                }
            }

            // 청산 신호가 나왔는가?
            if (this.signalPayOff(futureData)) {
                why = string.Format("{0} 의 청산 신호 감지\n", futureData.name_);
                futureData.payOffCode_ = PAY_OFF_CODE.payoffSignal;

                futureData.payOffReadyCount_ = 1;
                if (this.immediatePayOff(futureData)) {
                    return true;
                }
                return false;
            }

            // 안전장치 가드
            if (this.checkProfitSafer(futureData, out why2)) {
                why = why2.ToString();
                futureData.payOffReadyCount_ = 1;
                if (this.immediatePayOff(futureData)) {
                    return true;
                }
                return false;
            }

            return false;
        }
    }
    // 2%룰 기법   http://stock79.tistory.com/82

    // 캘리기법
    // 옵티멀 F 기법
}
