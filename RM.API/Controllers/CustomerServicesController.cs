using RM.API.Services;
using RM.Database.KingResearchContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RM.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerServicesController : ControllerBase
    {
        private readonly KingResearchContext _context;
        private readonly IPurchaseOrderService _purchaseOrderService;
        public CustomerServicesController(KingResearchContext context, IPurchaseOrderService purchaseOrderService)
        {
            _purchaseOrderService = purchaseOrderService;
            _context = context;
        }

        // GET: api/CustomerServices/5
        [HttpGet("{leadId}")]
        public async Task<IActionResult> GetCustomerService(string leadId)
        {
            Model.ApiCommonResponseModel customerService = await _purchaseOrderService.GetPurchaseOrdersByLead(Guid.Parse(leadId));
            return Ok(customerService);
        }

        // PUT: api/CustomerServices/5


        [HttpPut("{id}")]
        public async Task<IActionResult> PutCustomerService(string id, Database.KingResearchContext.CustomerService customerService)
        {
            if (id != customerService.PublicKey.ToString())
            {
                return BadRequest();
            }

            _context.Entry(customerService).State = EntityState.Modified;

            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Database.KingResearchContext.CustomerService> entry = _context.Entry(customerService);
            entry.Property(e => e.PublicKey).IsModified = false;
            entry.Property(e => e.CreatedOn).IsModified = false;
            entry.Property(e => e.CreatedBy).IsModified = false;

            try
            {
                _ = await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CustomerServiceExists(id))
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

        private bool CustomerServiceExists(string id)
        {
            return _context.CustomerServices.Any(e => e.PublicKey.ToString() == id);
        }
    }
}
