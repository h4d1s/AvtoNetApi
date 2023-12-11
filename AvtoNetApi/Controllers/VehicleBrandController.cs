using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AvtoNet.API.Data;
using AvtoNet.API.Models;
using Microsoft.AspNetCore.Authorization;

namespace AvtoNet.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehicleBrandController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public VehicleBrandController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/VehicleBrands
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VehicleBrand>>> GetVehiclesBrands()
        {
          if (_context.VehiclesBrands == null)
          {
              return NotFound();
          }
            return await _context.VehiclesBrands.ToListAsync();
        }

        // GET: api/VehicleBrands/5
        [HttpGet("{id}")]
        public async Task<ActionResult<VehicleBrand>> GetVehicleBrand(int id)
        {
          if (_context.VehiclesBrands == null)
          {
              return NotFound();
          }
            var vehicleBrand = await _context.VehiclesBrands.FindAsync(id);

            if (vehicleBrand == null)
            {
                return NotFound();
            }

            return vehicleBrand;
        }

        // PUT: api/VehicleBrands/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutVehicleBrand(int id, VehicleBrand vehicleBrand)
        {
            if (id != vehicleBrand.Id)
            {
                return BadRequest();
            }

            _context.Entry(vehicleBrand).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VehicleBrandExists(id))
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

        // POST: api/VehicleBrands
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<VehicleBrand>> PostVehicleBrand(VehicleBrand vehicleBrand)
        {
          if (_context.VehiclesBrands == null)
          {
              return Problem("Entity set 'ApplicationDbContext.VehiclesBrands'  is null.");
          }
            _context.VehiclesBrands.Add(vehicleBrand);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetVehicleBrand", new { id = vehicleBrand.Id }, vehicleBrand);
        }

        // DELETE: api/VehicleBrands/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteVehicleBrand(int id)
        {
            if (_context.VehiclesBrands == null)
            {
                return NotFound();
            }
            var vehicleBrand = await _context.VehiclesBrands.FindAsync(id);
            if (vehicleBrand == null)
            {
                return NotFound();
            }

            _context.VehiclesBrands.Remove(vehicleBrand);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool VehicleBrandExists(int id)
        {
            return (_context.VehiclesBrands?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
