using System.ComponentModel.DataAnnotations;

namespace B_S_Skyline.Models
{
    public class SecurityOfficer
    {
        public string Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string BadgeNumber { get; set; }
        public string CurrentProjectId { get; set; }
    }
}
