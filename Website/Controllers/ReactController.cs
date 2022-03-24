using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Website.Models;

namespace Website.Controllers
{
    public class ReactController : Controller
    {

        private readonly IOptions<OurSite> _ourSite;             // Get appsettings.json configs
        private readonly ILogger<ReactController> _logger;

        public ReactController(IOptions<OurSite> ourSite, ILogger<ReactController> logger)
        {
            _ourSite = ourSite;
            _logger = logger;
        }

        public IActionResult Index()
        {
            // Load the React Client as the index page.
            // Without this the MVC endpoint routing takes over and the admin login will be the default page.
            // "http://authclient.andyscode.net/login"
            _logger.LogInformation("ReactController. Redirect URL string is: " + _ourSite.Value.ReactClient);

            return Redirect(_ourSite.Value.ReactClient);
        }
    }
}
