using HappyTool.Stock.Calculate;
using HappyTool.Util;
using System;
using System.Collections.Generic;

namespace HappyTool.Stock.TradeModules {

    enum TRADE_MODULE_TYPE
    {
        볼린저_CCI,
        볼린저_HECT,
        기술_평가,
    };

    class TradeModule
    {
        public int stockCode_;
        public PRICE_TYPE priceType_;
        public EvalWeightAverage evalWeightAverage_;

        public void setTradeModule(int code, PRICE_TYPE type)
        {
            stockCode_ = code;
            priceType_ = type;
        }

        StockData getStockData()
        {
            return StockBot.getInstance.stockData(stockCode_);
        }

        List<CandleData> priceTable()
        {
            StockData stockData = this.getStockData();
            return stockData.priceTable(priceType_);
        }

        internal void setEvalWeightAverage(EvalWeightAverage evalWeightAverage)
        {
            evalWeightAverage_ = evalWeightAverage;
        }
        //------------------------------------------------------------------------//
        // 여기서 사고 파는 전략을 수립하자.
        public virtual bool buy(int timeIdx)
        {
            //TradeModuleStrategy strategy = null;
            //strategy = new BnfTradeModuleStrategy(this);       //56% 성공
            //strategy = new BollingerTradeModuleStrategyByHETC(this);   //25% 성공 (오히려 1시간 봉에서 승율이 높음
            ////strategy = new BollingerTradeModuleStrategy2(this);    //44% 성공율
            //if (strategy.buy(timeIdx) == false) {
            //    return false;
            //}
            return true;
        }

        public virtual bool sell(int timeIdx, double nowProfit)
        {
            //TradeModuleStrategy strategy = null;
            //strategy = new BnfTradeModuleStrategy(this);
            //strategy = new BollingerTradeModuleStrategyByHETC(this);
            ////strategy = new BollingerTradeModuleStrategy2(this);
            //if (strategy.sell(timeIdx, nowProfit) == false) {
            //    return false;
            //}
            return true;
        }

        //------------------------------------------------------------------------//
        // 현재 살때, 과거 기록상 승률이 있는가?
        public void doTest()
        {
            List<CandleData> priceTable = this.priceTable();
            if (priceTable == null) {
                return;
            }
            StockData stockData = this.getStockData();
            // 1년전 100만원을 투자한다 했을때,
            // 현재 조건으로 사고, 위 아래든 profit % 되면 판다고 할때, 몇번 승부를 보고, 현재 얼마인지 승률을 본다.
            double money = PublicVar.TradeModuleInitMoney;                // 현금 변화량
            int buyPrice = 0;                       // 살때 가격
            int buyCount = 0;                       // 몇개 샀는가?

            bool buyFlag = false;                   // 현재 모드
            int count = 0;                          // 몇번 샀는가? (이거 기록하는게 좋을듯)

            //double strikeRate = 0.0f;             // 승율
            BackTestHistory history = stockData.backTestHistory(priceType_);

            int lastTime = priceTable.Count - 1;

            for (int timeIdx = lastTime; timeIdx >= 0; timeIdx--) {
                int price = priceTable[timeIdx].price_;

                if (buyFlag == false) {
                    if (this.buy(timeIdx)) {
                        //이 시점에 산다.
                        buyPrice = price;
                        buyCount = (int) (money / price);
                        buyFlag = true;
                        count++;

                        history.addBuy(stockData.code_, timeIdx, priceTable[timeIdx].date_, price, buyCount);
                    }
                } else {
                    double nowProfit = (double) (price - buyPrice) / (double) price;
                    if (this.sell(timeIdx, nowProfit)) {
                        money = buyCount * price;
                        buyFlag = false;
                        history.addSell(stockData.code_, timeIdx, priceTable[timeIdx].date_, price, buyCount);
                    }

                }
            }
            // 마지막에 사있으면, 이걸 지금은 얼만지 본다.
            if (buyFlag) {
                const int NOW_DAY_IDX = 0;   // 배열상 0번에 오늘 가격 데이터가 들어가 있음.       

                int price = priceTable[NOW_DAY_IDX].price_;
                money = buyCount * price;
                history.addSell(stockData.code_, NOW_DAY_IDX, priceTable[NOW_DAY_IDX].date_, price, buyCount);
            }

            history.calcEval();
        }

        public DateTime firstTradeDate()
        {
            List<CandleData> priceTable = this.priceTable();
            if (priceTable == null) {
                return DateTime.Now;
            }
            StockData stockData = this.getStockData();
            BackTestHistory history = stockData.backTestHistory(priceType_);
            return history.firstTradeDate();
        }

        // 사용 전략 이걸 가지고 그래프 그려야 함.
        // 그래프 종류 추가되고 그걸로 전략을 그릴때 이걸 처리 해야 함.
        public bool useEma_ = false;
        public bool useMacd_ = false;
        public bool useBollinger_ = false;
        public bool useBnf_ = false;
    }

    //---------------------------------------------------------------------------------
    // 성공율이 높은건 이렇게 빼놓는다.
    class EvaluationTradeModule :TradeModule {

        // 여기서 사고 파는 전략을 수립하자.
        public override bool buy(int timeIdx)
        {
            StockTradeStrategy strategy = null;
            {
                strategy = new EvaluationStockTradeStrategy(this);   //% 성공
                if (strategy.buy(timeIdx) == false) {
                    return false;
                }
            }
            return true;
        }

        public override bool sell(int timeIdx, double nowProfit)
        {
            StockTradeStrategy strategy = null;
            {
                strategy = new EvaluationStockTradeStrategy(this);   //% 성공
                if (strategy.sell(timeIdx, nowProfit) == false) {
                    return false;
                }
            }
            return true;
        }
    }

    //-----------------------------------------------------------------------
    // 박스권일땐, 볼린저 상하단 + CCI로 64%승율
    //https://www.forexliga.kr/forum/class-river/%EB%B0%95%EC%8A%A4%EA%B6%8C%EB%A7%A4%EB%A7%A4%EA%B8%B0%EB%B2%95-%EB%B3%BC%EB%A6%B0%EC%A0%80%EB%B0%B4%EB%93%9C-cci%EC%A7%80%ED%91%9C-17617
    class BollengerCCITradeModule :TradeModule {

        // 여기서 사고 파는 전략을 수립하자.
        public override bool buy(int timeIdx)
        {
            StockTradeStrategy strategy = null;
            {
                strategy = new BollingerStockTradeStrategy(this);   //60% 성공
                if (strategy.buy(timeIdx) == false) {
                    return false;
                }
                //strategy = new CCITradeModuleStrategy(this);
                //if (strategy.buy(timeIdx) == false) {
                //    return false;
                //}
            }
            //strategy = new BnfTradeModuleStrategy(this);       //56% 성공
            //strategy = new BollingerTradeModuleStrategyByHETC(this);   //25% 성공 (오히려 1시간 봉에서 승율이 높음
            ////strategy = new BollingerTradeModuleStrategy2(this);    //44% 성공율
            //if (strategy.buy(timeIdx) == false) {
            //    return false;
            //}
            return true;
        }

        public override bool sell(int timeIdx, double nowProfit)
        {
            StockTradeStrategy strategy = null;
            {
                // 박스권일땐, 볼린저 상하단 + CCI로 64%승율
                strategy = new BollingerStockTradeStrategy(this);   //60% 성공
                if (strategy.sell(timeIdx, nowProfit) == false) {
                    return false;
                }
                strategy = new CCIStockTradeStrategy(this);
                if (strategy.sell(timeIdx, nowProfit) == false) {
                    return false;
                }
            }
            //strategy = new BnfTradeModuleStrategy(this);
            //strategy = new BollingerTradeModuleStrategyByHETC(this);
            ////strategy = new BollingerTradeModuleStrategy2(this);
            //if (strategy.sell(timeIdx, nowProfit) == false) {
            //    return false;
            //}
            return true;
        }
    }
}