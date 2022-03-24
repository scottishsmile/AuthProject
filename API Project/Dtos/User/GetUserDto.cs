using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API_Project.Dtos.User
{
    public class GetUserDto
    {
        // The data we want to send back to the admin, left out password stuff.
        public int Id { get; set; }
        public string Email { get; set; }
        public bool EmailVerified { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }                 // Basic, Premium or Admin.    Data.DataContext.cs sets default value as basic in OnModelCreating()

        // No need for LoginTries in the admin section.

        public bool AccountLocked { get; set; }                     // True = no more attempts for 1 hour.
        public DateTime DateLocked { get; set; }                    // DateTime of the lock so we can count an hour.
    }
}
