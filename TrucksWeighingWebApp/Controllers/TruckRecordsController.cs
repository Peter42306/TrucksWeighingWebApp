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
    public class TruckRecordsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public TruckRecordsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IMapper mapper)
        {
            _context = context;
            _userManager = userManager;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> PlateHints(string q, int take = 20, CancellationToken ct = default)
        {
            q ??= "";
            var items = await _context.TruckRecords
                .AsNoTracking()
                .Where(x => x.PlateNumber.StartsWith(q))
                .Select(x => x.PlateNumber)
                .Distinct()
                .OrderBy(x => x)
                .Take(take)
                .ToListAsync(ct);

            return Json(items);
        }

        // GET: TruckRecords
        [HttpGet]
        public async Task<IActionResult> Index(int inspectionId, CancellationToken ct)
        {
            var inspection = await _context.Inspections
                .AsNoTracking()
                .Include(i => i.TruckRecords)
                .FirstOrDefaultAsync(i => i.Id == inspectionId, ct);

            if (inspection == null)
            {
                return NotFound();
            }

            if (!await HasAccessAsync(inspection))
            {
                return NotFound();
            }

            var vm = new TruckRecordIndexViewModel
            {
                Inspection=inspection,
                New = new TruckRecordCreateViewModel
                {
                    InspectionId = inspection.Id,
                    PlateNumber = string.Empty
                }
            };

            return View(vm);
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int inspectionId, CancellationToken ct)
        {
            var record = await _context.TruckRecords
                .Include(x => x.Inspection)
                .FirstOrDefaultAsync(x => x.Id == id, ct);

            if (record == null)
            {
                return NotFound();
            }

            if (!await HasAccessAsync(record.Inspection))
            {
                return NotFound();
            }

            _context.TruckRecords.Remove(record);
            await _context.SaveChangesAsync(ct);

            return RedirectToAction(nameof(Index), new { inspectionId = record.InspectionId });
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TruckRecordCreateViewModel vm, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                var inspectionNotValid = await _context.Inspections
                    .AsNoTracking()
                    .Include(i => i.TruckRecords)
                    .FirstOrDefaultAsync(i => i.Id == vm.InspectionId, ct);

                if (inspectionNotValid == null)
                {
                    return NotFound();
                }

                if (!await HasAccessAsync(inspectionNotValid))
                {
                    return NotFound();
                }

                ViewData["Title"]=inspectionNotValid.Vessel;
                ViewBag.TimeZone = TimeZoneInfo.FindSystemTimeZoneById(inspectionNotValid.TimeZoneId);

                var indexViewModel = new TruckRecordIndexViewModel
                {
                    Inspection=inspectionNotValid,
                    New = vm
                };

                return View(nameof(Index), indexViewModel);
            }

            var inspection = await _context.Inspections
                .FirstOrDefaultAsync(i => i.Id == vm.InspectionId, ct);

            if (inspection == null)
            {
                return NotFound();
            }

            if (!await HasAccessAsync(inspection))
            {
                return NotFound();
            }

            var entity = new TruckRecord
            {
                Inspection = inspection,                
                PlateNumber = vm.PlateNumber,
                InitialWeight = vm.InitialWeight,
                FinalWeight = vm.FinalWeight
            };

            if (vm.InitialWeight.HasValue)
            {
                if (vm.InitialWeightAtUtc.HasValue)
                {
                    entity.InitialWeightAtUtc = ToUtc(vm.InitialWeightAtUtc.Value, inspection.TimeZoneId);
                }
                else
                {
                    entity.InitialWeightAtUtc = DateTime.UtcNow;
                }                
            }

            if (vm.FinalWeight.HasValue)
            {
                if (vm.FinalWeightAtUtc.HasValue)
                {
                    entity.FinalWeightAtUtc = ToUtc(vm.FinalWeightAtUtc.Value, inspection.TimeZoneId);
                }
                else
                {
                    entity.FinalWeightAtUtc = DateTime.UtcNow;
                }
            }

            _context.TruckRecords.Add(entity);
            await _context.SaveChangesAsync(ct);
            
            return RedirectToAction(nameof(Index), new { inspectionId = vm.InspectionId });
        }











        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateInline(TruckRecordCreateViewModel vm, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                Response.StatusCode = 400;
                return Content("Validation error");
            }

            var inspection = await _context.Inspections.FindAsync(new object?[] { vm.InspectionId }, ct);
            if (inspection == null)
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);
            if (User.IsInRole(RoleNames.User) && inspection.ApplicationUserId != currentUserId)
            {
                return NotFound();
            }

            var truckRecord = _mapper.Map<TruckRecord>(vm);

            if (vm.InitialWeight.HasValue)
            {
                if (vm.InitialWeightAtUtc.HasValue)
                {
                    truckRecord.InitialWeightAtUtc = ToUtc(vm.InitialWeightAtUtc.Value, inspection.TimeZoneId);
                }
                else
                {
                    truckRecord.InitialWeightAtUtc = DateTime.UtcNow;
                }
            }

            if (vm.FinalWeight.HasValue)
            {
                if (vm.FinalWeightAtUtc.HasValue)
                {
                    truckRecord.FinalWeightAtUtc = ToUtc(vm.FinalWeightAtUtc.Value, inspection.TimeZoneId);
                }
                else
                {
                    truckRecord.FinalWeightAtUtc = DateTime.UtcNow;
                }
            }

            _context.TruckRecords.Add(truckRecord);
            await _context.SaveChangesAsync();

            return PartialView("_TruckRecordRow", truckRecord);


        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="localUnspecified"></param>
        /// <param name="timeZoneId"></param>
        /// <returns></returns>
        private static DateTime ToUtc(DateTime localUnspecified, string timeZoneId)
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            var local = DateTime.SpecifyKind(localUnspecified, DateTimeKind.Unspecified);
            return TimeZoneInfo.ConvertTimeToUtc(local, tz);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="utc"></param>
        /// <param name="timeZoneId"></param>
        /// <returns></returns>
        private static DateTime FromUtc(DateTime utc, string timeZoneId)
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return TimeZoneInfo.ConvertTimeFromUtc(utc, tz);
        }


        




       


        //// GET: TruckRecords/Details/5
        //public async Task<IActionResult> Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var truckRecord = await _context.TruckRecords
        //        .Include(t => t.Inspection)
        //        .FirstOrDefaultAsync(m => m.Id == id);
        //    if (truckRecord == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(truckRecord);
        //}

        //// GET: TruckRecords/Create
        //public IActionResult Create()
        //{
        //    ViewData["InspectionId"] = new SelectList(_context.Inspections, "Id", "Id");
        //    return View();
        //}

        //// POST: TruckRecords/Create
        //// To protect from overposting attacks, enable the specific properties you want to bind to.
        //// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("Id,InspectionId,PlateNumber,InitialWeightAtUtc,InitialWeight,FinalWeightAtUtc,FinalWeight")] TruckRecord truckRecord)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _context.Add(truckRecord);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    ViewData["InspectionId"] = new SelectList(_context.Inspections, "Id", "Id", truckRecord.InspectionId);
        //    return View(truckRecord);
        //}

        //// GET: TruckRecords/Edit/5
        //public async Task<IActionResult> Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var truckRecord = await _context.TruckRecords.FindAsync(id);
        //    if (truckRecord == null)
        //    {
        //        return NotFound();
        //    }
        //    ViewData["InspectionId"] = new SelectList(_context.Inspections, "Id", "Id", truckRecord.InspectionId);
        //    return View(truckRecord);
        //}

        //// POST: TruckRecords/Edit/5
        //// To protect from overposting attacks, enable the specific properties you want to bind to.
        //// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, [Bind("Id,InspectionId,PlateNumber,InitialWeightAtUtc,InitialWeight,FinalWeightAtUtc,FinalWeight")] TruckRecord truckRecord)
        //{
        //    if (id != truckRecord.Id)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _context.Update(truckRecord);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!TruckRecordExists(truckRecord.Id))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }
        //    ViewData["InspectionId"] = new SelectList(_context.Inspections, "Id", "Id", truckRecord.InspectionId);
        //    return View(truckRecord);
        //}

        //// GET: TruckRecords/Delete/5
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var truckRecord = await _context.TruckRecords
        //        .Include(t => t.Inspection)
        //        .FirstOrDefaultAsync(m => m.Id == id);
        //    if (truckRecord == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(truckRecord);
        //}

        //// POST: TruckRecords/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    var truckRecord = await _context.TruckRecords.FindAsync(id);
        //    if (truckRecord != null)
        //    {
        //        _context.TruckRecords.Remove(truckRecord);
        //    }

        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}

        //private bool TruckRecordExists(int id)
        //{
        //    return _context.TruckRecords.Any(e => e.Id == id);
        //}






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
    }
}
