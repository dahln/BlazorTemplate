using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;

using BlazorTemplate.App;
using BlazorTemplate.App.Services;

using Blazored.LocalStorage;
using Blazored.Modal;
using Blazored.Toast;   
using BlazorSpinner;
using Blazored.SessionStorage;
using BlazorTemplate.Identity;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// register the cookie handler
builder.Services.AddScoped<CookieHandler>();

// set up authorization
builder.Services.AddAuthorizationCore();

// register the custom state provider
builder.Services.AddScoped<AuthenticationStateProvider, CookieAuthenticationStateProvider>();

// register the account management interface
builder.Services.AddScoped(
    sp => (IAccountManagement)sp.GetRequiredService<AuthenticationStateProvider>());

//This is a "static" approach to environments. Adjusted for your individual needs.
string ApiUrl;
if(builder.HostEnvironment.IsProduction())
    ApiUrl = @"https://BlazorTemplateApi.AzureWebsites.net";
else
    ApiUrl = @"https://localhost:7267/";

builder.Services.AddHttpClient("API", client => client.BaseAddress = new Uri(ApiUrl))
    .AddHttpMessageHandler<CookieHandler>();

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("API"));

builder.Services.AddScoped<API>();
builder.Services.AddScoped<SpinnerService>();

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddBlazoredToast();
builder.Services.AddBlazoredModal();
builder.Services.AddBlazoredSessionStorage();

await builder.Build().RunAsync();


