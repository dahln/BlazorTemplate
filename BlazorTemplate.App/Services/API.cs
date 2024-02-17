﻿using Blazored.Toast.Services;
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
    /// This 'API Library' supplements the HttpClient.
    /// 
    /// It wraps all calls with a 'Loading Spinner'
    /// It parses results
    /// It shows toast errors OR returns the results.
    /// 
    /// This API library is not required. 
    /// The 'HTTPClient' used in this API library is injected, and can be used without this library by simply injecting it into the component you want.
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
            var noErrors = await ParseErrorsDisplayAsToast(response, isIdentityRequest, redirectOn404);
            if(noErrors)
                return await ParseResponse<T>(response);
            else
                return default(T);
        }
        async public Task<bool> GetAsync(string path, bool showSpinner = true, bool isIdentityRequest = false, bool redirectOn404 = true) 
        {
            var response = await SendAsync(HttpMethod.Get, path, showSpinner);
            return await ParseErrorsDisplayAsToast(response, isIdentityRequest, redirectOn404);
        }



        async public Task<T> PostAsync<T>(string path, object content, bool showSpinner = true, bool isIdentityRequest = false, bool redirectOn404 = true) 
        {
            var response = await SendAsync(HttpMethod.Post, path, showSpinner, content);
            var noErrors = await ParseErrorsDisplayAsToast(response, isIdentityRequest, redirectOn404);
            if(noErrors)
                return await ParseResponse<T>(response);
            else
                return default(T);
        }
        async public Task<bool> PostAsync(string path, object content, bool showSpinner = true, bool isIdentityRequest = false, bool redirectOn404 = true) 
        {
            var response = await SendAsync(HttpMethod.Post, path, showSpinner, content);
            return await ParseErrorsDisplayAsToast(response, isIdentityRequest, redirectOn404);
        }

        /// <summary>
        /// Login 'POST' has a unique response. It was easier to create a dedicated login API method.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        async public Task<LoginResponse> LoginPostAsync(string path, object content) 
        {
            var response = await SendAsync(HttpMethod.Post, path, true, content);
            if(response.IsSuccessStatusCode == false)
            {
                string contents = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<LoginResponse>(contents);
            }
            return default(LoginResponse);
        }



        async public Task<T> PutAsync<T>(string path, object content, bool showSpinner = true, bool isIdentityRequest = false, bool redirectOn404 = true) 
        {
            var response = await SendAsync(HttpMethod.Put, path, showSpinner, content);
            var noErrors = await ParseErrorsDisplayAsToast(response, isIdentityRequest, redirectOn404);
            if(noErrors)
                return await ParseResponse<T>(response);
            else
                return default(T);
        }
        async public Task<bool> PutAsync(string path, object content, bool showSpinner = true, bool isIdentityRequest = false, bool redirectOn404 = true) 
        {
            var response = await SendAsync(HttpMethod.Put, path, showSpinner, content);
            return await ParseErrorsDisplayAsToast(response, isIdentityRequest, redirectOn404);
        }



        async public Task<bool> DeleteAsync(string path, bool showSpinner = true, bool isIdentityRequest = false, bool redirectOn404 = true) 
        {
            var response = await SendAsync(HttpMethod.Delete, path, showSpinner);
            return await ParseErrorsDisplayAsToast(response, isIdentityRequest, redirectOn404);
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
                    Console.WriteLine("API Parsing Error.");
                    return default(T);
                }
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
        private async Task<bool> ParseErrorsDisplayAsToast(HttpResponseMessage response, bool isIdentityRequest = false, bool redirectOn404 = true)
        {
            //If the respone is unauthorized, redirect back to the login page
            if(response.StatusCode == HttpStatusCode.Unauthorized && redirectOn404)
            {
                _navigationManager.NavigateTo("login", true);
            }
            
            if(response.IsSuccessStatusCode == false && isIdentityRequest == false)
            {
                string error = await ParseResponseError(response);
                DisplayErrorAsToast(error);
                return false;
            }
            else if(response.StatusCode == HttpStatusCode.Unauthorized && isIdentityRequest && !redirectOn404)
            {
                var errors = await ParseIdentityLoginResponseError(response);
                DisplayErrorAsToast(errors);
                return false;
            }
            else if(response.IsSuccessStatusCode == false && isIdentityRequest == true)
            {
                var errors = await ParseIdentityResponseError(response);
                DisplayErrorAsToast(errors);
                return false;
            }

            return true;
        }

        private async Task<List<string>> ParseErrors(HttpResponseMessage response, bool isIdentityRequest = false, bool redirectOn404 = true)
        {
            List<string> allErrors = new List<string>();
            //If the respone is unauthorized, redirect back to the login page
            if(response.StatusCode == HttpStatusCode.Unauthorized && redirectOn404)
            {
                _navigationManager.NavigateTo("login", true);
            }
            
            if(response.IsSuccessStatusCode == false && isIdentityRequest == false)
            {
                string error = await ParseResponseError(response);
                allErrors.Add(error);
            }
            else if(response.StatusCode == HttpStatusCode.Unauthorized && isIdentityRequest && !redirectOn404)
            {
                var errors = await ParseIdentityLoginResponseError(response);
                allErrors.AddRange(errors);
            }
            else if(response.IsSuccessStatusCode == false && isIdentityRequest == true)
            {
                var errors = await ParseIdentityResponseError(response);
                allErrors.AddRange(errors);
            }

            return allErrors;
        }

        private async Task<string> ParseResponseError(HttpResponseMessage response)
        {
            string errorResponse = await response.Content.ReadAsStringAsync();
            return errorResponse;
        }
        
        private async Task<List<string>> ParseIdentityResponseError(HttpResponseMessage response)
        {
            //Parsing errors from Identity API
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
                Console.WriteLine("Identity API Parsing Error.");
                return errors;
            }
        }

        private async Task<List<string>> ParseIdentityLoginResponseError(HttpResponseMessage response)
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
            catch
            {
                Console.WriteLine("Error Processing Login Request");
                return errors;
            }
        }

        private void DisplayErrorAsToast(string error)
        {
            _toastService.ShowError(error);
        }

        private void DisplayErrorAsToast(List<string> errors)
        {
            foreach(var error in errors) 
            {
                _toastService.ShowError(error);
            }
        }
    }
}
