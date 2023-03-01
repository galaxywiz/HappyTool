using System;
using System.Data;
using System.Globalization;
using System.Windows.Forms;
using TicTacTec.TA.Library;
using UtilLibrary;
using static StockLibrary.StrategyModuleList;

namespace StockLibrary
{
    // 공용 상수 주식 변수 셋팅용도... 주식과 선물 분리를 해야 할거 같아.
    public class PublicVar
    {
        protected static ExcelParser configParser_;
        protected static string configFileName_ = "Config.xlsx";
        protected static string configTableName_ = "Config";
        protected static string tradeModuleTableName_ = "StrategyModule";
        //---------------------------------------------------------------------------//
        // 인디게이터 조합
        public static int[] avgMax = { 3, 5, 10, 20, 50, 100, 200 };  //평균 기준
                                                                      //      public static int[] rainbowMax = { 50, 60, 70, 80, 90, 100, 110, 120, 130, 140 };    //https://m.blog.naver.com/tankun25/221277280101
        public static int[] rainbowMax = { 50, 70, 90, 110, 130, 150, 170, 190, 210, 230 };    //https://m.blog.naver.com/tankun25/221277280101
        //public static int[] avgMax = { 5, 10, 20, 60, 120};  //평균 기준
        public static int[] macdDay = { 12, 26, 9 }; //중기 투자
        //public static int[] macdDay = { 5, 20, 3 };	//단기 투자
        public static PRICE_TYPE initType = PRICE_TYPE.MIN_1;

        public static int envelopeTerm = 25;
        public static double envelopePercent = 10;
        public static double envelopeMin = 2.0f;
        public static double envelopeMax = 25.0f;
        public static int priceChannel = 20;

        public static int bollingerTerm = 20;
        public static int bollingerAnalysisTerm = 30;
        public static double bollingerConst = 2.0f;
        public static double bollingerBandGrowUp = 1.3f;
        public static double bollingerBandGrowDown = 0.7f;

        public static int stochN = 12;
        public static int stochK = 5;
        public static int stochD = 5;

        public static int rsiTerm = 14;
        public static int adxTerm = 14;
        public static int williams = 14;
        public static int cciTerm = 9;
        public static int atrTerm = 14;
        public static int rocTerm = 12;

        public static int highlowsTerm = 14;

        public static int ultimate7 = 7;
        public static int ultimate14 = 14;
        public static int ultimate28 = 28;

        public static int bullbearTerm = 14;
        //---------------------------------------------------------------------------//
        public static string monitorIp_ = "192.168.1.20";
        public static int monitorPort_ = 10900;
        public static string aiIp_ = "192.168.1.20";
        public static int aiPort_ = 30000;

        public static string passwordKey = "";
        public static string smtpGMail = "happytool84@gmail.com";
        public static string smtpPw = "Ryzen!Q2w";
        public static string notifyMail = "galaxy_wiz@naver.com";  // notify할 메일
        public static string dbIp = "";
        public static string dbUser = "";
        public static string dbPw = "";
        public static string accountPW = "0000";            // 선물은 패스워드가 저장이 안된다;;
        public static DateTime tradeUpdateTime = new DateTime(2018, 8, 10, 16, 0, 0);
        public static bool lostBanUse = true;
        public static int buyCount = 3;                     // 몇개까지 갖고 있을지
        public static int haveTimeHour = 5;                 // 몇시간을 기본으로 들고 있을지

        public static int defaultCalcCandleCount = 20;      // 백테스팅 할때 계산이 안됬을 값들 빼고 처리해야함. (이걸 늘린다고 해서 승율이 오르진 않음)
        public static int priceType_min1 = 5;               // 분봉 타입 
        public static int priceType_min2 = 30;
        public static int priceType_min3 = 60;
        public static int priceTableCount = 500;

        public static int stockTradeMoney = 1000000;        // 1종목당 100만원어치 단타 할꺼임.
        public static double tradeTax = 0.0033;                 // 주식의 경우 수수료, 거래세등 하면 0.33%를 빼야 함

        public static int stockPoolCountMax = 50;                   // 최대 감시할 주식 숫자
        public static int tradeModuleUpdateMins = 30;
        public static int tradeModuleCount = 100;

        public static double profitRate = 0.02f;            //이익 초과값
        public static int targetStockProfit = 15000;
        public static double loseCutRate = -0.05f;          // 주식당 손절매율

        public static string telegramStockBotAPI = "511370550:AAGdKo6WPjecx2pPVSSgdxdT22bYVHpXTJE";
        public static long telegramStockBotCHATID = 508897948;

        public static string fundPoolKey = "GenFundManagement_{0}";
        public static int 캔들완성_IDX = 1;               // 캔들이 제대로 확정된 것으로 처리 (0으로 할시, price 캔들은 시작으로부터 20초 되었을때 가격임)

        public static string yesterDayFilterDB = "D:\\Work\\StockPrice_Crawler\\DB\\HappyTool_StockList.db";
        //---------------------------------------------------------------------------//
        // 테스트 이 밑은 지워야 하나..
        public static int backTestInitMoney = 1000000;
        public static int simulateInitMoney = 1000000 * 10;
        public static int simulateBuyCount = 3;

        public static bool reverseOrder = false;        // 계속 - 가 나갈땐 이걸 키는걸 고려해 보자.

        public static FINDER_MODE finderMode = FINDER_MODE.보통;

        public static int fundUpdateHours = 12;        // 매매 전략 업데이트 주기
        public static bool eachFundManagement = true;
        public static int quickPoolCount = 20;

        public static int allowTradeCount = 2;  // 2회 이상 되어야 좀 매매 반응이 옴
        public static double allowTradeWinRate = 0.65; // 50% 승율은 되어야... 믿을만 하지

        // 실제 거래할 주식 가격폭
        public static ulong limitVolumes = 10000;                    // 매매를 위한 최소 거래량
        public static int limitLowStockPrice = 2000;
        public static int limitHighStockPrice = 500000;

        /* ==================================================================
      * 기존 매매 기법에 대한 고찰
      * 기존에 (2018.11 ~ 2019.09) 매매는 매매 모듈 조합을 백테스팅 해서
      * 단기간에 가장 괜찮은걸 기반으로 통계를 잡아, 다시 같은 신호가 나오면
      * 진입 하는 전략이었음.
      * 
      * 그런데 추세를 따르는건 아니고 그때 그때 운에 따르는것이라, 손해를 봄.
      * 
      * 이번에 엔진을 바꿔서 
      * 추세를 따라가되, 매매 모듈 신호가 그 방향과 맞으면 진입.
      * 추세가 역전되거나, 매매 모듈 신호가 반대거나, 익절 타임이 오면 스톱 하도록 로직 수정 해보자
      */
        public enum 매매_전략
        {
            백테스트기반,
            추세돌파,
        }

        public static 매매_전략 fundManageStrategy = 매매_전략.백테스트기반;

        public static string fundManageName = "베스트01";       // safeBackTestRef 사용 여부

        public static int fundTestDays = -14;     // 백테스팅 과거 데이터 분량치
        public static string testLog = "비고";
        public static bool immediatelyOrder = false;    // 즉시 주문
        public static double expectTimeMax = 1.5f;     // profitLost 에서 ExpectMax 의 배율값
        public static double expectTimeMin = 1.0f;     // profitLost 에서 ExpectMin 의 배율값
        public static int buyCountTime = 3;
        public static bool allInOneStrategy = false;
        public static bool allTradeModuleUse = false;
        static PublicVar()
        {
            PublicVar.readConfig();
            PublicFutureVar.readConfig2();
        }

        protected static bool convertBool(string value)
        {
            value = value.ToLower().Trim();
            return (value.StartsWith("true") || value.StartsWith("1")) ? true : false;
        }

        public static void readConfig()
        {
            configParser_ = new ExcelParser(Application.StartupPath + "\\" + configFileName_);
            configParser_.read();
            DataTable dt = configParser_.table().Tables[configTableName_];

            var fmt = new NumberFormatInfo();
            fmt.NegativeSign = "-";

            try {
                foreach (DataRow row in dt.Rows) {
                    string value = row[1].ToString().Trim();
                    switch (row[0].ToString()) {
                        case "암호":
                            passwordKey = value;
                            break;
                        case "행복의모니터IP":
                            monitorIp_ = value;
                            break;
                        case "행복의AI":
                            aiIp_ = value;
                            break;
                        // ------ 매매 모듈 --------//
                        case "매매모듈찾기모드":
                            switch (value) {
                                case "쾌속":
                                    finderMode = FINDER_MODE.쾌속;
                                    break;
                                case "보통":
                                    finderMode = FINDER_MODE.보통;
                                    break;
                                case "느리게":
                                    finderMode = FINDER_MODE.느리게;
                                    break;
                            }
                            break;
                        case "매매전략갱신시간":
                            fundUpdateHours = int.Parse(value);
                            break;
                        case "매매모듈개수":
                            tradeModuleCount = int.Parse(value);
                            break;
                        case "허용매매수":
                            allowTradeCount = int.Parse(value, fmt);
                            break;
                        case "허용매매승율":
                            allowTradeWinRate = double.Parse(value, fmt);
                            break;
                        case "주식리스트선별":
                            yesterDayFilterDB = value;
                            break;
                        // ------- 매매 전략 --------//
                        case "역주문":
                            reverseOrder = convertBool(value);
                            break;   // 이건 정말 최후의 수단
                        case "손해시4시간거래금지":
                            lostBanUse = convertBool(value);
                            break;
                        case "각항목전략":
                            eachFundManagement = convertBool(value);
                            break;

                        case "기본전략":
                            fundManageName = value;
                            break;

                        case "매매전략":
                            if (value.StartsWith("백테스트")) {
                                fundManageStrategy = 매매_전략.백테스트기반;
                                break;
                            }
                            if (value.StartsWith("추세돌파")) {
                                fundManageStrategy = 매매_전략.추세돌파;
                                break;
                            }
                            break;
                        case "올인원매매":
                            allInOneStrategy = convertBool(value);
                            break;
                        case "모든매매모듈사용":
                            allTradeModuleUse = convertBool(value);
                            break;
                        // default
                        case "smtpGMail":
                            smtpGMail = value;
                            break;
                        case "smtpPw":
                            smtpPw = value;
                            break;
                        case "notifyMail":
                            notifyMail = value;
                            break;
                        case "dbIp":
                            dbIp = value;
                            break;
                        case "dbUser":
                            dbUser = value;
                            break;
                        case "dbPw":
                            dbPw = value;
                            break;
                        case "최대주식보유수":
                            buyCount = int.Parse(value);
                            break;
                        case "종목당_목표값":
                            targetStockProfit = int.Parse(value);
                            break;
                        case "익절율":
                            profitRate = double.Parse(value, fmt);
                            break;
                        case "손절율":
                            loseCutRate = double.Parse(value, fmt);
                            if (loseCutRate > 0) {
                                loseCutRate = -loseCutRate;
                            }
                            break;
                        case "초기가격타입": {
                            switch (value) {
                                case "min1":
                                    initType = PRICE_TYPE.MIN_1;
                                    break;
                                case "min2":
                                    initType = PRICE_TYPE.MIN_2;
                                    break;
                                case "min3":
                                    initType = PRICE_TYPE.MIN_3;
                                    break;
                                case "1day":
                                    initType = PRICE_TYPE.DAY;
                                    break;
                                case "1week":
                                    initType = PRICE_TYPE.WEEK;
                                    break;
                            }
                        }
                        break;
                        case "분봉":
                            priceType_min1 = int.Parse(row[1].ToString());
                            priceType_min2 = int.Parse(row[2].ToString());
                            priceType_min3 = int.Parse(row[3].ToString());
                            break;
                        case "최대감시":
                            stockPoolCountMax = int.Parse(value);
                            break;
                        case "종목당거래액":
                            stockTradeMoney = int.Parse(value);
                            break;
                        case "거래량설정":
                            limitVolumes = ulong.Parse(value);
                            break;
                        case "테스트로그":
                            testLog = value;
                            break;
                        case "즉시매수":
                            immediatelyOrder = convertBool(value);
                            break;
                        case "기대최대배율값":
                            expectTimeMax = double.Parse(value);
                            break;
                        case "기대최소배율값":
                            expectTimeMin = double.Parse(value);
                            break;
                        case "거래주가최소값":
                            limitLowStockPrice = int.Parse(value);
                            break;
                        case "거래주가최대값":
                            limitHighStockPrice = int.Parse(value);
                            break;
                        case "연승최대배율":
                            buyCountTime = int.Parse(value);
                            break;
                        //--------------------------------------------------------------//
                        // 기술지표 계산 시드값
                        case "Ma":
                            for (int i = 0; i < 6; ++i) {
                                avgMax[i] = int.Parse(row[i + 1].ToString(), fmt);
                            }
                            break;
                        case "macd":
                            for (int i = 0; i < 3; ++i) {
                                macdDay[i] = int.Parse(row[i + 1].ToString(), fmt);
                            }
                            break;
                        case "envelopeTerm":
                            envelopeTerm = int.Parse(value, fmt);
                            break;
                        case "envelopePercent":
                            envelopePercent = double.Parse(value, fmt);
                            break;
                        case "bollingerTerm":
                            bollingerTerm = int.Parse(value, fmt);
                            break;
                        case "bollingerConst":
                            bollingerConst = double.Parse(value, fmt);
                            break;
                        case "stochN,K,D":
                            stochN = int.Parse(row[1].ToString(), fmt);
                            stochK = int.Parse(row[2].ToString(), fmt);
                            stochD = int.Parse(row[3].ToString(), fmt);
                            break;
                        case "rsiTerm":
                            rsiTerm = int.Parse(value, fmt);
                            break;
                        case "adxTerm":
                            adxTerm = int.Parse(value, fmt);
                            break;
                        case "williams":
                            williams = int.Parse(value, fmt);
                            break;
                        case "cciTerm":
                            cciTerm = int.Parse(value, fmt);
                            break;
                        case "atrTerm":
                            atrTerm = int.Parse(value, fmt);
                            break;
                        case "rocTerm":
                            rocTerm = int.Parse(value, fmt);
                            break;
                        case "highlowsTerm":
                            highlowsTerm = int.Parse(value, fmt);
                            break;
                        case "ultimate":
                            ultimate7 = int.Parse(row[1].ToString(), fmt);
                            ultimate14 = int.Parse(row[2].ToString(), fmt);
                            ultimate28 = int.Parse(row[3].ToString(), fmt);
                            break;
                        case "bullbearTerm":
                            bullbearTerm = int.Parse(value, fmt);
                            break;
                    }
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
                Console.Write(e.StackTrace);
            }
        }
    }

    public enum 매매기한
    {
        무제한,
        일간,
        주간,
    }
    public class PublicFutureVar: PublicVar
    {
        public static string telegramBotAPI = "644574439:AAGM0lU55I2SKdyZHbVt5x2JNsI3U0_zQpk";
        public static long telegramCHATID = 508897948;

        public static string currencyFmt = "###,###0,#####";
        public static double dataReceiveFailRate = 0.1;

        public static 매매기한 tradePeriod = 매매기한.무제한;   // 매매기한
        //public static double targetProfit = 1000;
        //public static double lostCutProfit = -300;
        public static double targetProfitTime = 10;
        public static double lostCutTime = -10;
        public static double myShareRate = 0.0f;
        public static double safeTargetMoney = 500.0f;
        public static double safeProfitTodayTargetRate = 0.05f;    // 5% 만 올라도 2주면 2배가 됨. 욕심부리지 말자구
        public static double safeLostTodayTargetRate = -0.1f;    // 10% 이하 손해면 그날은 그냥 접자.
        public static bool safeTrade = true;

        public static double feeTax = 100;   // 수수료 왕복 8+8 달러 + 기타 해서 30이긴 한데, 나도 먹을거 생각한 금액
        public static double pureFeeTex = 30;   // 실제 1개약당 수수료 30달러 듬.
        public static int ignoreBandTick = 20;  // 사고 20 틱 내외는 기다려 본다

        public static int accountTime = 10000;   // 이걸 기준으로 몇개를 살지 결정
        public static int 키움증권_가격표갯수 = 600;
        public static int selectAccount_ = 0;       // 몇번째 계좌 사용

        public static int maxBuyCount = 5;

        // https://stock79.tistory.com/248 이쪽 사이트 참고.
        public static double allowTrustMarginTime = 0.5;
        static PublicFutureVar()
        {
            PublicFutureVar.readConfig2();
            PublicVar.readConfig();
        }

        public enum 해선_종목
        {
            ONLY_CME, //CME 종목만
            ONLY_COMEX, //COMEX만
            ONLY_NYMEX, //NYMEX만

            일반, //18년 11월 최대 수익 냈을때 종목들
        }
        public static 해선_종목 해선_종목_확장 = 해선_종목.ONLY_CME;           // 영향이 있을지도 확장하고 수익율이 -가 됨

        public static void readConfig2()
        {
            configParser_ = new ExcelParser(Application.StartupPath + "\\" + configFileName_);
            configParser_.read();
            DataTable dt = configParser_.table().Tables[configTableName_];

            var fmt = new NumberFormatInfo();
            fmt.NegativeSign = "-";

            foreach (DataRow row in dt.Rows) {
                string value = row[1].ToString();
                switch (row[0].ToString()) {
                    case "텔레그램API":
                        telegramBotAPI = value;
                        break;
                    case "텔레그램ID":
                        telegramCHATID = long.Parse(value);
                        break;
                    case "거래소":
                        switch (value) {
                            case "CME":
                                해선_종목_확장 = 해선_종목.ONLY_CME;
                                break;
                            case "COMEX":
                                해선_종목_확장 = 해선_종목.ONLY_COMEX;
                                break;
                            case "NYMEX":
                                해선_종목_확장 = 해선_종목.ONLY_NYMEX;
                                break;
                            default:
                                해선_종목_확장 = 해선_종목.일반;
                                break;
                        }
                        break;
                    case "일일목표달성":
                        safeTrade = convertBool(value);
                        break;
                    case "이익당_내몫":
                        myShareRate = double.Parse(value);
                        break;
                    case "매매기한":
                        switch (value) {
                            case "무제한":
                                tradePeriod = 매매기한.무제한;
                                break;
                            case "일간":
                                tradePeriod = 매매기한.일간;
                                break;
                            case "주간":
                                tradePeriod = 매매기한.주간;
                                break;
                        }
                        break;
                    case "기본계좌배율":
                        accountTime = int.Parse(value);
                        break;
                    case "금일목표금액":
                        safeTargetMoney = double.Parse(value);
                        if (safeTargetMoney < 0) {
                            safeTargetMoney *= -1;
                        }
                        break;
                    case "금일목표이익율":
                        safeProfitTodayTargetRate = double.Parse(value);
                        if (safeProfitTodayTargetRate < 0) {
                            safeProfitTodayTargetRate *= -1;
                        }
                        break;
                    case "금일손절이익율":
                        safeLostTodayTargetRate = double.Parse(value);
                        if (safeLostTodayTargetRate > 0) {
                            safeLostTodayTargetRate *= -1;
                        }
                        break;

                    case "강제손익배율":
                        targetProfitTime = double.Parse(value, fmt);
                        break;
                    case "강제로스컷배율":
                        lostCutTime = double.Parse(value, fmt);
                        if (lostCutTime > 0) {
                            lostCutTime = -lostCutTime;
                        }
                        break;
                    case "수수료":
                        feeTax = double.Parse(value, fmt);
                        break;
                    case "실제수수료":
                        pureFeeTex = double.Parse(value, fmt);
                        break;
                    case "무시변동틱":
                        ignoreBandTick = int.Parse(value, fmt);
                        break;

                    case "허용증거금비율":
                        allowTrustMarginTime = double.Parse(value, fmt);
                        break;
                    case "사용할계좌순번":
                        selectAccount_ = int.Parse(value);
                        break;
                    case "최대매매갯수":
                        maxBuyCount = int.Parse(value);
                        break;
                }
            }
        }
    }
}