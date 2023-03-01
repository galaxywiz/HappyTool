using AxKFOpenAPILib;
using HappyFuture.DialogControl.FutureDlg;
using KiwoomCode;
using KiwoomEngine;
using StockLibrary;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using UtilLibrary;

namespace HappyFuture
{
    class FutureItem
    {
        public string code_;
        public string stk_code_;    //		12	종목코드
        public string arti_code_;   //		6	품목코드(6A, ES, ….)
        public string arti_hnm_;    //		40	품목명
        public string arti_tp_;     //		3	품목구분(IDX, CUR, ….)
        public string crnc_code_;   //	    3	결제통화코드(USD, JPY, …)
        public string tick_unit_;   //		15	TICK 단위
        public string tick_value_;  //	    15	TICK 가치
        public string deal_unit_;   //		15	거래단위
        public string deal_mtal_;   //		15	거래승수
        public string ntt_code_;    //		1	진법코드로서 1~9, A ~Z으로 표현되며, 실수형 또는 진법표현을 위한 구분값임.
        public string ntt_calc_unit_;   //	15	가격표시조정계수
        public string frgn_exch_code_;  //	10	해외거래소코드
        public string expr_dt_;     //		8	만기일자
        public string fprc_;        //		10	소숫점자리수
        public string gubun_;       //		1	최근월물구분
        public string atv_code_;    //		1	액티브월물구분

        public FutureItem(string code, string info)
        {
            if (info == string.Empty) {
                return;
            }
            this.code_ = code;
            int offset = 0;
            this.stk_code_ = info.Substring( offset, 12).Trim();
            offset += 12;
            this.arti_code_ = info.Substring(offset, 6).Trim();
            offset += 6;
            this.arti_hnm_ = info.Substring(offset, 40).Trim();
            offset += 40;
            this.arti_tp_ = info.Substring(offset, 3).Trim();
            offset += 3;

            this.crnc_code_ = info.Substring(offset, 3).Trim();
            offset += 3;
            this.tick_unit_ = info.Substring(offset, 15).Trim();
            offset += 15;
            this.tick_value_ = info.Substring(offset, 15).Trim();
            offset += 15;

            this.deal_unit_ = info.Substring(offset, 15).Trim();
            offset += 15;
            this.deal_mtal_ = info.Substring(offset, 15).Trim();
            offset += 15;

            this.ntt_code_ = info.Substring(offset, 1).Trim();
            offset += 1;
            this.ntt_calc_unit_ = info.Substring(offset, 15).Trim();
            offset += 15;

            this.frgn_exch_code_ = info.Substring(offset, 10).Trim();
            offset += 10;
            this.expr_dt_ = info.Substring(offset, 8).Trim();
            offset += 8;
            this.fprc_ = info.Substring(offset, 10).Trim();
            offset += 10;
            this.gubun_ = info.Substring(offset, 1).Trim();
            offset += 1;
            this.atv_code_ = info.Substring(offset, 1).Trim();
            offset += 1;
        }
    }

    class FutureEngine: Engine
    {
        protected AxKFOpenAPI openApi_ = null;
        readonly HappyFutureDlg happyFuture_ = null;

        public FutureEngine(HappyFutureDlg happTool)
        {
            this.happyFuture_ = happTool;
        }

        protected override int apiRequestDelay()
        {
            return 1000 / 3;
        }

        public void setup(Bot bot, AxKFOpenAPI api)
        {
            this.setup(bot);
            this.bot_ = bot;
            this.openApi_ = api;
            this.openApi_.CreateControl();
        }

        public AxKFOpenAPI openApi()
        {
            return this.openApi_;
        }

        public bool connected()
        {
            if (openApi_ == null) {
                return false;
            }
            int state = this.openApi_.GetConnectState();
            if (state == 0) {
                return false;
            }
            return true;
        }

        public bool openLoginDlg()
        {
            if (openApi_ == null) {
                return false;
            }
            if (this.openApi_.CommConnect(1) != 0) {
                return false;
            }
            return true;
        }

        public bool start()
        {
            if (this.connected() == true) {
                return true;
            }

            //ocx 콜백 함수 등록
            // 일반 시세, 계좌 조회 콜백
            this.openApi_.OnReceiveRealData += new _DKFOpenAPIEvents_OnReceiveRealDataEventHandler(this.AxKFOpenAPI_OnReceiveRealData);

            // 선물 주문 콜백
            this.openApi_.OnReceiveTrData += new _DKFOpenAPIEvents_OnReceiveTrDataEventHandler(this.AxKFOpenAPI_OnReceiveTrData);
            this.openApi_.OnReceiveMsg += new _DKFOpenAPIEvents_OnReceiveMsgEventHandler(this.AxKFOpenAPI_OnReceiveMsg);
            this.openApi_.OnReceiveChejanData += new _DKFOpenAPIEvents_OnReceiveChejanDataEventHandler(this.AxKFOpenAPI_OnReceiveChejanData);
            //
            this.openApi_.OnEventConnect += new _DKFOpenAPIEvents_OnEventConnectEventHandler(this.AxKFOpenAPI_OnEventConnect);

            if (this.openLoginDlg() == false) {
                Logger.getInstance.print(Log.에러, "로그인창 열기 실패");
                return false;
            }

            Logger.getInstance.print(Log.API조회, "로그인창 열기 성공");
            return true;
        }

        public void quit()
        {
            if (openApi_ == null) {
                return;
            }
            this.shutdown();

            if (this.connected()) {
                this.openApi_.CommTerminate();
            }
            openApi_ = null;
            this.thread_.Abort();
        }

        string getChejan(int code)
        {
            if (openApi_ == null) {
                return "";
            }
            return this.openApi_.GetChejanData(code).Trim();
        }

        public override string userId()
        {
            if (openApi_ == null) {
                return "";
            }
            return this.openApi_.GetLoginInfo("USER_ID").Trim();
        }

        public void setAccountNumber(string number)
        {
            accountNum_ = number;
        }

        public string accountNum()
        {
            return accountNum_;
        }

        //---------------------------------------------------------------------
        // 주문등의 주식 관련 명령 실행
        public override bool loadAccountInfo()
        {
            this.happyFuture_.futureDlg_.Label_id.Text = this.userId();
            //lbl이름.Text = khOpenApi_.GetLoginInfo("USER_NAME");

            string[] accountNumber = this.openApi_.GetLoginInfo("ACCNO").Split(';');
            var accountCombo = this.happyFuture_.comboBox_account;
            accountCombo.Items.Clear();

            foreach (string num in accountNumber) {
                if (num == "") {
                    continue;
                }

                // 계좌가 해외선물 계좌인지 확인
                var sub = num.Substring(num.Length - 2, 2);
                if (sub != "72") {
                    continue;
                }
                accountCombo.Items.Add(num.Trim());
            }
            if (this.isTestServer()) {
                accountCombo.SelectedIndex = 0;
            }
            else {
                accountCombo.SelectedIndex = PublicFutureVar.selectAccount_;
            }
            var account = accountCombo.SelectedItem.ToString();
            this.setAccountNumber(account);
            this.happyFuture_.futureDlg_.setAccountNumber(account);

            Logger.getInstance.print(Log.API결과, "계좌 번호 가지고 오기 성공, [{0}]의 비밀번호 입력하시오", accountNum_);
            Program.happyFuture_.loaded_ = true;
            return true;
        }

        public List<FutureItem> requestFutureList(List<string> watchingCodes)
        {
            List<FutureItem> retList = new List<FutureItem>();
            //string itemLists = this.openApi_.GetGlobalFutureItemlist();
            //string[] itemList = itemLists.Split(';');
            foreach (var item in watchingCodes) {
                if (item == string.Empty || item == null) {
                    continue;
                }
                string codeLists;
                try {
                    codeLists = this.openApi_.GetGlobalFutureCodelist(item);
                }
                catch (Exception e) {
                    string log = string.Format("[{0}]선물 코드 조회 에러[{1}], 거래소 시세 요금 신청이 안된듯\n아래 사이트에서 거래소 확인할것.\nhttps://fx.kiwoom.com/fxk.templateFrameSet.do", item, e.Message);
                    Logger.getInstance.print(Log.에러, log);
                    bot_.telegram_.sendMessage(log);
                    MessageBox.Show(log, "경고, 시세요금", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
                }

                string[] codeList = codeLists.Split(';');
                foreach (var code in codeList) {
                    if (code == string.Empty || code == null) {
                        continue;
                    }
                    string info = this.openApi_.GetGlobalFutOpCodeInfoByCode(code);
                    if (info == string.Empty || info == null) {
                        continue;
                    }
                    FutureItem fItem = new FutureItem(code, info);
                    if (fItem.frgn_exch_code_ != "CME") {
                        break;
                    }
                    // 활성화 된것만
                    if (fItem.atv_code_ == "1") {
                        retList.Add(fItem);
                        break;
                    }
                }
            }

            return retList;
        }

        public override void requestStockData(string code, PRICE_TYPE priceType, bool forceBuy = false)
        {
            FutureInfoStatement statement = null;

            switch (priceType) {
                case PRICE_TYPE.MIN_1:
                statement = new HistoryMin1Future(code);
                break;
                case PRICE_TYPE.MIN_2:
                statement = new HistoryMin2Future(code);
                break;
                case PRICE_TYPE.MIN_3:
                statement = new HistoryMin3Future(code);
                break;
                case PRICE_TYPE.ONE_MIN:
                statement = new OneMinFuture(code);
                break;
                case PRICE_TYPE.DAY:
                statement = new HistoryTheDaysFuture(code);
                break;
                case PRICE_TYPE.WEEK:
                statement = new HistoryTheWeeksFuture(code);
                break;
            }
            this.addOrder(statement);
        }

        public void requestStockBuyCount(FutureData futureData)
        {
            var code = futureData.code_;
            var nowPrice = futureData.nowPrice();

            this.addOrder(new FutureBuyAbleInfoStatement(code, TRADING_STATUS.매도, nowPrice));
            this.addOrder(new FutureBuyAbleInfoStatement(code, TRADING_STATUS.매수, nowPrice));
        }

        public void requestOutstandingOrder()
        {
            this.addOrder(new OutstandingOrderFutureStatement(TRADING_STATUS.매도));
            this.addOrder(new OutstandingOrderFutureStatement(TRADING_STATUS.매수));
        }

        public void requestFutureInfo(string code)
        {
            this.addOrder(new FutureInfoLoadStatement(code));
        }

        public override void addOrder(Statement statement)
        {
            base.addOrder(statement);
        }

        public bool isTestServer()
        {
            if (this.openApi_ == null) {
                return false;
            }

            var ret = this.openApi_.GetCommonFunc("GetServerGubunW", "");
            if (ret == "0") {
                // 0 리턴 실거래
                return false;
            }
            return true;
        }
        
        public void showAccountPw()
        {
            if (openApi_ == null) {
                return;
            }
            this.openApi_.GetCommonFunc("ShowAccountWindow", "");
        }

        void sendTelelgram(string log)
        {
            var bot = ControlGet.getInstance.futureBot();
            var telegram = bot.telegram_;
            if (telegram != null) {
                telegram.sendMessage(log);
            }
        }
        //---------------------------------------------------------------------
        // 주식 모듈의 콜벡 델리게이션
        public void AxKFOpenAPI_OnEventConnect(object sender, _DKFOpenAPIEvents_OnEventConnectEvent apiEvent)
        {
            Logger logger = Logger.getInstance;
            var bot = ControlGet.getInstance.futureBot();

            try {
                var loginRet = "[로그인 처리결과] " + Error.GetErrorMessage();
                if (Error.IsError(apiEvent.nErrCode)) {
                    if (bot.activate_) {
                        return;
                    }

                    logger.print(Log.StockAPI콜백, loginRet);
                    this.loadAccountInfo();

                    var button = this.happyFuture_.Button_Future;
                    button.Enabled = true;
                    button.Focus();

                    this.showAccountPw();
                    // 이것들이 암호를 매번 입력하게 해줘야 하네...
                    //happyFuture_.button_Future_Click(sender, null);
                }
                else {
                    if (apiEvent.nErrCode == KOAErrorCode.OP_ERR_DISCONNECT) {
                        loginRet = "Open API의 접속 이 끊어졌습니다.";
                        logger.print(Log.에러, loginRet);
                        this.sendTelelgram(loginRet);
                        this.openLoginDlg();
                    } else {
                        logger.print(Log.에러, loginRet);
                        this.sendTelelgram(loginRet);
                        Program.happyFuture_.quit();
                    }
                }
            }
            catch (AccessViolationException execption) {
                logger.print(Log.에러, "[로그인 콜백 에러]" + execption.Message);
            }
        }

        // OnReceiveTRData()이벤트 함수는 주문후 호출되며 주문번호를 얻을수 있습니다.
        // 만약 이 이벤트 함수에서 주문번호를 얻을수 없으면 해당 주문은 실패한 것입니다.
        public virtual void AxKFOpenAPI_OnReceiveTrData(object sender, _DKFOpenAPIEvents_OnReceiveTrDataEvent apiEvent)
        {
            Logger logger = Logger.getInstance;
            try {
                string keyStr = apiEvent.sScrNo;
                StockStatement statement = (StockStatement) this.getStockStatement(keyStr);
                if (statement != null) {
                    statement.receive(sender, apiEvent);
                    //받은 시각 기록
                    statement.receivedTick_ = DateTime.Now.Ticks;
                }
                else {
                    //이거 이외의 값은 선물 매매 관련임.
                    // 그냥 선물 데이터를 재 로딩 해주자
                    return;
                }
                if (statement.GetType() == typeof(SellFuture) || statement.GetType() == typeof(BuyFuture)) {
                    Logger.getInstance.print(Log.StockAPI콜백, "{0} 에 선물 주문 처리중", keyStr);
                    var order = (TradingStatement) statement;
                    FutureData futureData = (FutureData) this.bot_.getStockDataCode(order.code_);
                    if (futureData != null) {
                        futureData.regScreenNumber_ = keyStr;
                    }
                }
                else {
                    this.removeScreenNum(keyStr);
                }
            }
            catch (AccessViolationException execption) {
                string logRet = string.Format("[주문 콜백 에러] {0}\n{1}\n{2}", execption.Message, execption.StackTrace, execption.InnerException);
                logger.print(Log.에러, logRet);
                this.sendTelelgram(logRet);
            }
        }

        public void AxKFOpenAPI_OnReceiveMsg(object sender, _DKFOpenAPIEvents_OnReceiveMsgEvent apiEvent)
        {
            // 그냥 이 주문이 잘 실행되었습니다 하는 콜백...
            //string log = string.Format("화면번호:{0} | RQName:{1} | TRCode:{2} | 메세지:{3}", apiEvent.sScrNo, apiEvent.sRQName, apiEvent.sTrCode, apiEvent.sMsg);
            //Logger.getInstance.print(Log.StockAPI콜백, log);
        }

        int getChejanFieldCode(string name)
        {
            foreach (var keyValue in KOACode.futureFIDName_) {
                if (keyValue.Value == name) {
                    int code = int.Parse(keyValue.Key);
                    return code;
                }
            }
            return 0;
        }

        // OnReceiveChejan()이벤트 함수는 주문접수, 체결, 잔고발생시 호출되며
        // 이 이벤트 함수를 통해 대부분의 주문관련 정보를 얻을 수 있습니다.
        public virtual void AxKFOpenAPI_OnReceiveChejanData(object sender, _DKFOpenAPIEvents_OnReceiveChejanDataEvent apiEvent)
        {
            Logger logger = Logger.getInstance;
            try {
                string strGubun = apiEvent.sGubun, strFidlist = apiEvent.sFidList, strText;
                string strOneData, strFIDName;

                string[] strArrData = null;
                char[] cSplit = { ';' };
                strArrData = strFidlist.Split(cSplit);

                for (int i = 0; i < strArrData.Length - 1; i++) {
                    if (!KOACode.futureFIDName_.TryGetValue(strArrData[i], out strFIDName)) {
                        strFIDName = strArrData[i];
                    }

                    strOneData = this.getChejan(int.Parse(strArrData[i])).Trim();

                    strText = string.Format("구분[{0:s}] FID[{1:4s}:{2:s}] = [{3:s}]", strGubun, strArrData[i], strFIDName, strOneData);
                    logger.print(Log.StockAPI콜백, strText);
                }

                string code = this.getChejan(this.getChejanFieldCode("종목코드"));
                FutureData futureData = (FutureData) this.bot_.getStockDataCode(code);
                if (futureData != null) {
                    string orderNumber = this.getChejan(this.getChejanFieldCode("주문번호"));
                    futureData.orderNumber_ = orderNumber;
                }

                string remainCountStr = this.getChejan(this.getChejanFieldCode("미체결수량"));
                int remainCount = 0;
                if (int.TryParse(remainCountStr, out remainCount) == false) {
                    logger.print(Log.StockAPI콜백, "미체결수량 을 읽을 수 없음");
                }

                // 미체결 수량이 없으면
                if (remainCount == 0) {
                    this.sendChejanLog("체결 통보");
                    this.bot_.requestMyAccountInfo();
                }
            }
            catch (AccessViolationException execption) {
                string logRet = string.Format("[채결 / 잔고 처리 콜백 에러] {0}\n{1}\n{2}", execption.Message, execption.StackTrace, execption.InnerException);                
                logger.print(Log.에러, logRet);
                this.sendTelelgram(logRet);
            }
        }

        public void sendChejanLog(string title)
        {
            string code = this.getChejan(this.getChejanFieldCode("종목코드"));
            FutureData futureData = (FutureData) this.bot_.getStockDataCode(code);
            if (futureData == null) {
                Logger.getInstance.print(Log.에러, "{0} 코드의 종목이 체결 데이터로 왔는데 이건 모니터닝이 아님", code);
                return;
            }

            string log = string.Format("*** {0} ***\n", title);
            log += string.Format(" - 주문번호: {0}\n", futureData.orderNumber_);
            var dateStr = this.getChejan(this.getChejanFieldCode("주문/체결시간"));
            log += string.Format(" - 시간: {0}\n", dateStr);
            log += string.Format(" - 종목: {0}:{1}\n", futureData.name_, code);

            string tradingAmountStr = this.getChejan(this.getChejanFieldCode("주문수량"));
            int tradingAmount = 0;
            int.TryParse(tradingAmountStr, out tradingAmount);

            // 체결 수량
            string tradingCountStr = this.getChejan(this.getChejanFieldCode("체결수량"));
            int tradingCount = 0;
            int.TryParse(tradingCountStr, out tradingCount);
            log += string.Format(" - 체결 수량: {0}/{1}\n", tradingCountStr, tradingAmountStr);

            string tradePriceStr = this.getChejan(this.getChejanFieldCode("체결가격"));
            double tradePrice = 0;
            double.TryParse(tradePriceStr, out tradePrice);
            log += string.Format(" - 체결가 : {0}", tradePriceStr);

            this.removeScreenNum(futureData.regScreenNumber_);
            futureData.resetBuyInfo();

            var bot = this.bot_ as FutureBot;
            bot.requestStockBuyCount();
            //     this.bot_.telegram_.sendMessage(log);
            bot.updatePoolView();
            FutureDlgInfo.getInstance.updateBuyPoolView();

            if (tradingCount > 0) {
                // 현재 상황 갱신
                var dlgInfo = FutureDlgInfo.getInstance;
                dlgInfo.tradeRecoderView_.update();
                dlgInfo.tradeHistoryView_.update();
            }
        }

        double getDouble(string data)
        {
            double price = 0;
            data = data.Trim();
            if (double.TryParse(data, out price) == false) {
                return 0;
            }

            return Math.Abs(price);
        }
        Int64 getInt64(string data)
        {
            Int64 price = 0;
            data = data.Trim();
            if (Int64.TryParse(data, out price) == false) {
                return 0;
            }

            return Math.Abs(price);
        }

        public void AxKFOpenAPI_OnReceiveRealData(object sender, _DKFOpenAPIEvents_OnReceiveRealDataEvent apiEvent)
        {
            Logger logger = Logger.getInstance;
            try {
                if (this.bot_.nowStockMarketTime() == false) {
                    return;
                }
                string code = apiEvent.sJongmokCode;
                string realType = apiEvent.sRealType;
                if ((realType.Trim() == "해외선물시세") || (realType.Trim() == "해외옵션시세")) {
                    var futureData = bot_.getStockDataCode(code) as FutureData;
                    if (futureData == null) {
                        return;
                    }
                    if (futureData.priceDataCount() < PublicVar.priceTableCount) {
                        return;
                    }
                    //string dateTime = openApi_.GetCommRealData(code, 20 /*"체결시간n"*/);  08:00 분:초 같은 형식
                    string priceStr = openApi_.GetCommRealData(code, 10 /*"현재가n"*/);
                    double price = this.getDouble(priceStr);
                    if (price == 0) {
                        return;
                    }

                    string volStr = openApi_.GetCommRealData(code, 15 /*"체결량n"*/);
                    Int64 vol = this.getInt64(volStr);
                    if (vol == 0) {
                        return;
                    }
                    
                    futureData.setRealTimeData(bot_, price, vol);
                }
            }
            catch (AccessViolationException execption) {
                string logRet = string.Format("[주식 데이터 콜백 에러] {0}\n{1}\n{2}", execption.Message, execption.StackTrace, execption.InnerException);
                logger.print(Log.에러, logRet);
                this.sendTelelgram(logRet);
            }
        }
    }
}
