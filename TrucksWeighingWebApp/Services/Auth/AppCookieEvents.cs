using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using TrucksWeighingWebApp.Data;
using TrucksWeighingWebApp.Models;

namespace TrucksWeighingWebApp.Services.Auth
{
    public class AppCookieEvents : CookieAuthenticationEvents
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AppCookieEvents> _logger;

        public AppCookieEvents(
            UserManager<ApplicationUser> userManager,
            ILogger<AppCookieEvents> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public override async Task SignedIn(CookieSignedInContext context)
        {
            try
            {
                var userId = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return;
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return;
                }

                // Login Count
                user.LoginCount += 1;
                user.LastLoginAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                // New Session to fix new login
                var db = context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
                var now = DateTime.UtcNow;
                var ip = context.HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault() 
                    ?? context.HttpContext.Connection.RemoteIpAddress?.ToString();
                var userAgent = context.HttpContext.Request.Headers["User-Agent"].ToString();

                db.UserSessions.Add(new UserSession
                {
                    UserId = userId,
                    StartedUtc = now,
                    LastSeenUtc = now,
                    Ip = ip,
                    UserAgent = userAgent,
                    IsClosed = false
                });

                await db.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to update login counter on SignedIn");
            }
        }

        public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
        {
            try
            {
                var userId = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return;
                }
                                              
                var user = await _userManager.FindByIdAsync(userId);
                
                if (user == null || !user.IsActive)
                {
                    context.RejectPrincipal();
                    await context.HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
                    return;
                }

                if (user.LockoutEnabled && await _userManager.IsLockedOutAsync(user))
                {
                    context.RejectPrincipal();
                    await context.HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
                    return;
                }

            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to block user");
            }
        }
    }
}
