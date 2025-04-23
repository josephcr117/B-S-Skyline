using Firebase.Auth.Providers;
using Firebase.Auth;
using Firebase.Database.Query;

namespace B_S_Skyline.Services
{
    public static class FirebaseAuthHelper
    {
        public const string firebaseAppId = "bsskyline-dc061";
        public const string firebaseApiKey = "AIzaSyDuU3-cIPSct_TcOL2CUyunYkMLcWqpIBQ";

        public static FirebaseAuthClient AuthClient => new(new FirebaseAuthConfig()
        {
            ApiKey = firebaseApiKey,
            AuthDomain = $"{firebaseAppId}.firebaseapp.com",
            Providers = new FirebaseAuthProvider[] { new EmailProvider() }
        });

        public static async Task CreateUser(string email, string password, string role)
        {
            var auth = AuthClient;
            var user = await auth.CreateUserWithEmailAndPasswordAsync(email, password);

            var claims = new Dictionary<string, object> { { "role", role } };
            await FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance
                .SetCustomUserClaimsAsync(user.User.Uid, claims);
        }

        //public static Dictionary<string, string> UserRoles = new()
        //{
        //    {"brian@bsskyline.com", "Owner"},
        //    {"steven@bsskyline.com", "Resident"},
        //    {"mathias@bsskyline.com", "SecurityOfficer"}
        //};

        public static async Task<string?> GetUserRoleAsync(string userId)
        {
            try
            {
                var client = FirebaseService.GetFirebaseClient();
                var snapshot = await client
                    .Child("Users")
                    .Child(userId)
                    .Child("Role")
                    .OnceSingleAsync<string>();
                return snapshot;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public static async Task ChangePasswordAsync(string currentPassword, string newPassword, string userEmail)
        {
            try
            {
                var auth = AuthClient;
                var signInResult = await auth.SignInWithEmailAndPasswordAsync(userEmail, currentPassword);

                await signInResult.User.ChangePasswordAsync(newPassword);
            }
            catch (FirebaseAuthException ex)
            {
                throw new Exception($"Failed to change password: {ex.Reason}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error changing password: {ex.Message}");
            }
        }
    }
}
