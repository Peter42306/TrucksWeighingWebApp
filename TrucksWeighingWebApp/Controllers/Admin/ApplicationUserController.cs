using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using TrucksWeighingWebApp.Data;
using TrucksWeighingWebApp.Infrastructure.Identity;
using TrucksWeighingWebApp.Models;

namespace TrucksWeighingWebApp.Controllers.Admin
{
    [Authorize(Roles = RoleNames.Admin)]
    [Route("admin/users")]
    public class ApplicationUserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;

        public ApplicationUserController(UserManager<ApplicationUser> userManager, ApplicationDbContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var now = DateTime.UtcNow;
            var onlineThreshold = now.AddMinutes(-5);

            // users and their last activity
            var baseQuery = _db.Users
                .AsNoTracking()
                .Select(u => new
                {
                    u.Id,
                    u.Email,
                    u.IsActive,
                    u.CreatedAt,
                    u.LoginCount,
                    u.AdminNote,

                    LastSeenUtc = _db.UserSessions
                        .Where(s => s.UserId == u.Id)
                        .Select(s => (DateTime?)s.LastSeenUtc)
                        .Max(),

                    IsOnline = _db.UserSessions
                        .Where(s => s.UserId == u.Id && s.LastSeenUtc >= onlineThreshold)
                        .Any(),

                    LastStartedUtc = _db.UserSessions
                        .Where(s => s.UserId == u.Id)
                        .OrderByDescending(s => s.LastSeenUtc)
                        .Select(s => (DateTime?)s.StartedUtc)
                        .FirstOrDefault(),

                    LastIp = _db.UserSessions
                        .Where(s => s.UserId == u.Id)
                        .OrderByDescending(s => s.LastSeenUtc)
                        .Select(s => s.Ip)
                        .FirstOrDefault(),

                    LastUserAgent = _db.UserSessions
                        .Where(s => s.UserId == u.Id)
                        .OrderByDescending(s => s.LastSeenUtc)
                        .Select(s => s.UserAgent)
                        .FirstOrDefault()
                });

            var model = await baseQuery
                .OrderByDescending(x => x.IsOnline)
                .ThenByDescending(x => x.LastSeenUtc)
                .ThenByDescending(x => x.LoginCount)
                .Select(x => new UserRow(
                    x.Id,
                    x.Email,
                    x.IsActive,
                    x.CreatedAt,                    
                    x.LastSeenUtc,
                    x.LoginCount,
                    x.IsOnline,
                    x.AdminNote,
                    x.LastStartedUtc,
                    x.LastIp,
                    x.LastUserAgent))
                .ToListAsync();

            return View(model);
        }

        [HttpPost("update")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus()
        {
            var id = Request.Form["id"].ToString();
            if (string.IsNullOrWhiteSpace(id))
            {
                TempData["Error"] = "User ID is required";
                return RedirectToAction(nameof(Index));
            }

            var isActive = Request.Form["isActive"].Contains("true");
            var adminNote = Request.Form["adminNote"].ToString();

            var currentUser = await _userManager.GetUserAsync(User);
            if (id == currentUser?.Id && !isActive)
            {
                TempData["Error"] = "You cannot deactivate your own admin account";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["Error"] = "User not found";
                return RedirectToAction(nameof(Index));
            }

            user.IsActive = isActive;
            user.AdminNote = adminNote;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                TempData["Error"] = string.Join(", ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction(nameof(Index));
        }


        public class UserRow
        {
            public string Id { get; set; }
            public string? Email { get; set; }
            public bool IsActive { get; set; }
            public DateTime CreatedAtUtc { get; set; }
            public DateTime? LastSeenUtc { get; set; }
            public int LoginCount { get; set; }
            public bool IsOnline { get; set; }
            public string? AdminNote { get; set; }
            public DateTime? LastStartedUtc { get; set; }
            public string? LastIp { get; set; }
            public string? LastUserAgent { get; set; }

            public UserRow(
                string id,
                string? email,
                bool isActive,
                DateTime createdAtUtc,
                DateTime? lastSeenUtc,
                int loginCount,
                bool isOnline,
                string? adminNote,
                DateTime? lastStartedUtc,
                string? lastIp,
                string? lastUserAgent)
            {
                Id = id;
                Email = email;
                IsActive = isActive;
                CreatedAtUtc = createdAtUtc;
                LastSeenUtc = lastSeenUtc;
                LoginCount = loginCount;
                IsOnline = isOnline;
                AdminNote = adminNote;
                LastStartedUtc = lastStartedUtc;
                LastIp = lastIp;
                LastUserAgent = lastUserAgent;
            }
        }
    }    
}
