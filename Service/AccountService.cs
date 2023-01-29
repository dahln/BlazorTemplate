using Azure.Identity;
using BlazorDemoCRUD.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;

namespace BlazorDemoCRUD.Service
{
    public class AccountService
    {
        private IConfiguration _configuration;
        private DBContext _dbContext;

        public AccountService(IConfiguration configuration, DBContext dBContext)
        {
            _configuration = configuration;
            _dbContext = dBContext;
        }

        public GraphServiceClient GetGraphClient()
        {
            var tenantId = _configuration.GetSection("AzureAdB2C").GetValue<string>("TenantId");
            var clientId = _configuration.GetSection("AzureAdB2C").GetValue<string>("ClientId");
            var secret = _configuration.GetSection("AzureAdB2C").GetValue<string>("Secret");

            var scopes = new[] { "https://graph.microsoft.com/.default" };
            var clientSecretCredential = new ClientSecretCredential(tenantId, clientId, secret);
            var graphClient = new GraphServiceClient(clientSecretCredential, scopes);

            return graphClient;
        }

        async public Task<ServiceResponse> ChangeName(string userId, Common.ChangeName model)
        {
            ServiceResponse response = new ServiceResponse();
            
            var graphClient = GetGraphClient();

            var user = new User
            {
                DisplayName = model.NewName,
            };

            try
            {
                await graphClient.Users[userId]
                    .Request()
                    .UpdateAsync(user);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"{userId} ||| {ex.Message}";
            }

            return response;
        }

        async public Task<ServiceResponse> ChangeAccountPassword(string userId, Common.ChangePassword model)
        {
            ServiceResponse response = new ServiceResponse();

            if(model.NewPassword != model.ConfirmPassword)
            {
                response.Success = false;
                response.Message = "Passwords do not match";
            }
            else if(string.IsNullOrEmpty(model.NewPassword))
            {
                response.Success = false;
                response.Message = "Password is required";
            }

            var graphClient = GetGraphClient();

            var user = new User
            {
                PasswordProfile = new PasswordProfile
                {
                    ForceChangePasswordNextSignIn = false,
                    Password = model.NewPassword
                }
            };

            try
            {
                await graphClient.Users[userId]
                    .Request()
                    .UpdateAsync(user);
            }
            catch(Exception ex)
            {
                response.Success = false;
                response.Message = $"{userId} ||| {ex.Message}";
            }

            return response;
        }


        async public Task<ServiceResponse> DeleteAccount(string userId)
        {
            ServiceResponse response = new ServiceResponse();

            var graphClient = GetGraphClient();

            try
            {
                await graphClient.Users[userId]
                    .Request()
                    .DeleteAsync();
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"{userId} ||| {ex.Message}";
            }

            return response;
        }

        async public Task<ServiceResponse<string>> GetUserIdByUserEmail(string email)
        {
            ServiceResponse<string> response = new ServiceResponse<string>();

            // var tokenData = await GetAuth0Token();

            // var apiClient = new ManagementApiClient(tokenData.access_token, new Uri(_configuration.GetSection("Auth0").GetValue<string>("ManagementAPI")));

            // try
            // {
            //     var user = await apiClient.Users.GetUsersByEmailAsync(email);
            //     if (user == null || user.Count == 0)
            //     {
            //         response.Message = "User not found";
            //         response.Success = false;
            //         return response;
            //     }

            //     response.Data = user.First().UserId;
            // }
            // catch { }
            return response;
        }

        async public Task<List<ServiceResponseKeyValue<string, string>>> GetUsersById(List<string> userIds)
        {
            List<ServiceResponseKeyValue<string, string>> response = new List<ServiceResponseKeyValue<string, string>>();

            // var tokenData = await GetAuth0Token();

            // var apiClient = new ManagementApiClient(tokenData.access_token, new Uri(_configuration.GetSection("Auth0").GetValue<string>("ManagementAPI")));

            // foreach(var userId in userIds)
            // {
            //     var user = await apiClient.Users.GetAsync(userId);
            //     if(user != null)
            //     {
            //         response.Add(new ServiceResponseKeyValue<string, string>()
            //         {
            //             Key = userId,
            //             Value = user.Email
            //         });
            //     }
            // }

            return response;
        }


    }
}
