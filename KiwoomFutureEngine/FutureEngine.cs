using AxKFOpenAPILib;
using KiwoomCode;
using StockLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UtilLibrary;

namespace FutureEngine
{
    public class KiwoomFutureEngine
    {
        private Thread thread_ = null;
        protected Bot bot_ = null;

        const Int16 KH_LIMIT_ORDER = 200;       // 키움증권 제한된 키값 갯수
        // 
        private object orderPoolLock_ = null;
        private List<Statement> statmentOrderPool_ = new List<Statement>();       //주문을 쌓는 pool

        // 매매 이외의 주문은 모두 이쪽으로 처리 receive 처리함.
        private object receivePoolLock_ = null;
        private Dictionary<string, Statement> statmentReceivePool_ = new Dictionary<string, Statement>(); // 매매 이외의 주문 관리

        bool[] screenNumPool_ = new bool[KH_LIMIT_ORDER];

        
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
            // 분리하면 선물 모듈로의 send /recv 가 꼬일 수 있음.. orz (그래서 MFC가 실패..)
            while (this.runLoop()) {
                Thread.Sleep(1);

                this.processStockOrder();
                bot_.process();
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
        //선물 실제 주문 명령 실행 부분
        private void processStockOrder()
        {
            lock (orderPoolLock_) {
                ArrayList deleteOrder = new ArrayList();

                foreach (Statement statement in statmentOrderPool_) {
                    string screenNum = this.getScreenNum();
                    if (screenNum.Length < 1) {
                        continue;
                    }
                    // 키값이 성공적으로 가져 오면, 실행하고
                    statement.screenNo_ = screenNum;
                    statement.request();

                    //recive 풀에 넣기, buy, sell 같은 주문은 넣지 않는다.
                    if (statement.isTradingOrder() == false) {
                        this.addStatmentReceive(statement);
                    }
                    deleteOrder.Add(statement);

                    // 키움증권 제한 1초에 1번만 요청 가능 (나쁜놈들)
                    Thread.Sleep(1000 / 1);

                    if (!this.runLoop()) {
                        break;
                    }
                }

                foreach (Statement statement in deleteOrder) {
                    statmentOrderPool_.Remove(statement);
                }
            }
        }

        private void processOrderCleanup()
        {
            // 선물 명령을 내린뒤, receive를 여러번 받을 수 있음. 
            // 그래서 지연 삭제를 구현함.
            long now = DateTime.Now.Ticks;
            long turn = TimeSpan.TicksPerSecond * 60;
            lock (receivePoolLock_) {
                ArrayList deleteOrder = new ArrayList();

                foreach (KeyValuePair<string, Statement> data in statmentReceivePool_) {
                    Statement stat = data.Value;
                    //1분 0초 + 10초 < 1분 20초
                    if (stat.receivedTick_ + turn < now) {
                        deleteOrder.Add(data.Key);
                    }
                }

                foreach (Statement deleteKey in deleteOrder) {
                    statmentOrderPool_.Remove(deleteKey);
                }
            }
        }

        public void addOrder(Statement statement)
        {
            lock (orderPoolLock_) {
                statmentOrderPool_.Add(statement);
            }
        }

        private void addStatmentReceive(Statement statement)
        {
            lock (receivePoolLock_) {
                statmentReceivePool_[statement.screenNo_] = statement;
            }
        }

        private Statement getStockStatement(string key)
        {
            lock (receivePoolLock_) {
                foreach (KeyValuePair<string, Statement> data in statmentReceivePool_) {
                    if (data.Key == key) {
                        return data.Value;
                    }
                }
            }
            return null;
        }

        private Statement popStockStatement(string key)
        {
            lock (receivePoolLock_) {
                Statement statement = null;

                if (statmentReceivePool_.TryGetValue(key, out statement)) {
                    statmentReceivePool_.Remove(key);
                    return statement;
                }
            }
            return null;
        }

        public virtual void loadStockData(string code, PRICE_TYPE priceType)
        {
        }

        //---------------------------------------------------------------------
        //라이브러리 포팅 함수들
        public AxKFOpenAPILib.AxKFOpenAPI khOpenApi()
        {
            return openApi_;
        }

        public bool connected()
        {
            int state = openApi_.GetConnectState();
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

            if (openApi_.CommConnect(1) != 0) {
                Logger.getInstance.print(Log.에러, "로그인창 열기 실패");
                return false;
            }

            Logger.getInstance.print(Log.API조회, "로그인창 열기 성공");
            //ocx 콜백 함수 등록
            // 일반 시세, 계좌 조회 콜백
            openApi_.OnReceiveRealData += new AxKFOpenAPILib._DKFOpenAPIEvents_OnReceiveRealDataEventHandler(AxKFOpenAPI_OnReceiveRealData);

            // 선물 주문 콜백
            openApi_.OnReceiveTrData += new AxKFOpenAPILib._DKFOpenAPIEvents_OnReceiveTrDataEventHandler(AxKFOpenAPI_OnReceiveTrData);
            openApi_.OnReceiveMsg += new AxKFOpenAPILib._DKFOpenAPIEvents_OnReceiveMsgEventHandler(AxKFOpenAPI_OnReceiveMsg);
            openApi_.OnReceiveChejanData += new AxKFOpenAPILib._DKFOpenAPIEvents_OnReceiveChejanDataEventHandler(AxKFOpenAPI_OnReceiveChejanData);

            //
            openApi_.OnEventConnect += new AxKFOpenAPILib._DKFOpenAPIEvents_OnEventConnectEventHandler(AxKFOpenAPI_OnEventConnect);
            return true;
        }

        public void quit()
        {
            this.shutdown();

            if (connected()) {
                openApi_.CommTerminate();
            }

            thread_.Abort();
        }

        //---------------------------------------------------------------------
        // 주문등의 선물 관련 명령 실행
        protected static string accountNum_;
        public virtual bool loadAccountInfo()
        {
            if (connected() == false) {
                return false;
            }
            return true;
        }

        public static string accountNumber()
        {
            return accountNum_;
        }

        //---------------------------------------------------------------------
        // 선물 모듈의 콜벡 델리게이션
        public virtual void AxKFOpenAPI_OnEventConnect(object sender, AxKFOpenAPILib._DKFOpenAPIEvents_OnEventConnectEvent apiEvent)
        {
            Logger logger = Logger.getInstance;
            try {
                if (Error.IsError(apiEvent.nErrCode)) {
                    logger.print(Log.StockAPI콜백, "[로그인 처리결과] " + Error.GetErrorMessage());
                    this.loadAccountInfo();
                } else {
                    logger.print(Log.에러, "[로그인 처리결과] " + Error.GetErrorMessage());
                }
            } catch (AccessViolationException execption) {
                logger.print(Log.에러, "[로그인 콜백 에러]" + execption.Message);
            }
        }

        // OnReceiveTRData()이벤트 함수는 주문후 호출되며 주문번호를 얻을수 있습니다.
        // 만약 이 이벤트 함수에서 주문번호를 얻을수 없으면 해당 주문은 실패한 것입니다.
        public virtual void AxKFOpenAPI_OnReceiveTrData(object sender, AxKFOpenAPILib._DKFOpenAPIEvents_OnReceiveTrDataEvent apiEvent)
        {
            Logger logger = Logger.getInstance;
            try {
                string keyStr = apiEvent.sScrNo;
                Statement statement = this.getStockStatement(keyStr);
                if (statement != null) {
                    statement.receive(sender, apiEvent);
                    //받은 시각 기록
                    statement.receivedTick_ = DateTime.Now.Ticks;
                } else {
                    //이거 이외의 값은 선물 매매 관련임.
                    // 그냥 선물 데이터를 재 로딩 해주자
                    //     ControlGet.getInstance.stockBot().loadMyAccountInfo();
                }
                removeScreenNum(keyStr);
            } catch (AccessViolationException execption) {
                logger.print(Log.에러, "[주문 콜백 에러] {0}\n{1}\n{2}", execption.Message, execption.StackTrace, execption.InnerException);
            }
        }

        // OnReceiveMsg()이벤트 함수는 주문성공, 실패 메시지를 코드와 함께 전달하므로 상세한 내용을 파악할 수 있습니다.
        public virtual void AxKFOpenAPI_OnReceiveMsg(object sender, AxKFOpenAPILib._DKFOpenAPIEvents_OnReceiveMsgEvent apiEvent)
        {
            // 그냥 이 주문이 잘 실행되었습니다 하는 콜백...
            //화면번호:1204 | RQName:선물분봉차트조회요청 | TRCode:OPT10080 | 메세지:[00Z310] 모의투자 조회가 완료되었습니다
            string log = string.Format("화면번호:{0} | RQName:{1} | TRCode:{2} | 메세지:{3}", apiEvent.sScrNo, apiEvent.sRQName, apiEvent.sTrCode, apiEvent.sMsg);
            Logger.getInstance.print(Log.StockAPI콜백, log);
            //  StockDlgInfo.getInstance.addTradeLog(true, log);

            // 장 종료이거나 하면 돈을 다시 넣어줘야 함.
            if (apiEvent.sMsg.StartsWith("[00Z218]")) {
                string tradingMoney = openApi_.GetChejanData(KOAChejanCode.미채결수량);
                Int64 money = 0;
                Int64.TryParse(tradingMoney, out money);
                //  bot_.addAccountMoney(money);
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
        public virtual void AxKFOpenAPI_OnReceiveChejanData(object sender, AxKFOpenAPILib._DKFOpenAPIEvents_OnReceiveChejanDataEvent apiEvent)
        {
            Logger logger = Logger.getInstance;
            try {
                string stockName = openApi_.GetChejanData(KOAChejanCode.종목명);

                // 주문 수량
                string tradingAmountStr = openApi_.GetChejanData(KOAChejanCode.주문수량);
                int tradingAmount = 0;
                if (int.TryParse(tradingAmountStr, out tradingAmount)) {
                    logger.print(Log.StockAPI콜백, "주문 양을 읽을 수 없음");
                }

                // 체결 수량
                string tradingCountStr = openApi_.GetChejanData(KOAChejanCode.체결량);
                int tradingCount = 0;
                if (int.TryParse(tradingCountStr, out tradingCount)) {
                    logger.print(Log.StockAPI콜백, "체결 양을 읽을 수 없음");
                }
                if (apiEvent.sGubun == "0") {
                    // 주문이 체결 될때마다 옴
                    logger.print(Log.StockAPI콜백, "구분 : 주문체결통보");
                    logger.print(Log.StockAPI콜백, "주문/체결시간 : " + openApi_.GetChejanData(KOAChejanCode.주문_체결시간));
                    logger.print(Log.StockAPI콜백, "종목명 : " + stockName);
                    logger.print(Log.StockAPI콜백, "주문수량 : " + tradingAmountStr);
                    logger.print(Log.StockAPI콜백, "주문가격 : " + openApi_.GetChejanData(KOAChejanCode.주문가격));
                    logger.print(Log.StockAPI콜백, "체결수량 : " + tradingCountStr);
                    logger.print(Log.StockAPI콜백, "체결가격 : " + openApi_.GetChejanData(KOAChejanCode.체결가));
                    this.sendChejanLog("체결 통보");

                    // 체결수량과 주문양이 같으면 계좌 다시 로딩.
                    if (tradingAmount != 0 && (tradingAmount == tradingCount)) {
                        bot_.requestMyAccountInfo();
                    }
                } else if (apiEvent.sGubun == "1") {
                    // 주문 체결로 예수금 변동 사항이 있을때 마다 옴.
                    logger.print(Log.StockAPI콜백, "구분 : 잔고통보");
                    string moneyStr = openApi_.GetChejanData(KOAChejanCode.예수금);
                    Int64 money = 0;
                    if (Int64.TryParse(moneyStr, out money)) {
                        logger.print(Log.StockAPI콜백, "예수금을 읽을 수 없음");
                    }
                    string log = string.Format("잔고통보 {0}\n", openApi_.GetChejanData(KOAChejanCode.주문_체결시간));
                    log += string.Format(" - 종목명 : {0}\n", openApi_.GetChejanData(KOAChejanCode.종목명));
                    log += string.Format("예수금 : {0} 변경", moneyStr);
                    //  TelegramBot.getInstance.sendMessage(log);

                } else if (apiEvent.sGubun == "3") {
                    logger.print(Log.StockAPI콜백, "구분 : 특이신호");
                }
            } catch (AccessViolationException execption) {
                logger.print(Log.에러, "[채결 / 잔고 처리 콜백 에러] {0}\n{1}\n{2}", execption.Message, execption.StackTrace, execption.InnerException);
            }
        }

        public virtual string sendChejanLog(string title)
        {
            string log = string.Format("*** {0} ***\n", title);
            log += string.Format(" - 매매 구분: {0} / {1}\n", openApi_.GetChejanData(KOAChejanCode.매수_매도구분), openApi_.GetChejanData(KOAChejanCode.매매구분));
            log += string.Format(" - 시간: {0}\n", openApi_.GetChejanData(KOAChejanCode.주문_체결시간));
            log += string.Format(" - 종목: {0}:{1}\n", openApi_.GetChejanData(KOAChejanCode.종목명), openApi_.GetChejanData(KOAChejanCode.종목코드));

            string tradingAmountStr = openApi_.GetChejanData(KOAChejanCode.주문수량);
            int tradingAmount = 0;
            int.TryParse(tradingAmountStr, out tradingAmount);

            // 체결 수량
            string tradingCountStr = openApi_.GetChejanData(KOAChejanCode.체결량);
            int tradingCount = 0;
            int.TryParse(tradingCountStr, out tradingCount);
            log += string.Format(" - 체결 수량: {0}/{1}\n", tradingCountStr, tradingAmountStr);
            log += string.Format(" - 체결가 : {0}", openApi_.GetChejanData(KOAChejanCode.체결가));

            return log;
        }

        // 조회요청이 성공하면 관련 실시간 데이터를 서버에서 자동으로 OnReceiveRealData()이벤트 함수로 전달해줍니다.
        public virtual void AxKFOpenAPI_OnReceiveRealData(object sender, AxKFOpenAPILib._DKFOpenAPIEvents_OnReceiveRealDataEvent apiEvent)
        {
            Logger logger = Logger.getInstance;
            try {
                if (apiEvent.sRealType == "선물시세") {
                    string code = apiEvent.sJongmokCode;
                    var stockData = bot_.getStockDataCode(code);
                    if (stockData == null) {
                        return;
                    }

                    double price = double.Parse(openApi_.GetCommRealData(apiEvent.sRealType, 10).Trim());     // 현제가
                    double start = double.Parse(openApi_.GetCommRealData(apiEvent.sRealType, 16).Trim());     // 시가
                    double high = double.Parse(openApi_.GetCommRealData(apiEvent.sRealType, 17).Trim());      // 고가
                    double low = double.Parse(openApi_.GetCommRealData(apiEvent.sRealType, 18).Trim());       // 저가
                    int vol = int.Parse(openApi_.GetCommRealData(apiEvent.sRealType, 13).Trim());             // 거래량
                    if (stockData.setRealTimeData(bot_, bot_.priceType_, Math.Abs(price))) {
                        this.updateStockDataView(stockData);
                    }
                }
            } catch (AccessViolationException execption) {
                logger.print(Log.에러, "[선물 데이터 콜백 에러] {0}\n{1}\n{2}", execption.Message, execption.StackTrace, execption.InnerException);
            }
        }
        protected virtual void updateStockDataView(StockData stockData)
        {
        }
    }
}
