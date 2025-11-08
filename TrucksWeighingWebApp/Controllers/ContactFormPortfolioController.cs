using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using TrucksWeighingWebApp.ViewModels;

namespace TrucksWeighingWebApp.Controllers
{
    [ApiController]
    [Route("api/contact")]    
    [AllowAnonymous]
    public class ContactFormPortfolioController : ControllerBase
    {
        private readonly IEmailSender _emailSender;

        public ContactFormPortfolioController(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        [HttpPost]
        public async Task<IActionResult> Send([FromBody] ContactFormPortfolioDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var subject = $"Portfolio Contact: {dto.Subject}";

            var body = $@"<b>From:
                                    </b> {dto.Name} ({dto.Email})<br/>
                                    <hr/>
                                    {dto.Message.Replace("\n", "<br/>")}";

            await _emailSender.SendEmailAsync(
                "pzalizko@gmail.com", 
                subject, 
                body + $"<br/><hr/><small>Reply-To: {dto.Email}</small>"
                );

            return Ok(new { success = true });
        }
    }
}
