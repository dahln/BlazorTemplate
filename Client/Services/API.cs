using Blazored.Toast.Services;
using BlazorSpinner;
using BlazorTemplate.Common;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Newtonsoft.Json;
using System.Text;

namespace BlazorTemplate.Client.Services
{
    public class API
    {
        private HttpClient _httpClient { get; set; }
        private NavigationManager _navigationManger { get; set; }
        private IToastService _toastService { get; set; }
        private SpinnerService _spinnerService { get; set; }
        private IAccessTokenProvider _authenticationService { get; set; }
        public API(HttpClient httpClient, IAccessTokenProvider authenticationService, NavigationManager navigationManager, IToastService toastService, SpinnerService spinnerService)
        {
            _httpClient = httpClient;

            _authenticationService = authenticationService;
            _navigationManger = navigationManager;
            _toastService = toastService;
            _spinnerService = spinnerService;
        }


        #region Account Management
        async public Task<bool> AccountChangeEmail(ChangeEmail content)
        {
            var response = await PostAsync<bool>($"api/v1/account/email", content);
            return response;
        }
        async public Task<bool> AccountChangePassword(ChangePassword content)
        {
            var response = await PostAsync<bool>($"api/v1/account/password", content);
            return response;
        }
        async public Task AccountDeleteUser()
        {
            await DeleteAsync($"api/v1/account");
        }
        #endregion

        #region Customer CRUD/Search
        async public Task<string> CustomerCreate(Customer content)
        {
            return await PostAsync<string>($"api/v1/customer", content);
        }
        async public Task<Customer> CustomerGetById(string id)
        {
            return await GetAsync<Customer>($"api/v1/customer/{id}");
        }
        async public Task<string> CustomerUpdateById(Customer content, string id)
        {
            return await PutAsync<string>($"api/v1/customer/{id}", content);
        }
        async public Task CustomerDeleteById(string id)
        {
            await DeleteAsync($"api/v1/customer/{id}");
        }
        async public Task<SearchResponse<Customer>> CustomerSearch(Search content)
        {
            return await PostAsync<SearchResponse<Customer>>($"api/v1/customers", content);
        }
        #endregion

        async public Task SeedDB(int number)
        {
            await GetAsync($"api/v1/seed/create/{number}");
        }


        #region HTTP Methods
        private async Task GetAsync(string path, bool showSpinner = true)
        {
            await Send(HttpMethod.Get, path, showSpinner);
        }
        private async Task<T> GetAsync<T>(string path, bool showSpinner = true)
        {
            var response = await Send(HttpMethod.Get, path, showSpinner);
            T result = await ParseResponseObject<T>(response);
            return result;
        }
        private async Task PostAsync(string path, object content, bool showSpinner = true)
        {
            await Send(HttpMethod.Post, path, showSpinner, content);
        }
        private async Task<T> PostAsync<T>(string path, object content, bool showSpinner = true)
        {
            var response = await Send(HttpMethod.Post, path, showSpinner, content);
            return await ParseResponseObject<T>(response);
        }
        private async Task PutAsync(string path, object content, bool showSpinner = true)
        {
            await Send(HttpMethod.Put, path, showSpinner, content);
        }
        private async Task<T> PutAsync<T>(string path, object content, bool showSpinner = true)
        {
            var response = await Send(HttpMethod.Put, path, showSpinner, content);
            return await ParseResponseObject<T>(response);
        }
        private async Task PutAsync(string path, bool showSpinner = true)
        {
            await Send(HttpMethod.Put, path, showSpinner);
        }
        private async Task DeleteAsync(string path, bool showSpinner = true)
        {
            await Send(HttpMethod.Delete, path, showSpinner);
        }
        private async Task<T> DeleteAsync<T>(string path, bool showSpinner = true)
        {
            var response = await Send(HttpMethod.Delete, path, showSpinner);
            return await ParseResponseObject<T>(response);
        }
        private async Task DeleteAsync(string path, object content, bool showSpinner = true)
        {
            await Send(HttpMethod.Delete, path, showSpinner, content);
        }


        private async Task<HttpResponseMessage> Send(HttpMethod method, string path, bool showSpinner, object content = null)
        {
            if(showSpinner)
                _spinnerService.Show();

            var httpWebRequest = new HttpRequestMessage(method, path);
            httpWebRequest.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue() { NoCache = true };

            if (content != null)
            {
                string json = JsonConvert.SerializeObject(content);
                StringContent postContent = new StringContent(json, UnicodeEncoding.UTF8, "application/json");

                httpWebRequest.Content = postContent;
            }

            HttpResponseMessage response = new HttpResponseMessage()
            {
                StatusCode = System.Net.HttpStatusCode.BadRequest
            };
            try
            {
                response = await _httpClient.SendAsync(httpWebRequest);

                if (response.IsSuccessStatusCode == false)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(responseContent))
                    {
                        _toastService.ShowError(responseContent);
                    }
                }

                if(showSpinner)
                    _spinnerService.Hide();
            }
            catch (AccessTokenNotAvailableException exception)
            {
                exception.Redirect();
            }
            return response;
        }

        private async Task<T> ParseResponseObject<T>(HttpResponseMessage response)
        {
            if(typeof(T) == typeof(bool))
            {
                return (T)Convert.ChangeType(response.StatusCode == System.Net.HttpStatusCode.OK, typeof(T));
            }
            else if (response != null && response.IsSuccessStatusCode && response.Content != null)
            {
                string responseContent = await response.Content.ReadAsStringAsync();

                //Can't deseriazlize a string unless it starts with a "
                if (typeof(T) == typeof(string))
                    responseContent = $"\"{responseContent}\"";

                return JsonConvert.DeserializeObject<T>(responseContent);
            }
            else
            {
                return default(T);
            }
        }
        #endregion
    }
}
