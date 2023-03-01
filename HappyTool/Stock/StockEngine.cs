using System;
using System.Collections.Generic;
using System.Collections;
using KiwoomCode;
using System.Threading;
using HappyTool.Stock;
using HappyTool.Network;
using NetLibrary;
using System.Runtime.ExceptionServices;

namespace HappyTool
{
    class StockEngine :SingleTon<StockEngine>
    {
        private Thread thread_ = null;
        private HappyTool happyTool_ = null;
        private AxKHOpenAPILib.AxKHOpenAPI khOpenApi_ = null;

        const Int16 KH_LIMIT_ORDER = 200;       // 키움증권 제한된 키값 갯수
        // 
        private object orderPoolLock_ = null;
        private List<StockStatement> statmentOrderPool_ = new List<StockStatement>();       //주문을 쌓는 pool

        // 매매 이외의 주문은 모두 이쪽으로 처리 receive 처리함.
        private object receivePoolLock_ = null;
        private Dictionary<string, StockStatement> statmentReceivePool_ = new Dictionary<string, StockStatement>(); // 매매 이외의 주문 관리

        //매매 관련 주문은 다른 경로로 데이터가 흐름.
        private object tradingPoolLock_ = null;
        private Dictionary<string, TradingStatement> tradingReceivePool_ = new Dictionary<string, TradingStatement>(); // 매매 주문 관리

        ToolServer server_ = null;
        bool[] screenNumPool_ = new bool[KH_LIMIT_ORDER];

        public void setup(HappyTool happyTool)
        {
            happyTool_ = happyTool;
            khOpenApi_ = happyTool_.openApi();
            khOpenApi_.CreateControl();
            //khOpenApi_.SkinAllThreads();

            orderPoolLock_ = new object();
            receivePoolLock_ = new object();
            tradingPoolLock_ = new object();

            for(int i=0; i < KH_LIMIT_ORDER; ++i) {
                screenNumPool_[i] = false;
            }
            thread_ = new Thread(this.run);
            thread_.Start();

            if (server_ == null) {
                //server_ = new ToolServer(NetUtil.localIp(), 50000);
            }
        }

        //---------------------------------------------------------------------
        // 쓰레드 실행 루프
        private bool runLoop_ = true;
        public bool runLoop()
        {
            return runLoop_;
        }

        public void shutdown()
        {
            runLoop_ = false;
        }

        private void run()
        {
            // 요청과 상태 쓰레드를 동시에 가지고 간다. 
            // 분리하면 주식 모듈로의 send /recv 가 꼬일 수 있음.. orz (그래서 MFC가 실패..)
            while (this.runLoop()) {
                Thread.Sleep(1);

                this.processStockOrder();
                StockBot.getInstance.process();
            //    this.processOrderCleanup();
            }
            if (server_ != null) {
                server_.closeServer();
            }
        }

        string getScreenNum()
        {
            Int16 index = 0;
            for (; index < KH_LIMIT_ORDER; ++index) {
                if (screenNumPool_[index] == false) {
                    screenNumPool_[index] = true;
                    break;
                }
            }
            if (index == KH_LIMIT_ORDER) {
                return "";
            }
            return makeScreenNum(index);
        }

        void removeScreenNum(string key)
        {
            string temp = key.Substring(3);
            int index = int.Parse(temp);
            screenNumPool_[index] = false;
        }

        string makeScreenNum(Int16 key)
        {
            return string.Format("scr{0}", key);
        }

        //---------------------------------------------------------------------
        //주식 실제 주문 명령 실행 부분
        private void processStockOrder()
        {
            lock (orderPoolLock_) {
                ArrayList deleteOrder = new ArrayList();

                foreach (StockStatement statement in statmentOrderPool_) {
                    string screenNum = this.getScreenNum();
                    if (screenNum.Length < 1) {
                        continue;
                    }
                    // 키값이 성공적으로 가져 오면, 실행하고
                    statement.screenNo_ = screenNum;
                    statement.request();

                    //recive 풀에 넣기
                    if ((statement.GetType() == typeof(BuyStock))
                        || (statement.GetType() == typeof(SellStock))) {
                        TradingStatement tradingStat = (TradingStatement) statement;
                        this.addTradingReceive(tradingStat);
                    } else {
                        this.addStatmentReceive(statement);
                    }
                    deleteOrder.Add(statement);

                    // 키움증권 제한 1초에 1번만 요청 가능 (나쁜놈들)
                    Thread.Sleep(1000 / 1);

                    if (!this.runLoop()) {
                        break;
                    }
                }

                foreach (StockStatement statement in deleteOrder) {
                    statmentOrderPool_.Remove(statement);
                }
            }
        }

        private void processOrderCleanup()
        {
            // 주식 명령을 내린뒤, receive를 여러번 받을 수 있음. 
            // 그래서 지연 삭제를 구현함.
            long now = DateTime.Now.Ticks;
            long turn = TimeSpan.TicksPerSecond * 60;
            lock (receivePoolLock_) {
                ArrayList deleteOrder = new ArrayList();

                foreach (KeyValuePair<string, StockStatement> data in statmentReceivePool_) {
                    StockStatement stat = data.Value;
                    //1분 0초 + 10초 < 1분 20초
                    if (stat.receivedTick_ + turn < now) {
                        deleteOrder.Add(data.Key);
                    }
                }

                foreach (StockStatement deleteKey in deleteOrder) {
                    statmentOrderPool_.Remove(deleteKey);
                }
            }
        }

        public void addOrder(StockStatement statement)
        {
            lock (orderPoolLock_) {
                statmentOrderPool_.Add(statement);
            }
        }

        private void addStatmentReceive(StockStatement statement)
        {
            lock (receivePoolLock_) {
                statmentReceivePool_[statement.screenNo_] = statement;
            }
        }

        private StockStatement getStockStatement(string key)
        {
            lock (receivePoolLock_) {
                foreach (KeyValuePair<string, StockStatement> data in statmentReceivePool_) {
                    if (data.Key == key) {
                        return data.Value;
                    }
                }
            }
            return null;
        }

        private StockStatement popStockStatement(string key)
        {
            lock (receivePoolLock_) {
                StockStatement statement = null;

                if (statmentReceivePool_.TryGetValue(key, out statement)) {
                    statmentReceivePool_.Remove(key);
                    return statement;
                }
            }
            return null;
        }

        // 주식 주문은 이쪽 풀에서 따로 관리
        // 리시브로 받을때 스크린number가 없어서 키를 주식 코드로 해야 함.
        private void addTradingReceive(TradingStatement statement)
        {
            lock (tradingPoolLock_) {
                string key = statement.stockCode_.ToString();
                tradingReceivePool_[key] = statement;
            }
        }

        private TradingStatement getTradingStatement(string stockCode)
        {
            lock (tradingPoolLock_) {
                foreach (KeyValuePair<string, TradingStatement> data in tradingReceivePool_) {
                    TradingStatement statement = data.Value;
                    string key = statement.stockCode_.ToString();
                    if (key == stockCode) {
                        return data.Value;
                    }
                }
            }
            return null;
        }

        private TradingStatement popTradingStatement(string key)
        {
            lock (tradingPoolLock_) {
                TradingStatement statement = null;

                if (tradingReceivePool_.TryGetValue(key, out statement)) {
                    tradingReceivePool_.Remove(key);
                    return statement;
                }
            }
            return null;
        }

        //---------------------------------------------------------------------
        //라이브러리 포팅 함수들
        public AxKHOpenAPILib.AxKHOpenAPI khOpenApi()
        {
            return khOpenApi_;
        }

        public bool connected()
        {
            int state = khOpenApi_.GetConnectState();
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

            if (khOpenApi_.CommConnect() != 0) {
                Logger.getInstance.print(Log.에러, "로그인창 열기 실패");
                return false;
            }
                
            Logger.getInstance.print(Log.API조회, "로그인창 열기 성공");
            //ocx 콜백 함수 등록
            khOpenApi_.OnReceiveTrData += new AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrDataEventHandler(axKHOpenAPI_OnReceiveTrData);
            khOpenApi_.OnReceiveRealData += new AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveRealDataEventHandler(axKHOpenAPI_OnReceiveRealData);
            khOpenApi_.OnReceiveMsg += new AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveMsgEventHandler(axKHOpenAPI_OnReceiveMsg);
            khOpenApi_.OnReceiveChejanData += new AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveChejanDataEventHandler(axKHOpenAPI_OnReceiveChejanData);
            khOpenApi_.OnEventConnect += new AxKHOpenAPILib._DKHOpenAPIEvents_OnEventConnectEventHandler(axKHOpenAPI_OnEventConnect);
            khOpenApi_.OnReceiveRealCondition += new AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveRealConditionEventHandler(axKHOpenAPI_OnReceiveRealCondition);
            khOpenApi_.OnReceiveTrCondition += new AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrConditionEventHandler(axKHOpenAPI_OnReceiveTrCondition);
            khOpenApi_.OnReceiveConditionVer += new AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveConditionVerEventHandler(axKHOpenAPI_OnReceiveConditionVer);

            return true;
        }

        public void quit()
        {
            if (connected()) {
                khOpenApi_.CommTerminate();
            }
            thread_.Join();
        }

        //---------------------------------------------------------------------
        // 주문등의 주식 관련 명령 실행
        static string accountNum_;
        public bool loadAccountInfo()
        {
            if (connected() == false) {
                return false;
            }
            happyTool_.stockDlg_.userId().Text = khOpenApi_.GetLoginInfo("USER_ID");
            //lbl이름.Text = khOpenApi_.GetLoginInfo("USER_NAME");

            string[] accountNumber = khOpenApi_.GetLoginInfo("ACCNO").Split(';');

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

        public static string accountNumber()
        {
            return accountNum_;
        }

        //---------------------------------------------------------------------
        // 주식 모듈의 콜벡 델리게이션
        public void axKHOpenAPI_OnEventConnect(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnEventConnectEvent apiEvent)
        {
            try {
                if (Error.IsError(apiEvent.nErrCode)) {
                    Logger.getInstance.print(Log.StockAPI콜백, "[로그인 처리결과] " + Error.GetErrorMessage());
                    this.loadAccountInfo();
                    happyTool_.buttonStart().Enabled = true;
                }
                else {
                    Logger.getInstance.print(Log.에러, "[로그인 처리결과] " + Error.GetErrorMessage());
                }
            } catch (AccessViolationException execption) {
                Logger.getInstance.print(Log.에러, "[로그인 콜백 에러]" + execption.Message);
            }
        }

        // 일반 주문
        public void axKHOpenAPI_OnReceiveTrData(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrDataEvent apiEvent)
        {
            Logger logger = Logger.getInstance;
            try {
                string keyStr = apiEvent.sScrNo;
                StockStatement statement = this.getStockStatement(keyStr);
                if (statement != null) {
                    statement.receive(sender, apiEvent);
                    //받은 시각 기록
                    statement.receivedTick_ = DateTime.Now.Ticks;
                }
                else {
                    ////주식 매매에 대한거면 key값은 그 주식 코드값임.
                    TradingStatement stat = this.getTradingStatement(keyStr);
                    if (stat != null) {
                        stat.receive(sender, apiEvent);
                    } else {
                        logger.print(Log.StockAPI콜백, "[이 주문표 데이터가 삭제되었음. {0}", keyStr);
                    }
                }
                removeScreenNum(keyStr);
            } catch (AccessViolationException execption) {
                logger.print(Log.에러, "[주문 콜백 에러] {0}\n{1}\n{2}", execption.Message, execption.StackTrace, execption.InnerException);
            }
        }

        public void axKHOpenAPI_OnReceiveMsg(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveMsgEvent apiEvent)
        {
            // 그냥 이 주문이 잘 실행되었습니다 하는 콜백...
            //화면번호:1204 | RQName:주식분봉차트조회요청 | TRCode:OPT10080 | 메세지:[00Z310] 모의투자 조회가 완료되었습니다
            //    Logger.getInstance.print(Log.StockAPI콜백, "화면번호:{0} | RQName:{1} | TRCode:{2} | 메세지:{3}", e.sScrNo, e.sRQName, e.sTrCode, e.sMsg);
        }

        // 체결 주문 관련
        public void axKHOpenAPI_OnReceiveChejanData(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveChejanDataEvent apiEvent)
        {
            Logger logger = Logger.getInstance;
            try {
                if (apiEvent.sGubun == "0") {
                    string stockName = khOpenApi_.GetChejanData(302);

                    // 주문 수량
                    string tradingAmountStr = khOpenApi_.GetChejanData(900);
                    int tradingAmount = 0;
                    if (int.TryParse(tradingAmountStr, out tradingAmount)) {
                        logger.print(Log.StockAPI콜백, "주문 양을 읽을 수 없음");
                        return;
                    }

                    // 체결 수량
                    string tradingCountStr = khOpenApi_.GetChejanData(911);
                    int tradingCount = 0;
                    if (int.TryParse(tradingCountStr, out tradingCount)) {
                        logger.print(Log.StockAPI콜백, "체결 양을 읽을 수 없음");
                        return;
                    }

                    // 주식 코드
                    string codeStr = khOpenApi_.GetChejanData(9001);
                    int code = 0;
                    if (Int32.TryParse(codeStr, out code)) {
                        logger.print(Log.StockAPI콜백, "주식 코드를 읽을 수 없음");
                        return;
                    }

                    TradingStatement tradingStat = this.popTradingStatement(code.ToString());
                    if (tradingStat != null) {
                        tradingStat.tradingCount_ -= tradingCount;

                        // 양이 아직 남아 있으면, tradingPool 에 다시 넣어준다.
                        if (tradingStat.tradingCount_ > 0) {
                            Logger.getInstance.print(Log.StockAPI콜백, "구분 : 주문체결통보");
                            Logger.getInstance.print(Log.StockAPI콜백, "주문/체결시간 : " + khOpenApi_.GetChejanData(908));
                            Logger.getInstance.print(Log.StockAPI콜백, "종목명 : " + stockName);
                            Logger.getInstance.print(Log.StockAPI콜백, "주문수량 : " + tradingAmountStr);
                            Logger.getInstance.print(Log.StockAPI콜백, "주문가격 : " + khOpenApi_.GetChejanData(901));
                            Logger.getInstance.print(Log.StockAPI콜백, "체결수량 : " + tradingCountStr);
                            Logger.getInstance.print(Log.StockAPI콜백, "체결가격 : " + khOpenApi_.GetChejanData(910));

                            this.addTradingReceive(tradingStat);
                        } else {
                            // 다 팔렸으면 account 재 갱신한다.
                            logger.print(Log.StockAPI콜백, "체결통보 {0}:{1} 주문수량/체결수량: {2}/{3}, 체결가격{4}", stockName, code, tradingAmountStr, tradingCountStr, khOpenApi_.GetChejanData(910));
                            StockBot.getInstance.reloadAccountInfo();
                        }
                    }
                }
                else if (apiEvent.sGubun == "1") {
                    logger.print(Log.StockAPI콜백, "구분 : 잔고통보");
                }
                else if (apiEvent.sGubun == "3") {
                    logger.print(Log.StockAPI콜백, "구분 : 특이신호");
                }
            }
            catch (AccessViolationException execption) {
                logger.print(Log.에러, "[채결 / 잔고 처리 콜백 에러] {0}\n{1}\n{2}", execption.Message, execption.StackTrace, execption.InnerException);
            }
}

        public void axKHOpenAPI_OnReceiveRealData(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveRealDataEvent apiEvent)
        {
            try {
                if (apiEvent.sRealType == "주식시세") {
                    Logger.getInstance.print(Log.StockAPI콜백, "종목코드 : {0} | 현재가 : {1:C} | 등락율 : {2} | 누적거래량 : {3:N0} ",
                            apiEvent.sRealKey,
                            Int32.Parse(khOpenApi_.GetCommRealData(apiEvent.sRealType, 10).Trim()),
                            khOpenApi_.GetCommRealData(apiEvent.sRealType, 12).Trim(),
                            Int32.Parse(khOpenApi_.GetCommRealData(apiEvent.sRealType, 13).Trim()));
                }
            }
            catch (AccessViolationException execption) {
                Logger.getInstance.print(Log.에러, "[주식 데이터 콜백 에러] {0}\n{1}\n{2}", execption.Message, execption.StackTrace, execption.InnerException);
            }
        }

        public void axKHOpenAPI_OnReceiveRealCondition(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveRealConditionEvent apiEvent)
        {
            try { 
            Logger.getInstance.print(Log.StockAPI콜백, "========= 조건조회 실시간 편입/이탈 ==========");
            Logger.getInstance.print(Log.StockAPI콜백, "[종목코드] : " + apiEvent.sTrCode);
            Logger.getInstance.print(Log.StockAPI콜백, "[실시간타입] : " + apiEvent.strType);
            Logger.getInstance.print(Log.StockAPI콜백, "[조건명] : " + apiEvent.strConditionName);
            Logger.getInstance.print(Log.StockAPI콜백, "[조건명 인덱스] : " + apiEvent.strConditionIndex);

                //// 자동주문 로직
                //if (e.strType == "I") {
                //    // 해당 종목 1주 시장가 주문
                //    // =================================================

                //    // 계좌번호 입력 여부 확인
                //    if (happyTool_.account().Text.Length != 10) {
                //        Logger.getInstance.print(Log.에러, "계좌번호 10자리를 입력해 주세요");

                //        return;
                //    }

                //    // =================================================
                //    // 주식주문
                //    int lRet;

                //    lRet = khOpenApi_.SendOrder("주식주문",
                //                                "10",//this.getScrNum(),
                //                                happyTool_.account().Text.Trim(),
                //                                1,      // 매매구분
                //                                e.sTrCode.Trim(),   // 종목코드
                //                                1,      // 주문수량
                //                                1,      // 주문가격 
                //                                "03",    // 거래구분 (시장가)
                //                                "0");    // 원주문 번호

                //    if (lRet == 0) {
                //        Logger.getInstance.print(Log.실시간, "주문이 전송 되었습니다");
                //    }
                //    else {
                //        Logger.getInstance.print(Log.에러, "주문이 전송 실패 하였습니다. [에러] : " + lRet);
                //    }
                //}
            }
            catch (AccessViolationException execption) {
                Logger.getInstance.print(Log.에러, "[실시간 조건 조회 콜백 에러] {0}\n{1}\n{2}", execption.Message, execption.StackTrace, execption.InnerException);
            }
        }

        public void axKHOpenAPI_OnReceiveTrCondition(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrConditionEvent apiEvent)
        {
            try { 
                Logger.getInstance.print(Log.StockAPI콜백, "[화면번호] : " + apiEvent.sScrNo);
                Logger.getInstance.print(Log.StockAPI콜백, "[종목리스트] : " + apiEvent.strCodeList);
                Logger.getInstance.print(Log.StockAPI콜백, "[조건명] : " + apiEvent.strConditionName);
                Logger.getInstance.print(Log.StockAPI콜백, "[조건명 인덱스 ] : " + apiEvent.nIndex.ToString());
                Logger.getInstance.print(Log.StockAPI콜백, "[연속조회] : " + apiEvent.nNext.ToString());
            }
            catch (AccessViolationException execption) {
                Logger.getInstance.print(Log.에러, "[컨디션 체크 콜백 에러] {0}\n{1}\n{2}", execption.Message, execption.StackTrace, execption.InnerException);
            }
        }

        public void axKHOpenAPI_OnReceiveConditionVer(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveConditionVerEvent apiEvent)
        {
            try {
                if (apiEvent.lRet == 1) {
                    Logger.getInstance.print(Log.StockAPI콜백, "[이벤트] 조건식 저장 성공");
                }
                else {
                    Logger.getInstance.print(Log.에러, "[이벤트] 조건식 저장 실패 : " + apiEvent.sMsg);
                }
            }
            catch (AccessViolationException execption) {
                Logger.getInstance.print(Log.에러, "[이벤트 콜백 에러] {0}\n{1}\n{2}", execption.Message, execption.StackTrace, execption.InnerException);
            }
        }
    }
}
