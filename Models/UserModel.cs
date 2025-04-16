using B_S_Skyline.Services;
using LiteDB;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace B_S_Skyline.Models
{
    public class VehicleModel
    {
        public string LicensePlate { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
    }
    public class UserModel
    {
        public string Uid { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;
        public string Name { get; set; }
        public string Dni { get; set; }
        public List<string> PhoneNumbers { get; set; } = new();
        public string ProjectId { get; set; }
        public string HouseNumber { get; set; }
        public Dictionary<string, VehicleModel> Vehicles { get; set; } = new();
        [Newtonsoft.Json.JsonIgnore]
        public List<VehicleModel> VehiclesList => Vehicles?.Values.ToList() ?? new List<VehicleModel>();
        public string PhotoUrl { get; set; } = "";
        public string BadgeNumber { get; set; }
        public string CurrentProjectId { get; set; }
    }
}