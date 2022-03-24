using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API_Project.Models;
using MailKit;                          // Send SMTP emails
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;                          // MIME email message format
using MimeKit.Text;
using AutoMapper.Configuration;
using Microsoft.Extensions.Options;
using API_Project.Data;
using Microsoft.Extensions.Logging;

namespace API_Project.Services.EmailService
{
    public class EmailService : IEmailService
    {

        private readonly DataContext _context;                                          // Entity DB
        private readonly ILogger<EmailService> _logger;
        private readonly IOptions<EmailConfig> _emailConfig;                            // EmailConfiguration settings in appsettings.json

        // Constructor
        public EmailService(IOptions<EmailConfig> emailConfig, ILogger<EmailService>  logger, DataContext context)
        {
            _emailConfig = emailConfig;
            _context = context;
            _logger = logger;
        }

        public void SendEmail(string destination, string subject, string message)
        {
            // Emails sent using MailKit
            // Multipurpose Internet Mail Extension (MIME) format. It allows us to send fancy emails containing attachements, video etc.

            /*
            // Create Email
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse("admin@andystestsite.net"));
            email.To.Add(MailboxAddress.Parse(destination));
            email.Subject = subject;
            email.Body = new TextPart(TextFormat.Plain) { Text = message };
            // email.Body = new TextPart(TextFormat.Html) { Text = message };

            // Send Email
            using var smtp = new SmtpClient();
            smtp.Connect("smtp.ethereal.email", 587, SecureSocketOptions.StartTls);                             // Our SMTP server
            smtp.Authenticate("arlie.wyman39@ethereal.email", "7qkRvs1AnCdMHj4T1Z");
            smtp.Send(email);
            smtp.Disconnect(true);
            */

            // Uses "Email Configuration" settings inside appsettings.json
            // Use IOptions<IEmailConfig> _emailConfig.Value to access them.
            try {
                // Create Email
                var email = new MimeMessage();
                email.From.Add(MailboxAddress.Parse(_emailConfig.Value.AdminEmailAddress));
                email.To.Add(MailboxAddress.Parse(destination));
                email.Subject = subject;
                email.Body = new TextPart(TextFormat.Html) { Text = message };


                // Send Email
                using var smtp = new SmtpClient();
                smtp.Connect(_emailConfig.Value.SmtpServer, _emailConfig.Value.SmtpPort);         // This can also be added - SecureSocketOptions.Auto - Allow the IMailService to decide which SSL or TLS options to use. Could also be SecureSocketOptions.SslOnConnect or SecureSocketOptions.StartTls.
                smtp.Authenticate(_emailConfig.Value.SmtpUsername, _emailConfig.Value.SmtpPassword);
                smtp.Send(email);
                smtp.Disconnect(true);
            }
            catch (Exception ex)
            {
                _logger.LogError("{Time} -SMTP Error in EmailService.SendEmail . Likely the SMTP server is misconfigured or firewall is blocking comms. Error - {1}", DateTime.UtcNow, ex.Message);

            }
        }


        // Forgot Password. Password Reset Email.
        public void SendPasswordResetEmail(User user, string token)
        {
            // Build reset link url
            // Our Base Url is the WebsiteUrl in appsettings.json
            // https://localhost:44378/ResetPass?id=2&token=233rgths567sdsfg
            string resetUrl = _emailConfig.Value.WebsiteUrl + "ResetPass?id=" + user.Id + "&token=" + token;

            string message = $@"<p>A password reset has been requested for your account. Ignore this email if you did not request the change!</p>
                                <p>Please click the link to reset your password: <a href='{resetUrl}'>Reset Password</a><p>";

            string destination = user.Email;

            string subject = "Password Reset Request";

            // Password has been reset before if ForgotPass == true
            // Check if an hour has expired. If it has, allow password reset.
            if (user.ForgotPass == true)
            {
                var compareTimes = (DateTime.UtcNow - user.DatePassResetEmailSent).Duration().TotalHours;

                if (compareTimes > 1)
                {
                    user.ForgotPass = false;        // Allow password reset emails to be sent again.
                                                    
                }
            }

            // Only allow 1 password reset email an hour to stop SPAM.
            // ForgotPass = true if password reset email sent in last hour.
            if (user.ForgotPass == false)
            {
                user.ForgotPass = true;                                 // We are sending the forgot pass email now, so set to true.
                user.DatePassResetEmailSent = DateTime.UtcNow;          // Track time last password reset email was sent
                user.ResetToken = token;                                // Save the password reset token to the user's DB entry for comparison later.

                _context.SaveChangesAsync();                      // SaveChangesAsync() is an Entity command to write changes to the database.
                SendEmail(destination, subject, message);               // Send the email
            }
        }

        public void SendVerificationEmail(User user, string token)
        {
            // Build email verification link url
            // Our Base Url is the WebsiteUrl in appsettings.json
            // https://localhost:44378/VerifyEmail?id=2&token=233rgths567sdsfg
            string verifyUrl = _emailConfig.Value.WebsiteUrl + "VerifyEmail?id=" + user.Id + "&token=" + token;

            string message = $@"<p>One more step to register! Ignore this email if you did not request a user account with us!</p>
                                <p>Please click the link to verify your email address: <a href='{verifyUrl}'>Verify Email Address</a><p>";

            string destination = user.Email;

            string subject = "Verify Your Email Address";

            // Only allow 1 email verification an hour to stop SPAM.
            // By default the DateEmailVerifySent DateTime should be 1st Jan 2022 on new accounts. It doesn't like null values.
            var compareTimes = (DateTime.UtcNow - user.DateEmailVerifySent).Duration().TotalHours;

            if (compareTimes > 1)
            {
                user.DateEmailVerifySent = DateTime.UtcNow;             // Track time last verify email was sent
                _context.SaveChangesAsync();                            // SaveChangesAsync() is an Entity command to write changes to the database.
                SendEmail(destination, subject, message);               // Send the email
            }

        }

        // Similar to above SendVerificationEmail but will not check for 1 hour time limit.
        // It will resend the email verification link immediately.
        // Only used when user has supplied the correct username and password, but hasn't already verified their account.
        public void ReSendVerificationEmail(User user, string token)
        {
            // Build email verification link url
            // Our Base Url is the WebsiteUrl in appsettings.json
            // https://localhost:44378/VerifyEmail?id=2&token=233rgths567sdsfg
            string verifyUrl = _emailConfig.Value.WebsiteUrl + "VerifyEmail?id=" + user.Id + "&token=" + token;

            string message = $@"<p>One more step to register! Ignore this email if you did not request a user account with us!</p>
                                <p>Please click the link to verify your email address: <a href='{verifyUrl}'>Verify Email Address</a><p>";

            string destination = user.Email;

            string subject = "Verify Your Email Address";

            user.DateEmailVerifySent = DateTime.UtcNow;             // Track time last verify email was sent
            _context.SaveChangesAsync();                            // SaveChangesAsync() is an Entity command to write changes to the database.
            SendEmail(destination, subject, message);               // Send the email

        }

    }
}
