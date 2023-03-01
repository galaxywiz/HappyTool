using System;
using KiwoomCode;
using HappyTool.Stock;
using HappyTool.DialogControl.StockDialog;
using HappyTool.Dlg;
using UtilLibrary;
using StockLibrary;
using KiwoomEngine;
using System.Globalization;

namespace HappyTool
{       
    public abstract class StockStatement : Statement
    {
        // 화면번호 생산
        public StockStatement()
        {
            receivedTick_ = 0;
        }
        // 주식 모듈로 결과를 받는 부분
        public abstract void receive(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrDataEvent apiEvent);

        //--------------------------------------------------------//
        // 매크로
        protected bool setParam(string columName, string value)
        {
            try {
                StockEngine engine = ControlGet.getInstance.stockBot().engine_;
                engine.khOpenApi().SetInputValue(columName, value);
                return true;
            } catch (AccessViolationException execption) {
                Logger.getInstance.print(Log.에러, "[setParam:{0}, {1}] {2}\n{3}\n{4}", columName, value, execption.Message, execption.StackTrace, execption.InnerException);
            }
            return false;
        }

        protected int getRowCount(AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrDataEvent apiEvent)
        {
            try {
                StockEngine engine = ControlGet.getInstance.stockBot().engine_;
                int count = engine.khOpenApi().GetRepeatCnt(apiEvent.sTrCode, apiEvent.sRQName);
                return count;
            } catch (AccessViolationException execption) {
                Logger.getInstance.print(Log.에러, "[getRowCount:{0}] {1}\n{2}\n{3}", apiEvent.sRQName, execption.Message, execption.StackTrace, execption.InnerException);
            }
            return 0;
        }

        protected string getData(string columName, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrDataEvent apiEvent, int index)
        {
            try {
                StockEngine engine = ControlGet.getInstance.stockBot().engine_;
                string data = engine.khOpenApi().CommGetData(apiEvent.sTrCode, "", apiEvent.sRQName, index, columName).Trim();
                return data;
            } catch (AccessViolationException execption) {
                Logger.getInstance.print(Log.에러, "[getData:{0}] {1}\n{2}\n{3}", apiEvent.sRQName, execption.Message, execption.StackTrace, execption.InnerException);
            }
            return "";
        }
        protected double getDouble(string columName, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrDataEvent apiEvent, int index)
        {
            string val = this.getData(columName, apiEvent, index);
            double valDouble = 0;
            if (double.TryParse(val, out valDouble)) {
                return valDouble;
            }
            return double.MaxValue;
        }

        protected UInt64 getUInt64(string columName, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrDataEvent apiEvent, int index)
        {
            string val = this.getData(columName, apiEvent, index);
            UInt64 valDouble = 0;
            if (UInt64.TryParse(val, out valDouble)) {
                return valDouble;
            }
            return UInt64.MaxValue;
        }
    }

    public class OrderStockStatement :StockStatement
    {
        protected string requestCode_ {
            get;
            set;
        }
        //--------------------------------------------------------//
        // 처리 명령어
        protected override bool setInput()
        {
            return false;
        }

        public override void request()
        {
            if (this.setInput() == false) {
                return;
            }
            try {
                StockEngine engine = ControlGet.getInstance.stockBot().engine_;
                int nRet = engine.khOpenApi().CommRqData(requestName_, requestCode_, 0, screenNo_);

                Log level = Log.API조회;
                if (!Error.IsError(nRet)) {
                    level = Log.에러;
                }
                Logger.getInstance.print(level, "주문 [{0}:{1}]\t\t결과 메시지 : {2}", requestCode_, requestName_, Error.GetErrorMessage());
            } catch (AccessViolationException execption) {
                Logger.getInstance.print(Log.에러, "[request:{0}] {1}\n{2}\n{3}", this.ToString(), execption.Message, execption.StackTrace, execption.InnerException);
            }
        }

        public override void receive(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrDataEvent apiEvent)
        {
            Logger.getInstance.print(Log.에러, "{0} 주문 클래스 receive 구현이 안되어 있음", base.ToString());
        }
    }

    //--------------------------------------------------------//
    // 각 주문 명령 클래스들
    // 계좌 관련, 주식 리스트(각 리스트의 내부 데이터말고)만 로드
    //public class AccountStockStatement :OrderStockStatement
    //{
    //    public AccountStockStatement()
    //    {
    //        requestName_ = "계좌수익률요청";
    //        requestCode_ = "OPT10085";
    //    }

    //    protected override bool setInput()
    //    {
    //        return this.setParam("계좌번호", StockEngine.accountNumber());
    //    }

    //    public override void receive(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrDataEvent apiEvent)
    //    {
    //        int count = this.getRowCount(apiEvent);
    //        for (int i = 0; i < count; i++) {
    //            try {
    //                string code = this.getData("종목코드", apiEvent, i);
    //                string name = this.getData("종목명", apiEvent, i);
    //                var buyCountStr = this.getData("보유수량", apiEvent, i);
                   
    //                StockBot bot = ControlGet.getInstance.stockBot();
    //                if (buyCountStr == "") {
    //                    //StockData stockData = new StockData(code, name);
    //                    //bot.addStockData(stockData);
    //                    //stockData.loadPriceData(bot);
    //                } else {
    //                    string date = this.getData("일자", apiEvent, i);
    //                    var buyPrice = this.getData("매입가", apiEvent, i);

    //                    KStockData kStockData = bot.getStockDataCode(code) as KStockData;
    //                    if (kStockData == null) {
    //                        kStockData = new KStockData(code, name);
    //                        bot.addStockData(kStockData);
    //                        bot.loadFromDB(kStockData);
    //                        kStockData.loadPriceData(bot);
    //                    }
    //                    kStockData.setBuyInfo(buyCountStr, buyPrice);
    //                    kStockData.buyTime_ = CandleData.strToDateTime(date);
    //                }

    //                Logger.getInstance.consolePrint("종목코드:{0} | 종목명:{1} | 현재가:{2} | 보유수량:{3} | 매입가:{4} | 당일매도손익: {5}",
    //                    this.getData("종목코드", apiEvent, i),
    //                    this.getData("종목명", apiEvent, i),
    //                    this.getData("현재가", apiEvent, i),
    //                    this.getData("보유수량", apiEvent, i),
    //                    this.getData("매입가", apiEvent, i),
    //                    this.getData("당일매도손익", apiEvent, i));
    //            } catch (AccessViolationException execption) {
    //                Logger.getInstance.print(Log.에러, "[receive:{0}] {1}\n{2}\n{3}", this.ToString(), execption.Message, execption.StackTrace, execption.InnerException);
    //            }
    //        }
    //        StockDlgInfo.getInstance.updateBuyPoolView();
    //    }
    //}

    public class AccountStockStatement: OrderStockStatement
    {
        public AccountStockStatement()
        {
            requestName_ = "계좌평가현황요청";
            requestCode_ = "OPW00004";
        }

        protected override bool setInput()
        {
            if (this.setParam("계좌번호", StockEngine.accountNumber()) == false) {
                return false;
            }

            //비밀번호 = 사용안함(공백)
            if (this.setParam("비밀번호", "") == false) {
                return false;
            }

            //상장폐지조회구분 = 0:전체, 1 : 상장폐지종목제외
            if (this.setParam("상장폐지조회구분", "0") == false) {
                return false;
            }

            //비밀번호입력매체구분 = 00
            if (this.setParam("비밀번호입력매체구분", "00") == false) {
                return false;
            }
            return true;
        }

        public override void receive(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrDataEvent apiEvent)
        {
            try {
                double prepareMoney = this.getDouble("예수금", apiEvent, 0);
                double totalBuyMoney = this.getDouble("총매입금액", apiEvent, 0);
                double totalMoney = this.getDouble("추정예탁자산", apiEvent, 0);

                var bot = ControlGet.getInstance.stockBot();
                bot.setAccountMoney(prepareMoney);
                bot.totalEvaluationMoney_ = totalMoney;
                bot.totalBuyMoney_ = totalBuyMoney;

                int count = this.getRowCount(apiEvent);

                var tradeView = StockDlgInfo.getInstance.tradeRecoderView_;
                var dateTable = tradeView.makeNewTable();
                string fmt = "##,###0.##";

                double totalProfit = 0.0f;
                for (int i = 0; i < count; i++) {
                    string code = this.getData("종목코드", apiEvent, i);
                    if (code.Length == 0) {
                        continue;
                    }
                    code = code.Substring(1, code.Length - 1);  // 앞에 1글자 떼어내야함

                    string name = this.getData("종목명", apiEvent, i);

                    KStockData kStockData = bot.getStockDataCode(code) as KStockData;
                    if (kStockData == null) {
                        kStockData = new KStockData(code, name);
                        bot.addStockData(kStockData);
                        bot.loadFromDB(kStockData);
                        kStockData.loadPriceData(bot);
                    }

                    var buyCountStr = this.getData("보유수량", apiEvent, i);
                    var buyPrice = this.getData("평균단가", apiEvent, i);

                    kStockData.setBuyInfo(buyCountStr, buyPrice);
                    var profit = this.getDouble("손익금액", apiEvent, i);
                    totalProfit += profit;
                    var profitRate = this.getDouble("손익율", apiEvent, i);

                    dateTable.Rows.Add(code, name, kStockData.buyCount_, kStockData.buyPrice_, profit.ToString(fmt), profitRate.ToString(fmt));
                }
                dateTable.Rows.Add(0, "종합", 0, 0, totalProfit.ToString(fmt), 0.0f);
                bot.nowTotalProfit_ = totalProfit;
                tradeView.print(dateTable);
            }
            catch (Exception execption) {
                Logger.getInstance.print(Log.에러, "[receive:{0}] {1}\n{2}\n{3}", this.ToString(), execption.Message, execption.StackTrace, execption.InnerException);
            }
        }
    }

    public class TradeHistoryStatement: OrderStockStatement
    {
        public TradeHistoryStatement()
        {
            requestName_ = "일별계좌수익률상세현황요청";
            requestCode_ = "OPW00016";
        }

        public DateTime dateStart_ = DateTime.Now.AddDays(-30);
        public DateTime dateEnd_ = DateTime.Now;
        protected override bool setInput()
        {
            if (this.setParam("계좌번호", StockEngine.accountNumber()) == false) {
                return false;
            }

            //비밀번호 = 사용안함(공백)
            if (this.setParam("비밀번호", "") == false) {
                return false;
            }

            if (this.setParam("평가시작일", this.dateStart_.ToString("yyyyMMdd")) == false) {
                return false;
            }

            if (this.setParam("평가종료일", this.dateEnd_.ToString("yyyyMMdd")) == false) {
                return false;
            }

            if (this.setParam("비밀번호입력매체구분", "") == false) {
                return false;
            }
            return true;
        }

        public override void receive(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrDataEvent apiEvent)
        {
            try {
                int count = this.getRowCount(apiEvent);
                if (count == 0) {
                    return;
                }
                var tradeView = StockDlgInfo.getInstance.tradeHistoryView_;
                var dateTable = tradeView.makeNewTable();

                for (int i = 0; i < count; ++i) {
                    string dateStr = this.getData("일자", apiEvent, i);
                    DateTime date = DateTime.Now;
                    if (dateStr == string.Empty) {
                        continue;
                    }
                    date = DateTime.ParseExact(dateStr, "yyyyMMdd", CultureInfo.InvariantCulture);

                    double money = this.getDouble("예수금_초", apiEvent, i);
                    double money2 = this.getDouble("예수금_말", apiEvent, i);
                    double rentMoney = this.getDouble("투자원금평잔", apiEvent, i);
                    double profit = this.getDouble("평가손익", apiEvent, i);
                    double profitRate = this.getDouble("수익율", apiEvent, i);

                    dateTable.Rows.Add(date, money, money2, rentMoney, profit, profitRate);
                }
                tradeView.print(dateTable);
            }
            catch (AccessViolationException execption) {
                Logger.getInstance.print(Log.에러, "[receive:{0}] {1}\n{2}\n{3}", this.ToString(), execption.Message, execption.StackTrace, execption.InnerException);
            }
        }
    }

    public class AccountMoney2Statement :OrderStockStatement
    {
        public AccountMoney2Statement()
        {
            requestName_ = "예수금상세현황요청";
            requestCode_ = "opw00001";
        }

        protected override bool setInput()
        {
            if (this.setParam("계좌번호", StockEngine.accountNumber()) == false) {
                return false;
            }

            //비밀번호 = 사용안함(공백)
            if (this.setParam("비밀번호", "") == false) {
                return false;
            }

            //비밀번호입력매체구분 = 00
            if (this.setParam("비밀번호입력매체구분", "00") == false) {
                return false;
            }

            //조회구분 = 1:추정조회, 2:일반조회
            if (this.setParam("조회구분", "1") == false) {
                return false;
            }
            return true;
        }

        public override void receive(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrDataEvent apiEvent)
        {
            try {
                string temp1 = this.getData("d+2추정예수금", apiEvent, 0);
                string temp2 = this.getData("d+2출금가능금액", apiEvent, 0);
                string temp3 = this.getData("예수금", apiEvent, 0);
                string temp4 = this.getData("주식증거금현금", apiEvent, 0);
                string temp5 = this.getData("d+1추정예수금", apiEvent, 0);
                string temp6 = this.getData("d+1출금가능금액", apiEvent, 0);
                Int64 money = 0;
                if (temp1.Length != 0) {
                    money = Int64.Parse(temp1);
                } else if (temp2.Length != 0) {
                    money = Int64.Parse(temp2);
                } else if (temp3.Length != 0) {
                    money = Int64.Parse(temp3);
                } else if (temp4.Length != 0) {
                    money = Int64.Parse(temp4);
                }

                StockBot bot = ControlGet.getInstance.stockBot();
                if (money == 0) {
                   // bot.engine_.addOrder(new AccountMoneyStatement());
                    return;
                }
                bot.setAccountMoney(money);
                bot.updatePoolView();
                Logger.getInstance.print(Log.API결과, "주문 가능금액 {0} | 예수금 {1} ", temp1, temp2);

            } catch (AccessViolationException execption) {
                Logger.getInstance.print(Log.에러, "[receive:{0}] {1}\n{2}\n{3}", this.ToString(), execption.Message, execption.StackTrace, execption.InnerException);
            }
        }
    }

    public class AccountMoney3Statement: OrderStockStatement
    {
        public AccountMoney3Statement()
        {
            requestName_ = "계좌평가잔고내역요청";
            requestCode_ = "opw00018";
        }

        protected override bool setInput()
        {
            if (this.setParam("계좌번호", StockEngine.accountNumber()) == false) {
                return false;
            }

            //비밀번호 = 사용안함(공백)
            if (this.setParam("비밀번호", "") == false) {
                return false;
            }

            //상장폐지조회구분 = 0:전체, 1 : 상장폐지종목제외
            if (this.setParam("비밀번호입력매체구분", "00") == false) {
                return false;
            }

            //조회구분 = 1:합산, 2:개별
            if (this.setParam("조회구분", "1") == false) {
                return false;
            }
            return true;
        }

        public override void receive(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrDataEvent apiEvent)
        {
            try {
                var bot = ControlGet.getInstance.stockBot();

                bot.totalBuyMoney_ = this.getDouble("총매입금액", apiEvent, 0);
                bot.totalEvaluationMoney_ = this.getDouble("총평가금액", apiEvent, 0);

            }
            catch (AccessViolationException execption) {
                Logger.getInstance.print(Log.에러, "[receive:{0}] {1}\n{2}\n{3}", this.ToString(), execption.Message, execption.StackTrace, execption.InnerException);
            }
        }
    }

    //--------------------------------------------------------//
    // 계좌 로딩 클래스 정의 데이터 받는 부분이 같음
    public class StockInfoLoadStatement :OrderStockStatement
    {
        public bool isETFLoad_ = false;

        bool isETF_ETNStock(string name)
        {
            string[] etnfString = {
                "TIGER", "KODEX", "레버리지", "ETN", "ARIRANG", "인버스", "선물", "옵션",
            };

            foreach (string etn in etnfString) {
                if (name.Contains(etn)) {
                    return true;
                }
            }

            return false;
        }

        // watcing 주식 데이터 가지고 오는 공용 함수
        public override void receive(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrDataEvent apiEvent)
        {
            StockBot bot = ControlGet.getInstance.stockBot();
            int count = this.getRowCount(apiEvent);
            Logger.getInstance.print(Log.API결과, "{0} 개의 주식 가지고 옴", count);

            //int remainCount = PublicVar.stockPoolCountMax - bot.stockPoolCount();
            //count = Math.Min(count, remainCount);
            const int LIMIT = 30;
            try {
                for (int i = 0; i < count; i++) {
                    string codeStr = this.getData("종목코드", apiEvent, i);
                    string name = this.getData("종목명", apiEvent, i);

                    KStockData kStockData = bot.getStockDataCode(codeStr) as KStockData;
                    if (kStockData == null) {
                        kStockData = new KStockData(codeStr, name);
                        bot.addStockData(kStockData);
                        bot.loadFromDB(kStockData);
                    }
                    if (i > LIMIT) {
                        break;
                    }
                }
            }
            catch (AccessViolationException execption) {
                Logger.getInstance.print(Log.에러, "[receive:{0}] {1}\n{2}\n{3}", this.ToString(), execption.Message, execption.StackTrace, execption.InnerException);
            }
        }
    }

    //--------------------------------------------------------//
    // 주식 목록을 불러오는 명령어들

    public class NewHighPriceStock :StockInfoLoadStatement
    {
        public NewHighPriceStock()
        {
            requestName_ = "신고저가요청";
            requestCode_ = "OPT10016";
        }

        protected override bool setInput()
        {
            //시장구분 = 000:전체, 001 : 코스피, 101 : 코스닥
            if (this.setParam("시장구분", "000") == false) {
                return false;
            }

            //신고저구분 = 1:신고가, 2 : 신저가
            if (this.setParam("신고저구분", "1") == false) {
                return false;
            }

            //고저종구분 = 1:고저기준, 2 : 종가기준
            if (this.setParam("고저종구분", "1") == false) {
                return false;
            }

            //종목조건 = 0:전체조회, 1 : 관리종목제외, 3 : 우선주제외, 5 : 증100제외, 6 : 증100만보기, 7 : 증40만보기, 8 : 증30만보기
            if (this.setParam("종목조건", "0") == false) {
                return false;
            }

            //거래량구분 = 00000:전체조회, 00010 : 만주이상, 00050 : 5만주이상, 00100 : 10만주이상, 00150 : 15만주이상, 00200 : 20만주이상, 00300 : 30만주이상, 00500 : 50만주이상, 01000 : 백만주이상
            if (this.setParam("거래량구분", "00100") == false) {
                return false;
            }

            //신용조건 = 0:전체조회, 1 : 신용융자A군, 2 : 신용융자B군, 3 : 신용융자C군, 4 : 신용융자D군, 9 : 신용융자전체
            if (this.setParam("신용조건", "0") == false) {
                return false;
            }

            //상하한포함 = 0:미포함, 1 : 포함
            if (this.setParam("상하한포함", "1") == false) {
                return false;
            }

            //기간 = 5:5일, 10 : 10일, 20 : 20일, 60 : 60일, 250 : 250일, 250일까지 입력가능
            if (this.setParam("기간", "20") == false) {
                return false;
            }
            return true;
        }
    }

    public class ApproachHighPriceTradingStock :StockInfoLoadStatement
    {
        public ApproachHighPriceTradingStock()
        {
            requestName_ = "고저가근접요청";
            requestCode_ = "OPT10018";
        }

        protected override bool setInput()
        {
            //고저구분 = 1:고가, 2 : 저가
            if (this.setParam("고저구분", "1") == false) {
                return false;
            }

            //근접율 = 05:0.5 10 : 1.0, 15 : 1.5, 20 : 2.0. 25 : 2.5, 30 : 3.0
            if (this.setParam("근접율", "2.0") == false) {
                return false;
            }

            //시장구분 = 000:전체, 001 : 코스피, 101 : 코스닥
            if (this.setParam("시장구분", "000") == false) {
                return false;
            }

            //거래량구분 = 00000:전체조회, 00010 : 만주이상, 00050 : 5만주이상, 00100 : 10만주이상, 00150 : 15만주이상, 00200 : 20만주이상, 00300 : 30만주이상, 00500 : 50만주이상, 01000 : 백만주이상
            if (this.setParam("거래량구분", "00100") == false) {
                return false;
            }

            //종목조건 = 0:전체조회, 1 : 관리종목제외, 3 : 우선주제외, 5 : 증100제외, 6 : 증100만보기, 7 : 증40만보기, 8 : 증30만보기
            if (this.setParam("종목조건", "1") == false) {
                return false;
            }

            //신용조건 = 0:전체조회, 1 : 신용융자A군, 2 : 신용융자B군, 3 : 신용융자C군, 4 : 신용융자D군, 9 : 신용융자전체
            if (this.setParam("신용조건", "0") == false) {
                return false;
            }
            return true;
        }
    }

    public class SuddenlyHighTradingStock :StockInfoLoadStatement
    {
        public SuddenlyHighTradingStock()
        {
            requestName_ = "가격급등락요청";
            requestCode_ = "OPT10019";
        }

        protected override bool setInput()
        {
            //시장구분 = 000:전체, 001 : 코스피, 101 : 코스닥, 201 : 코스피200
            if (this.setParam("시장구분", "000") == false) {
                return false;
            }

            //등락구분 = 1:급등, 2 : 급락
            if (this.setParam("등락구분", "1") == false) {
                return false;
            }

            //시간구분 = 1:분전, 2 : 일전
            if (this.setParam("시간구분", "2") == false) {
                return false;
            }

            //시간 = 분 혹은 일입력
            if (this.setParam("시간", "일") == false) {
                return false;
            }

            //거래량구분 = 00000:전체조회, 00010 : 만주이상, 00050 : 5만주이상, 00100 : 10만주이상, 00150 : 15만주이상, 00200 : 20만주이상, 00300 : 30만주이상, 00500 : 50만주이상, 01000 : 백만주이상
            if (this.setParam("거래량구분", "00200") == false) {
                return false;
            }

            //종목조건 = 0:전체조회, 1 : 관리종목제외, 3 : 우선주제외, 5 : 증100제외, 6 : 증100만보기, 7 : 증40만보기, 8 : 증30만보기
            if (this.setParam("종목조건", "1") == false) {
                return false;
            }

            //신용조건 = 0:전체조회, 1 : 신용융자A군, 2 : 신용융자B군, 3 : 신용융자C군, 4 : 신용융자D군, 9 : 신용융자전체
            if (this.setParam("신용조건", "0") == false) {
                return false;
            }

            //가격조건 = 0:전체조회, 1 : 1천원미만, 2 : 1천원~2천원, 3 : 2천원~3천원, 4 : 5천원~1만원, 5 : 1만원이상, 8 : 1천원이상
            if (this.setParam("가격조건", "0") == false) {
                return false;
            }

            //상하한포함 = 0:미포함, 1 : 포함
            if (this.setParam("상하한포함", "0") == false) {
                return false;
            }
            return true;
        }
    }

    public class YesterdayHighTradingStock :StockInfoLoadStatement
    {
        public YesterdayHighTradingStock()
        {
            requestName_ = "전일대비등락률상위요청";
            requestCode_ = "OPT10027";
        }

        protected override bool setInput()
        {
            //시장구분 = 000:전체, 001 : 코스피, 101 : 코스닥
            if (this.setParam("시장구분", "000") == false) {
                return false;
            }

            //정렬구분 = 1:상승률, 2 : 상승폭, 3 : 하락률, 4 : 하락폭
            if (this.setParam("정렬구분", "1") == false) {
                return false;
            }

            //거래량조건 = 0000:전체조회, 0010 : 만주이상, 0050 : 5만주이상, 0100 : 10만주이상, 0150 : 15만주이상, 0200 : 20만주이상, 0300 : 30만주이상, 0500 : 50만주이상, 1000 : 백만주이상
            if (this.setParam("거래량조건", "0100") == false) {
                return false;
            }

            //종목조건 = 0:전체조회, 1 : 관리종목제외, 4 : 우선주 + 관리주제외, 3 : 우선주제외, 5 : 증100제외, 6 : 증100만보기, 7 : 증40만보기, 8 : 증30만보기, 9 : 증20만보기, 11 : 정리매매종목제외
            if (this.setParam("종목조건", "1") == false) {
                return false;
            }

            //신용조건 = 0:전체조회, 1 : 신용융자A군, 2 : 신용융자B군, 3 : 신용융자C군, 4 : 신용융자D군, 9 : 신용융자전체
            if (this.setParam("신용조건", "0") == false) {
                return false;
            }

            //상하한포함 = 0:불 포함, 1 : 포함
            if (this.setParam("상하한포함", "0") == false) {
                return false;
            }

            //가격조건 = 0:전체조회, 1 : 1천원미만, 2 : 1천원~2천원, 3 : 2천원~5천원, 4 : 5천원~1만원, 5 : 1만원이상, 8 : 1천원이상
            if (this.setParam("가격조건", "0") == false) {
                return false;
            }

            //거래대금조건 = 0:전체조회, 3 : 3천만원이상, 5 : 5천만원이상, 10 : 1억원이상, 30 : 3억원이상, 50 : 5억원이상, 100 : 10억원이상, 300 : 30억원이상, 500 : 50억원이상, 1000 : 100억원이상, 3000 : 300억원이상, 5000 : 500억원이상
            if (this.setParam("거래대금조건", "10") == false) {
                return false;
            }
            return true;
        }
    }

    public class YesterdayTopTradingStock :StockInfoLoadStatement
    {
        public YesterdayTopTradingStock()
        {
            requestName_ = "전일거래량상위요청";
            requestCode_ = "OPT10031";
        }

        protected override bool setInput()
        {
            //시장구분 = 000:전체, 001 : 코스피, 101 : 코스닥
            if (this.setParam("시장구분", "000") == false) {
                return false;
            }

            //조회구분 = 1:전일거래량 상위100종목, 2 : 전일거래대금 상위100종목
            if (this.setParam("조회구분", "1") == false) {
                return false;
            }

            //순위시작 = 0 ~100 값 중에  조회를 원하는 순위 시작값
            if (this.setParam("순위시작", "0") == false) {
                return false;
            }

            //순위끝 = 0 ~100 값 중에  조회를 원하는 순위 끝값
            if (this.setParam("순위끝", "30") == false) {
                return false;
            }
            return true;
        }
    }

    public class TodayTopTradingStock :StockInfoLoadStatement
    {
        public TodayTopTradingStock()
        {
            requestName_ = "당일거래량상위요청";
            requestCode_ = "OPT10030";
        }

        protected override bool setInput()
        {
            //시장구분 = 000:전체, 001 : 코스피, 101 : 코스닥
            if (this.setParam("시장구분", "000") == false) {
                return false;
            }

            //조회구분 = 1:전일거래량 상위100종목, 2 : 전일거래대금 상위100종목
            if (this.setParam("조회구분", "1") == false) {
                return false;
            }

            //순위시작 = 0 ~100 값 중에  조회를 원하는 순위 시작값
            if (this.setParam("순위시작", "0") == false) {
                return false;
            }

            //순위끝 = 0 ~100 값 중에  조회를 원하는 순위 끝값
            if (this.setParam("순위끝", "30") == false) {
                return false;
            }
            return true;
        }
    }

    public class HighTradingStock :StockInfoLoadStatement
    {
        public HighTradingStock()
        {
            requestName_ = "거래량급증요청";
            requestCode_ = "OPT10023";
        }

        protected override bool setInput()
        {
            //시장구분 = 000:전체, 001 : 코스피, 101 : 코스닥
            if (this.setParam("시장구분", "000") == false) {
                return false;
            }

            //정렬구분 = 1:급증량, 2:급증률
            if (this.setParam("정렬구분", "1") == false) {
                return false;
            }

            //시간구분 = 1:분, 2:전일
            if (this.setParam("시간구분", "2") == false) {
                return false;
            }

            //거래량구분 = 5:5천주이상, 10:만주이상, 50:5만주이상, 100:10만주이상, 200:20만주이상, 300:30만주이상, 500:50만주이상, 1000:백만주이상
            if (this.setParam("거래량구분", "300") == false) {
                return false;
            }

            //시간 = 분 입력
            if (this.setParam("시간", "60") == false) {
                return false;
            }

            //종목조건 = 0:전체조회, 1:관리종목제외, 5:증100제외, 6:증100만보기, 7:증40만보기, 8:증30만보기, 9:증20만보기
            if (this.setParam("종목조건", "1") == false) {
                return false;
            }

            //가격구분 = 0:전체조회, 2:5만원이상, 5:1만원이상, 6:5천원이상, 8:1천원이상, 9:10만원이상
            if (this.setParam("가격구분", "0") == false) {
                return false;
            }
            return true;
        }
    }

    public class TopTradingStocks: StockInfoLoadStatement
    {
        public TopTradingStocks()
        {
            requestName_ = "매매상위요청";
            requestCode_ = "OPT10036";
        }

        protected override bool setInput()
        {
            //시장구분 = 000:전체, 001 : 코스피, 101 : 코스닥
            if (this.setParam("시장구분", "000") == false) {
                return false;
            }

            //기간 = 0:당일, 1 : 전일, 5 : 5일, 10; 10일, 20:20일, 60 : 60일
            if (this.setParam("기간", "5") == false) {
                return false;
            }
            return true;
        }
    }

    public class ForeignerTradingSotck :StockInfoLoadStatement
    {
        public ForeignerTradingSotck()
        {
            requestName_ = "외인연속순매매상위요청";
            requestCode_ = "OPT10035";
        }

        protected override bool setInput()
        {
            //시장구분 = 000:전체, 001 : 코스피, 101 : 코스닥
            if (this.setParam("시장구분", "000") == false) {
                return false;
            }

            //매매구분 = 1:연속순매도, 2 : 연속순매수
            if (this.setParam("매매구분", "2") == false) {
                return false;
            }

            //기간 = 0:당일, 1 : 전일, 5 : 5일, 10; 10일, 20:20일, 60 : 60일
            if (this.setParam("기간", "3") == false) {
                return false;
            }
            return true;
        }
    }

    public class AgencyTradingStock :StockInfoLoadStatement
    {
        public AgencyTradingStock()
        {
            requestName_ = "기관 일별기관매매종목요청";
            requestCode_ = "OPT10044";
        }

        protected override bool setInput()
        {
            DateTime now = DateTime.Now;
            DateTime start = now.AddDays(-5);
            //시작일자 = YYYYMMDD(20160101 연도4자리, 월 2자리, 일 2자리 형식)
            string startDay = start.ToString("yyyyMMdd");
            if (this.setParam("시작일자", startDay) == false) {
                return false;
            }

            //종료일자 = YYYYMMDD(20160101 연도4자리, 월 2자리, 일 2자리 형식)
            DateTime end = now.AddDays(-1);
            string endDay = end.ToString("yyyyMMdd");
            if (this.setParam("종료일자", endDay) == false) {
                return false;
            }

            //매매구분 = 0:전체, 1 : 순매도, 2 : 순매수
            if (this.setParam("매매구분", "2") == false) {
                return false;
            }

            //시장구분 = 001:코스피, 101 : 코스닥
            if (this.setParam("시장구분", "000") == false) {
                return false;
            }
            return true;
        }
    }
   
    //--------------------------------------------------------//
    // 주식 데이터 정보(차트 정보)
    public class StockInfoStatement :OrderStockStatement
    {
        public string stockCode_;
        protected PRICE_TYPE priceType_;
        public bool immediatelyBuy_ = false;
        protected string stockCode()
        {
            return string.Format("{0:D6}", stockCode_);
        }

        public override void request()
        {
            this.setInput();
            StockEngine engine = ControlGet.getInstance.stockBot().engine_;
            int nRet = engine.khOpenApi().CommRqData(requestName_, requestCode_, 0, screenNo_);
            Logger.getInstance.consolePrint("주문 [{0}:{1}]\t\t결과 메시지 : {2}", requestCode_, requestName_, Error.GetErrorMessage());
        }

        protected virtual string dateTime(int index, object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrDataEvent apiEvent)
        {
            string dateTime = this.getData("체결시간", apiEvent, index);
            return dateTime;
        }

        public override void receive(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrDataEvent apiEvent)
        {
            StockBot bot = ControlGet.getInstance.stockBot();
            Logger logger = Logger.getInstance;
            try {
                StockData stockData = ControlGet.getInstance.stockBot().getStockDataCode(stockCode_);
                if (stockData == null) {
                    return;
                }
                int count = this.getRowCount(apiEvent);
                if (count == 0) {
                    logger.print(Log.에러, "stock {0}:{1} 의 데이터 로딩 실패. 목록에서 제거 시킴. 아마 거래 정지가 풀린거 일수도", stockData.name_, stockData.code_);
                    ControlGet.getInstance.stockBot().removeStock(stockCode_);
                    return;
                }
                stockData.clearPrice();
                for (int i = 0; i < count; i++) {
                    string dateTime = this.dateTime(i, sender, apiEvent);
                    double price = this.getDouble("현재가", apiEvent, i);
                    double startPrice = this.getDouble("시가", apiEvent, i);
                    double highPrice = this.getDouble("고가", apiEvent, i);
                    double lowPrice = this.getDouble("저가", apiEvent, i);
                    if (price == double.MaxValue
                        || startPrice == double.MaxValue
                        || highPrice == double.MaxValue
                        || lowPrice == double.MaxValue) {
                        // 데이터 깨진거임.
                        return;
                    }
                    UInt64 volume = getUInt64("거래량", apiEvent, i);
                    if (volume == UInt64.MaxValue) {
                        return;
                    }

                    CandleData priceData = new CandleData(dateTime, price, startPrice, highPrice, lowPrice, volume);
                    stockData.updatePrice(priceData);
                }
                stockData.updatePriceTable(bot);
                stockData.recvedDataTime_ = DateTime.Now;

                if (priceType_ == PRICE_TYPE.DAY) {
                    bot.searchBestTradeModuleAnStock(stockData);
                }

                bot.hangOnWatching_ = DateTime.Now;
                StockDlg dlg = Program.happyTool_.stockDlg_;                
                dlg.printStatus(string.Format("키움API get data [{0}], [{1}]분봉, [{2}] 주식 데이터 {3}/{4} ", DateTime.Now.ToLongTimeString(), bot.priceTypeMin(), stockData.name_, ControlGet.getInstance.stockBot().stockPoolIndex_++, ControlGet.getInstance.stockBot().stockPoolCount()));
            } catch (AccessViolationException execption) {
                logger.print(Log.에러, "[receive:{0}] {1}\n{2}\n{3}", this.ToString(), execption.Message, execption.StackTrace, execption.InnerException);
            }
        }
    }

    //--------------------------------------------------------//
    // 주식 종목의 차트 조회
    public class HistoryMin1Stock :StockInfoStatement
    {
        public HistoryMin1Stock(string stockCode)
        {
            stockCode_ = stockCode;
            requestName_ = string.Format("주식{0}분차트조회요청", PublicVar.priceType_min1);
            requestCode_ = "OPT10080";
            priceType_ = PRICE_TYPE.MIN_1;
        }

        protected override bool setInput()
        {
            if (this.setParam("종목코드", this.stockCode()) == false) {
                return false;
            }
            // 틱범위 = 1:1분, 3:3분, 5:5분, 10:10분, 15:15분, 30:30분, 45:45분, 60:60분
            if (this.setParam("틱범위", PublicVar.priceType_min1.ToString()) == false) {
                return false;
            }
            //수정주가구분 = 0 or 1, 수신데이터 1:유상증자, 2:무상증자, 4:배당락, 8:액면분할, 16:액면병합, 32:기업합병, 64:감자, 256:권리락
            if (this.setParam("수정주가구분", "1") == false) {
                return false;
            }
            return true;
        }

        protected override string dateTime(int index, object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrDataEvent apiEvent)
        {
            return this.getData("체결시간", apiEvent, index);
        }
    }

    public class HistoryMin2Stock :StockInfoStatement
    {
        public HistoryMin2Stock(string stockCode)
        {
            stockCode_ = stockCode;
            requestName_ = string.Format("주식{0}분차트조회요청", PublicVar.priceType_min2);
            requestCode_ = "OPT10080";
            priceType_ = PRICE_TYPE.MIN_2;
        }

        protected override bool setInput()
        {
            if (this.setParam("종목코드", this.stockCode()) == false) {
                return false;
            }
            // 틱범위 = 1:1분, 3:3분, 5:5분, 10:10분, 15:15분, 30:30분, 45:45분, 60:60분
            if (this.setParam("틱범위", PublicVar.priceType_min2.ToString()) == false) {
                return false;
            }
            //수정주가구분 = 0 or 1, 수신데이터 1:유상증자, 2:무상증자, 4:배당락, 8:액면분할, 16:액면병합, 32:기업합병, 64:감자, 256:권리락
            if (this.setParam("수정주가구분", "1") == false) {
                return false;
            }
            return true;
        }

        protected override string dateTime(int index, object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrDataEvent apiEvent)
        {
            return this.getData("체결시간", apiEvent, index);
        }
    }

    public class HistoryMin3Stock :StockInfoStatement
    {
        public HistoryMin3Stock(string stockCode)
        {
            stockCode_ = stockCode;
            requestName_ = string.Format("주식{0}분차트조회요청", PublicVar.priceType_min3);
            requestCode_ = "OPT10080";
            priceType_ = PRICE_TYPE.MIN_3;
        }

        protected override bool setInput()
        {
            if (this.setParam("종목코드", this.stockCode()) == false) {
                return false;
            }
            // 틱범위 = 1:1분, 3:3분, 5:5분, 10:10분, 15:15분, 30:30분, 45:45분, 60:60분
            if (this.setParam("틱범위", PublicVar.priceType_min3.ToString()) == false) {
                return false;
            }
            //수정주가구분 = 0 or 1, 수신데이터 1:유상증자, 2:무상증자, 4:배당락, 8:액면분할, 16:액면병합, 32:기업합병, 64:감자, 256:권리락
            if (this.setParam("수정주가구분", "1") == false) {
                return false;
            }
            return true;
        }

        protected override string dateTime(int index, object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrDataEvent apiEvent)
        {
            return this.getData("체결시간", apiEvent, index);
        }
    }

    public class HistoryTheDaysStock :StockInfoStatement
    {
        public HistoryTheDaysStock(string stockCode)
        {
            stockCode_ = stockCode;
            requestName_ = "주식일봉차트조회";
            requestCode_ = "OPT10081";
            priceType_ = PRICE_TYPE.DAY;
        }

        protected override bool setInput()
        {
            if (this.setParam("종목코드", this.stockCode()) == false) {
                return false;
            }
            //기준일자 = YYYYMMDD (20160101 연도4자리, 월 2자리, 일 2자리 형식)
            if (this.setParam("기준일자", DateTime.Now.ToString("yyyyMMdd")) == false) {
                return false;
            }
            //수정주가구분 = 0 or 1, 수신데이터 1:유상증자, 2:무상증자, 4:배당락, 8:액면분할, 16:액면병합, 32:기업합병, 64:감자, 256:권리락
            if (this.setParam("수정주가구분", "1") == false) {
                return false;
            }
            return true;
        }

        protected override string dateTime(int index, object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrDataEvent apiEvent)
        {
            return this.getData("일자", apiEvent, index);
        }
    }

    public class HistoryTheWeeksStock :StockInfoStatement
    {
        public HistoryTheWeeksStock(string stockCode)
        {
            stockCode_ = stockCode;
            requestName_ = "주식주봉차트조회";
            requestCode_ = "OPT10082";
            priceType_ = PRICE_TYPE.WEEK;
        }

        protected override bool setInput()
        {
            if (this.setParam("종목코드", this.stockCode()) == false) {
                return false;
            }
            //기준일자 = YYYYMMDD (20160101 연도4자리, 월 2자리, 일 2자리 형식)
            if (this.setParam("기준일자", DateTime.Now.ToString("yyyyMMdd")) == false) {
                return false;
            }
            //수정주가구분 = 0 or 1, 수신데이터 1:유상증자, 2:무상증자, 4:배당락, 8:액면분할, 16:액면병합, 32:기업합병, 64:감자, 256:권리락
            if (this.setParam("수정주가구분", "1") == false) {
                return false;
            }
            return true;
        }

        protected override string dateTime(int index, object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrDataEvent apiEvent)
        {
            return this.getData("일자", apiEvent, index);
        }
    }

    //--------------------------------------------------------//
    // 주식 주문 
    public class TradingStatement :StockStatement
    {
        // 스크린번호를 주문에 대해서는 따로 대역대를 가지고 간다.

        // =================================================
        // 매매구분 취득
        // (1:신규매수, 2:신규매도 3:매수취소, 
        // 4:매도취소, 5:매수정정, 6:매도정정)
        protected KOACode.OrderType dealingType_;      //매매 구분

        // =================================================
        // 거래구분 취득
        // 0:지정가, 3:시장가, 5:조건부지정가, 6:최유리지정가, 7:최우선지정가,
        // 10:지정가IOC, 13:시장가IOC, 16:최유리IOC, 20:지정가FOK, 23:시장가FOK,
        // 26:최유리FOK, 61:장개시전시간외, 62:시간외단일가매매, 81:시간외종가
        protected KOACode.HogaGb hogaType_;         //거래 구분 (hoga/호가) 

        // 이하 입력 값들
        public int tradingCount_ { get; set; }          //주문 수량
        protected double tradingPrice_;                    //주문 가격
        public string code_ { get; set; }             //주식 코드
        public string orderNumber_;                     //주문 번호

        public TradingStatement()
        {
            orderNumber_ = "";
        }

        private string stockCode()
        {
            return string.Format("{0:D6}", code_);
        }

        protected override bool setInput()
        {
            return true;
        }

        public override bool isTradingOrder()
        {
            return true;
        }

        public override void request()
        {
            if (tradingCount_ <= 0) {
                return;
            }

            if (this.setInput() == false) {
                return;
            }
            StockEngine engine = ControlGet.getInstance.stockBot().engine_;
            int nRet = engine.khOpenApi().SendOrder(
                                        requestName_,
                                        screenNo_,
                                        StockEngine.accountNumber(),
                                        dealingType_.code,
                                        this.stockCode(),
                                        tradingCount_,
                                        (int) tradingPrice_,
                                        hogaType_.code,
                                        orderNumber_);
            if (Error.IsError(nRet)) {
                Logger.getInstance.print(Log.API조회, "주식주문:{0}, 주식 코드:{1}, 갯수:{2}, 주문가:{3}, in 계좌:{4}",
                    Error.GetErrorMessage(), this.stockCode(), tradingCount_, tradingPrice_, StockEngine.accountNumber());
            } else {
                Logger.getInstance.print(Log.에러, "주식주문 : " + Error.GetErrorMessage());
            }
            Logger.getInstance.print(Log.API조회, "주문 완료 :{0}, 번호:{1}, 주문:{2}, 호가:{3}, 주식:{4}, 갯수:{5}, 주문가:{6}, 계좌:{7}"
                , requestName_, screenNo_, dealingType_.name, hogaType_.name, this.stockCode(), tradingCount_, tradingPrice_, StockEngine.accountNumber());
        }

        public override void receive(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrDataEvent apiEvent)
        {
            // 주문 완려시 갱신
         //   StockEngine engine = ControlGet.getInstance.stockBot().engine_;
         //   engine.addOrder(new AccountStockStatement());
        }
    }

    public class BuyStock :TradingStatement
    {
        public BuyStock(string stockCode, int tradingCount, double tradingPrice)
        {
            // 거래구분 취득
            // 0:지정가, 3:시장가, 5:조건부지정가, 6:최유리지정가, 7:최우선지정가,
            // 10:지정가IOC, 13:시장가IOC, 16:최유리IOC, 20:지정가FOK, 23:시장가FOK,
            // 26:최유리FOK, 61:장개시전시간외, 62:시간외단일가매매, 81:시간외종가
            requestName_ = "BUY_STOCK";
            code_ = stockCode;
            tradingCount_ = tradingCount;
            tradingPrice_ = tradingPrice;
        }

        const bool staticPrice = false;
        protected override bool setInput()
        {
            dealingType_ = KOACode.orderType[0];           //신규매수
            if (staticPrice || tradingPrice_ != 0) {
                hogaType_ = KOACode.hogaGb[0];             //지정가
            } else {
                hogaType_ = KOACode.hogaGb[1];             //시장가
                tradingPrice_ = 0;                         //시장가
            }
            Logger.getInstance.print(Log.API조회, "주식{0} 구입{1} 구입타입 {2} : 갯수 {3} x 가격 {4}"
                , code_, dealingType_.name, hogaType_.name, tradingCount_, tradingPrice_);

            double price = tradingPrice_;
            if (price == 0) {
                StockData stockData = ControlGet.getInstance.stockBot().getStockDataCode(code_);
                if (stockData == null) {
                    return false;
                }
                price = stockData.nowPrice();
            }
            return true;
        }

        public override void receive(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrDataEvent apiEvent)
        {
            base.receive(sender, apiEvent);
        }
    }

    public class SellStock :TradingStatement
    {
        public SellStock(KStockData ownerStock)
        {
            // 거래구분 취득
            // 0:지정가, 3:시장가, 5:조건부지정가, 6:최유리지정가, 7:최우선지정가,
            // 10:지정가IOC, 13:시장가IOC, 16:최유리IOC, 20:지정가FOK, 23:시장가FOK,
            // 26:최유리FOK, 61:장개시전시간외, 62:시간외단일가매매, 81:시간외종가
            requestName_ = "SELL_STOCK";
            code_ = ownerStock.code_;
            tradingCount_ = ownerStock.buyCount_;
           // tradingPrice_ = ownerStock.nowPrice();
        }

        const bool staticPrice = false;
        protected override bool setInput()
        {
            dealingType_ = KOACode.orderType[1];            //신규매도
            //if (staticPrice || tradingPrice_ != 0) {
            //    hogaType_ = KOACode.hogaGb[0];              //지정가
            //} else {
            //    hogaType_ = KOACode.hogaGb[1];              //시장가
            //    tradingPrice_ = 0;                          //시장가
            //}
            // 단타 할꺼니까 시장가로 빨랑 처리
            hogaType_ = KOACode.hogaGb[1];              //시장가
            tradingPrice_ = 0;                          //시장가
            Logger.getInstance.print(Log.API결과, "주식{0} 판매{1} 판매타입 {2} : 갯수 {3} x 가격 {4}"
               , code_, dealingType_.name, hogaType_.name, tradingCount_, tradingPrice_);

            return true;
        }

        public override void receive(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrDataEvent apiEvent)
        {
            base.receive(sender, apiEvent);
        }
    }
}
