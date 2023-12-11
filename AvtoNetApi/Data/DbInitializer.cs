using AvtoNet.API.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace AvtoNet.API.Data
{
    public class DbInitializer
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public DbInitializer(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IWebHostEnvironment webHostEnvironment
        )
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task Initialize()
        {
            if (_context.Listings.Any()
                && _context.Vehicles.Any()
                && _context.VehiclesBrands.Any()
                && _context.VehiclesModels.Any()
                && _context.ApplicationUsers.Any())
            {
                return;   // DB has been seeded
            }

            // Users

            var adminRole = new IdentityRole("Admin");
            var userRole = new IdentityRole("User");

            if (await _roleManager.FindByNameAsync("Admin") == null)
            {
                await _roleManager.CreateAsync(adminRole);
            }

            if (await _roleManager.FindByNameAsync("User") == null)
            {
                await _roleManager.CreateAsync(userRole);
            }

            var admin = new ApplicationUser
            {
                UserName = "admin",
                Email = "admin@example.com",
                FirstName = "Admin",
                LastName = "Admin",
                Street = Faker.Address.StreetName(),
                City = Faker.Address.City(),
                PhoneNumber = Faker.Phone.Number()
            };
            await _userManager.CreateAsync(admin, "Passw0rd!");
            await _userManager.AddToRoleAsync(admin, "Admin");

            var users = new List<ApplicationUser>();
            for (var i = 0; i < 10; i++)
            {
                var first = Faker.Name.First();
                var last = Faker.Name.Last();
                var username = first.ToLower() + last.ToLower();
                var user = new ApplicationUser
                {
                    UserName = username,
                    Email = username + "@example.com",
                    FirstName = first,
                    LastName = last,
                    Street = Faker.Address.StreetName(),
                    City = Faker.Address.City(),
                    PhoneNumber = Faker.Phone.Number()
                };
                users.Add(user);
            }

            foreach (var user in users)
            {
                if (await _userManager.FindByNameAsync(user.UserName) == null)
                {
                    var result = await _userManager.CreateAsync(user, "Passw0rd!");
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, "User");
                    }
                }
            }


            // Vehicles

            var brands = new Dictionary<string, string[]>();
            brands["Audi"] = new string[] { "A1", "A2", "A3", "A4", "A5", "A6", "A7", "A8" };
            brands["BMW"] = new string[] { "Series 1", "Series 2", "Series 3", "Series 4", "Series 5", "Series 6", "Series 7", "Series 8", "X Series", "Z Series" };
            brands["Mercedes-Benz"] = new string[] { "Series A", "Series B", "Series C", "Series E", "Series C", "Series G", "Series S" };
            brands["Volkswagen"] = new string[] { "Arteon", "Eos", "Golf", "Jetta", "Passat", "Polo" };
            
            var vehicles = new List<Vehicle>();
            foreach (KeyValuePair<string, string[]> item in brands)
            {
                var allModels = new List<VehicleModel>();
                foreach (string model in item.Value)
                {
                    var m = new VehicleModel { Name = model };
                    allModels.Add(m);
                }
                _context.VehiclesModels.AddRange(allModels);

                var brand = new VehicleBrand { Name = item.Key, Models = allModels };
                _context.VehiclesBrands.Add(brand);
                _context.SaveChanges();

                allModels.ForEach(m =>
                {
                    var vehicle = new Vehicle { Model = m, BrandId = brand.Id };
                    vehicles.Add(vehicle);
                });
            }
            _context.Vehicles.AddRange(vehicles);
            await _context.SaveChangesAsync();


            // Listings

            var random = new Random();
            var fuelTypes = new string[] { "Disel", "Gasoline" };
            var gearBoxes = new string[] { "Manual", "Automatic" };
            var colors = new string[] { "blue", "brown", "bronze", "yellow", "grey", "green", "red", "black", "silver", "violet", "white", "orange", "gold" };

            var listings = new List<Listing>();

            for (var i = 0; i < 20; i++)
            {
                var listing = new Listing {
                    Mileage = random.Next(0, 300000),
                    FuelType = fuelTypes[random.Next(0, 2)],
                    Gearbox = gearBoxes[random.Next(0, 2)],
                    YearOfProduction = random.Next(1990, DateTime.Now.Year),
                    Color = colors[random.Next(0, colors.Count())],
                    Price = random.Next(500, 100000),
                    Power = random.Next(44, 300),
                    EngineSize = random.Next(1000, 4000),
                    isSold = false,
                    PublishDate = DateTime.UtcNow.AddDays(new Random().Next(90)),
                    Vehicle = vehicles[random.Next(0, vehicles.Count())],
                    User = users[random.Next(0, users.Count())],
                };
                _context.Listings.Add(listing);

                var webRootPath = _webHostEnvironment.WebRootPath;
                var fileName = "1.jpg";

                var sampleImagePath = Path.Combine(_webHostEnvironment.WebRootPath, "sample-images", "1.jpg");
                var uploadsPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", listing.Id.ToString());
                var uploadsUrl = Path.Combine("uploads", listing.Id.ToString());

                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                File.Copy(sampleImagePath, Path.Combine(uploadsPath, "1.jpg"), true);

                var image = new Image {
                    CreatedAt = DateTime.Now,
                    Path = Path.Combine(uploadsUrl, fileName),
                    Listing = listing,
                };
                _context.Images.Add(image);
            }
            await _context.SaveChangesAsync();
        }
    }
}
