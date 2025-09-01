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
    public class ServiceTypesController : ControllerBase
    {
        private readonly KingResearchContext _context;

        public ServiceTypesController(KingResearchContext context)
        {
            _context = context;
        }

        // GET: api/ServiceTypes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServiceType>>> GetServiceTypes()
        {
            return await _context.ServiceTypes.Where(c => c.IsDelete == 0).ToListAsync();
        }

        // GET: api/ServiceTypes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceType>> GetServiceType(string id)
        {
            ServiceType serviceType = await _context.ServiceTypes.FirstOrDefaultAsync(c => c.PublicKey.ToString() == id);

            return serviceType == null ? (ActionResult<ServiceType>)NotFound() : (ActionResult<ServiceType>)serviceType;
        }

        // PUT: api/ServiceTypes/5


        [HttpPut("{id}")]
        public async Task<IActionResult> PutServiceType(string id, ServiceType serviceType)
        {
            if (id != serviceType.PublicKey.ToString())
            {
                return BadRequest();
            }

            _context.Entry(serviceType).State = EntityState.Modified;
            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<ServiceType> entry = _context.Entry(serviceType);
            entry.Property(e => e.PublicKey).IsModified = false;
            entry.Property(e => e.CreatedOn).IsModified = false;
            entry.Property(e => e.CreatedBy).IsModified = false;

            try
            {
                _ = await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ServiceTypeExists(id))
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

        // POST: api/ServiceTypes


        [HttpPost]
        public async Task<ActionResult<ServiceType>> PostServiceType(ServiceType serviceType)
        {
            _ = _context.ServiceTypes.Add(serviceType);
            _ = await _context.SaveChangesAsync();

            return CreatedAtAction("GetServiceType", new { id = serviceType.Id }, serviceType);
        }



        private bool ServiceTypeExists(string id)
        {
            return _context.ServiceTypes.Any(e => e.PublicKey.ToString() == id);
        }
    }
}
