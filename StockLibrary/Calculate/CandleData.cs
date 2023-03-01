using System;
using System.Globalization;

namespace StockLibrary
{
    // 평균 샘플링
    public enum AVG_SAMPLEING
    {
        AVG_5,                 //최근에서 5번
        AVG_10,                //10번 ...
        AVG_20,
        AVG_50,
        AVG_100,
        AVG_200,

        AVG_MAX,
    };

    public enum ENVELOPE_LINE
    {
        ENVELOPE_UPPER,
        ENVELOPE_CENTER,
        ENVELOPE_LOWER,

        ENVELOPE_MAX,
    }

    // 평가 데이터
    public enum EVALUATION_DATA
    {
        SMA_3,
        SMA_5,              //단순 이동평균     // 0
        SMA_10,
        SMA_20,
        SMA_50,
        SMA_100,
        SMA_200,                                // 5

        EMA_3,
        EMA_5,              //지수 이동평균
        EMA_10,
        EMA_20,
        EMA_50,
        EMA_100,
        EMA_200,                                 // 11

        WMA_3,
        WMA_5,
        WMA_10,
        WMA_20,
        WMA_50,
        WMA_100,
        WMA_200,

        MACD,               //MACD
        MACD_SIGNAL,
        MACD_OSCIL,

        SMA_BOLLINGER_UP,    //볼린저
        SMA_BOLLINGER_CENTER,   //중심선
        SMA_BOLLINGER_DOWN,

        EMA_BOLLINGER_UP,    //볼린저
        EMA_BOLLINGER_CENTER,   //중심선
        EMA_BOLLINGER_DOWN,

        WMA_BOLLINGER_UP,    //볼린저
        WMA_BOLLINGER_CENTER,   //중심선
        WMA_BOLLINGER_DOWN,

        PRICE_CHANNEL_UP,
        PRICE_CHANNEL_CENTER,
        PRICE_CHANNEL_DOWN,

        ENVELOPE_UP,          // 과매수 매도 구간
        ENVELOPE_CENTER,
        ENVELOPE_DOWN,

        // 기술지표
        RSI,
        RSI_SIGNAL,
        STOCHASTIC_K,       //slow k/d
        STOCHASTIC_D,
        STOCHASTIC_RSI_K,
        STOCHASTIC_RSI_D,

        DI_PLUS,            // 추세 판단 기준. ID+ 는 14일간 주식이 오른 빈도율
        DI_MINUS,           // -DI는 14일간 주식이 내린 빈도율
        ADX,                // 추세가 강할수록 값의 크기가 커진다. (즉 값이 작으면 추세가 약하다는 뜻)

        WILLIAMS,
        CCI,
        ATR,
        ATR_30,
        ROC,
        ULTIMATE_OSCIL,
        PARABOLIC_SAR,

        BULL_POWER,         // 불뤼시 (황소) 폭등 파워
        BEAR_POWER,         // 베어쉬 (곰) 폭락 파워
    }

    public enum CandleType
    {
        음봉_장대,
        음봉_역망치,
        음봉_망치,
        음봉_일반,

        십자형,

        양봉_장대,
        양봉_역망치,
        양봉_망치,
        양봉_일반,
    }

    public class CandleData
    {
        public System.DateTime date_ { get; }
        public string dateStr_ { get; }
        public bool makeCandle_ = false;        // 직접 만든건가?
        public double price_ { get; set; }              // 현재가
        public double startPrice_ { get; set; }         // 시가
        public double highPrice_ { get; set;  }          // 고가
        public double lowPrice_ { get; set; }           // 저가
        public UInt64 volume_ { get; set; }            // 거래량
        public double centerPrice_ { get; set; }     // 중간값 (현재, 시가, 고가, 저가의 평균)
        public double[] calc_ { get; set; }
        public bool dbSaved_;

        public static DateTime strToDateTime(string date)
        {
            if (date.Length == 8) {
                return DateTime.ParseExact(date, "yyyyMMdd", CultureInfo.InvariantCulture);
            }
            else if (date.Length == 14) {
                return DateTime.ParseExact(date, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
            }
            return DateTime.Now;
        }

        public CandleData(DateTime date, double price, double startPrice, double highPrice, double lowPrice, UInt64 volume)
        {
            this.dateStr_ = date.ToString("yyyyMMddHHmm00");
            this.date_ = date;

            this.setPrice(price, startPrice, highPrice, lowPrice, volume);
        }

        public CandleData(string date, double price, double startPrice, double highPrice, double lowPrice, UInt64 volume)
        {
            this.dateStr_ = date;
            this.date_ = CandleData.strToDateTime(this.dateStr_);
            this.setPrice(price, startPrice, highPrice, lowPrice, volume);
        }

        void setPrice(double price, double startPrice, double highPrice, double lowPrice, UInt64 volume)
        {
            this.price_ = Math.Abs(price);
            this.startPrice_ = Math.Abs(startPrice);
            this.highPrice_ = Math.Abs(highPrice);
            this.lowPrice_ = Math.Abs(lowPrice);
            this.volume_ = volume;
            this.setAvgPrice();

            this.calc_ = new double[Enum.GetValues(typeof(EVALUATION_DATA)).Length];
            this.dbSaved_ = false;
        }

        void setAvgPrice()
        {
            this.centerPrice_ = (this.price_ + this.highPrice_ + this.lowPrice_) / 3;
        }

        // 지표 계산에 쓸 캔들 가격.
        public double avgPrice()
        {
            return centerPrice_;
        }

        public bool isSame(CandleData other)
        {
            if (this.dateStr_ != other.dateStr_) {
                return false;
            }
            if (this.price_ != other.price_) {
                return false;
            }
            if (this.startPrice_ != other.startPrice_) {
                return false;
            }
            if (this.highPrice_ != other.highPrice_) {
                return false;
            }
            if (this.lowPrice_ != other.lowPrice_) {
                return false;
            }

            return true;
        }

        public double height()
        {
            return this.highPrice_ - this.lowPrice_;
        }

        public double heightOfCandleBody()
        {
            return Math.Abs(this.startPrice_ - this.price_);            
        }

        public double minPrice()
        {
            return Math.Min(Math.Min(Math.Min(this.price_, this.startPrice_), this.highPrice_), this.lowPrice_);
        }
        public double maxPrice()
        {
            return Math.Max(Math.Max(Math.Max(this.price_, this.startPrice_), this.highPrice_), this.lowPrice_);
        }

        public CandleType type()
        {
            //-----------------------------------------------------------//
            // 장대 양봉 : 시가 = 하한가, 종가 = 상한가
            //if (this.startPrice_ == this.lowPrice_ && this.price_ == this.highPrice_) {
            //    return CandleType.양봉_장대;
            //}
            //// 양봉 역망치 : 시가 = 하한가, 종가 < 상한가
            //if (this.startPrice_ == this.lowPrice_ && this.price_ < this.highPrice_) {
            //    return CandleType.양봉_역망치;
            //}
            //// 양봉 망치 : 시가 ＞ 하한가, 종가 = 상한가
            //if (this.startPrice_ > this.lowPrice_ && this.price_ == this.highPrice_) {
            //    return CandleType.양봉_망치;
            //}
            //// 양봉 : 시가 > 하한가, 종가 < 상한가
            //if (this.startPrice_ > this.lowPrice_ && this.price_ < this.highPrice_) {
            //    return CandleType.양봉_일반;
            //}
            if (this.startPrice_ < this.price_) {
                return CandleType.양봉_일반;
            }

            ////-----------------------------------------------------------//
            //// 음봉 장대 :시가 = 상한가, 종가 = 하한가
            //if (this.startPrice_ == this.highPrice_ && this.price_ == this.lowPrice_) {
            //    return CandleType.음봉_장대;
            //}
            //// 음봉 역망치 : 시가 = 상한가, 종가 < 하한가
            //if (this.startPrice_ == this.highPrice_ && this.price_ < this.lowPrice_) {
            //    return CandleType.음봉_역망치;
            //}
            //// 음봉 망치 : 시가 ＞ 상한가, 종가 = 하한가
            //if (this.startPrice_ > this.highPrice_ && this.price_ == this.lowPrice_) {
            //    return CandleType.음봉_망치;
            //}
            //// 음봉 : 시가 > 상한가, 종가 < 하한가
            //if (this.startPrice_ > this.highPrice_ && this.price_ < this.lowPrice_) {
            //    return CandleType.음봉_일반;
            //}
            if (this.startPrice_ > this.price_) {
                return CandleType.음봉_일반;
            }
            //-----------------------------------------------------------//
            // 그이외는 십자
            return CandleType.십자형;
        }

        public bool isMinusCandle()
        {
            if (startPrice_ > price_) {
                return true;
            }
            return false;
        }

        public bool isPlusCandle()
        {
            if (startPrice_ < price_) { 
                return true;
            }
            return false;
        }

        // 가격이 캔들에 닪았는가?
        public bool isInnerCandle(double price)
        {
            if (UtilLibrary.Util.isRange(this.lowPrice_, price, this.highPrice_)) {
                return true;
            }
            return false;
        }

        public bool isNowBadCandle(double baseSize)
        {
            var t = (highPrice_ - lowPrice_) / baseSize;
            if (t <= (baseSize * 3)) {
                return true;
            }
            return false;
        }

        // 정배열인가?
        public bool isMaUpeer(EVALUATION_DATA maStart, EVALUATION_DATA maEnd)
        {
            var total = maEnd - maStart;
            var maS = this.calc_[(int) maStart];
            for (int i = 1; i < total; ++i) {
                var maV = this.calc_[(int) maStart + i];
                if (maS < maV) {
                    return false;
                }
                maS = maV;
            }
            return true;
        }
        public bool isMaDown(EVALUATION_DATA maStart, EVALUATION_DATA maEnd)
        {
            var total = maEnd - maStart;
            var maS = this.calc_[(int) maStart];
            for (int i = 1; i < total; ++i) {
                var maV = this.calc_[(int) maStart + i];
                if (maS > maV) {
                    return false;
                }
                maS = maV;
            }
            return true;
        }

        public double candleClosePosition()
        {
            var pos = (price_ - lowPrice_) / (highPrice_ - lowPrice_) * 100;
            return pos;
        }
    };
}
