using B_S_Skyline.Filters;
using B_S_Skyline.Models;
using B_S_Skyline.Services;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.AspNetCore.Mvc;

namespace B_S_Skyline.Controllers
{
    //[TypeFilter(typeof(RoleFilter), Arguments = new object[] { "Owner" })]
    public class UserManagementController : Controller
    {
        private readonly FirebaseAuthClient _auth;
        private readonly FirebaseClient _db;

        public UserManagementController()
        {
            _auth = FirebaseAuthHelper.AuthClient;
            _db = FirebaseService.GetFirebaseClient();
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var users = (await _db.Child("Users").OnceAsync<UserModel>())
            .Select(u => new UserModel
            {
                Uid = u.Key,
                Email = u.Object.Email,
                Role = u.Object.Role,
                CreatedAt = u.Object.CreatedAt,
                IsActive = u.Object.IsActive
            })
            .OrderByDescending(u => u.CreatedAt)
            .ToList();

                return View(users);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading users";
                return View(new List<UserModel>());
            }
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View(new UserModel());
        }
        [HttpPost]
        public async Task<IActionResult> Create(UserModel user, string password)
        {
            try
            {
                var authUser = await _auth.CreateUserWithEmailAndPasswordAsync(user.Email, password);
                user.Uid = authUser.User.Uid;

                await _db.Child("Users")
                    .Child(user.Uid)
                    .PutAsync(user);

                await SetCustomClaims(user.Uid, user.Role);

                TempData["Success"] = $"{user.Role} user created successfully";
                return RedirectToAction("Index");
            }
            catch (FirebaseAuthException ex)
            {
                TempData["Error"] = $"Failed to create user: {ex.Message}";
                return View(user);
            }
        }
        private async Task SetCustomClaims(string uid, string role)
        {
            var claims = new Dictionary<string, object>
            {
                { "role", role},
                { "accountActive", true},
            };

            await FirebaseAdminHelper.SetCustomUserClaimsAsync(uid, claims);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                var user = await _db.Child("Users").Child(id).OnceSingleAsync<UserModel>();
                if (user == null)
                {
                    TempData["Error"] = "User not found";
                    return RedirectToAction("Index");
                }
                return View(user);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading user: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
        [HttpPost]
        public async Task<IActionResult> Edit(UserModel user)
        {
            try
            {
                var existingUser = await _db.Child("Users").Child(user.Uid).OnceSingleAsync<UserModel>();
                existingUser.Email = user.Email;
                existingUser.Role = user.Role;
                existingUser.BadgeNumber = user.BadgeNumber;
                existingUser.ResidentId = user.ResidentId;

                await _db.Child("Users").Child(user.Uid).PutAsync(existingUser);

                if (existingUser.Role != user.Role)
                {
                    await SetCustomClaims(user.Uid, user.Role);
                }
                TempData["Success"] = "User updated successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error updating user: {ex.Message}";
                return View(user);
            }
        }
        [HttpPost]
        public async Task<IActionResult> Deactivate(string uid)
        {
            try
            {
                await _db.Child("Users")
                    .Child(uid)
                    .PatchAsync(new { IsActive = false });

                await FirebaseAdminHelper.SetCustomUserClaimsAsync(uid, new Dictionary<string, object>
        {
            { "accountActive", false }
        });

                TempData["Success"] = "User deactivated successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deactivating user: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
        [HttpPost]
        public async Task<IActionResult> Activate(string uid)
        {
            try
            {
                await _db.Child("Users")
                    .Child(uid)
                    .PatchAsync(new { IsActive = true });
                await FirebaseAdminHelper.SetCustomUserClaimsAsync(uid, new Dictionary<string, object>
        {
            { "accountActive", true }
        });
                TempData["Success"] = "User activated successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error activating user: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
    }
}
