using System.Security.Claims;
using BlazorTemplate.API.Utility;
using BlazorTemplate.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

//Add this so we can access HTTPContext data in other services.
builder.Services.AddHttpContextAccessor();

//Add the Database context.
builder.Services.AddDbContext<ApplicationDbContext>(
    options => options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

//Authorization is handles by Identity
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdministratorRole", policy => policy.RequireRole("Administrator"));
});

//Identiy API needs to be accessible
builder.Services.AddIdentityApiEndpoints<IdentityUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => { 
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "BlazorTemplate API", Version = "v1" }); 
        c.ResolveConflictingActions((apiDescriptions) => apiDescriptions.First());
    });


builder.Services.Configure<DataProtectionTokenProviderOptions>(options => options.TokenLifespan = TimeSpan.FromDays(1));

//Identity options. Uncomment this to required email confirmation before allowing sign in.
//Other options are available.
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Lockout.AllowedForNewUsers = true;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    //options.SignIn.RequireConfirmedEmail = true;
});

//Adding necessary services.
builder.Services.AddTransient<UserManager<IdentityUser>>();
builder.Services.AddTransient<RoleManager<IdentityRole>>();
builder.Services.AddTransient<SignInManager<IdentityUser>>();
builder.Services.AddTransient<IEmailSender<IdentityUser>, EmailSender>();

var app = builder.Build();

//Check for and automatically apply pending migrations
//You can run DB migrations manually, or allow the system to run DB migrations on startup.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var db = services.GetRequiredService<ApplicationDbContext>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    if (db != null)
    {
        var migrations = db.Database.GetPendingMigrations();
        if (migrations.Any())
            db.Database.Migrate();
    }

    var roleSystemAdministratorExists = await roleManager.RoleExistsAsync("Administrator");
    if (!roleSystemAdministratorExists)
    {
        // Create the "Subscribed" role if it doesn't exist
        await roleManager.CreateAsync(new IdentityRole("Administrator"));
    }

    var systemSettings = db.SystemSettings.Any();
    if(systemSettings == false)
    {
        var newSystemSettings = new SystemSetting();
        db.SystemSettings.Add(newSystemSettings);
        db.SaveChanges();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//Host the BlazorTemplate.App files in the same 'Server' process as the API.
app.MapStaticAssets();
app.UseRouting();

//Authorization was added, now the app needs to use it.
app.UseAuthorization();

app.MapControllers();

//Necessary For the BlazorTemplate.App
app.MapFallbackToFile("index.html");

//Expose identity API endpoints. Identity API doesn't include a logout method. One was created in the account controller, along with other account related endpoints.
app.MapIdentityApi<IdentityUser>();
app.MapPost("/register", () => "Deprecated. Use /api/v1/account/register."); //This will disable the built in 'Identity /register' method.

app.Run();

