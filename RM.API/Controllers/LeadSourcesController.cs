using RM.Database.KingResearchContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RM.API.Controllers
{
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class LeadSourcesController : ControllerBase
    {
        private readonly KingResearchContext _context;

        public LeadSourcesController(KingResearchContext context)
        {
            _context = context;
        }

        // GET: api/LeadSources
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LeadSource>>> GetLeadSources()
        {
            return await _context.LeadSources.Where(c => c.IsDelete == 0).ToListAsync();
        }

        // GET: api/LeadSources/all
        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<LeadSource>>> GetAllLeadSources()
        {
            return await _context.LeadSources.ToListAsync();
        }
        // GET: api/LeadSources/5
        [HttpGet("{id}")]
        public async Task<ActionResult<LeadSource>> GetLeadSource(string id)
        {
            LeadSource leadSource = await _context.LeadSources.FirstOrDefaultAsync(c => c.PublicKey.ToString() == id);

            return leadSource == null ? (ActionResult<LeadSource>)NotFound() : (ActionResult<LeadSource>)leadSource;
        }

        // PUT: api/LeadSources/5


        [HttpPut("{id}")]
        public async Task<IActionResult> PutLeadSource(string id, LeadSource leadSource)
        {
            if (id != leadSource.PublicKey.ToString())
            {
                return BadRequest();
            }

            _context.Entry(leadSource).State = EntityState.Modified;
            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<LeadSource> entry = _context.Entry(leadSource);
            entry.Property(e => e.PublicKey).IsModified = false;
            entry.Property(e => e.CreatedOn).IsModified = false;
            entry.Property(e => e.CreatedBy).IsModified = false;

            try
            {
                _ = await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LeadSourceExists(id))
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

        // POST: api/LeadSources


        [HttpPost]
        public async Task<ActionResult<LeadSource>> PostLeadSource(LeadSource leadSource)
        {
            _ = _context.LeadSources.Add(leadSource);
            _ = await _context.SaveChangesAsync();

            return CreatedAtAction("GetLeadSource", new { id = leadSource.Id }, leadSource);
        }



        private bool LeadSourceExists(string id)
        {
            return _context.LeadSources.Any(e => e.PublicKey.ToString() == id);
        }
    }
}
