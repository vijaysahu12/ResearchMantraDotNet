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
    public class CommunicationTemplatesController : ControllerBase
    {
        private readonly KingResearchContext _context;

        public CommunicationTemplatesController(KingResearchContext context)
        {
            _context = context;
        }

        // GET: api/CommunicationTemplates
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CommunicationTemplate>>> GetCommunicationTemplates()
        {
            return await _context.CommunicationTemplates.Where(c => c.IsDelete == 0).ToListAsync();
        }

        // GET: api/CommunicationTemplates/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CommunicationTemplate>> GetCommunicationTemplates(string id)
        {
            CommunicationTemplate communicationTemplates = await _context.CommunicationTemplates.FirstOrDefaultAsync(c => c.PublicKey.ToString() == id);

            return communicationTemplates == null ? (ActionResult<CommunicationTemplate>)NotFound() : (ActionResult<CommunicationTemplate>)communicationTemplates;
        }

        // PUT: api/CommunicationTemplates/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCommunicationTemplates(string id, CommunicationTemplate communicationTemplates)
        {
            if (id != communicationTemplates.PublicKey.ToString())
            {
                return BadRequest("Mismatched ID and PublicKey.");
            }

            var existingEntity = await _context.CommunicationTemplates.AsNoTracking()
                .FirstOrDefaultAsync(e => e.PublicKey.ToString() == id);

            if (existingEntity == null)
            {
                return NotFound("Entity not found.");
            }

            // Only update modifiable fields
            existingEntity.Template = communicationTemplates.Template;
            existingEntity.Name = communicationTemplates.Name;

            _context.CommunicationTemplates.Update(existingEntity);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CommunicationTemplatesExists(id))
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

        // POST: api/CommunicationTemplates
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        public async Task<ActionResult<CommunicationTemplate>> PostCommunicationTemplates(CommunicationTemplate communicationTemplates)
        {
            _ = _context.CommunicationTemplates.Add(communicationTemplates);
            _ = await _context.SaveChangesAsync();

            return CreatedAtAction("GetCommunicationTemplates", new { id = communicationTemplates.Id }, communicationTemplates);
        }



        private bool CommunicationTemplatesExists(string id)
        {
            return _context.CommunicationTemplates.Any(e => e.PublicKey.ToString() == id);
        }


        [HttpDelete]
        public async Task<IActionResult> Delete([FromBody] CommunicationTemplate communicationTemplates)
        {
            // Validate the input object
            if (communicationTemplates == null || string.IsNullOrEmpty(communicationTemplates.PublicKey.ToString()))
            {
                return BadRequest("Invalid data provided.");
            }

            // Find the strategy by PublicKey
            var item = await _context.CommunicationTemplates.FirstOrDefaultAsync(s => s.PublicKey == communicationTemplates.PublicKey);
            if (item == null)
            {
                return NotFound("Strategy not found.");
            }

            // Remove the strategy
            _context.CommunicationTemplates.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent(); // Return 204 No Content for successful deletion
        }
    }
}
