using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using Website.Models;
using Website.Dtos.Users;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

namespace Website.Services
{
    public class HttpClientService : IHttpClientService
    {
        private readonly IHttpClientFactory _httpClient;
        private readonly JsonSerializerOptions _options;
        private readonly ILogger<HttpClientService> _logger;

        public HttpClientService(IHttpClientFactory httpClient, ILogger<HttpClientService> logger)
        {
            _httpClient = httpClient;
            _options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            _logger = logger;
        }


        [HttpPost]
        public async Task<ServiceResponse<LoginResponseDto>> Login(UserLoginDto user)
        {
            try
            {
                // Convert the user's email, username, password into json
                HttpContent httpContent = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");

                // POST
                var httpClient = _httpClient.CreateClient("OurAPI");                         // Startup.cs services has our named client "OurAPI". Base Url addresss there is https://localhost:44331/

                var response = await httpClient.PostAsync("Auth/Login", httpContent);       // https://localhost:44331/Auth/Login

                response.EnsureSuccessStatusCode();                                         // Throws an exception if not 200 OK

                // Json Deserialization
                var stream = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ServiceResponse<LoginResponseDto>>(stream);

                return result;

            }
            catch(Exception ex)
            {
                // Triggers if we get a 404, or anything not a 200 OK.
                _logger.LogError("{Time} - Login Error - HttpClientService.Login  - USER: {1} - {2}", DateTime.UtcNow, user.Username, ex.Message);


                var response = new ServiceResponse<LoginResponseDto>();
                response.Data = null;
                response.Success = false;
                response.Message = "Error Logging In";

                return response;
            }
        }

        /*
        [HttpGet]
        public async Task<List<GetUserDto>> GetAllUsers(string token)
        {
            try
            {
                // GET
                var httpClient = _httpClient.CreateClient("OurAPI");        // Startup.cs services has our named client "OurAPI". Base Url addresss there is https://localhost:44378/

                // Attach token to POST headers
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

                var response = await httpClient.GetAsync("User/GetAllUsers");     // https://localhost:44378/User/GetAllUsers

                response.EnsureSuccessStatusCode();                         // Throws an exception if not 200 OK

                // Json Deserialization
                // Server is returning a List inside a Service response, in the Data section.
                var stream = await response.Content.ReadAsStringAsync();
                var resultData = JsonConvert.DeserializeObject<ServiceResponse<List<GetUserDto>>>(stream);

                // Index.cshtml doesn't accept async tasks Task<List<GetUserDto>> so lets convert the list into a standard List.
                List<GetUserDto> resultList = new List<GetUserDto>();

                foreach (GetUserDto result in resultData.Data)
                {
                    resultList.Add(result);
                }

                return resultList;
            }
            catch
            {
                Console.WriteLine("Get Error 2. HttpClientService.cs");

                List<GetUserDto> errorList = new List<GetUserDto>();
                errorList.Add(new GetUserDto(1, "error@error.com", "error", "error"));

                return errorList;
            }

        }
        */


        [HttpGet]
        public async Task<GetPagedUserDto> GetPagedUsers(string token, int currentPage)
        {
            try
            {
                // GET
                var httpClient = _httpClient.CreateClient("OurAPI");        // Startup.cs services has our named client "OurAPI". Base Url addresss there is https://localhost:44378/

                // Attach token to POST headers
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

                var response = await httpClient.GetAsync("User/GetPagedUsers/" + currentPage);     // https://localhost:44378/User/GetPagedUsers/1

                response.EnsureSuccessStatusCode();                         // Throws an exception if not 200 OK

                // Json Deserialization
                // Server is returning a List inside a Service response, in the Data section.
                var stream = await response.Content.ReadAsStringAsync();
                var resultData = JsonConvert.DeserializeObject<ServicePaged<GetPagedUserDto>>(stream);

                // Index.cshtml doesn't accept async tasks Task<List<GetUserDto>> so lets convert the list into a standard List.
                List<GetUserDto> resultList = new List<GetUserDto>();

                // Mapping
                // Just send the GetPagedUserDto to the webpage, no need for the serviceResponse wrapper status messages.
                GetPagedUserDto serviceResponse = new GetPagedUserDto();
                serviceResponse.Users = resultData.Data.Users;
                serviceResponse.CurrentPageIndex = resultData.Data.CurrentPageIndex;
                serviceResponse.PageCount = resultData.Data.PageCount;

                return serviceResponse;
            }
            catch(Exception ex)
            {
                // Triggers if we get a 404, or anything not a 200 OK.
                _logger.LogError("{Time} - Error - HttpClientService.GetPagedUsers - {1}", DateTime.UtcNow, ex.Message);

                List<GetUserDto> errorList = new List<GetUserDto>();
                errorList.Add(new GetUserDto(1, "error@error.com", false, "error", "error", true, DateTime.UtcNow));

                GetPagedUserDto serviceResponse = new GetPagedUserDto();
                serviceResponse.Users = errorList;
                serviceResponse.CurrentPageIndex = 1;
                serviceResponse.PageCount = 1;

                return serviceResponse;
            }

        }


        [HttpGet]
        public async Task<ServiceResponse<GetUserDto>> GetSingleUser(string token, int id)
        {
            try
            {
                // GET
                var httpClient = _httpClient.CreateClient("OurAPI");

                // Attach token to POST headers
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

                var response = await httpClient.GetAsync("User/" + id);     // https://localhost:44378/User/{id}

                response.EnsureSuccessStatusCode();                         // Throws an exception if not 200 OK

                // Json Deserialization
                var stream = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ServiceResponse<GetUserDto>>(stream);

                return result;
            }
            catch(Exception ex)
            {
                // Triggers if we get a 404, or anything not a 200 OK.
                _logger.LogError("{Time} - Error - HttpClientService.GetSingleUser - ID: {1} - {2}", DateTime.UtcNow, id, ex.Message);


                var response = new ServiceResponse<GetUserDto>();
                response.Data = null;
                response.Success = false;
                response.Message = "Error Getting User Id";

                return response;
            }

        }

        [HttpGet]
        public async Task<ServiceResponse<GetUserDto>> GetUserByUsername(string token, string username)
        {
            try
            {
                // GET
                var httpClient = _httpClient.CreateClient("OurAPI");

                // Attach token to POST headers
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

                var response = await httpClient.GetAsync("Username/" + username);     // https://localhost:44378/Username/{username}

                response.EnsureSuccessStatusCode();                         // Throws an exception if not 200 OK

                // Json Deserialization
                var stream = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ServiceResponse<GetUserDto>>(stream);

                return result;
            }
            catch(Exception ex)
            {
                // Triggers if we get a 404, or anything not a 200 OK.
                _logger.LogError("{Time} - Error - HttpClientService.GetUserByUsername - USERNAME: {1} - {2}", DateTime.UtcNow, username, ex.Message);

                var response = new ServiceResponse<GetUserDto>();
                response.Data = null;
                response.Success = false;
                response.Message = "Error Getting Username";

                return response;
            }
        }


        // UPDATE
        [HttpPut]
        public async Task<ServiceResponse<GetUserDto>> UpdateUser(string token, UpdateUserDto update)
        {
            try
            {

                // Convert the updated user's email, username, password into json
                HttpContent httpContent = new StringContent(JsonConvert.SerializeObject(update), Encoding.UTF8, "application/json");

                // Create API client from startup.cs stuff
                var httpClient = _httpClient.CreateClient("OurAPI");

                // Attach token to PUT headers
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

                var response = await httpClient.PutAsync("User", httpContent);     // PUT to https://localhost:44378/User

                response.EnsureSuccessStatusCode();                         // Throws an exception if not 200 OK

                // Json Deserialization
                var stream = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ServiceResponse<GetUserDto>>(stream);

                return result;


            } catch(Exception ex)
            {
                // Triggers if we get a 404, or anything not a 200 OK.
                _logger.LogError("{Time} - Error - HttpClientService.UpdateUser - ID: {1}  USERNAME: {2}  EMAIL: {3} - {4}", DateTime.UtcNow, update.Id, update.Username, update.Email, ex.Message);


                var response = new ServiceResponse<GetUserDto>();
                response.Data = null;
                response.Success = false;
                response.Message = "Error Updating User.";

                return response;
            }
        }


        // ADD
        [HttpPost]
        public async Task<ServiceResponse<List<GetUserDto>>> AddUser(string token, AddUserDto user)
        {
            try
            {

                // Convert the updated user's email, username, password into json
                HttpContent httpContent = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");

                // Create API client from startup.cs stuff
                var httpClient = _httpClient.CreateClient("OurAPI");

                // Attach token to PUT headers
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

                var response = await httpClient.PostAsync("User", httpContent);     // POST to https://localhost:44378/User

                response.EnsureSuccessStatusCode();                         // Throws an exception if not 200 OK

                // Json Deserialization
                var stream = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ServiceResponse<List<GetUserDto>>>(stream);

                return result;


            }
            catch(Exception ex)
            {
                // Triggers on a 404
                _logger.LogError("{Time} - Error - HttpClientService.AddUser - USERNAME: {1}  EMAIL: {2} - {3}", DateTime.UtcNow,  user.Username, user.Email, ex.Message);


                var response = new ServiceResponse<List<GetUserDto>>();
                response.Data = null;
                response.Success = false;
                response.Message = "Error Updating User.";

                return response;
            }
        }

        // DELETE
        [HttpDelete]
        public async Task<ServiceResponse<List<GetUserDto>>> DeleteUser(string token, int id)
        {
            try
            {

                // Create API client from startup.cs stuff
                var httpClient = _httpClient.CreateClient("OurAPI");

                // Attach token to DELETE headers
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

                var response = await httpClient.DeleteAsync("User/" + id);     // DELETE to https://localhost:44378/User/{id}

                response.EnsureSuccessStatusCode();                         // Throws an exception if not 200 OK

                // Json Deserialization
                var stream = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ServiceResponse<List<GetUserDto>>>(stream);

                return result;


            }
            catch(Exception ex)
            {
                // Triggers on a 404
                _logger.LogError("{Time} - Error - HttpClientService.DeleteUser - ID: {1} - {2}", DateTime.UtcNow, id, ex.Message);

                var response = new ServiceResponse<List<GetUserDto>>();
                response.Data = null;
                response.Success = false;
                response.Message = "Error Deleting User.";

                return response;
            }
        }



        // Change User's Password
        [HttpPut]
        public async Task<ServiceResponse<string>> ChangePass(ChangePassDto newDetails)
        {
            try
            {

                // Convert the user id, token and new password into json
                HttpContent httpContent = new StringContent(JsonConvert.SerializeObject(newDetails), Encoding.UTF8, "application/json");

                // PUT
                var httpClient = _httpClient.CreateClient("OurAPI");                         // Startup.cs services has our named client "OurAPI". Base Url addresss there is https://localhost:44331/

                // Attach token to PUT headers
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + newDetails.Token);

                var response = await httpClient.PutAsync("User/ChangePass/", httpContent);       // API - https://localhost:44331/User/ChangePass

                response.EnsureSuccessStatusCode();                         // Throws an exception if not 200 OK

                // Json Deserialization
                var stream = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ServiceResponse<string>>(stream);

                return result;


            }
            catch(Exception ex)
            {
                // Triggers on a 404
                _logger.LogError("{Time} - Error - HttpClientService.ChangePass - ID: {1} - {2}", DateTime.UtcNow, newDetails.Id, ex.Message);

                var response = new ServiceResponse<string>();
                response.Success = false;
                response.Message = "Error Changing Password.";

                return response;
            }
        }


        // Verify Email Address
        [HttpPost]
        public async Task<ServiceResponse<string>> EmailVerified(VerifyEmailDto details)
        {
            try
            {

                // Convert the user id and token into json
                HttpContent httpContent = new StringContent(JsonConvert.SerializeObject(details), Encoding.UTF8, "application/json");

                // POST
                var httpClient = _httpClient.CreateClient("OurAPI");                         // Startup.cs services has our named client "OurAPI". Base Url addresss there is https://localhost:44331/

                // Attach token to POST headers
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + details.Token);

                var response = await httpClient.PostAsync("User/VerifyEmail/", httpContent);       // API - https://localhost:44331/User/VerifyEmail

                response.EnsureSuccessStatusCode();                         // Throws an exception if not 200 OK

                // Json Deserialization
                var stream = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ServiceResponse<string>>(stream);

                return result;


            }
            catch (Exception ex)
            {
                // Triggers on a 404
                _logger.LogError("{Time} - Error - HttpClientService.VerifyEmail - ID: {1} - {2}", DateTime.UtcNow, details.Id, ex.Message);

                var response = new ServiceResponse<string>();
                response.Success = false;
                response.Message = "Error Verifying Email.";

                return response;
            }
        }
    }
}
