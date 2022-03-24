using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API_Project.Dtos.User
{
    // Dto for the user to update their own details.
    public class UpdateSelfDto
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }            // This password will be hashed in UserService.cs
    }
}
