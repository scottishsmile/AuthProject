using API_Project.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API_Project.Services.EmailService
{
    public interface IEmailService
    {
       void SendEmail(string destination, string subject, string message);                // Send an email using the website host's SMTP server.
       void SendPasswordResetEmail(User user, string token);                              // Forgot Password. Send email with JWT token Url so they can reset the password.
       void SendVerificationEmail(User user, string token);                            // Verify email address.
       void ReSendVerificationEmail(User user, string token);                            // Verify email address.

    }
}
