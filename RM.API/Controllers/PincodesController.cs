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
    public class PincodesController : ControllerBase
    {
        private readonly ResearchMantraContext _context;

        public PincodesController(ResearchMantraContext context)
        {
            _context = context;
        }

        // GET: api/Pincodes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Pincode>>> GetPincodes()
        {
            return await _context.Pincodes.Where(c => c.IsDelete == 0).ToListAsync();
        }

        // GET: api/Pincodes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Pincode>> GetPincode(string id)
        {
            Pincode pincode = await _context.Pincodes.FirstOrDefaultAsync(c => c.PublicKey.ToString() == id);

            return pincode == null ? (ActionResult<Pincode>)NotFound() : (ActionResult<Pincode>)pincode;
        }

        [AllowAnonymous]
        // GET: api/Pincodes/5
        [HttpGet("PullPinCodes{id}")]
        public async Task<ActionResult> PullPinCodes(string id)
        {
            List<Pincode> pincode = await _context.Pincodes.Where(c => c.Pincode1 == id).ToListAsync();

            return pincode == null ? NotFound() : Ok(pincode);
        }

        // PUT: api/Pincodes/5


        [HttpPut("{id}")]
        public async Task<IActionResult> PutPincode(string id, Pincode pincode)
        {
            if (id != pincode.PublicKey.ToString())
            {
                return BadRequest();
            }

            _context.Entry(pincode).State = EntityState.Modified;
            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Pincode> entry = _context.Entry(pincode);
            entry.Property(e => e.PublicKey).IsModified = false;
            entry.Property(e => e.CreatedOn).IsModified = false;
            entry.Property(e => e.CreatedBy).IsModified = false;

            try
            {
                _ = await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PincodeExists(id))
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

        // POST: api/Pincodes


        [HttpPost]
        public async Task<ActionResult<Pincode>> PostPincode(Pincode pincode)
        {
            _ = _context.Pincodes.Add(pincode);
            _ = await _context.SaveChangesAsync();

            return CreatedAtAction("GetPincode", new { id = pincode.Id }, pincode);
        }


        private bool PincodeExists(string id)
        {
            return _context.Pincodes.Any(e => e.PublicKey.ToString() == id);
        }
    }
}
