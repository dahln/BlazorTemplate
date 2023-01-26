using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BlazorDemoCRUD.Server.Utility;
using BlazorDemoCRUD.Service;
using Newtonsoft.Json;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace BlazorDemoCRUD.Server.Controllers
{
    /// <summary>
    /// Client side configuration values are stored in the server side configuration.
    /// This enables the 'Blazor WASM Hosted' application to have client side environment specific configuration.
    /// 
    /// When the client app loads, it will first call this API endpoint which provides the client app with the necessary client side configuration values.
    /// In some scenerios these configuration values would be stored in a client side configuration settings file.
    /// Because the values are meant to be used in the client side app, there is no expectation that the values be kept secret. 
    /// The secret/secure values are the values used on the server side.
    /// </summary>
    public class ConfigurationController : Controller
    {
        private IConfiguration _configuration;
        public ConfigurationController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        [Route("api/v1/configuration")]
        async public Task<IActionResult> GetClientConfiguration()
        {
            Common.Configuration response = new Common.Configuration()
            {
                ClientId = _configuration.GetSection("Auth0").GetValue<string>("AppClientId"),
                Audience = _configuration.GetSection("Auth0").GetValue<string>("Audience"),
                Authority = _configuration.GetSection("Auth0").GetValue<string>("Authority")
            };
            return Ok(response);
        }

    }//End Controller
}
