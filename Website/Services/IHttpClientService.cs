using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Website.Models;
using Website.Dtos.Users;

namespace Website.Services
{
    public interface IHttpClientService
    {
        Task<ServiceResponse<LoginResponseDto>> Login(UserLoginDto user);                           // Login and get JWT token, Email, Username, Role

        /* Task<List<GetUserDto>> GetAllUsers(string token);                       // Get All of the users. */

        Task<GetPagedUserDto> GetPagedUsers(string token, int currentPage);      // Paged Reqests. Table Pagination. Get 100 users at a time. Display "pages" on tables.

        Task<ServiceResponse<GetUserDto>> GetSingleUser(string token, int id);                  // Get a single user by Id.

        Task<ServiceResponse<GetUserDto>> GetUserByUsername(string token, string username);     // Get a single user by username.

        Task<ServiceResponse<GetUserDto>> UpdateUser(string token, UpdateUserDto update);       // Update Exisiting User.

        Task<ServiceResponse<List<GetUserDto>>> AddUser(string token, AddUserDto user);      // Add new user.

        Task<ServiceResponse<List<GetUserDto>>> DeleteUser(string token, int id);               // Delete a user. ServiceResponse.Success = true if the user was deleted.

        Task<ServiceResponse<string>> ChangePass(ChangePassDto newDetails);      // Change a user's password. User identified by Id.

        Task<ServiceResponse<string>> EmailVerified(VerifyEmailDto details);        // Verify user's email address.
    }
}
