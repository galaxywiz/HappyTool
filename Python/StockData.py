# talib 를 위해 아나콘다 설치 필요
# pip install .\TA_Lib-0.4.17-cp37-cp37m-win_amd64.whl
# OR
# conda install -c quantopian ta-lib 
# conda install -c masdeseiscaracteres ta-lib
# conda install -c developer ta-lib
# OR
# pip install ta-lib
# 결국 vscode 에서 아나콘다의 파이썬으로 pip install ta-lib 하니까 됨.

from enum import Enum
import os

import talib
import talib.abstract as ta
from talib import MA_Type
import dataframe

import pandas as pd 
import numpy as np

import Util

class BuyState (Enum):
    없음 = 0
    매수 = 1
    매도 = 2

class stockData:
    buyCount_ = 0
    buyPrice_ = 0
    position_ = BuyState.없음
    predicPrice_ = 0

    def __init__(self, code, name, dataFrame):
        self.code_ = code
        self.name_ = name
        self.indicators_ = dataFrame

    def canPredic(self):
        size = len(self.indicators_)
        if size < 300:
            return False
        return True

    def calcPredicRate(self):
        if self.canPredic() == False:
            return 0
        
        nowPrice = self.candle0()
        rate = Util.calcRate(nowPrice["close"], self.predicPrice_)
        return rate

    # 지금 캔들(갱신될 수 있음)
    def candle0(self):
        rowCnt = self.indicators_.shape[0]
        if rowCnt == 0:
            return None
        return self.indicators_.iloc[-1]

    # 완전히 완성된 캔들 (고정된 가장 최신 캔들)
    def candle1(self):
        rowCnt = self.indicators_.shape[0]
        if rowCnt < 1:
            return None
        return self.indicators_.iloc[-2]

    # 완성된 캔들의 직전 캔들 (지표간 cross 등 판단을 위함.)
    def candle2(self):
        rowCnt = self.indicators_.shape[0]
        if rowCnt < 2:
            return None
        return self.indicators_.iloc[-3]

    def calcProfit(self):
        if self.buyCount_ == 0:
            return 0
     
        profit = self.buyCount_ * self.buyPrice_
        return profit    

    # 각종 보조지표, 기술지표 계산
    def calcIndicator(self):        
        arrClose = np.asarray(self.indicators_["close"], dtype='f8')
        arrHigh = np.asarray(self.indicators_["high"], dtype='f8')
        arrLow = np.asarray(self.indicators_["low"], dtype='f8')
     
        # 이평선 계산
        self.indicators_["sma5"] = ta._ta_lib.SMA(arrClose, 5)
        self.indicators_["sma10"] = ta._ta_lib.SMA(arrClose, 10)
        self.indicators_["sma20"] = ta._ta_lib.SMA(arrClose, 20)
        self.indicators_["sma50"] = ta._ta_lib.SMA(arrClose, 50)
        self.indicators_["sma100"] = ta._ta_lib.SMA(arrClose, 100)
        self.indicators_["sma200"] = ta._ta_lib.SMA(arrClose, 200)

        self.indicators_["ema5"] = ta._ta_lib.EMA(arrClose, 5)
        self.indicators_["ema10"] = ta._ta_lib.EMA(arrClose, 10)
        self.indicators_["ema20"] = ta._ta_lib.EMA(arrClose, 20)
        self.indicators_["ema50"] = ta._ta_lib.EMA(arrClose, 50)
        self.indicators_["ema100"] = ta._ta_lib.EMA(arrClose, 100)
        self.indicators_["ema200"] = ta._ta_lib.EMA(arrClose, 200)

        self.indicators_["wma5"] = ta._ta_lib.WMA(arrClose, 5)
        self.indicators_["wma10"] = ta._ta_lib.WMA(arrClose, 10)
        self.indicators_["wma20"] = ta._ta_lib.WMA(arrClose, 20)
        self.indicators_["wma50"] = ta._ta_lib.WMA(arrClose, 50)
        self.indicators_["wma100"] = ta._ta_lib.WMA(arrClose, 100)
        self.indicators_["wma200"] = ta._ta_lib.WMA(arrClose, 200)
  
        #볼린저 계산
        upper, middle, low = ta._ta_lib.BBANDS(arrClose, 20, 2, 2, matype=MA_Type.SMA)
        self.indicators_["bbandUp"] = upper
        self.indicators_["bbandMid"] = middle
        self.indicators_["bbandLow"] = low

        # 기타 자주 사용되는 것들
        self.indicators_["rsi"] = ta._ta_lib.RSI(arrClose, 14)
        self.indicators_["cci"] = ta._ta_lib.CCI(arrHigh, arrLow, arrClose, 14)
        self.indicators_["williumR"] = ta._ta_lib.WILLR(arrHigh, arrLow, arrClose, 14)
        self.indicators_["parabol"] = ta._ta_lib.VAR(arrClose, 5, 1)
        self.indicators_["adx"]  = ta._ta_lib.ADX(arrHigh, arrLow, arrClose, 14)
        self.indicators_["plusDI"]  = ta._ta_lib.PLUS_DI(arrHigh, arrLow, arrClose, 14)
        self.indicators_["plusDM"]  = ta._ta_lib.PLUS_DM(arrHigh, arrLow, 14)
       
        self.indicators_["atr"] = ta._ta_lib.ATR(arrHigh, arrLow, arrClose, 30)
        