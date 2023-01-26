[![CI/CD Azure App Service](https://github.com/dahln/BlazorDemoCRUD/actions/workflows/master_BlazorDemoCRUD.yml/badge.svg)](https://github.com/dahln/BlazorDemoCRUD/actions/workflows/master_BlazorDemoCRUD.yml)

![BlazorDemoCRUD : A .NET Web App Demo](https://repository-images.githubusercontent.com/564882428/88dae1ef-7454-4b37-8240-6a59a9338f05)

## Demo
https://BlazorDemoCRUD.azurewebsites.net

## What & Why
This project is a example one way to build a .NET Web application, using Blazor, Web API, and SQL. Demonstrating simple CRUD & Search operations, protected by Authentication/Authorization. This is an active application that I continue update as .NET is updated and expanded. In some scenerios, this application could be used as a template for starting new projects.

## Technologies
 - .NET 7
 - Blazor WASM
 - Web API
 - MS SQL
 - Auth0
 - GitHub Actions
 - Azure
 - Blazored Libraries
 - Bootstrap
 - Font Awesome

## Getting Started
Getting started with this project is easy.
1. You will need a MSSQL Database. I recommend an Azure SQL DB. [These instructions from Microsoft outline how to create an Azure SQL DB - adjust the billing/tier to your specific billing needs](https://docs.microsoft.com/en-us/azure/azure-sql/database/single-database-create-quickstart). After the database is created, copy the connection string into the Server/appsettings.json, in the "ConnectionStrings.DefaultConnection" property.
2. Setup Auth0 - see details below.

## Why Auth0?
I have used a lot of different Authentication/Authorization methods. In my opinion, Auth0 offers a simple, clean, easy-to-use Authentication service. They are reputable, and affordable - the free tier meets my needs most of the time. An alternative to Auth0 would be Azure Active Directory B2C; a good product, but I felt like it was a bit complicated. This articles from Auth0 outlines how to setup Auth0 with a Blazor WASM app: [Follow these instructions from Auth0](https://auth0.com/blog/securing-blazor-webassembly-apps/#Registering-the-Blazor-WASM-App-with-Auth0)

## Specific Instructions for Auth0 Setup
1. Create a new tenant
2. Delete the Generic "Default App"
3. Create a new Auth0 application, select "Single Page Web Applications". In the new client, find the 'Allowed Callback URLs, and enter "https://{YOURDOMAIN}/authentication/login-callback". Then find 'Allowed Logout URL' and enter "https://{YOURDOMAIN}". Replace "YOURDOMAIN" with your specific domain.
4. From the new client app, copy the "Domain" value and past it into Client AppSettings.json, prepending "https://", so that the Authority field in the appsettings reads: "Authority":"https://BlazorDemoCRUD.us.auth0.com"
5. From the new client app, copy the ClientId into the Server AppSettings.Json => AppClientId 
6. In the Server AppSettings.json, in the Audience field, enter the API Url. In the case of the demo app, the Audience is "https://BlazorDemoCRUD.dahln.com"
6. Create another Auth0 application, but this on will be under the "API" section. Provide a name, and identifier.
7. Open the Auth0 Management API - System API app (this is not that application you just created). Go to the Machine to Machine applications tab. Select the API application and "Authorize" it. Under the permissions section select "create:client_grants","update:users","delete:users", and "read:users". Click update to apply the changes.
8. Go to the Applications section, and open the Machine to Machine app. Copy the Client Id to the Server AppSettings.json file. Also copy the Client Secret to the AppSettings.json file.
9. Copy the Authority URL into the other fields, but with a few variations, the end result should look similar to this:

## Auth0 Logout Issue
Note: As of right this writing (1/3/2023), there are appears to be a bug in (either in the library from Auth0 or Blazor Authetication/Authorization) where logout is sometimes not successful on the first attempt. This behaviour has been noted by other developers who have also created Blazor WASM apps using Auth0. The logout issue I have observed has been reported: [GitHub 40046](https://github.com/dotnet/aspnetcore/issues/40046). [This article for Auth0](https://auth0.com/blog/securing-blazor-webassembly-apps/) outlines how to secure a Blazor WASM app using Auth0. The article recognizes an issue with logout. As I implemented authentication/authorization in the application, I also had issues with logouts.  The issue appears to be an issue with Navigation.NavigateToLogout(...) creating a race condition during logouts. My solution was to NOT use that method. Instead, I created a new logout component. I had issus with the "RemoteAuthenticatorView" component - my own component worked well. The TopNavigation component (which has the logout button) calls the Auth0 logout URL, with a returnTo call back of "{BaseURL}/logout". The logout component will clear the session storage, which effectivly ends the session. The logout component then redirects the user back the root page and forces a page/app reload. This solution has worked well in my testing. It hope that Microsoft and/or Auth0 will find solutions to this issue so that I can use a 'stock' logout process.

I should note the Azure B2C does not have the same logout issue that Auth0 has. But I don't know why.

#### Server AppSettings.json
```
"Auth0": {
    "Audience": "https://BlazorDemoCRUD.dahln.com",
    "Domain": "BlazorDemoCRUD.us.auth0.com",
    "Authority": "https://BlazorDemoCRUD.us.auth0.com",
    "ManagementAPI": "https://BlazorDemoCRUD.us.auth0.com/api/v2/",
    "TokenAPI": "https://BlazorDemoCRUD.us.auth0.com/oauth/token",
    "AppClientId": "SINGLE-PAGE-APP-CLIENTID",
    "ClientId": "MACHINE-TO-MACHINE-ID-SHOULD-REPLACE-THIS",
    "Secret": "MACHINE-TO-MACHINE-SECRET-SHOULD-REPLACE-THIS"
} 
```


## DB Migrations
This project includes the necessary "Initial Creation" DB migration, used for the inital creation of a DB when the appication connects to the DB for the first time. The StartUp.cs in the Server project will automatically check for DB migrations which need to run, and run them automatically. You can run the DB migrations manually is desired. The instructions below outline how to generate a DB migration and run a migration.

Notice the use of Folder names and not csproj names when using the dotnet ef cli.

https://docs.microsoft.com/en-us/ef/core/cli/dotnet#common-options

## Using Visual Studio
```
Add-Migration -Project BlazorDemoCRUD.Data -StartupProject BlazorDemoCRUD.Server -Name InitialCreate
```
```
Update-Database -Project BlazorDemoCRUD.Data -StartupProject BlazorDemoCRUD.Server
```
## Using dotnet CLI
```
dotnet ef migrations add InitialCreate --project Data --startup-project Server
```
```
dotnet ef database update --project Data --startup-project Server -- --environment Development
```
Note: DB migrations are automatically checked for and applied when necessary. See Server/Startup.cs, Line: 34


## Ignore Local Changes to AppSettings.json
Sensative configuration data, such as the DB connection strings, are kept in the  appsettings.json files. AppSettings exist in the API and Web projects. Depending on your situation, you may not want to check in these values to the repo. Use the following commands to ignore changes to the appsettings.json files:
 ```
 git update-index --assume-unchanged .\Server\appsettings.json
 ```
 To reverse the ignore, use these commands:
 ```
 git update-index --no-assume-unchanged .\Server\appsettings.json
 ```

## Setup CI/CD
Most applications need to be build and deployed. Outlined here are steps to setup automatted build and deployments (CI/CD)
1. [Microsoft outlines how to use Azure App Service Deployment Center to setup CI/CD with GitHub Actions](https://docs.microsoft.com/en-us/azure/app-service/deploy-github-actions?tabs=applevel#use-the-deployment-center). 
2. This project is a ["Hosted Blazor"](https://docs.microsoft.com/en-us/aspnet/core/blazor/host-and-deploy/webassembly?view=aspnetcore-6.0#hosted-deployment-with-aspnet-core) application. When I deployed this application, I found that it wouldn't startup automatically. The default yaml workflow file, created by Azure, builds and publishes all projects in the solution. The consequence of this is that multiple .runtimeconfig files are created. Specifying that the build and publish should build the 'Server' project solves this issue. As a result, no special startup commands are necessary.

## Licensing
This project uses the 'Unlicense'.  It is a simple license - review it at your own leisure.

## Misc & Recommended Tools
1. [Azure](https://portal.azure.com)
2. [Namecheap Logo Maker](https://www.namecheap.com/logo-maker/)
3. [SSLS](https://www.ssls.com/)
4. [SVG Crop](https://svgcrop.com/)
