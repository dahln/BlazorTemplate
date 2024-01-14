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
    public class ApiResponse<T>
    {
        public T Data { get; set; }
        public bool Success { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
    public class ApiResponse
    {
        public bool Success { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }


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

        async public Task<ApiResponse> SendRequestAsync(HttpMethod method, string path, object content = null, bool showSpinner = true, bool isIdentityRequest = false)
        {
            if(showSpinner)
                _spinnerService.Show();

            var httpWebRequest = new HttpRequestMessage(method, path);
            httpWebRequest.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue() { NoCache = true };

            //POST or PUT the context as json.
            if (content != null)
            {
                string json = JsonConvert.SerializeObject(content);
                StringContent postContent = new StringContent(json, UnicodeEncoding.UTF8, "application/json");

                httpWebRequest.Content = postContent;
            }

            //Send the request.
            HttpResponseMessage response = await _httpClient.SendAsync(httpWebRequest);

            //Prep the response object to return to the caller.
            ApiResponse apiResponse = new Services.ApiResponse()
            {
                Success = response.IsSuccessStatusCode
            };

            //If the response was a 401 unauthorized then reload the application and send the user to the Login page.
            if(response.StatusCode == HttpStatusCode.Unauthorized)
            {
                _navigationManager.NavigateTo("login", true);
            }

            //Parse the data for either success or errors.
            if(response.IsSuccessStatusCode == false && isIdentityRequest)
            {
                var identityErrors = await ParseIdentityErrorsResponse(response);
                apiResponse.Errors.AddRange(identityErrors);
            }

            if(showSpinner)
                _spinnerService.Hide();

            return apiResponse;
        }
        async public Task<ApiResponse<T>> SendRequestAsync<T>(HttpMethod method, string path, object content = null, bool showSpinner = true, bool isIdentityRequest = false)
        {
            if(showSpinner)
                _spinnerService.Show();

            var httpWebRequest = new HttpRequestMessage(method, path);
            httpWebRequest.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue() { NoCache = true };

            //POST or PUT the context as json.
            if (content != null)
            {
                string json = JsonConvert.SerializeObject(content);
                StringContent postContent = new StringContent(json, UnicodeEncoding.UTF8, "application/json");

                httpWebRequest.Content = postContent;
            }

            //Send the request.
            HttpResponseMessage response = await _httpClient.SendAsync(httpWebRequest);

            //Prep the response object to return to the caller.
            ApiResponse<T> apiResponse = new Services.ApiResponse<T>()
            {
                Success = response.IsSuccessStatusCode
            };

            //If the response was a 401 unauthorized then reload the application and send the user to the Login page.
            if(response.StatusCode == HttpStatusCode.Unauthorized)
            {
                //_navigationManager.NavigateTo("login", true);
            }

            //Parse the data for either success or errors.
            if(response.IsSuccessStatusCode)
            {
                apiResponse.Data = await ParseResponseObject<T>(response);
            }
            else if(response.IsSuccessStatusCode == false && isIdentityRequest)
            {
                var identityErrors = await ParseIdentityErrorsResponse(response);
                apiResponse.Errors.AddRange(identityErrors);
            }

            if(showSpinner)
                _spinnerService.Hide();

            return apiResponse;
        }

        public async Task<T> ParseResponseObject<T>(HttpResponseMessage response)
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
            catch
            {
                _toastService.ShowError("API Parsing Error.");
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
            catch
            {
                string error = "API Parsing Error.";
                _toastService.ShowError(error);
                errors.Add(error);
                return errors;
            }
        }
    }
}
