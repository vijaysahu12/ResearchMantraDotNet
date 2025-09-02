using RM.Database.ResearchMantraContext;
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
    public class ServicesController : ControllerBase
    {
        private readonly ResearchMantraContext _context;

        public ServicesController(ResearchMantraContext context)
        {
            _context = context;
        }


        //public async Task<ActionResult<IEnumerable<Service>>> GetServices()
        //{
        //    //SqlParameter param1 = new SqlParameter("@ServiceId", 0);
        //    //var serviceslist = _context.Services.FromSqlRaw("Sp_GetServices @ServiceId", param1).ToList();
        //    //return serviceslist;

        //     return await _context.Services.ToListAsync();

        //}


        // GET: api/Services

        //public async Task<ActionResult<IList<Service>>> GetServices()
        //{
        //    //SqlParameter param1 = new SqlParameter("@ServiceId", 3);
        //    List<Service> serviceslist =  await _context.Services.FromSqlInterpolated($"Sp_GetServices").ToListAsync();
        //    return serviceslist;

        //}

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult> GetServices()
        {
            var services = await (from se in _context.Services
                                  from sc in _context.ServiceCategories
                                  from st in _context.ServiceTypes
                                  where (se.ServiceCategoryKey == sc.PublicKey.ToString()) && (se.ServiceTypeKey == st.PublicKey.ToString()) && (se.IsDelete == 0)
                                  orderby se.Name ascending
                                  select new { se.Id, se.Name, ServiceName = se.Name, Cost = se.ServiceCost, Category = sc.Name, ServicesType = st.Name, se.PublicKey, se.IsDisabled, ServiceTypeKey = st.PublicKey.ToString(), ServiceCategoryKey = sc.PublicKey.ToString() }).ToListAsync();

            return Ok(services);

        }



        //[AllowAnonymous]
        //[HttpGet("GetServiceDetails")]
        //public async Task<ActionResult> GetServiceDetails()
        //{
        //List<Service> services = await _context.Services.FromSqlInterpolated($"Sp_GetServices").ToListAsync();
        //return services;
        //   var services = from se in _context.Services
        //                   from sc in _context.ServiceCategories
        //                   from st in _context.ServiceTypes
        //                   where ((se.ServiceCategoryKey == sc.PublicKey.ToString()) && (se.ServiceTypeKey == st.PublicKey.ToString()) && (se.IsDelete == 0))
        //                   select new {Id = se.Id, Name = se.Name, Price = se.ServiceCost, Category = sc.Name, ServicesType = st.Name,PublicKey = se.PublicKey };

        //    return Ok(services.ToList());

        //}


        // GET: api/Services/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Service>> GetService(string id)
        {
            Service service = await _context.Services.FirstOrDefaultAsync(c => c.PublicKey.ToString() == id);

            return service == null ? (ActionResult<Service>)NotFound() : (ActionResult<Service>)service;
        }

        // PUT: api/Services/5


        [HttpPut("{id}")]
        public async Task<IActionResult> PutService(string id, Service service)
        {
            if (id != service.PublicKey.ToString())
            {
                return BadRequest();
            }

            _context.Entry(service).State = EntityState.Modified;
            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Service> entry = _context.Entry(service);
            entry.Property(e => e.PublicKey).IsModified = false;
            entry.Property(e => e.CreatedOn).IsModified = false;
            entry.Property(e => e.CreatedBy).IsModified = false;

            try
            {
                _ = await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ServiceExists(id))
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

        [HttpPut("ToggleServiceStatus/{id}")]
        public async Task<IActionResult> ToggleServiceStatus([FromRoute] Guid id, [FromQuery] bool isDisable)
        {
            var service = await _context.Services.FirstOrDefaultAsync(s => s.PublicKey == id);
            if (service == null)
            {
                return NotFound();
            }

            service.IsDisabled = isDisable ? (byte)1 : (byte)0;
            await _context.SaveChangesAsync();  // 🔥 Important: Save to database

            return Ok(new { message = "Service status updated successfully", service });
        }







        // POST: api/Services


        [HttpPost]
        public async Task<ActionResult<Service>> PostService(Service service)
        {
            _ = _context.Services.Add(service);
            _ = await _context.SaveChangesAsync();

            return CreatedAtAction("GetService", new { id = service.Id }, service);
        }



        private bool ServiceExists(string id)
        {
            return _context.Services.Any(e => e.PublicKey.ToString() == id);
        }
    }
}
