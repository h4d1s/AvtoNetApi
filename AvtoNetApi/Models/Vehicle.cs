using Microsoft.Extensions.Hosting;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AvtoNet.API.Models
{
    public class Vehicle
    {
        [Key]
        public Guid Id { get; set; }
        public int BrandId { get; set; }
        [JsonIgnore]
        public VehicleModel Model { get; set; }
    }
}
