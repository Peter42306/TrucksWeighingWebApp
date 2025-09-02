using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrucksWeighingWebApp.Data;
using TrucksWeighingWebApp.Infrastructure.Identity;

namespace TrucksWeighingWebApp.Controllers.Admin
{
    [Authorize(Roles = RoleNames.Admin)]
    [Route("admin/stats")]
    public class StatsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public StatsController(ApplicationDbContext db)
        {
            _db = db;
        }

        public class MonthlyRow
        {
            public int Year { get; set; }
            public int Month { get; set; }  // 1..12
            public int ActiveUsers { get; set; }
            public int Registrations { get; set; }
            public int Logins { get; set; }
            public int UniqueLogins { get; set; }
            //public int Inspections { get; set; }
            //public int ExcelExports { get; set; }
        }

        public class StatsVm
        {
            public int ActiveUsersToday { get; set; }
            public int ActiveUsersThisMonth { get; set; }

            public int RegistrationsThisMonth { get; set; }
            public int RegistrationsTotal { get; set; }

            //public int InspectionsThisMonth { get; set; }
            //public int InspectionsTotal { get; set; }

            //public int ExportsThisMonth { get; set; }
            //public int ExportsTotal { get; set; }

            public int LoginsThisMonth { get; set; }
            public int LoginsTotal { get; set; }

            public int UniqueLoginsThisMonth { get; set; }
            public int UniqueLoginsTotal { get; set; }

            public List<MonthlyRow> Monthly { get; set; } = new();
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var now = DateTime.UtcNow;
            var todayStart = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
            var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            // --- Active users
            var activeUsersToday = await _db.UserSessions
                .AsNoTracking()
                .Where(u => u.LastSeenUtc >= todayStart)
                .Select(u => u.UserId).Distinct().CountAsync();

            var activeUsersThisMonth = await _db.UserSessions
                .AsNoTracking()
                .Where(u => u.LastSeenUtc >= monthStart)
                .Select(u => u.UserId).Distinct().CountAsync();

            var monthlyActiveUsers = await _db.UserSessions
                .AsNoTracking()
                .GroupBy(s => new { s.LastSeenUtc.Year, s.LastSeenUtc.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Select(x => x.UserId).Distinct().Count() })
                .ToListAsync();

            // --- Registrations (по ApplicationUser.CreatedAt)
            var registrationsThisMonth = await _db.Users
                .AsNoTracking()
                .Where(u => u.CreatedAt >= monthStart)
                .CountAsync();

            var registrationsTotal = await _db.Users
                .AsNoTracking()
                .CountAsync();

            var monthlyRegistrations = await _db.Users
                .AsNoTracking()
                .GroupBy(u => new { u.CreatedAt.Year, u.CreatedAt.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
                .ToListAsync();

            // --- Logins / Unique logins (по UserSessions)
            var loginsThisMonth = await _db.UserSessions
                .AsNoTracking()
                .Where(s => s.StartedUtc >= monthStart)
                .CountAsync();

            var loginsTotal = await _db.UserSessions
                .AsNoTracking()
                .CountAsync();

            var monthlyLogins = await _db.UserSessions
                .AsNoTracking()
                .GroupBy(s => new { s.StartedUtc.Year, s.StartedUtc.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
                .ToListAsync();

            var uniqueLoginsThisMonth = await _db.UserSessions
                .AsNoTracking()
                .Where(s => s.StartedUtc >= monthStart)
                .Select(s => s.UserId)
                .Distinct()
                .CountAsync();

            var uniqueLoginsTotal = await _db.UserSessions
                .AsNoTracking()
                .Select(s => s.UserId)
                .Distinct()
                .CountAsync();

            var monthlyUniqueLogins = await _db.UserSessions
                .AsNoTracking()
                .GroupBy(s => new { s.StartedUtc.Year, s.StartedUtc.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Select(x => x.UserId).Distinct().Count() })
                .ToListAsync();

            
            //int inspectionsThisMonth = 0, inspectionsTotal = 0;
            //List<(int Year, int Month, int Count)> monthlyInspections = new();
            //if (_db.Model.FindEntityType("TrucksWeighingWebApp.Models.Inspection") != null)
            //{
            //    inspectionsThisMonth = await _db.Inspections
            //        .AsNoTracking()
            //        .Where(i => i.CreatedAt >= monthStart).CountAsync();

            //    inspectionsTotal = await _db.Inspections.AsNoTracking().CountAsync();

            //    monthlyInspections = await _db.Inspections
            //        .AsNoTracking()
            //        .GroupBy(i => new { i.CreatedAt.Year, i.CreatedAt.Month })
            //        .Select(g => new ValueTuple<int, int, int>(g.Key.Year, g.Key.Month, g.Count()))
            //        .ToListAsync();
            //}

            // --- Excel exports
            //int exportsThisMonth = 0, exportsTotal = 0;
            //List<(int Year, int Month, int Count)> monthlyExports = new();
            //if (_db.Model.FindEntityType("TrucksWeighingWebApp.Models.ExcelExportLog") != null)
            //{
            //    var exports = _db.Set<object>(); // заглушка — поменяешь на DbSet<ExcelExportLog> когда добавишь модель
            //    // здесь оставляем нули; блок готов к расширению
            //}

            // --- Merge monthly
            var keys = monthlyActiveUsers.Select(x => (x.Year, x.Month))
                .Concat(monthlyRegistrations.Select(x => (x.Year, x.Month)))                
                .Concat(monthlyLogins.Select(x => (x.Year, x.Month)))
                .Concat(monthlyUniqueLogins.Select(x => (x.Year, x.Month)))
                .Distinct()
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToList();

            //int GetMonthly(List<(int Year, int Month, int Count)> list, int y, int m)
            //    => list.FirstOrDefault(t => t.Year == y && t.Month == m).Count;

            var monthly = new List<MonthlyRow>();
            foreach (var k in keys)
            {
                monthly.Add(new MonthlyRow
                {
                    Year = k.Year,
                    Month = k.Month,
                    ActiveUsers   = monthlyActiveUsers.FirstOrDefault(x => x.Year == k.Year && x.Month == k.Month)?.Count ?? 0,
                    Registrations = monthlyRegistrations.FirstOrDefault(x => x.Year == k.Year && x.Month == k.Month)?.Count ?? 0,
                    //Inspections   = GetMonthly(monthlyInspections, k.Year, k.Month),
                    //ExcelExports  = GetMonthly(monthlyExports, k.Year, k.Month),
                    Logins        = monthlyLogins.FirstOrDefault(x => x.Year == k.Year && x.Month == k.Month)?.Count ?? 0,
                    UniqueLogins  = monthlyUniqueLogins.FirstOrDefault(x => x.Year == k.Year && x.Month == k.Month)?.Count ?? 0,
                });
            }

            var vm = new StatsVm
            {
                ActiveUsersToday = activeUsersToday,
                ActiveUsersThisMonth = activeUsersThisMonth,

                RegistrationsThisMonth = registrationsThisMonth,
                RegistrationsTotal = registrationsTotal,                

                LoginsThisMonth = loginsThisMonth,
                LoginsTotal = loginsTotal,

                UniqueLoginsThisMonth = uniqueLoginsThisMonth,
                UniqueLoginsTotal = uniqueLoginsTotal,

                Monthly = monthly
            };

            return View(vm);
        }
    }
}
