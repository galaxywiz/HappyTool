### 먼저 설치할것들
# python -m pip install --upgrade pip
# conda update -n base conda 
# conda update --all 
# pip install pandas
# pip install pandas-datareader
# pip install dataframe
# pip install schedule

from datetime import datetime
from datetime import timedelta
import time
import schedule

import Bot

# def job(bot):
#     now = time.localtime()
#     current = "%04d-%02d-%02d T%02d:%02d:%02d" % (now.tm_year, now.tm_mon, now.tm_mday, now.tm_hour, now.tm_min, now.tm_sec)
#     print("지금 시간:", str(current))

#     bot.getStockInfo()
#     bot.doPredict()
#     bot.printPredic()

#     print("처리 완료")


# 메인 함수 시작
if __name__ == '__main__':
    bot = Bot.Bot(1000 * 10000)
    bot.getStockInfo()
    bot.doPredict()
    bot.printPredic()

    # schedule.every().day.at("18:00").do(job(bot))

    # while True:
    #     schedule.run_pending()
    #     time.sleep(1)
