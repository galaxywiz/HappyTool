using System.Net;
using System.Net.Mail;

namespace UtilLibrary
{
    //Mail 설정에 대한 클래스... 암호 바꾸면 여기 다시 컴파일 ㅠㅠ
    public class Mailer
    {
        //smtp 서버 설정
        readonly SmtpClient smtp_ = new SmtpClient("smtp.gmail.com", 587);
        readonly string fromMailAddr_ = "happytool84@gmail.com";
        readonly string passwd_ = "HappyTool!Q2w";
        string toMailAddr_;
        string subject_;
        string body_;
        string attacheFile_ = "";

        public Mailer()
        {
            this.smtp_.EnableSsl = true;
            this.smtp_.Credentials = new NetworkCredential(this.fromMailAddr_, this.passwd_);
        }

        public void setToMailAddr(string str)
        {
            this.toMailAddr_ = str;
        }

        public void setSubject(string str)
        {
            this.subject_ = str;
        }

        public void setBody(string str)
        {
            this.body_ = str;
        }

        public void setAttachFile(string str)
        {
            this.attacheFile_ = str;
        }

        public void send()
        {
#if DEBUG
#else
            //메일 내용 쓰기
            MailMessage mail = new MailMessage();

            mail.From = new MailAddress(this.fromMailAddr_);
            if (this.toMailAddr_.Length == 0) {
                this.toMailAddr_ = this.fromMailAddr_;
            }
            mail.To.Add(this.toMailAddr_);
            mail.Subject = this.subject_;
            mail.Body = this.body_;

            // 파일 첨부
            if (this.attacheFile_.Length != 0) {
                System.Net.Mail.Attachment attachement;
                attachement = new System.Net.Mail.Attachment(this.attacheFile_);
                mail.Attachments.Add(attachement);
            }

            try {
                this.smtp_.Send(mail);
                //    Logger.getInstance.print(Log.주식봇, "메일 전송");
            }
            catch (SmtpException) {
                //  Logger.getInstance.print(Log.에러, e.Message);
            }
#endif
        }
    }
}
