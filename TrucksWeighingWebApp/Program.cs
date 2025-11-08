using AutoMapper;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net;
using System.Security.Claims;
using TrucksWeighingWebApp.Data;
using TrucksWeighingWebApp.Infrastructure.Identity;
using TrucksWeighingWebApp.Infrastructure.Telemetry;
using TrucksWeighingWebApp.Mappings;
using TrucksWeighingWebApp.Models;
using TrucksWeighingWebApp.Services;
using TrucksWeighingWebApp.Services.Auth;
using TrucksWeighingWebApp.Services.Export;
using TrucksWeighingWebApp.Services.Logos;


var builder = WebApplication.CreateBuilder(args);

// Add user-secrets
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

// SeedOptions
//builder.Services.Configure<SeedOptions>(builder.Configuration.GetSection("Seed"));
builder.Services
    .AddOptions<SeedOptions>()
    .Bind(builder.Configuration.GetSection("Seed"))
    .Validate(o => o.Roles is { Length: > 0 }, "Seed: Roles must contain at least one role")
    .Validate(o => !o.EnsureAdmin || (!string.IsNullOrWhiteSpace(o.AdminEmail) && !string.IsNullOrWhiteSpace(o.AdminPassword)), "Seed: EnsureAdmin=true requires AdminEmail and AdminPassword")
    .ValidateOnStart();

// DB: PostgreSQL
var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Identity + Roles
builder.Services
    .AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;        
        options.User.RequireUniqueEmail = true;

        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.AllowedForNewUsers = true;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// LoginCount
builder.Services.AddTransient<AppCookieEvents>();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.EventsType = typeof(AppCookieEvents);
});

// Email
builder.Services
    .AddOptions<SendGridOptions>()
    .Bind(builder.Configuration.GetSection("SendGrid"))
    .Validate(options => !string.IsNullOrWhiteSpace(options.ApiKey),"ApiKey missing")
    .Validate(options => !string.IsNullOrWhiteSpace(options.FromEmail), "FromEmail missing")
    .ValidateOnStart();
builder.Services.AddTransient<IEmailSender, SendGridEmailService>();

// Excel export
builder.Services.AddSingleton<ITruckExcelExporter, TruckExcelExporter>();

builder.Services.AddScoped<IUserLogoService, UserLogoService>();

// QuestPDF licence
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

// Automapper
builder.Services.AddAutoMapper(typeof(InspectionProfile));

// MVC
builder.Services.AddControllersWithViews();

builder.Services.AddMemoryCache();

// Ensure persistent Data Protection keys directory exists
// Keeps authentication cookies and tokens valid after app restarts

var keyPath = builder.Environment.IsDevelopment()
    ? Path.Combine(builder.Environment.ContentRootPath, "keys")
    : "/var/lib/aspnet/keys";

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keyPath))
    .SetApplicationName("TrucksWeighingWebApp");

// Forwarded headers (Nginx)
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
    options.KnownProxies.Add(IPAddress.Loopback);
    options.KnownProxies.Add(IPAddress.IPv6Loopback);
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.HttpOnly = true;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("PortfolioCors", policy => 
    {
        policy.AllowAnyHeader()
            .AllowAnyMethod()
            .WithOrigins(
                "https://p.zalizko.site",
                "http://localhost:3000"
                );
    });
});


var app = builder.Build();

// Ensure required folder for DataProtection keys exists
try
{
    Directory.CreateDirectory(keyPath);
    
    var dirInfo = new DirectoryInfo(keyPath);
    dirInfo.Attributes |= FileAttributes.Hidden;
}
catch (Exception ex)
{
    app.Logger.LogWarning(ex, "Cannot create/access DataProtection key folder {KeyPath}", keyPath);
}




// Ensure required folder logos for wwwroot exists
var logosDir = Path.Combine(app.Environment.WebRootPath, "uploads", "logos");
try
{
    Directory.CreateDirectory(logosDir);
}
catch (Exception ex)
{
    app.Logger.LogWarning(ex, "Failed to create or access logos directory {LogosDir}", logosDir);
}


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();

    var services = scope.ServiceProvider;
    await IdentitySeed.SeedAsync(services);
}

// Pipeline
app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}


app.UseStaticFiles();

app.UseRouting();

app.UseCors("PortfolioCors");

app.UseAuthentication();

app.UseMiddleware<UserSessionMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
