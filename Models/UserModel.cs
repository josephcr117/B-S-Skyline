using B_S_Skyline.Services;
using LiteDB;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace B_S_Skyline.Models
{
    public class UserModel
    {
        public string Uid { get; set; }
        [DisplayName("Email Address")]
        [Required]
        public string Email { get; set; }
        [Required]
        public string Role { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;

        [DisplayName("Full Name")]
        [Required]
        public string Name { get; set; }
        [Required]
        [DisplayName("ID")]
        public string Dni { get; set; }
        [Required]
        [DisplayName("Phone Numbers")]
        public List<string> PhoneNumbers { get; set; } = new();
        [Required]
        public string ProjectId { get; set; }
        [DisplayName("House or Unit Number")]
        [Required]
        public string UnitNumber { get; set; }
        public List<string> Vehicles { get; set; } = new();
        public string PhotoUrl { get; set; } = "";
        [DisplayName("Badge Number")]
        [Required]
        public string BadgeNumber { get; set; }
        [Required]
        public string CurrentProjectId { get; set; }
    }
}