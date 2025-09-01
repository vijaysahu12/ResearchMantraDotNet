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
    public class CustomerTypesController : ControllerBase
    {
        private readonly KingResearchContext _context;

        public CustomerTypesController(KingResearchContext context)
        {
            _context = context;
        }

        // GET: api/CustomerTypes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomerType>>> GetCustomerTypes()
        {
            return await _context.CustomerTypes.Where(c => c.IsDelete == 0).ToListAsync();
        }

        // GET: api/CustomerTypes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerType>> GetCustomerType(string id)
        {
            CustomerType customerType = await _context.CustomerTypes.FirstOrDefaultAsync(c => c.PublicKey.ToString() == id);

            return customerType == null ? (ActionResult<CustomerType>)NotFound() : (ActionResult<CustomerType>)customerType;
        }

        // PUT: api/CustomerTypes/5


        [HttpPut("{id}")]
        public async Task<IActionResult> PutCustomerType(string id, CustomerType customerType)
        {
            if (id != customerType.PublicKey.ToString())
            {
                return BadRequest();
            }

            _context.Entry(customerType).State = EntityState.Modified;
            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<CustomerType> entry = _context.Entry(customerType);
            entry.Property(e => e.PublicKey).IsModified = false;
            entry.Property(e => e.CreatedOn).IsModified = false;
            entry.Property(e => e.CreatedBy).IsModified = false;

            try
            {
                _ = await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CustomerTypeExists(id))
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

        // POST: api/CustomerTypes


        [HttpPost]
        public async Task<ActionResult<CustomerType>> PostCustomerType(CustomerType customerType)
        {
            _ = _context.CustomerTypes.Add(customerType);
            _ = await _context.SaveChangesAsync();

            return CreatedAtAction("GetCustomerType", new { id = customerType.Id }, customerType);
        }



        private bool CustomerTypeExists(string id)
        {
            return _context.CustomerTypes.Any(e => e.PublicKey.ToString() == id);
        }
    }
}
