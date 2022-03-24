using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Website.Dtos.Users
{
    public class UpdateUserDto
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public bool EmailVerified { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }            // This password will be hashed by API
        public string Role { get; set; } = "basic";
        public bool AccountLocked { get; set; }
        public DateTime DateLocked { get; set; }
    }
}
