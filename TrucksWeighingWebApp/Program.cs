using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TrucksWeighingWebApp.Data;
using TrucksWeighingWebApp.Infrastructure.Identity;
using TrucksWeighingWebApp.Infrastructure.Telemetry;
using TrucksWeighingWebApp.Mappings;
using TrucksWeighingWebApp.Models;
using TrucksWeighingWebApp.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using TrucksWeighingWebApp.Services.Auth;
using TrucksWeighingWebApp.Services.Export;

var builder = WebApplication.CreateBuilder(args);

// Add user-secrets
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

// SeedOptions
builder.Services.Configure<SeedOptions>(builder.Configuration.GetSection("Seed"));

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
builder.Services.AddTransient<IEmailSender, SendGridEmailService>();

// Excel export
builder.Services.AddSingleton<ITruckExcelExporter, TruckExcelExporter>();

// QuestPDF licence
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

// Automapper
builder.Services.AddAutoMapper(typeof(InspectionProfile));

// MVC
builder.Services.AddControllersWithViews();

builder.Services.AddMemoryCache();


var app = builder.Build();

using(var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await IdentitySeed.SeedAsync(services);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

//!!! uncomment when production
//app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseMiddleware<UserSessionMiddleware>();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
