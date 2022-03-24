using API_Project.Dtos.User;
using API_Project.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API_Project.Data
{
    public interface IAuthRepository
    {
        // User Authentication Interface
        Task<ServiceResponse<int>> Register(User user, string password);        // Register a new user. <int> is the user's id.
        Task<ServiceResponse<LoginResponseDto>> Login(string username, string password);  // User Login. Can use Email address OR Username.
        Task<bool> UserExists(string username);                                 // Check if a user exists. Boolean. True = exists.
        Task<ServiceResponse<string>> ForgotPass(string emailOrUsername);          // User can reset their password. Generate JWT token and send reset email.
        

    }
}
