import pandas as pd 
from pandas import Series, DataFrame
import numpy as np
import dataframe
from datetime import datetime
from datetime import timedelta
import sqlite3
import datetime

import StockCrawler
import SqliteStockDB
import StockData
import StockPredic
import Telegram
import Util
import locale

class Bot:
    stockPool_ = {}
    accountMoney_ = 0
    dayPriceDB_ = None

    def __init__(self, money):
        self.accountMoney_ = money
        self.__initStockDB()
        locale.setlocale(locale.LC_ALL, '') 

    def addStockData(self, stockData):
        self.stockPool_[stockData.name_] = stockData

    def removeStockData(self, name):
        del self.stockPool_[name]

    def sendMessage(self, log):
        Telegram.TelegramBot.sendMessage(self, log)

    #----------------------------------------------------------#
    def trade(self):
        for stockData in self.stockPool_.values():
            log = "[%s] 매매 처리" % (stockData.name_)
            print(log)

    # api 로 매수 주문
    def buy(self, stockData, count, price):
        stockData.position_ = StockData.BuyState.매수
        stockData.buyPrice_ = price
        stockData.buyCount_ = count
        self.accountMoney_ = self.accountMoney_ - (count * price)
        self.addStockData(stockData)
        log = "[%s]를 매수 하였음" % (stockData.name_)
        #self.sendMessage(log)
        print(log)
        return True

    # api 로 매도 주문
    def sell(self, stockData, count, price):
        stockData.position_ = StockData.BuyState.매도
        stockData.buyPrice_ = price
        stockData.buyCount_ = count
        self.accountMoney_ = self.accountMoney_ - (count * price)
        self.addStockData(stockData)
        log = "[%s]를 매도 하였음" % (stockData.name_)
        #self.sendMessage(log)
        print(log)
        return True

    # api 로 청산 주문
    def payOff(self, stockData):
        price = stockData.nowCandle()["close"]
        self.accountMoney_ = self.accountMoney_ + (stockData.buyCount_ * price)

        stockData.position_ = StockData.BuyState.없음
        stockData.buyPrice_ = 0
        stockData.buyCount_ = 0
        self.removeStockData(stockData)
        log = "[%s]를 청산 하였음" % (stockData.name_)
        #self.sendMessage(log)
        print(log)
        return True

    #----------------------------------------------------------#
    # db 에 데이터 저장 하고 로딩!
    def __initStockDB(self):
        self.dayPriceDB_ = SqliteStockDB.dayPriceDB('KoreaStockData.db')

    def getStockInfo(self):
        # 인베스팅 크롤러
        # getter = StockCrawler.investGetter()
        # df = getter.getKoreaStockData('005930', loadDays)
        # print(df)

        #getter = StockCrawler.yahooGetter()
        #df = getter.getKoreaStockData('005930', 10)
        #df = getter.getFuturesData('NG=F')

        # 네이버 데이터 크롤러
        getter = StockCrawler.naverGetter()
        stockDf = getter.getKoreaStocksFromFile()
        #stockDf = getter.getKoreaStocksFromWeb()    # 웹에 있는 2300여개 종목을 긁어온다.

        # 주식의 일자데이터 크롤링 / db 에서 갖고 오기
        for idxi, rowCode in stockDf.iterrows():
            name = rowCode['name']
            code = rowCode['code']

            self.__getStockInfoFromWeb2DB(getter, name, code)
            self.__loadStockData(name, code)

    def __getStockInfoFromWeb2DB(self, getter, name, code):
          loadDays = 10
          # DB에 데이터가 없으면 테이블을 만듬
          sel = self.dayPriceDB_.getTable(code)
          if sel == 0:
              return None
          elif sel == 1:  # 신규 생성 했으면
              loadDays = 1000

          # 크롤러에게 code 넘기고 넷 데이터 긁어오기
          df = getter.getKoreaStockData(code, loadDays)
          if df is None:
              print("! 주식 [%s] 의 크롤링 실패" % (name))
              return None

          # 데이터 저장
          self.dayPriceDB_.save(code, df)
          print("====== 주식 일봉 데이터 [%s] 저장 완료 =====" % (name))

    def __loadStockData(self, name, code):  
            ret, df = self.dayPriceDB_.load(code)
            if ret == False:
                return None

            sd = StockData.stockData(code, name, df)
            self.stockPool_[name] = sd
            sd.calcIndicator()

    #----------------------------------------------------------#
    # 주식데이터 각각을 예측해봄
    def doPredict(self):
        for name, sd in self.stockPool_.items():
            if sd.canPredic() == False:
                print( "# 주식 [%s]을 예측 자료 부족, (일 데이터 [%d] 개)"  % (name))
                continue
            
            vm = StockPredic.StockPredic(sd)
            figPath, predicPrice = vm.predic("log/")
            sd.predicPrice_ = predicPrice

            #vm = MachineLearningPredic.MachinePredic(sd, 28, 1)
            #vm.predic(True)
            # 여러 전략들을 조합해서 구매에 적합한 전략을 찾기
            
    #----------------------------------------------------------#
    def printPredic(self):
        for name, sd in self.stockPool_.items():
            if sd.canPredic() == False:
                continue
            
            candle = sd.candle0()
            close = format(candle["close"], ",")
            predic = locale.format("%.2f", sd.predicPrice_)
            rate = locale.format("%.2f", Util.calcRate(candle["close"], sd.predicPrice_))
            
            print( "# 주식 [%s]의 [%s] 종가 [%s], 내일은 아마도 [%s], 증감 [%s]"  % (sd.name_, candle['candleTime'], close, predic, rate))
            