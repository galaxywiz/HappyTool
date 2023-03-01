namespace UtilLibrary
{
    public enum PRICE_TYPE
    {
        MIN_1,          // 5분 가격표
        MIN_2,          // 15분 가격표
        MIN_3,          // 1시각 가격표
        ONE_MIN,        // 매 분 데이터 (백테스팅을 위해)
        DAY,            // 일 가격표
        WEEK,           // 주간 가격표

        MAX,
    };
}
