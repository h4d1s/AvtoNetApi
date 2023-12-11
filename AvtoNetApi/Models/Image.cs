using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Metadata;
using System.Text.Json.Serialization;

namespace AvtoNet.API.Models
{
    public class Image
    {
        public Guid Id { get; set; }
        public string Path { get; set; }
        public DateTime CreatedAt { get; set; }
        [ForeignKey("Listing")]
        public Guid ListingId { get; set; }
        [JsonIgnore]
        public Listing Listing { get; set; } = null!;
    }
}
