using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API_Project.Dtos.User
{
    // This is our reply back tot he React client after a successful login.
    // React will need to use the Username and Role for some of it's routing decisions.
    public class LoginResponseDto
    {
        public string Token { get; set; }
        public int Id { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
    }
}
