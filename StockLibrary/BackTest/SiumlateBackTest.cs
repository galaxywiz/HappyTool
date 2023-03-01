using System;
using System.Collections.Generic;
using UtilLibrary;

namespace StockLibrary
{
    public class SiumlateBackTest
    {
        readonly Bot bot_;

        public SiumlateBackTest(Bot bot)
        {
            this.bot_ = bot;
        }

        double money_ = 0;
        DateTime tradeStartTime_ = DateTime.Now;
        readonly int INIT_MONEY = PublicVar.simulateInitMoney;

        int getLastTimeIdxInStockPool()
        {
            const int LIMIT_TABLE_COUNT = 120;
            int lastTime = 0;

            StockDataEach eachDo = (string code, StockData stockData) => {
                List<CandleData> priceTable = stockData.priceTable();
                if (priceTable == null) {
                    return;
                }
                // 120개 이상은 있어야 함.
                if (priceTable.Count < LIMIT_TABLE_COUNT) {
                    return;
                }
                lastTime = Math.Max(priceTable.Count - 1, lastTime);
            };
            this.bot_.doStocks(eachDo);

            if (lastTime < LIMIT_TABLE_COUNT) {
                return 0;
            }
            return lastTime;
        }

        //로드한 주식풀을 이용해서 전체 백테스팅을 해본다. bot 매매 기준을 처리, false 할시 주식이 갖고 있는 매매로 시뮬
        //SimulateBackTestRecoder doSimulate()
        //{
        //    int lastTime = this.getLastTimeIdxInStockPool();
        //    if (lastTime == 0) {
        //        return null;
        //    }
        //    int backTestSuccess = 0;

        //    this.money_ = this.INIT_MONEY;
        //    int maxBuyCount = 100;
        //    string[] buyStockCode = new string[maxBuyCount];
        //    double[] buyPrice = new double[maxBuyCount];
        //    int[] buyCount = new int[maxBuyCount];
        //    DateTime[] buyDateTime = new DateTime[maxBuyCount];

        //    for (int i = 0; i < maxBuyCount; ++i) {
        //        buyStockCode[i] = "";
        //        buyPrice[i] = 0;
        //        buyCount[i] = 0;
        //    }

        //    SimulateBackTestRecoder backTestRecorder = new SimulateBackTestRecoder();
        //    for (int stockTimeIdx = lastTime; stockTimeIdx >= 0; --stockTimeIdx) {
        //        DateTime standTime = new DateTime(2000, 1, 1);

        //        StockDataEach eachDo = (string code, StockData stockData) => {
        //            List<CandleData> priceTable = stockData.priceTable();
        //            if (priceTable == null) {
        //                return;
        //            }
        //            if (priceTable.Count <= stockTimeIdx) {
        //                return;
        //            }
        //            DateTime dateTime = priceTable[stockTimeIdx].date_;

        //            int timeIdx = stockTimeIdx;
        //            if (standTime.Year == 2000) {
        //                standTime = dateTime;
        //            }
        //            else {
        //                if (standTime > dateTime) {
        //                    int idx = Math.Min(priceTable.Count - 1, lastTime - 1);
        //                    for (; idx >= 0; --idx) {
        //                        DateTime dt = priceTable[idx].date_;
        //                        if (standTime == dt) {
        //                            break;
        //                        }
        //                    }

        //                    if (idx <= 0) {
        //                        return;
        //                    }
        //                    timeIdx = idx;
        //                }
        //                else if (standTime < dateTime) {
        //                    return;
        //                }
        //            }
        //            dateTime = priceTable[timeIdx].date_;
        //            if (dateTime != standTime) {
        //                Logger.getInstance.print(KiwoomCode.Log.에러, "백테스팅 에러 {0}, {1}date", stockData.name_, dateTime);
        //                return;
        //            }
        //            double price = priceTable[timeIdx].price_;

        //            // 파는걸 먼저 처리함.
        //            for (int i = 0; i < maxBuyCount; ++i) {
        //                if (buyStockCode[i] == code) {
        //                    double nowProfit = (price - buyPrice[i]) / price;

        //                    if (stockData.isSellTime(this.bot_, buyPrice[i], timeIdx)) {
        //                        this.money_ = this.money_ + (buyCount[i] * price);
        //                        this.money_ = this.money_ - (this.money_ * PublicVar.tradeTax);    // 세금 제거
        //                        buyStockCode[i] = "";
        //                        backTestRecorder.addTrade(code, timeIdx, buyDateTime[i], dateTime, buyPrice[i], price, buyCount[i]);
        //                        if (nowProfit > 0.0f) {
        //                            backTestSuccess++;
        //                        }
        //                        break;
        //                    }
        //                }
        //            }

        //            int buyIdx = -1;
        //            for (int i = 0; i < maxBuyCount; ++i) {
        //                if (buyStockCode[i].Length == 0) {
        //                    buyIdx = i;
        //                    break;
        //                }
        //            }
        //            int remainBuy = 0;
        //            int buyedCount = 0;
        //            for (int i = 0; i < maxBuyCount; ++i) {
        //                if (buyStockCode[i].Length == 0) {
        //                    remainBuy++;
        //                }
        //                else {
        //                    buyedCount++;
        //                }
        //            }

        //            // 살자리가 있으면
        //            if (buyIdx != -1) {
        //                int buyAble = 0;
        //                if (this.bot_.calcBuyAbleCount(this.money_, price, buyedCount, out buyAble) == false) {
        //                    return;
        //                }
        //                if (stockData.isBuyTime(this.bot_, timeIdx)) {
        //                    buyPrice[buyIdx] = price;
        //                    buyCount[buyIdx] = buyAble;
        //                    this.money_ = this.money_ - (buyPrice[buyIdx] * buyCount[buyIdx]);
        //                    buyStockCode[buyIdx] = code;
        //                    buyDateTime[buyIdx] = dateTime;

        //                    if (dateTime < this.tradeStartTime_) {
        //                        this.tradeStartTime_ = dateTime;
        //                    }
        //                }
        //            }
        //        };
        //        this.bot_.doStocks(eachDo);
        //    }

        //    // 지금까지 보유중인 주식에 대해선 현재 값어치를 평가해 준다.
        //    for (int i = 0; i < maxBuyCount; ++i) {
        //        if (buyStockCode[i].Length != 0) {
        //            const int NOW_DAY_IDX = 0;   // 배열상 0번에 오늘 가격 데이터가 들어가 있음.       
        //            StockData stockData = this.bot_.getStockDataCode(buyStockCode[i]);
        //            List<CandleData> priceTable = stockData.priceTable();
        //            if (priceTable == null) {
        //                continue;
        //            }
        //            double price = priceTable[NOW_DAY_IDX].price_;
        //            DateTime dateTime = priceTable[NOW_DAY_IDX].date_;

        //            this.money_ = this.money_ + (buyCount[i] * price);
        //            this.money_ = this.money_ - (this.money_ * PublicVar.tradeTax);    // 세금 제거
        //            backTestRecorder.addTrade(buyStockCode[i], 0, buyDateTime[i], dateTime, buyPrice[i], price, buyCount[i]);
        //            buyStockCode[i] = "";

        //            double nowProfit = (price - buyPrice[i]) / price;
        //            if (nowProfit > 0.0f) {
        //                backTestSuccess++;
        //            }
        //        }
        //    }
        //    backTestRecorder.calcEval(this.money_, this.INIT_MONEY);
        //    backTestRecorder.avgProfitRate_ = UtilLibrary.Util.calcProfitRate(this.INIT_MONEY, this.money_);
        //    backTestRecorder.winCount_ = backTestSuccess;
        //    return backTestRecorder;
        //}

        //public SimulateBackTestRecoder run(out string log)
        //{
        //    var result = this.doSimulate();
        //    if (result == null) {
        //        log = "";
        //        return null;
        //    }
        //    log = string.Format("*** 주식 시뮬레이딩 ***\n");
        //    log += string.Format(" => 분봉 : {0}\n", this.bot_.priceTypeMin());
        //    log += string.Format(" => 매매시작 : {0}\n", result.firstTradeDate());
        //    log += string.Format(" => 매매 횟수 : {0}\n", result.tradeCount_);
        //    log += string.Format(" => 승리 횟수 : {0}, 승율 {1:#0.##}%\n", result.winCount_, result.winRate());
        //    log += string.Format(" => 결과 : 자산 {0:#,###} -> {1:#,###}, 이익율 {2:#0.##}%", this.INIT_MONEY, this.money_, result.avgProfitRate_ * 100);

        //    result.log_ = log;
        //    return result;
        //}
    }
}
