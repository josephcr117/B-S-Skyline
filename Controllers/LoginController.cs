using B_S_Skyline.Services;
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

                var role = FirebaseAuthHelper.UserRoles[email.ToLower()];

                HttpContext.Session.SetString("UserId", result.User.Uid);
                HttpContext.Session.SetString("UserEmail", email);
                HttpContext.Session.SetString("UserRole", role);

                return role switch
                {
                    "Owner" => RedirectToAction("Index", "ResidentialProjects"),
                    "Resident" => RedirectToAction("Index", "Visits"),
                    "SecurityOfficer" => RedirectToAction("Dashboard", "Security"),
                    _ => RedirectToAction("Index", "Home")
                };
            }
            catch (Exception)
            {
                TempData["Error"] = "Invalid email or password.";
                return RedirectToAction("Index");
            }
        }
        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}