using Blazored.Toast.Services;
using BlazorSpinner;
using BlazorTemplate.Common;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using System.Data;
using System.Net;
using System.Text;
using System.Text.Json;

namespace BlazorTemplate.App.Services
{
    /// <summary>
    /// This is a library of all the external API calls. This promotes reusabilit of calls. 
    /// It also creates a standard way to parse responses and handle the 'loading spinner' when 
    /// waiting for a response.
    /// </summary>
    public class API
    {
        private HttpClient _httpClient { get; set; }
        private SpinnerService _spinnerService { get; set; }
        private IToastService _toastService { get; set; }
        private  NavigationManager _navigationManager { get; set; }


        public API(HttpClient httpClient, SpinnerService spinnerService, IToastService toastService, NavigationManager navigationManager)
        {
            _httpClient = httpClient;
            _spinnerService = spinnerService;
            _toastService = toastService;
            _navigationManager = navigationManager;
        }

        async public Task<T> GetAsync<T>(string path, bool showSpinner = true, bool isIdentityRequest = false, bool redirectOn404 = true) 
        {
            var response = await SendAsync(HttpMethod.Get, path, showSpinner);
            var noErrors = await ParseErrors(response, isIdentityRequest, redirectOn404);
            if(noErrors)
                return await ParseResponse<T>(response);
            else
                return default(T);
        }
        async public Task<bool> GetAsync(string path, bool showSpinner = true, bool isIdentityRequest = false, bool redirectOn404 = true) 
        {
            var response = await SendAsync(HttpMethod.Get, path, showSpinner);
            return await ParseErrors(response, isIdentityRequest, redirectOn404);
        }
        async public Task<T> PostAsync<T>(string path, object content, bool showSpinner = true, bool isIdentityRequest = false, bool redirectOn404 = true) 
        {
            var response = await SendAsync(HttpMethod.Post, path, showSpinner, content);
            var noErrors = await ParseErrors(response, isIdentityRequest, redirectOn404);
            if(noErrors)
                return await ParseResponse<T>(response);
            else
                return default(T);
        }
        async public Task<bool> PostAsync(string path, object content, bool showSpinner = true, bool isIdentityRequest = false, bool redirectOn404 = true) 
        {
            var response = await SendAsync(HttpMethod.Post, path, showSpinner, content);
            return await ParseErrors(response, isIdentityRequest, redirectOn404);
        }
        async public Task<T> PutAsync<T>(string path, object content, bool showSpinner = true, bool isIdentityRequest = false, bool redirectOn404 = true) 
        {
            var response = await SendAsync(HttpMethod.Put, path, showSpinner, content);
            var noErrors = await ParseErrors(response, isIdentityRequest, redirectOn404);
            if(noErrors)
                return await ParseResponse<T>(response);
            else
                return default(T);
        }
        async public Task<bool> PutAsync(string path, object content, bool showSpinner = true, bool isIdentityRequest = false, bool redirectOn404 = true) 
        {
            var response = await SendAsync(HttpMethod.Put, path, showSpinner, content);
            return await ParseErrors(response, isIdentityRequest, redirectOn404);
        }
        async public Task<bool> DeleteAsync(string path, bool showSpinner = true, bool isIdentityRequest = false, bool redirectOn404 = true) 
        {
            var response = await SendAsync(HttpMethod.Delete, path, showSpinner);
            return await ParseErrors(response, isIdentityRequest, redirectOn404);
        } 

        private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, bool showSpinner, object content = null)
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

            response = await _httpClient.SendAsync(httpWebRequest);
          
            if(showSpinner)
                _spinnerService.Hide();
           
            return response;
        }

        private async Task<T> ParseResponse<T>(HttpResponseMessage response)
        {
            //Parse the data for either success or errors.
            if(response.IsSuccessStatusCode)
            {
                return await ParseResponseObject<T>(response);
            }

            return default(T);
        }

        /// <summary>
        /// False means it failed because of errors. True means no errors and it succeeded. 
        /// </summary>
        /// <param name="response"></param>
        /// <param name="isIdentityRequest"></param>
        /// <param name="redirectOn404"></param>
        /// <returns></returns>
        private async Task<bool> ParseErrors(HttpResponseMessage response, bool isIdentityRequest = false, bool redirectOn404 = true)
        {
            if(response.StatusCode == HttpStatusCode.Unauthorized && redirectOn404)
            {
                _navigationManager.NavigateTo("login", true);
            }
            
            //Parse the data for either success or errors.
            if(response.StatusCode == HttpStatusCode.Unauthorized && isIdentityRequest && !redirectOn404)
            {
                var errorsIdentity = await ParseIdentityLoginErrorsResponse(response);
                foreach(var error in errorsIdentity) 
                {
                    _toastService.ShowError(error);
                }
                return false;
            }
            else if(response.IsSuccessStatusCode == false && isIdentityRequest == false)
            {
                string errorResponse = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(errorResponse))
                {
                    _toastService.ShowError(errorResponse);
                }
                return false;
            }
            else if(response.IsSuccessStatusCode == false && isIdentityRequest == true)
            {
                var errorsIdentity = await ParseIdentityErrorsResponse(response);
                foreach(var error in errorsIdentity) 
                {
                    _toastService.ShowError(error);
                }
                return false;
            }

            return true;
        }

        private async Task<T> ParseResponseObject<T>(HttpResponseMessage response)
        {
            try
            {
                if (response != null && response.IsSuccessStatusCode && response.Content != null)
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
            catch(Exception ex)
            {
                _toastService.ShowError($"API Parsing Error. Exception: {ex.Message} - {ex.StackTrace}");
                // _toastService.ShowError("API Parsing Error.");
                return default(T);
            }
        }

        public async Task<List<string>> ParseIdentityErrorsResponse(HttpResponseMessage response)
        {
            var errors = new List<string>();
            try
            {
                var details = await response.Content.ReadAsStringAsync();
                var problemDetails = JsonDocument.Parse(details);
                var errorList = problemDetails.RootElement.GetProperty("errors");
                foreach (var errorEntry in errorList.EnumerateObject())
                {
                    if (errorEntry.Value.ValueKind == JsonValueKind.String)
                    {
                        errors.Add(errorEntry.Value.GetString()!);
                    }
                    else if (errorEntry.Value.ValueKind == JsonValueKind.Array)
                    {
                        errors.AddRange(
                            errorEntry.Value.EnumerateArray().Select(
                                e => e.GetString() ?? string.Empty)
                            .Where(e => !string.IsNullOrEmpty(e)));
                    }
                }

                return errors;
            }
            catch(Exception ex)
            {
                string error = "Identity API Parsing Error.";
                // _toastService.ShowError(error);
                _toastService.ShowError($"{error} Exception: {ex.Message} - {ex.StackTrace}");
                errors.Add(error);
                return errors;
            }
        }

        /// <summary>
        /// The login API call returns a different response object. This will parse it.
        /// This was necessary inorder to show a 'Lockout' at the appropriate times.
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public async Task<List<string>> ParseIdentityLoginErrorsResponse(HttpResponseMessage response)
        {
            var errors = new List<string>();
            try
            {
                //Login error:
                var details = await response.Content.ReadAsStringAsync();
                Newtonsoft.Json.Linq.JObject jsonObject = Newtonsoft.Json.Linq.JObject.Parse(details);
                string detailValue = (string)jsonObject["detail"];
                if(!string.IsNullOrEmpty(detailValue))
                    errors.Add(detailValue);

                //Don't want this error. It simply means incorrect username/password.
                errors = errors.Where(x => x != "Failed").ToList();

                return errors;
            }
            catch(Exception ex)
            {
               //Console.WriteLine($"API Parsing Login Error. Exception: {ex.Message} - {ex.StackTrace}");
               return errors;
            }
        }
    }
}
