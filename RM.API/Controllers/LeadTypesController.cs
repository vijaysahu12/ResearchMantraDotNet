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
    public class LeadTypesController : ControllerBase
    {
        private readonly ResearchMantraContext _context;

        public LeadTypesController(ResearchMantraContext context)
        {
            _context = context;
        }

        // GET: api/LeadTypes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LeadType>>> GetLeadTypes()
        {
            return await _context.LeadTypes.Where(c => c.IsDelete == 0).ToListAsync();
        }

        // GET: api/LeadTypes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<LeadType>> GetLeadType(string id)
        {
            LeadType leadType = await _context.LeadTypes.FirstOrDefaultAsync(c => c.PublicKey.ToString() == id);

            return leadType == null ? (ActionResult<LeadType>)NotFound() : (ActionResult<LeadType>)leadType;
        }

        // PUT: api/LeadTypes/5


        [HttpPut("{id}")]
        public async Task<IActionResult> PutLeadType(string id, LeadType leadType)
        {
            if (id != leadType.PublicKey.ToString())
            {
                return BadRequest();
            }

            _context.Entry(leadType).State = EntityState.Modified;
            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<LeadType> entry = _context.Entry(leadType);
            entry.Property(e => e.PublicKey).IsModified = false;
            entry.Property(e => e.CreatedOn).IsModified = false;
            entry.Property(e => e.CreatedBy).IsModified = false;

            try
            {
                _ = await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LeadTypeExists(id))
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

        // POST: api/LeadTypes


        [HttpPost]
        public async Task<ActionResult<LeadType>> PostLeadType(LeadType leadType)
        {
            _ = _context.LeadTypes.Add(leadType);
            _ = await _context.SaveChangesAsync();

            return CreatedAtAction("GetLeadType", new { id = leadType.Id }, leadType);
        }


        private bool LeadTypeExists(string id)
        {
            return _context.LeadTypes.Any(e => e.PublicKey.ToString() == id);
        }
    }
}
