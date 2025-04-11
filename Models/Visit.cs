using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace B_S_Skyline.Models
{
    public class Visit
    {
        public string Id { get; set; }
        public DateTime EntryTime { get; set; } = DateTime.Now;
        public DateTime? ExitTime { get; set; }

        [Required]
        public string ResidentId { get; set; }

        [Required]
        [Display(Name = "Name")]
        public string VisitorName { get; set; }
        [Display(Name = "ID number")]
        public string VisitorDni { get; set; }
        [Display(Name = "License Plate")]
        public string LicensePlate { get; set; }
        public bool IsDelivery { get; set; }
        public DeliveryServiceType DeliveryService { get; set; }
        public enum DeliveryServiceType
        {
            UberEats = 0,
            Rappi = 1,
            Didi = 2,
            PedidosYa = 3,
            Other = 4
        }
        public string DeliveryServiceDisplayName => DeliveryService switch
        {
            DeliveryServiceType.UberEats => "UberEats",
            DeliveryServiceType.Rappi => "Rappi",
            DeliveryServiceType.Didi => "Didi",
            DeliveryServiceType.PedidosYa => "PedidosYa",
            _ => "Other Delivery"
        };
    }
}
