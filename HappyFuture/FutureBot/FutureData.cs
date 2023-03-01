using StockLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using UtilLibrary;

namespace HappyFuture
{
    //선물은 이거 자체가 배팅이 되는 기능이 있어야 함.
    //매수 배팅 / 매도 배팅 과 같은 개념

    public class MarginMoney
    {
        public double trustMargin_ = 0.0f;         // 위탁증거금
        public double retaindMargin_ = 0.0f;        // 유지증거금 (이거 이하면 로스컷)

        public double lostCut()
        {
            return this.retaindMargin_ - this.trustMargin_;
        }
    }

    public class FutureData: StockData
    {
        public int lastBuyCount_ = 0;

        public FutureData(string code, string name) : base(code, name)
        {
            this.resetBuyInfo();
            this.envelopeRate_ = 0.15;
        }
        
        public override string regularCode()
        {
            var code = code_;
            if (code_.Length > 3) {
                code = code.Substring(0, code.Length - 3);
            }
            return code;
        }

        public override object Clone()
        {
            FutureData clone = new FutureData(this.code_, this.name_);
            StockData copy = clone;
            this.copyPriceDatas(ref copy);

            clone.tickSize_ = this.tickSize_;
            clone.tickValue_ = this.tickValue_;
            clone.margineMoney_ = this.margineMoney_;
            clone.position_ = this.position_;

            clone.buyCount_ = this.buyCount_;
            clone.buyPrice_ = this.buyPrice_;
            clone.tradeModuleUpdateTime_ = this.tradeModuleUpdateTime_;
            clone.envelopeRate_ = 0.15;
            return clone;
        }

        public override bool isBuyedItem()
        {
            switch (this.position_) {
                case TRADING_STATUS.모니터닝:
                return false;
            }
            return true;
        }

        public int canBuyCount_ = 0;            // 매수 체결 가능수
        public int canSellCount_ = 0;           // 매도 체결 가능수

        public override bool canBuy()
        {
            if (this.canBuyCount_ == 0 && this.canSellCount_ == 0) {
                return false;
            }
            return true;
        }

        const int END_DAY_LIMIT = -1000;
        public int endDays_ = END_DAY_LIMIT;                // 잔존 만기, 이거 1이하면 무족건 청산 시켜야 함

        // 잔존만기 3일전, 오전1시(10시기준) 반대 매매 들어감.
        // 주문이 아예 안걸리니 적당히 5일전까지만..
        const int LIMIT_DAY = 5;
        public bool isEndDay()
        {
            if (this.endDays_ == END_DAY_LIMIT) {
                return false;
            }

             if (this.endDays_ < LIMIT_DAY) {
                return true;
            }
            return false;
        }

        public DateTime tradeEndTime_ = DateTime.MinValue;
        public bool isAlmostEndDay()
        {
            if (this.endDays_ == END_DAY_LIMIT) {
                return false;
            }

            if (this.endDays_ - 1 < LIMIT_DAY) {
                return true;
            }
            return false;
        }

        public bool isMicroCode()
        {
            if (name_.StartsWith("Micro")) {
                return true;
            }
            if (code_.StartsWith("M")) {
                return true;
            }
            return false;
        }

        public double tickValue_;               // 1틱당 가치
        public double tickSize_;                // 틱 단위
        public void setTickInfo(string tickValue, string tickStep)
        {
            if (double.TryParse(tickValue, out this.tickValue_) == false) {
                return;
            }

            if (double.TryParse(tickStep, out this.tickSize_) == false) {
                return;
            }
        }

        public int nowProfitTicks()
        {
            if (isBuyedItem() == false) {
                return 0;
            }
            var oneProfit = this.nowOneProfit();
            if (oneProfit == 0) {
                return 0;
            }
            var tick = oneProfit / tickValue_;
            return (int) tick;
        }

        void setBuyDateTime()
        {
            if (this.lastChejanDate_ == DateTime.MinValue) {
                var realCandle = this.realCandle();
                if (realCandle == null) {
                    return;
                }
                var now = realCandle.date_;
                if (now == DateTime.MinValue) {
                    now = DateTime.Now;
                }
                this.lastChejanDate_ = now;
                this.resetTradeBanTime();
            }
        }

        public void setBuyInfo(string isBuy, string count, string buyPrice)
        {
            if (int.TryParse(count, out this.buyCount_) == false) {
                return;
            }

            if (double.TryParse(buyPrice, out this.buyPrice_) == false) {
                return;
            }

            if (PublicVar.reverseOrder) {
                const string BUY_CODE = "2";
                const string SELL_CODE = "1";
                switch (isBuy) {
                    case SELL_CODE:
                    this.position_ = TRADING_STATUS.매수;
                    this.setBuyDateTime();
                    break;
                    case BUY_CODE:
                    this.position_ = TRADING_STATUS.매도;
                    this.setBuyDateTime();
                    break;
                    default:
                    return;
                }
            }
            else {
                const string BUY_CODE = "1";
                const string SELL_CODE = "2";
                switch (isBuy) {
                    case SELL_CODE:
                    this.position_ = TRADING_STATUS.매수;
                    this.setBuyDateTime();
                    break;
                    case BUY_CODE:
                    this.position_ = TRADING_STATUS.매도;
                    this.setBuyDateTime();
                    break;
                    default:
                    return;
                }
            }
        }

        public override void resetBuyInfo()
        {
            this.lastPosition_ = this.position_;
            this.lastBuyCount_ = this.buyCount_;

            this.position_ = TRADING_STATUS.모니터닝;
            this.payOffReadyCount_ = 0;
            base.resetBuyInfo();
        }

        // 600개만 취하는게 다름.        
        protected override void sortPrice()
        {
            if (this.priceDataCount() == 0) {
                return;
            }
        //    lock (this.dataLock_) {
                // 날짜 중복 제거 하고 내림차순으로 정렬함.
                var distinctCandle = this.priceTable_.GroupBy(x => x.date_).Select(y => y.First());
                var newList = (from candle in distinctCandle
                               orderby candle.date_ descending
                               select candle).Take(PublicFutureVar.키움증권_가격표갯수);

                this.priceTable_ = new List<CandleData>(newList);
       //     }
        }

        const int MIN_PROFIT_TIME = 5;
        // 위탁 증거금 바탕으로 정한다.
        public override double lostCutProfit()
        {
            const double DEFAULT = 200.0f;
            if (this.tickValue_ == 0.0f) {
                return -DEFAULT;
            }

            var time = PublicFutureVar.lostCutTime;
            var allowStep = time;
            var lostCut = allowStep * this.tickValue_;
            if (lostCut > 0) {
                lostCut = -lostCut;
            }

            if (Util.isRange(-MIN_PROFIT_TIME, lostCut, MIN_PROFIT_TIME)) {
                return -DEFAULT;
            }
            if (Util.isRange(-(DEFAULT * 5), lostCut, (DEFAULT * 5)) == false) {
                return -DEFAULT;
            }
            return lostCut;
        }

        public override double targetProfit()
        {
            const double DEFAULT = 300.0f;
            if (this.tickValue_ == 0.0f) {
                return DEFAULT;
            }

            var time = PublicFutureVar.targetProfitTime;

            var allowStep = time;
            var targetProfit = allowStep * this.tickValue_;
            if (targetProfit < 0) {
                targetProfit = Math.Abs(targetProfit);
            }

            if (Util.isRange(-MIN_PROFIT_TIME, targetProfit, MIN_PROFIT_TIME)) {
                return DEFAULT;
            }
            if (Util.isRange(-(DEFAULT * 5), targetProfit, (DEFAULT * 5)) == false) {
                return DEFAULT;
            }
            return targetProfit;
        }

        // 선물은 개념이 달라서 약정 금액을 넣어야 함.
        // 즉, 1개 주문할때 이 금액만큼이 예수금으로 필요함
        public MarginMoney margineMoney_ = new MarginMoney();
        public void setMargine(MarginMoney margine)
        {
            // 입력이 잘못된경우임.
            if (margine.retaindMargin_ < 1) {
                var bot = ControlGet.getInstance.futureBot();
                bot.setFutureDataInfo(this);
                Logger.getInstance.print(KiwoomCode.Log.주식봇, "{0} 의 마진 데이터 잘못 입력됨.", name_);
                return;
            }
            this.margineMoney_.retaindMargin_ = margine.retaindMargin_;
            this.margineMoney_.trustMargin_ = margine.trustMargin_;
        }

        public double trustMargin()
        {
            var trust = this.margineMoney_.trustMargin_;  // 위탁증거
            if (trust < 1) { 
                var bot = ControlGet.getInstance.futureBot();
                bot.setFutureDataInfo(this);
                trust = this.margineMoney_.trustMargin_;
            }
            return trust;
        }

        public override double todayCenterPrice()
        {
            double max = double.MinValue;
            double min = double.MaxValue;
            var now = this.nowDateTime();
            DateTime todayStart;
            var bot = ControlGet.getInstance.futureBot();
            if (bot.isSummerTime()) {
                todayStart = new DateTime(now.Year, now.Month, now.Day, 6, 0, 0);
                if (now.Hour < 6) {
                    todayStart = todayStart.AddDays(-1);
                }
            }
            else {
                todayStart = new DateTime(now.Year, now.Month, now.Day, 7, 0, 0);
                if (now.Hour < 7) {
                    todayStart = todayStart.AddDays(-1);
                }
            }

            foreach (var candle in priceTable_) {
                if (todayStart < candle.date_) {
                    continue;
                }
                var price = candle.price_;
                max = Math.Max(max, price);
                min = Math.Min(min, price);
            }
            return (max + min) / 2;
        }

        double oneProfit(CandleData candle)
        {
            var now = candle.price_;
            var buyPrice = this.buyPrice_;
            return this.calcProfit(buyPrice, now, 1, this.position_);
        }

        public override double realOneProfit()
        {
            if (this.priceDataCount() == 0) {
                return 0;
            }
            var candle = this.realCandle();
            return oneProfit(candle);
        }

        public override double nowOneProfit()
        {
            var candle = this.nowCandle();
            if (candle == null) {
                return 0;
            }
            return oneProfit(candle);
        }

        public override double prevOneProfit()
        {
            var candle = this.prevCandle();
            if (candle == null) {
                return 0;
            }
            return oneProfit(candle);
        }

        public override double realProfit()
        {
            return this.realOneProfit() * this.buyCount_;
        }

        public override double nowProfit()
        {
            return this.nowOneProfit() * this.buyCount_;
        }

        public override double prevProfit()
        {
            return this.prevOneProfit() * this.buyCount_;
        }

        public double nowPureProfit()
        {
            return this.nowProfit() - (this.buyCount_ * PublicFutureVar.pureFeeTex);
        }

        public override double slope(EVALUATION_DATA data, int timeIdx)
        {
            var priceTable = this.priceTable();
            if (priceTable == null) {
                return 0;
            }
            if (priceTable.Count() < timeIdx) {
                return 0;
            }
            var tickSize = this.tickSize_;
            var now = priceTable[timeIdx].calc_[(int) data] / tickSize;
            var prev = priceTable[timeIdx + 1].calc_[(int) data] / tickSize;

            var slope = (prev - now) / (0 - 1);
            return slope;
        }

        public override double slopeCandle(int timeIdx)
        {
            var priceTable = this.priceTable();
            if (priceTable == null) {
                return 0;
            }
            if (priceTable.Count() < timeIdx) {
                return 0;
            }
            var tickSize = this.tickSize_;
            var now = priceTable[timeIdx].price_ / tickSize;
            var prev = priceTable[timeIdx].startPrice_ / tickSize;

            var slope = (prev - now) / (0 - 1);
            return slope;
        }

        public override BackTestRecoder newBackTestRecoder()
        {
            return new FutureBackTestRecoder();
        }

        public override double oneTickValue()
        {
            return tickValue_;
        }
        public override double oneTickSize()
        {
            return tickSize_;
        }

        public override double calcProfit(double sellPrice, int buyCount)
        {
            double profit = 0.0f;

            if (tickSize_ <= 0) {
                var bot = ControlGet.getInstance.futureBot();
                bot.setFutureDataInfo(this);
            }

            switch (this.position_) {
                case TRADING_STATUS.매수:
                profit = (((sellPrice - buyPrice_) / this.tickSize_) * this.tickValue_) * buyCount;
                break;
                case TRADING_STATUS.매도:
                profit = (((buyPrice_ - sellPrice) / this.tickSize_) * this.tickValue_) * buyCount;
                break;
            }

            return profit;
        }

        public double calcProfit(double buyPrice, double sellPrice, int buyCount, TRADING_STATUS position)
        {
            double profit = 0.0f;
      
            if (tickSize_ <= 0) {
                var bot = ControlGet.getInstance.futureBot();
                bot.setFutureDataInfo(this);
            }

            switch (position) {
                case TRADING_STATUS.매수:
                profit = (((sellPrice - buyPrice) / this.tickSize_) * this.tickValue_) * buyCount;
                break;
                case TRADING_STATUS.매도:
                profit = (((buyPrice - sellPrice) / this.tickSize_) * this.tickValue_) * buyCount;
                break;
            }

            return profit;
        }

        public override bool isBuyTime(Bot bot, int timeIdx = 0)
        {
            double nowPrice = this.nowPrice();
            if (nowPrice == this.buyPrice_) {
                return false;
            }
            if (this.priceDataCount() == 0) {
                return false;
            }
            var module = this.tradeModule();
            if (module == null) {
                return false;
            }

            if (module.buyTradeModule_.buy(this, timeIdx)) {
                return true;
            }
            return false;
        }

        public bool isSellTime(Bot bot, int timeIdx = 0)
        {
            if (this.priceDataCount() == 0) {
                return false;
            }

            double nowPrice = this.nowPrice();
            if (nowPrice == this.buyPrice_) {
                return false;
            }

            var module = this.tradeModule();
            if (module == null) {
                return false;
            }

            if (module.sellTradeModule_.buy(this, timeIdx)) {
                return true;
            }
            return false;
        }

        //----------------------------------------------------------------------
        public string exchangeName_;
        public string exchangeUrl()
        {
            string url = string.Format("https://finance.yahoo.com/quote/{0}.{1}", this.code_, this.exchangeName_);
            return url;
        }

        //----------------------------------------------------------------------
        // 이전에 기대 범위를 넘긴 했는데, 다시 가격 하락을 해서 범위를 나갈경우, 빠른 손절
        public override bool isOutOfExpectedRange()
        {
            if (this.isBuyedItem() == false) {
                return false;
            }
            var recode = this.tradeModule();
            if (recode == null) {
                return false;
            }
            if (this.priceDataCount() == 0) {
                return false;
            }

            var maxProfit = this.maxProfit_;

            var expectedAvg = recode.avgProfit_;
            var expectedMin = expectedAvg - recode.deviation_;
            if (expectedMin < PublicFutureVar.feeTax) {
                return false;
            }
            if (maxProfit < expectedMin) {
                return false;
            }

            var nowOneProfit = this.nowOneProfit();
            if (nowOneProfit < expectedMin) {
                return true;
            }

            return false;
        }

        public override bool isPlusOutOfExpectedRange()
        {
            if (this.isBuyedItem() == false) {
                return false;
            }
            var recode = this.tradeModule();
            if (recode == null) {
                return false;
            }
            if (this.priceDataCount() == 0) {
                return false;
            }

            var maxProfit = this.maxProfit_;

            var expectedAvg = recode.avgProfit_;
            var expectedMax = expectedAvg + recode.deviation_;
            if (expectedMax < PublicFutureVar.feeTax) {
                return false;
            }
            if (maxProfit < expectedMax) {
                return false;
            }

            var nowOneProfit = this.nowOneProfit();
            if (nowOneProfit < expectedMax) {
                return true;
            }

            return false;
        }

        public override string logProfitInfo()
        {
            string text = string.Format("현재 포지션: {0}\n", position_);
            text += string.Format("현재 이익: {0:#,###0.##} $\n", nowProfit());
            return text;
        }

        string nextCode(string orgCode)
        {
            var year = int.Parse(orgCode.Substring(orgCode.Length - 2, 2));
            var monthStr = orgCode.Substring(orgCode.Length - 3, 1);
            var code = orgCode.Substring(0, orgCode.Length - 3);
            //   1,   2,   3,   4,   5,   6,   7,   8,   9,  10,  11,  12
            string[] mon = { "F", "G", "H", "J", "K", "M", "N", "Q", "U", "V", "X", "Z" };
            int month = 0;
            foreach (var m in mon) {
                month++;
                if (monthStr == m) {
                    break;
                }
            }
            if (month == 12) {
                year++;
                monthStr = "F";
            } else {
                monthStr = mon[month];
            }
            return code + monthStr + year.ToString();
        }

    }
}
