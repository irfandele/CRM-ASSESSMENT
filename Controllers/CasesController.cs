using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRM.Data;
using CRM.Models.Entities;
using Newtonsoft.Json;

namespace CRM.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CasesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CasesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("whatsapp")]
        public async Task<ActionResult<IEnumerable<Case>>> GetWhatsappCases()
        {
            List<Case> whatsappCases = await _context.Cases
                .Where(c => c.CaseChannel.ToLower() == "whatsapp")
                .Include(c => c.User)
                .ToListAsync();

            if (!whatsappCases.Any()) return NotFound();
            return Ok(whatsappCases);
        }

        [HttpGet("{customerId}")]
        public async Task<ActionResult<Case>> GetCase(Guid customerId)
        {
            var @case = await _context.Cases
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.CustomerID == customerId);

            if (@case == null) return NotFound();
            return @case;
        }

        [HttpPut("{customerId}")]
        public async Task<IActionResult> PutCase(Guid customerId, Case updatedCase)
        {
            var existingCase = await _context.Cases.FindAsync(customerId);
            if (existingCase == null) return NotFound();

            // Preserve original EmpID and CreatedBy
            updatedCase.EmpID = existingCase.EmpID;
            updatedCase.CreatedBy = existingCase.CreatedBy;

            // Update all editable fields explicitly
            existingCase.CustomerName = updatedCase.CustomerName;
            existingCase.Contact = updatedCase.Contact;
            existingCase.CaseChannel = updatedCase.CaseChannel;
            existingCase.Description = updatedCase.Description;
            existingCase.Status = updatedCase.Status;

            Console.WriteLine($"Received update request: {JsonConvert.SerializeObject(updatedCase)}");

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Update failed: {ex.InnerException?.Message ?? ex.Message}");
            }

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<Case>> PostCase(Case @case)
        {
            @case.CustomerID = Guid.NewGuid();
            @case.CreatedBy = @case.EmpID;

            var userExists = await _context.Users.AnyAsync(u => u.EmpID == @case.EmpID);
            if (!userExists) return BadRequest("Employee not found.");

            _context.Cases.Add(@case);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCase), new { customerId = @case.CustomerID }, @case);
        }

        [HttpDelete("{customerId}")]
        public async Task<IActionResult> DeleteCase(Guid customerId)
        {
            var @case = await _context.Cases.FindAsync(customerId);
            if (@case == null) return NotFound();

            _context.Cases.Remove(@case);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CaseExists(Guid customerId)
        {
            return _context.Cases.Any(e => e.CustomerID == customerId);
        }
    }
}