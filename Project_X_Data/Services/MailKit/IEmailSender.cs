namespace Project_X_Data.Services.MailKit
{
    public interface IEmailSender
    {
        void SendEmail(string toEmail, string subject, string body);
    }
}
