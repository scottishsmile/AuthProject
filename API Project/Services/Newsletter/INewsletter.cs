using API_Project.Dtos.User;
using API_Project.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API_Project.Services.Newsletter
{
    public interface INewsletter
    {
        Task<ServiceResponse<string>> Subscribe(VerifyEmailDto details);            // After Email validation, subscibe user to newsletter.
    }
}
