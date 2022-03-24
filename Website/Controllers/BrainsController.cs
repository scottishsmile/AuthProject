using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Website.Models;
using Website.Services;
using Website.Dtos.Users;
using System.Net.Http;
using API_Project.Services.Validation;
using Microsoft.Extensions.Options;

/*
 * To Use ReadAsAsync() from the HttpClient install one of these:
 * System.Net.Http.Formatting package is now legacy and can instead be found in the Microsoft.AspNet.WebApi.Client
 */

namespace Website.Controllers
{
    public class BrainsController : Controller
    {
        private readonly ILogger<BrainsController> _logger;
        private readonly IHttpClientService _httpClient;
        private readonly IOptions<OurSite> _ourSite;             // Get appsettings.json configs

        public BrainsController(ILogger<BrainsController> logger, IHttpClientService httpClient, IOptions<OurSite> ourSite)
        {
            _logger = logger;
            _httpClient = httpClient;
            _ourSite = ourSite;
        }


        // SignIn
        // When login form button is pressed
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignInButton()
        {

            try
            {
                // Get User info from the form fields
                UserLoginDto user = new UserLoginDto();
                user.Username = Request.Form["Username"];       // Can be username or email.
                user.Password = Request.Form["Password"];

                // Validation of user input
                IValidate validate = new Validate();
                int passValidation = validate.alphabetValidation(user.Username) + validate.alphabetValidation(user.Password);

                // 0 is a pass, 1 is a fail.
                if (passValidation == 0)
                {

                    // POST request to API to get JWT Token
                    ServiceResponse<LoginResponseDto> response = await _httpClient.Login(user);

                    // The jwt token is in response.data
                    /*
                         {
                           "data": "eyJh....",
                           "success": true,
                           "message": null
                          }
                     */

                    if (response.Data != null)
                    {
                        // save token as apiToken in session storage
                        HttpContext.Session.SetString("apiToken", response.Data.Token);

                        // The React client needs the Email, Username and Role for routing decisions
                        // However, this admin section doesn't, it's much simpler, we just need the JWT auth token to let the admin login.

                        // Navigate to the Index page
                        return RedirectToAction("Index", "Brains");
                    }
                    else
                    {

                        // Display error for user below login form
                        ViewBag.errorMsg = "Login Error! Try Again.";
                        _logger.LogInformation("{Time} - User failed to login BrainsController.SignInButton - {1}", DateTime.UtcNow);

                        return View("SignIn");
                    }
                }
                else
                {
                    // Validation Failed
                    // Display error for user below login form
                    ViewBag.errorMsg = "Login Error! Validation Failed!";
                    _logger.LogInformation("{Time} - Error - Validation failed. BrainsController.SignInButton - Username: {1}", DateTime.UtcNow, user.Username);

                    return View("SignIn");
                }
            }
            catch (Exception ex)
            {
                // Exception

                // Display error for user below login form
                ViewBag.errorMsg = "Login Error! Try Again.";
                _logger.LogError("{Time} - Exception occured in BrainsController.SignInButton - {1}", DateTime.UtcNow, ex.Message);

                return View("SignIn");
            }
        }

    public IActionResult SignIn()
        {

            return View();
        }



        // LOGOUT
        public IActionResult LogOutButton()
        {
            // Delete the JWT token in the bowser session
            HttpContext.Session.Remove("apiToken");

            return RedirectToAction("SignIn", "Brains");

        }



        public IActionResult Index()
        {
            return View();
        }


        /*
         * GetAllPaged() is loaded first, the table has the starting page of "1" sent to it GetPagedUsers(token, 1)
         * GetAllPaged(int currentPageIndex) is loaded when the user clicks on the table's paged buttons.
         * Then the GetPagedUsers(token, currentPageIndex) is loaded. We pass the page ID to the API.
         */

        [HttpGet]
        public async Task<IActionResult> GetAllPaged()
        {
            try
            {
                // Get token from session storage in browser
                var token = HttpContext.Session.GetString("apiToken");

                if (string.IsNullOrEmpty(token))
                {
                    // No JWT Token, redirect back to login page.
                    return RedirectToAction("SignIn", "Brains");
                }
                else
                {
                    // GET request to API to GetPagedUsers
                    GetPagedUserDto pagedUsers = await _httpClient.GetPagedUsers(token, 1);


                    if (pagedUsers != null)
                    {
                        return View(pagedUsers);
                    }
                    else
                    {
                        // Error

                        // The error message is dispalyed by
                        // @Html.ValidationSummary(true, "", new { @class = "text-danger" })
                        ModelState.AddModelError(string.Empty, "Error! Couldn't get user data.");
                        return View();

                    }
                }
            }
            catch (Exception ex)
            {
                // Exception

                // The error message is dispalyed by
                // @Html.ValidationSummary(true, "", new { @class = "text-danger" })
                ModelState.AddModelError(string.Empty, "Error! Couldn't get user data.");
                _logger.LogError("{Time} - Exception occured in BrainsController.GetAllPaged() - {1}", DateTime.UtcNow, ex.Message);
                return View();

            }
        }

        [HttpPost]
        public async Task<IActionResult> GetAllPaged(int currentPageIndex)
        {
            try
            {
                var token = HttpContext.Session.GetString("apiToken");

                if (string.IsNullOrEmpty(token))
                {
                    return RedirectToAction("SignIn", "Brains");
                }
                else
                {
                    // GET request to API to GetPagedUsers
                    GetPagedUserDto pagedUsers = await _httpClient.GetPagedUsers(token, currentPageIndex);


                    if (pagedUsers != null)
                    {
                        return View(pagedUsers);
                    }
                    else
                    {
                        // Error

                        // The error message is dispalyed by
                        // @Html.ValidationSummary(true, "", new { @class = "text-danger" })
                        ModelState.AddModelError(string.Empty, "Error! Couldn't get user data.");
                        return View();

                    }
                }
            }
            catch (Exception ex)
            {
                // Exception

                // The error message is dispalyed by
                // @Html.ValidationSummary(true, "", new { @class = "text-danger" })
                ModelState.AddModelError(string.Empty, "Error! Couldn't get user data.");
                _logger.LogError("{Time} - Exception occured in BrainsController.GetAllPaged(int currentPageIndex) - {1}", DateTime.UtcNow, ex.Message);
                return View();

            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateButton(EditFormModel model)
        {
            try
            {
                // Get token from session storage in browser
                var token = HttpContext.Session.GetString("apiToken");

                if (string.IsNullOrEmpty(token))
                {
                    return RedirectToAction("SignIn", "Brains");
                }
                else
                {
                    // Create UpdateUserDto
                    UpdateUserDto update = new UpdateUserDto();
                    update.Id = int.Parse(Request.Form["Id"]);
                    update.Email = Request.Form["Email"];
                    update.EmailVerified = model.EmailVerified;
                    update.Username = Request.Form["Username"];
                    update.Role = Request.Form["Role"];
                    update.AccountLocked = model.AccountLocked;                         // Use @Html.CheckBoxFor(model => model.AccountLocked) on the page then have (EditFormModel model) as a parameter in this method.
                    update.DateLocked = DateTime.UtcNow;                                // This will use a Utc format and will display the time the account was last locked/unlocked.

                    // Password
                    // If the password field is blank "" then we want to keep the existing password
                    // We can't send "" as the password, as it will fail validation. Not good to accept users sending blank fields.
                    // Send code "#keepSame#" instead.
                    update.Password = Request.Form["Password"];

                    if(update.Password == "")
                    {
                        update.Password = "#keepSame#";
                    }

                    // Update user via API
                    ServiceResponse<GetUserDto> sendUpdate = await _httpClient.UpdateUser(token, update);

                    if (sendUpdate != null)
                    {
                        // If everything's ok, send user back to the paged user list.
                        // the updates/changes can be seen there.
                        return RedirectToAction("GetAllPaged", "Brains");
                    }
                    else
                    {
                        // Error

                        // The error message is dispalyed by
                        // @Html.ValidationSummary(true, "", new { @class = "text-danger" })
                        ModelState.AddModelError(string.Empty, "Error! Couldn't send the request.");
                        return View("EditUser");

                    }

                }
            }
            catch (Exception ex)
            {
                // Exception

                // The error message is dispalyed by
                // @Html.ValidationSummary(true, "", new { @class = "text-danger" })
                ModelState.AddModelError(string.Empty, "Error! Couldn't send the request.");
                _logger.LogError("{Time} - Exception occured in BrainsController.UpdateButton - {1}", DateTime.UtcNow, ex.Message);
                return View();

            }
        }

        // PUT Request
        public async Task<IActionResult> EditUser(int id)
        {
            try
            {
                // need to lookup user by id and fill in form fields.

                // Get token from session storage in browser
                var token = HttpContext.Session.GetString("apiToken");

                if (string.IsNullOrEmpty(token))
                {
                    return RedirectToAction("SignIn", "Brains");
                }
                else
                {

                    // GET request for the exisiting user's data. Send Id.
                    ServiceResponse<GetUserDto> getUser = await _httpClient.GetSingleUser(token, id);

                    if (getUser.Data != null)
                    {
                        // Fill the page's form fields with the exisiting user data.
                        // Id field will be readonly in html form
                        // Password field will be blank, won't display exisiting password.
                        /*
                        @ViewData["Id"] = getUser.Data.Id;
                        @ViewData["Email"] = getUser.Data.Email;
                        @ViewData["EmailVerified"] = getUser.Data.EmailVerified;
                        @ViewData["Username"] = getUser.Data.Username;
                        @ViewData["Role"] = getUser.Data.Role;
                        */

                        // Much easier to just pass a model to the view, that way the checkboxes will be checked/unchecked depending on the database values.
                        EditFormModel m = new EditFormModel();
                        m.Id = getUser.Data.Id;
                        m.Email = getUser.Data.Email;
                        m.EmailVerified = getUser.Data.EmailVerified;
                        m.Username = getUser.Data.Username;
                        m.Role = getUser.Data.Role;
                        m.AccountLocked = getUser.Data.AccountLocked;

                        return View(m);
                    }
                    else
                    {
                        // Error

                        // The error message is dispalyed by
                        // @Html.ValidationSummary(true, "", new { @class = "text-danger" })
                        ModelState.AddModelError(string.Empty, "Error! Couldn't get the user data.");
                        return View();

                    }
                }
            }
            catch (Exception ex)
            {
                // Exception

                // The error message is dispalyed by
                // @Html.ValidationSummary(true, "", new { @class = "text-danger" })
                ModelState.AddModelError(string.Empty, "Error! Couldn't get the user data.");
                _logger.LogError("{Time} - Exception occured in BrainsController.EditUser - {1}", DateTime.UtcNow, ex.Message);
                return View();

            }

        }


        // DELETE

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteButton()
        {
            try
            {
                // Get token from session storage in browser
                var token = HttpContext.Session.GetString("apiToken");

                if (string.IsNullOrEmpty(token))
                {
                    return RedirectToAction("SignIn", "Brains");
                }
                else
                {
                    // Grab the ID from the readonly form
                    int id = int.Parse(Request.Form["Id"]);

                    // GET request for the exisiting user's data. Send Id.
                    ServiceResponse<List<GetUserDto>> deleteUser = await _httpClient.DeleteUser(token, id);

                    if (deleteUser.Success == true)
                    {
                        // The ServiceResponse.Success = true if the user was deleted.
                        // Send user to confirmation page.

                        return View("DeleteConfirmed");
                    }
                    else
                    {
                        // Error

                        // The error message is dispalyed by
                        // @Html.ValidationSummary(true, "", new { @class = "text-danger" })
                        ModelState.AddModelError(string.Empty, "Error! User not deleted.");
                        return View("ErrorPg");

                    }
                }
            }
            catch (Exception ex)
            {
                // Exception

                // The error message is dispalyed by
                // @Html.ValidationSummary(true, "", new { @class = "text-danger" })
                ModelState.AddModelError(string.Empty, "Error! User not deleted.");
                _logger.LogError("{Time} - Exception occured in BrainsController.DeleteButton - {1}", DateTime.UtcNow, ex.Message);
                return View("ErrorPg");

            }
        }

        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                // need to lookup user by id to display on page

                // Get token from session storage in browser
                var token = HttpContext.Session.GetString("apiToken");

                if (string.IsNullOrEmpty(token))
                {
                    return RedirectToAction("SignIn", "Brains");
                }
                else
                {

                    // GET request for the exisiting user's data. Send Id.
                    ServiceResponse<GetUserDto> getUser = await _httpClient.GetSingleUser(token, id);

                    if (getUser.Data != null)
                    {
                        // Fill the page's form fields with the exisiting user data.
                        @ViewData["Id"] = getUser.Data.Id;
                        @ViewData["Email"] = getUser.Data.Email;
                        @ViewData["Username"] = getUser.Data.Username;
                        @ViewData["Role"] = getUser.Data.Role;

                        return View();
                    }
                    else
                    {
                        // Error

                        // The error message is dispalyed by
                        // @Html.ValidationSummary(true, "", new { @class = "text-danger" })
                        ModelState.AddModelError(string.Empty, "Error! Couldn't get the user data.");
                        return View();

                    }
                }
            }
            catch (Exception ex)
            {
                // Exception

                // The error message is dispalyed by
                // @Html.ValidationSummary(true, "", new { @class = "text-danger" })
                ModelState.AddModelError(string.Empty, "Error! Couldn't get the user data.");
                _logger.LogError("{Time} - Exception occured in BBrainsController.DeleteUser - {1}", DateTime.UtcNow, ex.Message);
                return View();

            }

        }


        // I had to create a duplicate of the UpdateButton() here as the update button uses the EditFormModel to pass the checkbox true/false
        // And the search page has it's own seperate model SearchFormModel as it needs Username and Id fields...
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SearchUpdateButton(SearchFormModel model)
        {
            try
            {
                // Get token from session storage in browser
                var token = HttpContext.Session.GetString("apiToken");

                if (string.IsNullOrEmpty(token))
                {
                    return RedirectToAction("SignIn", "Brains");
                }
                else
                {
                    // Create UpdateUserDto
                    UpdateUserDto update = new UpdateUserDto();
                    update.Id = int.Parse(Request.Form["Id"]);
                    update.Email = Request.Form["Email"];
                    update.EmailVerified = model.EmailVerified;
                    update.Username = Request.Form["Username"];
                    update.Role = Request.Form["Role"];
                    update.AccountLocked = model.AccountLocked;                         // Use @Html.CheckBoxFor(model => model.AccountLocked) on the page then have (SearchFormModel model) as a parameter in this method.
                    update.DateLocked = DateTime.UtcNow;                                // This will use a Utc format and will display the time the account was last locked/unlocked.

                    // Password
                    // If the password field is blank "" then we want to keep the existing password
                    // We can't send "" as the password, as it will fail validation. Not good to accept users sending blank fields.
                    // Send code "#keepSame#" instead.
                    update.Password = Request.Form["Password"];

                    if (update.Password == "")
                    {
                        update.Password = "#keepSame#";
                    }

                    // Update user via API
                    ServiceResponse<GetUserDto> sendUpdate = await _httpClient.UpdateUser(token, update);

                    if (sendUpdate != null)
                    {
                        // If everything's ok, send user back to the paged user list.
                        // the updates/changes can be seen there.
                        return RedirectToAction("GetAllPaged", "Brains");
                    }
                    else
                    {
                        // Error

                        // The error message is dispalyed by
                        // @Html.ValidationSummary(true, "", new { @class = "text-danger" })
                        ModelState.AddModelError(string.Empty, "Error! Couldn't send the request.");
                        return View("EditUser");

                    }
                }
            }
            catch (Exception ex)
            {
                // Exception

                // The error message is dispalyed by
                // @Html.ValidationSummary(true, "", new { @class = "text-danger" })
                ModelState.AddModelError(string.Empty, "Error! Couldn't send the request.");
                _logger.LogError("{Time} - Exception occured in BrainsController.SearchUpdateButton - {1}", DateTime.UtcNow, ex.Message);

                return View("EditUser");

            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SearchByIdButton()
        {
            try
            {
                // Get token from session storage in browser
                var token = HttpContext.Session.GetString("apiToken");

                if (string.IsNullOrEmpty(token))
                {
                    return RedirectToAction("SignIn", "Brains");
                }
                else
                {
                    // Grab Id from Id serach box
                    int id = int.Parse(Request.Form["SearchId"]);

                    // GET request for the exisiting user's data. Send Id.
                    ServiceResponse<GetUserDto> getUser = await _httpClient.GetSingleUser(token, id);

                    if (getUser.Data != null)
                    {
                        // Fill the page's form fields with the exisiting user data.
                        @ViewData["Id"] = getUser.Data.Id;
                        @ViewData["Email"] = getUser.Data.Email;
                        @ViewData["EmailVerified"] = getUser.Data.EmailVerified;
                        @ViewData["Username"] = getUser.Data.Username;
                        @ViewData["Role"] = getUser.Data.Role;

                        return View("UserSearch");
                    }
                    else
                    {
                        // Error

                        // The error message is dispalyed by
                        // @Html.ValidationSummary(true, "", new { @class = "text-danger" })
                        ModelState.AddModelError(string.Empty, "Error! Couldn't find the User. No ID match.");
                        return View("UserSearch");

                    }
                }
            }
            catch (Exception ex)
            {
                // Exception

                // The error message is dispalyed by
                // @Html.ValidationSummary(true, "", new { @class = "text-danger" })
                ModelState.AddModelError(string.Empty, "Error! Couldn't find the User. No ID match.");
                _logger.LogError("{Time} - Exception occured in BrainsController.SearchByIdButton - {1}", DateTime.UtcNow, ex.Message);

                return View("UserSearch");

            }

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SearchByUsernameButton()
        {
            try
            {
                // Get token from session storage in browser
                var token = HttpContext.Session.GetString("apiToken");

                if (string.IsNullOrEmpty(token))
                {
                    return RedirectToAction("SignIn", "Brains");
                }
                else
                {
                    // Grab username from serach box
                    string username = Request.Form["SearchUsername"];

                    // GET request for the exisiting user's data. Send Id.
                    ServiceResponse<GetUserDto> getUser = await _httpClient.GetUserByUsername(token, username);

                    if (getUser.Data != null)
                    {
                        // Fill the page's form fields with the exisiting user data.
                        @ViewData["Id"] = getUser.Data.Id;
                        @ViewData["Email"] = getUser.Data.Email;
                        @ViewData["EmailVerified"] = getUser.Data.EmailVerified;
                        @ViewData["Username"] = getUser.Data.Username;
                        @ViewData["Role"] = getUser.Data.Role;

                        return View("UserSearch");
                    }
                    else
                    {
                        // Error

                        // The error message is dispalyed by
                        // @Html.ValidationSummary(true, "", new { @class = "text-danger" })
                        ModelState.AddModelError(string.Empty, "Error! Couldn't find the User. No USERNAME match.");
                        return View("UserSearch");

                    }
                }
            }
            catch (Exception ex)
            {
                // Exception

                // The error message is dispalyed by
                // @Html.ValidationSummary(true, "", new { @class = "text-danger" })
                ModelState.AddModelError(string.Empty, "Error! Couldn't find the User. No USERNAME match.");
                _logger.LogError("{Time} - Exception occured in BrainsController.SearchByUsernameButton - {1}", DateTime.UtcNow, ex.Message);

                return View("UserSearch");

            }
        }
       

        public IActionResult UserSearch()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddButton(AddFormModel model)
        {
            // Get token from session storage in browser
            var token = HttpContext.Session.GetString("apiToken");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("SignIn", "Brains");
            }
            else
            {
                // Grab info from Add Form
                // Id not needed to add a new user, let the database assign it.
                AddUserDto user = new AddUserDto();
                user.Email = Request.Form["Email"];
                user.EmailVerified = model.EmailVerified;
                user.Username = Request.Form["Username"];
                user.Password = Request.Form["Password"];
                user.Role = Request.Form["Role"];

                // POST request to add new user.
                ServiceResponse<List<GetUserDto>> addUser = await _httpClient.AddUser(token, user);

                if (addUser.Success == true)
                {
                    // The ServiceResponse.Success = true if the user was added.
                    // Send user to confirmation page.
                    return View("AddConfirmed");
                }
                else
                {
                    // Error

                    // The error message is dispalyed by
                    // @Html.ValidationSummary(true, "", new { @class = "text-danger" })
                    ModelState.AddModelError(string.Empty, "Error! Can't create new user.");
                    return View("AddUser");

                }

            }
        }

        public IActionResult AddUser() {

            return View();
        }


        // Reset Password Button
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassButton(ResetPassModel model)
        {
            try
            {

                // Grab new password from form
                int id = Int32.Parse(Request.Form["FormId"]);
                string token = Request.Form["FormToken"];
                string newpass = Request.Form["NewPassword1"];
                string newpass2 = Request.Form["NewPassword2"];

                // Rebuild the model so we can pass it to ResetPassAgain incase of errors.
                var newModel = new ResetPassModel
                {
                    Id = id,
                    Token = token,
                };

                if (string.IsNullOrEmpty(token))
                {
                    // Error, no token supplied
                    // Use TempData[] with RedirectToAction instead of ViewBag
                    TempData["errorMsg"] = "You're not authorized to reset this password!";

                    // We need to pass the model with ID and Token back to the page
                    // The original ResetPass expects a query string input not a model.
                    // Best option is to use a similar controller method as we can't do overloading.
                    return RedirectToAction("ResetPassAgain", newModel);
                }
                else if (id < 1)
                {
                    // Error, Id is 0 or negative value
                    TempData["errorMsg"] = "You're not authorized to reset this password!";

                    return RedirectToAction("ResetPassAgain", newModel);
                }
                else if (newpass != newpass2)
                {
                    // Passwords don't match
                    // Display error for user below password reset form
                    TempData["errorMsg"] = "Sorry, Passwords don't match.";

                    return RedirectToAction("ResetPassAgain", newModel);
                }
                else
                {
                    // Validation of new password fields
                    IValidate validate = new Validate();
                    int passValidation = validate.alphabetValidation(newpass) + validate.alphabetValidation(newpass2);

                    // 0 is a pass, 1 is a fail.
                    if (passValidation == 0)
                    {

                        // PUT to API to change password
                        ChangePassDto newDetails = new ChangePassDto();
                        newDetails.Id = id;
                        newDetails.Token = token;
                        newDetails.Newpass = newpass;

                        ServiceResponse<string> changePass = await _httpClient.ChangePass(newDetails);

                        if (changePass.Success == true)
                        {
                            // Success! Password Changed.

                            TempData["login_again"] = _ourSite.Value.ReactClient;           // Pass the site login url from appsettings.json to the page

                            return View("ResetPassSuccess");
                        }
                        else
                        {
                            // Display error for user below password reset form
                            // Use TempData[] with RedirectToAction instead of ViewBag
                            TempData["errorMsg"] = "Error! Password not changed. " + changePass.Message;
                            _logger.LogInformation("{Time} - Error - Wrong Token or Id. BrainsController.ResetPassButton - ID: {1}", DateTime.UtcNow, id);


                            // We need to pass the model with ID and Token back to the page
                            // The original ResetPass expects a query string input not a model.
                            // Best option is to use a similar controller method as we can't do overloading.
                            return RedirectToAction("ResetPassAgain", newModel);

                        }
                    } else
                    {
                        // Validation Failed
                        TempData["errorMsg"] = "Error! Password not changed. Input Validation Failed.";
                        _logger.LogInformation("{Time} - Error - Validation failed. BrainsController.ResetPassButton - ID: {1}", DateTime.UtcNow, id);

                        return RedirectToAction("ResetPassAgain", newModel);
                    }
                }
            }
            catch (Exception ex)
            {
                // Exception

                ViewBag.errorMsg = "Error! Password not changed. Exception Occured. Close this page and use the email link again!";
                _logger.LogError("{Time} - Exception occured in BrainsController.ResetPassButton - {1}", DateTime.UtcNow, ex.Message);

                return View("ResetPass");

            }
        }

        // Reset Password Page
        // Users sent here via an email link
        // Email is generated by API AuthController.ForgotPass(). It only allows 1 email to be sent per hour.
        // Get User ID and resetpass JWT token from Url. Used to verify they are allowed to change the password.
        // https://localhost:44378/ResetPass?id=2&token=233rgths567sdsfg
        [HttpGet("ResetPass")]
        public IActionResult ResetPass([FromQuery] int id, [FromQuery] string token)
        {
            var model = new ResetPassModel
            {
                Id = id,
                Token = token,
            };

            return View("ResetPass", model);
        }

        [HttpGet("ResetPassAgain")]
        public IActionResult ResetPassAgain(ResetPassModel model)
        {

            // Accepts the model with ID, Token and the error message from the ResetPassButton
            // Easier than building a query string url and then redirecting to it.
            return View("ResetPass", model);
        }


        // Email Verified Page
        // Users sent here via an email link
        // Email is generated by API AuthController.Register(). It only allows 1 email to be sent per hour.
        // https://localhost:44378/VerifyEmail?id=2&token=233rgths567sdsfg
        [HttpGet("VerifyEmail")]
        public async Task<IActionResult> EmailVerified([FromQuery] int id, [FromQuery] string token)
        {
            try
            {
                var model = new VerifyEmailModel
                {
                    Id = id,
                    Token = token,
                };

                VerifyEmailDto details = new VerifyEmailDto();
                details.Id = id;
                details.Token = token;

                ServiceResponse<string> emailVerified = await _httpClient.EmailVerified(details);

                if (emailVerified.Success == true)
                {
                    // Success! Email verified.

                    TempData["login_again"] = _ourSite.Value.ReactClient;           // Pass the site login url from appsettings.json to the page

                    return View("EmailVerified", model);
                }
                else
                {
                    // Display error for user below password reset form
                    // Use TempData[] with RedirectToAction instead of ViewBag
                    TempData["errorMsg"] = "Error! Email Address Couldn't Be Verified. " + emailVerified.Message;
                    _logger.LogInformation("{Time} - Error - Wrong Token or Id. BrainsController.EmailVerified - ID: {1}", DateTime.UtcNow, id);

                    TempData["login_again"] = _ourSite.Value.ReactClient;           // Pass the site login url from appsettings.json to the page

                    return View("EmailNotVerified", model);

                }
            }
            catch (Exception ex)
            {
                // Exception

                ViewBag.errorMsg = "Error! Email Not Verified. Exception Occured. Close this page and use the email link again!";
                _logger.LogError("{Time} - Exception occured in BrainsController.EmailVerified - {1}", DateTime.UtcNow, ex.Message);

                return View("EmailNotVerified");

            }

        }
    }
}
