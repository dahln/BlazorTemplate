﻿namespace BlazorTemplate.Service
{
    public class Auth0TokenResponse
    {
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public string scope { get; set; }
        public string token_type { get; set; }
    }
}
