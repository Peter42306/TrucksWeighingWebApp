using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TrucksWeighingWebApp.Data;
using TrucksWeighingWebApp.Models;

namespace TrucksWeighingWebApp.Controllers
{
    public class TruckRecordsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TruckRecordsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TruckRecords
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.TruckRecords.Include(t => t.Inspection);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: TruckRecords/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var truckRecord = await _context.TruckRecords
                .Include(t => t.Inspection)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (truckRecord == null)
            {
                return NotFound();
            }

            return View(truckRecord);
        }

        // GET: TruckRecords/Create
        public IActionResult Create()
        {
            ViewData["InspectionId"] = new SelectList(_context.Inspections, "Id", "Id");
            return View();
        }

        // POST: TruckRecords/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,InspectionId,PlateNumber,InitialWeightAtUtc,InitialWeight,FinalWeightAtUtc,FinalWeight,NetWeight")] TruckRecord truckRecord)
        {
            if (ModelState.IsValid)
            {
                _context.Add(truckRecord);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["InspectionId"] = new SelectList(_context.Inspections, "Id", "Id", truckRecord.InspectionId);
            return View(truckRecord);
        }

        // GET: TruckRecords/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var truckRecord = await _context.TruckRecords.FindAsync(id);
            if (truckRecord == null)
            {
                return NotFound();
            }
            ViewData["InspectionId"] = new SelectList(_context.Inspections, "Id", "Id", truckRecord.InspectionId);
            return View(truckRecord);
        }

        // POST: TruckRecords/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,InspectionId,PlateNumber,InitialWeightAtUtc,InitialWeight,FinalWeightAtUtc,FinalWeight,NetWeight")] TruckRecord truckRecord)
        {
            if (id != truckRecord.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(truckRecord);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TruckRecordExists(truckRecord.Id))
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
            ViewData["InspectionId"] = new SelectList(_context.Inspections, "Id", "Id", truckRecord.InspectionId);
            return View(truckRecord);
        }

        // GET: TruckRecords/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var truckRecord = await _context.TruckRecords
                .Include(t => t.Inspection)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (truckRecord == null)
            {
                return NotFound();
            }

            return View(truckRecord);
        }

        // POST: TruckRecords/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var truckRecord = await _context.TruckRecords.FindAsync(id);
            if (truckRecord != null)
            {
                _context.TruckRecords.Remove(truckRecord);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TruckRecordExists(int id)
        {
            return _context.TruckRecords.Any(e => e.Id == id);
        }
    }
}
