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
    public class CommunicationModesController : ControllerBase
    {
        private readonly ResearchMantraContext _context;

        public CommunicationModesController(ResearchMantraContext context)
        {
            _context = context;
        }

        // GET: api/CommunicationModes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CommunicationMode>>> GetCommunicationModes()
        {
            return await _context.CommunicationModes.Where(c => c.IsDelete == 0).ToListAsync();
        }

        // GET: api/CommunicationModes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CommunicationMode>> GetCommunicationMode(string id)
        {
            CommunicationMode communicationMode = await _context.CommunicationModes.FirstOrDefaultAsync(c => c.PublicKey.ToString() == id);

            return communicationMode == null ? (ActionResult<CommunicationMode>)NotFound() : (ActionResult<CommunicationMode>)communicationMode;
        }

        // PUT: api/CommunicationModes/5


        [HttpPut("{id}")]
        public async Task<IActionResult> PutCommunicationMode(string id, CommunicationMode communicationMode)
        {
            if (id != communicationMode.PublicKey.ToString())
            {
                return BadRequest();
            }

            _context.Entry(communicationMode).State = EntityState.Modified;

            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<CommunicationMode> entry = _context.Entry(communicationMode);
            entry.Property(e => e.PublicKey).IsModified = false;
            entry.Property(e => e.CreatedOn).IsModified = false;
            entry.Property(e => e.CreatedBy).IsModified = false;

            try
            {
                _ = await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CommunicationModeExists(id))
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

        // POST: api/CommunicationModes


        [HttpPost]
        public async Task<ActionResult<CommunicationMode>> PostCommunicationMode(CommunicationMode communicationMode)
        {
            _ = _context.CommunicationModes.Add(communicationMode);
            _ = await _context.SaveChangesAsync();

            return CreatedAtAction("GetCommunicationMode", new { id = communicationMode.Id }, communicationMode);
        }


        private bool CommunicationModeExists(string id)
        {
            return _context.CommunicationModes.Any(e => e.PublicKey.ToString() == id);
        }
    }
}
