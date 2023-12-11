using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AvtoNet.API.Models
{
    public class Listing
    {
        [Key]
        public Guid Id { get; set; }
        public int Mileage { get; set; }
        public string FuelType { get; set; }
        public string Gearbox { get; set; }
        public int YearOfProduction { get; set; }
        public string Color { get; set; }
        public int Price { get; set; }
        public int Power { get; set; }
        public int EngineSize { get; set; }
        public DateTime PublishDate { get; set; }
        public bool isSold { get; set; }
        [JsonIgnore]
        public ApplicationUser User { get; set; } = null!;
        [JsonIgnore]
        public Vehicle Vehicle { get; set; } = null!;
        [JsonIgnore]
        public Image? Image { get; set; }
    }
}
