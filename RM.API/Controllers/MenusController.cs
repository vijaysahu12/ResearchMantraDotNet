using RM.Database.ResearchMantraContext;
using RM.Model.Common;
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
    public class MenusController : ControllerBase
    {
        private readonly ResearchMantraContext _context;

        public MenusController(ResearchMantraContext context)
        {
            _context = context;
        }

        // GET: api/Menus
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Menu>>> GetMenus()
        {
            return await _context.Menus.Where(x => x.IsDelete == 0).ToListAsync();
        }

        // GET: api/Menus/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Menu>> GetMenus(string id)
        {
            Menu menus = await _context.Menus.FirstOrDefaultAsync(c => (c.PublicKey.ToString() == id) && (c.IsDelete == 0));

            return menus == null ? (ActionResult<Menu>)NotFound() : (ActionResult<Menu>)menus;
        }

        // PUT: api/Menus/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMenus(string id, Menu menus)
        {
            if (id != menus.PublicKey.ToString())
            {
                return BadRequest();
            }

            _context.Entry(menus).State = EntityState.Modified;
            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Menu> entry = _context.Entry(menus);
            entry.Property(e => e.Id).IsModified = false; entry.Property(e => e.PublicKey).IsModified = false;
            entry.Property(e => e.CreatedOn).IsModified = false;
            entry.Property(e => e.CreatedBy).IsModified = false;

            try
            {
                _ = await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MenusExists(id))
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

        // POST: api/Menus
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        public async Task<ActionResult<Menu>> PostMenus(Menu menus)
        {
            _ = _context.Menus.Add(menus);
            _ = await _context.SaveChangesAsync();

            return CreatedAtAction("GetMenus", new { id = menus.Id }, menus);
        }


        private bool MenusExists(string id)
        {
            return _context.Menus.Any(e => e.PublicKey.ToString() == id);
        }

        [HttpPost("FilteredMenus")]
        public async Task<ActionResult<Menu>> FilteredMenus(QueryValues value)
        {
            if (value.PrimaryKey == "")
            {
                return Ok(await _context.Menus.Where(x => x.IsDelete == 0).ToListAsync());

            }

            List<Menu> a = await _context.Menus.Where(item => item.Name == value.PrimaryKey || item.Url.Contains(value.PrimaryKey)).ToListAsync();
            return a.Count == 0 ? (ActionResult<Menu>)Ok(null) : (ActionResult<Menu>)Ok(a);
        }




    }
}
