using System.Security.Claims;
using BlazorTemplate.API.Utility;
using BlazorTemplate.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

//Add CORS policy for WASM client. URL for the client is pulled from AppSettings.
builder.Services.AddCors(
    options => options.AddPolicy(
        "wasm",
        policy => policy.WithOrigins(builder.Configuration.GetValue<string>("Client").Split(","))
            .AllowAnyMethod()
            .SetIsOriginAllowed(pol => true)
            .AllowAnyHeader()
            .AllowCredentials()));

//Add the Database context.
builder.Services.AddDbContext<ApplicationDbContext>(
    options => options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

//Authorization is handles by Identity
builder.Services.AddAuthorization();

//Identiy API needs to be accessible
builder.Services.AddIdentityApiEndpoints<IdentityUser>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Identity options. Uncomment this to required email confirmation before allowing sign in.
//Other options are available.
builder.Services.Configure<IdentityOptions>(options =>
{
    //options.SignIn.RequireConfirmedEmail = true;
});

//Adding necessary services.
builder.Services.AddTransient<UserManager<IdentityUser>>();
builder.Services.AddTransient<SignInManager<IdentityUser>>();
builder.Services.AddTransient<IEmailSender<IdentityUser>, EmailSender>();
builder.Services.Configure<AuthMessageSenderOptions>(builder.Configuration);

var app = builder.Build();

//Check for and automatically apply pending migrations
//You can run DB migrations manually, or allow the system to run DB migrations on startup.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var db = services.GetRequiredService<ApplicationDbContext>();
    if (db != null)
    {
        var migrations = db.Database.GetPendingMigrations();
        if (migrations.Any())
            db.Database.Migrate();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//CORS policy was created earlier. The app needs to specify that it is being used.
app.UseCors("wasm");

//Authorization was added, now the app needs to use it.
app.UseAuthorization();

app.MapControllers();

//Expose identity API endpoints. Identity API doesn't include a logout method. One was created in the account controller, along with other account related endpoints.
app.MapIdentityApi<IdentityUser>();

app.Run();


