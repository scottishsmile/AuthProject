using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Website.Models
{
    public class OurSite
    {
        // Takes our site values from appsettings.json so we can use them throughout the program.
        public string Url { get; set; }         // The Admin website url
        public string Api { get; set; }         // The ASP Web API Url
        public string ReactClient { get; set; }         // The ReactClient Url. www.yourdomain.com/login
    }
}
