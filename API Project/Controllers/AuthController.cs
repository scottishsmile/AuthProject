using API_Project.Data;
using API_Project.Dtos.User;
using API_Project.Models;
using API_Project.Services.UserService;
using API_Project.Services.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API_Project.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _authRepo;
        private readonly ILogger<AuthController> _logger;
        private readonly IUserService _userService;

        // Constructor
        // Inject IAuthRepository, that way any changes to it don't need to be made here. De-coupled.
        public AuthController(IAuthRepository authRepo, ILogger<AuthController> logger, IUserService userService )
        {
            _authRepo = authRepo;
            _logger = logger;
            _userService = userService;
        }


        // User Registration
        [HttpPost]
        [Route("Register")]
        public async Task<ActionResult<ServiceResponse<int>>> Register(UserRegisterDto request)
        {
            try
            {
                // We will validate the Username, Email and Password in the UserRegisterDto.
                // Each of the validations returns 0 if it passed or 1 if it failed.
                IValidate validate = new Validate();
                int passValidation = validate.alphabetValidation(request.Username) + validate.emailValidation(request.Email) + validate.alphabetValidation(request.Password);

                // 0 is a pass, 1+ is a fail.
                if (passValidation == 0)
                {

                    // Calls AuthRepository.Register() which also checks if the user currently exists.
                    var response = await _authRepo.Register(
                        new User { Username = request.Username, Email = request.Email, Newsletter = request.Newsletter }, request.Password
                    );

                    if (!response.Success)
                    {
                        return BadRequest(response);
                    }

                    return Ok(response);
                }

                // Send a message to tell user of the error
                var serviceResponse = new ServiceResponse<int>();
                serviceResponse.Success = false;
                serviceResponse.Message = "Validation Failed.";

                _logger.LogInformation("{Time} - Validation Failed in AuthController.Register - USER: {1} - EMAIL: {2}", DateTime.UtcNow, request.Username, request.Email);

                return NotFound(serviceResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError("{Time} - Exception in AuthController.Register - {1}", DateTime.UtcNow, ex.Message);

                return NotFound();
            }

        }



        // User Login
        [HttpPost]
        [Route("Login")]
        public async Task<ActionResult<ServiceResponse<LoginResponseDto>>> Login(UserLoginDto request)
        {
            try
            {
                // We will validate the Username and Password in the UserRegisterDto.
                // Each of the validations returns 0 if it passed or 1 if it failed.
                // Username could be username or email address. The alphabetValidation is good enough as it checks the blacklist words for SQL and stops length of string at 50 characters.
                IValidate validate = new Validate();
                int passValidation = validate.alphabetValidation(request.Username) + validate.alphabetValidation(request.Password);

                // 0 is a pass, 1+ is a fail.
                if (passValidation == 0)
                {
                    // Calls AuthRepository.Login()
                    // request.Username can hold either the Username string or the Emaill address string. That way user can log in with either.
                    var response = await _authRepo.Login(
                        request.Username, request.Password
                    );

                    if (!response.Success)
                    {
                        return BadRequest(response);
                    }

                    return Ok(response);
                }

                // Send a message to tell user of the error
                var serviceResponse = new ServiceResponse<LoginResponseDto>();
                serviceResponse.Data = null;
                serviceResponse.Success = false;
                serviceResponse.Message = "Validation Failed.";
                _logger.LogInformation("{Time} - Validation Failed in AuthController.Login - USER: {1}", DateTime.UtcNow, request.Username);

                return NotFound(serviceResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError("{Time} - Exception in AuthController.Login - {1}", DateTime.UtcNow, ex.Message);
                return NotFound();
            }
        }


        // Reset Password
        // https://localhost:44378/Auth/ForgotPass/angel95@hotmail.com
        [HttpGet]
        [Route("ForgotPass/{emailOrUsername}")]
        public async Task<ActionResult<ServiceResponse<string>>> ForgotPass(string emailOrUsername)
        {
            try
            {
                // We will validate the Username passed in
                // Username could be username or email address. The alphabetValidation is good enough as it checks the blacklist words for SQL and stops length of string at 50 characters.
                IValidate validate = new Validate();
                int passValidation = validate.alphabetValidation(emailOrUsername);

                // 0 is a pass, 1+ is a fail.
                if (passValidation == 0)
                {
                    // Reset Password
                    // Generate JWT token and send reset email to user.
                    var response = await _authRepo.ForgotPass(emailOrUsername);

                    return Ok(response);
                }

                // Send a message to tell user of the error
                var serviceResponse = new ServiceResponse<string>();
                serviceResponse.Success = false;
                serviceResponse.Message = "Validation Failed.";
                _logger.LogInformation("{Time} - Validation Failed in AuthController.ForgotPass - USER: {1}", DateTime.UtcNow, emailOrUsername);

                return NotFound(serviceResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError("{Time} - Exception in AuthController.ForgotPass - {1}", DateTime.UtcNow, ex.Message);
                return NotFound();
            }
            
        }


    }
}
