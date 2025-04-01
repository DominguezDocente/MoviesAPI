using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesAPI.DTOs;
using MoviesAPI.Entities;
using MoviesAPI.Helpers;

namespace MoviesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [DisableLimitRequests]
    public class InvoicesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public InvoicesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult> Pay(PayInvoiceDTO dto)
        {
            Invoice invoice = await _context.Invoices.Include(i => i.User).FirstOrDefaultAsync(i => i.Id == dto.InvoiceId);

            if (invoice is null)
            {
                return NotFound();
            }

            if (invoice.Paid)
            {
                ModelState.AddModelError(nameof(dto.InvoiceId), "La factura ya fue saldada");
                return ValidationProblem();
            }

            // Pretender que el pago fue exitoso
            invoice.Paid = true;
            await _context.SaveChangesAsync();

            // Validar si hay mas factiras vencidas
            bool thereAreOverdueInvoices = await _context.Invoices.AnyAsync(i => i.UserId == invoice.UserId
                                                                                && !i.Paid
                                                                                && i.LimitPaymentDate < DateTime.UtcNow);

            if (!thereAreOverdueInvoices)
            {
                invoice.User!.DefaultingDebtor = false;
                await _context.SaveChangesAsync();
            }

            return NoContent();
        }
    }
}
