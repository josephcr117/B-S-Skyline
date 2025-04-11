using B_S_Skyline.Filters;
using B_S_Skyline.Models;
using B_S_Skyline.Services;
using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace B_S_Skyline.Controllers
{
    [TypeFilter(typeof(RoleFilter), Arguments = new object[] { "Resident" })]
    public class VisitsController : Controller
    {
        private readonly FirebaseClient _firebase;
        public VisitsController() =>
            _firebase = FirebaseService.GetFirebaseClient();

        public async Task<IActionResult> Index()
        {
            var visits = (await _firebase
            .Child("visits")
            .OnceAsync<Visit>())
            .Select(v => new Visit
            {
                Id = v.Key,
                VisitorName = v.Object.VisitorName,
                EntryTime = v.Object.EntryTime,
                ResidentId = v.Object.ResidentId,
                LicensePlate = v.Object.LicensePlate,
            }).ToList();
            return View(visits);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(Visit visit)
        {
            try
            {

                visit.IsDelivery = visit.DeliveryService != default;

                visit.EntryTime = DateTime.Now;
                await _firebase.Child("visits").PostAsync(visit);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View(visit);
            }
        }

        public async Task<IActionResult> Edit(string id)
        {
            var visit = await _firebase
                .Child("visits")
                .Child(id)
                .OnceSingleAsync<Visit>();

            if (visit == null) return NotFound();
            visit.Id = id;
            return View(visit);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(string id, Visit visit)
        {
            try
            {
                visit.IsDelivery = visit.DeliveryService != default;

                await _firebase
                    .Child("visits")
                    .Child(id)
                    .PutAsync(visit);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View(visit);
            }
        }

        public async Task<IActionResult> Delete(string id)
        {
            var visit = await _firebase
                .Child("visits")
                .Child(id)
                .OnceSingleAsync<Visit>();

            if (visit == null) return NotFound();
            return View(new Visit
            {
                Id = id,
                VisitorName = visit.VisitorName,
                EntryTime = visit.EntryTime,
                ResidentId = visit.ResidentId,
                LicensePlate = visit.LicensePlate,
            });
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                await _firebase
                    .Child("visits")
                    .Child(id)
                    .DeleteAsync();
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
        public async Task<IActionResult> Details(string id)
        {
            var visit = await _firebase
                .Child("visits")
                .Child(id)
                .OnceSingleAsync<Visit>();
            if (visit == null) return NotFound();

            if (!string.IsNullOrEmpty(visit.ResidentId))
            {
                var resident = (await _firebase
                    .Child("residents")
                    .Child(visit.ResidentId)
                    .OnceSingleAsync<Resident>());
                ViewBag.ResidentName = resident?.Name;
            }
            visit.Id = id;
            return View(visit);
        }
    }
}
