using RM.Database.KingResearchContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RM.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerTagsController : ControllerBase
    {
        private readonly KingResearchContext _context;

        public CustomerTagsController(KingResearchContext context)
        {
            _context = context;
        }

        // GET: api/CustomerTags
        [HttpGet]
        public async Task<ActionResult> GetCustomerTags()
        {
            // return await _context.CustomerTags.Where(c => c.IsDelete == 0).ToListAsync();

            var customerTags = await (from cu in _context.Customers
                                      join le in _context.Leads on cu.LeadKey equals le.PublicKey.ToString()
                                      join ct in _context.CustomerTags on cu.PublicKey.ToString() equals ct.CustomerKey.ToString()
                                      join tg in _context.Tags on ct.TagKey equals tg.PublicKey.ToString()
                                      where (ct.CustomerKey == cu.PublicKey.ToString()) && (ct.TagKey == tg.PublicKey.ToString()) && (ct.IsDelete == 0)
                                      orderby ct.Id descending
                                      select new { ct.Id, CustomerKey = le.FullName, TagKey = tg.Name, ct.CreatedOn, ct.PublicKey }).ToListAsync();

            return Ok(customerTags);
        }

        // GET: api/CustomerTags/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerTag>> GetCustomerTag(string id)
        {
            CustomerTag customerTag = await _context.CustomerTags.FirstOrDefaultAsync(c => c.PublicKey.ToString() == id);

            return customerTag == null ? (ActionResult<CustomerTag>)NotFound() : (ActionResult<CustomerTag>)customerTag;
        }

        // PUT: api/CustomerTags/5


        [HttpPut("{id}")]
        public async Task<IActionResult> PutCustomerTag(string id, CustomerTag customerTag)
        {
            if (id != customerTag.PublicKey.ToString())
            {
                return BadRequest();
            }

            _context.Entry(customerTag).State = EntityState.Modified;

            try
            {
                _ = await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CustomerTagExists(id))
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

        // POST: api/CustomerTags


        [HttpPost]
        public async Task<ActionResult<CustomerTag>> PostCustomerTag(CustomerTag customerTag)
        {
            _ = _context.CustomerTags.Add(customerTag);
            _ = await _context.SaveChangesAsync();

            return CreatedAtAction("GetCustomerTag", new { id = customerTag.Id }, customerTag);
        }

        [HttpPost("PullCustomerTags")]
        public ActionResult<CustomerTag> PullCustomerTags(CustomerTag customerTag)
        {
            try
            {


                //var customerTags = await (from ct in _context.CustomerTags
                //                          from cu in _context.Customers
                //                          from le in _context.Leads
                //                          from tg in _context.Tags
                //                          where ((ct.CustomerKey == cu.PublicKey.ToString()) && (ct.CustomerKey == customerTag.CustomerKey) && (ct.TagKey == tg.PublicKey.ToString()) && (ct.IsDelete == 0))
                //                          orderby ct.Id descending
                //                          select new { Id = ct.Id, CustomerKey = le.FullName, TagKey = tg.Name, CreatedOn = ct.CreatedOn, PublicKey = ct.PublicKey }).ToListAsync();


                var customerTags = (from ct in _context.CustomerTags
                                    join tg in _context.Tags on ct.TagKey equals tg.PublicKey.ToString()
                                    join cu in _context.Customers on ct.CustomerKey.ToString() equals cu.PublicKey.ToString()
                                    join le in _context.Leads on cu.LeadKey.ToString() equals le.PublicKey.ToString()
                                    select new { ct.Id, CustomerKey = le.FullName, TagKey = tg.Name, ct.CreatedOn, ct.PublicKey }).ToListAsync();


                return Ok(customerTags);
            }
            catch (Exception)
            {

                throw;
            }
        }


        private bool CustomerTagExists(string id)
        {
            return _context.CustomerTags.Any(e => e.PublicKey.ToString() == id);
        }
    }
}
