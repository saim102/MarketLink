using System.Net;
using System.Net.Mail;

namespace IdentityDemoApp.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(
            string toEmail,
            string subject,
            string htmlBody)
        {
            // ==========================================
            // SMTP SETTINGS
            // ==========================================

            var smtpHost =
                _configuration["EmailSettings:SmtpHost"];

            var smtpPort =
                int.Parse(
                    _configuration["EmailSettings:SmtpPort"] ?? "587"
                );

            var senderEmail =
                _configuration["EmailSettings:SenderEmail"];

            var senderName =
                _configuration["EmailSettings:SenderName"]
                ?? "MarketLink";

            var senderPassword =
                _configuration["EmailSettings:SenderPassword"];


            // ==========================================
            // VALIDATION
            // ==========================================

            if (string.IsNullOrWhiteSpace(toEmail))
            {
                throw new ArgumentException(
                    "Customer email address is required."
                );
            }

            if (string.IsNullOrWhiteSpace(smtpHost))
            {
                throw new InvalidOperationException(
                    "SMTP Host is missing."
                );
            }

            if (string.IsNullOrWhiteSpace(senderEmail))
            {
                throw new InvalidOperationException(
                    "Sender Email is missing."
                );
            }

            if (string.IsNullOrWhiteSpace(senderPassword))
            {
                throw new InvalidOperationException(
                    "Sender Password is missing."
                );
            }


            // ==========================================
            // CREATE EMAIL
            // ==========================================

            using var mail = new MailMessage();

            mail.From = new MailAddress(
                senderEmail,
                senderName
            );

            mail.To.Add(
                new MailAddress(toEmail)
            );

            mail.Subject = subject;

            mail.Body = htmlBody;

            mail.IsBodyHtml = true;


            // ==========================================
            // SMTP CLIENT
            // ==========================================

            using var smtp = new SmtpClient(
                smtpHost,
                smtpPort
            );

            smtp.EnableSsl = true;

            smtp.UseDefaultCredentials = false;

            smtp.Credentials =
                new NetworkCredential(
                    senderEmail,
                    senderPassword
                );


            // ==========================================
            // SEND EMAIL
            // ==========================================

            await smtp.SendMailAsync(mail);
        }
    }
}