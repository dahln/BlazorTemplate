# This branch exlores AzureB2C intead of Auth0.

## What & Why
This project is a demo one way to build a .NET Web application, using Blazor, Web API, and SQL. Demonstrating simple CRUD & Search operations, protected by Authentication/Authorization. This is an active application that I continue update as .NET is updated and expanded. In some scenerios, this application could be used as a template for starting new projects.

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
1. You will need a MSSQL Database. I recommend an Azure SQL DB. [These instructions from Microsoft outline how to create an Azure SQL DB - adjust the billing/tio your specific billing needs](https://docs.microsoft.com/en-us/azure/azure-sql/database/single-database-create-quickstart). After the database is created, copy the connection string into the Server/appsettings.json, in the "ConnectionStrings.DefaultConnection" property.
2. Setup Azure AD B2C for authentication - see details below.

## Setup Azure AD B2C
These articles are helpful in setting up Azure B2C. Copy the necessary values from the server and client apps. I hope to include more detailed instructions later.
1. [Azure B2C & Blazor WASM Hosted](https://learn.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/hosted-with-azure-active-directory-b2c?view=aspnetcore-6.0)
2. [Helpful Blog on Azure B2C Setup](https://code-maze.com/azure-active-directory-b2c-with-blazor-webassembly-hosted-apps/)
3. [Using Microsoft Graph to allow users to Self-Manage](https://learn.microsoft.com/en-us/azure/active-directory-b2c/microsoft-graph-operations)
4. [Add User Administrator role to Application](https://learn.microsoft.com/en-us/azure/active-directory-b2c/microsoft-graph-get-started?tabs=app-reg-ga#optional-grant-user-administrator-role)


#### Server AppSettings.json
```
{
  "ConnectionStrings": {
    "DefaultConnection": "",
  },
  "AllowedHosts": "",
  "AzureAdB2C": {
    "Instance": "",
    "ClientId": "",
    "Domain": "",
    "SignUpSignInPolicyId": "",
    "TenantId": "",
    "Secret": ""
  },
  "AzureAdB2CClient": {
    "Authority": "",
    "ClientId": "",
    "DefaultAccessTokenScopes": "",
    "ValidateAuthority": false
  }
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
2. Make sure that the CI/CD config yaml file builds the Server project. Not specifying the Server project will build all projects and cause startup conflicts.

## Licensing
This project uses the 'Unlicense'.  It is a simple license - review it at your own leisure.

## Misc & Recommended Tools
1. [Azure](https://portal.azure.com)
2. [Namecheap Logo Maker](https://www.namecheap.com/logo-maker/)
3. [SSLS](https://www.ssls.com/)
4. [SVG Crop](https://svgcrop.com/)
5. [Azure B2C & Blazor WASM Hosted](https://learn.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/hosted-with-azure-active-directory-b2c?view=aspnetcore-6.0)
6. [Helpful Blog on Azure B2C Setup](https://code-maze.com/azure-active-directory-b2c-with-blazor-webassembly-hosted-apps/)
7. [Using Microsoft Graph to allow users to Self-Manage](https://learn.microsoft.com/en-us/azure/active-directory-b2c/microsoft-graph-operations)
8. [Add User Administrator role to Application](https://learn.microsoft.com/en-us/azure/active-directory-b2c/microsoft-graph-get-started?tabs=app-reg-ga#optional-grant-user-administrator-role)
