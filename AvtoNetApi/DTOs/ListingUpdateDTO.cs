namespace AvtoNet.API.DTOs
{
    public class ListingUpdateDTO
    {
        public int Mileage { get; set; }
        public string FuelType { get; set; }
        public string Gearbox { get; set; }
        public int YearOfProduction { get; set; }
        public string Color { get; set; }
        public int Price { get; set; }
        public int Power { get; set; }
        public int EngineSize { get; set; }
        public IFormFile? ImageFile { get; set; }
    }
}
