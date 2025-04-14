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
        private readonly string _currentUserId;
        public VisitsController(IHttpContextAccessor httpContextAccessor)
        {
            _firebase = FirebaseService.GetFirebaseClient();
            _currentUserId = httpContextAccessor.HttpContext?.Session.GetString("UserId");
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var allVisits = await _firebase
                    .Child("visits")
                    .OnceAsync<Visit>();

                var visits = allVisits
                    .Where(v => v.Object.ResidentId == _currentUserId)
                    .OrderByDescending(v => v.Object.EntryTime)
                    .Select(v => new Visit
                    {
                        Id = v.Key,
                        VisitorName = v.Object.VisitorName,
                        EntryTime = v.Object.EntryTime,
                        LicensePlate = v.Object.LicensePlate,
                        IsDelivery = v.Object.IsDelivery,
                        DeliveryService = v.Object.DeliveryService
                    })
                    .ToList();

                return View(visits);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading visits. Please try again.";
                return View(new List<Visit>());
            }
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View(new Visit { ResidentId = _currentUserId });
        }
        [HttpPost]
        public async Task<IActionResult> Create(Visit visit)
        {
            try
            {
                visit.IsDelivery = visit.DeliveryService != Visit.DeliveryServiceType.Other && visit.DeliveryService != default(Visit.DeliveryServiceType);

                visit.EntryTime = DateTime.Now;
                visit.ResidentId = _currentUserId;

                await _firebase.Child("visits").PostAsync(visit);

                TempData["SuccessMessage"] = "Visit created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while creating the visit: {ex.Message}";
                return View(visit);
            }
        }
        [HttpGet]
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
                visit.IsDelivery = visit.DeliveryService != Visit.DeliveryServiceType.Other && visit.DeliveryService != default(Visit.DeliveryServiceType);

                await _firebase
                    .Child("visits")
                    .Child(id)
                    .PutAsync(visit);

                TempData["SuccessMessage"] = "Visit updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while updating the visit: {ex.Message}";
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
        [HttpPost]
        public async Task<IActionResult> ToggleFavorite(string id)
        {
            try
            {
                var visit = await _firebase
                    .Child("visits")
                    .Child(id)
                    .OnceSingleAsync<Visit>();

                if (visit == null) return NotFound();

                visit.IsFavorite = !visit.IsFavorite;
                await _firebase.Child("visits").Child(id).PutAsync(visit);

                if (visit.IsFavorite)
                {
                    var favorite = new FavoriteVisitor
                    {
                        ResidentId = _currentUserId,
                        VisitorName = visit.VisitorName,
                        LicensePlate = visit.LicensePlate,
                        IsDeliveryContact = visit.IsDelivery,
                        DeliveryService = visit.DeliveryService
                    };
                    await _firebase.Child("favoriteVisitors").PostAsync(favorite);
                }
                else
                {
                    var favorites = await _firebase
                        .Child("favoriteVisitors")
                        .OrderBy("ResidentId")
                        .EqualTo(_currentUserId)
                        .OnceAsync<FavoriteVisitor>();

                    var toDelete = favorites
                        .Where(f => f.Object.VisitorName == visit.VisitorName)
                        .ToList();

                    foreach (var fav in toDelete)
                    {
                        await _firebase
                            .Child("favoriteVisitors")
                            .Child(fav.Key)
                            .DeleteAsync();
                    }
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error updating favorite: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
        public async Task<IActionResult> Favorites()
        {
            try
            {
                var allFavorites = await _firebase
                    .Child("favoriteVisitors")
                    .OnceAsync<FavoriteVisitor>();

                var userFavorites = allFavorites
                    .Where(f => f.Object.ResidentId == _currentUserId)
                    .OrderBy(f => f.Object.VisitorName)
                    .Select(f => new FavoriteVisitor
                    {
                        Id = f.Key,
                        VisitorName = f.Object.VisitorName,
                        LicensePlate = f.Object.LicensePlate,
                        IsDeliveryContact = f.Object.IsDeliveryContact,
                        DeliveryService = f.Object.DeliveryService,
                        CreatedAt = f.Object.CreatedAt
                    }).ToList();
                return View(userFavorites);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading favorites: {ex.Message}";
                return View(new List<FavoriteVisitor>());
            }
        }

        public async Task<IActionResult> EditFavorite(string id)
        {
            var favorite = await _firebase
                .Child("favoriteVisitors")
                .Child(id)
                .OnceSingleAsync<FavoriteVisitor>();

            if (favorite == null) return NotFound();

            return View(favorite);
        }

        [HttpPost]
        public async Task<IActionResult> EditFavorite(string id, FavoriteVisitor favorite)
        {
            try
            {
                await _firebase
                    .Child("favoriteVisitors")
                    .Child(id)
                    .PutAsync(favorite);

                TempData["Success"] = "Favorite updated successfully";
                return RedirectToAction(nameof(Favorites));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error updating favorite: {ex.Message}";
                return View(favorite);
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteFavorite(string id)
        {
            try
            {
                await _firebase
                    .Child("favoriteVisitors")
                    .Child(id)
                    .DeleteAsync();

                TempData["Success"] = "Favorite removed successfully";
                return RedirectToAction(nameof(Favorites));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error removing favorite: {ex.Message}";
                return RedirectToAction(nameof(Favorites));
            }
        }
    }
}
