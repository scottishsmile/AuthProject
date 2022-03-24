using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API_Project.Dtos.User
{
    public class GetPagedUserDto
    {
        // A list of users.
        public List<GetUserDto> Users { get; set; }

        // This Dto is for use with Tables / GridViews that need paging information
        // For counting how many "Pages" are in the database and what page we are on.
        // 500 Records? Break that into 5 pages of 100 records.
        // Do 5 seperate fetch requests with Services.UserService.GetPagedUsers() for 100 records each.
        public int CurrentPageIndex { get; set; }
        public int PageCount { get; set; }
    }
}
