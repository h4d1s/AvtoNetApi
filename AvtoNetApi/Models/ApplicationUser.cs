using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AvtoNet.API.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Street { get; set; } = null!;
        public string City { get; set; } = null!;
        [JsonIgnore]
        public ICollection<Listing>? Listings { get; set; }
    }
}
