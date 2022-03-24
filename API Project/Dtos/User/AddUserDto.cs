using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API_Project.Dtos.User
{
    public class AddUserDto
    {

        public string Email { get; set; }
        public bool EmailVerified { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; } = "basic";
    }
}
