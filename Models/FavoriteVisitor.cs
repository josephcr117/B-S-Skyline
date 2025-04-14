namespace B_S_Skyline.Models
{
    public class FavoriteVisitor
    {
        public string Id { get; set; }
        public string ResidentId { get; set; }
        public string VisitorName { get; set; }
        public string LicensePlate { get; set; }
        public bool IsDeliveryContact { get; set; }
        public Visit.DeliveryServiceType? DeliveryService { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
