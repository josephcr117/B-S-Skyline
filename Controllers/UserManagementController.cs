using B_S_Skyline.Filters;
using B_S_Skyline.Models;
using B_S_Skyline.Services;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace B_S_Skyline.Controllers
{
    [TypeFilter(typeof(RoleFilter), Arguments = new object[] { "Owner" })]
    public class UserManagementController : Controller
    {
        private readonly FirebaseAuthClient _auth;
        private readonly FirebaseClient _firebase;

        public UserManagementController()
        {
            FirebaseAdminHelper.Initialize();
            _auth = FirebaseAuthHelper.AuthClient;
            _firebase = FirebaseService.GetFirebaseClient();
        }
        public async Task<IActionResult> Index()
        {
            try
            {
                var users = (await _firebase.Child("Users").OnceAsync<UserModel>())
                    .Select(u => new UserModel
                    {
                        Uid = u.Key,
                        Email = u.Object.Email,
                        Role = u.Object.Role,
                        Name = u.Object.Name,
                        Dni = u.Object.Dni,
                        HouseNumber = u.Object.HouseNumber,
                        ProjectId = u.Object.ProjectId,
                        BadgeNumber = u.Object.BadgeNumber,
                        IsActive = u.Object.IsActive,
                        CreatedAt = u.Object.CreatedAt,
                        Vehicles = u.Object.Vehicles ?? new Dictionary<string, VehicleModel>()
                    })
                    .OrderByDescending(u => u.CreatedAt)
                    .ToList();
                return View(users);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading users: " + ex.Message;
                return View(new List<UserModel>());
            }
        }
        [HttpGet]
        public async Task<IActionResult> CreateResident()
        {
            ViewBag.Projects = (await _firebase
                .Child("projects")
                .OnceAsync<ResidentialProject>())
                .Select(p => new SelectListItem
                {
                    Value = p.Key,
                    Text = p.Object.Name
                }).ToList();

            ViewBag.Units = new List<SelectListItem>();
            return View(new UserModel());
        }
        [HttpPost]
        public async Task<IActionResult> CreateResident(UserModel resident, string password, string phoneNumbersInput)
        {
            try
            {
                if (string.IsNullOrEmpty(password) || password.Length < 6)
                {
                    ModelState.AddModelError("", "Password must be at least 6 characters");
                    await LoadProjectsViewBag();
                    return View(resident);
                }

                var existingUsers = (await _firebase
                    .Child("Users")
                    .OnceAsync<UserModel>())
                    .Where(u => u.Object.ProjectId == resident.ProjectId);

                //if (existingUsers.Any(u => u.Object.Email == resident.Email))
                //{
                //    ModelState.AddModelError("Email", "Email already exists in this project.");
                //    await LoadProjectsViewBag();
                //    return View(resident);
                //}
                //if (existingUsers.Any(u => u.Object.Dni == resident.Dni))
                //{
                //    ModelState.AddModelError("Dni", "ID already exists in this project.");
                //    await LoadProjectsViewBag();
                //    return View(resident);
                //}
                if (existingUsers.Any(u => u.Object.HouseNumber == resident.HouseNumber))
                {
                    ModelState.AddModelError("HouseNumber", "House is already taken. Please pick another.");
                    await LoadProjectsViewBag();
                    return View(resident);
                }

                resident.Role = "Resident";
                resident.CreatedAt = DateTime.Now;
                resident.IsActive = true;
                resident.PhoneNumbers = phoneNumbersInput?
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => p.Trim())
                    .ToList() ?? new List<string>();

                FirebaseAdminHelper.Initialize();
                var authUser = await _auth.CreateUserWithEmailAndPasswordAsync(resident.Email, password);
                resident.Uid = authUser.User.Uid;

                await _firebase
                    .Child("Users")
                    .Child(resident.Uid)
                    .PutAsync(resident);

                await FirebaseAdminHelper.SetCustomUserClaimsAsync(resident.Uid, new Dictionary<string, object> { { "role", resident.Role } });
                TempData["Success"] = "User created successfully!";
                return RedirectToAction("Index");

            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error creating user: {ex.Message}";
                await LoadProjectsViewBag();
                return View(resident);
            }
        }
        [HttpGet]
        public IActionResult CreateSecurityOfficer()
        {
            return View(new UserModel { Role = "Security Officer" });
        }
        [HttpPost]
        public async Task<IActionResult> CreateSecurityOfficer(UserModel officer, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(password) || password.Length < 6)
                {
                    ModelState.AddModelError("", "Password must be at least 6 characters");
                    return View(officer);
                }
                var existingUsers = await _firebase
                    .Child("Users")
                    .OnceAsync<UserModel>();

                if (existingUsers.Any(u => u.Object.Email == officer.Email))
                {
                    ModelState.AddModelError("Email", "Email is already in use");
                    return View(officer);
                }
                if (existingUsers.Any(u => u.Object.BadgeNumber == officer.BadgeNumber))
                {
                    ModelState.AddModelError("BadgeNumber", "Badge number already exists");
                    return View(officer);
                }
                officer.Role = "SecurityOfficer";
                officer.CreatedAt = DateTime.Now;
                officer.IsActive = true;

                FirebaseAdminHelper.Initialize();
                var authUser = await _auth.CreateUserWithEmailAndPasswordAsync(officer.Email, password);
                officer.Uid = authUser.User.Uid;

                await _firebase
                    .Child("Users")
                    .Child(officer.Uid)
                    .PutAsync(officer);

                TempData["Success"] = "Security officer created successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error creating security officer: {ex.Message}";
                return View(officer);
            }
        }
        [HttpGet]
        public async Task<IActionResult> EditResident(string id)
        {
            try
            {
                var user = await _firebase
                    .Child("Users")
                    .Child(id)
                    .OnceSingleAsync<UserModel>();
                if (user == null || user.Role != "Resident")
                {
                    TempData["Error"] = "Resident not found";
                    return RedirectToAction("Index");
                }
                await LoadProjectsViewBag();
                ViewBag.PhoneNumbers = string.Join(", ", user.PhoneNumbers ?? new List<string>());
                return View(user);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading resident: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
        [HttpPost]
        public async Task<IActionResult> EditResident(string id, UserModel user, string phoneNumbersInput)
        {
            try
            {
                ModelState.Clear();

                if (string.IsNullOrEmpty(user.Name))
                    ModelState.AddModelError("Name", "Name is required");

                if (string.IsNullOrEmpty(user.Dni))
                    ModelState.AddModelError("Dni", "ID is required");

                if (string.IsNullOrEmpty(user.ProjectId))
                    ModelState.AddModelError("ProjectId", "Project is required");

                if (string.IsNullOrEmpty(user.HouseNumber))
                    ModelState.AddModelError("HouseNumber", "Unit number is required");

                if (!ModelState.IsValid)
                {
                    await LoadProjectsViewBag();
                    ViewBag.PhoneNumbers = phoneNumbersInput ?? "";
                    return View(user);
                }

                var phoneNumbers = phoneNumbersInput?
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => p.Trim())
                    .ToList() ?? new List<string>();

                var updates = new
                {
                    Name = user.Name,
                    Dni = user.Dni,
                    ProjectId = user.ProjectId,
                    HouseNumber = user.HouseNumber,
                    PhoneNumbers = phoneNumbers
                };

                await _firebase
                    .Child("Users")
                    .Child(id)
                    .PatchAsync(updates);

                TempData["Success"] = "Resident updated successfully!";
                return RedirectToAction("Index");
            }
            catch (FirebaseException ex)
            {
                TempData["Error"] = $"Database error: {ex.Message}";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Unexpected error: {ex.Message}";
            }

            await LoadProjectsViewBag();
            ViewBag.PhoneNumbers = phoneNumbersInput ?? "";
            return View(user);
        }
        [HttpGet]
        public async Task<IActionResult> EditSecurityOfficer(string id)
        {
            try
            {
                var user = await _firebase
                    .Child("Users")
                    .Child(id)
                    .OnceSingleAsync<UserModel>();
                if (user == null || user.Role != "SecurityOfficer")
                {
                    TempData["Error"] = "Officer not found";
                    return RedirectToAction("Index");
                }

                await LoadProjectsViewBag();
                return View(user);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading officer: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
        [HttpPost]
        public async Task<IActionResult> EditSecurityOfficer(string id, UserModel user)
        {
            try
            {
                ModelState.Clear();
                if (string.IsNullOrEmpty(user.Name))
                {
                    ModelState.AddModelError("Name", "Name is required");
                }

                if (string.IsNullOrEmpty(user.BadgeNumber))
                {
                    ModelState.AddModelError("BadgeNumber", "Badge number is required");
                }

                if (!ModelState.IsValid)
                {
                    await LoadProjectsViewBag();
                    return View(user);
                }

                await _firebase.Child("Users").Child(id).PatchAsync(new
                {
                    user.Name,
                    user.BadgeNumber,
                    user.CurrentProjectId
                });

                TempData["Success"] = "Officer updated successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error updating officer: {ex.Message}";
                await LoadProjectsViewBag();
                return View(user);
            }
        }
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(string uid, bool isActive)
        {
            try
            {
                var user = await _firebase.Child("Users").Child(uid).OnceSingleAsync<UserModel>();
                bool newStatus = !user.IsActive;
                FirebaseAdminHelper.Initialize();

                await _firebase.Child("Users").Child(uid).PatchAsync(new { IsActive = newStatus });
                await SetCustomClaims(uid, null, newStatus);

                TempData["Success"] = $"User {(newStatus ? "activated" : "deactivated")}";
                return RedirectToAction("Index");
            }
            catch
            {
                return RedirectToAction("Index");
            }
        }
        [HttpPost]
        public async Task<IActionResult> Delete(string uid)
        {
            var user = await _firebase.Child("Users").Child(uid).OnceSingleAsync<UserModel>();
            if (user == null)
            {
                TempData["Error"] = "User not found";
                return RedirectToAction("Index");
            }
            try
            {
                await _firebase.Child("Users").Child(uid).DeleteAsync();
                await FirebaseAdminHelper.DeleteUserAsync(uid);
                TempData["Success"] = $"User {user.Name} (Email: {user.Email}) has been successfully deleted.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting user: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
        private async Task SetCustomClaims(string uid, string role = null, bool? isActive = null)
        {
            await FirebaseAdminHelper.UpdateCustomClaimsAsync(uid, claims =>
            {
                if (role != null) claims["role"] = role;
                if (isActive != null) claims["accountActive"] = isActive.Value;
            });
        }

        private async Task LoadProjectsViewBag()
        {
            ViewBag.Projects = (await _firebase.Child("projects").OnceAsync<ResidentialProject>())
                .Select(p => new SelectListItem
                {
                    Value = p.Key,
                    Text = $"{p.Object.Name} - {p.Object.Code}"
                }).ToList();
        }
    }
}
