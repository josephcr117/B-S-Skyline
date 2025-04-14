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
                        UnitNumber = u.Object.UnitNumber,
                        ProjectId = u.Object.ProjectId,
                        BadgeNumber = u.Object.BadgeNumber,
                        IsActive = u.Object.IsActive,
                        CreatedAt = u.Object.CreatedAt
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
        public async Task<IActionResult> Create()
        {
            await LoadProjectsViewBag();
            return View(new UserModel());
        }
        [HttpPost]
        public async Task<IActionResult> Create(UserModel user, string password, string phoneNumbersInput)
        {
            try
            {
                if (string.IsNullOrEmpty(password) || password.Length < 6)
                {
                    ModelState.AddModelError("", "Password must be at least 6 characters");
                    await LoadProjectsViewBag();
                    return View(user);
                }

                FirebaseAdminHelper.Initialize();

                UserCredential authUser;
                try
                {
                    authUser = await _auth.CreateUserWithEmailAndPasswordAsync(user.Email, password);
                    user.Uid = authUser.User.Uid;
                    user.CreatedAt = DateTime.Now;
                    user.IsActive = true;

                    user.PhoneNumbers = phoneNumbersInput?
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(p => p.Trim())
                        .ToList() ?? new List<string>();

                    await _firebase
                        .Child("Users")
                        .Child(user.Uid)
                        .PutAsync(user);

                    await FirebaseAdminHelper.SetCustomUserClaimsAsync(user.Uid,
                        new Dictionary<string, object> { { "role", user.Role } });

                    TempData["Success"] = "User created successfully!";
                    return RedirectToAction("Index");
                }
                catch (FirebaseAuthException ex)
                {
                    TempData["Error"] = ex.Reason switch
                    {
                        AuthErrorReason.EmailExists => "Email already in use",
                        AuthErrorReason.InvalidEmailAddress => "Invalid email format",
                        _ => $"Authentication error: {ex.Message}"
                    };
                    await LoadProjectsViewBag();
                    return View(user);
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error creating user: {ex.Message}";
                await LoadProjectsViewBag();
                return View(user);
            }
        }
        [HttpGet]
        public async Task<IActionResult> EditResident(string id)
        {
            try
            {
                var user = await _firebase.Child("Users").Child(id).OnceSingleAsync<UserModel>();
                if (user == null || user.Role != "Resident")
                {
                    TempData["Error"] = "Resident not found.";
                    return RedirectToAction("Index");
                }
                await LoadProjectsViewBag();
                ViewBag.PhoneNumbers = string.Join(", ", user.PhoneNumbers);
                return View(user);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading resident: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
        [HttpPost]
        public async Task<IActionResult> EditResident(UserModel user, string phoneNumbersInput, string id)
        {
            try
            {
                if (string.IsNullOrEmpty(user.Dni))
                {
                    ModelState.AddModelError("Dni", "DNI is required for residents.");
                }
                if (!ModelState.IsValid)
                {
                    await LoadProjectsViewBag();
                    ViewBag.PhoneNumbers = phoneNumbersInput;
                    return View(user);
                }

                user.PhoneNumbers = phoneNumbersInput?
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => p.Trim())
                    .ToList() ?? new List<string>();

                await _firebase
                    .Child("Users")
                    .Child(id)
                    .PatchAsync(new
                    {
                        user.Name,
                        user.Dni,
                        user.ProjectId,
                        user.UnitNumber,
                        user.PhoneNumbers,
                        user.Vehicles
                    });

                TempData["Success"] = "Resident updated successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to update resident: {ex.Message}";
                await LoadProjectsViewBag();
                ViewBag.phoneNumbers = phoneNumbersInput;
                return View(user);
            }
        }
        [HttpGet]
        public async Task<IActionResult> EditOfficer(string id)
        {
            try
            {
                var user = await _firebase.Child("Users").Child(id).OnceSingleAsync<UserModel>();
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
        public async Task<IActionResult> EditOfficer(string id, UserModel user)
        {
            try
            {
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
                await _firebase.Child("Users").Child(uid).PatchAsync(new { IsActive = isActive });
                await SetCustomClaims(uid, null, isActive);

                TempData["Success"] = $"User {(isActive ? "activated" : "deactivated")}";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error updating status: {ex.Message}";
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
