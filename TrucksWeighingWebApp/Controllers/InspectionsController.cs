using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TrucksWeighingWebApp.Data;
using TrucksWeighingWebApp.Models;

namespace TrucksWeighingWebApp.Controllers
{
    //[Authorize(Roles = "Admin, Inspector")]
    public class InspectionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public InspectionsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Inspections
        public async Task<IActionResult> Index()
        {
            IQueryable<Inspection> query = _context.Inspections
                .Include(i => i.Inspector)
                .OrderByDescending(i => i.Id);

            if (!User.IsInRole("Admin"))
            {
                var userId = _userManager.GetUserId(User);
                query = query.Where(i => i.InspectorId == userId);
            }

            var inspections = await query
                .AsNoTracking()
                .ToListAsync();
            
            return View(inspections);
        }

        // GET: Inspections/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inspection = await _context.Inspections
                .Include(i => i.Inspector)
                .Include(i => i.TruckRecords.OrderBy(tr => tr.Id))
                .FirstOrDefaultAsync(m => m.Id == id);

            if (inspection == null)
            {
                return NotFound();
            }

            return View(inspection);
        }

        // GET: Inspections/Create
        public IActionResult Create()
        {            
            return View();
        }

        // POST: Inspections/Create        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,InspectorId,Vessel,Cargo,Place,DeclaredTotalWeight,CreatedAt,TimeZoneId,WeighedTotalWeight,DifferenceWeight,DifferencePercent")] Inspection inspection)
        {
            if (!ModelState.IsValid)
            {
                return View(inspection);
            }            

            inspection.InspectorId = _userManager.GetUserId(User);
            inspection.CreatedAt = DateTime.UtcNow;
            inspection.WeighedTotalWeight = 0;
            inspection.DifferenceWeight = 0;
            inspection.DifferencePercent = 0;

            _context.Add(inspection);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = inspection.Id });
        }

        // GET: Inspections/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inspection = await _context.Inspections.FindAsync(id);
            if (inspection == null)
            {
                return NotFound();
            }
            ViewData["InspectorId"] = new SelectList(_context.Users, "Id", "Id", inspection.InspectorId);
            return View(inspection);
        }

        // POST: Inspections/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,InspectorId,Vessel,Cargo,Place,DeclaredTotalWeight,CreatedAt,TimeZoneId,WeighedTotalWeight,DifferenceWeight,DifferencePercent")] Inspection inspection)
        {
            if (id != inspection.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(inspection);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InspectionExists(inspection.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["InspectorId"] = new SelectList(_context.Users, "Id", "Id", inspection.InspectorId);
            return View(inspection);
        }

        // GET: Inspections/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inspection = await _context.Inspections
                .Include(i => i.Inspector)
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
    }
}
