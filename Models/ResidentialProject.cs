namespace B_S_Skyline.Models
{
    public class ResidentialProject
    {
        public string Id { get; set; }
        public string LogoUrl { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string OfficePhone { get; set; }
        public List<string> HouseNumbers { get; set; } = new List<string>();
        public UnitRange UnitRange { get; set; }
    }
    public class UnitRange
    {
        public int Min { get; set; }
        public int Max { get; set; }
    }
}
