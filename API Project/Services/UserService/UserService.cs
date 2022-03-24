using API_Project.Data;
using API_Project.Dtos.User;
using API_Project.Models;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API_Project.Services.UserService
{
    public class UserService : IUserService
    {
        private readonly IMapper _mapper;                                   // Automapper
        private readonly DataContext _context;                              // Entity DB
        private readonly ILogger<UserService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;         // To access our session object outside of a Controller you need to inject HttpContext.Session

        // Constructor
        public UserService(IMapper mapper, DataContext context, ILogger<UserService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _mapper = mapper;
            _context = context;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }


        // GET THE USER ID FROM THE CURRENT HTTP SESSION
        // User has logged in, so we can use HttpContextAccessor to grab to user's name identifier (the user ID).
        private int GetUserId() => int.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));


        // Get User Role
        // Grab the logged in user's role, you may need to know if they're basic, premium or admin to allow access to certain pages.
        private string GetUserRole() => _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Role);


        //Password Hashing
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }


        /*
        // GET ALL
        public async Task<ServiceResponse<List<GetUserDto>>> GetAllUsers()
        {
            var serviceResponse = new ServiceResponse<List<GetUserDto>>();

            // Check if the user role is "admin" ? [true] Show all users in database : [false] return only their user info

            List<User> dbUsers =
            GetUserRole().Equals("admin") ?
            await _context.Users.ToListAsync() :
            await _context.Users.Where(u => u.Id == GetUserId()).ToListAsync();


            serviceResponse.Data = dbUsers.Select(c => _mapper.Map<GetUserDto>(c)).ToList();
            return serviceResponse;
        }
        */

        // GET 10
        public async Task<ServicePaged<GetPagedUserDto>> GetPagedUsers(int currentPage)
        {
            int maxRows = 10;

            // Need to create a Data object to be placed inside the service request.
            var servicePaged = new ServicePaged<GetPagedUserDto>();
            var Data = new GetPagedUserDto();

            // Check if the user role is "admin" ? [true] Show all users in database : [false] return only their user info

            List<User> dbUsers =
            GetUserRole().Equals("admin") ?
            await _context.Users.OrderBy(u => u.Id).Skip((currentPage - 1) * maxRows).Take(maxRows).ToListAsync() :
            await _context.Users.Where(u => u.Id == GetUserId()).ToListAsync();

            double pageCount = (double)((decimal)_context.Users.Count() / Convert.ToDecimal(maxRows));
            int pages = (int)Math.Ceiling(pageCount);

            Data.PageCount = pages;
            Data.CurrentPageIndex = currentPage;
            Data.Users = dbUsers.Select(c => _mapper.Map<GetUserDto>(c)).ToList();

            servicePaged.Data = Data;

            return servicePaged;
        }


        // GET BY ID
        public async Task<ServiceResponse<GetUserDto>> GetUserById(int id)
        {
            var serviceResponse = new ServiceResponse<GetUserDto>();

            // Get user by ID from the database
            // Check if the user role is "admin" ? [true] Lookup the specified user Id : [false] return only their user info

            var dbUser =
                GetUserRole().Equals("admin") ?
                await _context.Users.FirstOrDefaultAsync(c => c.Id == id) :
                await _context.Users.Where(u => u.Id == GetUserId()).FirstOrDefaultAsync();

            // FirstOrDefault() returns null if nothing is found.
            if (dbUser != null)
            {
                serviceResponse.Data = _mapper.Map<GetUserDto>(dbUser);
            }
            else
            {
                // Send a message to tell user of the error
                serviceResponse.Success = false;
                serviceResponse.Message = "Sorry, we couldn't find that user.";
                _logger.LogInformation("{Time} - Error - Admin area search for an unknown user. UserService.GetUserById - ID: {1}", DateTime.UtcNow, id);

            }

            return serviceResponse;
        }


        // GET BY USERNAME
        public async Task<ServiceResponse<GetUserDto>> GetUserByUsername(string username)
        {
            var serviceResponse = new ServiceResponse<GetUserDto>();

            // Get user by USERNAME from the database
            // Check if the user role is "admin" ? [true] Lookup the specified user Id : [false] return only their user info

            var dbUser =
                GetUserRole().Equals("admin") ?
                await _context.Users.FirstOrDefaultAsync(c => c.Username.Contains(username)) :                          // .Conatins() is linq for SQL LIKE.
                await _context.Users.Where(u => u.Id == GetUserId()).FirstOrDefaultAsync();

            // FirstOrDefault() returns null if nothing is found.
            if (dbUser != null)
            {
                serviceResponse.Data = _mapper.Map<GetUserDto>(dbUser);
            }
            else
            {
                // Send a message to tell user of the error
                serviceResponse.Success = false;
                serviceResponse.Message = "Sorry, we couldn't find that user.";
                _logger.LogInformation("{Time} - Error - Admin area search for an unknown user. UserService.GetUserByUsername - USERNAME: {1}", DateTime.UtcNow, username);
            }

            return serviceResponse;
        }

        /*
        // ADD
        public async Task<ServiceResponse<List<GetUserDto>>> AddUser(AddUserDto newUser)
        {
            // ServiceResponse is a generic object we send back from the server. Allows more detailed error messages than just HTTP status codes. Has Data, Success and Message fields.
            var serviceResponse = new ServiceResponse<List<GetUserDto>>();

            try
            {

                // Map or Copy the passed in newUser incoming DTO to a user object so they have the same fields and data.
                // Check if the user role is "admin" ? [true] Map the info to User user : [false] return null
                User user =
                    GetUserRole().Equals("admin") ?
                        _mapper.Map<User>(newUser) :
                        null;

                if (user != null)
                {
                    // Hash Password
                    // Grab the password from the newUserDto then hash it.
                    // CreatePasswordHash() at top of page.
                    CreatePasswordHash(newUser.Password, out byte[] passwordHash, out byte[] passwordSalt);
                    user.PasswordHash = passwordHash;
                    user.PasswordSalt = passwordSalt;


                    // Add a new user to the database. _context is Entity ORM which talks to the database.
                    // Characters is the Entity DB model.
                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();              // SaveChangesAsync() is an Entity command to write changes to the database.

                    // Map or Copy the above updated users list to our outgoing DTO so the server can respond with it
                    serviceResponse.Data = await _context.Users
                        .Select(c => _mapper.Map<GetUserDto>(c)).ToListAsync();

                }
                else
                {
                    // Send a message to tell user of the error
                    serviceResponse.Success = false;
                    serviceResponse.Message = "Sorry, we couldn't find that user";
                    Console.WriteLine("Error, admin search for a user we couldn't find. UserService.AddUser");
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = "Error in Admin Section.";
                Console.WriteLine("Error in UserService.AddUser - " + ex.Message);
            }

            return serviceResponse;
        }
        */

        // We don't want to return a whole list of ALL the users plus the added user. As above. Could be 1000s of records.
        // Lets just send back a http 200OK response and a ServiceResponse.Success = true if we could add the user and ServiceResponse.Success = false if we couldn't

        // ADD
        public async Task<ServiceResponse<List<GetUserDto>>> AddUser(AddUserDto newUser)
        {
            // ServiceResponse is a generic object we send back from the server. Allows more detailed error messages than just HTTP status codes. Has Data, Success and Message fields.
            var serviceResponse = new ServiceResponse<List<GetUserDto>>();

            try
            {

                // Map or Copy the passed in newUser incoming DTO to a user object so they have the same fields and data.
                // Check if the user role is "admin" ? [true] Map the info to User user : [false] return null
                User user =
                    GetUserRole().Equals("admin") ?
                        _mapper.Map<User>(newUser) :
                        null;

                if (user != null)
                {
                    // Hash Password
                    // Grab the password from the newUserDto then hash it.
                    // CreatePasswordHash() at top of page.
                    CreatePasswordHash(newUser.Password, out byte[] passwordHash, out byte[] passwordSalt);
                    user.PasswordHash = passwordHash;
                    user.PasswordSalt = passwordSalt;

                    // New users will have their account unlocked
                    // Date locked needs a value, can't be null so just use current time.
                    user.AccountLocked = false;
                    user.DateLocked = DateTime.UtcNow;

                    // Add a new user to the database. _context is Entity ORM which talks to the database.
                    // Characters is the Entity DB model.
                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();              // SaveChangesAsync() is an Entity command to write changes to the database.

                    // Just return Success = true. Lets not send back a whole list of potentially 1000s of users.
                    serviceResponse.Success = true;
                    serviceResponse.Message = "User Added";

                }
                else
                {
                    // Send a message to tell user of the error
                    serviceResponse.Success = false;
                    serviceResponse.Message = "Sorry, we couldn't find that user";
                    _logger.LogInformation("{Time} - Error - searched for a user we couldn't find. UserService.AddUser - EMAIL: {1}  USERNAME: {2}", DateTime.UtcNow, newUser.Email, newUser.Username);
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = "Error when adding user.";
                _logger.LogError("{Time} - Exception in UserService.AddUser - {1}", DateTime.UtcNow, ex.Message);

            }

            return serviceResponse;
        }



        // UPDATE
        public async Task<ServiceResponse<GetUserDto>> UpdateUser(UpdateUserDto updatedUser)
        {
            var serviceResponse = new ServiceResponse<GetUserDto>();

            try
            {

                // Update user by ID from the database
                // Check if the user role is "admin" ? [true] Get the user's info using Id : [false] return null
                User user =
                    GetUserRole().Equals("admin") ?
                    await _context.Users.FirstOrDefaultAsync(u => u.Id == updatedUser.Id) :
                    null;

                // FirstOrDefault() returns null if nothing is found.
                if (user != null)
                {
                    // Update the user's fields
                    // We have recieved an incoming DTO named updatedUser with the data we want to update.
                    // Do this manually as AutoMapper would update everything, erasing data we want to keep like history/stats.
                    user.Email = updatedUser.Email;
                    user.EmailVerified = updatedUser.EmailVerified;
                    user.Username = updatedUser.Username;
                    user.Role = updatedUser.Role;
                    user.AccountLocked = updatedUser.AccountLocked;
                    user.DateLocked = updatedUser.DateLocked;

                    // If the password is sent as "#keepSame#" then we want to keep the existing password
                    if (updatedUser.Password != "#keepSame#")
                    {
                        // Grab the password from the UpdatedUserDto then hash it.
                        // CreatePasswordHash() at top of page.
                        CreatePasswordHash(updatedUser.Password, out byte[] passwordHash, out byte[] passwordSalt);
                        user.PasswordHash = passwordHash;
                        user.PasswordSalt = passwordSalt;
                    }


                    // Entity - Save changes to the database
                    await _context.SaveChangesAsync();

                    // Map or Copy the above updated character object to our outgoing DTO so the server can respond with it
                    serviceResponse.Data = _mapper.Map<GetUserDto>(user);
                }
                else
                {
                    // Send a message to tell user of the error
                    serviceResponse.Success = false;
                    serviceResponse.Message = "Sorry, we couldn't find that user";
                    _logger.LogInformation("{Time} - Error - searched for a user we couldn't find. UserService.UpdateUser - ID: {1}  EMAIL: {2}  USERNAME: {3}", DateTime.UtcNow, updatedUser.Id, updatedUser.Email, updatedUser.Username);
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = "Error when updating user.";
                _logger.LogError("{Time} - Exception in UserService.UpdateUser - {1}", DateTime.UtcNow, ex.Message);

            }

            return serviceResponse;

        }


        // Update Self
        // Allows useres to update their own email username and password.
        public async Task<ServiceResponse<UpdateSelfDto>> UpdateSelf(UpdateSelfDto updateSelf)
        {
            var serviceResponse = new ServiceResponse<UpdateSelfDto>();

            try
            {

                // Update user by ID from the database
                // Get the User's ID from the http session, check it's the same as the ID they want to update. Users can ONLY update themselves.
                // Check if the user's ID matches the updateSelf.Id ? [true] Get the user's info using Id : [false] return null
                User user =
                    GetUserId().Equals(updateSelf.Id) ?
                    await _context.Users.FirstOrDefaultAsync(u => u.Id == updateSelf.Id) :
                    null;

                // FirstOrDefault() returns null if nothing is found.
                if (user != null)
                {
                    // Update the user's fields
                    // We have recieved an incoming DTO named updatedUser with the data we want to update.
                    // Do this manually as AutoMapper would update everything, erasing data we want to keep like history/stats.
                    user.Email = updateSelf.Email;
                    user.Username = updateSelf.Username;


                    // If the password is sent as "#keepSame#" then we want to keep the existing password
                    if (updateSelf.Password != "#keepSame#")
                    {
                        // Grab the password from the UpdatedUserDto then hash it.
                        // CreatePasswordHash() at top of page.
                        CreatePasswordHash(updateSelf.Password, out byte[] passwordHash, out byte[] passwordSalt);
                        user.PasswordHash = passwordHash;
                        user.PasswordSalt = passwordSalt;
                    }


                    // Entity - Save changes to the database
                    await _context.SaveChangesAsync();

                    // Map or Copy the above updated character object to our outgoing DTO so the server can respond with it
                    serviceResponse.Data = _mapper.Map<UpdateSelfDto>(user);
                }
                else
                {
                    // Send a message to tell user of the error
                    serviceResponse.Success = false;
                    serviceResponse.Message = "Sorry, you can only update your own details.";
                    _logger.LogInformation("{Time} - Error - requested an update for an ID we couldn't find. UserService.UpdateSelf - ID: {1}", DateTime.UtcNow, updateSelf.Id);
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = "User Update Error.";
                _logger.LogError("{Time} - Exception in UserService.UpdateSelf - {1}", DateTime.UtcNow, ex.Message);

            }

            return serviceResponse;
        }





        /*
        // DELETE
        public async Task<ServiceResponse<List<GetUserDto>>> DeleteUser(int id)
        {
            var serviceResponse = new ServiceResponse<List<GetUserDto>>();

            try
            {
                // FirstOrDefault() returns null if nothing is found.
                // First() throws an exception if there's no matches.

                // Check if the user role is "admin" ? [true] Get the user's data so we can delete it : [false] return null
                User user =
                    GetUserRole().Equals("admin") ?
                    await _context.Users.FirstOrDefaultAsync(u => u.Id == id) :
                    null;


                // FirstOrDefault() returns null if nothing is found.
                if (user != null)
                {

                    _context.Users.Remove(user);                                   // Delete the user

                    await _context.SaveChangesAsync();                             // Entity - Save changes to the database

                    // We want to return the list of all users, the deleted user wont be in it anymore.
                    serviceResponse.Data = _context.Users
                        .Select(c => _mapper.Map<GetUserDto>(c)).ToList();
                }
                else
                {
                    // Send a message to tell user of the error
                    serviceResponse.Success = false;
                    serviceResponse.Message = "Sorry, we couldn't find that user.";
                    Console.WriteLine("Error, admin search for a user we couldn't find. UserService.DeleteUser");
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = "Error in AdminSection.";
                Console.WriteLine("Error in UserService.DeleteUser - " + ex.Message);
            }

            return serviceResponse;
        }
        */

        // We don't want to return a whole list of ALL the users minus the deleted user. As above. Could be 1000s of records.
        // Lets just send back ServiceResponse.Success = true if we can delete the user and ServiceResponse.Success = false if we can't

        // DELETE
        public async Task<ServiceResponse<List<GetUserDto>>> DeleteUser(int id)
        {
            var serviceResponse = new ServiceResponse<List<GetUserDto>>();

            try
            {
                // FirstOrDefault() returns null if nothing is found.
                // First() throws an exception if there's no matches.

                // Check if the user role is "admin" ? [true] Get the user's data so we can delete it : [false] return null
                User user =
                    GetUserRole().Equals("admin") ?
                    await _context.Users.FirstOrDefaultAsync(u => u.Id == id) :
                    null;


                // FirstOrDefault() returns null if nothing is found.
                if (user != null)
                {

                    _context.Users.Remove(user);                                   // Delete the user

                    await _context.SaveChangesAsync();                             // Entity - Save changes to the database

                    // Just return Success = true. Lets not send back a whole list of potentially 1000s of users.
                    serviceResponse.Success = true;
                    serviceResponse.Message = "User Deleted";
                }
                else
                {
                    // Send a message to tell user of the error
                    serviceResponse.Success = false;
                    serviceResponse.Message = "Sorry, we couldn't find that user.";
                    _logger.LogInformation("{Time} - Error - searched for a user we couldn't find. UserService.DeleteUser - ID: {1}", DateTime.UtcNow, id);
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = "Error in AdminSection.";
                _logger.LogError("{Time} - Exception in UserService.DeleteUser - {1}", DateTime.UtcNow, ex.Message);

            }

            return serviceResponse;
        }


        // PREMIUM UPGRADE (Admin Website)
        // Upgrade Basic user role to Premium
        public async Task<ServiceResponse<GetUserDto>> upgradeToPremium(int id)
        {
            var serviceResponse = new ServiceResponse<GetUserDto>();

            try
            {

                User user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

                // Check user role isn't admin. Prevents us from downgrading the admin user to premium accidentally.
                if (user != null && user.Role != "admin")
                {

                    user.Role = "premium";                                          // Change role to premium

                    await _context.SaveChangesAsync();                             // Entity - Save changes to the database

                    // Return Success = true if we can do the upgrade
                    serviceResponse.Success = true;
                    serviceResponse.Message = "User upgraded to premium";
                }
                else
                {
                    // Send a message to tell user of the error
                    serviceResponse.Success = false;
                    serviceResponse.Message = "Sorry, we couldn't find that user.";
                    _logger.LogInformation("{Time} - Error - we couldn't find the user to upgrade them. UserService.upgradeToPremium - ID: {1}", DateTime.UtcNow, id);
                }

            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = "Error with upgrade to premium";
                _logger.LogError("{Time} - Exception in UserService.upgradeToPremium - {1}", DateTime.UtcNow, ex.Message);

            }

            return serviceResponse;
        }


        // PREMIUM UPGRADE (User Front End)
        // Upgrade Basic user role to Premium
        public async Task<ServiceResponse<string>> Paid4Premium(string username)
        {
            var serviceResponse = new ServiceResponse<string>();

            try
            {

                User user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

                // Check user role isn't admin. Prevents us from downgrading the admin user to premium accidentally.
                if (user != null && user.Role != "admin")
                {

                    user.Role = "premium";                                          // Change role to premium

                    await _context.SaveChangesAsync();                             // Entity - Save changes to the database

                    // Return Success = true if we can do the upgrade
                    serviceResponse.Success = true;
                    serviceResponse.Message = "User upgraded to premium";
                }
                else
                {
                    // Send a message to tell user of the error
                    serviceResponse.Success = false;
                    serviceResponse.Message = "Sorry, we couldn't find that user.";
                    _logger.LogInformation("{Time} - Error - we couldn't find the user to upgrade them. Paid4Premium - USER: {1}", DateTime.UtcNow, username);
                }

            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = "Error with upgrade to premium";
                _logger.LogError("{Time} - Exception in UserService.Paid4Premium - {1}", DateTime.UtcNow, ex.Message);

            }

            return serviceResponse;
        }


        // Change User Password
        public async Task<ServiceResponse<string>> ChangePass(ChangePassDto newDetails)
        {
            var serviceResponse = new ServiceResponse<string>();

            try
            {

                // Update user by ID from the database
                // Users can only update their own record as we check their UserID equals the ID passed in.
                User user =
                    GetUserId().Equals(newDetails.Id) ?
                    await _context.Users.FirstOrDefaultAsync(u => u.Id == newDetails.Id) :
                    null;

                // FirstOrDefault() returns null if nothing is found.
                if (user != null)
                {
                    // Check the reset pass token matches what was saved for that user in the database.
                    if (user.ResetToken == newDetails.Token)
                    {
                        // Hash the new password then save it.
                        CreatePasswordHash(newDetails.Newpass, out byte[] passwordHash, out byte[] passwordSalt);
                        user.PasswordHash = passwordHash;
                        user.PasswordSalt = passwordSalt;


                        // Entity - Save changes to the database
                        await _context.SaveChangesAsync();

                        serviceResponse.Success = true;
                        serviceResponse.Message = "Password Changed Sucessfully!";
                    }
                    else
                    {
                        // Reset Token Doesn't Match
                        serviceResponse.Success = false;
                        serviceResponse.Message = "Error, Reset Token Doesn't Match.";
                        _logger.LogInformation("{Time} - Error - Password Reset Token didn't match. UserService.ChangePass - ID: {1}", DateTime.UtcNow, newDetails.Id);
                    }

                }
                else
                {
                    // User object was null
                    serviceResponse.Success = false;
                    serviceResponse.Message = "Error, no user found!";
                    _logger.LogInformation("{Time} - Error - user requested an update for an ID we couldn't find. UserService.ChangePass - ID: {1}", DateTime.UtcNow, newDetails.Id);
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = "Password Change Error.";
                _logger.LogError("{Time} - Exception in UserService.ChangePass - {1}", DateTime.UtcNow, ex.Message);

            }

            return serviceResponse;
        }



        // Verify User's Email Address
        public async Task<ServiceResponse<string>> EmailVerified(VerifyEmailDto details)
        {
            var serviceResponse = new ServiceResponse<string>();

            try
            {
                // Get user by ID from the database
                User user = await _context.Users.FirstOrDefaultAsync(u => u.Id == details.Id);

                // FirstOrDefault() returns null if nothing is found.
                if (user != null)
                {
                    // Check the email verification token matches what was saved for that user in the database.
                    if (user.EmailVerifyToken == details.Token)
                    {
                        // Mark EmailVerified as true.
                        user.EmailVerified = true;

                        // Entity - Save changes to the database
                        await _context.SaveChangesAsync();

                        serviceResponse.Success = true;
                        serviceResponse.Message = "Email Address Successfully Verified!";
                    }
                    else
                    {
                        // Reset Token Doesn't Match
                        serviceResponse.Success = false;
                        serviceResponse.Message = "Error, Reset Token Doesn't Match.";
                        _logger.LogInformation("{Time} - Error - Email Verification Token didn't match. UserService.EmailVerified - ID: {1}", DateTime.UtcNow, details.Id);
                    }

                }
                else
                {
                    // User object was null
                    serviceResponse.Success = false;
                    serviceResponse.Message = "Error, no user found!";
                    _logger.LogInformation("{Time} - Error - user requested an update for an ID we couldn't find. UserService.EmailVerified - ID: {1}", DateTime.UtcNow, details.Id);
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
