using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Website.Dtos.Users;

namespace Website.Dtos.Users
{
    public class UserLoginDto
    {
        [Required]
        public string Username { get; set; }        // Can be username or email address.

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
