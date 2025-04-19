using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace B_S_Skyline.ViewModels
{
    public class ProjectSelectionVM
    {
        public List<SelectListItem> Projects { get; set; } = new List<SelectListItem>();
    }
}