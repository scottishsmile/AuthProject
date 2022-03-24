using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Website.Dtos.Users;

namespace Website.Dtos.Users
{
    public class GetUserDto
    {
        public GetUserDto(int Id, string Email, bool EmailVerified, string Username, string Role, bool AccountLocked, DateTime DateLocked)
        {
            this.Id = Id;
            this.Email = Email;
            this.EmailVerified = EmailVerified;
            this.Username = Username;
            this.Role = Role;
            this.AccountLocked = AccountLocked;
            this.DateLocked = DateLocked;
        }

            public int Id { get; set; }
            public string Email { get; set; }
            public bool EmailVerified { get; set; }
            public string Username { get; set; }
            public string Role { get; set; }
            public bool AccountLocked { get; set; }
            public DateTime DateLocked { get; set; }
    }
}
