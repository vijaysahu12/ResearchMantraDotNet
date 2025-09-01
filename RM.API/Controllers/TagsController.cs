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
    public class TagsController : ControllerBase
    {
        private readonly KingResearchContext _context;

        public TagsController(KingResearchContext context)
        {
            _context = context;
        }

        // GET: api/Tags
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tag>>> GetTags()
        {
            return await _context.Tags.Where(c => c.IsDelete == 0).ToListAsync();
        }

        // GET: api/Tags/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Tag>> GetTags(string id)
        {
            Tag tags = await _context.Tags.FirstOrDefaultAsync(c => c.PublicKey.ToString() == id);

            return tags == null ? (ActionResult<Tag>)NotFound() : (ActionResult<Tag>)tags;
        }

        // PUT: api/Tags/5


        [HttpPut("{id}")]
        public async Task<IActionResult> PutTags(string id, Tag tags)
        {
            if (id != tags.PublicKey.ToString())
            {
                return BadRequest();
            }

            _context.Entry(tags).State = EntityState.Modified;
            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Tag> entry = _context.Entry(tags);
            entry.Property(e => e.PublicKey).IsModified = false;
            entry.Property(e => e.CreatedOn).IsModified = false;
            entry.Property(e => e.CreatedBy).IsModified = false;

            try
            {
                _ = await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TagsExists(id))
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

        // POST: api/Tags


        [HttpPost]
        public async Task<ActionResult<Tag>> PostTags(Tag tags)
        {
            _ = _context.Tags.Add(tags);
            _ = await _context.SaveChangesAsync();

            return CreatedAtAction("GetTags", new { id = tags.Id }, tags);
        }


        private bool TagsExists(string id)
        {
            return _context.Tags.Any(e => e.PublicKey.ToString() == id);
        }
    }
}
