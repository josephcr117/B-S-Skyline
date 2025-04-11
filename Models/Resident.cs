using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace B_S_Skyline.Models
{
    public class Resident
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Dni { get; set; }
        public List<string> PhoneNumbers { get; set; } = new List<string>();
        public string Email { get; set; }
        [Display(Name = "Profile Photo")]
        [DataType(DataType.ImageUrl)]
        public string PhotoUrl { get; set; } = "";
        [NotMapped]
        public IFormFile PhotoFile { get; set; }
        public string ProjectId { get; set; }
        public string UnitNumber { get; set; }
        [Display(Name = "Registered Vehicles")]
        public List<string> Vehicles { get; set; } = new List<string>();
    }
}