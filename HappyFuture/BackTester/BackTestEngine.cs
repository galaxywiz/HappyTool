using HappyFuture.DB;
using HappyFuture.DialogControl.FutureDlg;
using HappyFuture.FundManage;
using HappyFuture.Officer;
using StockLibrary;
using StockLibrary.StrategyManager;
using StockLibrary.StrategyManager.StrategyModuler;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using UtilLibrary;

//-> 괜찮은 전략??
// https://m.blog.naver.com/juun41/221546431240

// 추후 기존 1,2주간 기록을 바탕으로
// 각 종목당 최적의 blance 전략을 조합 시키고
// 그걸 해당 종목에 적용 시켜 매매를 해보자

namespace HappyFuture.TradeStrategyFinder
{
    public class WinTradeModule
    {
        public BackTestRecoder module_;
        public int count_;

        public WinTradeModule(BackTestRecoder recoder)
        {
            module_ = new BackTestRecoder();
            module_.buyTradeModule_ = recoder.buyTradeModule_;
            module_.sellTradeModule_ = recoder.sellTradeModule_;
            count_ = 1;
        }

        public void addCount()
        {
            count_++;
        }
    }

    public class BackTestEngine
    {
        internal TestBot bot_ = null;
        internal FuturePriceDB priceDB_ = null;
        internal TradeStrategyTestResult origin_;

        protected List<DateTime> timeLine_ = new List<DateTime>();
        protected double initAccountMoney_;
        protected double accumulateWeekProfit_ = 0.0f;

        internal BackTestSimulView simulView_ = null;
        internal TextBox testAccountBox_;
        internal ProgressBarInfo progressBarInfo_ = null;
        internal ControlPrint info_ = null;
        internal ControlPrint nowRet_ = null;

        StockData stockData_ = null;    // 이게 들어 있으면, 해당 종목에 대해서만 테스팅을 한다.
        bool strategySave_ = true;
        public BackTestEngine(StockData stockData)
        {
            var startTime = new DateTime(2017, 1, 1);
            var endTime = DateTime.Now;
            stockData_ = stockData.Clone() as StockData;
            this.setup(startTime, endTime);
        }

        public BackTestEngine(DateTime startTime, DateTime endTime)
        {
            this.setup(startTime, endTime);
        }

        bool isOneStockTest()
        {
            if (stockData_ != null) {
                return true;
            }
            return false;
        }

        protected virtual void setup(DateTime startTime, DateTime endTime)
        {
            this.bot_ = new TestBot();
            this.startTime_ = startTime;
            this.endTime_ = endTime;

            var bot = ControlGet.getInstance.futureBot();
            if (bot.futureDBHandler_ == null) {
                return;
            }
            this.priceDB_ = bot.futureDBHandler_.priceDB_;

            var fund = bot.fundManagement_ as FutureFundManagement;
            this.origin_ = new TradeStrategyTestResult(fund, PublicFutureVar.lostCutTime, PublicFutureVar.targetProfitTime);

            this.setGoogleLog();
            this.bot_.isTotalTest_ = true;

            this.setupControl();
            this.loadInitData();
        }

        ~BackTestEngine()
        {
            this.bot_ = null;
        }

        protected DateTime startTime_;
        protected DateTime endTime_;

        protected void setupControl()
        {
            var dlg = Program.happyFuture_.futureDlg_;
            this.testAccountBox_ = dlg.textBox_testAccount;

            double DEFAULT_MONEY = 100000;// ControlGet.getInstance.futureBot().accountMoney_;
            if (this.testAccountBox_.Text.ToString().Length == 0) {
                this.setMoneyBox(DEFAULT_MONEY);
                this.initAccountMoney_ = DEFAULT_MONEY;
            }
            else {
                if (double.TryParse(this.testAccountBox_.Text, out this.initAccountMoney_) == false) {
                    this.setMoneyBox(DEFAULT_MONEY);
                }
            }
            Program.happyFuture_.futureDlg_.setEnableTestButtons(false);

            if (isOneStockTest()) {
                return;
            }
            this.progressBarInfo_ = new ProgressBarInfo();
            this.progressBarInfo_.setup(dlg.progressBar_backTest);

            this.info_ = new ControlPrint(dlg.label_backTestInfo);
            this.nowRet_ = new ControlPrint(dlg.backTestLog_TextBox);

            this.simulView_ = new BackTestSimulView();
            this.simulView_.setup(dlg.DataGridView_backTestResult);
        }

        protected bool loadInitData()
        {
            // 1. 테이블 리스트 갖고 오기
            this.getFutureTable();
            // 2. 각 테이블 데이터 다 갖고 오기
            this.getFutureData();
            return this.setTimeLine();
        }

        protected List<string> codePool_ = new List<string>();
        void getFutureTable()
        {
            codePool_.Clear();
            this.priceDB_.getTableList(ref this.codePool_);

            if (isOneStockTest()) {
                var code = stockData_.regularCode();
                if (codePool_.Exists(x => x == code)) {
                    codePool_.Clear();
                    codePool_.Add(code);
                }
                else {
                    codePool_.Clear();
                }
                return;
            }
        }

        protected Dictionary<string, StockData> stockPool_ = new Dictionary<string, StockData>();
        protected List<Dictionary<string, StockData>> refStockPool_ = new List<Dictionary<string, StockData>>();
        protected virtual void getFutureData()
        {
            refStockPool_.Clear();
            foreach (REF_PRICE_TYPE refType in Enum.GetValues(typeof(REF_PRICE_TYPE))) {
                refStockPool_.Add(new Dictionary<string, StockData>());
            }

            stockPool_.Clear();
            var bot = ControlGet.getInstance.futureBot();
            var min = bot.priceTypeMin();
            this.priceDB_.selectForSimule(this.codePool_, this.startTime_, this.endTime_, min, ref this.stockPool_);
            bot_.copyRefStocks();

            //var pool = refStockPool_[(int) REF_PRICE_TYPE.중간_분봉];
            //var elop = endTime_ - startTime_;
            //this.priceDB_.selectForSimule(this.codePool_, this.startTime_.AddMinutes(-elop.TotalMinutes * 3), this.endTime_, 30, ref pool);
            //pool = refStockPool_[(int) REF_PRICE_TYPE.시간_분봉];
            //this.priceDB_.selectForSimule(this.codePool_, this.startTime_.AddMinutes(-elop.TotalMinutes * 6), this.endTime_, 60, ref pool);
        }

        bool setTimeLine()
        {
            foreach (var keyValue in this.stockPool_) {
                var stockData = keyValue.Value;
                var priceTable = stockData.priceTable();
                foreach (var candle in priceTable) {
                    if (this.timeLine_.Contains(candle.date_) == false) {
                        this.timeLine_.Add(candle.date_);
                    }
                }
            }

            if (timeLine_.Count() < PublicFutureVar.defaultCalcCandleCount) {
                return false;
            }
            var query = from candle in timeLine_
                        orderby candle ascending
                        select candle;
            this.timeLine_ = new List<DateTime>(query);

            // @@@ 일봉 보기 일단 무시
            //var first = timeLine_.First();
            //var last = timeLine_.Last();
            //var elop = last - first;
            //var initTime = last.AddMinutes(-elop.TotalMinutes * 6);
            //for (; initTime <= last; initTime = initTime.AddMinutes(10)) {
            //    for (var type = REF_PRICE_TYPE.중간_분봉; type <= REF_PRICE_TYPE.시간_분봉; ++type) {
            //        var pool = refStockPool_[(int) type];
            //        foreach (var keyValue in pool) {
            //            var code = keyValue.Key;
            //            var stockData = keyValue.Value;

            //            var candle = stockData.findCandle(initTime);
            //            if (candle != null) {
            //                this.bot_.addRefCandle(code, candle, type, this.testLostCutTime_, this.testProfitTime_);
            //            }
            //        }
            //    }
            //}
            return true;
        }

        protected void addCandleToBot(DateTime nowDateTime)
        {
            foreach (var keyValue in this.stockPool_) {
                var code = keyValue.Key;
                var stockData = keyValue.Value;

                var candle = stockData.findCandle(nowDateTime);
                if (candle != null) {
                    this.bot_.addCandle(code, candle, this.testLostCutTime_, this.testProfitTime_);
                }
                else {
                    var botStockData = this.bot_.getStockDataCode(code);
                    if (botStockData == null) {
                        return;
                    }
                    candle = botStockData.realCandle();
                    var fakeCandle = new CandleData(nowDateTime, candle.price_, candle.startPrice_, candle.highPrice_, candle.lowPrice_, candle.volume_);
                    this.bot_.addCandle(code, fakeCandle, this.testLostCutTime_, this.testProfitTime_);
                }
            }
            // @@@ 일봉 보기 일단 무시
            //for (var type = REF_PRICE_TYPE.중간_분봉; type <= REF_PRICE_TYPE.시간_분봉; ++type) {
            //    var pool = refStockPool_[(int) type];
            //    foreach (var keyValue in pool) {
            //        var code = keyValue.Key;
            //        var stockData = keyValue.Value;

            //        var candle = stockData.findCandle(nowDateTime);
            //        if (candle != null) {
            //            this.bot_.addRefCandle(code, candle, type, this.testLostCutTime_, this.testProfitTime_);
            //        }
            //    }
            //}
        }
        protected int testCount_ = 0;
        protected int totalTestCount_ = 0;
        public bool processingTest_ = false;

        // 레코더 갱신시 정보창 갱신
        void updateInfoLog(TradeStrategyTestResult result, bool final = false)
        {
            var recoder = this.bot_.recoder_;
            if (recoder.tradingCount() == this.recodeCount_) {
                return;
            }

            this.recodeCount_ = recoder.tradingCount();
            if (nowRet_ != null) {
                var rsq = recoder.rsq_;
                var log = new StringBuilder();
                log.AppendFormat("## [lost:{0}, profit:{1}] ", result.lostCut_, result.targetProfit_);
                log.AppendFormat("[목표율 달성일 {0}, 진행일{1}] ", result.daysForAchieveGoalRate_, result.days_);
                log.AppendFormat("[진행중인 테스트 [{0}/{1}]]\n", this.testCount_, this.totalTestCount_);
                log.Append(recoder.getLog());
                this.nowRet_.print(log.ToString());
            }

            if (simulView_ != null) {
                DataTable dt = recoder.getRecodeDataTable(final);
                this.simulView_.print(dt);
            }
            var account = this.bot_.totalAccountMoney();
            this.setMoneyBox(account);
        }

        // 매월 시세 조회 비용 차감을 위함.
        DateTime payMarketFee_ = DateTime.MinValue;
        void subMarketFee(DateTime now)
        {
            if (isOneStockTest()) {
                return;
            }
            if (payMarketFee_ != DateTime.MinValue) {
                if (payMarketFee_.Month == now.Month) {
                    return;
                }
            }
            payMarketFee_ = now;

            const double MARKET_FEE = -180;
            this.bot_.accountMoney_ += MARKET_FEE;
            BackTestRecode recode = new BackTestRecode {
                code_ = "시세 수수료",
                buyModule_ = "없음",
                sellModule_ = "없음",
                tradeModuelAvgProfit_ = 0,
            };
            recode.buyTime_ = now;
            recode.payoffTime_ = now;
            recode.profit_ = MARKET_FEE;
            recode.grossEarnings_ = this.bot_.recoder_.calcTotalProfit() - MARKET_FEE;
            this.bot_.recoder_.addRecode(recode);

            this.bot_.pullingMoney();
            this.bot_.pullingNextYear(now);
        }

        void processTest(TradeStrategyTestResult result)
        {
            this.processingTest_ = true;

            this.setEnableMoneyBox(false);
            if (this.progressBarInfo_ != null) {
                this.progressBarInfo_.setMax(this.timeLine_.Count);
                this.progressBarInfo_.setInit();
            }
            this.testCount_++;
            bool brakeDown = false;
            var fund = this.bot_.fundManagement_ as FutureFundManagement;

            if (timeLine_.Count() < PublicFutureVar.defaultCalcCandleCount) {
                return;
            }
            try {
                foreach (var nowDateTime in this.timeLine_) {
                    if (this.progressBarInfo_ != null) {
                        this.progressBarInfo_.performStep();
                    }
                    if (this.info_ != null) {
                        var newWeekDays = new string[] { "일", "월", "화", "수", "목", "금", "토" };
                        var fundName = fund.name();
                        this.info_.print(string.Format("{0} 전략 / {1} ({2}) 진행중", fundName, nowDateTime, newWeekDays[(int) nowDateTime.DayOfWeek]));
                    }

                    this.subMarketFee(nowDateTime);

                    // tradeModule 체크
                    if (this.isTodayStartTime(nowDateTime)) {
                        this.bot_.updateTodayProfit();
                        result.days_++;
                        brakeDown = false;
                        this.bot_.initSafeTrade();
                    }
                    if (brakeDown) {
                        continue;
                    }

                    // 정리
                    if (this.isWeekStartTime(nowDateTime)) {
                        this.bot_.clearBuyedStockTradeModule();
                        this.bot_.clearWinRate();
                    }
                    this.bot_.checkMarginCanTrade();
                    // 지금 시간대 캔들 데이터 주입시키고
                    this.addCandleToBot(nowDateTime);

                    // buy카운트 재 계산
                    this.bot_.setBuyAbleCount();

                    this.bot_.checkStocksTradeModule();

                    if (this.bot_.breakDownTrade()) {
                        result.daysForAchieveGoalRate_++;
                        brakeDown = true;
                        continue;
                    }

                    // 실제 매매 로직
                    this.bot_.tradeForPayoff(false);
                    this.bot_.tradeForBuy(false);
                    //  this.bot_.tradeInRealCandle();

                    this.updateInfoLog(result);

                    if (this.processingTest_ == false) {
                        break;
                    }

                    if (bot_.checkStrategyFail()) {
                        break;
                    }
                }
                if (this.progressBarInfo_ != null) {
                    this.progressBarInfo_.setInit();
                }

                this.bot_.allPayOff();
                this.updateInfoLog(result, true);
            }
            catch (Exception e) {
                Logger.getInstance.print(KiwoomCode.Log.에러, "시뮬레이션 도중 에러, {0}", e.Message);
            }
        }

        void setEnableMoneyBox(bool flag)
        {
            if (isOneStockTest()) {
                return;
            }
            if (this.testAccountBox_.InvokeRequired) {
                this.testAccountBox_.BeginInvoke(new Action(() => this.testAccountBox_.Enabled = flag));
            }
            else {
                this.testAccountBox_.Enabled = flag;
            }
        }

        void setMoneyBox(double money)
        {
            if (isOneStockTest()) {
                return;
            }
            if (this.testAccountBox_.InvokeRequired) {
                this.testAccountBox_.BeginInvoke(new Action(() => this.testAccountBox_.Text = money.ToString()));
            }
            else {
                this.testAccountBox_.Text = money.ToString();
            }
        }

        int recodeCount_ = 0;
        protected void initValue()
        {
            this.stopWatch_.Reset();
            this.bot_.clearAllStocks();
            this.recodeCount_ = 0;
        }

        protected Stopwatch stopWatch_ = new Stopwatch();
        internal List<TradeStrategyTestResult> backTestResultList_ = new List<TradeStrategyTestResult>();

        int saveCount_ = 0;
        internal void saveReport(ref TradeStrategyTestResult result)
        {
            try {
                var recoder = this.bot_.recoder_;
                var fundManage = this.bot_.fundManagement_ as FutureFundManagement;
                result.totalAccount_ = bot_.accountMoney_;

                result.machineRecoder_ = recoder;
                if (this.isAppropriateResults(result)) {
                    if (isOneStockTest()) {
                        stockData_.backTestResultList_.Add(result);
                    }
                    else {
                        this.backTestResultList_.Add(result);
                    }
                    if ((++saveCount_ % 10) == 0) {
                        this.saveFundManageResult();
                    }
                }
                string log = this.sendGoogleLog("종합 시뮬", ref result);
                if (this.nowRet_ != null) {
                    this.nowRet_.print(log);
                }
            }
            catch (Exception e) {
                Logger.getInstance.print(KiwoomCode.Log.에러, "{0}/{1}", e.Message, e.StackTrace);
            }
        }

        double testLostCutTime_;
        double testProfitTime_;

        protected bool isTodayStartTime(DateTime time)
        {
            if (this.bot_.isSummerTime()) {
                if (time.Hour == 7 && time.Minute == 0) {
                    return true;
                }
                return false;
            }

            if (time.Hour == 8 && time.Minute == 0) {
                return true;
            }
            return false;
        }

        public bool isWeekStartTime(DateTime time)
        {
            if (this.isTodayStartTime(time) == false) {
                return false;
            }

            if (time.DayOfWeek != DayOfWeek.Monday) {
                return false;
            }

            return true;
        }

        internal void backTest(ref TradeStrategyTestResult result)
        {
            this.testLostCutTime_ = result.lostCut_;
            this.testProfitTime_ = result.targetProfit_;
            this.bot_.setFundManagement(result.fundManage_);

            var money = this.initAccountMoney_;
            this.bot_.saveMyMoney_ = 0;
            // 나중에 여길 모듈 조립 식으로 변경해서 가장 좋은 해를 찾도록 변경하자
            this.bot_.initRecode();
            this.bot_.initAccountMoney_ = money;
            this.bot_.accountMoney_ = money;
            this.bot_.yesterAccountMoney_ = money;
            this.initValue();

            this.stopWatch_.Restart();
            // 3. 특정 일 지정 해서 데이터 체워넣기
            // 지금 갖고 있는 데이터 기준으로 2018-10-12일 부터 시작 하면 될듯
            // 4. 마켓풀을 가지고 bot에 주입
            this.processTest(result);
            this.stopWatch_.Stop();

            // 5. account 결과와 log 표 / 차트로 정리
            this.saveReport(ref result);
        }

        internal TestBotLogForGoogleSheet logGoogleSheet_ = null;
        protected void setGoogleLog()
        {
            string sheetId = "1VL49mcb77V2eTufsgjxa8FYVOGDRTuzFKJOG7j8Z_Js";
            string sheetName = "해선테스트";
            this.logGoogleSheet_ = new TestBotLogForGoogleSheet(sheetId, sheetName);
        }

        protected string sendGoogleLog(string categorize, ref TradeStrategyTestResult result)
        {
            if (this.logGoogleSheet_ == null) {
                return "";
            }

            var recoder = this.bot_.recoder_;
            if (recoder.tradingCount() == 0) {
                return "";
            }

            var fundManage = this.bot_.fundManagement_ as FutureFundManagement;
            var rsq = result.rsq();
            var totalMin = recoder.endTime_ - recoder.startTime_;
            var nextProfit = result.predictedProfit((int) totalMin.TotalMinutes);
            var title = isOneStockTest() ? stockData_.regularCode() : "TOTAL";

            StringBuilder log = new StringBuilder();
            log.AppendFormat("- 로스컷 배율 : {0}, 타겟 배율: {1}, 목표율 달성일 [{2} / {3}] \n", result.lostCut_, result.targetProfit_, result.daysForAchieveGoalRate_, result.days_);
            log.AppendFormat("- 종목 : {0}\n", title);
            log.AppendFormat("{0}", recoder.getLog());
            log.AppendFormat("- 시뮬 총 시간 {0} \n", this.stopWatch_.Elapsed);

            Logger.getInstance.print(KiwoomCode.Log.주식봇, log.ToString());
            var eval = result.overallAssessment((int) totalMin.TotalMinutes);
            if (this.logGoogleSheet_ != null) {
                IList<Object> obj = new List<Object>();
                obj.Add(title);
                obj.Add(fundManage.name());
                obj.Add(fundManage.strategyModule_.name());
                obj.Add(recoder.startTime_);                // 거래시작
                obj.Add(recoder.endTime_);                  // 거래 끝
                obj.Add(totalMin.TotalDays);                // 총거래시간
                obj.Add(recoder.winCount_);                 // 승리횟수
                obj.Add(recoder.tradingCount());            // 총거래횟수
                obj.Add(recoder.winRate_);                  // 승률
                obj.Add(this.initAccountMoney_);            // 시작 예수금
                obj.Add(result.totalAccount_);              // 마지막 예수금

                obj.Add(recoder.grossEarnings_);            // 총이익
                obj.Add(recoder.saveMyMoney_);              // 나의 몫
                //obj.Add(recoder.grossEarningsMinus_);       // 역매매시 이익
                obj.Add(rsq);                               // rsq
                obj.Add(nextProfit);                        // 거래기간만큼 후의 예상 이익
                obj.Add(result.avgProfit());                // 평균이익
                obj.Add(result.itemCount());               // 거래 항목
                obj.Add(result.deviation());                // 거래항목간 표준편차
                obj.Add(result.machineRecoder_.avgMinProfit_); // 평균 최소이익
                obj.Add(result.machineRecoder_.avgMaxProfit_); // 평균 최대 이익
                obj.Add(result.machineRecoder_.minProfit_); // 평균 최소이익
                obj.Add(result.machineRecoder_.maxProfit_); // 평균 최대 이익
                obj.Add(DateTime.Now);                      // 업데이트시간
                obj.Add(eval);                              // 평가치
                obj.Add(result.machineRecoder_.minProfitRate_); //1일 최소 수익율
                obj.Add(result.machineRecoder_.maxProfitRate_); // 1일 최대 수익율
                var profitRate = Util.calcProfitRate(this.initAccountMoney_, (this.initAccountMoney_ + recoder.grossEarnings_)) * 100;
                obj.Add(profitRate);            // 전체 수익율
                obj.Add(result.daysForAchieveGoalRate_);       // 테스트중 수익 달성한 날
                obj.Add(PublicFutureVar.tradePeriod + "/" + PublicVar.testLog);

                this.logGoogleSheet_.addLog(obj);
            }

            // 평가 값이 합격해야 텔레그램 송신
            if (this.isAppropriateResults(result) == false) {
                return log.ToString();
            }

            var dlg = FutureDlgInfo.getInstance;
            dlg.caputreForm();
            string image = dlg.captureFormImgName();
            this.bot_.telegram_.sendPhoto(image, log.ToString());

            string path = Application.StartupPath + "\\BackTestResult";
            DirectoryInfo di = new DirectoryInfo(path);
            if (di.Exists == false) {
                di.Create();
            }

            string fileName = string.Format("{0}\\{1}_{2}_{3}",
                path, title, fundManage.strategyModule_.dbString(), DateTime.Now.ToString("yyyyMMddHHmm"));

            ExcelParser parser = new ExcelParser(fileName);
            var dt = recoder.getRecodeDataTable(true);
            parser.save(dt);

            return log.ToString();
        }

        protected void findEachFundManagementTest()
        {
            var ffManageList = FutureFundManagementList.getInstance;
            int i = 0;
            foreach (var keyVal in ffManageList.pool_) {
                var entrieName = keyVal.Value;
                var fund = ffManageList.getFundManagementCombineStrategyModule(entrieName, this.bot_);
                if (fund == null) {
                    continue;
                }
                fund.lostCutTime_ = PublicFutureVar.lostCutTime;
                fund.targetProfitTime_ = PublicFutureVar.targetProfitTime;
                fund.rankIdx_ = i++;

                var result = new TradeStrategyTestResult(fund, PublicFutureVar.lostCutTime, PublicFutureVar.targetProfitTime);
                this.backTest(ref result);
                if (this.processingTest_ == false) {
                    return;
                }
            }
            this.saveFundManageResult();
        }

        // 아래 조건이 충족해야, 좀 쓸만한 전략
        protected bool isAppropriateResults(TradeStrategyTestResult result)
        {
            const double LIMIT_PROFIT = 0;

            if (result.avgProfit() < PublicFutureVar.pureFeeTex) {
                return false;
            }

            if (result.totalProfit() < 1000) {
                return false;
            }

            var elpe = result.machineRecoder_.endTime_ - result.machineRecoder_.startTime_;
            var tradeRate = result.tradeCount() / elpe.TotalDays;
            //if (tradeRate < 0.015) {
            //    return false;
            //}
            if (result.overallAssessment((int) elpe.TotalMinutes) < LIMIT_PROFIT) {
                return false;
            }

            return true;
        }

        public IEnumerable<TradeStrategyTestResult> resultSort(List<TradeStrategyTestResult> backTestResultList, int limit = 50)
        {
            var predicMin = this.endTime_ - this.startTime_;
            var query = (from testRecode in backTestResultList
                         orderby testRecode.overallAssessment((int) predicMin.TotalMinutes) descending
                         select testRecode).Take(limit);

            //int min = PublicVar.fundUpdateHours * 60;

            //// 결과 찍기
            //foreach (var recode in query) {
            //    var log = new StringBuilder();
            //    log.AppendFormat("name: {0}, lostCut: {1}, target: {2}, totalProfit: {3}, rsq: {4}, predic: {5}",
            //        code, recode.lostCut_, recode.targetProfit_, recode.totalProfit(), recode.rsq(), recode.predictedProfit(min));
            //    Logger.getInstance.print(KiwoomCode.Log.주식봇, log.ToString());
            //}

            return query;
        }

        //------------------------------------------------------------------------------------------//
        // db저장용
        protected void saveFundManageResult()
        {
            if (strategySave_ == false) {
                return;
            }
            try {
                var futureBot = ControlGet.getInstance.futureBot();

                if (isOneStockTest()) {
                    var futureData = stockData_ as FutureData;
                    var results = this.resultSort(futureData.backTestResultList_);
                    if (results == null) {
                        Logger.getInstance.print(KiwoomCode.Log.주식봇, "{0}의 평가에 마땅한 전략이 없음. ㅠㅠ", futureData.name_);
                        return;
                    }
                    if (results.Count() == 0) {
                        return;
                    }
                    futureData.fundList_.Clear();
                    foreach (var module in results) {
                        var fund = module.fundManage_ as FutureFundManagement;
                        var key = fund.strategyModule_.name();
                        if (futureData.fundList_.ContainsKey(key) == false) {
                            futureData.fundList_.Add(key, fund);
                        }
                    }

                    futureBot.updateFundPool(futureData);
                    return;
                }

                // 전체 종목 단일 테스트에 대한 취합 및 저장
                var resultList = this.resultSort(this.backTestResultList_);
                if (resultList == null) {
                    Logger.getInstance.print(KiwoomCode.Log.주식봇, "종합 평가에 마땅한 전략이 없음. ㅠㅠ");
                    return;
                }

                var autoPool = FutureFundManagementList.getInstance.universalPool_;
                autoPool.Clear();

                foreach (var module in resultList) {
                    var fund = module.fundManage_ as FutureFundManagement;
                    var key = fund.strategyModule_.name();
                    if (autoPool.ContainsKey(key) == false) {
                        autoPool.Add(key, fund);
                    }
                }

                futureBot.updateUniversalFundPool();
            }
            catch (Exception e) {
                Logger.getInstance.print(KiwoomCode.Log.에러, "{0}/{1}", e.Message, e.StackTrace);
            }
        }

        void doTestMA(List<EVALUATION_DATA> fastList, List<EVALUATION_DATA> slowList, List<EVALUATION_DATA> upList, List<EVALUATION_DATA> pfList, List<EVALUATION_DATA> psList, LONG_SHORT_TRADE longShortTrade)
        {
            foreach (var bFast in fastList) {
                foreach (var bSlow in slowList) {
                    if (bSlow <= bFast) {
                        continue;
                    }
                    foreach (var up in upList) {
                        if (bSlow < up) {
                            continue;
                        }
                        foreach (var pFast in pfList) {
                            foreach (var pSlow in psList) {
                                if (pSlow <= pFast) {
                                    continue;
                                }
                                var ffManageList = FutureFundManagementList.getInstance;
                                var fund = ffManageList.getFundManagementCombineStrategyModule("GoldenCrossStrategyModule", this.bot_);
                                if (fund == null) {
                                    return;
                                }
                                var maCross = fund.strategyModule_ as GoldenCrossStrategyModule;
                                maCross.setMA(bFast, bSlow, up, pFast, pSlow);
                                // maCross.setMA(bFast, bSlow, up);
                                maCross.setBettor(new LongTermAssetBettor());
                                // maCross.setBettor(new CumulativeAssetBettor());
                                maCross.tradeLongShort_ = longShortTrade;
                                var result = new TradeStrategyTestResult(fund, PublicFutureVar.lostCutTime, PublicFutureVar.targetProfitTime);
                                this.backTest(ref result);
                                if (this.processingTest_ == false) {
                                    return;
                                }
                            }
                        }

                    }
                }
            }
        }

        delegate void TestFunc(EVALUATION_DATA data);
        void doTestMA()
        {
            var fastList = new List<EVALUATION_DATA>();
            var slowList = new List<EVALUATION_DATA>();
            var upList = new List<EVALUATION_DATA>();
            var pfList = new List<EVALUATION_DATA>();
            var psList = new List<EVALUATION_DATA>();

            TestFunc testFunc = (ma) => {
                fastList.Clear();
                fastList.Add(ma + 1);   //sma 5
                fastList.Add(ma + 2);   //sma10
                fastList.Add(ma + 3);   //sma20

                slowList.Clear();
                slowList.Add(ma + 3);   //sma20
                slowList.Add(ma + 4);   //sma50
                slowList.Add(ma + 5);   //sma100
                slowList.Add(ma + 6);   //sma200

                upList.Clear();
                upList.Add(ma + 5);   //sma100
                upList.Add(ma + 6);   //sma200

                pfList.Clear();
                pfList.Add(ma + 1);   //sma 5
                pfList.Add(ma + 2);   //sma10
                pfList.Add(ma + 3);   //sma20

                psList.Clear();
                psList.Add(ma + 3);   //sma20
                psList.Add(ma + 4);   //sma50
                psList.Add(ma + 5);   //sma100
                psList.Add(ma + 6);   //sma200

                //this.doTestMA(fastList, slowList, upList, pfList, psList, LONG_SHORT_TRADE.ONLY_LONG);
                //this.doTestMA(fastList, slowList, upList, pfList, psList, LONG_SHORT_TRADE.ONLY_SHORT);
                this.doTestMA(fastList, slowList, upList, pfList, psList, LONG_SHORT_TRADE.BOTH);
            };
            testFunc(EVALUATION_DATA.SMA_3);
            testFunc(EVALUATION_DATA.EMA_3);
            testFunc(EVALUATION_DATA.WMA_3);
        }

        void doTestDevStrategy()
        {
            var assetList = AssetBettorList.getInstance;
            foreach (var asset in assetList.bettorList_) {

                var ffManageList = FutureFundManagementList.getInstance;
           //     var fund = ffManageList.getFundManagementCombineStrategyModule("MACD_StrategyModule", this.bot_);
                var fund = ffManageList.getFundManagementCombineStrategyModule("PriceChanelStrategyModule", this.bot_);
                if (fund == null) {
                    return;
                }
                fund.strategyModule_.setBettor(asset);
                var result = new TradeStrategyTestResult(fund, PublicFutureVar.lostCutTime, PublicFutureVar.targetProfitTime);
                this.backTest(ref result);
                if (this.processingTest_ == false) {
                    return;
                }
            }
        }

        void doAllStrategyTest()
        {
            //this.combinProfitLostTest("BackTestFundManagement");
            //this.combinProfitLostTest("AI_StrategyModule");

            //this.combinProfitLostTest("LarryRStrategyModule");
            //this.combinProfitLostTest("MaStrategyModule");
            //this.combinProfitLostTest("PriceChanelStrategyModule");

            //this.combinProfitLostTest("HitTheBullsEyesFundManagement");

            //this.combinProfitLostTest("BollingerReverseFundManagement");

            //this.combinProfitLostTest("BollingerFundManagement");

            // 위의 조합으로 찾은걸 테스트
            //   this.findEachFundManagementTest();
            //     this.bot_.updateTradeModuleList();

            //this.doTestFundList();
            this.doTestDevStrategy();
            this.doTestMA();

            this.saveFundManageResult();

            this.setEnableMoneyBox(true);
            Program.happyFuture_.futureDlg_.setEnableTestButtons(true);
            MessageBox.Show("전체 테스트 완료");
        }

        void doDBStrategyTest()
        {
            var bot = ControlGet.getInstance.futureBot();
            bot.selectUniversalFundPool();
            var fundList = FutureFundManagementList.getInstance;
            
            var assetList = AssetBettorList.getInstance;
            var autoPool = fundList.universalPool_;
            
            strategySave_ = false;
            foreach (var pair in autoPool) {
                foreach (var asset in assetList.bettorList_) {
                    var strategy = pair.Value;
                    var ffManageList = FutureFundManagementList.getInstance;
                    strategy.strategyModule_.setBettor(asset);
                    var result = new TradeStrategyTestResult(strategy, PublicFutureVar.lostCutTime, PublicFutureVar.targetProfitTime);
                    this.backTest(ref result);
                    if (this.processingTest_ == false) {
                        return;
                    }
                }
            }
            strategySave_ = true;
        }

        public void run(bool isAllTestMode = true)
        {
            var bot = ControlGet.getInstance.futureBot();
            Logger.getInstance.logActive_ = false;

            bot.telegram_.sendMessage("테스트 시작");
            Thread thread;
            if (isAllTestMode) {
                thread = new Thread(() => this.doAllStrategyTest());
            }
            else {
                thread = new Thread(() => this.doDBStrategyTest());
            }
            thread.Start();

            Logger.getInstance.logActive_ = true;
        }

        public void doCoreTest()
        {
            this.findEachFundManagementTest();
            var dlg = Program.happyFuture_.futureDlg_;

            this.setEnableMoneyBox(true);
            dlg.setEnableTestButtons(true);
        }

        public void runEachStockDataFind()
        {
            var bot = ControlGet.getInstance.futureBot();
            Logger.getInstance.logActive_ = false;

            bot.telegram_.sendMessage("{0} 테스트 시작", stockData_.name_);
            Thread thread = new Thread(() => this.doCoreTest());
            thread.Start();
            // thread.Join();
            Logger.getInstance.logActive_ = true;
        }
    }
}
