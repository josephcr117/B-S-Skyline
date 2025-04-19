using B_S_Skyline.Models;
using B_S_Skyline.Services;
using Firebase.Database;
using Microsoft.AspNetCore.Mvc;
using B_S_Skyline.ViewModels;
using Firebase.Database.Query;
using B_S_Skyline.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;

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
            var activeProjectId = TempData["ActiveProject"]?.ToString();

            if (string.IsNullOrEmpty(activeProjectId))
            {
                TempData["Error"] = "Please select a project first.";
                return RedirectToAction("SelectProject");
            }

            var projectSnapshot = await _firebase.Child("projects").Child(activeProjectId).OnceSingleAsync<ResidentialProject>();
            var activeProjectName = projectSnapshot?.Name ?? "Unknown Project";

            Console.WriteLine($"Debug: Active Project Name = {activeProjectName}");

            var model = new SecurityDashboardVM
            {
                ActiveVisits = await GetActiveVisits(activeProjectId),
                ActiveProjectName = activeProjectName 
            };

            return View(model);
        }
        private async Task<List<Visit>> GetActiveVisits(string projectId)
        {
            var allVisits = await _firebase.Child("visits").OnceAsync<Visit>();

            var visits = allVisits
                .Where(v => v.Object.ProjectId == projectId &&
                            v.Object.ExitTime == null &&
                            (v.Object.ApprovedBySecurity == false || v.Object.ApprovedBySecurity == null))
                .Select(v => new Visit
                {
                    Id = v.Key,
                    VisitorName = v.Object.VisitorName,
                    LicensePlate = v.Object.LicensePlate,
                    EntryTime = v.Object.EntryTime,
                    ResidentId = v.Object.ResidentId,
                    ProjectId = v.Object.ProjectId
                })
                .ToList();

            return visits;
        }
        [HttpPost]
        public async Task<IActionResult> SetProject(string projectId)
        {
            var userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(projectId))
            {
                TempData["Error"] = "Invalid user or project selection.";
                Console.WriteLine($"Debug: userId = {userId}, projectId = {projectId}");
                return RedirectToAction("SelectProject");
            }

            var projectSnapshot = await _firebase.Child("projects").Child(projectId).OnceSingleAsync<ResidentialProject>();
            var project = projectSnapshot ?? new ResidentialProject();

            if (projectSnapshot == null)
            {
                TempData["Error"] = "Project not found in the database.";
                return RedirectToAction("SelectProject");
            }

            if (project.SecurityOfficers == null)
            {
                project.SecurityOfficers = new List<string>();
            }

            if (!project.SecurityOfficers.Contains(userId))
            {
                project.SecurityOfficers.Add(userId);
                try
                {
                    await _firebase.Child("projects").Child(projectId).Child("SecurityOfficers").PutAsync(project.SecurityOfficers);
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Failed to update project: {ex.Message}";
                    return RedirectToAction("SelectProject");
                }
            }

            TempData["ActiveProject"] = projectId; 
            TempData["Success"] = "Project assigned successfully!";
            return RedirectToAction("Dashboard");
        }
        public IActionResult SelectProject()
        {
            TempData["ActiveProject"] = null;

            var projectsSnapshot = _firebase.Child("projects").OnceAsync<ResidentialProject>().Result; 
            var projects = projectsSnapshot.Select(p => new SelectListItem
            {
                Value = p.Key, 
                Text = p.Object.Name 
            }).ToList();

            var model = new ProjectSelectionVM
            {
                Projects = projects
            };

            return View(model); 
        }
        [HttpPost]
        public async Task<IActionResult> ApproveVisit(string visitId)
        {
            if (string.IsNullOrEmpty(visitId))
                return Json(new { success = false, error = "Visit ID is required." });

            try
            {
                await _firebase
                    .Child("visits")
                    .Child(visitId)
                    .PatchAsync(new
                    {
                        ApprovedBySecurity = true,
                        ApprovalTime = DateTime.Now
                    });

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }
    }
}