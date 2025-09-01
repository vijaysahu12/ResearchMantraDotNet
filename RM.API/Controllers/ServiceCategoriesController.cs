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
    public class ServiceCategoriesController : ControllerBase
    {
        private readonly KingResearchContext _context;

        public ServiceCategoriesController(KingResearchContext context)
        {
            _context = context;
        }

        // GET: api/ServiceCategories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServiceCategory>>> GetServiceCategories()
        {
            return await _context.ServiceCategories.Where(c => c.IsDelete == 0).ToListAsync();
        }

        // GET: api/ServiceCategories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceCategory>> GetServiceCategory(string id)
        {
            ServiceCategory serviceCategory = await _context.ServiceCategories.FirstOrDefaultAsync(c => c.PublicKey.ToString() == id);

            return serviceCategory == null ? (ActionResult<ServiceCategory>)NotFound() : (ActionResult<ServiceCategory>)serviceCategory;
        }

        // PUT: api/ServiceCategories/5


        [HttpPut("{id}")]
        public async Task<IActionResult> PutServiceCategory(string id, ServiceCategory serviceCategory)
        {
            if (id != serviceCategory.PublicKey.ToString())
            {
                return BadRequest();
            }

            _context.Entry(serviceCategory).State = EntityState.Modified;
            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<ServiceCategory> entry = _context.Entry(serviceCategory);
            entry.Property(e => e.PublicKey).IsModified = false;
            entry.Property(e => e.CreatedOn).IsModified = false;
            entry.Property(e => e.CreatedBy).IsModified = false;

            try
            {
                _ = await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ServiceCategoryExists(id))
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

        // POST: api/ServiceCategories


        [HttpPost]
        public async Task<ActionResult<ServiceCategory>> PostServiceCategory(ServiceCategory serviceCategory)
        {
            _ = _context.ServiceCategories.Add(serviceCategory);
            _ = await _context.SaveChangesAsync();

            return CreatedAtAction("GetServiceCategory", new { id = serviceCategory.Id }, serviceCategory);
        }



        private bool ServiceCategoryExists(string id)
        {
            return _context.ServiceCategories.Any(e => e.PublicKey.ToString() == id);
        }
    }
}
