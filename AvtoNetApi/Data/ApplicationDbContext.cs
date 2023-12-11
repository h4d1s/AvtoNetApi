using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AvtoNet.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;

namespace AvtoNet.API.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            IWebHostEnvironment webHostEnvironment
        )
            : base(options)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        //public override int SaveChanges()
        //{
        //    var deletedImages = ChangeTracker.Entries<Image>()
        //        .Where(e => e.State == EntityState.Deleted);

        //    foreach (var entry in deletedImages)
        //    {
        //        var deletedImage = entry.Entity;
        //        var uploadsPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", deletedImage.ListingId.ToString());
        //        if (Directory.Exists(uploadsPath))
        //        {
        //            Directory.Delete(uploadsPath, true);
        //        }
        //    }

        //    return base.SaveChanges();
        //}

        public DbSet<Listing> Listings { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<VehicleBrand> VehiclesBrands { get; set; }
        public DbSet<VehicleModel> VehiclesModels { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Image> Images { get; set; }
    }
}
