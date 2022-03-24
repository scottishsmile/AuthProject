using API_Project.Data;
using API_Project.Dtos.User;
using API_Project.Models;
using API_Project.Services.Newsletter;
using API_Project.Services.UserService;
using API_Project.Services.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace API_Project.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]         // So we can access the route by just "User"
    public class UserController : ControllerBase
    {

        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;
        private readonly INewsletter _newsletter;

        // Constructor
        public UserController(IUserService userService, ILogger<UserController> logger, INewsletter newsletter)
        {
            _userService = userService;
            _logger = logger;
            _newsletter = newsletter;
        }


        // Get All Users
        /*
        [Authorize(Roles = "admin")]
        [HttpGet]
        [Route("GetAllUsers")]
        public async Task<ActionResult<ServiceResponse<List<GetUserDto>>>> Get()
        {
            return Ok(await _userService.GetAllUsers());  // 200 OK
        }
        */


        // Paged User Request
        // Get 100 users at a time
        // Useful for displaying data in tables or GridViews as we don't need to send ALL 1000s of records.
        [Authorize(Roles = "admin")]
        [HttpGet]
        [Route("GetPagedUsers/{currentPage}")]
        public async Task<ActionResult<ServicePaged<GetPagedUserDto>>> GetPagedUsers(int currentPage)
        {
            try
            {
                // Validation
                // Do validation at the controller level, before passing data anywhere else.
                IValidate validate = new Validate();
                int passValidation = validate.numberValidation(currentPage.ToString());

                // 0 is a pass, 1 is a fail.
                if (passValidation == 0)
                {
                    return Ok(await _userService.GetPagedUsers(currentPage));
                }

                // Send a message to tell user of the error
                // The 200 OK simulates our standard response if we couldn't find a matching username. Can't really have validation return a 404 Not Found but wrong username returns a 200 OK.
                var serviceResponse = new ServicePaged<GetPagedUserDto>();
                serviceResponse.Data = null;
                serviceResponse.Success = false;
                serviceResponse.Message = "Validation Failed.";

                _logger.LogInformation("{Time} - Validation Failed in UserController.GetPagedUsers - PAGENUM: {1} ", DateTime.UtcNow,  currentPage.ToString());

                return Ok(serviceResponse);
            }
            catch(Exception ex)
            {
                _logger.LogError("{Time} - Exception in UserController.GetPagedUsers - {1}", DateTime.UtcNow, ex.Message);
                return NotFound();
            }
        }


        // Returns a single user. Search By ID.
        // https://localhost:44378/User/1
        [Authorize(Roles = "admin")]
        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<ServiceResponse<GetUserDto>>> GetSingleUser(int id)
        {
            try
            {
                // Validation
                IValidate validate = new Validate();
                int passValidation = validate.numberValidation(id.ToString());

                // 0 is a pass, 1 is a fail.
                if (passValidation == 0)
                {
                    return Ok(await _userService.GetUserById(id));
                }

                // Send a message to tell user of the error
                // The 200 OK simulates our standard response if we couldn't find a matching user. Can't really have validation return a 404 Not Found but wrong user id returns a 200 OK.
                var serviceResponse = new ServiceResponse<GetUserDto>();
                serviceResponse.Data = null;
                serviceResponse.Success = false;
                serviceResponse.Message = "Validation Failed.";

                _logger.LogInformation("{Time} - Validation Failed in UserController.GetSingleUser - ID: {1} ", DateTime.UtcNow, id.ToString());

                return Ok(serviceResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError("{Time} - Exception in UserController.GetSingleUser - {1}", DateTime.UtcNow, ex.Message);
                return NotFound();
            }
        }


        // Returns a single user. Search By Username.
        // https://localhost:44378/Username/sexypants95
        [Authorize(Roles = "admin")]
        [HttpGet]
        [Route("/Username/{username}")]
        public async Task<ActionResult<ServiceResponse<GetUserDto>>> GetUserByUsername(string username)
        {
            try
            {
                // Validation
                IValidate validate = new Validate();
                int passValidation = validate.alphabetValidation(username);

                // 0 is a pass, 1 is a fail.
                if (passValidation == 0)
                {
                    return Ok(await _userService.GetUserByUsername(username));
                }

                // Send a message to tell user of the error
                var serviceResponse = new ServiceResponse<GetUserDto>();
                serviceResponse.Data = null;
                serviceResponse.Success = false;
                serviceResponse.Message = "Validation Failed.";
                Console.WriteLine("Validation Failed in UserController.GetUserByUsername - USER: " + username);
                _logger.LogInformation("{Time} - Validation Failed in UserController.GetUserByUsername - USER: {1} ", DateTime.UtcNow, username);

                return Ok(serviceResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError("{Time} - Exception in UserController.GetUserByUsername - {1}", DateTime.UtcNow, ex.Message);
                return NotFound();
            }
        }

        /*
        // POST. Add a new user
        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<ActionResult<ServiceResponse<List<GetUserDto>>>> AddUser(AddUserDto newUser)
        {

            var response = await _userService.AddUser(newUser);

            // Check if data has null fields and return a 404 not found.
            if (response.Data == null)
            {
                Console.Write("Error in UserController.AddUser - Null data was recieved.");
                return NotFound(response);
            }

            return Ok(response);
        }
        */

        // We don't want to return a whole list of ALL the users plus the added user. As above. Could be 1000s of records.
        // Lets just send back a http 200OK response and a ServiceResponse.Success = true if we could add the user and ServiceResponse.Success = false if we couldn't

        // POST. Add a new user
        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<ActionResult<ServiceResponse<List<GetUserDto>>>> AddUser(AddUserDto newUser)
        {
            try
            {
                // We will validate the Username, Email, Password and Role in the AddUserDto.
                // Each of the validations returns 0 if it passed or 1 if it failed.
                IValidate validate = new Validate();
                int passValidation = validate.alphabetValidation(newUser.Username) + validate.emailValidation(newUser.Email) + validate.alphabetValidation(newUser.Password) + validate.alphabetValidation(newUser.Role);

                // 0 is a pass, 1+ is a fail.
                if (passValidation == 0)
                {

                    var response = await _userService.AddUser(newUser);

                    if (response.Success == false)
                    {
                        _logger.LogInformation("{Time} - Error in UserController.AddUser - User was not added.", DateTime.UtcNow);

                        return NotFound();
                    }

                    return Ok(response);
                }

                // Send a message to tell user of the error
                var serviceResponse = new ServiceResponse<List<GetUserDto>>();
                serviceResponse.Data = null;
                serviceResponse.Success = false;
                serviceResponse.Message = "Validation Failed.";

                _logger.LogInformation("{Time} - Validation Failed in UserController.AddUser - USER: {1} - EMAIL: {2} ", DateTime.UtcNow, newUser.Username, newUser.Email);


                return Ok(serviceResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError("{Time} - Exception in UserController.AddUser- {1}", DateTime.UtcNow, ex.Message);
                return NotFound();
            }
        }



        // PUT. Update a user.
        [Authorize(Roles = "admin")]
        [HttpPut]
        public async Task<ActionResult<ServiceResponse<GetUserDto>>> UpdateUser(UpdateUserDto updatedUser)
        {
            try
            {
                // We will validate the ID, Username, Email, Password, Role, DateLocked in the UpdateUserDto.
                // No need to validate the AccountLocked bool, if it's not true or false it will be rejected.
                // Each of the validations returns 0 if it passed or 1 if it failed.
                IValidate validate = new Validate();
                int passValidation = validate.numberValidation(updatedUser.Id.ToString()) + validate.alphabetValidation(updatedUser.Username) + validate.emailValidation(updatedUser.Email) + validate.alphabetValidation(updatedUser.Password) + validate.alphabetValidation(updatedUser.Role) + validate.dateTimeUtcValidation(updatedUser.DateLocked);

                // 0 is a pass, 1+ is a fail.
                if (passValidation == 0)
                {
                    var response = await _userService.UpdateUser(updatedUser);

                    // Check if data has null fields and return a 404 not found.
                    if (response.Data == null)
                    {
                        _logger.LogInformation("{Time} - Error in UserController.UpdateUser - Null data was recieved.", DateTime.UtcNow);

                        return NotFound(response);
                    }

                    return Ok(response);
                }
                // Send a message to tell user of the error
                var serviceResponse = new ServiceResponse<GetUserDto>();
                serviceResponse.Data = null;
                serviceResponse.Success = false;
                serviceResponse.Message = "Validation Failed.";

                _logger.LogInformation("{Time} - Validation Failed in UserController.UpdateUser - ID: {1} - USER: {2} - EMAIL: {3}", DateTime.UtcNow, updatedUser.Id, updatedUser.Username, updatedUser.Email);


                return Ok(serviceResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError("{Time} - Exception in UserController.UpdateUser - {1}", DateTime.UtcNow, ex.Message);
                return NotFound();
            }
        }



        // PUT. Update Self.
        // Allows the user to update their own emil, username or password.
        // https://localhost:44378/User/UpdateSelf
        [Authorize(Roles = "basic, premium, admin")]
        [HttpPut]
        [Route("/User/UpdateSelf/")]
        public async Task<ActionResult<ServiceResponse<UpdateSelfDto>>> UpdateSelf(UpdateSelfDto updateSelf)
        {
            try
            {
                // We will validate the ID, Username, Email, Password in UpdateSelfDto
                // Each of the validations returns 0 if it passed or 1 if it failed.
                IValidate validate = new Validate();
                int passValidation = validate.numberValidation(updateSelf.Id.ToString()) + validate.alphabetValidation(updateSelf.Username) + validate.emailValidation(updateSelf.Email) + validate.alphabetValidation(updateSelf.Password);

                // 0 is a pass, 1+ is a fail.
                if (passValidation == 0)
                {
                    // Check password complexity
                    // Returns true if it passes, false if it fails
                    StrongPassword passStrength = new StrongPassword();
                    if (passStrength.Check(updateSelf.Password) == true)
                    {
                        // Do the update
                        var response = await _userService.UpdateSelf(updateSelf);

                        // Check if data has null fields and return a 404 not found.
                        if (response.Data == null)
                        {
                            _logger.LogInformation("{Time} - Error in UserController.UpdateSelf - Null data was recieved.", DateTime.UtcNow);
                            return NotFound(response);
                        }

                        return Ok(response);
                    } else
                    {
                        // Send a message to tell user of the error
                        var response = new ServiceResponse<UpdateSelfDto>();
                        response.Data = null;
                        response.Success = false;
                        response.Message = "Password MUST be at least 8 characters. Include Uppercase, lowercase, a number and a special character.";

                        _logger.LogInformation("{Time} - Error - Password Complexity Failed. UserController.UpdateSelf() - ID: {1} - USER: {2}", DateTime.UtcNow, updateSelf.Id, updateSelf.Username);

                        return response;
                    }
                }

                // Send a message to tell user of the error
                var serviceResponse = new ServiceResponse<UpdateSelfDto>();
                serviceResponse.Data = null;
                serviceResponse.Success = false;
                serviceResponse.Message = "Validation Failed.";

                _logger.LogInformation("{Time} - Validation Failed in UserController.UpdateSelf - ID: {1} - USER: {2} - EMAIL: {3} ", DateTime.UtcNow, updateSelf.Id, updateSelf.Username, updateSelf.Email);


                return Ok(serviceResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError("{Time} - Exception in UserController.UpdateSelf - {1}", DateTime.UtcNow, ex.Message);
                return NotFound();
            }
        }


        // Change User Password
        // Allows the user to reset their password.
        // https://localhost:44378/User/ChangePass
        [Authorize(Roles = "basic, premium, admin")]
        [HttpPut]
        [Route("/User/ChangePass/")]
        public async Task<ActionResult<ServiceResponse<string>>> ChangePass(ChangePassDto newDetails)
        {
            try
            {
                // We will validate the ID and password in the Dto
                // Each of the validations returns 0 if it passed or 1 if it failed.
                IValidate validate = new Validate();
                int passValidation = validate.numberValidation(newDetails.Id.ToString()) + validate.alphabetValidation(newDetails.Newpass);

                // 0 is a pass, 1+ is a fail.
                if (passValidation == 0)
                {
                    // Check password complexity
                    // Returns true if it passes, false if it fails
                    StrongPassword passStrength = new StrongPassword();
                    if (passStrength.Check(newDetails.Newpass) == true)
                    {
                        // Do the update
                        var response = await _userService.ChangePass(newDetails);

                        return Ok(response);
                    }
                    else
                    {
                        // Send a message to tell user of the error
                        var response = new ServiceResponse<string>();
                        response.Data = null;
                        response.Success = false;
                        response.Message = "Password MUST be at least 8 characters. Include Uppercase, lowercase, a number and a special character.";

                        _logger.LogInformation("{Time} - Error - Password Complexity Failed. UserController.ChangePass - ID: {1}", DateTime.UtcNow, newDetails.Id);


                        return Ok(response);
                    }
                }

                // Send a message to tell user of the error
                var serviceResponse = new ServiceResponse<string>();
                serviceResponse.Data = null;
                serviceResponse.Success = false;
                serviceResponse.Message = "Validation Failed.";

                _logger.LogInformation("{Time} - Validation Failed in UserController.ChangePass - ID: {1}", DateTime.UtcNow, newDetails.Id);


                return Ok(serviceResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError("{Time} - Exception in UserController.ChangePass - {1}", DateTime.UtcNow, ex.Message);

                return NotFound();
            }
        }



        /*
        // DELETE
        [Authorize(Roles = "admin")]
        [HttpDelete]
        [Route("{id}")]
        public async Task<ActionResult<ServiceResponse<List<GetUserDto>>>> Delete(int id)
        {
            var response = await _userService.DeleteUser(id);

            if (response.Data == null)
            {
                Console.Write("Error in UserController.DeleteUser - Null data was recieved.");
                return NotFound(response);
            }

            return Ok(response);
        }
        */

        // We don't want to return a whole list of ALL the users minus the deleted user. As above. Could be 1000s of records.
        // Lets just send back a http 200OK response and a ServiceResponse.Success = true if we could delete the user and ServiceResponse.Success = false if we couldn't

        // DELETE
        [Authorize(Roles = "admin")]
        [HttpDelete]
        [Route("{id}")]
        public async Task<ActionResult<ServiceResponse<List<GetUserDto>>>> Delete(int id)
        {
            try
            {
                // Validation
                IValidate validate = new Validate();
                int passValidation = validate.numberValidation(id.ToString());

                // 0 is a pass, 1 is a fail.
                if (passValidation == 0)
                {
                    var response = await _userService.DeleteUser(id);

                    if (response.Success == false)
                    {
                        _logger.LogInformation("{Time} - Error in UserController.DeleteUser - User Deletion Failed - ID: {1} ", DateTime.UtcNow, id.ToString());

                        return NotFound();
                    }

                    return Ok(response);
                }

                // Send a message to tell user of the error
                var serviceResponse = new ServiceResponse<List<GetUserDto>>();
                serviceResponse.Data = null;
                serviceResponse.Success = false;
                serviceResponse.Message = "Validation Failed.";

                _logger.LogInformation("{Time} - VValidation Failed in UserController.Delete - ID: {1} ", DateTime.UtcNow, id.ToString());


                return Ok(serviceResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError("{Time} - Exception in UserController.Delete - {1}", DateTime.UtcNow, ex.Message);

                return NotFound();
            }
        }


        // PREMIUM UPGRADE (Admin Website)
        // PATCH request. PUT replaces the whole record, PATCH only replaces one part of it.
        // It should only upgrade basic users to premium, we don't want to downgrade the admin user accidentally.
        // https://localhost:44378/User/Upgrade/11
        [Authorize(Roles = "basic, admin")]
        [HttpPatch]
        [Route("Upgrade/{id}")]
        public async Task<ActionResult<ServiceResponse<GetUserDto>>> upgradeToPremium(int id)
        {
            try
            {
                // Validation
                IValidate validate = new Validate();
                int passValidation = validate.numberValidation(id.ToString());

                // 0 is a pass, 1 is a fail.
                if (passValidation == 0)
                {
                    var response = await _userService.upgradeToPremium(id);

                    if (response.Success == false)
                    {
                        _logger.LogInformation("{Time} - Error in UserController.UpgradeToPremium - upgrade Failed. User may be premium already. ID: {1}", DateTime.UtcNow, id.ToString());

                        return NotFound();
                    }

                    return Ok(response);
                }

                // Send a message to tell user of the error
                var serviceResponse = new ServiceResponse<GetUserDto>();
                serviceResponse.Data = null;
                serviceResponse.Success = false;
                serviceResponse.Message = "Validation Failed.";

                _logger.LogInformation("{Time} - Validation Failed in UserController.upgradeToPremium - ID: {1} ", DateTime.UtcNow, id.ToString());



                return Ok(serviceResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError("{Time} - Exception in UserController.upgradeToPremium - {1}", DateTime.UtcNow, ex.Message);

                return NotFound();
            }
        }

        // PREMIUM UPGRADE (User Front End)
        // PATCH request. PUT replaces the whole record, PATCH only replaces one part of it.
        // USe this as the success url for paypal / Google pay payments.
        // https://localhost:44378/User/Paid4Premium/<username>
        [Authorize(Roles = "basic, admin")]
        [HttpPatch]
        [Route("Paid4Premium/{username}")]
        public async Task<ActionResult<ServiceResponse<string>>> Paid4Premium(string username)
        {
            try
            {
                // Validation
                IValidate validate = new Validate();
                int passValidation = validate.alphabetValidation(username.ToString());

                // 0 is a pass, 1 is a fail.
                if (passValidation == 0)
                {
                    var response = await _userService.Paid4Premium(username);

                    if (response.Success == false)
                    {
                        _logger.LogInformation("{Time} - Error in UserController.Paid4Premium - upgrade Failed. User may be premium already. USER: {1}", DateTime.UtcNow, username.ToString());

                        return NotFound();
                    }

                    return Ok(response);
                }

                // Send a message to tell user of the error
                var serviceResponse = new ServiceResponse<string>();
                serviceResponse.Data = null;
                serviceResponse.Success = false;
                serviceResponse.Message = "Validation Failed.";

                _logger.LogInformation("{Time} - Validation Failed in UserController.Paid4Premium -  USER: {1}", DateTime.UtcNow, username.ToString());



                return Ok(serviceResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError("{Time} - Exception in UserController.Paid4Premium - {1}", DateTime.UtcNow, ex.Message);

                return NotFound();
            }
        }



        // Verify Email Address
        // Has to allow unauthorized users as this is part of the registration process, and the user's aren't fully set up yet.
        // https://localhost:44378/User/VerifyEmail
        [AllowAnonymous]
        [HttpPost]
        [Route("/User/VerifyEmail/")]
        public async Task<ActionResult<ServiceResponse<string>>> VerifyEmail(VerifyEmailDto details)
        {
            try
            {
                // Validate the Id
                // Each of the validations returns 0 if it passed or 1 if it failed.
                IValidate validate = new Validate();
                int passValidation = validate.numberValidation(details.Id.ToString());

                // 0 is a pass, 1+ is a fail.
                if (passValidation == 0)
                {
                    
                    // Verify the email address
                    // Check the token supplied matches the one we have for that ID in our database.
                    var response = await _userService.EmailVerified(details);


                    // Newsletter Subscription - SendInBlue.com
                    // Only subscribe user once their email address is verified AND they have ticked the subscribe checkbox
                    // We will look up userID in the newsletter module and check if user.Newsletter == true.
                    if(response.Success == true)
                    {
                        var newsletterResponse = await _newsletter.Subscribe(details);
                    }


                    return Ok(response);
                }

                // Send a message to tell user of the error
                var serviceResponse = new ServiceResponse<string>();
                serviceResponse.Data = null;
                serviceResponse.Success = false;
                serviceResponse.Message = "Validation Failed.";

                _logger.LogInformation("{Time} - Validation Failed in UserController.VerifyEmail - ID: {1}", DateTime.UtcNow, details.Id);


                return Ok(serviceResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError("{Time} - Exception in UserController.VerifyEmail - {1}", DateTime.UtcNow, ex.Message);

                return NotFound();
            }
        }


    }
}
