using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Telegram.Bot.Types.Enums;

//출처: http://deafjwhong.tistory.com/52?category=675699

//@happy_tool_bot 으로 텔레그램 통지기능 구현
namespace UtilLibrary
{
    public class TelegramBot
    {
        readonly long chatBotID_ = 0;
        readonly string chatBotAPI_ = "";
        private readonly Telegram.Bot.TelegramBotClient bot_ = null;
        readonly Thread sendPoolThread_ = null;

        // 아래의 nuget 패키지 설치가 필요 (하위에는 json만)
        //Newtonsoft.Json
        //TelegramBot
        public TelegramBot(string chatBotAPI, long chatBotId)
        {
            this.chatBotAPI_ = chatBotAPI;
            this.chatBotID_ = chatBotId;
            this.bot_ = new Telegram.Bot.TelegramBotClient(this.chatBotAPI_);
            this.sendPoolThread_ = new Thread(this.run);

            this.setTelegramEvent();
            this.telegramAPIAsync();
        }

        ~TelegramBot()
        {

        }

        public void start()
        {
            this.sendPoolThread_.Start();
        }

        private async void telegramAPIAsync()
        {
            var me = await this.bot_.GetMeAsync();
        }

        private void setTelegramEvent()
        {
            this.bot_.OnMessage += this.botOnMessage;
            this.bot_.OnMessageEdited += this.botOnMessage;
            this.bot_.StartReceiving();
        }

        private async void botOnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            var msg = e.Message;
            if (msg == null || msg.Type != MessageType.Text) {
                return;
            }

            this.doOrder(msg.Text);
        }

        public int messageCount()
        {
            return this.messagePool_.Count;
        }

        readonly List<string> messagePool_ = new List<string>();
        readonly object lockObject_ = new object();

        public void sendMessage(string format, params Object[] args)
        {

            string message = string.Format(format, args);
            this.sendMessage(message);
        }

        public void sendMessage(string message)
        {
            //#if DEBUG
            //#else
            lock (lockObject_) {
                this.messagePool_.Add(message);
            }           
            //#endif
        }

        public async void sendPhoto(string filePath, string message)
        {
            //#if DEBUG
            //                return;
            //#endif

            if (message.Length == 0) {
                return;
            }

            if (filePath.Length == 0) {
                this.sendMessage(message);
                return;
            }

            var fileInfo = new System.IO.FileInfo(filePath);
            if (fileInfo.Exists == false) {
                this.sendMessage(message);
                return;
            }

            try {
                using (var stream = File.Open(filePath, FileMode.Open)) {
                    var rep = await this.bot_.SendPhotoAsync(this.chatBotID_, stream, message);
                }
            }
            catch (Exception e) {
                this.sendMessage(message + e.Message);
            }
        }

        public async void sendFile(string filePath, string message)
        {
            if (message.Length == 0) {
                return;
            }

            if (filePath.Length == 0) {
                this.sendMessage(message);
                return;
            }

            var fileInfo = new System.IO.FileInfo(filePath);
            if (fileInfo.Exists == false) {
                this.sendMessage(message);
                return;
            }

            try {
                using (var stream = File.Open(filePath, FileMode.Open)) {
                    var rep = await this.bot_.SendDocumentAsync(this.chatBotID_, stream, message);
                }
            }
            catch (Exception e) {
                this.sendMessage(message + e.Message);
            }
        }

        public string getMessage()
        {
            string message = "";
            lock (lockObject_) {
                if (this.messagePool_.Count > 0) {
                    if (this.messagePool_[0] != null) {
                        message = this.messagePool_[0];
                        this.messagePool_.RemoveAt(0);
                    }
                }
            }            
            return message;
        }

        // 실제 메시지 보내는곳
        public async void sendTextMessage(string message)
        {
            try {
                await this.bot_.SendTextMessageAsync(this.chatBotID_, message);
            }
            catch (Exception e) {
                string log = string.Format("텔레그램 에러 : {0}\n, -> trace\n{1}\n=== origin message ===\n{2}", e.Message, e.StackTrace, message);
                //MailOffice.getInstance.sendMessage(log);
            }
        }

        public virtual void doOrder(string message)
        {
        }

        async void run()
        {
            while (true) {
                string msg = this.getMessage();
                if (msg.Length > 0) {
                    this.sendTextMessage(msg);
                }
                Thread.Sleep(3000);
            }
        }
    }
}