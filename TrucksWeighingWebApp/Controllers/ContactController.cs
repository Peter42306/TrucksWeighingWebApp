using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TrucksWeighingWebApp.Data;
using TrucksWeighingWebApp.Models;
using TrucksWeighingWebApp.ViewModels;

namespace TrucksWeighingWebApp.Controllers
{    
    public class ContactController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ContactController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new FeedbackTicketViewModel());
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendFeedback(FeedbackTicketViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", vm);
            }

            var msg = vm.Message.Trim();
            if (string.IsNullOrWhiteSpace(msg) || msg.Length < 5)
            {
                ModelState.AddModelError(nameof(vm.Message), "Please enter at least 5 characters.");
                return View("Index", vm);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                return NotFound();
            }

            var email = await _userManager.GetEmailAsync(user) ?? "(no email)";

            var ticket = new FeedbackTicket
            {
                ApplicationUserId = user.Id,
                ApplicationUser = user,
                UserEmail = email,
                Message = msg
            };

            _context.FeedbackTickets.Add(ticket);
            await _context.SaveChangesAsync();

            TempData["FeedbackSent"] = "Thank you! Your message has been sent.";
            return RedirectToAction(nameof(Index));
        }
    }
}
