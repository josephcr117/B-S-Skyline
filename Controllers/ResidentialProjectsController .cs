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
            try
            {
                var parsedNumbers = houseNumbersInput.Split(',')
                    .Select(range =>
                    {
                        var parts = range.Split('-').Select(p => p.Trim()).ToList();
                        if (parts.Count == 2 && int.TryParse(parts[0], out int start) && int.TryParse(parts[1], out int end))
                        {
                            return Enumerable.Range(start, end - start + 1).Select(n => n.ToString()).ToList();
                        }
                        if (int.TryParse(range.Trim(), out int singleNumber))
                        {
                            return new List<string> { singleNumber.ToString() };
                        }
                        return null;
                    })
                    .Where(x => x != null)
                    .SelectMany(x => x) 
                    .ToList();

                if (!parsedNumbers.Any())
                {
                    ModelState.AddModelError("HouseNumbers", "Invalid house numbers format");
                    return View(project);
                }

                project.HouseNumbers = parsedNumbers;

                await _firebase.Child("projects").PostAsync(project);
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error creating project: {ex.Message}";
                return View(project);
            }
        }
        public async Task<IActionResult> Index(string search)
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
                })
                .ToList();

            if (!string.IsNullOrEmpty(search))
            {
                projects = projects.Where(p =>
                    p.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    p.Code.Contains(search, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

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
            try
            {
                var parsedNumbers = houseNumbersInput.Split(',')
                    .Select(range =>
                    {
                        var parts = range.Split('-').Select(p => p.Trim()).ToList();
                        if (parts.Count == 2 && int.TryParse(parts[0], out int start) && int.TryParse(parts[1], out int end))
                        {
                            return Enumerable.Range(start, end - start + 1).Select(n => n.ToString()).ToList();
                        }
                        if (int.TryParse(range.Trim(), out int singleNumber))
                        {
                            return new List<string> { singleNumber.ToString() };
                        }
                        return null;
                    })
                    .Where(x => x != null) 
                    .SelectMany(x => x) 
                    .ToList();

                if (!parsedNumbers.Any())
                {
                    ModelState.AddModelError("HouseNumbers", "Invalid house numbers format. Please use numbers or ranges.");
                    return View(project);
                }

                project.HouseNumbers = parsedNumbers;

                var client = FirebaseService.GetFirebaseClient();
                await client.Child("projects").Child(id).PutAsync(project);

                TempData["SuccessMessage"] = "Project updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating project: {ex.Message}";
                return View(project);
            }
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
                    .Child("Users")
                    .OnceAsync<UserModel>();
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
