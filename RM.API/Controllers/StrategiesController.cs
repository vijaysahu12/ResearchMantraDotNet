using RM.CommonServices.Helpers;
using RM.Database.ResearchMantraContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RM.API.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class StrategiesController : ControllerBase
    {
        private readonly ResearchMantraContext _context;

        public StrategiesController(ResearchMantraContext context)
        {
            _context = context;
        }

        // GET: api/Strategies
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Strategy>>> GetStrategies()
        {
            return await _context.Strategies.Where(c => c.IsDelete == 0).ToListAsync();
        }

        // GET: api/Strategies/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Strategy>> GetStrategies(string id)
        {
            Strategy strategies = await _context.Strategies.FirstOrDefaultAsync(c => c.PublicKey.ToString() == id);

            return strategies == null ? (ActionResult<Strategy>)NotFound() : (ActionResult<Strategy>)strategies;
        }

        // PUT: api/Strategies/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutStrategies(string id, Strategy strategies)
        {
            string userPublicKeyClaim = UserClaimsHelper.GetClaimValue(User, "userPublicKey");
            Guid loggedUser = Guid.Empty; // Default value

            if (!string.IsNullOrEmpty(userPublicKeyClaim) && Guid.TryParse(userPublicKeyClaim, out Guid parsedUser))
            {
                loggedUser = parsedUser;
            }

            if (id != strategies.PublicKey.ToString())
            {
                return BadRequest("Mismatched ID.");
            }

            Strategy existingStrategy = await _context.Strategies
                .Where(item => item.PublicKey == Guid.Parse(id))
                .FirstOrDefaultAsync();

            if (existingStrategy == null)
            {
                return NotFound();
            }

            existingStrategy.Name = strategies.Name;
            existingStrategy.Description = strategies.Description;
            existingStrategy.ModifiedOn = DateTime.Now;
            existingStrategy.ModifiedBy = loggedUser != Guid.Empty ? loggedUser.ToString() : existingStrategy.ModifiedBy;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StrategiesExists(id))
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


        // POST: api/Strategies
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        public async Task<ActionResult<Strategy>> PostStrategies(Strategy strategies)
        {
            _ = _context.Strategies.Add(strategies);
            _ = await _context.SaveChangesAsync();

            return CreatedAtAction("GetStrategies", new { id = strategies.PublicKey }, strategies);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromBody] Strategy strategies)
        {
            // Validate the input object
            if (strategies == null || string.IsNullOrEmpty(strategies.PublicKey.ToString()))
            {
                return BadRequest("Invalid data provided.");
            }

            // Find the strategy by PublicKey
            var item = await _context.Strategies.FirstOrDefaultAsync(s => s.PublicKey == strategies.PublicKey);
            if (item == null)
            {
                return NotFound("Strategy not found.");
            }

            // Remove the strategy
            _context.Strategies.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent(); // Return 204 No Content for successful deletion
        }



        private bool StrategiesExists(string id)
        {
            return _context.Strategies.Any(e => e.PublicKey.ToString() == id);
        }
    }
}
