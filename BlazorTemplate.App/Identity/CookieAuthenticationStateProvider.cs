using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Text.Json;
using System.Net.Http.Json;
using BlazorTemplate.Identity.Models;
using BlazorTemplate.App.Services;

namespace BlazorTemplate.Identity
{
    public class CookieAuthenticationStateProvider : AuthenticationStateProvider, IAccountManagement
    {
            
        private API API { get; set; }
        private bool _authenticated = false;
        private readonly ClaimsPrincipal Unauthenticated = new(new ClaimsIdentity());
        public CookieAuthenticationStateProvider(API api)
        {
            API = api;
        }

        public async Task<FormResult> RegisterAsync(string email, string password)
        {
            try
            {
                // make the request
                var content = new
                {
                    email,
                    password
                };
                var result = await API.SendRequestAsync(HttpMethod.Post,"register", content, true, true);


                // successful?
                if (result.Success)
                {
                    return new FormResult { Succeeded = true };
                }

                // return the error list
                return new FormResult
                {
                    Succeeded = false,
                    ErrorList = result.Errors
                };
            }
            catch { }
             
            // unknown error
            return new FormResult
            {
                Succeeded = false,
                ErrorList = ["An unknown error prevented registration from succeeding."]
            };
        }

        public async Task<FormResult> LoginAsync(string email, string password)
        {
            try
            {
                var content = new
                {
                    email,
                    password
                };
                // login with cookies
                var result = await API.SendRequestAsync(HttpMethod.Post, "login?useCookies=true", content, true, true);

                // success?
                if (result.Success)
                {
                    // need to refresh auth state
                    NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

                    // success!
                    return new FormResult { Succeeded = true };
                }                
            }
            catch { }

            // unknown error
            return new FormResult
            {
                Succeeded = false,
                ErrorList = ["Invalid email and/or password."]
            };
        }

        /// <summary>
        /// Get authentication state.
        /// </summary>
        /// <remarks>
        /// Called by Blazor anytime and authentication-based decision needs to be made, then cached
        /// until the changed state notification is raised.
        /// </remarks>
        /// <returns>The authentication state asynchronous request.</returns>
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            _authenticated = false;

            // default to not authenticated
            var user = Unauthenticated;

            try
            {
                // the user info endpoint is secured, so if the user isn't logged in this will fail
                var userResponse = await API.SendRequestAsync<UserInfo>(HttpMethod.Get, "manage/info");

                
                if (userResponse.Success)
                {
                    // in our system name and email are the same
                    var claims = new List<Claim>
                    {
                        new(ClaimTypes.Name, userResponse.Data.Email),
                        new(ClaimTypes.Email, userResponse.Data.Email)
                    };

                    // add any additional claims
                    claims.AddRange(
                        userResponse.Data.Claims.Where(c => c.Key != ClaimTypes.Name && c.Key != ClaimTypes.Email)
                            .Select(c => new Claim(c.Key, c.Value)));

                    // set the principal
                    var id = new ClaimsIdentity(claims, nameof(CookieAuthenticationStateProvider));
                    user = new ClaimsPrincipal(id);
                    _authenticated = true;
                }
            }
            catch { }

            // return the state
            return new AuthenticationState(user);
        }

        public async Task LogoutAsync()
        {
            await API.SendRequestAsync(HttpMethod.Post, "api/v1/account/logout", null);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public async Task<bool> CheckAuthenticatedAsync()
        {
            await GetAuthenticationStateAsync();
            return _authenticated;
        }
    }
}
