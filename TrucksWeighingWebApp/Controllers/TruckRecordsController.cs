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
        public async Task<IActionResult> PlateHints(int inspectionId, string q, int take = 20, CancellationToken ct = default)
        {
            q = (q ?? "").Trim();
            if (q.Length==0)
            {
                return Json(Array.Empty<string>());
            }

            var query = _context.TruckRecords.AsNoTracking();

            var pattern = "%" + q + "%";

            var items = await query
                .Where(x => 
                    x.InspectionId == inspectionId &&
                    !string.IsNullOrWhiteSpace(x.PlateNumber) && 
                    EF.Functions.ILike(x.PlateNumber!,pattern))
                .Select(x => x.PlateNumber!)
                .Distinct()
                .OrderBy(x => x)
                .Take(take)
                .ToListAsync(ct);

            return Json(items);
        }

        // GET: TruckRecords
        [HttpGet]
        public async Task<IActionResult> Index(
            int inspectionId, 
            int? editId, 
            SortOrder sortOrder = SortOrder.Descending, 
            int page = 1, 
            int pageSize = PageSizes.Default, // 10, 50, int.MaxValue
            [FromQuery(Name = "weighRange")] WeighRangeFilterViewModel? weighRange = null,
            CancellationToken ct=default)
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

            // basic query
            var query = _context.TruckRecords
                .AsNoTracking()
                .Where(i => i.InspectionId == inspectionId);


            // validation of WeighRangeFilterViewModel
            TryValidateModel(weighRange);
            if (ModelState.IsValid)
            {

            }


            var totalTrucks = await query.CountAsync(ct);


            // sorting
            query = (sortOrder == SortOrder.Descending)
                ? query.OrderByDescending(x => x.Id)
                : query.OrderBy(x => x.Id);



            // pagination


            var effectivePageSize = (pageSize <= 0) ? PageSizes.Default : pageSize;

            List<TruckRecord> rows;

            if (effectivePageSize == PageSizes.All)
            {
                page = 1;
                rows = await query.ToListAsync(ct);
            }
            else
            {
                var totalPages = Math.Max(1, (int)Math.Ceiling((double)totalTrucks / effectivePageSize));
                
                page = Math.Clamp(page, 1, totalPages);                

                rows = await query
                    .Skip((page -1) * effectivePageSize)
                    .Take(effectivePageSize)
                    .ToListAsync(ct);                
            }

            


            // new row in table
            var vm = new TruckRecordIndexViewModel
            {
                Inspection=inspection,
                New = new TruckRecordCreateViewModel { InspectionId = inspection.Id, PlateNumber = string.Empty },
                SortOrder = sortOrder,
                TruckRecords = rows,

                Page = page,
                PageSize = effectivePageSize,
                TotalCount = totalTrucks
            };

            

            // edit row in table
            if (editId.HasValue)
            {
                var editRow = inspection.TruckRecords.FirstOrDefault(x => x.Id == editId.Value);
                if (editRow != null)
                {
                    DateTime? ConvertToLocalTime(DateTime? utc)
                    {
                        if (!utc.HasValue)
                        {
                            return null;
                        }

                        var tz = TimeZoneInfo.FindSystemTimeZoneById(inspection.TimeZoneId);
                        return TimeZoneInfo.ConvertTimeFromUtc(utc.Value, tz);
                    }

                    vm.Edit = new TruckRecordEditViewModel
                    {
                        Id = editRow.Id,
                        InspectionId = inspection.Id,                        
                        SerialNumber = editRow.SerialNumber,
                        PlateNumber = editRow.PlateNumber,
                        InitialWeight = editRow.InitialWeight,
                        InitialWeightAtUtc = ConvertToLocalTime(editRow.InitialWeightAtUtc),
                        FinalWeight = editRow.FinalWeight,
                        FinalWeightAtUtc = ConvertToLocalTime(editRow.FinalWeightAtUtc)
                    };
                }
            }

            return View(vm);
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(
            int id, int inspectionId, 
            SortOrder sortOrder, int page, int pageSize,
            CancellationToken ct)
        {
            var record = await _context.TruckRecords
                .Include(x => x.Inspection)
                .FirstOrDefaultAsync(x => x.Id == id && x.InspectionId == inspectionId, ct);

            if (record == null)
            {
                TempData["DeleteWarning"] = "This truck was already removed by someone else (Truck list was not renewed before deleting record).";
                return RedirectToAction(nameof(Index), new { inspectionId });
                //return NotFound();
            }

            if (!await HasAccessAsync(record.Inspection))
            {
                return NotFound();
            }

            using var tx = await _context.Database.BeginTransactionAsync(ct);
            try
            {
                await _context.Database.ExecuteSqlInterpolatedAsync(
                    $"SELECT 1 FROM \"Inspections\" WHERE \"Id\" = {inspectionId} FOR UPDATE",
                    ct);

                _context.TruckRecords.Remove(record);
                await _context.SaveChangesAsync(ct);

                await _context.Database.ExecuteSqlInterpolatedAsync(
                    $"UPDATE \"TruckRecords\" SET \"SerialNumber\" = \"SerialNumber\" - 1 WHERE \"InspectionId\" = {inspectionId} AND \"SerialNumber\" > {record.SerialNumber}"
                    , ct);
                
                await tx.CommitAsync();
            }
            catch (Exception)
            {
                await tx.RollbackAsync();
                throw;
            }

            

            return RedirectToAction(nameof(Index), new { inspectionId = record.InspectionId, sortOrder, page, pageSize });
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            TruckRecordCreateViewModel vm,
            SortOrder sortOrder, int page, int pageSize,
            CancellationToken ct)
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
                //return RedirectToAction(nameof(Index), new { inspectionId = vm.InspectionId });
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

            using var tx = await _context.Database.BeginTransactionAsync(ct);

            try
            {
                await _context.Database.ExecuteSqlInterpolatedAsync(
                    $"SELECT 1 FROM \"Inspections\" WHERE \"Id\" = {vm.InspectionId} FOR UPDATE",
                    ct);

                var existingMaxSerialNumber = await _context.TruckRecords
                    .Where(t => t.InspectionId == vm.InspectionId)
                    .Select(t => (int?)t.SerialNumber)
                    .MaxAsync(ct) ?? 0;
                var serialNumber = existingMaxSerialNumber + 1;

                var entity = new TruckRecord
                {
                    Inspection = inspection,
                    SerialNumber = serialNumber,
                    PlateNumber = (vm.PlateNumber ?? string.Empty).Trim().ToUpperInvariant().Replace(" ", ""),
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

                await tx.CommitAsync(ct);
            }
            catch
            {
                await tx.RollbackAsync(ct);                
                TempData["error"] = "Cannot create record. Try again.";
                return RedirectToAction(nameof(Index), new { inspectionId = vm.InspectionId });
            }

            return RedirectToAction(nameof(Index), new { inspectionId = vm.InspectionId, sortOrder, page, pageSize });
        }

        //[HttpGet]
        //public async Task<IActionResult>Edit(int id, CancellationToken ct)
        //{
        //    var truckRecord = await _context.TruckRecords
        //        .AsNoTracking()
        //        .FirstOrDefaultAsync(x => x.Id == id);

        //    if (truckRecord is null)
        //    {
        //        return NotFound();
        //    }

        //    var vm = new TruckRecordEditViewModel
        //    {
        //        Id = truckRecord.Id,
        //        InspectionId = truckRecord.InspectionId,
        //        SerialNumber = truckRecord.SerialNumber,
        //        PlateNumber = truckRecord.PlateNumber,
        //        InitialWeightAtUtc = truckRecord.InitialWeightAtUtc,
        //        InitialWeight = truckRecord.InitialWeight,
        //        FinalWeightAtUtc = truckRecord.FinalWeightAtUtc,
        //        FinalWeight = truckRecord.FinalWeight
        //    };

        //    return View(vm);
        //}


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            TruckRecordEditViewModel vm,
            SortOrder sortOrder, int page, int pageSize,
            CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
                //return RedirectToAction(nameof(Index), new { inspectionId = vm.InspectionId });
            }

            var editRow = await _context.TruckRecords
                .Include(x => x.Inspection)
                .FirstOrDefaultAsync(x => x.Id == vm.Id, ct);

            if (editRow == null)
            {
                return NotFound();
            }
                        
            editRow.PlateNumber = (vm.PlateNumber ?? string.Empty).Trim().ToUpperInvariant().Replace(" ", "");
            editRow.InitialWeight = vm.InitialWeight;
            editRow.FinalWeight = vm.FinalWeight;



            if (vm.InitialWeight.HasValue)
            {
                if (vm.InitialWeightAtUtc.HasValue)
                {
                    editRow.InitialWeightAtUtc = ToUtc(vm.InitialWeightAtUtc.Value, editRow.Inspection.TimeZoneId);
                }
                else
                {
                    editRow.InitialWeightAtUtc = DateTime.UtcNow;
                }
            }
            else
            {
                editRow.InitialWeightAtUtc = null;
            }

            if (vm.FinalWeight.HasValue)
            {
                if (vm.FinalWeightAtUtc.HasValue)
                {
                    editRow.FinalWeightAtUtc = ToUtc(vm.FinalWeightAtUtc.Value, editRow.Inspection.TimeZoneId);
                }
                else
                {
                    editRow.FinalWeightAtUtc = DateTime.UtcNow;
                }
            }
            else
            {
                editRow.FinalWeightAtUtc = null;
            }

            await _context.SaveChangesAsync(ct);

            return RedirectToAction(nameof(Index), new { inspectionId = vm.InspectionId, sortOrder, page, pageSize });
        }




        [HttpGet]
        public async Task<IActionResult>Status(int inspectionId, CancellationToken ct)
        {
            var vm = await TruckRecordPierIndexViewModel.CreateAsync(_context, inspectionId, ct);
            return View(vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartCargoOperations(int id, CancellationToken ct)
        {
            var t = await _context.TruckRecords.FindAsync(new object[] { id }, ct);
            if (t == null) return NotFound();

            if (t.InitialWeightAtUtc is null)
            {
                TempData["msgStartCargoOperations"] = "Initial weighing is required";
                return RedirectToAction(nameof(Status), new { inspectionId = t.InspectionId });
            }

            if (t.InitialBerthAtUtc is null)
            {
                t.InitialBerthAtUtc = DateTime.UtcNow;
                await _context.SaveChangesAsync(ct);
                TempData["msgStartCargoOperations"] = $"#{t.SerialNumber} - {t.PlateNumber}: Cargo Operations started";
            }
            else
            {
                TempData["msgStartCargoOperations"] = $"#{t.SerialNumber} - {t.PlateNumber}: already saved as under Cargo Operations";
            }

            return RedirectToAction(nameof(Status), new { inspectionId = t.InspectionId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteCargoOperations(int id, CancellationToken ct)
        {
            var t = await _context.TruckRecords.FindAsync(new object[] { id }, ct);
            if (t == null) return NotFound();

            if (t.InitialBerthAtUtc is null)
            {
                TempData["msgCompleteCargoOperations"] = "Cargo Operations should be started";
                return RedirectToAction(nameof(Status), new { inspectionId = t.InspectionId });
            }

            if (t.FinalBerthAtUtc is null)
            {
                t.FinalBerthAtUtc= DateTime.UtcNow;
                await _context.SaveChangesAsync(ct);
                TempData["msgCompleteCargoOperations"] = $"#{t.SerialNumber} - {t.PlateNumber}: Cargo Operations completed";
            }
            else
            {
                TempData["msgCompleteCargoOperations"] = $"#{t.SerialNumber} - {t.PlateNumber}: already saved as completed Cargo Operations";
            }

            return RedirectToAction(nameof(Status), new { inspectionId = t.InspectionId });

        }




        /// =================================================================================================

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

        
        private static DateTime ToUtc(DateTime localUnspecified, string timeZoneId)
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            var local = DateTime.SpecifyKind(localUnspecified, DateTimeKind.Unspecified);
            return TimeZoneInfo.ConvertTimeToUtc(local, tz);
        }

        
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
