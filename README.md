## What & Why
This project is an example of one way to build a .NET Web application, using Blazor, Web API, and SQL. Demonstrating simple CRUD & Search operations, protected by Authentication/Authorization. This is an active application that I continue to update as .NET is updated and expanded. I created this template because the stock templates from Microsoft do not offer all the functionality I want in a template.

Some of the page and components in the WASM client app are copied from or derived from the stock Microsoft templates, for example login and register.

My preferred method of hosting is to use an Azure App Service. This template uses Sqlite for the database. In my experience Sqlite doesn't perform reliably on an Azure App Service. This demo is hosted on an Ubuntu server. You can review the CI/CD action for ideas on deploying to a Linux server instead of an App Service. Feel free to reach out if you have questions for setting up a server. 

## Technologies
 - .NET 8 & C#
 - Web API
 - Blazor WASM
 - Web API
 - SQL
 - Identity API
 - Identity API 2FA
 - GitHub Actions
 - [Blazored Libraries (Toast, LocalStorage, Modal)](https://github.com/Blazored)
 - [Bootstrap CDN (Bootstrap and Boostrap Icons)](https://getbootstrap.com/)

## Getting Started
Getting started with this project is easy.
1. Recommendation: Use the Github 'Use this template' feature. Then clone the repo.
2. Using the powershell script 'RenameProject.ps1', rename the 'BlazorTemplate' folders, files, and code references from 'BlazorTemplate' to the 'NewProjectNameOfYourChoice'. From the root of the solution, run this command: 

   Some files and folders will not be updated if they are open. Close VS Code, Visual Studio, or other tools prior to running this script.
   ```
   .\RenameProject.ps1 -FolderPath . -OldName "BlazorTemplate"  -NewName "NewProjectNameOfYourChoice"
   ```
   You can delete the script after you use it. Unless you want to rename your project again, there is no reason to keep it.

4. From the root of the solution, start the API project by running this command:
   ```
   dotnet watch --project NewProjectNameOfYourChoice.API
   ```
   The API project acts as the host for the API and the App.

5. You can create data in the UI, or you can use the API and the 'seed' method to create a large quantity of seed data to expirament with.

## Misc Details    
1. This project has no required outside dependencies to get started. The database is a SQLite DB, and the database will be created automatically when you startup the project the first time. You simply need to clone the repo, then run the API project by calling 'dotnet watch' from the API project folder.
2. Authentication is handled by ASP Identity, and is stored in your own DB.
3. On optional (but recommended) dependency is SendGrid. This template uses SendGrid to send emails. The template does not require SendGrid in order to work, however some features are not available until you add a SendGrid API key and system email address to the AppSettings.json. Features you cannot use without SendGrid include:
   - Account Activation and Email Confirmation
   - Account/Password recovery
   - Allowing a user to change their email/username.
4. This application demonstrates simple CRUD operations. You can create data in the UI, or you can use the API and the 'seed' method to create a large quantity of seed data to expirament with.
5. This template has some basic admin features. The first user to register will automatically be given the administrator role. Administrative abilities include: enable/disable registration, setting the SendGrid email  API values, and listing/deleting registered users.

## Project Architecture
This application now has 5 projects in the solution:
 - **API**: Handling HTTP requests and delegating all database and business logic operations to the Service layer. Manages authentication and authorization. Hosts the App in the same process.
 - **Service**: The middle layer where all business logic and database operations are performed. The API does not directly interact with the database.
 - **App**: A Blazor WASM project that consumes the API. Hosted in the API process.
 - **Dto**: Contains data transfer objects shared between the API, Service, and App projects. This was previously named 'Common'.
 - **Database**: Contains all the Entities and DB Context.

This separation makes the architecture more maintainable and scalable. The API project hosts both the API and the App. The App, while an independent application, is hosted as part of the API, which removes the need for complex CORS configurations. On startup, you can browse to Swagger to use the API directly or browse to the App.

## API Versioning
There are tools to handle API versioning. Add which ever tools you prefer. This template handles API version manually by specifying v1 in the service URL.

## Why Identity for Authentication?
1. It enables quick setup and getting your project going easily. 
2. It is easy to customize and supported by Microsoft.
3. It allows for 100% control of your user data and authentication/authorization process. There are other Authentication options such as Azure B2C/Entra and Auth0.

## [SendGrid](https://sendgrid.com/en-us/pricing)
This project uses SendGrid to send emails. A SendGrid API key is required. The demo has a key configured, but that value/key is not checked into the Repo. You will need to specify your own SendGrid API key and system email address. Some features that require email are not available until you provide the necessary SendGrid values. It is a simple process to create your own SendGrid account and retreive your API key.

## Why SQLite?
It runs on Windows and Linux. It is great for this template. Depending on your projects needs, it may work for production. If you need more than SQLite offers then I recommend switching to Azure SQL. If you switch to Azure SQL, besure to delete your SQLite DB migrations and create new a 'Initial Migration' for your new Azure SQL DB.

## DB Migrations
This project includes the necessary "Initial Creation" DB migration, used for the initial creation of a DB when the application connects to the DB for the first time. The Program.cs in the API project will automatically check for DB migrations which need to run, and run them automatically. You can run the DB migrations manually if desired. The commands below outline how to generate a DB migration and run a migration.

https://docs.microsoft.com/en-us/ef/core/cli/dotnet#common-options

## Using dotnet CLI
Run these commands from the root of the solution. Adjust these commands to match the name of your project (Replace 'BlazorTemplate')
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


## Licensing
This project uses the 'Unlicense'.  It is a simple license - review it at your own leisure.

## Resources
- [Identity API with WebAPI](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity-api-authorization?view=aspnetcore-8.0)
- [Blazor Environment Variables](https://learn.microsoft.com/en-us/aspnet/core/blazor/fundamentals/environments?view=aspnetcore-8.0)

## Misc & Recommended Tools
1. [Azure](https://portal.azure.com)
2. [Namecheap](https://namecheap.com)
2. [Namecheap Logo Maker](https://www.namecheap.com/logo-maker/)
3. [SSLS](https://www.ssls.com/)
4. [SVG Crop](https://svgcrop.com/)



