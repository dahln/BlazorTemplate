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

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

//Get Configuration values from server
HttpClient _tempHttpClient = new HttpClient() { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
var configuration = await  _tempHttpClient.GetFromJsonAsync<BlazorDemoCRUD.Common.Configuration>("api/v1/configuration");
builder.Services.AddSingleton<BlazorDemoCRUD.Common.Configuration>(configuration);


builder.Services.AddHttpClient("API", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
    .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

// Supply HttpClient instances that include access tokens when making requests to the server project
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("API"));

builder.Services.AddMsalAuthentication(options =>
{
    options.ProviderOptions.Authentication.ClientId = configuration.ClientId;
    options.ProviderOptions.Authentication.Authority = configuration.Authority;
    options.ProviderOptions.Authentication.ValidateAuthority = configuration.ValidateAuthority;
    options.ProviderOptions.DefaultAccessTokenScopes.Add(configuration.DefaultAccessTokenScopes);
    options.ProviderOptions.LoginMode = "redirect";
});

builder.Services.AddScoped<API>();
builder.Services.AddScoped<SpinnerService>();

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddBlazoredToast();
builder.Services.AddBlazoredModal();

await builder.Build().RunAsync();

