namespace StockLibrary.StrategyManager.ProfitSafer
{
    public enum PAY_OFF_CODE
    {
        forcePayOff,
        errorOff,
        payoffSignal,
        reverseTrande,

        Profit_TargetPrice,
        LostCut_TargetPrice,
        Profit_TargetRate,
        LostCut_TargettRate,

        //해선
        defaultCheck,
        Profit_BaseTime,
        Profit_DetectPlugePrices,
        LostCut_BaseTime,
        Profit_OffenseTrend,
        Profit_ProtectionTrend,
        LostCut_Trend,
        LostCut_LongTimeHaved,
        Profit_PullBack
    }
}
