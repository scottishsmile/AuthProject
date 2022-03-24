using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API_Project.Dtos.User
{
    public class UserRegisterDto
    {
        // Used in IAuthRepository for Register()
        public string Email { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool Newsletter { get; set; }
    }
}
