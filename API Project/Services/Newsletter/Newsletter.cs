using API_Project.Data;
using API_Project.Dtos.User;
using API_Project.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using sib_api_v3_sdk.Api;                       // Send In Blue Email Newsletter
using sib_api_v3_sdk.Client;                    // Send In Blue Email Newsletter
using sib_api_v3_sdk.Model;                     // Send In Blue Email Newsletter
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Options;

namespace API_Project.Services.Newsletter
{
    public class Newsletter : INewsletter
    {
        // SEND IN BLUE NEWSLETTER MODULE
        // Uses sib_api_v3_sdk library for SendInBlue API 3.3.0
        // https://github.com/sendinblue/APIv3-csharp-library


        private readonly DataContext _context;                              // Entity DB
        private readonly ILogger<Newsletter> _logger;
        private readonly IOptions<SendInBlueConfig> _sibConfig;             // Get appsettings.json configs

        public Newsletter(DataContext context, ILogger<Newsletter> logger, IOptions<SendInBlueConfig> sibConfig)
        {
            _context = context;
            _logger = logger;
            _sibConfig = sibConfig;
        }



        // SUBSCRIBE - CREATE CONTACT
        // The VerifyEmailDto contains th euser ID and the email verification token. We only need the ID to look up the user here.
        public async Task<ServiceResponse<string>> Subscribe(VerifyEmailDto details)
        {
            var serviceResponse = new ServiceResponse<string>();

            try
            {
                // Get user by ID from the database
                User user = await _context.Users.FirstOrDefaultAsync(u => u.Id == details.Id);

                // FirstOrDefault() returns null if nothing is found.
                if (user != null)
                {
                    // Check the user wants to subscribe. Newsltter Checkbox checked.
                    if (user.Newsletter == true)
                    {
                        // Newsletter Subscription (SendInBlue)
                        // If Newsletter = true (a checkbox) then subscribe them
                        // Values are from appsettings.json config file
                        sib_api_v3_sdk.Client.Configuration.Default.ApiKey.Add("api-key", _sibConfig.Value.apiKey);

                        var apiInstance = new ContactsApi();
                        string email = user.Email;

                        JObject attributes = new JObject();
                        attributes.Add("LASTNAME", user.Username);

                        List<long?> listIds = new List<long?>();
                        listIds.Add(2);

                        bool emailBlacklisted = false;
                        bool smsBlacklisted = false;
                        bool updateEnabled = false;

                        try
                        {
                            var createContact = new CreateContact(email, attributes, emailBlacklisted, smsBlacklisted, listIds, updateEnabled);
                            CreateUpdateContactModel result = apiInstance.CreateContact(createContact);
                            _logger.LogInformation("{Time} - SendInBlue Success - USER: {1} - EMAIL: {2} --- {3}", DateTime.UtcNow, user.Username, user.Email, result.ToJson());
                        }
                        catch (Exception ex)
                        {
                            _logger.LogInformation("{Time} - SendInBlue Error - USER: {1} - EMAIL: {2} --- {3}", DateTime.UtcNow, user.Username, user.Email, ex.Message);
                        }
                    }
                    else
                    {
                        // User doesn't want to subscribe
                        serviceResponse.Success = false;
                        serviceResponse.Message = "User doesn't want to subscribe to newsletter.";
                        _logger.LogInformation("{Time} - User doesn't want to subscribe to newsletter. Newsletter.Subscribe - ID: {1}", DateTime.UtcNow, details.Id);
                    }

                }
                else
                {
                    // User object was null
                    serviceResponse.Success = false;
                    serviceResponse.Message = "Error, no user found!";
                    _logger.LogInformation("{Time} - Error - user requested an update for an ID we couldn't find. Newsletter.Subscribe - ID: {1}", DateTime.UtcNow, details.Id);
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = "Email Verification Error.";
                _logger.LogError("{Time} - Exception in UserService.EmailVerified - {1}", DateTime.UtcNow, ex.Message);

            }

            return serviceResponse;
        }
    }
}
