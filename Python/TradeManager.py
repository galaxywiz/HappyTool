import Bot

class TradManager:
    bot_ = None
    
    def __init__(self, bot):
        bot_ = bot

    def buy(self, stockData):
        return False

    def sell(self, stockData):
        return False

    def processMonitor(self, stockData):
        return False

    def processPayOff(self, stockData):
        return False