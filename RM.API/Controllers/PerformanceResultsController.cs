using RM.Database.ResearchMantraContext;
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
    public class PerformanceResultsController : ControllerBase
    {
        private readonly ResearchMantraContext _context;

        public PerformanceResultsController(ResearchMantraContext context)
        {
            _context = context;
        }

        // GET: api/PerformanceResults
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PerformanceResult>>> GetPerformanceResults()
        {
            return await _context.PerformanceResults.Where(c => c.IsDelete == 0).ToListAsync();
        }

        // GET: api/PerformanceResults/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PerformanceResult>> GetPerformanceResults(string id)
        {
            PerformanceResult performanceResults = await _context.PerformanceResults.FirstOrDefaultAsync(c => c.PublicKey.ToString() == id);

            return performanceResults == null ? (ActionResult<PerformanceResult>)NotFound() : (ActionResult<PerformanceResult>)performanceResults;
        }

        // PUT: api/PerformanceResults/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPerformanceResults(string id, PerformanceResult performanceResults)
        {

            if (id != performanceResults.PublicKey.ToString())
            {
                return BadRequest();
            }

            _context.Entry(performanceResults).State = EntityState.Modified;

            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<PerformanceResult> entry = _context.Entry(performanceResults);
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
                if (!PerformanceResultsExists(id))
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

        // POST: api/PerformanceResults
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        public async Task<ActionResult<PerformanceResult>> PostPerformanceResults(PerformanceResult performanceResults)
        {
            //performanceResults.PublicKey = new Guid();
            _ = _context.PerformanceResults.Add(performanceResults);
            _ = await _context.SaveChangesAsync();

            return CreatedAtAction("GetPerformanceResults", new { id = performanceResults.PublicKey }, performanceResults);
        }

        private bool PerformanceResultsExists(string id)
        {
            return _context.PerformanceResults.Any(e => e.PublicKey.ToString() == id);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromBody] PerformanceResult performanceResult)
        {
            // Validate the input object
            if (performanceResult == null || string.IsNullOrEmpty(performanceResult.PublicKey.ToString()))
            {
                return BadRequest("Invalid data provided.");
            }

            // Find the strategy by PublicKey
            var item = await _context.PerformanceResults.FirstOrDefaultAsync(s => s.PublicKey == performanceResult.PublicKey);
            if (item == null)
            {
                return NotFound("Strategy not found.");
            }

            // Remove the strategy
            _context.PerformanceResults.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent(); // Return 204 No Content for successful deletion
        }
    }
}
