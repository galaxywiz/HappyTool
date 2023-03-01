import pandas as pd 
from pandas import Series, DataFrame
import numpy as np
import dataframe
import sqlite3
import datetime
import Bot
from enum import Enum

class BackTestType (Enum):
    OneItem = 1
    AllItem = 2

class BackTester:
    def __init__(self, startTime, endTime, type):
        self.startTime_ = startTime
        self.endTime_ = endTime
        self.type_ = type

#데이터 로드
    def loadData(self):
        return None
#프로세스 진행
    def process(self):
        return None
#결과 취합
    def getResult(self):
        return None

#결과 출력 
    def printResult(self):
        return None

    def run(self):
        return None