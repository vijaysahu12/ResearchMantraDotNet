using RM.Database.KingResearchContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RM.API.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class ExpiriesController : ControllerBase
    {
        private readonly KingResearchContext _context;

        public ExpiriesController(KingResearchContext context)
        {
            _context = context;
        }

        // GET: api/Expiries
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Expiry>>> GetExpirys()
        {
            return await _context.Expirys.Where(c => c.IsDelete == 0).ToListAsync();
        }

        // GET: api/Expiries/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Expiry>> GetExpiry(string id)
        {
            Expiry expiry = await _context.Expirys.FirstOrDefaultAsync(c => c.PublicKey.ToString() == id);

            return expiry == null ? (ActionResult<Expiry>)NotFound() : (ActionResult<Expiry>)expiry;
        }

        // PUT: api/Expiries/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutExpiry(string id, Expiry expiry)
        {
            if (id != expiry.PublicKey.ToString())
            {
                return BadRequest();
            }

            _context.Entry(expiry).State = EntityState.Modified;

            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Expiry> entry = _context.Entry(expiry);
            entry.Property(e => e.Id).IsModified = false;
            entry.Property(e => e.PublicKey).IsModified = false;
            entry.Property(e => e.CreatedOn).IsModified = false;
            entry.Property(e => e.CreatedBy).IsModified = false;

            try
            {
                _ = await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ExpiryExists(id))
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

        // POST: api/Expiries
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        public async Task<ActionResult<Expiry>> PostExpiry(Expiry expiry)
        {
            _ = _context.Expirys.Add(expiry);
            _ = await _context.SaveChangesAsync();

            return CreatedAtAction("GetExpiry", new { id = expiry.Id }, expiry);
        }


        private bool ExpiryExists(string id)
        {
            return _context.Expirys.Any(e => e.PublicKey.ToString() == id);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromBody] Expiry expiry)
        {
            // Validate the input object
            if (expiry == null || string.IsNullOrEmpty(expiry.PublicKey.ToString()))
            {
                return BadRequest("Invalid data provided.");
            }

            // Find the strategy by PublicKey
            var item = await _context.Expirys.FirstOrDefaultAsync(s => s.PublicKey == expiry.PublicKey);
            if (item == null)
            {
                return NotFound("Strategy not found.");
            }

            // Remove the strategy
            _context.Expirys.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent(); // Return 204 No Content for successful deletion
        }
    }
}
