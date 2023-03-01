using System.Collections.Generic;

namespace KiwoomCode
{
    public enum Log
    {
        API조회,      // 조회창 출력
        API결과,
        StockAPI콜백, // StockAPI콜백 출력
        주식봇,
        백테스팅,
        백테스팅_csv,
        에러,         // 에러창 출력
    }

    public class KOAErrorCode
    {
        public const int OP_ERR_NONE = 0;     //"정상처리"
        public const int OP_ERR_LOGIN = -100;  //"사용자정보교환에 실패하였습니다. 잠시후 다시 시작하여 주십시오."
        public const int OP_ERR_CONNECT = -101;  //"서버 접속 실패"
        public const int OP_ERR_VERSION = -102;  //"버전처리가 실패하였습니다.
        public const int OP_ERR_DISCONNECT = -106;  // 연결이 끊김
        public const int OP_ERR_SISE_OVERFLOW = -200;  //”시세조회 과부하”
        public const int OP_ERR_RQ_STRUCT_FAIL = -201;  //”REQUEST_INPUT_st Failed”
        public const int OP_ERR_RQ_STRING_FAIL = -202;  //”요청 전문 작성 실패”
        public const int OP_ERR_ORD_WRONG_INPUT = -300;  //”주문 입력값 오류”
        public const int OP_ERR_ORD_WRONG_ACCNO = -301;  //”계좌비밀번호를 입력하십시오.”
        public const int OP_ERR_OTHER_ACC_USE = -302;  //”타인계좌는 사용할 수 없습니다.
        public const int OP_ERR_MIS_2BILL_EXC = -303;  //”주문가격이 20억원을 초과합니다.”
        public const int OP_ERR_MIS_5BILL_EXC = -304;  //”주문가격은 50억원을 초과할 수 없습니다.”
        public const int OP_ERR_MIS_1PER_EXC = -305;  //”주문수량이 총발행주수의 1%를 초과합니다.”
        public const int OP_ERR_MID_3PER_EXC = -306;  //”주문수량은 총발행주수의 3%를 초과할 수 없습니다.”
    }

    public class KOAChejanCode
    {
        public const int 주문번호 = 9203;
        public const int 종목코드 = 9201;
        public const int 주문상태 = 913;
        public const int 종목명 = 302;
        public const int 주문수량 = 900;
        public const int 주문가격 = 901;
        public const int 미채결수량 = 902;
        public const int 체결누계금액 = 903;
        public const int 원주문번호 = 904;
        public const int 주문구분 = 905;
        public const int 매매구분 = 906;
        public const int 매도수구분 = 907;
        public const int 주문_체결시간 = 908;
        public const int 체결번호 = 909;
        public const int 체결가 = 910;
        public const int 체결량 = 911;
        public const int 단위체결가 = 914;
        public const int 단위체결량 = 915;
        public const int 거부사유 = 919;
        public const int 보유수량 = 930;
        public const int 매입단가 = 931;
        public const int 총매입가 = 932;
        public const int 주문가능수량 = 933;
        public const int 당일순매수수량 = 945;
        public const int 매수_매도구분 = 946;
        public const int 당일총매도손일 = 950;
        public const int 예수금 = 951;
        public const int 손익율 = 8019;
    }

    public class KOACode
    {

        /// <summary>
        /// 주문코드 클래스
        /// </summary>
        public struct OrderType
        {
            private readonly string Name;
            private readonly int Code;

            public OrderType(int nCode, string strName)
            {
                this.Name = strName;
                this.Code = nCode;
            }

            public string name
            {
                get {
                    return this.Name;
                }
            }

            public int code
            {
                get {
                    return this.Code;
                }
            }
        }

        public readonly static OrderType[] orderType = new OrderType[6];
        public readonly static OrderType[] futureOrderType = new OrderType[6];

        /// <summary>
        /// 호가구분 클래스
        /// </summary>
        public struct HogaGb
        {
            private readonly string Name;
            private readonly string Code;

            public HogaGb(string strCode, string strName)
            {
                this.Code = strCode;
                this.Name = strName;
            }

            public string name
            {
                get {
                    return this.Name;
                }
            }

            public string code
            {
                get {
                    return this.Code;
                }
            }
        }

        public readonly static HogaGb[] hogaGb = new HogaGb[13];
        public readonly static HogaGb[] futureHoga = new HogaGb[4];
        public struct MarketCode
        {
            private readonly string Name;
            private readonly string Code;

            public MarketCode(string strCode, string strName)
            {
                this.Code = strCode;
                this.Name = strName;
            }

            public string name
            {
                get {
                    return this.Name;
                }
            }

            public string code
            {
                get {
                    return this.Code;
                }
            }
        }

        public readonly static MarketCode[] marketCode = new MarketCode[9];

        static KOACode()
        {
            // 주문타입 설정
            orderType[0] = new OrderType(1, "신규매수");
            orderType[1] = new OrderType(2, "신규매도");
            orderType[2] = new OrderType(3, "매수취소");
            orderType[3] = new OrderType(4, "매도취소");
            orderType[4] = new OrderType(5, "매수정정");
            orderType[5] = new OrderType(6, "매도정정");

            // 주문타입 설정
            futureOrderType[0] = new OrderType(1, "신규매도");
            futureOrderType[1] = new OrderType(2, "신규매수");
            futureOrderType[2] = new OrderType(3, "매도취소");
            futureOrderType[3] = new OrderType(4, "매수취소");
            futureOrderType[4] = new OrderType(5, "매도정정");
            futureOrderType[5] = new OrderType(6, "매수정정");

            // 호가타입 설정
            hogaGb[0] = new HogaGb("00", "지정가");
            hogaGb[1] = new HogaGb("03", "시장가");
            hogaGb[2] = new HogaGb("05", "조건부지정가");
            hogaGb[3] = new HogaGb("06", "최유리지정가");
            hogaGb[4] = new HogaGb("07", "최우선지정가");
            hogaGb[5] = new HogaGb("10", "지정가IOC");
            hogaGb[6] = new HogaGb("13", "시장가IOC");
            hogaGb[7] = new HogaGb("16", "최유리IOC");
            hogaGb[8] = new HogaGb("20", "지정가FOK");
            hogaGb[9] = new HogaGb("23", "시장가FOK");
            hogaGb[10] = new HogaGb("26", "최유리FOK");
            hogaGb[11] = new HogaGb("61", "시간외단일가매매");
            hogaGb[12] = new HogaGb("81", "시간외종가");

            futureHoga[0] = new HogaGb("2", "지정가");
            futureHoga[1] = new HogaGb("1", "시장가");
            futureHoga[2] = new HogaGb("2", "STOP");
            futureHoga[3] = new HogaGb("2", "STOPLIMIT");

            // 마켓코드 설정
            marketCode[0] = new MarketCode("0", "장내");
            marketCode[1] = new MarketCode("3", "ELW");
            marketCode[2] = new MarketCode("4", "뮤추얼펀드");
            marketCode[3] = new MarketCode("5", "신주인수권");
            marketCode[4] = new MarketCode("6", "리츠");
            marketCode[5] = new MarketCode("8", "ETF");
            marketCode[6] = new MarketCode("9", "하이일드펀드");
            marketCode[7] = new MarketCode("10", "코스닥");
            marketCode[8] = new MarketCode("30", "제3시장");

            initFIDName();
        }

        public static Dictionary<string, string> futureFIDName_ = new Dictionary<string, string>();
        private static void initFIDName()
        {
            futureFIDName_.Add("9201", "계좌번호");
            futureFIDName_.Add("9203", "주문번호");
            futureFIDName_.Add("9001", "종목코드");
            futureFIDName_.Add("907", "매도수구분");
            futureFIDName_.Add("905", "주문/체결구분");
            futureFIDName_.Add("904", "원주문번호");
            futureFIDName_.Add("302", "종목명");
            futureFIDName_.Add("906", "주문유형");
            futureFIDName_.Add("900", "주문수량");
            futureFIDName_.Add("901", "주문가격");
            futureFIDName_.Add("13333", "조건가격");
            futureFIDName_.Add("13330", "주문표시가격");
            futureFIDName_.Add("13332", "조건표시가격");
            futureFIDName_.Add("902", "미체결수량");
            futureFIDName_.Add("913", "주문상태");
            futureFIDName_.Add("919", "반대매매여부");
            futureFIDName_.Add("8046", "거래소코드");
            futureFIDName_.Add("947", "FCM코드");
            futureFIDName_.Add("8043", "통화코드");
            futureFIDName_.Add("908", "주문/체결시간");
            futureFIDName_.Add("909", "체결번호");
            futureFIDName_.Add("911", "체결수량");
            futureFIDName_.Add("910", "체결가격");
            futureFIDName_.Add("13331", "체결표시가격");
            futureFIDName_.Add("13329", "체결금액");
            futureFIDName_.Add("13326", "거부수량");
            futureFIDName_.Add("935", "체결수수료");
            futureFIDName_.Add("13327", "신규수량");
            futureFIDName_.Add("13328", "청산수량");
            futureFIDName_.Add("8018", "실현손익");
            futureFIDName_.Add("8009", "약정금액");
            futureFIDName_.Add("930", "미결제약정합계");
            futureFIDName_.Add("13334", "미결제약정단가표시(평균)");
        }
    }


    public class Error
    {
        private static string errorMessage;

        Error()
        {
            errorMessage = "";
        }

        ~Error()
        {
            errorMessage = "";
        }

        public static string GetErrorMessage()
        {
            return errorMessage;
        }

        public static bool IsError(int nErrorCode)
        {
            bool bRet = false;

            switch (nErrorCode) {
                case KOAErrorCode.OP_ERR_NONE:
                errorMessage = "[" + nErrorCode.ToString() + "] :" + "정상처리";
                bRet = true;
                break;
                case KOAErrorCode.OP_ERR_LOGIN:
                errorMessage = "[" + nErrorCode.ToString() + "] :" + "사용자정보교환에 실패하였습니다. 잠시 후 다시 시작하여 주십시오.";
                break;
                case KOAErrorCode.OP_ERR_CONNECT:
                errorMessage = "[" + nErrorCode.ToString() + "] :" + "서버 접속 실패";
                break;
                case KOAErrorCode.OP_ERR_VERSION:
                errorMessage = "[" + nErrorCode.ToString() + "] :" + "버전처리가 실패하였습니다";
                break;
                case KOAErrorCode.OP_ERR_DISCONNECT:
                errorMessage = "[" + nErrorCode.ToString() + "] :" + "연결이 끊겼습니다.";
                break;
                case KOAErrorCode.OP_ERR_SISE_OVERFLOW:
                errorMessage = "[" + nErrorCode.ToString() + "] :" + "시세조회 과부하";
                break;
                case KOAErrorCode.OP_ERR_RQ_STRUCT_FAIL:
                errorMessage = "[" + nErrorCode.ToString() + "] :" + "REQUEST_INPUT_st Failed";
                break;
                case KOAErrorCode.OP_ERR_RQ_STRING_FAIL:
                errorMessage = "[" + nErrorCode.ToString() + "] :" + "요청 전문 작성 실패";
                break;
                case KOAErrorCode.OP_ERR_ORD_WRONG_INPUT:
                errorMessage = "[" + nErrorCode.ToString() + "] :" + "주문 입력값 오류";
                break;
                case KOAErrorCode.OP_ERR_ORD_WRONG_ACCNO:
                errorMessage = "[" + nErrorCode.ToString() + "] :" + "계좌비밀번호를 입력하십시오.";
                break;
                case KOAErrorCode.OP_ERR_OTHER_ACC_USE:
                errorMessage = "[" + nErrorCode.ToString() + "] :" + "타인계좌는 사용할 수 없습니다.";
                break;
                case KOAErrorCode.OP_ERR_MIS_2BILL_EXC:
                errorMessage = "[" + nErrorCode.ToString() + "] :" + "주문가격이 20억원을 초과합니다.";
                break;
                case KOAErrorCode.OP_ERR_MIS_5BILL_EXC:
                errorMessage = "[" + nErrorCode.ToString() + "] :" + "주문가격은 50억원을 초과할 수 없습니다.";
                break;
                case KOAErrorCode.OP_ERR_MIS_1PER_EXC:
                errorMessage = "[" + nErrorCode.ToString() + "] :" + "주문수량이 총발행주수의 1%를 초과합니다.";
                break;
                case KOAErrorCode.OP_ERR_MID_3PER_EXC:
                errorMessage = "[" + nErrorCode.ToString() + "] :" + "주문수량은 총발행주수의 3%를 초과할 수 없습니다";
                break;
                default:
                errorMessage = "[" + nErrorCode.ToString() + "] :" + "알려지지 않은 오류입니다.";
                break;
            }

            return bRet;
        }
    }
}
