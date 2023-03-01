using KiwoomCode;
using StockLibrary.StrategyManager;
using StockLibrary.StrategyManager.ProfitSafer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UtilLibrary;
using static StockLibrary.StrategyModuleList;

namespace StockLibrary
{
    public delegate void StockDataEach(string code, StockData stockData);
    public enum STOCKDATA_VAL
    {
        NO_REAL_DATA_RECEIVE = -1,      // 이거 리얼 데이터 안받음
    }

    public class StatisticsCount
    {
        public int continueWin_;                // 연속 승리
        public int continueLose_;               // 연속 패배
        public int totalWin_;                   // 이 기록이 진행된 이후로 전체 승리
        public int totalTrade_;                 // 이 기록이 진행된 이후 전체 거래수
        public DateTime startDate_ = DateTime.MinValue;

        public List<bool> tradeHistory_ = new List<bool>();
        public void clear()
        {
            continueWin_ = 0;
            continueLose_ = 0;
            totalWin_ = 0;
            totalTrade_ = 0;
            tradeHistory_.Clear();
        }

        public int MAX_UNIT = 7;
        public void updateCount(bool isWin)
        {
            if (totalTrade_ == 0) {
                startDate_ = DateTime.Now;
            }

            totalTrade_++;

            if (isWin) {
                totalWin_++;

                continueWin_++;
                continueLose_ = 0;
            } else {
                continueWin_ = 0;
                continueLose_++;
            }

            tradeHistory_.Add(isWin);
            if (this.bettorUnit() > MAX_UNIT) {
                tradeHistory_.Clear();
            }
        }

        public int bettorUnit()
        {
            var unit = tradeHistory_.Count;
            if (unit == 0) {
                return 1;
            }

            int count = 1;
            foreach(var win in tradeHistory_) {
                if (win) {
                    count++;
                }
                else {
                    count -= 1;
                    if (count <= 0) {
                        count = 1;
                    }
                }
            }
           
            return count;
        }
    }
    
    public enum TRADING_STATUS
    {
        모니터닝,
        매수,
        매도,
    }

    public class StockData: ICloneable
    {
        const int INVALID_IDX = -1;

        // stockData 갱신 락
        protected object dataLock_ { get; set; }

        //-------------------------------------------------------------
        public string code_ { get; set; }
        public string name_ { get; }

        public TRADING_STATUS position_ = TRADING_STATUS.모니터닝;
        public TRADING_STATUS lastPosition_ = TRADING_STATUS.모니터닝;

        public bool canTrade_ = true;
        public bool recvPayOff_ = false;
        public int aiPredicCount_ = 0;              // ai 예측으로 살 갯수

        public virtual bool isBuyedItem()
        {
            return false;
        }
        public string orderNumber_ = "";                    // 주문 번호
        public string regScreenNumber_ = "";                // 주문시 스크린 번호
        public DateTime tradingTime_ = DateTime.MinValue;   // 거래 시간
        protected List<CandleData> priceTable_;             // 가격 테이블

        public string whyPayOff_ = string.Empty;
        public PAY_OFF_CODE payOffCode_;
        /// <summary>
        ///  이 2개를 가지고 배팅 비율을 조절해 나가야 함.
        ///  켈리 법칙을 좀..
        /// http://stock79.tistory.com/82
        /// https://cafe.naver.com/volanalysis/802
        /// </summary>
        public StatisticsCount statisticsCount_ = new StatisticsCount();

        public List<CandleData> priceTable()
        {
            if (this.priceTable_.Count == 0) {
                return null;
            }
            return this.priceTable_;
        }

        public StrategyManagement fundManagement_ = null;
        public List<TradeStrategyTestResult> backTestResultList_ = new List<TradeStrategyTestResult>();
        
        public Dictionary<string, StrategyManagement> fundList_ = new Dictionary<string, StrategyManagement>();
        //-------------------------------------------------------------
        // 함수
        public StockData(string code, string name)
        {
            this.code_ = code;
            this.name_ = name;

            this.priceTable_ = new List<CandleData>();

            this.dataLock_ = new object();

            this.tradeModuleList_ = new List<BackTestRecoder>();
            this.resetTradeModule();

            PRICE_TYPE priceType = PublicVar.initType;
        }

        ~StockData()
        {
            if (this.priceTable_ != null) {
                this.priceTable_.Clear();
            }
            this.resetFinedBestRecoders();
        }

        //-------------------------------------------------------------
        // 복사 메소드
        protected void copyPriceDatas(ref StockData clone)
        {
            clone.dataLock_ = new object();
            lock (this.dataLock_) {
                clone.priceTable_ = new List<CandleData>(this.priceTable_);
            }
            clone.lastChejanDate_ = this.lastChejanDate_;
            //clone.updatePriceTable();

            if (this.tradeModuleList_ != null) {
                clone.tradeModuleList_ = new List<BackTestRecoder>(this.tradeModuleList_);
            }
        }

        public virtual Object Clone()
        {
            StockData clone = new StockData(this.code_, this.name_);
            this.copyPriceDatas(ref clone);

            return clone;
        }

        public virtual string regularCode()
        {
            return code_;
        }

        public int priceDataCount()
        {
            List<CandleData> priceTable = this.priceTable();
            if (priceTable == null) {
                return 0;
            }
            return priceTable.Count;
        }

        public virtual double todayCenterPrice()
        {
            double max = double.MinValue;
            double min = double.MaxValue;
            var now = DateTime.Now;
            var todayStart = new DateTime(now.Year, now.Month, now.Day);
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

        int findTodayStartTimeIdx(Bot bot)
        {
            var startTime = bot.marketStartTime();
            var nowDateTime = this.nowDateTime();
            startTime = new DateTime(nowDateTime.Year, nowDateTime.Month, nowDateTime.Day, startTime.Hour, 0, 0);
            if (nowDateTime.Hour < startTime.Hour) {
                startTime = startTime.AddDays(-1);
            }
            int startIdx = INVALID_IDX;
            var endCandle = this.candleData(startTime, out startIdx);
            if (endCandle == null) {
                startTime = new DateTime(startTime.Year, startTime.Month, startTime.Day, startTime.Hour + 1, 0, 0);
                endCandle = this.candleData(startTime, out startIdx);
                if (endCandle == null) {
                    return INVALID_IDX;
                }
            }
            return startIdx;
        }

        public void todayStartPrice(Bot bot)
        {
            int endIdx = this.findTodayStartTimeIdx(bot);
            if (endIdx == INVALID_IDX) {
                return;
            }
            var candle = priceTable_[endIdx];
            todayStartPrice_ =candle.startPrice_;
        }
       
        public void yesterdayHeight(Bot bot)
        {
            int endIdx = this.findTodayStartTimeIdx(bot);
            if (endIdx == INVALID_IDX) {
                return;
            }
            var elps = priceTable_[0].date_ - priceTable_[1].date_;
            var min = elps.TotalMinutes;
            var hourOfCnt = 60 / min;
            var dayOfCnt = 23 * hourOfCnt;
            var idxMax = Math.Min(priceDataCount() - 1, endIdx + dayOfCnt);

            double high = double.MinValue;
            double low = double.MaxValue;
            for (int i = endIdx + 1; i <= idxMax; ++i) {
                var candle = priceTable_[i];
                high = Math.Max(high, candle.highPrice_);
                low = Math.Min(low, candle.lowPrice_);
            }
            yesterdayLowPrice_ = low;
            yesterdayHighPrice_ = high;
        }

        public double todayStartPrice_ = double.MinValue;
        public double yesterdayLowPrice_ = double.MinValue;
        public double yesterdayHighPrice_ = double.MinValue;

        public void makeYesterdayCandle(Bot bot)
        {
            var now = this.nowDateTime();
            if (now == null) {
                return;
            }
            if (this.priceDataCount() < PublicFutureVar.priceTableCount) {
                return;
            }
            if (bot.marketStartHour() == now.Hour) {
                this.todayStartPrice(bot);
                this.yesterdayHeight(bot);
            }
        }

        // now 시리즈, linq 나 기준점 잡는데 사용
        protected const int NOW_TIME_IDX = 1;             // 완성된 분봉은 1임.
        public CandleData realCandle()
        {
            List<CandleData> priceTable = this.priceTable();
            if (priceTable == null || priceTable.Count <= NOW_TIME_IDX) {
                return null;
            }

            return priceTable[0];
        }

        public CandleData nowCandle()
        {
            List<CandleData> priceTable = this.priceTable();
            if (priceTable == null || priceTable.Count <= NOW_TIME_IDX) {
                return null;
            }

            return priceTable[NOW_TIME_IDX];
        }

        public CandleData prevCandle()
        {
            List<CandleData> priceTable = this.priceTable();
            if (priceTable == null || priceTable.Count <= (NOW_TIME_IDX + 1)) {
                return null;
            }

            return priceTable[NOW_TIME_IDX + 1];
        }

        public DateTime realNowDateTime()
        {
            if (this.priceDataCount() == 0) {
                return DateTime.MinValue;
            }
            var candle = this.realCandle();
            if (candle == null) {
                return DateTime.MinValue;
            }

            return candle.date_;
        }

        public DateTime nowDateTime()
        {
            var candle = this.nowCandle();
            if (candle == null) {
                return DateTime.MinValue;
            }

            return candle.date_;
        }

        public double realNowPrice()
        {
            if (this.priceDataCount() == 0) {
                return 0;
            }
            var candle = this.realCandle();
            if (candle == null) {
                return 0;
            }
            return candle.price_;
        }

        public double nowPrice()
        {
            var candle = this.nowCandle();
            if (candle == null) {
                return 0;
            }
            return candle.price_;
        }

        public UInt64 nowVolume()
        {
            var candle = this.nowCandle();
            if (candle == null) {
                return 0;
            }
            return candle.volume_;
        }

        public virtual double realProfit()
        {
            return 0;
        }

        public virtual double realOneProfit()
        {
            return 0;
        }

        public virtual double nowProfit()
        {
            return 0.0f;
        }

        public virtual double prevProfit()
        {
            return 0.0f;
        }

        public virtual double nowOneProfit()
        {
            return 0.0f;
        }
        public virtual double prevOneProfit()
        {
            return 0.0f;
        }

        public virtual double calcProfit(double sellPrice, int buyCount)
        {
            return 0.0f;
        }

        public virtual double oneTickValue()
        {
            return 0;
        }
        public virtual double oneTickSize()
        {
            return 0;
        }

        // 기대 수익확율
        public double nowTradeModuleWinRate()
        {
            var tradeModule = this.tradeModule();
            if (tradeModule == null) {
                return double.MinValue;
            }
            return tradeModule.expectedWinRate_;
        }

        // 기대 수익
        public double nowTradeModuleProfit()
        {
            var tradeModule = this.tradeModule();
            if (tradeModule == null) {
                return double.MinValue;
            }
            return tradeModule.avgProfit_;
        }

        public double nowTradeModuelExpectedRate()
        {
            var tradeModule = this.tradeModule();
            if (tradeModule == null) {
                return double.MinValue;
            }
            return tradeModule.expectedWinRate_ * tradeModule.tradeCount_;
        }

        public virtual double slope(EVALUATION_DATA data, int timeIdx)
        {
            var priceTable = this.priceTable();
            if (priceTable == null) {
                return 0;
            }
            if (priceTable.Count() < timeIdx) {
                return 0;
            }
            var now = priceTable[timeIdx].calc_[(int) data];
            var prev = priceTable[timeIdx + 1].calc_[(int) data];

            var slope = (prev - now) / (0 - 1);
            return slope;
        }

        public virtual double slopeCandle(int timeIdx)
        {
            var priceTable = this.priceTable();
            if (priceTable == null) {
                return 0;
            }
            if (priceTable.Count() < timeIdx) {
                return 0;
            }
            var now = priceTable[timeIdx].price_;
            var prev = priceTable[timeIdx].startPrice_;

            var slope = (prev - now) / (0 - 1);
            return slope;
        }

        //----------------------------------------------------------------------------
        // 실시간 데이터 받는 처리
        //@@@ 실시간 데이터 성공시 지울 tempIdx
        public int realDataScreenNo_ = (int) STOCKDATA_VAL.NO_REAL_DATA_RECEIVE;       // 리얼 데이터 받는거에 등록        
        public DateTime realRecvied_ = DateTime.MinValue;

        public bool realTimeRecived_ = false;
        public virtual void setRealTimeData(Bot bot, double price, Int64 vol)
        {
            if (this.priceDataCount() < PublicVar.priceTableCount) {
                return;
            }
            if (Util.isRange(0, price, 1000000) == false) {
                return;
            }

            var now = DateTime.Now;
            int min = bot.priceTypeMin();
            realTimeRecived_ = true;
            realRecvied_ = now;

            var nowCandle = priceTable_[0]; // 여긴 강제로 0번째 끌구 온다.
            nowCandle.lowPrice_ = Math.Min(nowCandle.lowPrice_, price);
            nowCandle.highPrice_ = Math.Max(nowCandle.highPrice_, price);
            nowCandle.price_ = price;
            nowCandle.volume_ += (UInt64) vol;
            
            var nowCandleTime = nowCandle.date_.AddMinutes(min);
            if (nowCandleTime < now) {
                var dateTime = nowCandleTime;
                for (; dateTime.AddMinutes(min) < now.AddMinutes(min); dateTime = dateTime.AddMinutes(min)) {
                    Logger.getInstance.print(Log.주식봇, "[{0}:{1}], {2} 분봉 생성, 시작 {3:##,###0.####}$, 고가 {4:##,###0.####}$, 저가 {5:##,###0.####}$, 종가 {6:##,###0.####}$",
                  name_, code_, nowCandle.date_, nowCandle.startPrice_, nowCandle.lowPrice_, nowCandle.highPrice_, nowCandle.price_);

                    CandleData candle = new CandleData(dateTime, price, price, price, price, (UInt64) vol);
                    candle.makeCandle_ = true;
                    this.updatePrice(candle);
                    this.updatePriceTable(bot);
                    this.makeYesterdayCandle(bot);
                }

                if (bot.nowStockMarketTime()) {
                    bot.trade(this);
                }

                // 초기화
                realTimeRecived_ = false;
            } else {
                this.updatePriceTable(bot);
                this.makeYesterdayCandle(bot);
            }

            // 캔들 갱신하고 손절 / 손익 관련 
            bot.tradeRealCandle(this);
        }

        public DateTime recvedDataTime_ = DateTime.MinValue;
        public void updatePriceTable(Bot bot)
        {
            this.sortPrice();
            this.calculatePrice(bot);
            this.updateProfitPrice();
        }

        public DateTime lastChejanDate_ = DateTime.MinValue;
        public double minOneProfit_ = double.MaxValue;
        public double maxProfit_ = double.MinValue;            // 최대 이익금
        public virtual void updateProfitPrice()
        {
            this.maxProfit_ = Math.Max(this.maxProfit_, this.nowOneProfit());
            this.minOneProfit_ = Math.Min(this.minOneProfit_, this.nowOneProfit());
        }

        public virtual bool profitDecending(double per=-0.05f)
        { 
            // 강제 익절
            var maxProfit = this.maxProfit_;
            if (Util.isRange(-5, maxProfit, 5)) {
                return false;
            }
            if (maxProfit < PublicFutureVar.feeTax) {
                return true;
            }
            var nowOneProfit = this.nowOneProfit();
            var limitProfit = Util.calcProfitRate(maxProfit, nowOneProfit);
            if (limitProfit <= per) {
                return true;
            }
            return false;
        }
        //-------------------------------------------------------------
        //        
        public DateTime rquestdPriceDataTime_ = DateTime.MinValue;
        public void loadPriceData(Bot bot)
        {
            // 업데이트는 최소 30초 간격으로 요청 한다.
            var min = bot.priceTypeMin();
            if (this.rquestdPriceDataTime_.AddSeconds((min * 60) / 3) > DateTime.Now) {
                return;
            }

            bot.requestStockData(this.code_);
        }

        public void setPriceTableDBSaved()
        {
            lock (this.dataLock_) {
                for (int index = 0; index < this.priceDataCount(); ++index) {
                    this.priceTable_[index].dbSaved_ = true;
                }
            }
        }

        public int buyCount_;
        public double buyPrice_;
        public virtual void resetBuyInfo()
        {
            this.buyCount_ = 0;
            this.buyPrice_ = 0.0f;

            this.orderNumber_ = "";
            this.regScreenNumber_ = "";

            this.lastChejanDate_ = DateTime.MinValue;

            this.minOneProfit_ = 100000000;
            this.maxProfit_ = -100000000;
        }

        public virtual double lostCutProfit()
        {
            return 0;
        }

        public virtual double targetProfit()
        {
            return 0;
        }

        public virtual bool canBuy()
        {
            return true;
        }
               
        public CandleData findCandle(DateTime dateTime)
        {
            var finded = this.priceTable_.FindIndex(x => x.date_ == dateTime);
            if (finded != -1) {
                return this.priceTable_[finded];
            }
            return null;
        }

        // 기존 분봉에 추가 연장
        public void updatePrice(CandleData newPriceData)
        {
            lock (this.dataLock_) {
                var finded = this.priceTable_.FindIndex(x => x.date_ == newPriceData.date_);
                if (finded != -1) {
                    newPriceData.dbSaved_ = this.priceTable_[finded].dbSaved_;
                    this.priceTable_.RemoveAt(finded);
                }
                this.priceTable_.Add(newPriceData);
            }
        }

        public CandleData candleData(DateTime dateTime, out int timeIdx)
        {
            timeIdx = -1;
            if (priceDataCount() == 0) {
                return null;
            }
            var finded = this.priceTable_.FindIndex(x => x.date_ == dateTime);
            if (finded != -1) {
                timeIdx = finded;
                return priceTable_[finded];
            }
            return null;
        }
               
        // 테이블 체로 바꾸는거
        public bool updatePriceTable(List<CandleData> newTable)
        {
            lock (this.dataLock_) {
                if (newTable.Count == 0) {
                    return false;
                }
                int newLastIdx = newTable.Count - 1;
                if (newLastIdx < 1) {
                    return false;
                }

                foreach (var candle in newTable) {
                    var finded = this.priceTable_.FindIndex(x => x.date_ == candle.date_);
                    if (finded != -1) {
                        candle.dbSaved_ = priceTable_[finded].dbSaved_;
                    }
                }

                var old = this.priceTable_;
                this.priceTable_ = newTable;
                old.Clear();
                old = null;
            }
            return true;
        }

        protected virtual void sortPrice()
        {
            if (this.priceDataCount() == 0) {
                return;
            }
            lock (this.dataLock_) {
                //  날짜 중복 제거 하고 내림차순으로 정렬함.
                var distinctCandle = this.priceTable_.GroupBy(x => x.date_).Select(y => y.First());
                if (this.priceTable_.Count != distinctCandle.Count()) {
                    var newList = from candle in distinctCandle
                                  orderby candle.date_ descending
                                  select candle;

                    this.priceTable_ = new List<CandleData>(newList);
                }
                else {
                    this.priceTable_.Sort(delegate (CandleData a, CandleData b) {
                        if (a.date_ < b.date_)
                            return 1;
                        else if (a.date_ > b.date_)
                            return -1;
                        return 0;
                    });
                }
            }
        }

        //-------------------------------------------------------------
        // 인디게이터 처리
        public double envelopeRate_ = 0.05;
        List<Calculater> calculaters(Bot bot)
        {
            if (fundManagement_ != null) {
                return fundManagement_.strategyModule_.calculaterList_;
            }
            var fund = bot.fundManagement_;
            return fund.strategyModule_.calculaterList_;
        }

        protected void calculatePrice(Bot bot)
        {
            if (canTrade_ == false) {
                return;
            }
            try {
                lock (this.dataLock_) {
                    if (this.priceDataCount() == 0) {
                        return;
                    }
                    var list = this.calculaters(bot);
                    foreach (var calc in list) {
                        calc.calculate(ref this.priceTable_);
                    }
                }
            }
            catch (Exception e) {
                Logger.getInstance.print(Log.에러, "{0} 계산중 에러 {1)/{2}", this.name_, e.Message, e.StackTrace);
            }
        }

        public TRADING_STATUS tradingSignal(Bot bot)
        {
            var fundManager = bot.fundManagement_;
            if (fundManager == null) {
                return TRADING_STATUS.모니터닝;
            }
            return fundManager.checkEntryStrategy(this);
        }

        //----------------------------------------------------------------------
        // 이전에 기대 범위를 넘긴 했는데, 다시 가격 하락을 해서 범위를 나갈경우, 빠른 손절
        public virtual bool isOutOfExpectedRange()
        {
            return false;
        }
        public virtual bool isPlusOutOfExpectedRange()
        {
            return false;
        }

        // 양수 대역 밴드 체크 (이상적임)
        public bool isExpectedRange()
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

            var nowOneProfit = this.nowOneProfit();

            var expectedAvg = recode.avgProfit_;
            var expectedMin = expectedAvg - recode.deviation_;
            var expectedMax = expectedAvg + recode.deviation_;

            if (Util.isRange(expectedMin, nowOneProfit, expectedMax) == false) {
                return false;
            }
            return true;
        }

        public double expectedMin()
        {
            var recode = this.tradeModule();
            if (recode == null) {
                return double.MinValue;
            }
            return recode.avgProfit_ - recode.deviation_;
        }

        public double expectedMax()
        {
            var recode = this.tradeModule();
            if (recode == null) {
                return double.MinValue;
            }
            return recode.avgProfit_ + recode.deviation_;
        }
           
        //---------------------------------------------------------------------------
        // 백테스팅 전략 관련
        public List<BackTestRecoder> tradeModuleList_ = null;
        public BackTestRecoder tradeModule()
        {
            if (this.tradeModulesCount() == 0) {
                return null;
            }
            var module = this.tradeModuleList_[0];
            if (module.buyTradeModule_ == null) {
                return null;
            }
            if (module.sellTradeModule_ == null) {
                return null;
            }
            return module;
        }

        public bool hasCorrectTradeModule()
        {
            var tradeModule = this.tradeModule();
            if (tradeModule == null) {
                return false;
            }
            if (tradeModule.tradeCount_ == 0) {
                return false;
            }
            return true;
        }

        public StrategyModule buyTradeModule()
        {
            var recode = this.tradeModule();
            if (recode == null) {
                return null;
            }
            return recode.buyTradeModule_;
        }

        public StrategyModule sellTradeModule()
        {
            var recode = this.tradeModule();
            if (recode == null) {
                return null;
            }
            return recode.sellTradeModule_;
        }

        public int tradeModulesCount()
        {
            if (this.tradeModuleList_ == null) {
                return 0;
            }
            return this.tradeModuleList_.Count;
        }

        public List<BackTestRecoder> tradeModuleList()
        {
            if (this.tradeModuleList_ == null) {
                this.tradeModuleList_ = new List<BackTestRecoder>();
            }
            return this.tradeModuleList_;
        }

        public void resetFinedBestRecoders()
        {
            if (this.tradeModuleList_ != null) {
                this.tradeModuleList_.Clear();
            }
        }

        public virtual BackTestRecoder newBackTestRecoder()
        {
            return new BackTestRecoder();
        }

        public void checkAllSetTrade(Bot bot)
        {
            List<BackTestRecoder> newList = new List<BackTestRecoder>();
            foreach (var recode in this.tradeModuleList_) {
                if (recode.buyTradeModule_ == null || recode.sellTradeModule_ == null) {
                    continue;
                }
                BackTestRecoder newModule = this.newBackTestRecoder();
                if (bot.doBackTestAnStock(this, recode.buyTradeModule_, recode.sellTradeModule_, ref newModule) == false) {
                    continue;
                }

                // 산거는 기존의 sell 모듈을 보존 해야 함.
                if (this.isBuyedItem()) {
                    newList.Add(newModule);
                }
                else {
                    // 아니면 필터링을 한다.
                    if (bot.checkTradingAllow(newModule, this.lostCutProfit())) {
                        newList.Add(newModule);
                    }
                }
            }

            if (this.isBuyedItem()) {
                this.tradeModuleList_ = newList;
                return;
            }

            var list = bot.tradingAllowFilter(newList);
            this.tradeModuleList_ = list;
        }

        //----------------------------------------------------------------------
        // 매매 관련 로직
        public bool setupTradeModule(Bot bot, bool filter = true)
        {
            var recodeCount = this.tradeModulesCount();
            if (this.isBuyedItem()) {
                if (recodeCount == 0) {
                    if (this.isSearchingTradeModule(bot) == false) {
                        bot.searchBestTradeModuleAnStock(this);
                    }
                }
                else {
                    if (this.tradeModule().tradeCount_ == 0) {
                        this.checkAllSetTrade(bot);
                    }
                }
                return true;
            }

            if (recodeCount == 0) {
                return false;
            }

            List<BackTestRecoder> newList = new List<BackTestRecoder>();

            foreach (var recode in this.tradeModuleList_) {
                BackTestRecoder newModule = this.newBackTestRecoder();
                if (bot.doBackTestAnStock(this, recode.buyTradeModule_, recode.sellTradeModule_, ref newModule) == false) {
                    continue;
                }
                if (bot.checkTradingAllow(newModule, this.lostCutProfit())) {
                    newList.Add(newModule);
                }
            }
            var ret = bot.tradingAllowFilter(newList);

            // 필터링 했는데 없다는건 기준 미달이란 얘기지.
            if (ret == null) {
                Logger.getInstance.print(Log.백테스팅, "{0} 재 테스트를 해봤는데 기준 미달됨.", this.name_);
                return false;
            }

            return true;
        }

        public bool hasCorrectTradeModule(Bot bot)
        {
            if (this.tradeModulesCount() == 0) {
                return this.setupTradeModule(bot);
            }

            return true;
        }

        public bool needTradeModuleUpdate()
        {
            if(PublicVar.finderMode != FINDER_MODE.느리게) {
                return true;
            }

            if (this.tradeModuleUpdateTime_.AddMinutes(PublicVar.tradeModuleUpdateMins) >= this.nowDateTime()) {
                return false;
            }
            return true;
        }

        public void resetTradeModule(bool force = false)
        {
            if (force == false) {
                // 산 아이템은 더이상 찾지 않는다.
                if (this.isBuyedItem()) {
                    if (this.tradeModule() != null) {
                        return;
                    }
                }
            }
            this.resetFinedBestRecoders();
            this.tradeModuleUpdateTime_ = DateTime.MinValue;
        }

        public bool hasTradeModule()
        {
            if (this.tradeModulesCount() == 0) {
                return false;
            }
            if (this.buyTradeModule() == null) {
                return false;
            }
            if (this.sellTradeModule() == null) {
                return false;
            }
            return true;
        }

        public DateTime tradeModuleUpdateTime_ = DateTime.MinValue;

        public void searchBestTradeModule(Bot bot, bool force = false)
        {
            if (this.priceDataCount() == 0) {
                return;
            }

            // 이미 산 것들에 대해선 처리 안함.
            if (this.isBuyedItem() && force == false) {
                if (this.hasTradeModule()) {
                    if (this.tradeModule().tradeCount_ == 0) {
                        this.checkAllSetTrade(bot);
                    }
                    if (this.tradeModulesCount() != 0) {
                        return;
                    }
                }
            }

            bot.searchBestTradeModuleAnStock(this, force);
        }

        public bool isSearchingTradeModule(Bot bot)
        {
            return bot.stockModuleSearcher_.have(this);
        }

      
        public bool allowVolume()
        {
            var nowCandle = this.nowCandle();
            if (nowCandle == null) {
                return false;
            }
            var allow = PublicFutureVar.limitVolumes * 2;
            if ((double) nowCandle.volume_ < allow) {
                return false;
            }
            return true;
        }

        //--------------------------------------------------------------------------//
        const int TRADE_BAN_HOUR = 2;
        DateTime tradeBanTime_ = DateTime.MinValue;
        public void setTradeBanTime()
        {
            if (PublicVar.reverseOrder == true) {
                return;
            }
            if (PublicVar.lostBanUse) {
                if (this.nowOneProfit() < 0) {
                    this.tradeBanTime_ = this.nowDateTime().AddHours(TRADE_BAN_HOUR);
                }
            }
        }

        public void resetTradeBanTime()
        {
            this.tradeBanTime_ = DateTime.MinValue;
        }

        public bool isTradeBanTime()
        {
            if (this.tradeBanTime_ == DateTime.MinValue) {
                return false;
            }

            // 10시 + 4시 = 14시 < 15시
            if (this.tradeBanTime_.AddHours(TRADE_BAN_HOUR) < this.nowDateTime()) {
                this.resetTradeBanTime();
                return false;
            }
            return true;
        }

        // 보유 시간
        public int positionHaveMin()
        {
            DateTime now = this.nowDateTime();
            var elpe = now - this.lastChejanDate_;
            return (int) elpe.TotalMinutes;
        }

        //-------------------------------------------------------------
        // 굳이 첫 모듈가지고만 체크할 필요 없으니, 갖고 있는거 모두 테스트
        public int payOffReadyCount_ = 0;       // 청산 준비

        public virtual bool isBuyTime(Bot bot, int timeIdx = NOW_TIME_IDX)
        {
            if (this.priceDataCount() < timeIdx) {
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

        public bool isSellTime(Bot bot, double buyPrice, int timeIdx = NOW_TIME_IDX)
        {
            if (this.priceDataCount() < timeIdx) {
                return false;
            }

            double nowPrice = this.nowPrice();
            if (nowPrice == buyPrice) {
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

        //-------------------------------------------------------------
        // 로그 관련
        public virtual string logExpectedRange(string currencySymbol = "$")
        {
            var recode = this.tradeModule();
            if (recode == null) {
                return string.Empty;
            }
            var expectedMin = this.expectedMin();
            var expectedMax = this.expectedMax();
            return string.Format("{0:##,###0.##}{3} < {1:##,###0.##}{3} < {2:##,###0.##}{3}", expectedMin, recode.avgProfit_, expectedMax, currencySymbol);
        }

        public string logCandleInfo(DateTime dateTime)
        {
            int timeIdx;
            var candleData = this.candleData(dateTime, out timeIdx);
            var text = string.Format("시간:\t{0}\n", dateTime);
            if (candleData != null) {
                text += string.Format("종가:\t{0:#,###0.######} $\n\n", candleData.price_);
                //text += string.Format("볼린저상단:\t{0:#,###0.######}\n", candleData.calc_[(int) EVALUATION_DATA.SMA_BOLLINGER_UP]);
                //text += string.Format("볼린저중단:\t{0:#,###0.######}\n", candleData.calc_[(int) EVALUATION_DATA.SMA_BOLLINGER_CENTER]);
                //text += string.Format("볼린저하단:\t{0:#,###0.######}\n\n", candleData.calc_[(int) EVALUATION_DATA.SMA_BOLLINGER_DOWN]);

                //text += string.Format("EMA10:\t{0:#,###0.######}\n", candleData.calc_[(int) EVALUATION_DATA.EMA_10]);
                //text += string.Format("EMA50:\t{0:#,###0.######}\n", candleData.calc_[(int) EVALUATION_DATA.EMA_50]);
                //text += string.Format("EMA100:\t{0:#,###0.######}\n\n", candleData.calc_[(int) EVALUATION_DATA.EMA_100]);

                //text += string.Format("WMA5:\t{0:#,###0.######}\n", candleData.calc_[(int) EVALUATION_DATA.WMA_5]);
                //text += string.Format("WMA20:\t{0:#,###0.######}\n", candleData.calc_[(int) EVALUATION_DATA.WMA_20]);
                //text += string.Format("WMA100:\t{0:#,###0.######}\n\n", candleData.calc_[(int) EVALUATION_DATA.WMA_100]);
            }
            return text;
        }

        public virtual string logProfitInfo()
        {
            string text = string.Format("1주당 이익: {0:#,###0.##} $\n", nowProfit());
            return text;
        }

        //-------------------------------------------------------------
        //가격표 관련 처리
        public void clearPrice()
        {
            lock (this.dataLock_) {
                this.priceTable_.Clear();
            }
        }

        public enum PRICE_DATA_TABLE_COLUMN
        {
            시간,
            저가,
            고가,
            시가,
            종가,
            거래량,
            //볼린저_상단,
            //볼린저_중앙,
            //볼린저_하단,
            //EMA_3,
            //EMA_20,
            //EMA_50,
            //PRICE_CHANNEL_UPPER,
            //PRICE_CHANNEL_LOWER,
        }
        public DataTable getChartPriceTable()
        {
            DataTable dt = new DataTable();
            // 저가, 고가, 시가, 종가 순으로 정렬 이거 차트에 뿌릴 용도임.
            foreach (PRICE_DATA_TABLE_COLUMN name in Enum.GetValues(typeof(PRICE_DATA_TABLE_COLUMN))) {
                if (name == PRICE_DATA_TABLE_COLUMN.시간) {
                    dt.Columns.Add(name.ToString(), typeof(DateTime));
                }
                else {
                    dt.Columns.Add(name.ToString(), typeof(double));
                }
            }
            var priceTable = this.priceTable();
            foreach (var candle in priceTable) {
                dt.Rows.Add(candle.date_,
                    candle.lowPrice_,
                    candle.highPrice_,
                    candle.startPrice_,
                    candle.price_,
                    candle.volume_
                    //candle.calc_[(int) EVALUATION_DATA.SMA_BOLLINGER_UP],
                    //candle.calc_[(int) EVALUATION_DATA.SMA_BOLLINGER_CENTER],
                    //candle.calc_[(int) EVALUATION_DATA.SMA_BOLLINGER_DOWN],
                    //candle.calc_[(int) EVALUATION_DATA.EMA_3],
                    //candle.calc_[(int) EVALUATION_DATA.EMA_20],
                    //candle.calc_[(int) EVALUATION_DATA.EMA_50],
                    //candle.calc_[(int) EVALUATION_DATA.PRICE_CHANNEL_UP],
                    //candle.calc_[(int) EVALUATION_DATA.PRICE_CHANNEL_DOWN]
                    )
                ;
            }
            dt.TableName = this.code_;
            return dt;
        }
    }
}
