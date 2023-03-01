using HappyFuture.DialogControl.FutureDlg;
using KiwoomCode;
using KiwoomEngine;
using StockLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using UtilLibrary;

namespace HappyFuture
{

    public abstract class StockStatement: Statement
    {
        // 화면번호 생산
        public StockStatement()
        {
            this.receivedTick_ = 0;
        }

        // 주식 모듈로 결과를 받는 부분
        public abstract void receive(object sender, AxKFOpenAPILib._DKFOpenAPIEvents_OnReceiveTrDataEvent apiEvent);

        //--------------------------------------------------------//
        // 매크로
        protected bool setParam(string columName, string value)
        {
            try {
                var bot = ControlGet.getInstance.futureBot();
                bot.engine_.openApi().SetInputValue(columName, value);
                return true;
            }
            catch (AccessViolationException execption) {
                Logger.getInstance.print(Log.에러, "[setParam:{0}, {1}] {2}\n{3}\n{4}", columName, value, execption.Message, execption.StackTrace, execption.InnerException);
            }
            return false;
        }

        protected int getRowCount(AxKFOpenAPILib._DKFOpenAPIEvents_OnReceiveTrDataEvent apiEvent)
        {
            try {
                var bot = ControlGet.getInstance.futureBot();
                int count = bot.engine_.openApi().GetRepeatCnt(apiEvent.sTrCode, apiEvent.sRQName);
                return count;
            }
            catch (AccessViolationException execption) {
                Logger.getInstance.print(Log.에러, "[getRowCount:{0}] {1}\n{2}\n{3}", apiEvent.sRQName, execption.Message, execption.StackTrace, execption.InnerException);
            }
            return 0;
        }

        protected string getData(string columName, AxKFOpenAPILib._DKFOpenAPIEvents_OnReceiveTrDataEvent apiEvent, int index)
        {
            try {
                var bot = ControlGet.getInstance.futureBot();

                string data = bot.engine_.openApi().GetCommData(apiEvent.sTrCode, apiEvent.sRQName, index, columName).Trim();
                return data;
            }
            catch (AccessViolationException execption) {
                Logger.getInstance.print(Log.에러, "[getData:{0}] {1}\n{2}\n{3}", apiEvent.sRQName, execption.Message, execption.StackTrace, execption.InnerException);
            }
            return "";
        }

        protected double getDouble(string columName, AxKFOpenAPILib._DKFOpenAPIEvents_OnReceiveTrDataEvent apiEvent, int index)
        {
            string val = this.getData(columName, apiEvent, index);
            double valDouble = 0;
            if (double.TryParse(val, out valDouble)) {
                return valDouble;
            }
            return double.MaxValue;
        }

        protected UInt64 getUInt64(string columName, AxKFOpenAPILib._DKFOpenAPIEvents_OnReceiveTrDataEvent apiEvent, int index)
        {
            string val = this.getData(columName, apiEvent, index);
            UInt64 valDouble = 0;
            if (UInt64.TryParse(val, out valDouble)) {
                return valDouble;
            }
            return UInt64.MaxValue;
        }

        protected int getInt(string columName, AxKFOpenAPILib._DKFOpenAPIEvents_OnReceiveTrDataEvent apiEvent, int index)
        {
            string val = this.getData(columName, apiEvent, index);
            int valDouble = 0;
            if (int.TryParse(val, out valDouble)) {
                return valDouble;
            }
            return int.MaxValue;
        }
    }

    public class OrderFutureStatement: StockStatement
    {
        protected string requestCode_
        {
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
                var bot = ControlGet.getInstance.futureBot();
                int nRet = bot.engine_.openApi().CommRqData(this.requestName_, this.requestCode_, "", this.screenNo_);

                Log level = Log.API조회;
                if (!Error.IsError(nRet)) {
                    level = Log.에러;
                }
                Logger.getInstance.print(level, "주문 [{0}:{1}]\t\t결과 메시지 : {2}", this.requestCode_, this.requestName_, Error.GetErrorMessage());
            }
            catch (AccessViolationException execption) {
                Logger.getInstance.print(Log.에러, "[request:{0}] {1}\n{2}\n{3}", this.ToString(), execption.Message, execption.StackTrace, execption.InnerException);
            }
        }

        public override void receive(object sender, AxKFOpenAPILib._DKFOpenAPIEvents_OnReceiveTrDataEvent apiEvent)
        {
            Logger.getInstance.print(Log.에러, "{0} 주문 클래스 receive 구현이 안되어 있음", base.ToString());
        }
    }

    //--------------------------------------------------------//
    // 각 주문 명령 클래스들
    // 계좌 관련, 선물 리스트(각 리스트의 내부 데이터말고)만 로드
    public class AccountFutureStatement: OrderFutureStatement
    {
        public AccountFutureStatement()
        {
            this.requestName_ = "잔고내역";
            this.requestCode_ = "opw30003";
        }

        protected override bool setInput()
        {
            if (this.setParam("계좌번호", FutureEngine.accountNumber()) == false) {
                return false;
            }
            if (this.setParam("비밀번호", PublicVar.accountPW) == false) {
                return false;
            }
            if (this.setParam("비밀번호입력매체", "00") == false) {
                return false;
            }
            if (this.setParam("통화코드", "USD") == false) {
                return false;
            }
            return true;
        }

        public override void receive(object sender, AxKFOpenAPILib._DKFOpenAPIEvents_OnReceiveTrDataEvent apiEvent)
        {
            var bot = ControlGet.getInstance.futureBot();

            int count = this.getRowCount(apiEvent);
            try {
                for (int i = 0; i < count; i++) {
                    string code = this.getData("종목코드", apiEvent, i);
                    if (code.Length == 0) {
                        continue;
                    }
                    string isBuy = this.getData("매도수구분", apiEvent, i);
                    string fuCount = this.getData("수량", apiEvent, i);
                    string fuAbleCount = this.getData("청산가능", apiEvent, i);
                    string buyPrice = this.getData("평균단가", apiEvent, i);
                    string nowPrice = this.getData("현재가격", apiEvent, i);
                    string evalProfitRate = this.getData("평가손익", apiEvent, i);
                    double prepare = 0;
                    string preparePrice = this.getData("약정금액", apiEvent, i);
                    double.TryParse(preparePrice, out prepare);
                    string profit = this.getData("평가금액", apiEvent, i);
                    string profitRate = this.getData("수익율", apiEvent, i);
                    string tax = this.getData("수수료", apiEvent, i);
                    string currency = this.getData("통화코드", apiEvent, i);

                    FutureData futureData = (FutureData) bot.getStockDataCode(code);
                    if (futureData == null) {
                        Logger.getInstance.print(Log.에러, "{0} 선물 데이터는 잔존만기 근접 가능성 높음", code);
                        futureData = new FutureData(code, code);

                        bot.addStockData(futureData);
                        bot.engine_.requestFutureInfo(code);
                    }
                    futureData.setBuyInfo(isBuy, fuCount, buyPrice);

                    Logger.getInstance.consolePrint("종목코드:{0} | 매도수구분:{1} | 현재가:{2} | 보유수량:{3} | 평균단가:{4} | 수익율: {5}",
                        this.getData("종목코드", apiEvent, i),
                        this.getData("매도수구분", apiEvent, i),
                        this.getData("현재가격", apiEvent, i),
                        this.getData("수량", apiEvent, i),
                        this.getData("평균단가", apiEvent, i),
                        this.getData("수익율", apiEvent, i));
                }
            }
            catch (AccessViolationException execption) {
                Logger.getInstance.print(Log.에러, "[receive:{0}] {1}\n{2}\n{3}", this.ToString(), execption.Message, execption.StackTrace, execption.InnerException);
            }
            FutureDlgInfo.getInstance.updateBuyPoolView();
        }
    }

    public class AccountMoneyStatement: OrderFutureStatement
    {
        public AccountMoneyStatement()
        {
            this.requestName_ = "예수금및증거금현황조회";
            this.requestCode_ = "opw30009";
        }

        protected override bool setInput()
        {
            if (this.setParam("계좌번호", FutureEngine.accountNumber()) == false) {
                return false;
            }

            if (this.setParam("비밀번호", PublicVar.accountPW) == false) {
                return false;
            }

            //비밀번호입력매체구분 = 00
            if (this.setParam("비밀번호입력매체", "00") == false) {
                return false;
            }
            return true;
        }

        public override void receive(object sender, AxKFOpenAPILib._DKFOpenAPIEvents_OnReceiveTrDataEvent apiEvent)
        {
            try {
                double money = 0;
                //string temp1 = this.getData("주문가능금액", apiEvent, 0);
                //if (temp1.Length != 0) {
                //    money = double.Parse(temp1);
                //}
                string temp2 = this.getData("외화예수금", apiEvent, 0);
                if (temp2.Length != 0) {
                    money = double.Parse(temp2) / 100;
                }
                ControlGet.getInstance.futureBot().setAccountMoney(money);

                Logger.getInstance.consolePrint("외화 예수금 {0} ", money);
            }
            catch (AccessViolationException execption) {
                Logger.getInstance.print(Log.에러, "[receive:{0}] {1}\n{2}\n{3}", this.ToString(), execption.Message, execption.StackTrace, execption.InnerException);
            }
        }
    }

    //--------------------------------------------------------//
    // 선물의 자세한 데이터를 얻어와한다.
    // 1틱의 가치와, 세금, 단위등이 각 선물마다 다 다르므로 
    // 먼저 이걸로 데이터를 구축한다음 살것을 붙이고 모니터닝을 하면서 매매 해야함
    public class FutureInfoLoadStatement: OrderFutureStatement
    {
        readonly string code_;
        public FutureInfoLoadStatement(string code)
        {
            this.requestName_ = "종목정보조회";
            this.requestCode_ = "opt10001";
            this.code_ = code;
        }

        protected override bool setInput()
        {
            if (this.code_.Length == 0) {
                return false;
            }
            if (this.setParam("종목코드", this.code_) == false) {
                return false;
            }
            return true;
        }

        // watcing 선물 데이터 가지고 오는 공용 함수
        public override void receive(object sender, AxKFOpenAPILib._DKFOpenAPIEvents_OnReceiveTrDataEvent apiEvent)
        {
            var bot = ControlGet.getInstance.futureBot();
            FutureData futureData = (FutureData) bot.getStockDataCode(this.code_);
            if (futureData == null) {
                var name = this.getData("종목명", apiEvent, 0);
                futureData = new FutureData(this.code_, name);
            }
            string tickValue = this.getData("틱가치", apiEvent, 0);
            string tickStep = this.getData("틱단위", apiEvent, 0);
            futureData.setTickInfo(tickValue, tickStep);

            string endDaysStr = this.getData("잔존만기", apiEvent, 0);
            int endDays = 0;
            if (int.TryParse(endDaysStr, out endDays)) {
                futureData.endDays_ = endDays;
            }
            //08:00
            string startTime = this.getData("시작시간", apiEvent, 0);
            //07:00
            string endTime = this.getData("종료시간", apiEvent, 0);
            int hour = 6;
            int min = 0;
            var now = DateTime.Now;

            if (endTime.Length != 0) {
                string[] parse = endTime.Split(':');
                hour = int.Parse(parse[0]);
                min = int.Parse(parse[1]);
            }

            // 현재 시각이 끝나는 시각보다 크면 다음날로 설정
            if (now.Hour > hour) {
                now = now.AddDays(1);
            }
            futureData.tradeEndTime_ = new DateTime(now.Year, now.Month, now.Day, hour, min, 0);

            string exchange = this.getData("거래소", apiEvent, 0);
            futureData.exchangeName_ = exchange;
        }
    }

    public class FutureMarginMoneyStatement: OrderFutureStatement
    {
        readonly string codeCategory_;
        public FutureMarginMoneyStatement(string codeCategory)
        {
            this.requestName_ = "상품별증거금조회";
            this.requestCode_ = "opw20004";
            this.codeCategory_ = codeCategory;
        }

        protected override bool setInput()
        {
            //품목구분 = IDX:지수, CUR: 통화, MTL: 금속, ENG: 에너지, CMD: 농축산물
            if (this.codeCategory_.Length == 0) {
                return false;
            }

            if (this.setParam("품목구분", this.codeCategory_) == false) {
                return false;
            }

            if (this.setParam("적용일자", DateTime.Now.ToString("yyyyMMdd")) == false) {
                return false;
            }
            //해외파생구분 = "":전체, "FU":선물, "OP":옵션
            if (this.setParam("해외파생구분", "FU") == false) {
                return false;
            }
            //파생품목코드 = "":전체, "ES":상품코드
            if (this.setParam("파생품목코드", "") == false) {
                return false;
            }
            return true;
        }

        // watcing 선물 데이터 가지고 오는 공용 함수
        public override void receive(object sender, AxKFOpenAPILib._DKFOpenAPIEvents_OnReceiveTrDataEvent apiEvent)
        {
            var bot = ControlGet.getInstance.futureBot();
            int count = this.getRowCount(apiEvent);
            try {
                for (int i = 0; i < count; i++) {
                    var name = this.getData("품목명", apiEvent, i);
                    MarginMoney margin = new MarginMoney();

                    var trustMagin = this.getDouble("위탁증거금", apiEvent, i);
                    margin.trustMargin_ = trustMagin / 100;

                    var retaindMagin = this.getDouble("유지증거금", apiEvent, i);
                    margin.retaindMargin_ = retaindMagin / 100;
                    if (margin.trustMargin_ < 100 || margin.retaindMargin_ < 100) {
                        continue;
                    }
                    bot.updateMarginInfo(name, margin);
                }
                bot.eachSetMargineInfo();
            }
            catch (Exception e) {
                Logger.getInstance.print(Log.에러, "[receive:{0}] {1}\n{2}\n{3}", this.ToString(), e.Message, e.StackTrace, e.InnerException);
            }
        }
    }

    // 몇개나 살 수 있는지 알아보는것
    // 이걸로 체크를 해야 함.
    public class FutureBuyAbleInfoStatement: OrderFutureStatement
    {
        readonly string code_;
        readonly TRADING_STATUS buyType_;
        readonly double targetPrice_;

        public FutureBuyAbleInfoStatement(string code, TRADING_STATUS buyType, double targetPrice)
        {
            this.requestName_ = "주문가능수량조회";
            this.requestCode_ = "opw30011";
            this.code_ = code;
            this.buyType_ = buyType;
            this.targetPrice_ = targetPrice;
        }

        protected override bool setInput()
        {
            if (this.setParam("계좌번호", FutureEngine.accountNumber()) == false) {
                return false;
            }

            if (this.setParam("비밀번호", PublicVar.accountPW) == false) {
                return false;
            }

            //비밀번호입력매체구분 = 00
            if (this.setParam("비밀번호입력매체", "00") == false) {
                return false;
            }
            if (this.setParam("종목코드", this.code_) == false) {
                return false;
            }
            switch (this.buyType_) {
                case TRADING_STATUS.매도:
                if (this.setParam("매도수구분", "매도") == false) {
                    return false;
                }
                break;
                case TRADING_STATUS.매수:
                if (this.setParam("매도수구분", "매수") == false) {
                    return false;
                }
                break;
            }
            if (this.setParam("해외주문유형", "지정가") == false) {
                return false;
            }
            if (this.setParam("주문표시가격", this.targetPrice_.ToString("##0.####")) == false) {
                return false;
            }

            return true;
        }

        // watcing 선물 데이터 가지고 오는 공용 함수
        public override void receive(object sender, AxKFOpenAPILib._DKFOpenAPIEvents_OnReceiveTrDataEvent apiEvent)
        {
            var bot = ControlGet.getInstance.futureBot();
            FutureData futureData = (FutureData) bot.getStockDataCode(this.code_);
            if (futureData == null) {
                return;
            }
            string ableCountStr = this.getData("주문가능수량", apiEvent, 0);
            int ableCount = 0;
            if (int.TryParse(ableCountStr, out ableCount) == false) {
                return;
            }
            switch (this.buyType_) {
                case TRADING_STATUS.매도:
                futureData.canSellCount_ = ableCount;
                break;
                case TRADING_STATUS.매수:
                futureData.canBuyCount_ = ableCount;
                break;
            }

            if (ableCount > 0) {
                if (bot.nowStockMarketTime()) {
                    bot.trade(futureData);
                }
            }
        }
    }

    public class OutstandingOrderFutureStatement: OrderFutureStatement
    {
        readonly TRADING_STATUS buyType_;

        public OutstandingOrderFutureStatement(TRADING_STATUS buyType)
        {
            this.requestName_ = "미체결내역조회";
            this.requestCode_ = "opw30001";
            this.buyType_ = buyType;
        }

        protected override bool setInput()
        {
            if (this.setParam("계좌번호", FutureEngine.accountNumber()) == false) {
                return false;
            }

            if (this.setParam("비밀번호", "") == false) {
                return false;
            }
            if (this.setParam("비밀번호입력매체", "00") == false) {
                return false;
            }

            if (this.setParam("종목코드", " ") == false) {
                return false;
            }

            if (this.setParam("통화코드", " ") == false) {
                return false;
            }
            switch (this.buyType_) {
                case TRADING_STATUS.매도:
                if (this.setParam("매도수구분", "1") == false) {
                    return false;
                }
                break;
                case TRADING_STATUS.매수:
                if (this.setParam("매도수구분", "2") == false) {
                    return false;
                }
                break;
            }

            return true;
        }

        // watcing 선물 데이터 가지고 오는 공용 함수
        public override void receive(object sender, AxKFOpenAPILib._DKFOpenAPIEvents_OnReceiveTrDataEvent apiEvent)
        {
            var bot = ControlGet.getInstance.futureBot();
            int count = this.getRowCount(apiEvent);
            if (count == 0) {
                return;
            }
            var listView = FutureDlgInfo.getInstance.orderListView_;

            for (int i = 0; i < count; ++i) {
                string code = this.getData("종목코드", apiEvent, i);
                FutureData futureData = (FutureData) bot.getStockDataCode(code);
                if (futureData == null) {
                    continue;
                }
                string orderDate = this.getData("주문시각", apiEvent, i);

                string ordered = this.getData("체결수량", apiEvent, i);
                string orderCount = this.getData("주문수량", apiEvent, i);
                string orderNumber = this.getData("주문번호", apiEvent, i);

                listView.addInfo(futureData, orderDate, this.buyType_, ordered, orderCount, orderNumber);
            }
        }
    }

    public class TodayTradeRecodeStatement: OrderFutureStatement
    {
        public TodayTradeRecodeStatement()
        {
            this.requestName_ = "일자별종목별손익상세조회";
            this.requestCode_ = "opw30013";

            DateTime now = DateTime.Now;

            // 5시간 이전 요청이면 전날꺼 조회해줌.
            if (now.Hour < 5) {
                now = now.AddDays(-1);
                this.tradeDate_ = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59);
            }
        }

        public DateTime tradeDate_ = DateTime.Now;
        protected override bool setInput()
        {
            if (this.setParam("계좌번호", FutureEngine.accountNumber()) == false) {
                return false;
            }

            if (this.setParam("비밀번호", PublicVar.accountPW) == false) {
                return false;
            }

            //비밀번호입력매체구분 = 00
            if (this.setParam("비밀번호입력매체", "00") == false) {
                return false;
            }

            if (this.setParam("통화코드", "USD") == false) {
                return false;
            }

            if (this.setParam("거래일자", this.tradeDate_.ToString("yyyyMMdd")) == false) {
                return false;
            }

            return true;
        }

        public override void receive(object sender, AxKFOpenAPILib._DKFOpenAPIEvents_OnReceiveTrDataEvent apiEvent)
        {
            try {
                int count = this.getRowCount(apiEvent);
                if (count == 0) {
                    return;
                }
                var tradeView = FutureDlgInfo.getInstance.tradeRecoderView_;
                var dateTable = tradeView.makeNewTable();
                string fmt = "##,###0.##";

                for (int i = 0; i < count; ++i) {
                    string code = this.getData("종목코드", apiEvent, i);
                    int sellCount = this.getInt("매도수량", apiEvent, i);
                    int buyCount = this.getInt("매수수량", apiEvent, i);
                    double profit = this.getDouble("청산손익", apiEvent, i) / 100;
                    double tradeTax = this.getDouble("청산수수료", apiEvent, i) / 100;
                    double tradeProfit = this.getDouble("매매손익", apiEvent, i) / 100;

                    dateTable.Rows.Add(code, sellCount, buyCount, profit.ToString(fmt), tradeTax.ToString(fmt), tradeProfit.ToString(fmt));
                }
                tradeView.print(dateTable);
            }
            catch (AccessViolationException execption) {
                Logger.getInstance.print(Log.에러, "[receive:{0}] {1}\n{2}\n{3}", this.ToString(), execption.Message, execption.StackTrace, execption.InnerException);
            }
        }
    }

    public class TradeHistoryStatement: OrderFutureStatement
    {
        public TradeHistoryStatement()
        {
            this.requestName_ = "기간손익내역조회";
            this.requestCode_ = "opw40001";
        }

        public DateTime dateStart_ = DateTime.Now;
        public DateTime dateEnd_ = DateTime.Now;
        protected override bool setInput()
        {
            if (this.setParam("계좌번호", FutureEngine.accountNumber()) == false) {
                return false;
            }

            if (this.setParam("비밀번호", PublicVar.accountPW) == false) {
                return false;
            }

            //비밀번호입력매체구분 = 00
            if (this.setParam("비밀번호입력매체", "00") == false) {
                return false;
            }

            if (this.setParam("시작일자", this.dateStart_.ToString("yyyyMMdd")) == false) {
                return false;
            }

            if (this.setParam("종료일자", this.dateEnd_.ToString("yyyyMMdd")) == false) {
                return false;
            }

            if (this.setParam("통화코드", "USD") == false) {
                return false;
            }

            return true;
        }

        public override void receive(object sender, AxKFOpenAPILib._DKFOpenAPIEvents_OnReceiveTrDataEvent apiEvent)
        {
            try {
                int count = this.getRowCount(apiEvent);
                if (count == 0) {
                    return;
                }
                var tradeView = FutureDlgInfo.getInstance.tradeHistoryView_;
                var dateTable = tradeView.makeNewTable();

                for (int i = 0; i < count; ++i) {
                    string dateStr = this.getData("일자", apiEvent, i);
                    if (dateStr.Length == 0) {
                        continue;
                    }
                    DateTime date = DateTime.ParseExact(dateStr, "yyyyMMdd", CultureInfo.InvariantCulture);
                    double money = this.getDouble("외화예수금", apiEvent, i) / 100;
                    double profit = this.getDouble("청산손익", apiEvent, i) / 100;
                    double tax = this.getDouble("수수료", apiEvent, i) / 100;
                    double dayPofit = this.getDouble("일별손익금액", apiEvent, i) / 100;
                    double profitRate = this.getDouble("손익율", apiEvent, i) / 100;
                    double finalProfit = this.getDouble("최종결제청산손익", apiEvent, i) / 100;
                    dateTable.Rows.Add(date, money, profit, tax, dayPofit, profitRate, finalProfit);
                }
                tradeView.print(dateTable);
            }
            catch (AccessViolationException execption) {
                Logger.getInstance.print(Log.에러, "[receive:{0}] {1}\n{2}\n{3}", this.ToString(), execption.Message, execption.StackTrace, execption.InnerException);
            }
        }
    }

    //--------------------------------------------------------//
    // 선물 데이터 정보(차트 정보)
    public class FutureInfoStatement: OrderFutureStatement
    {
        protected string futureCode_;
        protected PRICE_TYPE priceType_;

        public override void request()
        {
            this.setInput();
            var bot = ControlGet.getInstance.futureBot();
            int nRet = bot.engine_.openApi().CommRqData(this.requestName_, this.requestCode_, "", this.screenNo_);
            Logger.getInstance.consolePrint("주문 [{0}:{1}]\t\t결과 메시지 : {2}", this.requestCode_, this.requestName_, Error.GetErrorMessage());
        }

        protected virtual string dateTime(int index, object sender, AxKFOpenAPILib._DKFOpenAPIEvents_OnReceiveTrDataEvent apiEvent)
        {
            string dateTime = this.getData("체결시간", apiEvent, index);
            return dateTime;
        }

        public override void receive(object sender, AxKFOpenAPILib._DKFOpenAPIEvents_OnReceiveTrDataEvent apiEvent)
        {
            var bot = ControlGet.getInstance.futureBot();

            Logger logger = Logger.getInstance;
            try {
                FutureData futureData = null;
                if (this.priceType_ == bot.priceType_) {
                    futureData = bot.getStockDataCode(this.futureCode_) as FutureData;
                }
                else if (this.priceType_ == PRICE_TYPE.MIN_2) {
                    futureData = bot.getRefStockDataCode(this.futureCode_, REF_PRICE_TYPE.중간_분봉) as FutureData;
                }
                else if (this.priceType_ == PRICE_TYPE.MIN_3) {
                    futureData = bot.getRefStockDataCode(this.futureCode_, REF_PRICE_TYPE.시간_분봉) as FutureData;
                }
                else if (this.priceType_ == PRICE_TYPE.ONE_MIN) {
                    futureData = bot.getRefStockDataCode(this.futureCode_, REF_PRICE_TYPE.기준_분봉) as FutureData;
                }

                if (futureData == null) {
                    return;
                }

                int count = this.getRowCount(apiEvent);
                if (count == 0) {
                    logger.print(Log.에러, "futures {0}:{1} 의 데이터 로딩 실패. 목록에서 제거 시킴. 아마 거래 정지가 풀린거 일수도", futureData.name_, futureData.code_);
             //       bot.removeStock(this.futureCode_);
                    return;
                }
                List<CandleData> newTable = new List<CandleData>();
                //@@@ 지금 받은 값이 정말 이 선물데이터 값인지 교차 검증 해야함.
                // 잘못된 선물 데이터를 줄때가 있음. (그럼 망함.)
                for (int i = 0; i < count; i++) {
                    string dateTime = "";
                    UInt64 volume = 0;
                    switch (this.priceType_) {
                        case PRICE_TYPE.MIN_1:
                        case PRICE_TYPE.MIN_2:
                        case PRICE_TYPE.MIN_3:
                        case PRICE_TYPE.ONE_MIN:
                        dateTime = this.dateTime(i, sender, apiEvent);
                        if (dateTime.Length == 0) {
                            continue;
                        }
                        volume = this.getUInt64("거래량", apiEvent, i);
                        if (volume == UInt64.MaxValue) {
                            return;
                        }
                        break;

                        case PRICE_TYPE.DAY:
                        case PRICE_TYPE.WEEK:
                        dateTime = this.getData("일자", apiEvent, i);
                        if (dateTime.Length == 0) {
                            continue;
                        }
                        volume = this.getUInt64("누적거래량", apiEvent, i);
                        if (volume == UInt64.MaxValue) {
                            return;
                        }
                        break;
                    }

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

                    CandleData priceData = new CandleData(dateTime, price, startPrice, highPrice, lowPrice, volume);
                    newTable.Add(priceData);

                    var newList = newTable.OrderByDescending(candleData => candleData.date_).ToList();
                    futureData.updatePriceTable(newList);
                }
                futureData.updatePriceTable(bot);
                futureData.recvedDataTime_ = DateTime.Now;

                FutureDlg dlg = Program.happyFuture_.futureDlg_;
                dlg.printStatus(string.Format("키움API get data [{0}], [{1}]분봉, [{2}] 선물 데이터 {3}/{4} ", DateTime.Now.ToLongTimeString(), bot.priceTypeMin(), futureData.name_, ControlGet.getInstance.futureBot().stockPoolIndex_++, ControlGet.getInstance.futureBot().stockPoolCount()));
            }
            catch (AccessViolationException execption) {
                logger.print(Log.에러, "[receive:{0}] {1}\n{2}\n{3}", this.ToString(), execption.Message, execption.StackTrace, execption.InnerException);
            }
        }
    }

    //--------------------------------------------------------//
    // 선물 종목의 차트 조회
    public class HistoryMin1Future: FutureInfoStatement
    {
        public HistoryMin1Future(string code)
        {
            this.futureCode_ = code;
            this.requestName_ = string.Format("선물{0}분차트조회요청", this.min());
            this.requestCode_ = "opc10002";
            this.priceType_ = PRICE_TYPE.MIN_1;
        }

        public virtual string min()
        {
            return PublicVar.priceType_min1.ToString();
        }

        protected override bool setInput()
        {
            // 종목코드 = 전문 조회할 종목코드(예시 : 6AM16, ESM16, ...)
            if (this.setParam("종목코드", this.futureCode_) == false) {
                return false;
            }
            // 시간단위 = 01:1분, 05:5분, 10:10분, 15:15분
            if (this.setParam("시간단위", this.min()) == false) {
                return false;
            }

            return true;
        }

        protected override string dateTime(int index, object sender, AxKFOpenAPILib._DKFOpenAPIEvents_OnReceiveTrDataEvent apiEvent)
        {
            return this.getData("체결시간", apiEvent, index);
        }
    }

    public class HistoryMin2Future: HistoryMin1Future
    {
        public HistoryMin2Future(string code) : base(code)
        {
            this.priceType_ = PRICE_TYPE.MIN_2;
        }
        public override string min()
        {
            return PublicVar.priceType_min2.ToString();
        }
    }

    public class HistoryMin3Future: HistoryMin1Future
    {
        public HistoryMin3Future(string code) : base(code)
        {
            this.priceType_ = PRICE_TYPE.MIN_3;
        }
        public override string min()
        {
            return PublicVar.priceType_min3.ToString();
        }
    }

    public class OneMinFuture: HistoryMin1Future
    {
        public OneMinFuture(string code) : base(code)
        {
            this.priceType_ = PRICE_TYPE.ONE_MIN;
        }
        public override string min()
        {
            return "1";
        }
    }

    public class HistoryTheDaysFuture: FutureInfoStatement
    {
        public HistoryTheDaysFuture(string code)
        {
            this.futureCode_ = code;
            this.requestName_ = "선물일봉차트조회";
            this.requestCode_ = "opc10003";
            this.priceType_ = PRICE_TYPE.DAY;
        }

        protected override bool setInput()
        {
            if (this.setParam("종목코드", this.futureCode_) == false) {
                return false;
            }
            //기준일자 = YYYYMMDD (20160101 연도4자리, 월 2자리, 일 2자리 형식)
            if (this.setParam("조회일자", "1") == false) {
                return false;
            }
            return true;
        }

        protected override string dateTime(int index, object sender, AxKFOpenAPILib._DKFOpenAPIEvents_OnReceiveTrDataEvent apiEvent)
        {
            return this.getData("일자", apiEvent, index);
        }
    }

    public class HistoryTheWeeksFuture: FutureInfoStatement
    {
        public HistoryTheWeeksFuture(string code)
        {
            this.futureCode_ = code;
            this.requestName_ = "선물주봉차트조회";
            this.requestCode_ = "opc10003";
            this.priceType_ = PRICE_TYPE.WEEK;
        }

        protected override bool setInput()
        {
            if (this.setParam("종목코드", this.futureCode_) == false) {
                return false;
            }
            //기준일자 = YYYYMMDD (20160101 연도4자리, 월 2자리, 일 2자리 형식)
            if (this.setParam("조회일자", "7") == false) {
                return false;
            }

            return true;
        }

        protected override string dateTime(int index, object sender, AxKFOpenAPILib._DKFOpenAPIEvents_OnReceiveTrDataEvent apiEvent)
        {
            return this.getData("일자", apiEvent, index);
        }
    }

    //--------------------------------------------------------//
    // 선물 주문 
    public class TradingStatement: StockStatement
    {
        // =================================================
        // 1:신규매도, 2:신규매수 3:매도취소, 4:매수취소, 5:매도정정, 6:매수정정
        protected KOACode.OrderType dealingType_;       //매매 구분

        // =================================================
        // 거래구분 취득(2:지정가, 1:시장가, 3:STOP, 4:STOP LIMIT)
        protected KOACode.HogaGb hogaType_;             //거래 구분 (hoga/호가) 

        // 이하 입력 값들
        public int tradingCount_ { get; set; }          //주문 수량
        public string tradingPrice_;                    //주문 가격
        public string stopPrice_ = "";
        public string code_ { get; set; }               //선물 코드
        public string orderNumber_ = "";                     //주문 번호

        protected int BUY_CODE = 1;
        protected int SELL_CODE = 0;

        public TradingStatement()
        {
            if (PublicVar.reverseOrder) {
                BUY_CODE = 0;
                SELL_CODE = 1;
            }
        }

        public string code()
        {
            return this.code_;
        }

        public override bool isTradingOrder()
        {
            return true;
        }

        protected override bool setInput()
        {
            return false;
        }

        public override void request()
        {
            if (this.tradingCount_ <= 0) {
                return;
            }

            if (this.setInput() == false) {
                return;
            }
#if DEBUG
            // 디버그 모드에선 주문을 걸지 말자.
            return;
#endif
            FutureDlgInfo.getInstance.captureFormImgName();
            var bot = ControlGet.getInstance.futureBot();
            int nRet = bot.engine_.openApi().SendOrder(
                                        "주문",
                                        this.screenNo_,
                                        FutureEngine.accountNumber(),
                                        this.dealingType_.code,
                                        this.code(),
                                        this.tradingCount_,
                                        this.tradingPrice_,
                                        this.stopPrice_,
                                        this.hogaType_.code,
                                        this.orderNumber_);
            if (Error.IsError(nRet)) {
                Logger.getInstance.print(Log.API조회, "선물주문:{0}, 선물 코드:{1}, 갯수:{2}, 주문가:{3}, in 계좌:{4}",
                    Error.GetErrorMessage(), this.code(), this.tradingCount_, this.tradingPrice_, FutureEngine.accountNumber());
            }
            else {
                Logger.getInstance.print(Log.에러, "선물주문 : " + Error.GetErrorMessage());
            }
            Logger.getInstance.print(Log.API조회, "주문 완료 :{0}, 번호:{1}, 주문:{2}, 호가:{3}, 선물:{4}, 갯수:{5}, 주문가:{6}, 계좌:{7}"
                , this.requestName_, this.screenNo_, this.dealingType_.name, this.hogaType_.name, this.code(), this.tradingCount_, this.tradingPrice_, FutureEngine.accountNumber());
        }

        public override void receive(object sender, AxKFOpenAPILib._DKFOpenAPIEvents_OnReceiveTrDataEvent apiEvent)
        {
        }
    }

    public class SellFuture: TradingStatement
    {
        // 신규 포지선 진입시
        public SellFuture(string code, int tradingCount, double tradingPrice)
        {
            this.requestName_ = "SELL_FUTURE";
            this.code_ = code;
            this.tradingCount_ = tradingCount;
            if (tradingPrice > 0) {
                this.tradingPrice_ = tradingPrice.ToString("###0.####");
                this.hogaType_ = KOACode.futureHoga[0];                  //지정가
            }
            else {
                this.tradingPrice_ = "";
                this.hogaType_ = KOACode.futureHoga[1];                  //시장가    
            }
        }

        // 기존 포지션 청산시
        public SellFuture(FutureData futureData)
        {
            this.requestName_ = "SELL_FUTURE";
            this.code_ = futureData.code_;
            this.tradingCount_ = futureData.buyCount_;
            this.tradingPrice_ = "";

            this.hogaType_ = KOACode.futureHoga[1];                  //시장가            
        }

        const bool staticPrice = false;
        protected override bool setInput()
        {
            this.dealingType_ = KOACode.futureOrderType[SELL_CODE];          //신규매도

            Logger.getInstance.print(Log.API조회, "선물[{0}] 매수[{1}] 구입타입[{2}] : 갯수 {3} x 가격 {4}"
                , this.code_, this.dealingType_.name, this.hogaType_.name, this.tradingCount_, this.tradingPrice_);
            return true;
        }
    }

    public class BuyFuture: TradingStatement
    {
        //신규 포지선 진입시
        public BuyFuture(string code, int tradingCount, double tradingPrice)
        {
            this.requestName_ = "BUY_FUTURE";
            this.code_ = code;
            this.tradingCount_ = tradingCount;
            if (tradingPrice > 0) {
                this.tradingPrice_ = tradingPrice.ToString("###0.####");
                this.hogaType_ = KOACode.futureHoga[0];                  //지정가
            }
            else {
                this.tradingPrice_ = "";
                this.hogaType_ = KOACode.futureHoga[1];                  //시장가
            }
        }

        // 기존 포지션 청산시
        public BuyFuture(FutureData futureData)
        {
            this.requestName_ = "BUY_FUTURE";
            this.code_ = futureData.code_;
            this.tradingCount_ = futureData.buyCount_;
            this.tradingPrice_ = "";

            this.hogaType_ = KOACode.futureHoga[1];                  //시장가
        }

        const bool staticPrice = false;
        protected override bool setInput()
        {
            this.dealingType_ = KOACode.futureOrderType[BUY_CODE];          //신규매수

            Logger.getInstance.print(Log.API결과, "선물[{0}] 매도 주문[{1}] 판매타입[{2}] : 갯수 {3} x 가격 {4}"
               , this.code_, this.dealingType_.name, this.hogaType_.name, this.tradingCount_, this.tradingPrice_);

            return true;
        }
    }


    public class SellCancleFuture: TradingStatement
    {
        // 기존 포지션 청산시
        public SellCancleFuture(FutureData futureData)
        {
            this.requestName_ = "SELL_CANCEL_FUTURE";
            this.code_ = futureData.code_;
            this.tradingCount_ = futureData.buyCount_;
            this.tradingPrice_ = "";

            this.hogaType_ = KOACode.futureHoga[1];                  //시장가
        }

        const bool staticPrice = false;
        protected override bool setInput()
        {
            this.dealingType_ = KOACode.futureOrderType[2];          //매도취소
            Logger.getInstance.print(Log.API조회, "선물[{0}] 매수[{1}] 구입타입[{2}] : 갯수 {3} x 가격 {4}"
                , this.code_, this.dealingType_.name, this.hogaType_.name, this.tradingCount_, this.tradingPrice_);
            return true;
        }
    }

    public class BuyCancleFuture: TradingStatement
    {
        // 기존 포지션 청산시
        public BuyCancleFuture(FutureData futureData)
        {
            this.requestName_ = "BUY_CANCLE_FUTURE";
            this.code_ = futureData.code_;
            this.tradingCount_ = futureData.buyCount_;
            this.tradingPrice_ = "";

            this.hogaType_ = KOACode.futureHoga[1];                  //시장가
        }

        const bool staticPrice = false;
        protected override bool setInput()
        {
            this.dealingType_ = KOACode.futureOrderType[3];          //매수취소
            Logger.getInstance.print(Log.API결과, "선물[{0}] 매도 주문[{1}] 판매타입[{2}] : 갯수 {3} x 가격 {4}"
               , this.code_, this.dealingType_.name, this.hogaType_.name, this.tradingCount_, this.tradingPrice_);

            return true;
        }
    }
}
