using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UtilLibrary;

namespace StockLibrary.StrategyManager
{
    public class AssetBettorList: SingleTon<AssetBettorList>
    {
        public List<AssetBettor> bettorList_ = new List<AssetBettor>();
        public AssetBettorList()
        {
            bettorList_.Add(new AssetBettor());
    //        bettorList_.Add(new TechAssetBettor());
            bettorList_.Add(new CumulativeAssetBettor());
            bettorList_.Add(new MAAssetBettor());
            bettorList_.Add(new ShortTermAssetBettor());
            bettorList_.Add(new LongTermAssetBettor());
        }

        public AssetBettor getAssetBettor(string className)
        {
            if (className.Length == 0) {
                return null;
            }
            Assembly assembly = Assembly.GetExecutingAssembly();
            Type type = assembly.GetType("StockLibrary.StrategyManager." + className);
            if (type == null) {
                return null;
            }
            AssetBettor instance = Activator.CreateInstance(type) as AssetBettor;
            return instance;
        }

    }

    // 배팅 관리자
    public class AssetBettor
    {
        public virtual int countTradeBuy(StockData stockData, int maxCount)
        {
            return 1;
        }

        public virtual int countTradeSell(StockData stockData, int maxCount)
        {
            return 1;
        }
    }

    //public class TechAssetBettor: AssetBettor
    //{
    //    public override int countTradeBuy(StockData stockData, int maxCount)
    //    {
    //        var signalRate = stockData.buySignal(bot_);
    //        var rate = signalRate;
    //        var c = (maxCount * (rate / 100));
    //        return (int) c;
    //    }

    //    public override int countTradeSell(StockData stockData, int maxCount)
    //    {
    //        var signalRate = stockData.sellSignal(bot_);
    //        var rate = signalRate;
    //        var c = (maxCount * (rate / 100));
    //        return (int) c;
    //    }
    //}

    //https://stock79.tistory.com/248?category=457286
    //https://stock79.tistory.com/249?category=457286
    public class CumulativeAssetBettor: AssetBettor
    {
        public override int countTradeBuy(StockData stockData, int maxCount)
        {
            var statics = stockData.statisticsCount_;
            var unit = statics.bettorUnit();
            return Math.Min(unit, maxCount);
        }

        public override int countTradeSell(StockData stockData, int maxCount)
        {
            var statics = stockData.statisticsCount_;
            var unit = statics.bettorUnit();
            return Math.Min(unit, maxCount);
        }
    }

    public class ShortTermAssetBettor: MAAssetBettor
    {
        public ShortTermAssetBettor()
        {
            int[] LIST = { 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 };
            AVG_LIST = LIST;
        }
    }
    public class LongTermAssetBettor: MAAssetBettor
    {
        public LongTermAssetBettor()
        {
            int[] LIST = { 50, 70, 90, 110, 130, 150, 170, 190, 210, 230 };
            AVG_LIST = LIST;
        }
    }

    public class MAAssetBettor: AssetBettor
    {
        MACalculater avgCalc_ = new MACalculater();
        protected int[] AVG_LIST;
        int MAX_COUNT = 10;

        public MAAssetBettor()
        {
            int[] LIST = {5, 10, 20, 50, 70, 90, 110, 130, 150, 170, 190, 210, 230, 250 };
            AVG_LIST = LIST;
        }

        public override int countTradeBuy(StockData stockData, int maxCount)
        {
            int count = 1;

            var nowPrice = stockData.nowCandle().centerPrice_;
            var prevPrice = stockData.prevCandle().centerPrice_;
            int outNBElement;

            foreach (var avg in AVG_LIST) {
                var maList = avgCalc_.calcAvg(stockData.priceTable(), TicTacTec.TA.Library.Core.MAType.Sma, avg, out outNBElement);
                if (outNBElement < 2) {
                    continue;
                }
                var ma = maList[outNBElement - 1];
                var prevMa = maList[outNBElement - 2];
                if (ma < nowPrice) {
                    if (prevMa < prevPrice) {
                        count++;
                    }
                    else {
                        break;
                    }
                }
                else {
                    break;
                }
            }
            var len = AVG_LIST.Length + 1;
            var avgRate = (double) count / (double) len * 100;
            int c = (int) Math.Round(maxCount * (avgRate / 100));
            return Math.Min(c, MAX_COUNT);
        }

        public override int countTradeSell(StockData stockData, int maxCount)
        {
            int count = 1;

            var nowPrice = stockData.nowCandle().centerPrice_;
            var prevPrice = stockData.prevCandle().centerPrice_;
            int outNBElement;

            foreach (var avg in AVG_LIST) {
                var maList = avgCalc_.calcAvg(stockData.priceTable(), TicTacTec.TA.Library.Core.MAType.Sma, avg, out outNBElement);
                if (outNBElement < 2) {
                    continue;
                }
                var ma = maList[outNBElement - 1];
                var prevMa = maList[outNBElement - 2];
                if (ma > nowPrice) {
                    if (prevMa > prevPrice) {
                        count++;
                    }
                    else {
                        break;
                    }
                }
                else {
                    break;
                }
            }
            var len = AVG_LIST.Length + 1;
            var avgRate = (double) count / (double) len * 100;
            int c = (int) Math.Round(maxCount * (avgRate / 100));
            return Math.Min(c, MAX_COUNT);
        }
    }
}
