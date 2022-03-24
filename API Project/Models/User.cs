using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace API_Project.Models
{
    public class User
    {
        public static object UtcDateTime { get; private set; }
        public int Id { get; set; }
        public string Email { get; set; }
        public bool EmailVerified { get; set; } = false;
        public string EmailVerifyToken { get; set; }
        public DateTime DateEmailVerifySent { get; set; } = new DateTime(2022, 1, 1);
        public string Username { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }

        [Required]
        public string Role { get; set; }                 // Basic, Premium or Admin.    Data.DataContext.cs sets default value as basic in OnModelCreating()

        public int LoginTries { get; set; }                         // Login attempt count. So we can lock the account after 5 tries.

        public bool AccountLocked { get; set; }                     // True = no more attempts for 1 hour.

        public DateTime DateLocked { get; set; }                    // DateTime of the lock so we can count an hour.

        public bool ForgotPass { get; set; } = false;                    // True = no more password reset attempts for 1 hour.

        public DateTime DatePassResetEmailSent { get; set; } = DateTime.UtcNow;     // DateTime of the last password reset email sent so we can count an hour.

        public string ResetToken { get; set; }                      // Holds the reset token assigned when user forgets their password. So we can verify if they should have access to the Website/Brains/ResetPass page

        public bool Newsletter { get; set; } = true;                     // Checkbox for mailchimp newsletter subscription. True = subscribe to newsletter.
    }
}
