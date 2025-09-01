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
    public class RolesController : ControllerBase
    {
        private readonly KingResearchContext _context;

        public RolesController(KingResearchContext context)
        {
            _context = context;
        }

        // GET: api/Roles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Role>>> GetRoles()
        {
            return await _context.Roles.Where(c => c.IsDelete == 0).ToListAsync();
        }

        // GET: api/Roles/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Role>> GetRole(string id)
        {
            Role role = await _context.Roles.FirstOrDefaultAsync(c => c.PublicKey.ToString() == id);

            return role == null ? (ActionResult<Role>)NotFound() : (ActionResult<Role>)role;
        }

        // PUT: api/Roles/5


        [HttpPut("{id}")]
        public async Task<IActionResult> PutRole(string id, Role role)
        {
            if (id != role.PublicKey.ToString())
            {
                return BadRequest();
            }

            _context.Entry(role).State = EntityState.Modified;

            try
            {
                _ = await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RoleExists(id))
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

        // POST: api/Roles


        [HttpPost]
        public async Task<ActionResult<Role>> PostRole(Role role)
        {
            _ = _context.Roles.Add(role);
            _ = await _context.SaveChangesAsync();

            return CreatedAtAction("GetRole", new { id = role.Id }, role);
        }

        // DELETE: api/Roles/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Role>> DeleteRole(string id)
        {
            Role role = await _context.Roles.FirstOrDefaultAsync(c => c.PublicKey.ToString() == id);
            if (role == null)
            {
                return NotFound();
            }

            _ = _context.Roles.Remove(role);
            _ = await _context.SaveChangesAsync();

            return role;
        }

        private bool RoleExists(string id)
        {
            return _context.Roles.Any(e => e.PublicKey.ToString() == id);
        }
    }
}
