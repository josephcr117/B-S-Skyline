using B_S_Skyline.Models;
using B_S_Skyline.Services;
using Firebase.Database;
using Microsoft.AspNetCore.Mvc;
using B_S_Skyline.ViewModels;
using Firebase.Database.Query;
using Microsoft.AspNetCore.Authorization;
using B_S_Skyline.Filters;

namespace B_S_Skyline.Controllers
{
    [TypeFilter(typeof(RoleFilter), Arguments = new object[] { "SecurityOfficer" })]
    public class SecurityController : Controller
    {
        private readonly FirebaseClient _firebase;

        public SecurityController() =>
            _firebase = FirebaseService.GetFirebaseClient();

        public async Task<IActionResult> Dashboard()
        {
            var model = new SecurityDashboardVM
            {
                Projects = await GetProjects(),
                ActiveVisits = await GetActiveVisits()
            };
            return View(model);
        }

        private async Task<List<ResidentialProject>> GetProjects()
        {
            return (await _firebase.Child("projects").OnceAsync<ResidentialProject>())
                .Select(p => new ResidentialProject { Id = p.Key, Name = p.Object.Name })
                .ToList();
        }

        private async Task<List<Visit>> GetActiveVisits(string projectId = null)
        {
            var visits = (await _firebase.Child("visits").OnceAsync<Visit>())
                .Where(v => v.Object.ExitTime == null)
                .Select(v => new Visit
                {
                    Id = v.Key,
                    VisitorName = v.Object.VisitorName,
                    LicensePlate = v.Object.LicensePlate,
                    EntryTime = v.Object.EntryTime,
                    ResidentId = v.Object.ResidentId
                }).ToList();

            return visits;
        }

        [HttpPost]
        public async Task<IActionResult> SetProject(string projectId)
        {
            TempData["ActiveProject"] = projectId;
            return RedirectToAction(nameof(Dashboard));
        }
        [HttpPost]
        public async Task<IActionResult> ApproveVisit(string visitId)
        {
            await _firebase
                .Child("visits")
                .Child(visitId)
                .PatchAsync(new { ApprovedBySecurity = true, ApprovalTime = DateTime.Now });

            return Json(new { success = true });
        }
        //public async Task<IActionResult> CheckResidentVehicle(string licensePlate)
        //{

        //    var cleanPlate = licensePlate?.Trim().Replace("-", "").Replace(" ", "").ToUpper();

        //    var residents = (await _firebase.Child("Users").OnceAsync<UserModel>())
        //        .Where(r => r.Object.Vehicles?.Any(v =>
        //        v.Split('|')[0]
        //        .Replace("-", "").Replace(" ", "").ToUpper() == cleanPlate) ?? false)
        //        .Select(r => new
        //        {
        //            r.Object.Name,
        //            r.Object.HouseNumber,
        //            VehicleInfo = r.Object.Vehicles?.FirstOrDefault(v =>
        //            v.StartsWith(licensePlate, StringComparison.OrdinalIgnoreCase))
        //        });
        //    return Json(residents);
        //}
    }
}