using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Website.Dtos.Users
{
    // Successful logins to the API reply using this DTO.
    public class LoginResponseDto
    {
        public string Token { get; set; }
        public int Id { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
    }
}
