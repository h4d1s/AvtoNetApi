using System.Text.Json.Serialization;

namespace AvtoNet.API.Models
{
    public class VehicleBrand
    {
        public int Id { get; set; }
        public string Name { get; set; }
        [JsonIgnore]
        public ICollection<VehicleModel> Models { get; set; }
    }
}
