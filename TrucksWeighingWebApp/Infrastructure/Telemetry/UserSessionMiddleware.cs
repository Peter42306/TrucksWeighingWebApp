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
                        .AsNoTracking()
                        .Where(s => s.UserId == userId && !s.IsClosed)
                        .OrderByDescending(s => s.LastSeenUtc)
                        .FirstOrDefaultAsync();

                    if (last == null || now - last.LastSeenUtc > SessionGap)
                    {
                        var s = new UserSession
                        {
                            UserId = userId,
                            StartedUtc = now,
                            LastSeenUtc = now,
                            Ip = httpContext.Request.Headers.UserAgent.ToString()
                        };
                        db.UserSessions.Add(s);
                    }
                    else
                    {
                        await db.UserSessions.ExecuteUpdateAsync(setters => setters.SetProperty(x => x.LastSeenUtc, now));
                    }

                    await db.SaveChangesAsync();

                    _cache.Set(cacheKey, true, TouchPeriod);
                }
            }

            await _next(httpContext);
        }
    }
}
