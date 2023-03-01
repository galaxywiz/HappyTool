#pip install telepot
import telepot

class TelegramBot:
    token_ = "643993591:AAF8ohY1Yi9lCXuRRRJyTLBa0a7IsUZwRVs"
    id_ = "508897948"
    bot_ = telepot.Bot(token_)

  #  def __init__(self, tokienId):
   #     self.token_ = tokienId
    #    self.bot_ = telepot.Bot(self.token_)

    def sendMessage(self, message):
        self.bot_.sendMessage(self.id_, message)
