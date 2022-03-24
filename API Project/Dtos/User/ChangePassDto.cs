using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API_Project.Dtos.User
{
    public class ChangePassDto
    {
        public int Id { get; set; }

        public string Token { get; set; }

        public string Newpass { get; set; }
    }
}
