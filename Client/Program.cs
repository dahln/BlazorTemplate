using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Blazored.LocalStorage;
using Blazored.Modal;
using Blazored.Toast;
using BlazorSpinner;
using BlazorDemoCRUD.Client;
using BlazorDemoCRUD.Client.Services;
using System.Net.Http.Json;
using System.Security.AccessControl;
using static System.Net.Mime.MediaTypeNames;
using Blazored.SessionStorage;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

#region GetSettingsFromServer
//Get Configuration values from server
HttpClient _tempHttpClient = new HttpClient() { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
var configuration = await  _tempHttpClient.GetFromJsonAsync<BlazorDemoCRUD.Common.Configuration>("api/v1/configuration");
builder.Services.AddSingleton<BlazorDemoCRUD.Common.Configuration>(configuration);
#endregion

builder.Services.AddHttpClient("API", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
    .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>()
    .AddHttpMessageHandler(sp =>
    {
        var handler = sp.GetService<AuthorizationMessageHandler>()
            .ConfigureHandler(
                authorizedUrls: new[] { configuration.Audience }); //<--- The URI used by the Server project.
        return handler;
    });

// Supply HttpClient instances that include access tokens when making requests to the server project
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("API"));

builder.Services.AddOidcAuthentication(options =>
{
    options.ProviderOptions.AdditionalProviderParameters.Add("audience", configuration.Audience);
    options.ProviderOptions.Authority = configuration.Authority;
    options.ProviderOptions.ClientId = configuration.ClientId;
    options.ProviderOptions.ResponseType = "code";
});

builder.Services.AddScoped<API>();
builder.Services.AddScoped<SpinnerService>();

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddBlazoredToast();
builder.Services.AddBlazoredModal();
builder.Services.AddBlazoredSessionStorage();

await builder.Build().RunAsync();

