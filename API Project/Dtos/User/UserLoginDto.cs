using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API_Project.Dtos.User
{
    public class UserLoginDto
    {
        // Used in IAuthRepository for Login()
        public string Username { get; set; }            // Can be username or email address.
        public string Password { get; set; }
    }
}
