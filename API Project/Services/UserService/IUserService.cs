using API_Project.Dtos.User;
using API_Project.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API_Project.Services.UserService
{
    // User Management
    // This whole interface is Admin use only.
    public interface IUserService
    {
       /* Task<ServiceResponse<List<GetUserDto>>> GetAllUsers();                  // Get all of our users. */
        Task<ServicePaged<GetPagedUserDto>> GetPagedUsers(int currentPage);     // Paged requests. Get 100 users at a time. Used in tables or GridViews to display only some of the data on "pages"
        Task<ServiceResponse<GetUserDto>> GetUserById(int startingId);          // Lookup a specific user. Search by ID.
        Task<ServiceResponse<GetUserDto>> GetUserByUsername(string username);      // Lookup a specific user. Search by Username.
        Task<ServiceResponse<List<GetUserDto>>> AddUser(AddUserDto newUser);    // Useful to add a user without having to go through the registration process.
        Task<ServiceResponse<GetUserDto>> UpdateUser(UpdateUserDto updatedUser);    // Edit user by PUT request. Admin Use Only. Admins can update any user.
        Task<ServiceResponse<UpdateSelfDto>> UpdateSelf(UpdateSelfDto updateSelf);    // Edit user by PUT request. Allows the user to update their own DB entry. IDs must match.
        Task<ServiceResponse<List<GetUserDto>>> DeleteUser(int Id);                 // Delete user by Id
        Task<ServiceResponse<GetUserDto>> upgradeToPremium(int id);                 // Change users role from basic to premium by PATCH request. Admin Website.
        Task<ServiceResponse<string>> Paid4Premium(string username);                // Change users role from basic to premium by PATCH request. User Front End.
        Task<ServiceResponse<string>> ChangePass(ChangePassDto newDetails);         // Change user password
        Task<ServiceResponse<string>> EmailVerified(VerifyEmailDto details);        // Verifies the user's email. Confirms token and ID match what's in our database and sets user's account Emailverified to true.

    }
}
