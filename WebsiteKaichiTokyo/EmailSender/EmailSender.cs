using System.Net;
using System.Net.Mail;

namespace WebsiteKaichiTokyo.EmailSender
{
    public class EmailSender : IEmailSendercs
    {
        public Task SendEmailAsync(string email, string subject,string bodyHtml)
        {
            var mail = "xlehieu@gmail.com";
            var pw = "czje usss wgsa vuzy";
            var client = new SmtpClient("smtp.gmail.com",587)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(mail, pw)
            };
            MailMessage mailMessage = new MailMessage
            {
                From = new MailAddress(mail),
                Subject = subject,
                Body = bodyHtml!=null?bodyHtml:"",
                IsBodyHtml = true,
            };
            mailMessage.To.Add(new MailAddress(email));
            return client.SendMailAsync(mailMessage);

        }
    }
}
