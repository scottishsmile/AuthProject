using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Website.Models
{
    public class SearchFormModel
    {
        // Similar to EditFormModel except we need objects for the SearchById and SearchByUsername forms
        public int SearchId { get; set; }
        public string SearchUsername { get; set; }

        public int Id { get; set; }
        public string Email { get; set; }
        public bool EmailVerified { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public bool AccountLocked { get; set; }
        public DateTime DateLocked { get; set; }
    }
}
