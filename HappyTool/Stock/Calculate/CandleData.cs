using System;
using System.Globalization;

namespace HappyTool.Stock.Calculate
{
    enum PRICE_TYPE
    {
        MIN15,          // 15분 가격표
        HOUR1,          // 1시각 가격표
        HOUR4,          // 1시각 가격표
        DAY,            // 일 가격표
        WEEK,           // 주간 가격표

        MAX,
    };

    // 평균 샘플링
    enum AVG_SAMPLEING
    {
        AVG_5,                 //최근에서 5번
        AVG_10,                //10번 ...
        AVG_20,
        AVG_50,
        AVG_100,
        AVG_200,

        AVG_MAX,
    };

    enum BNG_LINE
    {
        BNF_UPPER,
        BNF_CENTER,
        BNF_LOWER,

        BNF_MAX,
    }

    // 평가 데이터
    enum EVALUATION_DATA {
        SMA_5,              //단순 이동평균
        SMA_10,
        SMA_20,
        SMA_50,
        SMA_100,
        SMA_200,

        EMA_5,              //지수 이동평균
        EMA_10,
        EMA_20,
        EMA_50,
        EMA_100,
        EMA_200,

        BOLLINGER_UPPER,    //볼린저
        BOLLINGER_CENTER,   //중심선
        BOLLINGER_LOWER,

        MACD,               //MACD
        MACD_SIGNAL,
        MACD_OSCIL,

        BNF_UPPER,          // 과매수 매도 구간
        BNF_CENTER,
        BNF_LOWER,        

        RSI,
        STOCHASTIC_K,       //slow k/d
        STOCHASTIC_D,
        STOCHASTIC_RSI,

        DI_PLUS,
        DI_MINUS,
        ADX,

        WILLIAMS,
        CCI,
        ATR,
        ROC,
        ULTIMATE_OSCIL,

        BULL_POWER,
        BEAR_POWER,

        MAX,

        SMA_START = SMA_5,
        EMA_START = EMA_5,
        AVG_END = EMA_200 + 1,
    }

    enum CandleType
    {
        양봉_장대,
        양봉_역망치,
        양봉_망치,
        양봉_일반,

        음봉_장대,
        음봉_역망치,
        음봉_망치,
        음봉_일반,

        십자형,
    }

    struct CandleData : ICloneable
    {
        public System.DateTime date_;
        public string dateStr_;

        public int price_;              // 현재가
        public int startPrice_;         // 시가
        public int highPrice_;          // 고가
        public int lowPrice_;           // 저가
        public double centerPrice_;     // 중간값 (현재, 시가, 고가, 저가의 평균)
        public double[] calc_;

        public CandleData(string date, int price, int startPrice, int highPrice, int lowPrice)
        {
            dateStr_ = date;
            if (date.Length == 8)
            {
                date_ = DateTime.ParseExact(dateStr_, "yyyyMMdd", CultureInfo.InvariantCulture);
            }
            else
            {
                date_ = DateTime.ParseExact(dateStr_, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
            }

            price_ = Math.Abs(price);
            startPrice_ = Math.Abs(startPrice);
            highPrice_ = Math.Abs(highPrice);
            lowPrice_ = Math.Abs(lowPrice);
            centerPrice_ = (price_ + startPrice_ + highPrice_ + lowPrice_) / 4;

            calc_ = new double[(int) EVALUATION_DATA.MAX];
        }


        public int minPrice()
        {
            return Math.Min(Math.Min(Math.Min(price_, startPrice_), highPrice_), lowPrice_);
        }
        public int maxPrice()
        {
            return Math.Max(Math.Max(Math.Max(price_, startPrice_), highPrice_), lowPrice_);
        }

        public CandleType type()
        {
            //-----------------------------------------------------------//
            // 장대 양봉 : 시가 = 하한가, 종가 = 상한가
            if (startPrice_ == lowPrice_ && price_ == highPrice_)
            {
                return CandleType.양봉_장대;
            }
            // 양봉 역망치 : 시가 = 하한가, 종가 < 상한가
            if (startPrice_ == lowPrice_ && price_ < highPrice_)
            {
                return CandleType.양봉_역망치;
            }
            // 양봉 망치 : 시가 ＞ 하한가, 종가 = 상한가
            if (startPrice_ > lowPrice_ && price_ == highPrice_)
            {
                return CandleType.양봉_망치;
            }
            // 양봉 : 시가 > 하한가, 종가 < 상한가
            if (startPrice_ > lowPrice_ && price_ < highPrice_)
            {
                return CandleType.양봉_일반;
            }
            
            //-----------------------------------------------------------//
            // 음봉 장대 :시가 = 상한가, 종가 = 하한가
            if (startPrice_ == highPrice_ && price_ == lowPrice_)
            {
                return CandleType.음봉_장대;
            }
            // 음봉 역망치 : 시가 = 상한가, 종가 < 하한가
            if (startPrice_ == highPrice_ && price_ < lowPrice_)
            {
                return CandleType.음봉_역망치;
            }
            // 음봉 망치 : 시가 ＞ 상한가, 종가 = 하한가
            if (startPrice_ > highPrice_ && price_ == lowPrice_)
            {
                return CandleType.음봉_망치;
            }
            // 음봉 : 시가 > 상한가, 종가 < 하한가
            if (startPrice_ > highPrice_ && price_ < lowPrice_)
            {
                return CandleType.음봉_일반;
            }

            //-----------------------------------------------------------//
            // 그이외는 십자
            return CandleType.십자형;
        }

        public Object Clone()
        {
            CandleData clone = new CandleData(dateStr_, price_, startPrice_, highPrice_, lowPrice_);

            calc_.CopyTo(clone.calc_, 0);

            return clone;
        }
    };

}
