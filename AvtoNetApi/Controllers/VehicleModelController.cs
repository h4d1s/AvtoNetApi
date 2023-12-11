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
    public class VehicleModelController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public VehicleModelController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/VehicleModels
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VehicleModel>>> GetVehiclesModels()
        {
          if (_context.VehiclesModels == null)
          {
              return NotFound();
          }
          
          return await _context.VehiclesModels
                .ToListAsync();
        }

        // GET: api/VehicleModels/byBrand
        [HttpGet("byBrand/{brandId}")]
        public async Task<ActionResult<IEnumerable<VehicleModel>>> GetVehiclesModels(int brandId)
        {
            if (_context.VehiclesModels == null)
            {
                return NotFound();
            }

            var modelsByBrand = await _context.VehiclesModels
                  .Where(m => m.Brand.Id == brandId)
                  .ToListAsync();

            if (modelsByBrand == null || !modelsByBrand.Any())
            {
                return NotFound();
            }

            return Ok(modelsByBrand);
        }

        // GET: api/VehicleModels/5
        [HttpGet("{id}")]
        public async Task<ActionResult<VehicleModel>> GetVehicleModel(int id)
        {
          if (_context.VehiclesModels == null)
          {
              return NotFound();
          }
            var vehicleModel = await _context.VehiclesModels.FindAsync(id);

            if (vehicleModel == null)
            {
                return NotFound();
            }

            return vehicleModel;
        }

        // PUT: api/VehicleModels/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutVehicleModel(int id, VehicleModel vehicleModel)
        {
            if (id != vehicleModel.Id)
            {
                return BadRequest();
            }

            _context.Entry(vehicleModel).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VehicleModelExists(id))
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

        // POST: api/VehicleModels
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<VehicleModel>> PostVehicleModel(VehicleModel vehicleModel)
        {
          if (_context.VehiclesModels == null)
          {
              return Problem("Entity set 'ApplicationDbContext.VehiclesModels'  is null.");
          }
            _context.VehiclesModels.Add(vehicleModel);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetVehicleModel", new { id = vehicleModel.Id }, vehicleModel);
        }

        // DELETE: api/VehicleModels/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteVehicleModel(int id)
        {
            if (_context.VehiclesModels == null)
            {
                return NotFound();
            }
            var vehicleModel = await _context.VehiclesModels.FindAsync(id);
            if (vehicleModel == null)
            {
                return NotFound();
            }

            _context.VehiclesModels.Remove(vehicleModel);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool VehicleModelExists(int id)
        {
            return (_context.VehiclesModels?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
