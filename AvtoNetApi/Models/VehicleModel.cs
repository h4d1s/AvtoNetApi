using System.Text.Json.Serialization;

namespace AvtoNet.API.Models
{
    public class VehicleModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        [JsonIgnore]
        public VehicleBrand Brand { get; set; } = null!;
    }
}
