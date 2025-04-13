using B_S_Skyline.Services;
using LiteDB;
using System.ComponentModel.DataAnnotations;

namespace B_S_Skyline.Models
{
    public class UserModel
    {
        public string Uid { get; set; }
        [Required, EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Role { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;

        public string? ResidentId { get; set; }
        public string? BadgeNumber { get; set; }
    }

}