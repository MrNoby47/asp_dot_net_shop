using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldMarket.Utility
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var emailtosend = new MimeMessage();
            emailtosend.From.Add(MailboxAddress.Parse("lysias16@univmetiers.ci"));
            emailtosend.To.Add(MailboxAddress.Parse(email));
            emailtosend.Subject = subject;
            emailtosend.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = htmlMessage };

            // send email

            using (var emailClient = new SmtpClient()) {
                emailClient.Connect("smtp.office365.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                emailClient.Authenticate("lysias16@univmetiers.ci", "Zeze79532030");
                emailClient.Send(emailtosend);
                emailClient.Disconnect(true);
            
            
            }


            return Task.CompletedTask;
        }
    }
}
