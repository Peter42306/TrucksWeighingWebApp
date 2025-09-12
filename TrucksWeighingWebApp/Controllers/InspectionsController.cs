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
using TrucksWeighingWebApp.ViewModels;

namespace TrucksWeighingWebApp.Controllers
{
    [Authorize]
    public class InspectionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public InspectionsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IMapper mapper)
        {
            _context = context;
            _userManager = userManager;
            _mapper = mapper;
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
        public IActionResult Create()
        {
            FillTimeZone();
            return View(new InspectionCreateViewModel());
        }

        // POST: Inspections/Create        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InspectionCreateViewModel vm, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                FillTimeZone(vm.TimeZoneId);
                return View(vm);
            }

            var inspection = _mapper.Map<Inspection>(vm);

            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Forbid();
            }
            inspection.ApplicationUserId = userId;

            inspection.CreatedAt = DateTime.UtcNow;

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

            var vm = _mapper.Map<InspectionEditViewModel>(inspection);

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

            _mapper.Map(vm, inspection);
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
