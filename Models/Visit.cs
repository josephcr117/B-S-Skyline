﻿using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace B_S_Skyline.Models
{
    public class Visit
    {
        public string Id { get; set; }
        public DateTime? EntryTime { get; set; } = DateTime.Now;
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
        public string ProjectId { get; set; }
        public bool ApprovedBySecurity { get; set; }
        [Display(Name = "Is Delivery")]
        public bool IsDelivery { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DeliveryServiceType? DeliveryService { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum DeliveryServiceType
        {
            [Display(Name = "Uber Eats")]
            None = 0,
            [Display(Name = "Rappi")]
            UberEats= 1,
            [Display(Name = "Didi")]
            Rappi = 2,
            [Display(Name = "PedidosYa")]
            Didi = 3,
            [Display(Name = "Other")]
            PedidosYa = 4,
            Other = 5
        }
        public enum VisitType
        {
            Personal = 0,
            Delivery = 1,
            Other = 2
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
