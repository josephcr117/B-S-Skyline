using B_S_Skyline.Filters;
using B_S_Skyline.Models;
using B_S_Skyline.Services;
using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.InteropServices;

namespace B_S_Skyline.Controllers
{
    [TypeFilter(typeof(RoleFilter), Arguments = new object[] { "Owner" })]
    public class ResidentialProjectsController : Controller
    {
        private readonly FirebaseClient _firebase;
        public ResidentialProjectsController()
        {
            _firebase = FirebaseService.GetFirebaseClient();
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(ResidentialProject project, string houseNumbersInput)
        {
            project.HouseNumbers = houseNumbersInput.Split(',')
                .Select(x => x.Trim())
                .ToList();

            var client = FirebaseService.GetFirebaseClient();
            await client.Child("projects").PostAsync(project);
            return RedirectToAction("Index", "Home");
        }
        public async Task<IActionResult> Index()
        {
            var client = FirebaseService.GetFirebaseClient();
            var projects = (await client
                .Child("projects")
                .OnceAsync<ResidentialProject>())
                .Select(item => new ResidentialProject
                {
                    Id = item.Key,
                    Name = item.Object.Name,
                    Code = item.Object.Code,
                    Address = item.Object.Address,
                    OfficePhone = item.Object.OfficePhone,
                    HouseNumbers = item.Object.HouseNumbers
                }).ToList();
            return View(projects);
        }
        public async Task<IActionResult> Edit(string id)
        {
            var client = FirebaseService.GetFirebaseClient();
            var project = await client
                .Child("projects")
                .Child(id)
                .OnceSingleAsync<ResidentialProject>();

            project.Id = id;
            return View(project);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string id, ResidentialProject project, string houseNumbersInput)
        {
            project.HouseNumbers = houseNumbersInput.Split(',')
                                  .Select(x => x.Trim())
                                  .ToList();

            var client = FirebaseService.GetFirebaseClient();
            await client
                .Child("projects")
                .Child(id)
                .PutAsync(project);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var project = await _firebase
                    .Child("projects")
                    .Child(id)
                    .OnceSingleAsync<ResidentialProject>();
                if (project == null)
                {
                    return NotFound();
                }
                project.Id = id;
                return View(project);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading project: {ex.Message}";
                return RedirectToAction(nameof (Index));
            }
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                var residents = await _firebase
                    .Child("residents")
                    .OnceAsync<Resident>();
                if (residents.Any(r => r.Object.ProjectId == id))
                {
                    TempData["ErrorMessage"] = "Cannot delete - Project has assisgned residents";
                    return RedirectToAction(nameof(Index));
                }
                await _firebase
                    .Child("projects")
                    .Child(id)
                    .DeleteAsync();
                TempData["SuccessMessage"] = "Project deleted successfully";
                return RedirectToAction(nameof(Index));
            }
            catch(Exception ex) 
            {
                TempData["ErrorMessage"] = $"Error deleting project: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
