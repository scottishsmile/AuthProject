using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API_Project.Services.EmailService
{
    public class EmailConfig
    {

        // This is used to load the details from appsettings.json so they can be used in EmailService.cs
        // That way we only have to update the config file to change the SMTP server.
        public string AdminEmailAddress { get; set; }
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public string SmtpUsername { get; set; }
        public string SmtpPassword { get; set; }
        public string WebsiteUrl { get; set;  }
    }
}
