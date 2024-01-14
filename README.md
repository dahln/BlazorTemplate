## Demo
[https://template.dahln.com](https://template.dahln.com)

This is a fully functional demo. If this template was helpful to you, please consider supporting this project by donating.

## What & Why
This project is an example of one way to build a .NET Web application, using Blazor, Web API, and SQL. Demonstrating simple CRUD & Search operations, protected by Authentication/Authorization. This is an active application that I continue to update as .NET is updated and expanded. I created this template because the stock templates from Microsoft do not offer all the functionality I want in a template.

Some of the page and components in the WASM client app are copied from or derived from the stock Microsoft templates, for example login and register.

## Technologies
 - .NET 8 & C#
 - Web API
 - Blazor WASM
 - Web API
 - SQL
 - Identity API
 - Identity API 2FA - Comming Soon
 - GitHub Actions
 - [Blazored Libraries (Toast, LocalStorage, Modal)](https://github.com/Blazored)
 - [Bootstrap CDN (Bootstrap and Boostrap Icons)](https://getbootstrap.com/)

## Getting Started
Getting started with this project is easy.
1. This project has no outside dependencies you are required to setup. The database is a SQLite DB, and the database will be created automatically when you startup the project the first time. You simply need to clone the repo, then run the API project and the App project, by calling 'dotnet watch' from both the API and App projects.
2. Authentication is handled by ASP Identity, and is stored in your own DB.
3. This template uses SendGrid to send emails. The template does not require SendGrid in order to work, but some features are not available until you add a SendGrid API key to the AppSettings.json. Features you cannot use without SendGrid include:
- Account Activation and Email Confirmation
- Account/Password recovery
- Allowing a user to change their email/username.
4. This application demonstrates simple CRUD operations. You can create data in the UI, or you can use the API and the 'seed' method to create a large quantity of seed data to expirament with.

## Project Architecture
This application has 4 projects in the solution. The API, the App, Common, and Database. The API is all the server functionality; it contains all the database operations and server side logic. The App is a Blazor WASM project; it consumes the API. The Common project is data models that are common to both the API and the App; these 'Common' models make it easy to standardize the format of the data passed between the API and the App. The database project contains all the Entities and DB Context. Depending on your needs, you could create a service layer to contain all the business logic and remove DB operations from the API project. Every project is different, this template is a starting point.

## API Versioning
There are tools to handle API versioning. Add which ever tools you prefer. This template handles API version manually by specifying v1 in the service URL.

## Why Identity for Authentication?
1. It enables quick setup and getting your project going easily. 
2. It is easy to customize and supported by Microsoft.
3. It allows for 100% control of your user data and authentication/authorization process. There are other Authentication options such as Azure B2C/Entra and Auth0.

## Why SQLite?
It runs on Windows and Linux. It is great for this template. Depending on your projects needs, it may work for production. If you need more than SQLite offers then I recommend switching to Azure SQL. If you switch to Azure SQL, besure to delete your SQLite DB migrations and create new a 'Initial Migration' for your new Azure SQL DB.

## Accessing the API from the client App
The client App needs to know the URL of the API. This is specified in the Program.cs of the App project. Adjust it as necessary. 

## [SendGrid](https://sendgrid.com/en-us/pricing)
This project uses SendGrid to send emails. A SendGrid API key is required. The demo has a key configured, but that value/key is not checked into the Repo. You will need to specify your own SendGrid API key and system email address. Some features that require email are not available until you provide the necessary SendGrid values. It is a simple process to create your own SendGrid account and retreive your API key.

## CORS
Because the API and App are two seperate applications, CORS is a concern. The API creates a CORS policy. The 'Client' setting in AppSettings.json specifies the allowed origins for the CORS policy. The API reads and parses this as a comma separated list; more than likely you will only have a single origin.

## DB Migrations
This project includes the necessary "Initial Creation" DB migration, used for the initial creation of a DB when the application connects to the DB for the first time. The Program.cs in the API project will automatically check for DB migrations which need to run, and run them automatically. You can run the DB migrations manually if desired. The commands below outline how to generate a DB migration and run a migration.

https://docs.microsoft.com/en-us/ef/core/cli/dotnet#common-options

## Using dotnet CLI
Run these commands from the root of the solution.
```
dotnet ef migrations add InitialCreate --project BlazorTemplate.Database --startup-project BlazorTemplate.API
```
```
dotnet ef database update --project BlazorTemplate.Database --startup-project BlazorTemplate.API
```


## Ignore Local Changes to AppSettings.json
Sensative configuration data, such as the DB connection strings, are kept in the appsettings.json files. Depending on your situation, you may NOT want to check in these values to the repo. Use the following commands to ignore changes to the appsettings.json files:
 ```
 git update-index --assume-unchanged .\BlazorTemplate.API\appsettings.json
 ```
 To reverse the ignore, use these commands:
 ```
 git update-index --no-assume-unchanged .\BlazorTemplate.API\appsettings.json
 ```

## Environments
This template is set up for 2 environments: Development and Production. When running locally (Development), the App will detect you are running in a development environment and send requests to the local API. In production, it sends API requests to the URL specified in the App.Program.cs. Future versions of the template might add a different way to handle environment variables for the Blazor WebAssembly App. In production, add the necessary secrets or environment variables, as represented in the API.AppSettings.json.


## Setup CI/CD
Most applications need to be built and deployed. Outlined here are steps to setup automated build and deployments (CI/CD)
1. [Microsoft outlines how to use Azure App Service Deployment Center to setup CI/CD with GitHub Actions](https://docs.microsoft.com/en-us/azure/app-service/deploy-github-actions?tabs=applevel#use-the-deployment-center). 
2. This project requires two deployments. A deployment for the API and a deployment for the client App. Create an Azure App Service and use the 'Deployment Center' to sent up CI/CD for the API. Adjust the .yaml file to only build and publish the 'API' project.
3. Create a Azure Static Web App for the client App. Adjusted the generated .yaml file to only build and deploy the 'App' project.

## Licensing
This project uses the 'Unlicense'.  It is a simple license - review it at your own leisure.

## Resources
- [Identity API with WebAPI](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity-api-authorization?view=aspnetcore-8.0)
- [Blazor Environment Variables](https://learn.microsoft.com/en-us/aspnet/core/blazor/fundamentals/environments?view=aspnetcore-8.0)

## Misc & Recommended Tools
1. [Azure](https://portal.azure.com)
2. [Porkbun](https://porkbun.com)
2. [Namecheap Logo Maker](https://www.namecheap.com/logo-maker/)
3. [SSLS](https://www.ssls.com/)
4. [SVG Crop](https://svgcrop.com/)


