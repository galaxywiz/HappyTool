using StockLibrary.StrategyManager;
using StockLibrary.StrategyForTrade;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using UtilLibrary;
using static StockLibrary.StrategyModuleList;

namespace StockLibrary
{
    public struct TradeStrategyStruct
    {
        public StrategyModule buyModule_;
        public StrategyModule sellModule_;
    };

    public struct StrategyModuleRecode
    {
        public StrategyModule module_;
        public double winRate_;
        public int tradeCount_;
    }
    public enum REF_PRICE_TYPE
    {
        기준_분봉,
        중간_분봉,
        시간_분봉,
        일_분봉,
    };

    public abstract class Bot
    {
        public double accountMoney_ { get; set; }
        public double saveMyMoney_ = 0;

        public double yesterAccountMoney_ = 0.0f;
        public double todayTotalProfitRate_ = 0.0f;        //    "   이익율

        public PRICE_TYPE priceType_ = PublicVar.initType;
        //---------------------------------------------------------------------
        // 주식 봇이 갖고 있는 주식들에 대한처리
        protected object lockObject_ = null;
        protected Dictionary<string, StockData> stockPool_ = null;
        protected List<Dictionary<string, StockData>> refStockPool_ = new List<Dictionary<string, StockData>>();

        public bool activate_ = false;
        public bool tradeReady_ = false;      
        public double nowTotalProfit_ = 0.0f;                               // HTS에서 받은 오늘 총 이익금
        public double todayTotalProfit_ = 0.0f;
        //TODO 여기 밑에 4개는 상황봐서 삭제 처리

        // 하위 봇에서 생성할것
        public TelegramBot telegram_ = null;
        public StrategyManagement fundManagement_ = null;                           // 자금 운용
        public FindBestTradeModule stockModuleSearcher_ = null;                 // 모듈 서쳐
        internal protected TradeModuleFilter tradeFilter_ = null;

        public BestTradeModuleDB tradeModuleDB_ = null;                      // 모듈 db
        // 봇의 cpu, mem 사용량
        private PerformanceCounter prcessCpu_ = null;
        public int tradeCount_ = 0;
        public int tradeWinCount_ = 0;

        public bool havePayOffStock_ = false;
        //---------------------------------------------------------------------
        public Bot()
        {
            TechTradeModlueList.getInstance.makeList();

            this.lockObject_ = new object();
            this.stockPool_ = new Dictionary<string, StockData>();
            this.accountMoney_ = 0;
        }

        ~Bot()
        {
        }
        public bool test_ = false;      // 시뮬레이션 전용 기능

        public BotState botState_ = null;
        public bool allowDump_ = false;
        bool checkAllow()
        {

            // 2개는 페어를 맞춰야 함.
            string[] allowLocalIP = {
                "MJbVFugARcFT+PiA4JzU864XHp866vzmMDbc/w26cjw=",
                "N8Ix54BNtGAp9Hk0s+9+w/SefKGNBBGUFX3ZP5+dEn0=",
                "m/UIuZe3tRuoI93/TVT7GcPW2Rrv8Z2/V+R/+CJd/L8=",
                "JVVjNg8BtiOcMdo4bKSad8gb5emVtYAUF8aBTYXOliM=",
                "Ikd8X1TDHBYqs+5FbyXxqpgvLhJfJYrwBnX8VAbizkw=",
                "m/UIuZe3tRuoI93/TVT7GdCOf77I8KZ1tJ09M8yh/RA=",
                "m/UIuZe3tRuoI93/TVT7Gd0Krfvxb4qRyr3UQHeBe60=",
            };

            string[] allowHostIP = {
                "VcydahFexeyFTOVCHDRuSmbrwxuufCu/UqGlUwQR4Bg=",
                "VcydahFexeyFTOVCHDRuSgOfjKSNn5EGqBnYigwdktE=",
                "5u8hQb0gnDckU6mRqzVpacO6rwgMXgryS5K4tlP8AHE=",
                "5u8hQb0gnDckU6mRqzVpacO6rwgMXgryS5K4tlP8AHE=",
                "5u8hQb0gnDckU6mRqzVpacO6rwgMXgryS5K4tlP8AHE=",
                "5u8hQb0gnDckU6mRqzVpacO6rwgMXgryS5K4tlP8AHE=",
                "5u8hQb0gnDckU6mRqzVpacO6rwgMXgryS5K4tlP8AHE=",
            };
            AESEncrypt aesEncrypt = new AESEncrypt();
            string 키 = aesEncrypt.AESDecrypt128(PublicVar.passwordKey, "암호푸는키");

            int i = 0;
            foreach (string ip in allowLocalIP) {
                string temp = aesEncrypt.AESDecrypt128(ip, 키);

                if (UtilLibrary.Util.getMyIP() == temp) {
                    temp = aesEncrypt.AESDecrypt128(allowHostIP[i], 키);
                    if (UtilLibrary.Util.getHostIP().StartsWith(temp.Substring(12))) {
                        return true;
                    }
                }
                ++i;
            }

            return true;
        }

        public double accountRate()
        {
            if (double.IsNaN(accountMoney_)) {
                return 0;
            }
            if (yesterAccountMoney_ == 0) {
                yesterAccountMoney_ = accountMoney_;
            }
            var rate = accountMoney_ / yesterAccountMoney_;
            if (double.IsNaN(rate)) {
                return 0;
            }
            return rate;
        }

        protected abstract void setupTelegram();
        public abstract void setFundManagement();

        public virtual bool start()
        {
            this.tradeModuleDB_ = new BestTradeModuleDB("TradeModule.db");
            this.tradeModuleDB_.select(this);

            this.botState_.start();
            this.setupTelegram();

            try {
                this.prcessCpu_ = new PerformanceCounter("Process", "% Processor Time", Process.GetCurrentProcess().ProcessName);
            }
            catch (Exception e) {
                Logger.getInstance.print(KiwoomCode.Log.에러, "{0} 에러 {1}", e.Message, e.StackTrace);
            }

            return this.checkAllow();
        }

        public virtual void process()
        {
            if (this.botState_ != null) {
                this.botState_.process();
            }
        }

        //--------------------------------------------------------
        // 주식 추가
        public bool addStockData(StockData stockData)
        {
            string code = stockData.code_;
            if (code.Length == 0) {
                return false;
            }
            lock (lockObject_) {
                StockData oldData = this.getStockDataCode(code);
                if (oldData == null) {
                    this.stockPool_.Add(code, stockData);
                }

                return true;
            }
        }

        public void removeStock(string code)
        {
            lock (lockObject_) {
                this.stockPool_.Remove(code);
            }
        }

        // 산 주식만 제거 (재 갱신용)
        public virtual void removeBuyStock()
        {
            List<string> deletePool = new List<string>();
            StockDataEach eachDo = (string code, StockData stockData) => {
                if (stockData.isBuyedItem()) {
                    deletePool.Add(code);
                }
            };
            this.doStocks(eachDo);

            foreach (string code in deletePool) {
                this.removeStock(code);
            }
            deletePool = null;
        }

        public void clearStockPool()
        {
            lock (lockObject_) {
                this.stockPool_.Clear();
                this.copyRefStocks();
            }
        }

        // 주식 가지고 오기
        public StockData getStockDataCode(string code)
        {
            lock (lockObject_) {
                StockData stockData = null;
                if (this.stockPool_.TryGetValue(code, out stockData)) {
                    return stockData;
                }

                StockDataEach eachDo = (string c, StockData sd) => {
                    if (sd.regularCode().ToUpper() == code.ToUpper()) {
                        stockData = sd;
                    }
                };
                this.doStocks(eachDo);
                return stockData;
            }            
        }

        // 주식 가지고 오기
        public StockData getStockData(string name)
        {
            StockData ret = null;
            StockDataEach eachDo = (string code, StockData stockData) => {
                if (stockData.name_ == name) {
                    ret = stockData;
                }
            };
            this.doStocks(eachDo);
            return ret;
        }

        public int stockPoolCountOnlyBuyed()
        {
            int count = 0;
            StockDataEach eachDo = (string code, StockData stockData) => {
                if (stockData.isBuyedItem()) {
                    count++;
                }
            };
            this.doStocks(eachDo);
            return count;
        }

        protected void doStocksPool(Dictionary<string, StockData> pool, StockDataEach each, bool buyFirst = true)
        {
            List<StockData> doBuyList = new List<StockData>();
            List<StockData> doList = new List<StockData>();

            lock (lockObject_) {
                //산 주식 부터 먼저 처리 해준다.
                foreach (KeyValuePair<string, StockData> keyValue in pool) {
                    string code = keyValue.Key;
                    StockData stockData = keyValue.Value;
                    if (stockData.isBuyedItem()) {
                        doBuyList.Add(stockData);
                    }
                }
                foreach (KeyValuePair<string, StockData> keyValue in pool) {
                    string code = keyValue.Key;
                    StockData stockData = keyValue.Value;
                    if (stockData.isBuyedItem() == false) {
                        doList.Add(stockData);
                    }
                }

                if (doList.Count == 0 && doBuyList.Count == 0) {
                    return;
                }
            }
           
            try {
                // 거래량 많은순으로 정렬, 기대 수익이 높은 순으로 정렬
                IOrderedEnumerable<StockData> query = from stockData in doList
                                                      orderby
                                                              stockData.nowTradeModuelExpectedRate() descending,
                                                              stockData.nowTradeModuleWinRate() descending,
                                                              stockData.nowTradeModuleProfit() descending,
                                                              stockData.nowVolume() descending
                                                      select stockData;

                // 보유주식 먼저 처리
                if (buyFirst) {
                    foreach (StockData stockData in doBuyList) {
                        each(stockData.code_, stockData);
                    }

                    foreach (StockData stockData in query) {
                        each(stockData.code_, stockData);
                    }
                }
                else {
                    // 일반 주식 먼저 처리
                    foreach (StockData stockData in query) {
                        each(stockData.code_, stockData);
                    }
                    // 보유주식 
                    foreach (StockData stockData in doBuyList) {
                        each(stockData.code_, stockData);
                    }
                }
            }
            catch (Exception e) {
                Logger.getInstance.print(KiwoomCode.Log.에러, "주식 풀 에러 {0} \n{1}", e.Message, e.StackTrace);
            }
            doBuyList.Clear();
            doList.Clear();
        }

        public void doStocks(StockDataEach each, bool buyFirst = true)
        {
            this.doStocksPool(this.stockPool_, each, buyFirst);
        }
        //---------------------------------------------------------------------
        // 상위 분봉, 1분봉을 위한 풀

        public void doRefStocks(StockDataEach each, REF_PRICE_TYPE type)
        {
            this.doStocksPool(this.refStockPool_[(int) type], each);
        }

        public virtual void copyRefStocks()
        {
            refStockPool_.Clear();
            foreach (REF_PRICE_TYPE refType in Enum.GetValues(typeof(REF_PRICE_TYPE))) {
                refStockPool_.Add(new Dictionary<string, StockData>());
            }

            StockDataEach eachDo = (string code, StockData stockData) => {
                foreach (REF_PRICE_TYPE refType in Enum.GetValues(typeof(REF_PRICE_TYPE))) {
                    var clone = stockData.Clone() as StockData;
                    clone.clearPrice();

                    var pool = this.refStockPool_[(int) refType];
                    if (pool.ContainsKey(code) == false) {
                        pool.Add(code, clone);
                    }
                }
            };
            this.doStocks(eachDo);
        }

        public StockData getRefStockDataCode(string code, REF_PRICE_TYPE type)
        {
            StockData stockData = null;
            var pool = this.refStockPool_[(int) type];
            if (pool.TryGetValue(code, out stockData)) {
                return stockData;
            }
            return null;
        }

        public int stockPoolIndex_ = 0;
        public int stockPoolCount()
        {
            if (this.stockPool_ == null) {
                return 0;
            }
            return this.stockPool_.Count;
        }

        public int priceTypeMin()
        {
            int min = 60 * 24;
            switch (this.priceType_) {
                case PRICE_TYPE.MIN_1:
                min = PublicVar.priceType_min1;
                break;
                case PRICE_TYPE.MIN_2:
                min = PublicVar.priceType_min2;
                break;
                case PRICE_TYPE.MIN_3:
                min = PublicVar.priceType_min3;
                break;
            }
            return min;
        }

        public void setPriceMinType(PRICE_TYPE type)
        {
            this.priceType_ = type;
        }

        // stock봇에 문제 있을시 다시 하위로 가져가기
        // 이 로직은 선물쪽 특화임.
        public bool checkFullLoadedStocks()
        {
            bool fullLoaded = true;
            DateTime now = DateTime.Now;
            var min = this.priceTypeMin();
            StockDataEach eachDo = (string code, StockData stockData) => {
                if (code.Length == 0) {
                    return;
                }
                if (stockData.priceDataCount() > 0) {
                    if (stockData.recvedDataTime_.AddMinutes(min) > now) {
                        return;
                    }
                }

                // 로딩 실패 처리
                stockData.loadPriceData(this);
                fullLoaded = false;
            };
            this.doStocks(eachDo, false);

            if (fullLoaded) {
                this.stockPoolIndex_ = 0;
                this.updatePoolView();
            }
            return fullLoaded;
        }

        public abstract void updatePoolView();

        public double nowBuyedProfit_ = double.MinValue;
        public abstract void updateNowBuyedProfit();

        double transactionEvaluation()
        {
            if (this.tradeCount_ > 3) {
                return (this.tradeWinCount_ / (double) this.tradeCount_) * 100;
            }
            return -1.0f;
        }

        public string logForTradeWinRate()
        {
            double rate = this.transactionEvaluation();
            if (rate < 0) {
                return "거래 평가 불가";
            }
            string log = string.Format("* 거래 / 승리 [{0}/{1}]\n", this.tradeWinCount_, this.tradeCount_);
            log += string.Format("* 승율 [{0:##.##}]%\n", rate);

            return log;
        }

        public virtual double totalAccountMoney()
        {
            return accountMoney_;
        }
        //--------------------------------------------------------
        // 시간 처리
        public abstract int marketStartHour();

        public abstract bool nowStockMarketTime();
        public abstract DateTime marketStartTime();

        // 키움증권 증권사가 새벽 3~6시 사이 점검함.
        public abstract void checkShutdownTime();
        //--------------------------------------------------------
        public abstract void requestMyAccountInfo();

        public virtual void requestStockData(string code, bool forceBuy = false)
        {
            StockData stockData = this.getStockDataCode(code);
            if (stockData != null) {
                stockData.rquestdPriceDataTime_ = DateTime.Now;
            }
        }

        public abstract void saveToDBPriceAt(StockData stockData);

        public abstract void saveToDB(bool force = false);
        public abstract void loadFromDB();
        public abstract bool loadFromDB(StockData stockData);
        public abstract void updateTradeModuleList(StockTradeModuleList moduleList);
        public abstract void loadYesterDayFilterList();

        public void loadFromFile(string file)
        {
            try {
                var path = Path.GetDirectoryName(file);
                var fileName = Path.GetFileName(file);
                var fileExt = Path.GetExtension(file);
                var excelFile = path + "\\" + fileName;

              
                CultureInfo provider = CultureInfo.InvariantCulture;
                //2020/01/10-14:10:00
                var format = "yyyy/MM/dd-HH:mm:ss";

                ExcelParser parser = new ExcelParser(excelFile);
                //var sheetName = Path.GetFileNameWithoutExtension(file);
                parser.read();
                DataTable dt = parser.table().Tables[0];
                if (dt == null) {
                    Logger.getInstance.print(KiwoomCode.Log.에러, "{0} 파일에 테이블 {1} 없음", file, fileName);
                    return;
                }
                var token = fileName.Split('-');
                var code = token[0].Trim();
                var priceType = int.Parse(token[1].Trim());
                StockData stockData = null;

                if (priceType == this.priceTypeMin()) {
                    stockData = this.getStockDataCode(code);
                    if (stockData == null) {
                        stockData = this.getStockDataCode("M" + code);
                    }
                }
                if (stockData == null) {
                    Logger.getInstance.print(KiwoomCode.Log.에러, "{0} 파일에 해당되는 stockData 없음", file);
                    return;
                    // 또는
                    //stockData = new StockData()
                }
                foreach (DataRow row in dt.Rows) {
                    var dateStr = row[0].ToString().Trim();
                    var date = DateTime.ParseExact(dateStr, format, provider);
                    var start = double.Parse(row[1].ToString().Trim());
                    var high = double.Parse(row[2].ToString().Trim());
                    var low = double.Parse(row[3].ToString().Trim());
                    var close = double.Parse(row[4].ToString().Trim());
                    var vol = UInt64.Parse(row[5].ToString().Trim());

                    var candle = new CandleData(date, close, start, high, low, vol);
                    stockData.updatePrice(candle);
                }
            } catch (Exception e) {
                Logger.getInstance.print(KiwoomCode.Log.에러, "{0} 파일에 로딩 에러 {1}/{2}", file, e.Message, e.StackTrace);
            }
        }

        //--------------------------------------------------------
        public void clearBuyedStockTradeModule()
        {
            StockDataEach eachDo = (string code, StockData stockData) => {
                if (stockData.isBuyedItem() == false) {
                    return;
                }
                stockData.resetTradeModule(true);
            };
            this.doStocks(eachDo);
        }

        protected void checkStockTradeModule(StockData stockData)
        {
            // 로딩전
            if (stockData.priceDataCount() < PublicVar.priceTableCount) {
                return;
            }

            stockData.tradeModuleUpdateTime_ = stockData.nowDateTime();
            if (stockData.isBuyedItem()) {
                var tradeModule = stockData.tradeModule();
                if (tradeModule == null) {
                    this.researchTradeModuel(stockData, true);
                    return;
                }

                if (tradeModule.tradeCount_ == 0) {
                    stockData.checkAllSetTrade(this);

                    if (stockData.tradeModulesCount() == 0) {
                        this.researchTradeModuel(stockData, true);
                    }
                }
                return;
            }

            if (stockData.canBuy() == false) {
                return;
            }

            this.searchBestTradeModuleAnStock(stockData, true);
        }

        public void checkStocksTradeModule()
        {
            StockDataEach eachDo = (string code, StockData stockData) => {
                this.checkStockTradeModule(stockData);
            };

            this.doStocks(eachDo);
            this.updatePoolView();
            this.updateTradeModuleAllStock();
        }
                
        public void findEachFundmanagementModule()
        {
            StockDataEach eachDo = (string code, StockData stockData) => {              
                this.findEachFundmanagement(stockData);
            };

            Thread thread = new Thread(() => this.doStocks(eachDo));
            thread.Start();

            this.updatePoolView();
            this.updateTradeModuleAllStock();
        }

        protected abstract void findEachFundmanagement(StockData stockData);

        protected abstract void updateTradeModuleAllStock();

        public abstract void saveTradeModule(StockData stockData);

        public abstract void buyStock(string code, int tradeCount, double tradePrice);

        public abstract void sellStock(string code, int tradeCount, double tradePrice);

        public void activeTrade()
        {
            tradeReady_ = true;
        }

        public void trade(StockData stockData)
        {
            if (this.tradeReady_ == false) {
                return;
            }
            if (stockData.priceDataCount() < PublicVar.priceTableCount) {
                return;
            }
            if (stockData.canTrade_ == false) {
                return;
            }
           
            var stockFund = stockData.fundManagement_;
            if (stockFund != null) {
                stockFund.trade(stockData);
            } else { 
                this.fundManagement_.trade(stockData);
            }
        }

        public void tradeRealCandle(StockData stockData)
        {
            if (this.nowStockMarketTime() == false) {
                return;
            }
            if (stockData.isBuyedItem() == false) {
                return;
            }
            var stockFund = stockData.fundManagement_;
            if (stockFund != null) {
                stockFund.tradeRealCandle(stockData);
            }
            else {
                fundManagement_.tradeRealCandle(stockData);
            }
        }

        public void tradeForPayoff(bool checkMarketTime = true)
        {
            if (checkMarketTime) {
                if (this.nowStockMarketTime() == false) {
                    return;
                }
            }

            StockDataEach eachDo = (string code, StockData stockData) => {
                if (stockData.isBuyedItem()) {
                    this.trade(stockData);
                }
            };
            this.doStocks(eachDo);
        }

        public void tradeForBuy(bool checkMarketTime = true)
        {
            if (checkMarketTime) {
                if (this.nowStockMarketTime() == false) {
                    return;
                }
            }

            StockDataEach eachDo = (string code, StockData stockData) => {
                if (stockData.isBuyedItem() == false) {
                    this.trade(stockData);
                }
            };
            this.doStocks(eachDo);
        }

        public abstract void payOff(string code, string why);

        // 주식데이터, nowDateTime 중 가장 과거 시간을 리턴 시킴
        // 분동 데이터 갱신용
        public bool oldStockDateTime(out DateTime retDateTime)
        {
            bool searchSuccess = true;
            DateTime checkTime = DateTime.MinValue;

            StockDataEach eachDo = (string code, StockData stockData) => {
                if (stockData.canBuy() == false) {
                    return;
                }
                if (searchSuccess == false) {
                    return;
                }
                if (stockData.priceDataCount() == 0) {
                    searchSuccess = false;
                    return;
                }
                DateTime nowDate = stockData.nowDateTime();
                if (checkTime == DateTime.MinValue) {
                    checkTime = nowDate;
                    return;
                }
                if (nowDate < checkTime) {
                    checkTime = nowDate;
                }
            };
            this.doStocks(eachDo);
            retDateTime = checkTime;

            return searchSuccess;
        }
        //-----------------------------------------------------------------------
        // 백테스팅 관련
        // 각 주식에 대해 백테스팅 모듈 찾기
        public bool checkTradingAllow(BackTestRecoder recode, double lostCut)
        {
            if (this.tradeFilter_ == null) {
                return false;
            }
            return this.tradeFilter_.doFilter(recode, lostCut);
        }

        public virtual List<BackTestRecoder> tradingAllowFilter(List<BackTestRecoder> newList)
        {
            if (newList == null) {
                return null;
            }

            if (this.tradeFilter_ == null) {
                return null;
            }

            if (this.stockModuleSearcher_ == null) {
                return null;
            }

            IEnumerable<BackTestRecoder> orderList = this.tradeFilter_.doOrderBy(newList);
            if (orderList == null || orderList.Count() == 0) {
                return null;
            }
            return new List<BackTestRecoder>(orderList);
        }

        public string serachingStockName()
        {
            if (stockModuleSearcher_ == null) {
                return "NULL";
            }
            return this.stockModuleSearcher_.serachingItemName_;
        }

        public abstract void searchBestTradeModuleAnStock(StockData stockData, bool force = false);

        //------------------------------------------------------------------------//
        // 한 주식에 대해서 백테스팅 돌리는 코어 로직
        // 각 봇에서 따로 처리
        public abstract bool doBackTestAnStock(StockData stockData, StrategyModule buyStrategy, StrategyModule sellStragegy, ref BackTestRecoder recode);

        //-------------------------------------------------------------------------
        // 인디게이터의 조합으로 이루어진 모듈들을 가지고
        // 좋은 승률과 많은 거랫수를 가진 모듈을 찾아내는 함수.

        public void researchTradeModuel(StockData stockData, bool force = false)
        {
            stockData.resetTradeModule(force);
            stockData.searchBestTradeModule(this, force);
        }

        // 각 주식 항목마다 베스트 트레이딩 모듈을 찾는다.
        public void searchStockEachBestTradeModule(bool force = false)
        {
            StockDataEach eachDo = (string code, StockData stockData) => {
                if (stockData.isBuyedItem() && force == false) {
                    if (stockData.hasTradeModule()) {
                        if (stockData.tradeModulesCount() == 0) {
                            stockData.checkAllSetTrade(this);
                        }
                        if (stockData.tradeModulesCount() != 0) {
                            return;
                        }
                    }
                    return;
                }
                stockData.resetTradeModule();
                stockData.searchBestTradeModule(this, true);
            };
            this.doStocks(eachDo);
        }

        //-------------------------------------------------------------------------
        public double usesCpu()
        {
            if (this.prcessCpu_ == null) {
                return -100.0f;
            }

            int cpuCount = Environment.ProcessorCount;
            return this.prcessCpu_.NextValue() / cpuCount;
        }

        public double usesMem()
        {
            Process proc = Process.GetCurrentProcess();
            return proc.PrivateMemorySize64 / 1024 / 1024;
        }
    }
}
