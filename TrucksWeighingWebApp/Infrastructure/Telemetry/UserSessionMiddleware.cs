using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;
using TrucksWeighingWebApp.Data;
using TrucksWeighingWebApp.Models;

namespace TrucksWeighingWebApp.Infrastructure.Telemetry
{
    public class UserSessionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;

        private static readonly TimeSpan TouchPeriod = TimeSpan.FromMinutes(1);
        private static readonly TimeSpan SessionGap = TimeSpan.FromHours(6);

        public UserSessionMiddleware(RequestDelegate next, IMemoryCache cache)
        {
            _next = next;
            _cache = cache;
        }

        public async Task Invoke(HttpContext httpContext, ApplicationDbContext db)
        {
            var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrWhiteSpace(userId))
            {
                var cacheKey = $"sess_touch:{userId}";
                if (!_cache.TryGetValue(cacheKey, out _))
                {
                    var now = DateTime.UtcNow;

                    var last = await db.UserSessions                        
                        .Where(s => s.UserId == userId && !s.IsClosed)
                        .OrderByDescending(s => s.LastSeenUtc)
                        .FirstOrDefaultAsync();

                    var ip = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault() 
                        ?? httpContext.Connection.RemoteIpAddress?.ToString();

                    var userAgent= httpContext.Request.Headers["User-Agent"].ToString();

                    if (last == null)
                    {
                        db.UserSessions.Add(new UserSession
                        {
                            UserId = userId,
                            StartedUtc= now,
                            LastSeenUtc= now,
                            Ip = ip,
                            UserAgent = userAgent
                        });

                        await db.SaveChangesAsync();
                    }
                    else
                    {
                        await db.UserSessions
                            .Where(s => s.Id == last.Id)
                            .ExecuteUpdateAsync(setters => setters
                                .SetProperty(x => x.LastSeenUtc, now)
                                .SetProperty(x => x.Ip, ip)
                                .SetProperty(x => x.UserAgent, userAgent)
                        );
                    }                    

                    

                    _cache.Set(cacheKey, true, TouchPeriod);
                }
            }

            await _next(httpContext);
        }
    }
}
