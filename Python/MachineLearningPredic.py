# 딥러닝에 필요한 것들
# pip install tensorflow
# pip install keras
# pip install matplotlib
# pip install astroid==2.2.5 
# pip install pylint==2.3.1
# conda update tensorflow
#http://www.kwangsiklee.com/2019/03/keras%EC%97%90%EC%84%9C-%EB%AA%A8%EB%8D%B8-saveload%ED%95%98%EA%B8%B0/

import os
import pandas as pd 
import numpy as np
import matplotlib.pyplot as plt
import mpl_finance
import matplotlib.ticker as ticker

import tensorflow as tf
from keras.models import Sequential
from keras.layers import LSTM, Dropout, Dense, Activation
from keras.models import model_from_json
import datetime
import StockData


# 여기 참고해서 수정할것
#https://yjucho1.github.io/tensorflow/tensorflow-serving/
class MachinePredic:
    def __init__(self, stockData, seqLen, predicDay = 1): 
        self.stockData_ = stockData
        self.seqLen_ = seqLen
        self.predicDay_ = predicDay

    def modelName(self):
        name = "model_[%s]" % (self.stockData_.code_)
        return name

    def makeWindows(self):
        priceTable = self.stockData_.indicators_
        high = priceTable['high'].values
        low = priceTable['low'].values
        close = priceTable['close'].values
        mid = (high + low + close) / 3
        sequenceLen = self.seqLen_ + self.predicDay_ 
        
        result = []
        for index in range(len(mid) - sequenceLen):
            result.append(mid[index: index + sequenceLen])
        return result

    # 데이터를 정규화 시킴
    #  시간  0      1     2
    #   1. 윈도0, 윈도1, 윈도2, ...
    #   2. 윈도0, 윈도1, 윈도2, ...
    def makeNormalize(self, windows):
        normalizedData = []
        for window in windows:
            normalizedWindow = [((float(p) / float(window[0])) - 1) for p in window]
            normalizedData.append(normalizedWindow)

        normalizes = np.array(normalizedData)        
        return normalizes

    def inverseNormalize(self, normalList, row):
        priceTable = self.stockData_.indicators_
        arrClose = np.asarray(priceTable["close"], dtype='f8')
        arrHigh = np.asarray(priceTable["high"], dtype='f8')
        arrLow = np.asarray(priceTable["low"], dtype='f8')

        close = arrClose[row]
        high = arrHigh[row]
        low = arrLow[row]
        mid = (close + high + low) / 3

        inverse = []
        for normal in normalList:
            var = normal * mid + mid
            inverse.append(var)
        return inverse

    def makeTestData(self, row, normalizes):
        xTest = normalizes[row:, :-1]
        xTest = np.reshape(xTest, (xTest.shape[0], xTest.shape[1], self.predicDay_))

        yTest = normalizes[row:, -1]   
        return xTest, yTest

    def trainModel(self, row, normalizes, xTest, yTest):
        # split train and test data
        train = normalizes[:row, :]
        np.random.shuffle(train)
        
        xTrain = train[:, :-1]
        xTrain = np.reshape(xTrain, (xTrain.shape[0], xTrain.shape[1], self.predicDay_))
        yTrain = train[:, -1]
        #xTrain.shape

        model = tf.keras.models.Sequential()
        model.add(tf.keras.layers.LSTM(self.seqLen_, return_sequences=True, input_shape=(self.seqLen_, self.predicDay_)))
        model.add(tf.keras.layers.LSTM(256, return_sequences=False))
        model.add(tf.keras.layers.Dense(1, activation='linear'))
        model.compile(loss='mse', optimizer=tf.keras.optimizers.RMSprop(clipvalue=1.0))
        model.summary()
        
        model.fit(xTrain, yTrain, validation_data=(xTest, yTest), batch_size=10, epochs=20)
        return model

    def loadModel(self):
        savePath = "save/%s.h5" % (self.modelName())
        if os.path.isfile(savePath) == True:
            model = tf.keras.models.load_model(savePath)
            model.summary()
            return model

        return None

    def saveModel(self, model):
        savePath = "save/%s.h5" % (self.modelName())
        model.save(savePath)                

    def calcSlope(self, prev, now):
        slope = (prev - now) / (0 - 1)
        return slope

    def printFigure(self, yTest, pred):
        lastDay = 28
        yTest = yTest[-lastDay:]
        pred = pred[-lastDay:]

        fig = plt.figure(facecolor='white', figsize=(20, 10))
        fontTitle = {'family': 'D2Coding', 'size': 24,'color':  'black'}
        plt.title(self.stockData_.name_, fontdict=fontTitle)
        ax = fig.add_subplot(111)

        ax.plot(yTest, label='True')
        ax.plot(pred, label='Prediction')
        ax.legend()
        plt.grid()

        figFile = "flg_%s.png" % (self.stockData_.code_)
        plt.savefig("static/" + figFile, dpi=300)
        
        #price = pred[-1]
        #print("$ 차트 갱신 [%s] 내일 아마도 [%f]원 => [%s]" % (self.stockData_.name_, price, filename))
        return figFile

    def predic(self, reflush = False):
        windows = self.makeWindows()
        normalizes = self.makeNormalize(windows)
        row = int(round(normalizes.shape[0] * 0.7))
        xTest, yTest = self.makeTestData(row, normalizes)
                
        model = self.loadModel()
        if model == None or reflush :
            model = self.trainModel(row, normalizes, xTest, yTest)
            self.saveModel(model)
        
        pred = model.predict(xTest)
        slope = self.calcSlope(pred[-2], pred[-1])
        inverseTest = self.inverseNormalize(yTest, row)
        inversePred = self.inverseNormalize(pred, row)

        figName = self.printFigure(inverseTest, inversePred)
        return figName, slope
        