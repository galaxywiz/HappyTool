using StockLibrary.StrategyManager.Trade;
using System;
using System.Collections.Generic;
using UtilLibrary;

// 이런 변수 (SCOPE_TERM, candlehigh )를
// 매번 백테스팅 해서 적용하도록 수정해보자

namespace StockLibrary.StrategyManager.StrategyModuler
{
    public class LarryRStrategyModule: StrategyModule
    {
        /*
         * https://ldgeao99.tistory.com/443?category=875616
         * https://stock79.tistory.com/78?category=457287
         */
        public LarryRStrategyModule(Bot bot) : base(bot)
        {
            assetBettor_ = new AssetBettor();
            this.addTradeStrategy(new MaTrendTradeStrategy(EVALUATION_DATA.EMA_100));
            this.initCacluaterList();
        }

        protected override void initCacluaterList()
        {
            calculaterList_.Add(new EMACalculater());
        }
        protected const int TERM = 400;
        protected const int SCOPE_TERM = 20;

        protected double getNoice(List<CandleData> priceTable, int timeIdx)
        {
            double kSum = 0;
            for (int i = 1; i <= SCOPE_TERM; i++) {
                var candle = priceTable[timeIdx + i];
                var start = candle.startPrice_;
                var end = candle.price_;
                var high = candle.highPrice_;
                var low = candle.lowPrice_;
                if (high != low) {
                    var k = 1 - (Math.Abs(start - end) / (high - low));
                    kSum += k;
                }
            }

            var kAvg = (kSum / SCOPE_TERM);
            if (double.IsNaN(kAvg) || double.IsInfinity(kAvg)) {
                return double.MaxValue;
            }
            return kAvg;
        }
        protected double getAvgHight(List<CandleData> priceTable, int timeIdx)
        {
            double kSum = 0;
            for (int i = 1; i <= SCOPE_TERM; i++) {
                var candle = priceTable[timeIdx + i + 0];
                var k = candle.height();
                kSum += k;
            }

            var kAvg = (kSum / SCOPE_TERM);
            if (double.IsNaN(kAvg) || double.IsInfinity(kAvg)) {
                return double.MaxValue;
            }
            return kAvg;
        }

        protected const int START_HOUR = 9;
        protected const int END_HOUR = 5;
        protected const double NOISE = 0.7f;
        // 매수 조건
        protected override bool buy(StockData stockData, int timeIdx = 0)
        {
            if (stockData.todayStartPrice_ == double.MinValue) {
                return false;
            }
            
            var priceTable = this.priceTable(stockData, TERM);
            if (priceTable == null) {
                return false;
            }
            
            if (checkBuyTrade(stockData, timeIdx) == false) {
                return false;
            }

            var scopeAvg = NOISE;// this.getNoice(priceTable, timeIdx);
            if (scopeAvg == double.MaxValue) {
                return false;
            }

            var height = (stockData.yesterdayHighPrice_ - stockData.yesterdayLowPrice_);
            var stand = stockData.todayStartPrice_ + (height * scopeAvg);

            var nowPrice = stockData.nowPrice();
            if (nowPrice < stand) {
                return false;
            }
            return true;
        }

        // 청산 조건 원래 다음날 종가에 팔아 버림;;;
        public override bool buyPayOffAtCompleteCandle(StockData stockData, int timeIdx = -1)
        {
            if (stockData.positionHaveMin() < 120) {
                return false;
            }
            var now = stockData.nowDateTime();
            if (UtilLibrary.Util.isRange(END_HOUR, now.Hour, START_HOUR)) {
                return true;
            }
            return false;
        }

        // 실시간 청산 조건
        public override bool buyPayOffAtRealCandle(StockData stockData)
        {
            return false;
        }

        //-----------------------------------------------------------------------//
        // 매도 조건
        protected override bool sell(StockData stockData, int timeIdx = 0)
        {
            if (stockData.todayStartPrice_ == double.MinValue) {
                return false;
            }

            var priceTable = this.priceTable(stockData, TERM);
            if (priceTable == null) {
                return false;
            }

            if (checkSellTrade(stockData, timeIdx) == false) {
                return false;
            }

            var scopeAvg = NOISE;// this.getNoice(priceTable, timeIdx);
            if (scopeAvg == double.MaxValue) {
                return false;
            }

            var height = (stockData.yesterdayHighPrice_ - stockData.yesterdayLowPrice_);
            var stand = stockData.todayStartPrice_ + (height * scopeAvg);

            var nowPrice = stockData.nowPrice();
            if (nowPrice > stand) {
                return false;
            }
            return true;
        }

        // 청산 조건
        public override bool sellPayOffAtCompleteCandle(StockData stockData, int timeIdx = -1)
        {
            if (stockData.positionHaveMin() < 120) {
                return false;
            }
            var now = stockData.nowDateTime();
            if (UtilLibrary.Util.isRange(END_HOUR, now.Hour, START_HOUR)) {
                return true;
            }
            return false;
        }

        // 실시간 청산 조건
        public override bool sellPayOffAtRealCandle(StockData stockData)
        {
            return false;
        }
    }

    //-----------------------------------------------------------------------//
    // 선물용
    public class StockLarryRECStrategy: LarryRStrategyModule
    {
        public StockLarryRECStrategy(Bot bot) : base(bot)
        {
        }

        // 매수 청산
        public override bool buyPayOffAtCompleteCandle(StockData stockData, int timeIdx = -1)
        {
            var nowCandle = stockData.nowCandle();
            var nowProfitRate = Util.calcProfitRate(stockData.buyPrice_, nowCandle.price_);
            if (nowProfitRate <= PublicVar.loseCutRate) {
                // 손절
                return true;
            }
            if (PublicVar.profitRate <= nowProfitRate) {
                stockData.recvPayOff_ = true;
            }
            if (stockData.recvPayOff_) {
                var prevCandle = stockData.prevCandle();
                var prevProfitRate = Util.calcProfitRate(stockData.buyPrice_, prevCandle.price_);
                if (prevProfitRate >= nowProfitRate) {
                    // 더이상 이익이 오르지 않으면 손절
                    return true;
                }

                //14시 이후라면 그냥 손절
                if (nowCandle.date_.Hour >= 14) {
                    return true;
                }
            }
            
            if (Util.isRange(PublicVar.loseCutRate, nowProfitRate, PublicVar.profitRate) == false) {
                return true;
            }

            return false;
        }

        public override bool buyPayOffAtRealCandle(StockData stockData)
        {
            return base.buyPayOffAtRealCandle(stockData);
        }

        //---------------------------------------------------------------//
        // 주식은 공매도가 아닌이상 sell은 안씀.
        protected override bool sell(StockData stockData, int timeIdx = 0)
        {
            return false;
        }
        public override bool sellPayOffAtCompleteCandle(StockData stockData, int timeIdx = -1)
        {
            return false;
        }
        public override bool sellPayOffAtRealCandle(StockData stockData)
        {
            return false;
        }
    }
}
