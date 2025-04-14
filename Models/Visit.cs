using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace B_S_Skyline.Models
{
    public class Visit
    {
        public string Id { get; set; }
        public DateTime EntryTime { get; set; } = DateTime.Now;
        public DateTime? ExitTime { get; set; }
        public bool IsFavorite { get; set; }
        [Required]
        public string ResidentId { get; set; }

        [Required]
        [Display(Name = "Name")]
        public string VisitorName { get; set; }
        [Display(Name = "ID number")]
        public string VisitorDni { get; set; }
        [Display(Name = "License Plate")]
        public string LicensePlate { get; set; }
        [Display(Name = "Is Delivery")]
        public bool IsDelivery { get; set; }
        public DeliveryServiceType DeliveryService { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum DeliveryServiceType
        {
            [Display(Name = "Uber Eats")]
            UberEats = 0,
            [Display(Name = "Rappi")]
            Rappi = 1,
            [Display(Name = "Didi")]
            Didi = 2,
            [Display(Name = "PedidosYa")]
            PedidosYa = 3,
            [Display(Name = "Other")]
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
