using B_S_Skyline.Models;

namespace B_S_Skyline.ViewModels
{
    public class SecurityDashboardVM
    {
        public List<ResidentialProject> Projects { get; set; }
        public List<Visit> ActiveVisits { get; set; }
        public string ActiveProjectName { get; set; }
    }
}