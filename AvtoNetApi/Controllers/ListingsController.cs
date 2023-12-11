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
using AvtoNet.API.DTOs;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using System.Reflection;

namespace AvtoNet.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ListingsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;

        public ListingsController(
            ApplicationDbContext context,
            IHttpContextAccessor httpContextAccessor,
            IWebHostEnvironment webHostEnvironment,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
        }

        // GET: api/Listings
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ListingDTO>>> GetListings(
            int page = 1,
            int pageSize = 5,
            int? brandId = null,
            int? modelId = null,
            int? priceMin = null,
            int? priceMax = null,
            int? yearMin = null,
            int? yearMax = null,
            int? kmMax = null,
            int? kmMin = null,
            string? fuelType = null,
            string? userId = null
        )
        {
            if (_context.Listings == null)
            {
                return NotFound();
            }

            IQueryable<Listing> query = _context.Listings;

            if (brandId != null)
            {
                query = query.Where(l => l.Vehicle.Model.Brand.Id == brandId);
            }
            if (modelId != null)
            {
                query = query.Where(l => l.Vehicle.Model.Id == modelId);
            }
            if(priceMin != null)
            {
                query = query.Where(l => l.Price >= priceMin);
            }
            if (priceMax != null)
            {
                query = query.Where(l => l.Price <= priceMax);
            }
            if (yearMin != null)
            {
                query = query.Where(l => l.YearOfProduction >= yearMin);
            }
            if (yearMax != null)
            {
                query = query.Where(l => l.YearOfProduction <= yearMax);
            }
            if (kmMin != null)
            {
                query = query.Where(l => l.Mileage >= kmMin);
            }
            if (kmMax != null)
            {
                query = query.Where(l => l.Mileage <= kmMax);
            }
            if (fuelType != null)
            {
                query = query.Where(l => l.FuelType == fuelType);
            }
            if (userId != null)
            {
                query = query.Where(l => l.User.Id == userId);
            }

            query = query
                .OrderByDescending(l => l.PublishDate);

            var totalCount = query.Count();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var paginationMetadata = new
            {
                TotalCount = totalCount,
                PageSize = pageSize,
                CurrentPage = page,
                TotalPages = totalPages,
            };

            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

            var request = _httpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host.Value}";

            var listings = await query
                .Include(l => l.Vehicle)
                .Include(l => l.Image)
                .Select(l => new ListingDTO
                {
                    Id = l.Id,
                    Mileage = l.Mileage,
                    FuelType = l.FuelType,
                    Gearbox = l.Gearbox,
                    YearOfProduction = l.YearOfProduction,
                    Color = l.Color,
                    Price = l.Price,
                    Power = l.Power,
                    EngineSize = l.EngineSize,
                    PublishDate = l.PublishDate,
                    isSold = l.isSold,
                    Brand = l.Vehicle.Model.Brand.Name,
                    Model = l.Vehicle.Model.Name,
                    ImagePath = new Uri(new Uri(baseUrl), l.Image.Path).AbsoluteUri,
                })
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(listings);
        }

        // GET: api/Listings/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Listing>> GetListing(Guid id)
        {
            if (_context.Listings == null)
            {
                return NotFound();
            }

            var request = _httpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host.Value}";

            var listing = await _context.Listings
                .Select(l => new ListingDTO
                {
                    Id = l.Id,
                    Mileage = l.Mileage,
                    FuelType = l.FuelType,
                    Gearbox = l.Gearbox,
                    YearOfProduction = l.YearOfProduction,
                    Color = l.Color,
                    Price = l.Price,
                    Power = l.Power,
                    EngineSize = l.EngineSize,
                    PublishDate = l.PublishDate,
                    isSold = l.isSold,
                    Brand = l.Vehicle.Model.Brand.Name,
                    Model = l.Vehicle.Model.Name,
                    UserPhone = l.User.PhoneNumber,
                    ImagePath = new Uri(new Uri(baseUrl), l.Image.Path).AbsoluteUri,
                })
                .FirstOrDefaultAsync(a => a.Id == id);

            if (listing == null)
            {
                return NotFound();
            }

            return listing;
        }

        // PUT: api/Listings/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> PutListing(Guid id, [FromForm] ListingUpdateDTO model)
        {
            var existingListing = await _context.Listings
                .Include(l => l.Image)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (existingListing == null)
            {
                return NotFound();
            }

            existingListing.Mileage = model.Mileage;
            existingListing.FuelType = model.FuelType;
            existingListing.Gearbox = model.Gearbox;
            existingListing.YearOfProduction = model.YearOfProduction;
            existingListing.Color = model.Color;
            existingListing.EngineSize = model.EngineSize;
            existingListing.Power = model.Power;
            existingListing.Price = model.Price;

            if (model.ImageFile != null)
            {
                var uploadsPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", existingListing.Id.ToString());
                var uploadsUrl = Path.Combine("uploads", existingListing.Id.ToString());

                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }
                else
                {
                    string[] files = Directory.GetFiles(uploadsPath);
                    foreach (string file in files)
                    {
                        new FileInfo(file).Delete();
                    }
                }

                var filePath = Path.Combine(uploadsPath, model.ImageFile.FileName);
                var urlPath = Path.Combine(uploadsUrl, model.ImageFile.FileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(fileStream);
                }

                if (existingListing.Image != null)
                {
                    existingListing.Image.Path = urlPath;
                }
                else
                {
                    var image = new Image()
                    {
                        CreatedAt = DateTime.Now,
                        Path = urlPath,
                        Listing = existingListing
                    };
                    _context.Images.Add(image);
                    await _context.SaveChangesAsync();
                }
            }

            _context.Listings.Update(existingListing);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ListingExists(id))
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

        // POST: api/Listings
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Listing>> PostListing(ListingCreateDTO model)
        {
            var user = await _userManager.FindByIdAsync(model.userId);
            var vehicle = await _context.Vehicles
                .Where(v => v.BrandId == model.brandId && v.Model.Id == model.modelId)
                .FirstAsync();

            if (user == null || vehicle == null)
            {
                return NotFound("User or Vehicle not found");
            }

            var listing = new Listing() {
                Mileage = model.Mileage,
                FuelType = model.FuelType,
                Gearbox = model.Gearbox,
                YearOfProduction = model.YearOfProduction,
                Color = model.Color,
                Price = model.Price,
                Power = model.Power,
                EngineSize = model.EngineSize,
                User = user,
                Vehicle = vehicle,
                PublishDate = DateTime.UtcNow,
                isSold = false
            };

            _context.Listings.Add(listing);

            if (_context.Listings == null)
            {
              return Problem("Entity set 'ApplicationDbContext.Listings' is null.");
            }

            var image = new Image
            {
                CreatedAt = DateTime.Now,
            };

            if (model.ImageFile != null)
            {
                var uploadsPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", listing.Id.ToString());
                var uploadsUrl = Path.Combine("uploads", listing.Id.ToString());

                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                var filePath = Path.Combine(uploadsPath, model.ImageFile.FileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(fileStream);
                }

                image.Path = Path.Combine(uploadsUrl, model.ImageFile.FileName);
                listing.Image = image;
            }

            _context.Images.Add(image);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Problem("Cannot save.");
            }

            return CreatedAtAction("GetListing", new { id = listing.Id }, listing);
        }

        // DELETE: api/Listings/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteListing(Guid id)
        {
            if (_context.Listings == null)
            {
                return NotFound();
            }
            var listing = await _context.Listings
                .Include(l => l.Image)
                .FirstAsync(l => l.Id == id);
            if (listing == null)
            {
                return NotFound();
            }

            _context.Listings.Remove(listing);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ListingExists(Guid id)
        {
            return (_context.Listings?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
