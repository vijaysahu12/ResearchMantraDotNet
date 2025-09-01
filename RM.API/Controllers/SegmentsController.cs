using RM.Database.KingResearchContext;
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
    public class SegmentsController : ControllerBase
    {
        private readonly KingResearchContext _context;

        public SegmentsController(KingResearchContext context)
        {
            _context = context;
        }

        // GET: api/Segments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Segment>>> GetSegments()
        {
            return await _context.Segments.Where(c => c.IsDelete == 0).ToListAsync();
        }

        // GET: api/Segments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Segment>> GetSegment(string id)
        {
            Segment segment = await _context.Segments.FirstOrDefaultAsync(c => c.PublicKey.ToString() == id);

            return segment == null ? (ActionResult<Segment>)NotFound() : (ActionResult<Segment>)segment;
        }

        // PUT: api/Segments/5


        [HttpPut("{id}")]
        public async Task<IActionResult> PutSegment(string id, Segment segment)
        {
            if (id != segment.PublicKey.ToString())
            {
                return BadRequest();
            }

            _context.Entry(segment).State = EntityState.Modified;
            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Segment> entry = _context.Entry(segment);
            entry.Property(e => e.PublicKey).IsModified = false;
            entry.Property(e => e.CreatedOn).IsModified = false;
            entry.Property(e => e.CreatedBy).IsModified = false;

            try
            {
                _ = await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SegmentExists(id))
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

        // POST: api/Segments


        [HttpPost]
        public async Task<ActionResult<Segment>> PostSegment(Segment segment)
        {
            _ = _context.Segments.Add(segment);
            _ = await _context.SaveChangesAsync();

            return CreatedAtAction("GetSegment", new { id = segment.Id }, segment);
        }

        // DELETE: api/Segments/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Segment>> DeleteSegment(string id)
        {
            Segment segment = await _context.Segments.FirstOrDefaultAsync(c => c.PublicKey.ToString() == id);
            if (segment == null)
            {
                return NotFound();
            }

            _ = _context.Segments.Remove(segment);
            _ = await _context.SaveChangesAsync();

            return segment;
        }

        private bool SegmentExists(string id)
        {
            return _context.Segments.Any(e => e.PublicKey.ToString() == id);
        }
    }
}
