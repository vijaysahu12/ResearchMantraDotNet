using RM.Database.KingResearchContext;
using RM.Model.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace RM.API.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class RoleMenusController : ControllerBase
    {
        private readonly KingResearchContext _context;

        public RoleMenusController(KingResearchContext context)
        {
            _context = context;
        }

        // GET: api/RoleMenus
        [HttpGet]
        //public async Task<ActionResult<IEnumerable<RoleMenus>>> GetRoleMenus()
        //{
        //    return await _context.RoleMenus.Where(x => x.IsDelete == 0).ToListAsync();
        //}

        public ActionResult<RoleMenu> GetRoleMenus()
        {
            var MenusbyRoles = from rm in _context.RoleMenus
                               join m in _context.Menus on rm.MenuKey equals m.PublicKey.ToString()
                               join r in _context.Roles on rm.RoleKey equals r.PublicKey.ToString()
                               join pm in _context.Menus on m.ParentId equals pm.Id into parentMenuGroup
                               from pm in parentMenuGroup.DefaultIfEmpty() // LEFT JOIN for parent menu
                               where rm.IsDelete == 0 && r.IsDelete == 0 && m.IsDelete == 0
                               orderby rm.RoleKey ascending
                               select new
                               {
                                   rm.Id,
                                   roleKey = r.Name,
                                   menuKey = m.Name,
                                   rm.IsDisabled,
                                   url = m.Url,
                                   ParentId = pm != null ? pm.Name : null,
                                   rm.PublicKey
                               };

            return Ok(MenusbyRoles.ToList());
        }


        // GET: api/RoleMenus/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RoleMenu>> GetRoleMenus(string id)
        {
            RoleMenu roleMenus = await _context.RoleMenus.FirstOrDefaultAsync(c => (c.PublicKey.ToString() == id) && (c.IsDelete == 0));

            return roleMenus == null ? (ActionResult<RoleMenu>)NotFound() : (ActionResult<RoleMenu>)roleMenus;
        }

        [HttpPost("GetMenusbyRoles")]
        public ActionResult<Role> GetMenusbyRoles(QueryValues queryValues)
        {
            var MenusbyRoles = from m in _context.Menus
                               from r in _context.Roles
                               from rm in _context.RoleMenus
                               where (rm.RoleKey == queryValues.SecondaryKey)
                               && (rm.IsDelete == 0)
                               && (m.PublicKey.ToString() == rm.MenuKey)
                               && (r.PublicKey.ToString() == rm.RoleKey)
                               orderby m.SortOrder, m.ParentId ascending
                               select new { m.Id, m.ParentId, m.Name, icon = m.Description, url = m.Url, IsLHS = m.IsLhs, RoleName = r.Name, rm.IsDisabled, rm.PublicKey };

            return Ok(MenusbyRoles.ToList());
        }

        // PUT: api/RoleMenus/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRoleMenus(string id, RoleMenu roleMenus)
        {

            if (id != roleMenus.PublicKey.ToString())
            {
                return BadRequest();
            }

            _context.Entry(roleMenus).State = EntityState.Modified;
            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<RoleMenu> entry = _context.Entry(roleMenus);
            entry.Property(e => e.Id).IsModified = false; entry.Property(e => e.PublicKey).IsModified = false;
            entry.Property(e => e.CreatedOn).IsModified = false;
            entry.Property(e => e.CreatedBy).IsModified = false;


            try
            {
                _ = await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RoleMenusExists(id))
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

        [HttpDelete]
        public async Task<IActionResult> Delete([FromBody] RoleMenu rolemenu)
        {
            // Validate the input object
            if (rolemenu == null || string.IsNullOrEmpty(rolemenu.PublicKey.ToString()))
            {
                return BadRequest("Invalid data provided.");
            }

            // Find the strategy by PublicKey
            var item = await _context.RoleMenus.FirstOrDefaultAsync(s => s.PublicKey == rolemenu.PublicKey);
            if (item == null)
            {
                return NotFound("Role Menu not found.");
            }

            // Remove the strategy
            _context.RoleMenus.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent(); // Return 204 No Content for successful deletion
        }

        // POST: api/RoleMenus
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        public async Task<ActionResult<RoleMenu>> PostRoleMenus(RoleMenu roleMenus)
        {
            _ = _context.RoleMenus.Add(roleMenus);
            _ = await _context.SaveChangesAsync();

            return CreatedAtAction("GetRoleMenus", new { id = roleMenus.Id }, roleMenus);
        }



        private bool RoleMenusExists(string id)
        {
            return _context.RoleMenus.Any(e => e.PublicKey.ToString() == id);
        }
    }
}
