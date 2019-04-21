using System;
using System.Threading.Tasks;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace BaseApi.Helper {
    public class MailerSendGrid {
        public static async Task Send (string destination) {
            var apiKey = Environment.GetEnvironmentVariable ("SENDGRID_API_KEY");
            var client = new SendGridClient (apiKey);
            var from = new EmailAddress ("noreply@bookup.com", "I am noreply");
            var subject = "Bookup subscription";
            var to = new EmailAddress (destination);
            var htmlContent = "<strong>Hello</strong><br/>Thank your for your subscription<br/><strong>Good bye</strong>";
            var msg = MailHelper.CreateSingleEmail (from, to, subject, "", htmlContent);
            try {
                await client.SendEmailAsync (msg);
            } catch (Exception e) {
                Console.WriteLine(e.Message);
            }
        }
    }
}