using RM.Database.ResearchMantraContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RM.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentModesController : ControllerBase
    {
        private readonly ResearchMantraContext _context;

        public PaymentModesController(ResearchMantraContext context)
        {
            _context = context;
        }

        // GET: api/PaymentModes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PaymentMode>>> GetPaymentModes()
        {
            return await _context.PaymentModes.Where(c => c.IsDelete == 0).ToListAsync();
        }

        // GET: api/PaymentModes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentMode>> GetPaymentMode(string id)
        {
            PaymentMode paymentMode = await _context.PaymentModes.FirstOrDefaultAsync(c => c.PublicKey.ToString() == id);

            return paymentMode == null ? (ActionResult<PaymentMode>)NotFound() : (ActionResult<PaymentMode>)paymentMode;
        }

        // PUT: api/PaymentModes/5


        [HttpPut("{id}")]
        public async Task<IActionResult> PutPaymentMode(string id, PaymentMode paymentMode)
        {
            if (id != paymentMode.PublicKey.ToString())
            {
                return BadRequest();
            }

            _context.Entry(paymentMode).State = EntityState.Modified;
            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<PaymentMode> entry = _context.Entry(paymentMode);
            entry.Property(e => e.PublicKey).IsModified = false;
            entry.Property(e => e.CreatedOn).IsModified = false;
            entry.Property(e => e.CreatedBy).IsModified = false;

            try
            {
                _ = await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PaymentModeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/PaymentModes


        [HttpPost]
        public async Task<ActionResult<PaymentMode>> PostPaymentMode(PaymentMode paymentMode)
        {
            _ = _context.PaymentModes.Add(paymentMode);
            _ = await _context.SaveChangesAsync();

            return CreatedAtAction("GetPaymentMode", new { id = paymentMode.Id }, paymentMode);
        }



        private bool PaymentModeExists(string id)
        {
            return _context.PaymentModes.Any(e => e.PublicKey.ToString() == id);
        }
    }
}
