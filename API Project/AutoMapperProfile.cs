using API_Project.Dtos.User;
using API_Project.Models;
using AutoMapper;                                       // install AutoMapper.Extensions.Microsoft.DependencyInjection
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API_Project
{

    // AutoMapper Mappings Class
    // We map Character model to the DTO
    public class AutoMapperProfile : Profile
    {
        //Constructor
        public AutoMapperProfile()
        {
            // GETs
            CreateMap<User, GetUserDto>();
            CreateMap<User, UpdateSelfDto>();

            // POSTs
            CreateMap<AddUserDto, User>();
        }
    }
}
