using B_S_Skyline.Filters;
using B_S_Skyline.Models;
using B_S_Skyline.Services;
using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace B_S_Skyline.Controllers
{
    [TypeFilter(typeof(RoleFilter), Arguments = new object[] { "Owner" })]
    public class ResidentsController : Controller
    {
        public readonly FirebaseClient _firebase;
        public ResidentsController()
        {
            _firebase = FirebaseService.GetFirebaseClient();
        }
        public async Task<IActionResult> Index()
        {
            var residents = (await _firebase
                .Child("residents")
                .OnceAsync<Resident>())
                .Select(item => new Resident
                {
                    Id = item.Key,
                    Dni = item.Object.Dni,
                    Name = item.Object.Name,
                    ProjectId = item.Object.ProjectId,
                    UnitNumber = item.Object.UnitNumber
                }).ToList();

            return View(residents);
        }
        public async Task<IActionResult> Create()
        {
            try
            {
                var resident = new Resident { PhotoUrl = "" };

                var projects = (await _firebase
                    .Child("projects")
                    .OnceAsync<ResidentialProject>())
                    .Select(p => new SelectListItem
                    {
                        Value = p.Key,
                        Text = $"{p.Object.Name} - {p.Object.Code}"
                    }).ToList();
                ViewBag.Projects = projects;
                return View(resident);
            }
            catch (Exception ex)
            {
                ViewBag.Projects = new List<SelectListItem>();
                return View();
            }
        }
        [HttpPost]
        public async Task<IActionResult> Create(Resident resident, string phoneNumbersInput)
        {
            //if (resident.PhotoFile != null && resident.PhotoFile.Length > 0)
            //{
            //    resident.PhotoUrl = await UploadPhoto(resident.PhotoFile, resident.Dni);
            //}

            resident.PhoneNumbers = phoneNumbersInput?
            .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim())
            .ToList() ?? new List<string>();
            await _firebase
                .Child("residents")
                .PostAsync(resident);
            return RedirectToAction(nameof(Index));
        }
        //private async Task<string> UploadPhoto(IFormFile file, string dni)
        //{
        //    var storage = FirebaseService.GetFirebaseStorage();
        //    var fileName = $"residents/{dni}_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(file.FileName)}";

        //    using (var stream = file.OpenReadStream())
        //    {
        //        var task = storage
        //            .Child(fileName)
        //            .PutAsync(stream);
        //        return await task;
        //    }
        //}
        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                var resident = await _firebase
                    .Child("residents")
                    .Child(id)
                    .OnceSingleAsync<Resident>();

                resident.PhotoUrl ??= "";

                if (resident == null) return NotFound();
                resident.Id = id;
                var projects = (await _firebase
                    .Child("projects")
                    .OnceAsync<ResidentialProject>())
                    .Select(p => new SelectListItem
                    {
                        Value = p.Key,
                        Text = $"{p.Object.Name} - {p.Object.Code}",
                        Selected = p.Key == resident.ProjectId
                    }).ToList();
                ViewBag.Projects = projects;
                ViewBag.PhoneNumbersInput = string.Join(", ", resident.PhoneNumbers);
                return View();
            }
            catch
            {
                return RedirectToAction(nameof(Index));
            }
        }
        [HttpPost]
        public async Task<IActionResult> Edit(string id, Resident resident, string phoneNumbersInput)
        {
            try
            {
                var originalResident = await _firebase
                .Child("residents")
                .Child(id)
                .OnceSingleAsync<Resident>();

                resident.Dni = originalResident.Dni;

                resident.PhoneNumbers = phoneNumbersInput?
                    .Split(',')
                    .Select(p => p.Trim())
                    .ToList() ?? new List<string>();

                await _firebase
                    .Child("residents")
                    .Child(id)
                    .PutAsync(resident);

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                var projects = (await _firebase
                    .Child("projects")
                    .OnceAsync<ResidentialProject>())
                    .Select(p => new SelectListItem
                    {
                        Value = p.Key,
                        Text = $"{p.Object.Name} - {p.Object.Code}"
                    }).ToList();

                ViewBag.Projects = projects;
                return View(resident);
            }
        }
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var resident = await _firebase
                    .Child("residents")
                    .Child(id)
                    .OnceSingleAsync<Resident>();

                if (resident == null) return NotFound();

                if (!string.IsNullOrEmpty(resident.ProjectId))
                {
                    var project = await _firebase
                        .Child("residents")
                        .Child(id)
                        .OnceSingleAsync<ResidentialProject>();
                    ViewBag.ProjectName = project?.Name;
                }
                return View(new Resident
                {
                    Id = id,
                    Dni = resident.Dni,
                    Name = resident.Name,
                    UnitNumber = resident.UnitNumber,
                });
            }
            catch
            {
                TempData["ErrorMessage"] = "Error loading resident data";
                return RedirectToAction(nameof(Index));
            }
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                //var hasVisits = (await _firebase
                //    .Child("visits")
                //    .OnceAsync<Visit>())
                //    .Any(v => v.Object.ResidentId == id);

                //if (hasVisits)
                //{
                //    TempData["ErrorMessage"] = "Cannot delete - Resident has visit history";
                //    return RedirectToAction(nameof(Index));
                //}

                await _firebase
                    .Child("residents")
                    .Child(id)
                    .DeleteAsync();
                TempData["SuccessMessage"] = "Resident deleted successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Delete Failed: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
        public async Task<IActionResult> Details(string id)
        {
            try
            {
                var resident = await _firebase
                .Child("residents")
                .Child(id)
                .OnceSingleAsync<Resident>();

                if (resident == null) return NotFound();

                if (!string.IsNullOrEmpty(resident.ProjectId))
                {
                    var project = await _firebase
                        .Child("projects")
                        .Child(resident.ProjectId)
                        .OnceSingleAsync<ResidentialProject>();

                    ViewBag.ProjectName = project?.Name;
                    ViewBag.ProjectCode = project?.Code;
                }
                return View(resident);
            }
            catch
            {
                TempData["ErrorMessage"] = "Error loading resident details";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
