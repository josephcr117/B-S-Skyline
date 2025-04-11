using B_S_Skyline.Services;
using LiteDB;

namespace B_S_Skyline.Models
{
    public class UserModel
    {
        public string uuid { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
    }

}