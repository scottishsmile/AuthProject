using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Website.Models
{
    public class ResetPassModel
    {
        public int Id { set; get; }
        public string Token { set; get; }
        public string NewPassword1 { set; get; }            // Enter password box
        public string NewPassword2 { set; get; }            // Confirm password box
    }
}
