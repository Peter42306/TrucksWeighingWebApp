using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TrucksWeighingWebApp.Data;
using TrucksWeighingWebApp.Infrastructure.Identity;
using TrucksWeighingWebApp.Models;
using TrucksWeighingWebApp.Services.Logos;
using TrucksWeighingWebApp.ViewModels;

namespace TrucksWeighingWebApp.Controllers
{
    [Authorize]
    public class InspectionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IUserLogoService _logoService;

        public InspectionsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IMapper mapper,
            IUserLogoService logoService)
        {
            _context = context;
            _userManager = userManager;
            _mapper = mapper;
            _logoService = logoService;
        }

        // GET: Inspections
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            IQueryable<Inspection> query = _context.Inspections.AsNoTracking();

            if (User.IsInRole(RoleNames.Admin))
            {
                query = query.Include(x => x.ApplicationUser);
            }
            else
            {                
                query = query.Where(x => x.ApplicationUserId == _userManager.GetUserId(User));
            }

            //if (User.IsInRole(RoleNames.User))
            //{
            //    var currentUserId = _userManager.GetUserId(User);
            //    query = query.Where(x => x.ApplicationUserId == currentUserId);
            //}
            //else if (User.IsInRole(RoleNames.Admin))
            //{
            //    query = query.Include(x => x.ApplicationUser);
            //}


            var inspections = await query
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new InspectionIndexViewModel
                {
                    Id = x.Id,
                    ApplicationUser = x.ApplicationUser.Email,
                    Vessel = x.Vessel,
                    Cargo = x.Cargo,
                    Place = x.Place,
                    DeclaredTotalWeight = x.DeclaredTotalWeight,
                    CreatedAtUtc = x.CreatedAt,
                    TimeZoneId = x.TimeZoneId,
                    Notes = x.Notes
                })
                .ToListAsync(ct);

            var tzCache = new Dictionary<string, TimeZoneInfo>(StringComparer.OrdinalIgnoreCase);
            TimeZoneInfo GetTz(string id)
            {
                if (!tzCache.TryGetValue(id, out var tz))
                {
                    try
                    {
                        tz = TimeZoneInfo.FindSystemTimeZoneById(id);
                    }
                    catch
                    {
                        tz = TimeZoneInfo.Utc;
                    }
                    tzCache[id] = tz;
                }
                return tz;                
            }

            foreach (var item in inspections)
            {
                item.CreatedAtLocal = TimeZoneInfo.ConvertTimeFromUtc(item.CreatedAtUtc, GetTz(item.TimeZoneId));
            }

            var currentUserId = _userManager.GetUserId(User);
            if (!string.IsNullOrEmpty(currentUserId))
            {
                ViewBag.UserLogos = await _logoService.ListAsync(currentUserId, ct);
            }

            return View(inspections);
        }

        // GET: Inspections/Details/5
        public async Task<IActionResult> Details(int? id, CancellationToken ct)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inspection = await _context.Inspections
                .AsNoTracking()
                .Include(i => i.ApplicationUser)
                .Include(i => i.TruckRecords)
                .FirstOrDefaultAsync(m => m.Id == id, ct);
            if (inspection == null)
            {
                return NotFound();
            }

            return View(inspection);
        }

        // GET: Inspections/Create
        public async Task<IActionResult> Create( CancellationToken ct)
        {
            FillTimeZone();

            var vm = new InspectionCreateViewModel();
            var uid = _userManager.GetUserId(User);

            if (!string.IsNullOrEmpty(uid))
            {
                var logos = await _logoService.ListAsync(uid, ct);

                vm.LogoOptions = logos
                    .Select(l => new LogoOptionsViewModel
                    {
                        Id = l.Id,
                        Name = l.Name,
                        Height = l.Height,
                        PaddingBottom = l.PaddingBottom,
                        Position = l.Position,
                        FilePath = l.FilePath
                    }).ToList();
            }

            return View(vm);
        }

        // POST: Inspections/Create        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InspectionCreateViewModel vm, CancellationToken ct)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Forbid();
            }

            if (vm.UserLogoId.HasValue)
            {
                var logo = await _logoService.GetAsync(vm.UserLogoId.Value, userId, ct);

                if (logo == null)
                {
                    ModelState.AddModelError(nameof(vm.UserLogoId), "Invalid logo selection.");
                }
            }

            if (!ModelState.IsValid)
            {
                FillTimeZone(vm.TimeZoneId);


                var logos = await _logoService.ListAsync(userId, ct);

                vm.LogoOptions = logos
                    .Select(l => new LogoOptionsViewModel
                    {
                        Id = l.Id,
                        Name = l.Name,
                        Height= l.Height,
                        PaddingBottom= l.PaddingBottom,
                        Position = l.Position,
                        FilePath = l.FilePath
                    })
                    .ToList();

                return View(vm);
            }
                        
            var inspection = new Inspection
            {
                ApplicationUserId = userId,
                ApplicationUser = null!,
                UserLogoId = vm.UserLogoId,
                Vessel = vm.Vessel,
                Cargo = vm.Cargo,
                Place = vm.Place,
                DeclaredTotalWeight = vm.DeclaredTotalWeight,
                TimeZoneId = string.IsNullOrWhiteSpace(vm.TimeZoneId) ? "UTC" : vm.TimeZoneId,
                Notes = vm.Notes?.Trim(),
                CreatedAt = DateTime.UtcNow
            };            

            _context.Inspections.Add(inspection);
            await _context.SaveChangesAsync(ct);

            return RedirectToAction(nameof(Index));
        }

        // GET: Inspections/Edit/5
        public async Task<IActionResult> Edit(int? id, CancellationToken ct)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inspection = await _context.Inspections
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == id, ct);

            if (inspection == null)
            {
                return NotFound();
            }

            if (!await HasAccessAsync(inspection))
            {
                return NotFound();
            }

            var logoOwnerId = inspection.ApplicationUserId;
            var logos = await _logoService.ListAsync(logoOwnerId, ct);


            var vm = new InspectionEditViewModel
            {
                Id = inspection.Id,
                Vessel = inspection.Vessel,
                Cargo = inspection.Cargo,
                Place = inspection.Place,
                DeclaredTotalWeight= inspection.DeclaredTotalWeight,
                TimeZoneId = inspection.TimeZoneId,
                Notes = inspection.Notes,
                UserLogoId = inspection.UserLogoId,
                LogoOptions = logos
                    .Select(l => new LogoOptionsViewModel
                    {
                        Id = l.Id,
                        Name = l.Name,
                        Height = l.Height,
                        PaddingBottom = l.PaddingBottom,
                        Position = l.Position,
                        FilePath = l.FilePath
                    }).ToList()
            };

            FillTimeZone(vm.TimeZoneId);

            return View(vm);
        }

        // POST: Inspections/Edit/5       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(InspectionEditViewModel vm, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                var logoOwnerId = await _context.Inspections
                    .Where(i => i.Id == vm.Id)
                    .Select(i => i.ApplicationUserId)
                    .FirstOrDefaultAsync(ct);


                var logos = logoOwnerId is null 
                    ? new List<UserLogo>() 
                    : await _logoService.ListAsync(logoOwnerId, ct);

                vm.LogoOptions = logos
                    .Select(l => new LogoOptionsViewModel
                    {
                        Id = l.Id,
                        Name = l.Name,
                        Height = l.Height,
                        PaddingBottom = l.PaddingBottom,
                        Position = l.Position,
                        FilePath = l.FilePath
                    }).ToList();

                FillTimeZone(vm.TimeZoneId);

                return View(vm);
            }

            var inspection = await _context.Inspections
                .FirstOrDefaultAsync(i => i.Id == vm.Id, ct);

            if (inspection == null)
            {
                return NotFound();
            }

            if (!await HasAccessAsync(inspection))
            {
                return NotFound();
            }

            // validation that logo belongs to user
            if (vm.UserLogoId.HasValue)
            {
                var belongs = await _context.UserLogos
                    .AsNoTracking()
                    .AnyAsync(l => l.Id == vm.UserLogoId.Value && l.ApplicationUserId == inspection.ApplicationUserId, ct);

                if (!belongs)
                {
                    ModelState.AddModelError(nameof(vm.UserLogoId), "Invalid logo selelction");

                    var logos = await _context.UserLogos
                        .AsNoTracking()
                        .Where(l => l.ApplicationUserId == inspection.ApplicationUserId)
                        .OrderBy(l => l.Name)
                        .ToListAsync(ct);

                    vm.LogoOptions = logos.Select(l => new LogoOptionsViewModel
                    {
                        Id = l.Id,
                        Name = l.Name,
                        Height = l.Height,
                        PaddingBottom = l.PaddingBottom,
                        Position = l.Position,
                        FilePath = l.FilePath
                    }).ToList();

                    FillTimeZone(vm.TimeZoneId);

                    return View(vm);
                }
            }

            //_mapper.Map(vm, inspection);

            inspection.Vessel = vm.Vessel;
            inspection.Cargo = vm.Cargo;
            inspection.Place = vm.Place;
            inspection.DeclaredTotalWeight = vm.DeclaredTotalWeight;
            inspection.TimeZoneId = vm.TimeZoneId;
            inspection.Notes = vm.Notes;
            inspection.UserLogoId = vm.UserLogoId;

            await _context.SaveChangesAsync(ct);

            return RedirectToAction(nameof(Index));
        }

        // GET: Inspections/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inspection = await _context.Inspections
                .Include(i => i.ApplicationUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (inspection == null)
            {
                return NotFound();
            }

            return View(inspection);
        }

        // POST: Inspections/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var inspection = await _context.Inspections.FindAsync(id);
            if (inspection != null)
            {
                _context.Inspections.Remove(inspection);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Logo

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadLogo(
            string name,
            LogoPosition position,
            int height,
            int paddingBottom,
            IFormFile file,
            CancellationToken ct)
        {
            var uid = _userManager.GetUserId(User);
            if (uid == null)
            {
                return Forbid();
            }

            try
            {                
                await _logoService.UploadAsync(uid, file, name, height, paddingBottom, position, ct);
                TempData["Msg"] = "Logo uploaded.";
            }
            catch (Exception ex)
            {
                TempData["Err"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLogo(int id, CancellationToken ct)
        {
            var uid = _userManager?.GetUserId(User);
            if (uid == null)
            {
                return Forbid();
            }

            try
            {
                await _logoService.DeleteAsync(id, uid, ct);
                TempData["Msg"] = "Logo deleted.";
            }
            catch (Exception ex)
            {
                TempData["Err"] = ex.Message;                
            }

            return RedirectToAction(nameof(Index));
        }




        private bool InspectionExists(int id)
        {
            return _context.Inspections.Any(e => e.Id == id);
        }




        
        private async Task<bool> HasAccessAsync(Inspection inspection)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return false;
            }

            if (inspection.ApplicationUserId == user.Id || User.IsInRole(RoleNames.Admin))
            {
                return true;
            }

            return false;
        }

        
        private void FillTimeZone(string? selectedId = null)
        {
            ViewBag.TimeZones = TimeZoneInfo
                .GetSystemTimeZones()
                .Select(tz => new SelectListItem
                {
                    Value = tz.Id,
                    Text = tz.DisplayName,
                    Selected = (tz.Id == selectedId)
                })
                .ToList();
        }
    }
}
