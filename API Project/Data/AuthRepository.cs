using API_Project.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;           // To access appsettings.json
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using API_Project.Services.EmailService;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using API_Project.Dtos.User;

namespace API_Project.Data
{
    public class AuthRepository : IAuthRepository
    {
        private readonly DataContext _context;
        private readonly ILogger<AuthRepository> _logger;
        private readonly IConfiguration _configuration;
        private readonly IOptions<EmailConfig> _emailConfig;                            // EmailConfiguration settings in appsettings.json
        private readonly IEmailService _email;                                           // Send emails


        // Constructor
        // Inject the context again as we will need to access Entity/database
        // The JWT tokens need access to the Token in appsettings.json so inject the configuration variable.
        public AuthRepository(DataContext context, ILogger<AuthRepository> logger, IConfiguration configuration, IOptions<EmailConfig> emailConfig, IEmailService email)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
            _emailConfig = emailConfig;
            _email = email;
        }

        public async Task<ServiceResponse<LoginResponseDto>> Login(string username, string password)
        {
            var response = new ServiceResponse<LoginResponseDto>();

            User user = await _context.Users.FirstOrDefaultAsync(x => x.Username.ToLower().Equals(username.ToLower()));

            // If no matches for the username, try matching to the email address.
            if (user == null)
            {
                user = await _context.Users.FirstOrDefaultAsync(x => x.Email.ToLower().Equals(username.ToLower()));
            }

            // Both the username and email address returned no matches from the database.
            if (user == null)
            {
                response.Data = null;
                response.Success = false;
                response.Message = "Error. Check Username and Password";                     // Don't want to tell hackers that the user wasn't found.
                _logger.LogInformation("{Time} - Username Error - Login.AuthRepository.cs - {1} username not found", DateTime.UtcNow, username);


                return response;                                        // Break out of this function, no need to run the rest.
            }



            // AT THIS POINT WE HAVE A USER OBJECT. Found by either by username or Email address.


            // Account locked?
            // Check if account_locked flag is true
            if (user.AccountLocked == true)
            {
                // Check the Date/Time and see if 1 hr has passed.
                // Compare current time minus time account locked at.
                // If it's greater than 1 hour unlock the account.
                // Login tries should already be set to 0.
                // Duration() gives an absolute value, so there's no chance of any funny negative numbers or anything.
                var compareTimes = (DateTime.UtcNow - user.DateLocked).Duration().TotalHours;

                if (compareTimes > 1)
                {
                    // Set account_locked flag to false
                    // leave date_locked alone it will serve as a record of last time locked. It will also be rewritten on next lock.
                    user.AccountLocked = false;

                    // Entity - Save changes to the database
                    await _context.SaveChangesAsync();

                    // Recursive call to re-run login.
                    Login(username, password);

                    response.Data = null;
                    response.Success = true;
                    response.Message = "Account Unlocked.";
                    _logger.LogInformation("{Time} - Login.AuthRepository.cs - {1} Account Unlocked. ", DateTime.UtcNow, username);


                    return response;                                            // Break out of this function, no need to run the rest.

                }

                response.Data = null;
                response.Success = false;
                response.Message = "Account Is Locked for 1 hr.";
                _logger.LogInformation("{Time} - Login.AuthRepository.cs - {1} Account is Locked. ", DateTime.UtcNow, username);

                return response;                                            // Break out of this function, no need to run the rest.
            }


            // Check Login Attempts
            // If login tries >= 5 temporarily lock the account
            // Account is locked by setting the "account_locked" flag and a date it was locked.
            if (user.LoginTries >= 5)
            {

                // reset login_tries to 0
                user.LoginTries = 0;

                // set the account_locked flag to true
                user.AccountLocked = true;

                // set the Utc Date/time in date_locked
                user.DateLocked = DateTime.UtcNow;

                response.Data = null;
                response.Success = false;
                response.Message = "Too Many Login Attempts. Account Locked for 1 hr.";
                _logger.LogInformation("{Time} - Login.AuthRepository.cs - Too Many Login Attempts by {1}. Account Locked for 1 hr.", DateTime.UtcNow, username);


                // Entity - Save changes to the database
                await _context.SaveChangesAsync();

                return response;                                            // Break out of this function, no need to run the rest.
            }


            // Password Check
            // Does the password match what we have?
            if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
            {
                // User's password doesn't match what we have.

                // Increment the login count.
                user.LoginTries++;

                // Entity - Save changes to the database
                await _context.SaveChangesAsync();

                response.Data = null;
                response.Success = false;
                response.Message = "Error. Check Username and Password";            // Don't want to tell hackers the username exists but the password was wrong.
                _logger.LogInformation("{Time} - Wrong Password - Login.AuthRepository.cs - password {1} does not match {2} username", DateTime.UtcNow, password, username);

            }
            else
            {
                // Password is correct!
                // Is their email address verified?
                if (user.EmailVerified == false)
                {
                    // Re-send verification email
                    // It will not wait the 1hr to stop spam emails.
                    // User has already supplied correct username and password.
                    string emailToken = CreateEmailToken(user.Email);
                    user.EmailVerifyToken = emailToken;
                    user.DateEmailVerifySent = DateTime.UtcNow;
                    _email.ReSendVerificationEmail(user, emailToken);

                    await _context.SaveChangesAsync();                                  // Save changes to the database

                    response.Data = null;
                    response.Success = false;
                    response.Message = "Email address not yet verified. Verification email re-sent.";
                    _logger.LogInformation("{Time} - Login.AuthRepository.cs - Email address not yet verified. Verification email re-sent.", DateTime.UtcNow, username);

                    return response;                                            // Break out of this function, no need to run the rest.
                }
                else
                {
                    // Successful Login

                    // Map user information needed by React client into Dto.
                    // Can't use a mapper - don't want to send the password back, so have to do manually.
                    LoginResponseDto userInfo = new LoginResponseDto();
                    userInfo.Token = CreateToken(user);                             // Password was correct, returning JWT Token.
                    userInfo.Id = user.Id;
                    userInfo.Email = user.Email;
                    userInfo.Username = user.Username;
                    userInfo.Role = user.Role;


                    response.Data = userInfo;             // Password was correct, returning JWT Token and user Info.

                    // reset login tries to 0
                    user.LoginTries = 0;

                    // Entity - Save changes to the database
                    await _context.SaveChangesAsync();
                }
            }

            return response;
        }


        public async Task<ServiceResponse<int>> Register(User user, string password)
        {
            ServiceResponse<int> response = new ServiceResponse<int>();

            // Check if user already exists
            // User exisiting will return true
            if (await UserExists(user.Username))
            {
                response.Success = false;
                response.Message = "Username already exists.";

                _logger.LogInformation("{Time} - Error - Register.AuthRepository - {1} already exists.", DateTime.UtcNow, user.Username);


                return response;

            } else if (await UserExists(user.Email)){

                response.Success = false;
                response.Message = "Email address already exists.";

                _logger.LogInformation("{Time} - Error - Register.AuthRepository - {1} already exists.", DateTime.UtcNow, user.Email);


                return response;
            }

            // Require Strong Password
            StrongPassword passStrength = new StrongPassword();
            if (passStrength.Check(password) == false)
            {
                response.Success = false;
                response.Message = "Password MUST be at least 8 characters. Include Uppercase, lowercase, a number and a special character. Avoid weak words like 'test', 'abc' , '123', 'qwerty' and 'pass'";

                _logger.LogInformation("{Time} - Error - AuthRepository.cs - StrongPassword.cs - Password not strong enough. ID: {1} USER: {2}", DateTime.UtcNow, user.Id, user.Username);


                return response;
            }

            // Verify Email Address
            // Send the user an email with a link to click to verify

            // Generate Verify Email JWT token
            string emailToken = CreateEmailToken(user.Email);

            // Save the token to the user's DB entry for comparison later.
            user.EmailVerifyToken = emailToken;

            // Password hashing
            CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            // New users will have their account unlocked
            // Date locked needs a value, can't be null so just use current time.
            user.AccountLocked = false;
            user.DateLocked = DateTime.UtcNow;

            _context.Users.Add(user);                                           // Add user. Entity. Users is in the DbContext as a model.
            await _context.SaveChangesAsync();                                  // Save changes to the database

            // Send Email
            // Throws a 404 error if the SMTP server doesn't work/authenticate
            // Needs to be done after DB has assigned an Id.
            _email.SendVerificationEmail(user, emailToken);

            response.Data = user.Id;                                            // The server respose will return the new user's Id.

            return response;
        }

        public async Task<bool> UserExists(string username)
        {
            // Check the username matches Any of the database's Usernames.
            if(await _context.Users.AnyAsync(x => x.Username.ToLower().Equals(username.ToLower()))){
                return true;
            }

            // User could also supply an Email addresses instead of username.
            if (await _context.Users.AnyAsync(x => x.Email.ToLower().Equals(username.ToLower())))
            {
                return true;
            }

            return false;
        }


        // Passwords
        // We hash the password
        // We use a salt to help create a unique hash. Otherwise the hash could be decoded as it is always the same value.
        // Hashing uses a default salt and it always produces the same output, so it could be eventually guessed.
        // A salt is a qunique string we will supply to use instead of the default one.
        // Use SHA512 but also bcrypt is a newer alternative

        
        // The "out" keyword - An alias to an esisting variable. Rather than creating a new variable/memory location for the hash and salt just use the existing ones inside hmac.
        // "out" is like "ref" but doesn't need the parameter to already be initialized.

        /*
         * you can pass values in 2 ways, by value or by reference.
         * by value gives the method a copy of your data, so changing the data wont have any effect on the original data
         * by reference  gives the method the memory address of your data, so if the method modifies the data, it changes the original.
         * 
         * Out is a special type of ref, in that you do not need to initialise the variable before you call the method, it can be called with null being passed in.
         */

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            // HMAC - Hash-based Message Authentication Code.
            // hmac.Key just saves the random key it's using to generate the hash.

            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }


        // Password Verification
        // The user supplies a password, we grab our saved password hash from the database.
        // We run the user's suppleid password through the hashing alogorythm to get a computedHash.
        // The SHA512 algorythm will return the same hash everytime the same password is used as input.
        // So therefor, the computedHash should match the database's saved passwordHash if the correct password was used.
        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using(var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));

                // As we are using byte[] arays we need to compare the computed hash to the user's supplied password hash byte by byte
                // If one byte doesn't match it's the wrong password
                for(int i =0; i < computedHash.Length; i++)
                {
                    if(computedHash[i] != passwordHash[i])
                    {
                        return false;       // Passwords dont' match! User Not AUthenicated.
                    }
                }

                return true;            // User Authenticated!

            }
        }



        // Create JWT Token
        // The token key is in appsettings.json
        /*
         *  "AppSettings": {
         *  "Token": "Key_hy67af45DYBvpl#2dfaeg&%6s"
         *  },
         */
        // It is accessed via _configuration in the constructor.
        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),                   // ID as claim
                new Claim(ClaimTypes.Name, user.Username),                                  // Username as claim
                new Claim(ClaimTypes.Role, user.Role)                                       // Default role is "basic" as set in Data.DataContext.cs OnModelCreating()
            };

            // Location of our key, appsettings.json
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value));

            // Create a hash using SHA512 and our key.
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),                                       // List of our claims - ID and Username
                Expires = DateTime.UtcNow.AddHours(1),                                      // Token will expire in 1 hour.
                SigningCredentials = creds                                                  // Use the Sha512 algo above on our key in appsettings.json
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);          // Token Created
        }

        private string CreateEmailToken(string email)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, email),                                  // Email as claim
            };

            // Location of our key, appsettings.json
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value));

            // Create a hash using SHA512 and our key.
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),                                       // List of our claims - Email address
                Expires = DateTime.UtcNow.AddHours(1),                                      // Token will expire in 1 hour.
                SigningCredentials = creds                                                  // Use the Sha512 algo above on our key in appsettings.json
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);          // Token Created
        }


        // Forgot Password
        public async Task<ServiceResponse<string>> ForgotPass(string emailOrUsername)
        {
            ServiceResponse<string> response = new ServiceResponse<string>();

            // Make sure they're not trying to reset our admin pasword
            //      Uses "Email Configuration" settings inside appsettings.json
            //      Use IOptions<IEmailConfig> _emailConfig.Value to access them.
            if (emailOrUsername == _emailConfig.Value.AdminEmailAddress)
            {
                response.Success = false;
                response.Message = "Our Admin Email Password can't be reset!";

                _logger.LogInformation("{Time} - Error - Attempt to reset our admin email! AuthRepository.ForgotPass", DateTime.UtcNow);


                return response;
            }

            // Check if user already exists
            // User not exisiting will return false
            if (!await UserExists(emailOrUsername))
            {
                response.Success = false;
                response.Message = "No record of that username or email.";

                _logger.LogInformation("{Time} - Error - {1} No matches for username or email. AuthRepository.ForgotPass", DateTime.UtcNow, emailOrUsername);


                return response;
            }


            // Lookup User Account
            User user = await _context.Users.FirstOrDefaultAsync(x => x.Username.ToLower().Equals(emailOrUsername.ToLower()));

            // If no matches for the username, try matching to the email address.
            if (user == null)
            {
                user = await _context.Users.FirstOrDefaultAsync(x => x.Email.ToLower().Equals(emailOrUsername.ToLower()));
            }


            // Generate JWT token
            // Token passed to _email EmailService where it is saved to database only if user is allowed to reset their password.
            string token = CreateToken(user);

            // Send Email
            // Throws a 404 error if the SMTP server doesn't work/authenticate
            _email.SendPasswordResetEmail(user, token);


            // Success
            response.Success = true;
            response.Message = "Email sent to your account to reset password.";

            return response;

        }



    }
}
