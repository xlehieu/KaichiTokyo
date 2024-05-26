namespace WebsiteKaichiTokyo.EmailSender
{
    public interface IEmailSendercs
    {
        Task SendEmailAsync(string email,string subject,string bodyHtml);
    }
}
