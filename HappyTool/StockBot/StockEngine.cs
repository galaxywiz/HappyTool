using System;
using KiwoomCode;
using HappyTool.Stock;
using HappyTool.DialogControl.StockDialog;
using UtilLibrary;
using StockLibrary;
using KiwoomEngine;
using AxKHOpenAPILib;
using System.Collections.Generic;
using System.Globalization;

namespace HappyTool
{
    class StockEngine :Engine
    {
        AxKHOpenAPI openApi_ = null;
        HappyTool happyTool_ = null;
        public StockEngine(HappyTool happTool)
        {
            happyTool_ = happTool;
        }

        protected override int apiRequestDelay()
        {
            return 1500;
        }

        public void setup(Bot bot, AxKHOpenAPI api)
        {
            openApi_ = api;
            openApi_.CreateControl();
            this.setup(bot);
        }

        //---------------------------------------------------------------------
        //라이브러리 포팅 함수들
        public AxKHOpenAPI khOpenApi()
        {
            return openApi_;
        }

        public bool connected()
        {
            if (khOpenApi() == null) {
                return false;
            }
            int state = khOpenApi().GetConnectState();
            if (state == 0) {
                return false;
            }
            return true;
        }

        public bool start()
        {
            if (connected() == true) {
                return true;
            }
            if (doStart() == false) {
                return false;
            }

            //ocx 콜백 함수 등록
            // 일반 시세, 계좌 조회 콜백
            openApi_.OnReceiveRealData += new _DKHOpenAPIEvents_OnReceiveRealDataEventHandler(axKHOpenAPI_OnReceiveRealData);

            // 주식 주문 콜백
            openApi_.OnReceiveTrData += new _DKHOpenAPIEvents_OnReceiveTrDataEventHandler(axKHOpenAPI_OnReceiveTrData);
            openApi_.OnReceiveMsg += new _DKHOpenAPIEvents_OnReceiveMsgEventHandler(axKHOpenAPI_OnReceiveMsg);
            openApi_.OnReceiveChejanData += new _DKHOpenAPIEvents_OnReceiveChejanDataEventHandler(axKHOpenAPI_OnReceiveChejanData);

            //
            openApi_.OnEventConnect += new _DKHOpenAPIEvents_OnEventConnectEventHandler(axKHOpenAPI_OnEventConnect);
            openApi_.OnReceiveRealCondition += new _DKHOpenAPIEvents_OnReceiveRealConditionEventHandler(axKHOpenAPI_OnReceiveRealCondition);
            openApi_.OnReceiveTrCondition += new _DKHOpenAPIEvents_OnReceiveTrConditionEventHandler(axKHOpenAPI_OnReceiveTrCondition);
            openApi_.OnReceiveConditionVer += new _DKHOpenAPIEvents_OnReceiveConditionVerEventHandler(axKHOpenAPI_OnReceiveConditionVer);

            Logger.getInstance.print(Log.API조회, "로그인창 열기 성공");
            return true;
        }

        public bool doStart()
        {
            if (connected()) {
                return true;
            }
            if (khOpenApi() == null) {
                return false;
            }
            if (openApi_.CommConnect() != 0) {
                Logger.getInstance.print(Log.에러, "로그인창 열기 실패");
                return false;
            }
            return true;
        }

        public void doDisconnect()
        {
            if (khOpenApi() == null) {
                return;
            }
            if (connected()) {
                openApi_.CommTerminate();
            }
        }

        public void quit()
        {
            this.shutdown();
            if (khOpenApi() == null) {
                return;
            }
            if (connected()) {
                openApi_.CommTerminate();
            }

            thread_.Abort();
        }

        string getChejan(int code)
        {
            return openApi_.GetChejanData(code).Trim();
        }

        public override string userId()
        {
            return openApi_.GetLoginInfo("USER_ID").Trim();
        }
        
        public bool isTestServer()
        {
            if (accountNum_.ToString().StartsWith("5")) {
                return false;
            }
            return true;
        }

        //---------------------------------------------------------------------
        // 주문등의 주식 관련 명령 실행
        public override bool loadAccountInfo()
        {
            if (connected() == false) {
                return false;
            }
            happyTool_.stockDlg_.userId().Text = this.userId();

            string[] accountNumber = openApi_.GetLoginInfo("ACCNO").Split(';');

            foreach (string num in accountNumber) {
                if (num == "") {
                    continue;
                }
                happyTool_.stockDlg_.account().Text = num.Trim();
                accountNum_ = num.Trim();
            }

            Logger.getInstance.print(Log.API결과, "계좌 번호 가지고 오기 성공");
            Program.happyTool_.loaded_ = true;
            return true;
        }

        public override void requestStockData(string code, PRICE_TYPE priceType, bool forceBuy = false)
        {
            StockInfoStatement statement = null;
            switch (priceType) {
                case PRICE_TYPE.MIN_1:
                    statement = new HistoryMin1Stock(code);
                    break;
                case PRICE_TYPE.MIN_2:
                    statement = new HistoryMin2Stock(code);
                    break;
                case PRICE_TYPE.MIN_3:
                    statement = new HistoryMin3Stock(code);
                    break;
                case PRICE_TYPE.DAY:
                    statement = new HistoryTheDaysStock(code);
                    break;
                case PRICE_TYPE.WEEK:
                    statement = new HistoryTheWeeksStock(code);
                    break;
            }
            if (forceBuy) {
                statement.immediatelyBuy_ = forceBuy;
            }
            this.addOrder(statement);
        }

        public bool checkAlreadyOrderPriceLoad(string code)
        {
            foreach (var statement in statmentOrderPool_) {
                if (statement.GetType() == typeof(HistoryMin1Stock)
                 || statement.GetType() == typeof(HistoryMin2Stock)
                 || statement.GetType() == typeof(HistoryMin3Stock)
                 || statement.GetType() == typeof(HistoryTheDaysStock)
                 || statement.GetType() == typeof(HistoryTheWeeksStock)) {
                    var s = statement as StockInfoStatement;
                    if (s.stockCode_ == code) {
                        return true;
                    }
                }
            }
            return false;
        }

        //------------------------------------------------------------------------//
        // 주식은 워낙 많으니 리얼 데이터 받게 처리
        // 해선은 가짓수가 별로 없어서 리얼데이터 받는게 자동임
        List<string> receiveRealDataStocks_ = new List<string>();

        bool existRegRealData(string code)
        {
            // index 가 없으면 존재하지 않음.
            if (receiveRealDataStocks_.IndexOf(code) == -1) {
                return false;
            }
            return true;
        }

        bool addRegRealData(string code)
        {
            if (this.existRegRealData(code)) {
                return false;
            }
            receiveRealDataStocks_.Add(code);
            return true;
        }

        const int REAL_SCREN_NUM = 6000;
        const int REAL_LIMIT_NUMBER = 95;      // 키움에서 100개까지 real 데이터 수신 가능하게 했음.
        public int realLimit()
        {
            return REAL_LIMIT_NUMBER;
        }

        public int regRealData(string code)
        {
            if (this.existRegRealData(code)) {
                return -1;
            }

            int scrNum = receiveRealDataStocks_.IndexOf(code) + REAL_SCREN_NUM;
            int ret = openApi_.SetRealReg(scrNum.ToString(), code, "9001;10", "1");
            if (ret != 0) {
                Logger.getInstance.print(Log.에러, "주식 {0} 의 실시간 등록 실패", code);
                return -1;
            }
            if (this.addRegRealData(code) == false) {
                Logger.getInstance.print(Log.에러, "실시간주식 {0} 의 관리 등록 실패", code);
                return -1;
            }
            return scrNum;
        }

        public void unRegRealData(string code)
        {
            if (this.existRegRealData(code) == false) {
                return;
            }
            int scrNum = receiveRealDataStocks_.IndexOf(code) + REAL_SCREN_NUM;
            openApi_.SetRealRemove(scrNum.ToString(), code);

            receiveRealDataStocks_.Remove(code);
        }

        public void unRegRealDataAll()
        {
            openApi_.SetRealRemove("ALL","ALL");
            receiveRealDataStocks_.Clear();
        }

        //---------------------------------------------------------------------
        // 주식 모듈의 콜벡 델리게이션
        public void axKHOpenAPI_OnEventConnect(object sender, _DKHOpenAPIEvents_OnEventConnectEvent apiEvent)
        {
            var telegram = ControlGet.getInstance.stockBot().telegram_;
            Logger logger = Logger.getInstance;
            try {
                if (Error.IsError(apiEvent.nErrCode)) {
                    logger.print(Log.StockAPI콜백, "[로그인 결과] " + Error.GetErrorMessage());
                    this.loadAccountInfo();
                    happyTool_.buttonStart().Enabled = true;
                    happyTool_.Button_start_Click(sender, null);
                }
                else {
                    logger.print(Log.에러, "[로그인 에러] " + Error.GetErrorMessage());
                    if (telegram != null) {
                        telegram.sendMessage("로그인 에러: {0}", Error.GetErrorMessage());
                    }
                    happyTool_.quit();
                }
            }
            catch (AccessViolationException execption) {
                logger.print(Log.에러, "[로그인 콜백 에러]" + execption.Message);
                if (telegram != null) {
                    telegram.sendMessage("로그인 에러: {0}", execption.Message);
                }
                happyTool_.quit();
            }
        }

        // OnReceiveTRData()이벤트 함수는 주문후 호출되며 주문번호를 얻을수 있습니다.
        // 만약 이 이벤트 함수에서 주문번호를 얻을수 없으면 해당 주문은 실패한 것입니다.
        public virtual void axKHOpenAPI_OnReceiveTrData(object sender, _DKHOpenAPIEvents_OnReceiveTrDataEvent apiEvent)
        {
            Logger logger = Logger.getInstance;
            try {
                string keyStr = apiEvent.sScrNo;
                StockStatement statement = (StockStatement) this.getStockStatement(keyStr);
                if (statement == null) {
                    return;
                }

                statement.receive(sender, apiEvent);
                //받은 시각 기록
                statement.receivedTick_ = DateTime.Now.Ticks;

                if (statement.GetType() == typeof(SellStock) || statement.GetType() == typeof(BuyStock)) {
                    Logger.getInstance.print(Log.StockAPI콜백, "{0} 에 주문 처리중", keyStr);
                    var order = (TradingStatement) statement;
                    StockData stockData = bot_.getStockDataCode(order.code_);
                    if (stockData != null) {
                        stockData.regScreenNumber_ = keyStr;
                    }
                }
                else {
                    removeScreenNum(keyStr);
                }
            }
            catch (AccessViolationException execption) {
                logger.print(Log.에러, "[주문 콜백 에러] {0}\n{1}\n{2}", execption.Message, execption.StackTrace, execption.InnerException);
            }
        }

        public void axKHOpenAPI_OnReceiveMsg(object sender, _DKHOpenAPIEvents_OnReceiveMsgEvent apiEvent)
        {
            // 그냥 이 주문이 잘 실행되었습니다 하는 콜백...
            //화면번호:1204 | RQName:주식분봉차트조회요청 | TRCode:OPT10080 | 메세지:[00Z310] 모의투자 조회가 완료되었습니다
            string log = string.Format("화면번호:{0} | RQName:{1} | TRCode:{2} | 메세지:{3}", apiEvent.sScrNo, apiEvent.sRQName, apiEvent.sTrCode, apiEvent.sMsg);
            Logger.getInstance.print(Log.StockAPI콜백, log);

            // 장 종료이거나 하면 돈을 다시 넣어줘야 함.
            if (apiEvent.sMsg.StartsWith("[00Z218]")) {
                string tradingMoney = this.getChejan(KOAChejanCode.미채결수량);
                Int64 money = 0;
                Int64.TryParse(tradingMoney, out money);
                ControlGet.getInstance.stockBot().addAccountMoney(money);
            }
        }

        // OnReceiveChejan()이벤트 함수는 주문접수, 체결, 잔고발생시 호출되며
        // 이 이벤트 함수를 통해 대부분의 주문관련 정보를 얻을 수 있습니다.
        /* 이런식으로 로그가 옴.
        2018-07-20 오전 11:15:42,StockAPI콜백,체결통보 한샘                                    :0 체결수량: /, 체결가격  (sGubun = 0)
        2018-07-20 오전 11:15:42,StockAPI콜백,구분 : 잔고통보                                                            (sGubun = 1)
        2018-07-20 오전 11:15:42,StockAPI콜백,체결통보 한샘                                    :0 체결수량: /, 체결가격
        2018-07-20 오전 11:15:43,StockAPI콜백,구분 : 잔고통보
        */
        public void axKHOpenAPI_OnReceiveChejanData(object sender, _DKHOpenAPIEvents_OnReceiveChejanDataEvent apiEvent)
        {
            Logger logger = Logger.getInstance;
            try {
                string stockName = this.getChejan(KOAChejanCode.종목명);

                if (apiEvent.sGubun == "0") {
                    // 주문이 체결 될때마다 옴
                    //logger.print(Log.StockAPI콜백, "구분 : 주문체결통보");
                    //logger.print(Log.StockAPI콜백, "주문/체결시간 : " + this.getChejan(KOAChejanCode.주문_체결시간));
                    //logger.print(Log.StockAPI콜백, "종목명 : " + stockName);
                    //logger.print(Log.StockAPI콜백, "주문수량 : " + this.getChejan(KOAChejanCode.주문수량));
                    //logger.print(Log.StockAPI콜백, "주문가격 : " + this.getChejan(KOAChejanCode.주문가격));
                    //logger.print(Log.StockAPI콜백, "체결수량 : " + this.getChejan(KOAChejanCode.체결량));
                    //logger.print(Log.StockAPI콜백, "체결가격 : " + this.getChejan(KOAChejanCode.체결가));
                }
                else if (apiEvent.sGubun == "1") {
                    // 주문 체결로 예수금 변동 사항이 있을때 마다 옴.
                    logger.print(Log.StockAPI콜백, "구분 : 잔고통보");
                    string moneyStr = this.getChejan(KOAChejanCode.예수금);
                    Int64 money = 0;
                    if (Int64.TryParse(moneyStr, out money)) {
                        var stockBot = bot_ as StockBot;
                        stockBot.setAccountMoney(money);
                        
                    } else {
                        logger.print(Log.StockAPI콜백, "예수금을 읽을 수 없음");
                    }
                }
                else if (apiEvent.sGubun == "3") {
     //               logger.print(Log.StockAPI콜백, "구분 : 특이신호");
                }

                string code = this.getChejan(KOAChejanCode.종목코드);
                StockData stockData = bot_.getStockDataCode(code);
                if (stockData != null) {
                    string orderNumber = this.getChejan(KOAChejanCode.종목코드);
                    stockData.orderNumber_ = orderNumber;
                }

                string remainCountStr = this.getChejan(KOAChejanCode.미채결수량);
                int remainCount = 0;
                if (int.TryParse(remainCountStr, out remainCount) == false) {
                    logger.print(Log.StockAPI콜백, "미채결수량 을 읽을 수 없음");
                }
                // 미체결 수량이 없으면
                if (remainCount == 0) {
                 //   bot_.requestMyAccountInfo();
                    this.sendChejanLog("체결 통보");
                }
            }
            catch (AccessViolationException execption) {
                logger.print(Log.에러, "[채결 / 잔고 처리 콜백 에러] {0}\n{1}\n{2}", execption.Message, execption.StackTrace, execption.InnerException);
            }
        }

        public void sendChejanLog(string title)
        {
            string code = this.getChejan(KOAChejanCode.종목코드);
            string name = this.getChejan(KOAChejanCode.종목명);
            KStockData kStockData = (KStockData) bot_.getStockDataCode(code);
            if (kStockData == null) {
                kStockData = bot_.getStockData(name) as KStockData;
                if (kStockData == null) {
                    Logger.getInstance.print(Log.에러, "{0}:{1} 코드의 종목이 체결 데이터로 왔는데 이건 모니터닝이 아님", name, code);
                    return;
                }
            }

            try {
                string log = string.Format("*** {0} ***\n", title);
                log += string.Format(" - 매매 구분: {0} / {1}\n", this.getChejan(KOAChejanCode.매수_매도구분), this.getChejan(KOAChejanCode.매매구분));
                string dateStr = this.getChejan(KOAChejanCode.주문_체결시간);
                if (dateStr == "") {
                    return;
                }
                // 103134   (시분초)
                DateTime date = DateTime.ParseExact(dateStr, "HHmmss", CultureInfo.InvariantCulture);
                log += string.Format(" - 시간: {0}\n", date);
                log += string.Format(" - 종목: {0}\n", this.getChejan(KOAChejanCode.종목명));

                string tradingAmountStr = this.getChejan(KOAChejanCode.주문수량);
                int tradingAmount = 0;
                if (int.TryParse(tradingAmountStr, out tradingAmount) == false) {
                    return;
                }

                // 체결 수량
                string tradingCountStr = this.getChejan(KOAChejanCode.체결량);
                int tradingCount = 0;
                if (int.TryParse(tradingCountStr, out tradingCount) == false) {
                    return;
                }

                string tradingPriceStr = this.getChejan(KOAChejanCode.체결가);
                double tradingPrice = 0.0f;
                if (double.TryParse(tradingPriceStr, out tradingPrice) == false) {
                    return;
                }

                log += string.Format(" - 체결 수량: {0}/{1}\n", tradingCountStr, tradingAmountStr);
                log += string.Format(" - 체결가 : {0}\n", tradingPriceStr);
                log += string.Format(" - 총 구입 가격 : {0:##,###0}", tradingCount * tradingPrice);

                removeScreenNum(kStockData.regScreenNumber_);
                kStockData.resetBuyInfo();
                kStockData.setBuyInfo(tradingCountStr, tradingPriceStr);

                bot_.telegram_.sendMessage(log);
                bot_.updatePoolView();
                StockDlgInfo.getInstance.updateBuyPoolView();
            } catch (Exception e) {
                Logger.getInstance.print(Log.에러, "[체결 파싱 에러] {0}\n{1}\n{2}", e.Message, e.StackTrace, e.InnerException);
            }
        }

        // 조회요청이 성공하면 관련 실시간 데이터를 서버에서 자동으로 OnReceiveRealData()이벤트 함수로 전달해줍니다.
        public void axKHOpenAPI_OnReceiveRealData(object sender, _DKHOpenAPIEvents_OnReceiveRealDataEvent apiEvent)
        {
            //Logger logger = Logger.getInstance;
            //try {
            //    if (bot_.nowStockMarketTime() == false) {
            //        return;
            //    }

            //    if (apiEvent.sRealType == "주식체결" || apiEvent.sRealType == "주식시세") {
            //        string code = apiEvent.sRealKey;
            //        var stockData = bot_.getStockDataCode(code);
            //        if (stockData == null) {
            //            return;
            //        }

            //        // 이거 추가하려면, regRealData 함수의 SetRealReg 수정 필요
            //        double price = double.Parse(openApi_.GetCommRealData(apiEvent.sRealType, 10).Trim());     // 현재가
            //        //double start = double.Parse(openApi_.GetCommRealData(apiEvent.sRealType, 16).Trim());     // 시가
            //        //double high = double.Parse(openApi_.GetCommRealData(apiEvent.sRealType, 17).Trim());      // 고가
            //        //double low = double.Parse(openApi_.GetCommRealData(apiEvent.sRealType, 18).Trim());       // 저가
            //        //int vol = int.Parse(openApi_.GetCommRealData(apiEvent.sRealType, 13).Trim());             // 거래량

            //        if (stockData.setRealTimeData(bot_, Math.Abs(price))) {
            //        //    this.updateStockDataView(stockData);
            //        }

            ////        ControlGet.getInstance.stockBot().updatePoolView();
            //    }
            //}
            //catch (AccessViolationException execption) {
            //    logger.print(Log.에러, "[주식 데이터 콜백 에러] {0}\n{1}\n{2}", execption.Message, execption.StackTrace, execption.InnerException);
            //}
        }

        public virtual void axKHOpenAPI_OnReceiveRealCondition(object sender, _DKHOpenAPIEvents_OnReceiveRealConditionEvent apiEvent)
        {
            Logger logger = Logger.getInstance;
            try {
                //logger.print(Log.StockAPI콜백, "========= 조건조회 실시간 편입/이탈 ==========");
                //logger.print(Log.StockAPI콜백, "[종목코드] : " + apiEvent.sTrCode);
                //logger.print(Log.StockAPI콜백, "[실시간타입] : " + apiEvent.strType);
                //logger.print(Log.StockAPI콜백, "[조건명] : " + apiEvent.strConditionName);
                //logger.print(Log.StockAPI콜백, "[조건명 인덱스] : " + apiEvent.strConditionIndex);
            }
            catch (AccessViolationException execption) {
                logger.print(Log.에러, "[실시간 조건 조회 콜백 에러] {0}\n{1}\n{2}", execption.Message, execption.StackTrace, execption.InnerException);
            }
        }

        public void axKHOpenAPI_OnReceiveTrCondition(object sender, _DKHOpenAPIEvents_OnReceiveTrConditionEvent apiEvent)
        {
            Logger logger = Logger.getInstance;
            try {
                //logger.print(Log.StockAPI콜백, "[화면번호] : " + apiEvent.sScrNo);
                //logger.print(Log.StockAPI콜백, "[종목리스트] : " + apiEvent.strCodeList);
                //logger.print(Log.StockAPI콜백, "[조건명] : " + apiEvent.strConditionName);
                //logger.print(Log.StockAPI콜백, "[조건명 인덱스 ] : " + apiEvent.nIndex.ToString());
                //logger.print(Log.StockAPI콜백, "[연속조회] : " + apiEvent.nNext.ToString());
            }
            catch (AccessViolationException execption) {
                logger.print(Log.에러, "[컨디션 체크 콜백 에러] {0}\n{1}\n{2}", execption.Message, execption.StackTrace, execption.InnerException);
            }
        }

        public void axKHOpenAPI_OnReceiveConditionVer(object sender, _DKHOpenAPIEvents_OnReceiveConditionVerEvent apiEvent)
        {
            Logger logger = Logger.getInstance;
            try {
                if (apiEvent.lRet == 1) {
                    logger.print(Log.StockAPI콜백, "[이벤트] 조건식 저장 성공");
                }
                else {
                    logger.print(Log.에러, "[이벤트] 조건식 저장 실패 : " + apiEvent.sMsg);
                }
            }
            catch (AccessViolationException execption) {
                logger.print(Log.에러, "[이벤트 콜백 에러] {0}\n{1}\n{2}", execption.Message, execption.StackTrace, execption.InnerException);
            }
        }
    }
}
