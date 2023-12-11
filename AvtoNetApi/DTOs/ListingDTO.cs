using AvtoNet.API.Models;

namespace AvtoNet.API.DTOs
{
    public class ListingDTO: Listing
    {
        public string Brand { get; set; }
        public string Model { get; set; }
        public string UserPhone { get; set; }
        public string ImagePath { get; set; }
    }
}
