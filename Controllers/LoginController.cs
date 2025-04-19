using B_S_Skyline.Services;
using Firebase.Auth;
using Microsoft.AspNetCore.Mvc;

namespace B_S_Skyline.Controllers
{
    public class LoginController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            try
            {
                var auth = FirebaseAuthHelper.AuthClient;
                var result = await auth.SignInWithEmailAndPasswordAsync(email, password);

                var role = await FirebaseAuthHelper.GetUserRoleAsync(result.User.Uid) ?? "Unknown";

                HttpContext.Session.SetString("UserId", result.User.Uid);
                HttpContext.Session.SetString("UserEmail", email);
                HttpContext.Session.SetString("UserRole", role);

                var redirect = role switch
                {
                    "Owner" => RedirectToAction("Index", "UserManagement"),
                    "Resident" => RedirectToAction("Index", "Visits"),
                    "SecurityOfficer" => RedirectToAction("SelectProject", "Security"),
                    _ => RedirectToAction("Index", "Home")
                };
                return redirect;
            }
            catch
            {
                TempData["Error"] = "Invalid email or password.";
                return RedirectToAction("Index");
            }
        }
        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}