using System;
using System.Collections.Generic;
using UtilLibrary;
using static StockLibrary.StrategyModuleList;

//https://steemit.com/kr/@phuzion7/stochastic-and-bitcoin
//여기에 소개된 전략을 최대한 로직으로 녹여 본다.

namespace StockLibrary
{
    public class TechTradeModlueList: SingleTon<TechTradeModlueList>
    {
        /*
        사용된 전략                 개수
        MADoubleTechTradeModlue	    2
        ADX_CenterTechTradeModlue		13
        EMATechTradeModlue			14

        ADX_DITechTradeModlue			25
        StochastictRSITechTradeModlue	28

        RSITechTradeModlue			41
        UltimateTechTradeModlue		41
        ParabolicSARTechTradeModlue	47
        StochastictTechTradeModlue	64
        CCITechTradeModlue			73
        WilliamsTechTradeModlue		73
        CandleTechTradeModlue			141
*/

        public List<TechTradeModlue> list_ = new List<TechTradeModlue>();
        public void makeList()
        {
            this.list_.Add(new MADoubleTechTradeModlue());
         //   this.list_.Add(new EMATechTradeModlue());

            // this.list_.Add(new RSICenterTechTradeModlue());
            // this.list_.Add(new ADX_DITechTradeModlue2());
            // this.list_.Add(new ADX_CenterTechTradeModlue());
            // this.list_.Add(new BullBearTechTradeModlue());
            this.list_.Add(new ADX_DITechTradeModlue());

            this.list_.Add(new StochastictRSITechTradeModlue());

            this.list_.Add(new RSITechTradeModlue());
            this.list_.Add(new UltimateTechTradeModlue());
            this.list_.Add(new ParabolicSARTechTradeModlue());
            this.list_.Add(new StochastictTechTradeModlue());

            this.list_.Add(new WilliamsTechTradeModlue());
            this.list_.Add(new CCITechTradeModlue());

            this.list_.Add(new CandleTechTradeModlue());
        }
    }


    public class TechTradeModlue
    {
        protected StrategyModule test_;

        protected STOCK_EVALUATION evalTotal(StockData stockData, int timeIdx)
        {
            List<CandleData> priceTable = stockData.priceTable();
            if (priceTable == null) {
            }
            Evaluation stockAnalysis = new Evaluation();
            STOCK_EVALUATION evalTotal = stockAnalysis.analysis(priceTable, timeIdx);

            return evalTotal;
        }

        //-----------------------------------------------------------//
        // 각 클래스 마다 매매 전략 구현
        //-----------------------------------------------------------//
        public virtual bool buy(StockData stockData, int timeIdx)
        {
            return true;
        }

        public virtual bool sell(StockData stockData, int timeIdx)
        {
            return true;
        }

        public virtual bool buyPayOff(StockData stockData)
        {
            return this.sell(stockData, 0);
        }
        public virtual bool sellPayOff(StockData stockData)
        {
            return this.buy(stockData, 0);
        }
    }

    public class MADoubleTechTradeModlue: TechTradeModlue
    {
        //우리는 빠른 MA가 느린 MA(녹색 영역)를 통과 할 때를 구입하고 
        //빠른 MA가 느린 MA(적색 영역) 아래로 갈 때 판매합니다

        const int EMA_SLOW = (int) EVALUATION_DATA.EMA_50;
        const int EMA_FAST = (int) EVALUATION_DATA.EMA_20;
        // 5선이 20선을 돌파하면 골든 크로스임.
        public override bool buy(StockData stockData, int timeIdx)
        {
            const int TERM = 2;
            List<CandleData> priceTable = stockData.priceTable();
            if (priceTable == null) {
                return false;
            }
            int lastTime = priceTable.Count - TERM;
            if (lastTime < timeIdx) {
                return false;
            }

            if (timeIdx < TERM) {
                return false;
            }

            double slowEmaPrice = priceTable[timeIdx].calc_[EMA_SLOW];
            double fastEmaPrice = priceTable[timeIdx].calc_[EMA_FAST];

            double prevSlowEmaPrice = priceTable[timeIdx + 1].calc_[EMA_SLOW];
            double prevFastEmaPrice = priceTable[timeIdx + 1].calc_[EMA_FAST];

            if (prevSlowEmaPrice >= prevFastEmaPrice) {
                if (slowEmaPrice < fastEmaPrice) {
                    return true;
                }
            }
            return false;
        }

        // 5선이 20선 이하로 떨어지면 데드 크로스
        public override bool sell(StockData stockData, int timeIdx)
        {
            const int TERM = 2;
            List<CandleData> priceTable = stockData.priceTable();
            if (priceTable == null) {
                return false;
            }
            int lastTime = priceTable.Count - TERM;
            if (lastTime < timeIdx) {
                return false;
            }

            if (timeIdx < TERM) {
                return false;
            }

            double slowEmaPrice = priceTable[timeIdx].calc_[EMA_SLOW];
            double fastEmaPrice = priceTable[timeIdx].calc_[EMA_FAST];

            double prevSlowEmaPrice = priceTable[timeIdx + 1].calc_[EMA_SLOW];
            double prevFastEmaPrice = priceTable[timeIdx + 1].calc_[EMA_FAST];

            if (prevSlowEmaPrice <= prevFastEmaPrice) {
                if (slowEmaPrice > fastEmaPrice) {
                    return true;
                }
            }
            return false;
        }
    }

    public class EMATechTradeModlue: TechTradeModlue
    {
        //평균선 20일 선을 보고 판단.

        //캔들이 20일선을 뚫고 올라가면 상승 흐름으로 봄
        //캔들이 20일선 뚫고 내려가면 떡락
        const int EMA_SLOW = (int) EVALUATION_DATA.EMA_50;
        const int EMA_FAST = (int) EVALUATION_DATA.EMA_20;
        public override bool buy(StockData stockData, int timeIdx)
        {
            const int TERM = 3;
            List<CandleData> priceTable = stockData.priceTable();
            if (priceTable == null) {
                return false;
            }
            int lastTime = priceTable.Count - TERM;
            if (lastTime < timeIdx) {
                return false;
            }

            if (timeIdx < TERM) {
                return false;
            }

            double nowPrice = priceTable[timeIdx].price_;
            double ema20NowPrice = priceTable[timeIdx].calc_[EMA_FAST];

            double be1Price = priceTable[timeIdx + 1].price_;
            double ema20Be1Price = priceTable[timeIdx + 1].calc_[EMA_FAST];

            double be2Price = priceTable[timeIdx + 2].price_;
            double ema20Be2Price = priceTable[timeIdx + 2].calc_[EMA_FAST];

            // 2일전은 가격이 20일선 밑에 있다가
            if (be2Price < ema20Be2Price) {
                // 뚫고 올라오고
                if (be1Price >= ema20Be1Price) {
                    //그 기조를 유지한다 싶으면 구입
                    if (nowPrice > ema20NowPrice) {
                        return true;
                    }
                }
            }

            return false;
        }

        // 20선 이하로 가격이 떨어지면 팜
        public override bool sell(StockData stockData, int timeIdx)
        {
            const int TERM = 1;
            List<CandleData> priceTable = stockData.priceTable();
            if (priceTable == null) {
                return false;
            }
            int lastTime = priceTable.Count - TERM;
            if (lastTime < timeIdx) {
                return false;
            }

            if (timeIdx < TERM) {
                return false;
            }

            double nowPrice = priceTable[timeIdx].price_;
            double ema20NowPrice = priceTable[timeIdx].calc_[EMA_FAST];

            double be1Price = priceTable[timeIdx + 1].price_;
            double ema20Be1Price = priceTable[timeIdx + 1].calc_[EMA_FAST];

            double be2Price = priceTable[timeIdx + 2].price_;
            double ema20Be2Price = priceTable[timeIdx + 2].calc_[EMA_FAST];


            // 2일전은 가격이 20일선 밑에 있다가
            if (be2Price > ema20Be2Price) {
                // 뚫고 올라오고
                if (be1Price <= ema20Be1Price) {
                    //그 기조를 유지한다 싶으면 
                    if (nowPrice < ema20NowPrice) {
                        return true;
                    }
                }
            }
            return false;
        }
    }

    public class BollingerBandTechTradeModlue: TechTradeModlue
    {
        public override bool buy(StockData stockData, int timeIdx)
        {
            const int TERM = 2;
            List<CandleData> priceTable = stockData.priceTable();
            if (priceTable == null) {
                return false;
            }
            int lastTime = priceTable.Count - TERM;
            if (lastTime < timeIdx) {
                return false;
            }

            double low = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.SMA_BOLLINGER_DOWN];
            double price = priceTable[timeIdx].price_;

            if (price <= low) {
                return true;
            }

            return false;
        }

        public override bool sell(StockData stockData, int timeIdx)
        {
            const int TERM = 2;
            List<CandleData> priceTable = stockData.priceTable();
            if (priceTable == null) {
                return false;
            }
            int lastTime = priceTable.Count - TERM;
            if (lastTime < timeIdx) {
                return false;
            }

            double upper = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.SMA_BOLLINGER_UP];
            double price = priceTable[timeIdx].price_;

            if (price >= upper) {
                return true;
            }

            return false;
        }
    }


    public class BollingerBandCenterTechTradeModlue: TechTradeModlue
    {
        public override bool buy(StockData stockData, int timeIdx)
        {
            const int TERM = 2;
            List<CandleData> priceTable = stockData.priceTable();
            if (priceTable == null) {
                return false;
            }
            int lastTime = priceTable.Count - TERM;
            if (lastTime < timeIdx) {
                return false;
            }

            double prevLower = priceTable[timeIdx + 1].calc_[(int) EVALUATION_DATA.SMA_BOLLINGER_DOWN];
            double prevPrice = priceTable[timeIdx + 1].centerPrice_;

            double nowLower = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.SMA_BOLLINGER_DOWN];
            double nowCenter = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.SMA_BOLLINGER_CENTER];
            double nowPrice = priceTable[timeIdx].centerPrice_;

            if (nowPrice > nowCenter) {
         //       if (prevPrice <= prevLower) {
                    return true;
        //        }
            }

            return false;
        }

        public override bool sell(StockData stockData, int timeIdx)
        {
            const int TERM = 2;
            List<CandleData> priceTable = stockData.priceTable();
            if (priceTable == null) {
                return false;
            }
            int lastTime = priceTable.Count - TERM;
            if (lastTime < timeIdx) {
                return false;
            }

            double prevUpper = priceTable[timeIdx + 1].calc_[(int) EVALUATION_DATA.SMA_BOLLINGER_UP];
            double prevPrice = priceTable[timeIdx + 1].centerPrice_;

            double nowUpper = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.SMA_BOLLINGER_UP];
            double nowCenter = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.SMA_BOLLINGER_CENTER];
            double nowPrice = priceTable[timeIdx].centerPrice_;

            if (nowPrice < nowCenter) {
         //       if (prevPrice >= prevUpper) {
                    return true;
         //       }
            }

            return false;
        }
    }

    // RSI 지표를 이용해서 매매
    public class RSITechTradeModlue: TechTradeModlue
    {
        public override bool buy(StockData stockData, int timeIdx)
        {
            const int TERM = 2;
            List<CandleData> priceTable = stockData.priceTable();
            if (priceTable == null) {
                return false;
            }
            int lastTime = priceTable.Count - TERM;
            if (lastTime < timeIdx) {
                return false;
            }

            double prevRSI = priceTable[timeIdx + 1].calc_[(int) EVALUATION_DATA.RSI];
            double nowRSI = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.RSI];

            //RSI 30 돌파시 구입
            const double STAND_VALUE = 30;
            if (prevRSI <= STAND_VALUE) {
                if (nowRSI > STAND_VALUE) {
                    return true;
                }
            }

            return false;
        }

        public override bool sell(StockData stockData, int timeIdx)
        {
            const int TERM = 2;
            List<CandleData> priceTable = stockData.priceTable();
            if (priceTable == null) {
                return false;
            }
            int lastTime = priceTable.Count - TERM;
            if (lastTime < timeIdx) {
                return false;
            }

            double prevRSI = priceTable[timeIdx + 1].calc_[(int) EVALUATION_DATA.RSI];
            double nowRSI = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.RSI];

            //RSI 70 하강시 매도
            const double STAND_VALUE = 70;
            if (prevRSI >= STAND_VALUE) {
                if (nowRSI < STAND_VALUE) {
                    return true;
                }
            }

            return false;
        }
    }

    //https://www.tradingsetupsreview.com/trade-2-period-rsi/
    public class RSI2TechTradeModlue: TechTradeModlue
    {
        public override bool buy(StockData stockData, int timeIdx)
        {
            const int TERM = 2;
            List<CandleData> priceTable = stockData.priceTable();
            if (priceTable == null) {
                return false;
            }
            int lastTime = priceTable.Count - TERM;
            if (lastTime < timeIdx) {
                return false;
            }

            double prevRSI = priceTable[timeIdx + 1].calc_[(int) EVALUATION_DATA.RSI];
            double nowRSI = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.RSI];

            //RSI 30 돌파시 구입
            const double STAND_VALUE = 30;
            if (prevRSI <= STAND_VALUE) {
                if (nowRSI > STAND_VALUE) {
                    return true;
                }
            }

            return false;
        }

        public override bool sell(StockData stockData, int timeIdx)
        {
            const int TERM = 2;
            List<CandleData> priceTable = stockData.priceTable();
            if (priceTable == null) {
                return false;
            }
            int lastTime = priceTable.Count - TERM;
            if (lastTime < timeIdx) {
                return false;
            }

            double prevRSI = priceTable[timeIdx + 1].calc_[(int) EVALUATION_DATA.RSI];
            double nowRSI = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.RSI];

            //RSI 70 하강시 매도
            const double STAND_VALUE = 70;
            if (prevRSI >= STAND_VALUE) {
                if (nowRSI < STAND_VALUE) {
                    return true;
                }
            }

            return false;
        }
    }

    // RSI 지표를 이용해서 매매
    public class RSICenterTechTradeModlue: TechTradeModlue
    {
        public override bool buy(StockData stockData, int timeIdx)
        {
            const int TERM = 1;
            List<CandleData> priceTable = stockData.priceTable();
            if (priceTable == null) {
                return false;
            }
            int lastTime = priceTable.Count - TERM;
            if (lastTime < timeIdx) {
                return false;
            }

            double nowRSI = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.RSI];
            const double STAND_VALUE = 50;

            //RSI 50 돌파시 구입
            if (nowRSI > STAND_VALUE) {
                return true;
            }

            return false;
        }

        public override bool sell(StockData stockData, int timeIdx)
        {
            const int TERM = 1;
            List<CandleData> priceTable = stockData.priceTable();
            if (priceTable == null) {
                return false;
            }
            int lastTime = priceTable.Count - TERM;
            if (lastTime < timeIdx) {
                return false;
            }

            double nowRSI = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.RSI];
            const double STAND_VALUE = 50;

            //RSI 50 하강시 매도
            if (nowRSI < STAND_VALUE) {
                return true;
            }

            return false;
        }
    }

    // ADX 지표를 이용해서 매매
    public class ADX_DITechTradeModlue: TechTradeModlue
    {
        // ADX 랑 DMI 지표를 동시에 쓰자
        public override bool buy(StockData stockData, int timeIdx)
        {
            const int TERM = 2;
            List<CandleData> priceTable = stockData.priceTable();
            if (priceTable == null) {
                return false;
            }
            int lastTime = priceTable.Count - TERM;
            if (lastTime < timeIdx) {
                return false;
            }

            // 추세가 강할때 di를 사용
            double prevDI_Plus = priceTable[timeIdx + 1].calc_[(int) EVALUATION_DATA.DI_PLUS];
            double nowDI_Plus = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.DI_PLUS];

            double prevDI_Mi = priceTable[timeIdx + 1].calc_[(int) EVALUATION_DATA.DI_MINUS];
            double nowDI_Mi = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.DI_MINUS];

            // di+가 di-를 상향 돌파시 구입
            if (prevDI_Plus <= prevDI_Mi) {
                if (nowDI_Plus > nowDI_Mi) {
                    return true;
                }
            }

            return false;
        }

        public override bool sell(StockData stockData, int timeIdx)
        {
            const int TERM = 2;
            List<CandleData> priceTable = stockData.priceTable();
            if (priceTable == null) {
                return false;
            }
            int lastTime = priceTable.Count - TERM;
            if (lastTime < timeIdx) {
                return false;
            }

            // 추세가 강할때 di를 사용
            double prevDI_Plus = priceTable[timeIdx + 1].calc_[(int) EVALUATION_DATA.DI_PLUS];
            double nowDI_Plus = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.DI_PLUS];

            double prevDI_Mi = priceTable[timeIdx + 1].calc_[(int) EVALUATION_DATA.DI_MINUS];
            double nowDI_Mi = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.DI_MINUS];

            // di+가 di-를 하양 돌파시 매도
            if (prevDI_Plus >= prevDI_Mi) {
                if (nowDI_Plus < nowDI_Mi) {
                    return true;
                }
            }

            return false;
        }
    }

    // ADX 지표를 이용해서 매매
    public class ADX_DITechTradeModlue2: TechTradeModlue
    {
        // ADX 랑 DMI 지표를 동시에 쓰자
        public override bool buy(StockData stockData, int timeIdx)
        {
            const int TERM = 2;
            List<CandleData> priceTable = stockData.priceTable();
            if (priceTable == null) {
                return false;
            }
            int lastTime = priceTable.Count - TERM;
            if (lastTime < timeIdx) {
                return false;
            }

            // 추세가 강할때 di를 사용
            double nowADX = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.ADX];
            double prevADX = priceTable[timeIdx + 1].calc_[(int) EVALUATION_DATA.ADX];

            double nowDI_Plus = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.DI_PLUS];
            double nowDI_Mi = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.DI_MINUS];

            // pdi + adx 가 mid 보다 위에 있고, adx가 상승중이면 매수
            if (nowDI_Plus + nowADX > nowDI_Mi) {
                if (prevADX < nowADX) {
                    return true;
                }
            }

            return false;
        }

        public override bool sell(StockData stockData, int timeIdx)
        {
            const int TERM = 2;
            List<CandleData> priceTable = stockData.priceTable();
            if (priceTable == null) {
                return false;
            }
            int lastTime = priceTable.Count - TERM;
            if (lastTime < timeIdx) {
                return false;
            }

            // 추세가 강할때 di를 사용
            double nowADX = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.ADX];
            double prevADX = priceTable[timeIdx + 1].calc_[(int) EVALUATION_DATA.ADX];

            double nowDI_Plus = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.DI_PLUS];
            double nowDI_Mi = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.DI_MINUS];

            // mid + adx 가 pid 보다 위에 있고, adx가 상승중이면 매수
            if (nowDI_Mi + nowADX > nowDI_Plus) {
                if (prevADX < nowADX) {
                    return true;
                }
            }
            return false;
        }
    }

    // 현재 기조 판단용
    // adx 상승추세인지, 하락추세인지 확인
    public class ADX_CenterTechTradeModlue: TechTradeModlue
    {
        // ADX 랑 DMI 지표를 동시에 쓰자
        public override bool buy(StockData stockData, int timeIdx)
        {
            const int TERM = 1;
            List<CandleData> priceTable = stockData.priceTable();
            if (priceTable == null) {
                return false;
            }
            int lastTime = priceTable.Count - TERM;
            if (lastTime < timeIdx) {
                return false;
            }

            // plus가 minus 위에 있으면 매수 추세
            double nowDI_Plus = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.DI_PLUS];
            double nowDI_Mi = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.DI_MINUS];
            double nowADX = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.ADX];
            const double STAND_VALUE = 50;

            if (nowDI_Mi < nowDI_Plus) {
                if (nowADX >= STAND_VALUE) {
                    return true;
                }
            }
            return false;
        }

        public override bool sell(StockData stockData, int timeIdx)
        {
            const int TERM = 1;
            List<CandleData> priceTable = stockData.priceTable();
            if (priceTable == null) {
                return false;
            }
            int lastTime = priceTable.Count - TERM;
            if (lastTime < timeIdx) {
                return false;
            }

            // plus가 minus 밑에 있으면 매도 추세
            double nowDI_Plus = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.DI_PLUS];
            double nowDI_Mi = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.DI_MINUS];
            double nowADX = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.ADX];
            const double STAND_VALUE = 50;

            if (nowDI_Mi > nowDI_Plus) {
                if (nowADX < STAND_VALUE) {
                    return true;
                }
            }
            return false;
        }
    }
    // CCI 지표를 이용해서 매매
    public class CCITechTradeModlue: TechTradeModlue
    {
        public override bool buy(StockData stockData, int timeIdx)
        {
            const int TERM = 2;
            List<CandleData> priceTable = stockData.priceTable();
            if (priceTable == null) {
                return false;
            }
            int lastTime = priceTable.Count - TERM;
            if (lastTime < timeIdx) {
                return false;
            }

            double prevCCI = priceTable[timeIdx + 1].calc_[(int) EVALUATION_DATA.CCI];
            double nowCCI = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.CCI];

            //CCI가 +100을 상승 돌파시 매수
            const double STAND_PLUS = 100.0f;
            if (prevCCI <= STAND_PLUS) {
                if (nowCCI > STAND_PLUS) {
                    return true;
                }
            }
            //CCI가 -100을 상향 이탈시 매수
            const double STAND_MINUS = -100.0f;
            if (prevCCI <= STAND_MINUS) {
                if (nowCCI > STAND_MINUS) {
                    return true;
                }
            }
            return false;
        }

        public override bool sell(StockData stockData, int timeIdx)
        {
            const int TERM = 2;
            List<CandleData> priceTable = stockData.priceTable();
            if (priceTable == null) {
                return false;
            }
            int lastTime = priceTable.Count - TERM;
            if (lastTime < timeIdx) {
                return false;
            }

            double prevCCI = priceTable[timeIdx + 1].calc_[(int) EVALUATION_DATA.CCI];
            double nowCCI = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.CCI];

            //CCI가 +100을 하강 돌파시 매도
            const double STAND_PLUS = 100.0f;
            if (prevCCI >= STAND_PLUS) {
                if (nowCCI < STAND_PLUS) {
                    return true;
                }
            }
            //CCI가 -100을 하강 이탈시 매도
            const double STAND_MINUS = -100.0f;
            if (prevCCI >= STAND_MINUS) {
                if (nowCCI < STAND_MINUS) {
                    return true;
                }
            }
            return false;
        }
    }

    // Stochastict 지표 이용
    //https://steemit.com/kr/@pys/stochastics
    // 안전한 방법으로 75이상의 과매수 구간에서 %K선이 %D선을 하향 돌파할 때 매도 포지션을 취하고
    // 25이하의 과매도 구간에서 %K선이 %D선을 상향 돌파할 때 매수포지션을 취하는 전략을 구사하시면 됩니다.
    public class StochastictTechTradeModlue: TechTradeModlue
    {
        public override bool buy(StockData stockData, int timeIdx)
        {
            const int TERM = 2;
            List<CandleData> priceTable = stockData.priceTable();
            if (priceTable == null) {
                return false;
            }
            int lastTime = priceTable.Count - TERM;
            if (lastTime < timeIdx) {
                return false;
            }

            double prevStoK = priceTable[timeIdx + 1].calc_[(int) EVALUATION_DATA.STOCHASTIC_K];
            double prevStoD = priceTable[timeIdx + 1].calc_[(int) EVALUATION_DATA.STOCHASTIC_D];
            double avgprev = (prevStoK + prevStoD) / 2;

            double nowStoK = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.STOCHASTIC_K];
            double nowStoD = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.STOCHASTIC_D];
            double avgNow = (nowStoD + nowStoK) / 2;


            //스토케 25이하 과매도 구간에서 K가 D선 상향 돌파 하는지 봄.
            const double STAND_VALUE = 25;
            if (avgprev > STAND_VALUE || avgNow > STAND_VALUE) {
                return false;
            }

            // k선이 d선을 상향 돌파함.
            if (prevStoK <= prevStoD) {
                if (nowStoK > nowStoD) {
                    return true;
                }
            }
            return false;
        }

        public override bool sell(StockData stockData, int timeIdx)
        {
            const int TERM = 2;
            List<CandleData> priceTable = stockData.priceTable();
            if (priceTable == null) {
                return false;
            }
            int lastTime = priceTable.Count - TERM;
            if (lastTime < timeIdx) {
                return false;
            }

            double prevStoK = priceTable[timeIdx + 1].calc_[(int) EVALUATION_DATA.STOCHASTIC_K];
            double prevStoD = priceTable[timeIdx + 1].calc_[(int) EVALUATION_DATA.STOCHASTIC_D];
            double avgprev = (prevStoK + prevStoD) / 2;

            double nowStoK = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.STOCHASTIC_K];
            double nowStoD = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.STOCHASTIC_D];
            double avgNow = (nowStoD + nowStoK) / 2;

            //스토케 74 과매수 구간에서 K가 D선 하양 돌파 하는지 봄.
            const double STAND_VALUE = 75;
            if (avgprev < STAND_VALUE || avgNow < STAND_VALUE) {
                return false;
            }

            // k선이 d선을 하양 돌파함.
            if (prevStoK >= prevStoD) {
                if (nowStoK < nowStoD) {
                    return true;
                }
            }
            return false;
        }
    }

    // StochasticRSI 지표 이용
    //https://mystorage1.tistory.com/518
    public class StochastictRSITechTradeModlue: TechTradeModlue
    {
        public override bool buy(StockData stockData, int timeIdx)
        {
            const int TERM = 2;
            List<CandleData> priceTable = stockData.priceTable();
            if (priceTable == null) {
                return false;
            }
            int lastTime = priceTable.Count - TERM;
            if (lastTime < timeIdx) {
                return false;
            }

            double prevRSIK = priceTable[timeIdx + 1].calc_[(int) EVALUATION_DATA.STOCHASTIC_RSI_K];
            double nowRSIK = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.STOCHASTIC_RSI_K];

            //RSI 20% 상향 돌파시 매수
            const double STAND_VALUE = 0.2f;
            if (prevRSIK <= STAND_VALUE) {
                if (nowRSIK > STAND_VALUE) {

                    double prevRSID = priceTable[timeIdx + 1].calc_[(int) EVALUATION_DATA.STOCHASTIC_RSI_D];
                    double nowRSID = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.STOCHASTIC_RSI_D];

                    if (prevRSIK <= prevRSID) {
                        if (nowRSIK > nowRSID) {
                            return true;
                        }
                        else {
                            return false;
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        public override bool sell(StockData stockData, int timeIdx)
        {
            const int TERM = 2;
            List<CandleData> priceTable = stockData.priceTable();
            if (priceTable == null) {
                return false;
            }
            int lastTime = priceTable.Count - TERM;
            if (lastTime < timeIdx) {
                return false;
            }

            double prevRSIK = priceTable[timeIdx + 1].calc_[(int) EVALUATION_DATA.STOCHASTIC_RSI_K];
            double nowRSIK = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.STOCHASTIC_RSI_K];
            //RSI 80% 하양 돌파시 매도
            const double STAND_VALUE = 0.8f;
            if (prevRSIK >= STAND_VALUE) {
                if (nowRSIK < STAND_VALUE) {
                    double prevRSID = priceTable[timeIdx + 1].calc_[(int) EVALUATION_DATA.STOCHASTIC_RSI_D];
                    double nowRSID = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.STOCHASTIC_RSI_D];

                    if (prevRSIK >= prevRSID) {
                        if (nowRSIK < nowRSID) {
                            return true;
                        }
                        else {
                            return false;
                        }
                    }
                    return true;
                }
            }
            return false;
        }
    }

    //https://steemit.com/kr/@phuzion7/r-williams-r-eos-usd
    // Williams 지표 이용
    //Larry Williams 는 자신의 지표에 의한 매매규칙을 다음과 같이 정의했다고 합니다. 
    //%R 이 -100 에 도달후, (기간이 10일일 경우) 5일후에 다시 -85/-95% 위로 올라가면 매수를 하고, 
    //값이 0에 도달한 후 5일 후에 -15/-5% 이하로 내려가면 매도를 합니다.
    public class WilliamsTechTradeModlue: TechTradeModlue
    {
        public override bool buy(StockData stockData, int timeIdx)
        {
            const int TERM = 2;
            List<CandleData> priceTable = stockData.priceTable();
            if (priceTable == null) {
                return false;
            }
            int lastTime = priceTable.Count - TERM;
            if (lastTime < timeIdx) {
                return false;
            }

            double prevW = priceTable[timeIdx + 1].calc_[(int) EVALUATION_DATA.WILLIAMS];
            double nowW = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.WILLIAMS];

            const double STAND_VALUE = -80;
            //Wililiams 가 -80 을 상향 돌파시 과매도가 끝났다고 본다 
            if (prevW <= STAND_VALUE) {
                if (nowW > STAND_VALUE) {
                    return true;
                }
            }
            return false;
        }

        public override bool sell(StockData stockData, int timeIdx)
        {
            const int TERM = 2;
            List<CandleData> priceTable = stockData.priceTable();
            if (priceTable == null) {
                return false;
            }
            int lastTime = priceTable.Count - TERM;
            if (lastTime < timeIdx) {
                return false;
            }

            double prevW = priceTable[timeIdx + 1].calc_[(int) EVALUATION_DATA.WILLIAMS];
            double nowW = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.WILLIAMS];

            const double STAND_VALUE = -20;

            //Wililiams 가 -20 을 하향 돌파시 과매수가 끝났다고 본다 
            if (prevW >= STAND_VALUE) {
                if (nowW < STAND_VALUE) {
                    return true;
                }
            }
            return false;
        }
    }

    //ATR 지표.. 이건 변동성을 알아보는 지표
    //http://egloos.zum.com/economical/v/1646265
    public class ATRTechTradeModlue: TechTradeModlue
    {
        public override bool buy(StockData stockData, int timeIdx)
        {
            const int TERM = 2;
            List<CandleData> priceTable = stockData.priceTable();
            if (priceTable == null) {
                return false;
            }
            int lastTime = priceTable.Count - TERM;
            if (lastTime < timeIdx) {
                return false;
            }

            double prevAtr = priceTable[timeIdx + 1].calc_[(int) EVALUATION_DATA.ATR];
            double nowAtr = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.ATR];

            const double STAND_VALUE = 30;

            //RSI 30 돌파시 구입
            if (prevAtr < STAND_VALUE) {
                if (nowAtr >= STAND_VALUE) {
                    return true;
                }
            }
            return false;
        }

        public override bool sell(StockData stockData, int timeIdx)
        {
            const int TERM = 2;
            List<CandleData> priceTable = stockData.priceTable();
            if (priceTable == null) {
                return false;
            }
            int lastTime = priceTable.Count - TERM;
            if (lastTime < timeIdx) {
                return false;
            }

            double prevAtr = priceTable[timeIdx + 1].calc_[(int) EVALUATION_DATA.ATR];
            double nowAtr = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.ATR];
            const double STAND_VALUE = 70;

            //RSI 70 하강시 매도
            if (prevAtr > STAND_VALUE) {
                if (nowAtr <= STAND_VALUE) {
                    return true;
                }
            }
            return false;
        }
    }

    // Ultimate 지표 이용
    //https://booja.blogspot.com/2012/06/ultimate.html
    public class UltimateTechTradeModlue: TechTradeModlue
    {
        public override bool buy(StockData stockData, int timeIdx)
        {
            const int TERM = 2;
            List<CandleData> priceTable = stockData.priceTable();
            if (priceTable == null) {
                return false;
            }
            int lastTime = priceTable.Count - TERM;
            if (lastTime < timeIdx) {
                return false;
            }

            double prevUlt = priceTable[timeIdx + 1].calc_[(int) EVALUATION_DATA.ULTIMATE_OSCIL];
            double nowUlt = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.ULTIMATE_OSCIL];
            const double STAND_VALUE = 30;

            //ULTIMATE_OSCIL 30 돌파시 구입
            if (prevUlt < STAND_VALUE) {
                if (nowUlt >= STAND_VALUE) {
                    return true;
                }
            }
            return false;
        }

        public override bool sell(StockData stockData, int timeIdx)
        {
            const int TERM = 2;
            List<CandleData> priceTable = stockData.priceTable();
            if (priceTable == null) {
                return false;
            }
            int lastTime = priceTable.Count - TERM;
            if (lastTime < timeIdx) {
                return false;
            }

            double prevUlt = priceTable[timeIdx + 1].calc_[(int) EVALUATION_DATA.ULTIMATE_OSCIL];
            double nowUlt = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.ULTIMATE_OSCIL];
            const double STAND_VALUE = 70;

            //ULTIMATE_OSCIL 70 하강시 매도
            if (prevUlt > STAND_VALUE) {
                if (nowUlt <= STAND_VALUE) {
                    return true;
                }
            }
            return false;
        }
    }

    //------------------------------------------------------------------------------
    // 캔들을 보고 판단한다.
    /*
       캔들 흐름 타기
	- 아래 그림자 캔들
		망치형 -> 하락 추세에서 발생시, 가격이 상승할 신호
		(아랫 꼬리가 몸통보다 길어야함) 음봉 망치형은 단기 하락을 부추김
		반드시 음봉에서 양봉으로 바뀐 망치를 반전 신호로 볼수 있음.
		교수형 -> 상승 추세에서 발생시, 가격이 하락할 신호
	- 위 그림자 캔들
		역망치형 -> 하락 추세에서 발생시, 가격이 상승할 신호 (윗꼬리는 몸통보다 길어야함.)
		유성형 -> 상승추세에서 발생시, 가격이 하락할 신호
	- 매수 캔들 패턴
		샛별형 -> 3개봉으로 판단.하강 추세에서 나옴.
		-> 2번 봉이 다양하게 구성
		-> 3번 봉 종가가 1번 봉의 절반 이상을 장악해야 함.
	- 매도 캔들 패턴
		이브닝 스타 -> 3개봉으로 판단
		상승 추세에서 나올 수 있음.
		샛별의 반대형으로 나타남.
	- 추세 반전 지속 캔들 패턴
		하락 반전 패턴
		상승 반전 패턴
		추세 지속 패턴
		-> 상승이 크게 만들어 졌으나 눌림 발생으로 수렴.
		-> 이후 추세가 상승 or 하강
	- 캔들 중심 추세
		-> 흑삼병 캔들 - 하락시 음봉 가격으로 단일켄들 추세를 꺠지 않고 이동 할때 발생
		-> 적삼병 캔들 - 상승시 양봉 가격으로...
	- 모두 되돌림을 줌. (피보나치 비율)
*/
    public class CandleTechTradeModlue: TechTradeModlue
    {
        public override bool buy(StockData stockData, int timeIdx)
        {
            const int TERM = 3;
            List<CandleData> priceTable = stockData.priceTable();
            if (priceTable == null) {
                return false;
            }
            int lastTime = priceTable.Count - TERM;
            if (lastTime < timeIdx) {
                return false;
            }
            CandleData now = priceTable[timeIdx];

            // 이전이 음봉이었다가 양봉 망치가 나오면 상승 신호
            CandleData twoBefore = priceTable[timeIdx + 1];
            if (twoBefore.isMinusCandle()) {
                if (now.type() == CandleType.양봉_망치) {
                    return true;
                }
                if (now.type() == CandleType.양봉_역망치) {
                    return true;
                }
            }

            // 샛별형인가?
            CandleData threeBefore = priceTable[timeIdx + 2];
            if (threeBefore.isMinusCandle()) {
                // 지금이 2개 전보다 상승 장악형이면...
                if (threeBefore.centerPrice_ < now.price_) {
                    return true;
                }
            }

            // 적삼병 캔들?
            if (threeBefore.isPlusCandle() && twoBefore.isPlusCandle() && now.isPlusCandle()) {
                return true;
            }
            return false;
        }

        public override bool sell(StockData stockData, int timeIdx)
        {
            List<CandleData> priceTable = stockData.priceTable();
            if (priceTable == null) {
                return false;
            }
            const int TERM = 3;
            int lastTime = priceTable.Count - TERM;
            if (lastTime < timeIdx) {
                return false;
            }
            CandleData now = priceTable[timeIdx];

            // 이전이 양봉이었다가 음봉 망치가 나오면 상승 신호
            CandleData twoBefore = priceTable[timeIdx + 1];
            if (twoBefore.isPlusCandle()) {
                if (now.type() == CandleType.음봉_망치) {
                    return true;
                }
                if (now.type() == CandleType.음봉_역망치) {
                    return true;
                }
            }

            // 이브닝 인가?
            CandleData threeBefore = priceTable[timeIdx + 2];
            if (threeBefore.isPlusCandle()) {
                // 지금이 2개 전보다 하강 장악형이면...
                if (threeBefore.centerPrice_ > now.price_) {
                    return true;
                }
            }
            if (threeBefore.isMinusCandle() && twoBefore.isMinusCandle() && now.isMinusCandle()) {
                return true;
            }
            return false;
        }
    }

    // 황소, 곰 전략
    //https://translate.google.co.kr/translate?hl=ko&sl=en&u=https://www.mql5.com/en/blogs/post/21248&prev=search
    public class BullBearTechTradeModlue: TechTradeModlue
    {
        public override bool buy(StockData stockData, int timeIdx)
        {
            const int TERM = 2;
            List<CandleData> priceTable = stockData.priceTable();
            if (priceTable == null) {
                return false;
            }
            int lastTime = priceTable.Count - TERM;
            if (lastTime < timeIdx) {
                return false;
            }

            double yesterBULL = priceTable[timeIdx + 1].calc_[(int) EVALUATION_DATA.BULL_POWER];
            double nowBULL = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.BULL_POWER];

            // BULL 이 양수로 돌아서면 매수 신호
            if (yesterBULL < 0 && 0 < nowBULL) {
                return true;
            }
            return false;
        }

        public override bool sell(StockData stockData, int timeIdx)
        {
            const int TERM = 2;
            List<CandleData> priceTable = stockData.priceTable();
            if (priceTable == null) {
                return false;
            }
            int lastTime = priceTable.Count - TERM;
            if (lastTime < timeIdx) {
                return false;
            }

            double yesterBULL = priceTable[timeIdx + 1].calc_[(int) EVALUATION_DATA.BULL_POWER];
            double nowBULL = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.BULL_POWER];

            // BULL 이 음수로 돌아서면 매수 신호
            if (yesterBULL > 0 && 0 > nowBULL) {
                return true;
            }
            return false;
        }
    }

    public class ParabolicSARTechTradeModlue: TechTradeModlue
    {
        public override bool buy(StockData stockData, int timeIdx)
        {
            const int TERM = 2;
            List<CandleData> priceTable = stockData.priceTable();
            if (priceTable == null) {
                return false;
            }
            int lastTime = priceTable.Count - TERM;
            if (lastTime < timeIdx) {
                return false;
            }

            //      double yesPrabolic = priceTable[timeIdx + 1].calc_[(int) EVALUATION_DATA.PARABOLIC_SAR];
            //      double yesPrice = priceTable[timeIdx + 1].highPrice_;

            double nowPrabolic = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.PARABOLIC_SAR];
            double nowPrice = priceTable[timeIdx].lowPrice_;

            // 전에는 파라볼릭이 가격 위에 있었구
            //       if (yesPrice < yesPrabolic) {
            // 지금은 파라볼릭이 가격보다 아래
            if (nowPrabolic < nowPrice) {
                return true;
            }
            //   }
            return false;
        }

        public override bool sell(StockData stockData, int timeIdx)
        {
            const int TERM = 2;
            List<CandleData> priceTable = stockData.priceTable();
            if (priceTable == null) {
                return false;
            }
            int lastTime = priceTable.Count - TERM;
            if (lastTime < timeIdx) {
                return false;
            }
            //     double yesPrabolic = priceTable[timeIdx + 1].calc_[(int) EVALUATION_DATA.PARABOLIC_SAR];
            //     double yesPrice = priceTable[timeIdx + 1].lowPrice_;

            double nowPrabolic = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.PARABOLIC_SAR];
            double nowPrice = priceTable[timeIdx].highPrice_;

            //    if (yesPrabolic < yesPrice) {
            // 파라볼릭이 가격보다 위에
            if (nowPrice < nowPrabolic) {
                return true;
            }
            //    }
            return false;
        }
    }

    /*
    전략식은 딱 3줄입니다. (Simple is Best!!!)
        var1 = (C - L) / (H - L) * 100;   
        if var1< 20 Then   buy("매수"); 
        if var1 > 80 Then exitlong("매수청산");
*/
    public class TwentyEightStrategyTechTradeModlue: TechTradeModlue
    {
        public override bool buy(StockData stockData, int timeIdx)
        {
            const int TERM = 1;
            List<CandleData> priceTable = stockData.priceTable();
            if (priceTable == null) {
                return false;
            }
            int lastTime = priceTable.Count - TERM;
            if (lastTime < timeIdx) {
                return false;
            }
            var nowCandle = priceTable[timeIdx];
            var close = nowCandle.price_;
            var low = nowCandle.lowPrice_;
            var high = nowCandle.highPrice_;

            const int STAND = 20;
            var value = (close - low) / (high - low) * 100;
            if (value < STAND) {
                return true;
            }
            return false;
        }

        public override bool sell(StockData stockData, int timeIdx)
        {
            const int TERM = 1;
            List<CandleData> priceTable = stockData.priceTable();
            if (priceTable == null) {
                return false;
            }
            int lastTime = priceTable.Count - TERM;
            if (lastTime < timeIdx) {
                return false;
            }
            var nowCandle = priceTable[timeIdx];
            var close = nowCandle.price_;
            var low = nowCandle.lowPrice_;
            var high = nowCandle.highPrice_;

            const int STAND = 80;
            var value = (close - low) / (high - low) * 100;
            if (value > STAND) {
                return true;
            }
            return false;
        }
    }

    //https://blog.naver.com/chartist/221472197803
    /***
     * 	
[STAD_11] Hit The Bull's Eye  전략_STAD / 시스템트레이딩 전략 
2011. 4. 4. 19:12
복사https://blog.naver.com/chartist/30106017230
번역하기
 
전용뷰어 보기
//전략 내용은 인베스트라 p252참조

// atlimit전략이므로 슬리피지는 진입청산 각각 0.05pt 입력후 테스트 해야함

Inputs: RangeLength(5), XAvgLength(50), BarstoEnter(5), Factor(3);
Variables: BuyEntry(0), BuySetup(False), BuyCounter(0), LongExitTarget(0);
Variables: SellEntry(0), SellSetup(False), SellCounter(0), ShortExitTarget(0);

Condition1 = High == Highest(High, RangeLength);
Condition2 = Close > High[2];
Condition3 = ema(Close, XAvgLength) > ema(Close, XAvgLength)[1];
Condition4 = Low == Lowest(Low, RangeLength);
Condition5 = Close < Low[2];
Condition6 = ema(Close, XAvgLength) < ema(Close, XAvgLength)[1];

If MarketPosition <> 1 AND Condition1 AND Condition2 AND Condition3 Then Begin
 BuyEntry = MedianPrice[2];
 BuyCounter = 0;
 BuySetup = True;
 LongExitTarget = High + Factor * (High - BuyEntry);
End;

If BuyCounter > BarstoEnter Then
 BuySetup = False;
Else
 BuyCounter = BuyCounter + 1;

If MarketPosition == 1 Then Begin
 BuySetup = False;
 ExitLong("",atlimit,LongExitTarget);
End;

If BuySetup Then
 Buy("",atlimit,BuyEntry);
 
If MarketPosition <> -1 AND Condition4 AND Condition5 AND Condition6 Then Begin
 SellEntry = MedianPrice[2];
 SellCounter = 0;
 SellSetup = True;
 ShortExitTarget = Low - Factor * (SellEntry - Low);
End;

If SellCounter > BarstoEnter Then
 SellSetup = False;
Else
 SellCounter = SellCounter + 1;

If MarketPosition == -1 Then Begin
 SellSetup = False;
 ExitShort("",atlimit,ShortExitTarget);
End;

If SellSetup Then
 Sell("",atlimit,SellEntry);

# ATR Protective Stop
Inputs: ProtectiveATRs(3);
var : AtrV(0);

AtrV = ATR(30);

If MarketPosition <> 0 Then {
 ExitLong("EL_Protective Stop", atstop, EntryPrice - AtrV*ProtectiveATRs);
 ExitShort("ES_Protective Stop", atstop, EntryPrice + AtrV*ProtectiveATRs);
}
     ***/
    //https://blog.naver.com/chartist/30106017230
    public class HitTheBullsEyeTechTradeModlue: TechTradeModlue
    {
        public override bool buy(StockData stockData, int timeIdx)
        {
            const int TERM = 2;
            List<CandleData> priceTable = stockData.priceTable();
            if (priceTable == null) {
                return false;
            }
            int lastTime = priceTable.Count - TERM;
            if (lastTime < timeIdx) {
                return false;
            }

            double prevLower = priceTable[timeIdx + 1].calc_[(int) EVALUATION_DATA.SMA_BOLLINGER_DOWN];
            double prevPrice = priceTable[timeIdx + 1].centerPrice_;

            double nowLower = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.SMA_BOLLINGER_DOWN];
            double nowCenter = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.SMA_BOLLINGER_CENTER];
            double nowPrice = priceTable[timeIdx].centerPrice_;

            if (nowPrice > nowCenter) {
                //       if (prevPrice <= prevLower) {
                return true;
                //        }
            }

            return false;
        }

        public override bool sell(StockData stockData, int timeIdx)
        {
            const int TERM = 2;
            List<CandleData> priceTable = stockData.priceTable();
            if (priceTable == null) {
                return false;
            }
            int lastTime = priceTable.Count - TERM;
            if (lastTime < timeIdx) {
                return false;
            }

            double prevUpper = priceTable[timeIdx + 1].calc_[(int) EVALUATION_DATA.SMA_BOLLINGER_UP];
            double prevPrice = priceTable[timeIdx + 1].centerPrice_;

            double nowUpper = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.SMA_BOLLINGER_UP];
            double nowCenter = priceTable[timeIdx].calc_[(int) EVALUATION_DATA.SMA_BOLLINGER_CENTER];
            double nowPrice = priceTable[timeIdx].centerPrice_;

            if (nowPrice < nowCenter) {
                //       if (prevPrice >= prevUpper) {
                return true;
                //       }
            }

            return false;
        }
    }

}
