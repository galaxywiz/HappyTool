# 인베스팅 데이터 갖고오기
# https://pypi.org/project/investpy/
# pip install investpy==0.9.12

# 구글에서 갖고오기는 막힌듯... 야후에서 갖고와야...
# https://finance.yahoo.com/quote/CL=F?p=CL=F

import pandas as pd 
from pandas import Series, DataFrame
from pandas_datareader import data as web
import investpy
import numpy as np
import dataframe

from datetime import datetime
from datetime import timedelta

#----------------------------------------------------------#
# 주식 목록 갖고오기 (상위)
class stockCrawler:
    def getKoreaStocksFromWeb(self):
        # 한국 주식 회사 등록 정보 가지고 오기
        stockDf = pd.read_html('http://kind.krx.co.kr/corpgeneral/corpList.do?method=download&searchType=13', header=0)[0]
        stockDf.종목코드 = stockDf.종목코드.map('{:06d}'.format)
        stockDf = stockDf[['회사명', '종목코드']] 
        stockDf = stockDf.rename(columns={'회사명': 'name', '종목코드': 'code'})
        return stockDf

    def getKoreaStocksFromFile(self):
        with open("./targetList.txt", "r", encoding="utf-8") as f:
            targetList = f.read().splitlines()
        
        stockDf = DataFrame(columns = ("name", "code"))
        for text in targetList:
            tokens = text.split(':')
            row = DataFrame(data=[tokens], columns=["name", "code"])
            stockDf = stockDf.append(row)
            stockDf = stockDf.reset_index(drop=True)
        return stockDf

#----------------------------------------------------------#
### 구글이 안되니 아후에서 긁자.
class yahooGetter(stockCrawler):
    def getKoreaStockData(self, ticker, loadDays):
        ticker = ticker + ".KS"
        
        oldDate = datetime.now() - timedelta(days=loadDays)
        fromDate = oldDate.strftime("%Y-%m-%d")

        df = web.DataReader(ticker, start = fromDate, data_source='yahoo')
        
        df.reset_index(inplace=True, drop=False)
        df.rename(columns={'Date': 'candleTime', 'High': 'high', 'Low': 'low', 'Open': 'start', 'Close': 'close', 'Volume' : 'vol'}, inplace = True)
        df['candleTime'] = df['candleTime'].dt.strftime('%Y.%m.%d')
        stockDf = pd.DataFrame(df, columns=['candleTime', 'start', 'high', 'low', 'close', 'vol'])
        
        print(stockDf)
        return stockDf
    
    # df = getter.getStockData('TSLA', 1000)
    def getStockData(self, ticker, loadDays):
        oldDate = datetime.now() - timedelta(days=loadDays)
        fromDate = oldDate.strftime("%Y-%m-%d")

        df = web.DataReader(ticker, start = fromDate, data_source='yahoo')
        
        df.reset_index(inplace=True, drop=False)
        df['candleTime'] = df['candleTime'].dt.strftime('%Y.%m.%d')
        df.rename(columns={'Date': 'candleTime', 'High': 'high', 'Low': 'low', 'Open': 'start', 'Close': 'close', 'Volume' : 'vol'}, inplace = True)
        del df['Adj Close']
        return df
  
    # 선물은 안되나봄.
    def getFuturesData(self, ticker):
        oldDate = datetime.now() - timedelta(days=356)
        fromDate = oldDate.strftime("%Y-%m-%d")
        df = web.DataReader(ticker, start = fromDate, data_source='yahoo')
        return df

#----------------------------------------------------------#
# 만약을 위한 백업
class investGetter(stockCrawler):
    def getKoreaStockData(self, ticker, loadDays):
        now = datetime.now()
        oldDate = now - timedelta(days=loadDays)
        fromDate = oldDate.strftime("%Y/%m/%d")
        toDate = now.strftime("%Y/%m/%d")
        #df = investpy.get_stock_recent_data(stock=ticker, country="South Korea")
        df = investpy.get_stock_historical_data(stock=ticker, country="South Korea", from_date=fromDate, to_date=toDate)
        del df['Adj Close']
        return df

#----------------------------------------------------------#
# 이건 etf / etn 검색용
class naverGetter(stockCrawler):
    def getKoreaStocksFromWeb(self):
        # 한국 주식 회사 등록 정보 가지고 오기
        stockDf = pd.read_html('http://kind.krx.co.kr/corpgeneral/corpList.do?method=download&searchType=13', header=0)[0]
        stockDf.종목코드 = stockDf.종목코드.map('{:06d}'.format)
        stockDf = stockDf[['회사명', '종목코드']] 
        stockDf = stockDf.rename(columns={'회사명': 'name', '종목코드': 'code'})
        return stockDf

    def __getNaverURLCode(self, code):    
        url = 'http://finance.naver.com/item/sise_day.nhn?code={code}'.format(code=code)
        print("요청 URL = {}".format(url))
        return url

    # 종목 이름을 입력하면 종목에 해당하는 코드를 불러와 
    def getNaverStockURL(self, item_name, stockDf):
        code = stockDf.query("name=='{}'".format(item_name))['code'].to_string(index=False)
        url = self.__getNaverURLCode(code)
        return url

    def getKoreaStockData(self, ticker, loadDays):
        # 일자 데이터를 담을 df라는 DataFrame 정의
        df = pd.DataFrame()
        try:
            url = self.__getNaverURLCode(ticker)
            loadDays = (loadDays / 10) + 1
            # 1페이지가 10일. 100페이지 = 1000일 데이터만 가져오기 
            for page in range(1, int(loadDays)):
                pageURL = '{url}&page={page}'.format(url=url, page=page)
                df = df.append(pd.read_html(pageURL, header=0)[0], ignore_index=True)
            
            # df.dropna()를 이용해 결측값 있는 행 제거 
            df = df.dropna()
            stockDf = pd.DataFrame(df, columns=['날짜', '시가', '고가', '저가', '종가', '거래량'])
            stockDf.rename(columns={'날짜': 'candleTime', '고가': 'high', '저가': 'low', '시가': 'start', '종가': 'close', '거래량' : 'vol'}, inplace = True)

            print(stockDf)
            return stockDf
        except:
            return None