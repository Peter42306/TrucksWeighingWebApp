using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrucksWeighingWebApp.Data;

namespace TrucksWeighingWebApp.Controllers.Admin
{
    public class FeedbackAdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FeedbackAdminController( ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var items = await _context.FeedbackTickets
                .OrderByDescending(x => x.CreatedUtc)
                .ToListAsync();

            return View(items);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var ticket = await _context.FeedbackTickets
                .Include(u => u.ApplicationUser)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveNote(int id, string? adminNote)
        {
            var ticket = await _context.FeedbackTickets
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null)
            {
                return NotFound();
            }

            ticket.AdminNotes = string.IsNullOrWhiteSpace(adminNote) ? null : adminNote.Trim();
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id });
        }

    }
}
