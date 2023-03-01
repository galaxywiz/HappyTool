import pandas as pd 
import dataframe
import sqlite3
import datetime

class dayPriceDB:
    # DB 폴더가 준비 되어 있어여함.	
    def __init__(self, dbName):
        self.conn_ = sqlite3.connect('./DB/' + dbName)

    def __tableName(self, code):
        name = "DayPriceTable_" + code
        return name
    
    #----------------------------------------------------------#
    # 테이블이 있는지 확인하고, 있으면 -1, 없으면 0, 생성했으면 1
    def getTable(self, tableName):
        if self.checkTable(tableName) == False:
            if self.createTable(tableName) == False:
                return 0
            else:
                return 1
        return -1
    #----------------------------------------------------------#

    # 테이블 이름이 있는지 확인
    def checkTable(self, code):
        tableName = self.__tableName(code)
        with self.conn_:
            cur = self.conn_.cursor()
            sql = "SELECT count(*) FROM sqlite_master WHERE Name = \'%s\'" % tableName
            cur.execute(sql)
            rows = cur.fetchall()
            for row in rows:          
                if str(row[0]) == "1":
                    return True
            return False
    
    # 테이블 생성
    def createTable(self, code):
        tableName = self.__tableName(code)
        with self.conn_:
            try:
                cur = self.conn_.cursor()
                sql = "CREATE TABLE %s (candleTime DATETIME PRIMARY KEY, start INT, high INT, low INT, close INT, vol INT);" % tableName
                cur.execute(sql)
                return True
            except:
                log = "! [%s] table make fail" % tableName 
                print(log)
                return False
    
    # 데이터 저장
    def save(self, code, dataframe):    
        tableName = self.__tableName(code)
        with self.conn_:
            try:
                cur = self.conn_.cursor()
                sql = "INSERT OR REPLACE INTO \'%s\'" % tableName
                sql = sql + " ('candleTime', 'start', 'high', 'low', 'close', 'vol') VALUES(?, ?, ?, ?, ?, ?)"
                cur.executemany(sql, dataframe.values)    
                self.conn_.commit()
            except:
                return None
            
    # 데이터 로드
    def load(self, code):
        tableName = self.__tableName(code)
        with self.conn_:
            try:                
                sql = "SELECT candleTime, start, high, low, close, vol FROM \'%s\' ORDER BY candleTime ASC" % tableName
                df = pd.read_sql(sql, self.conn_, index_col=None)
                if len(df) == 0:
                    return False, None
            except:
                return False, None

        return True, df     
