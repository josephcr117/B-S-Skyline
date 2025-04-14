using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;

namespace B_S_Skyline.Services
{
    public static class FirebaseAdminHelper
    {
        private static bool _initialized = false;
        private static FirebaseApp _firebaseApp;
        public static void Initialize()
        {
            if (!_initialized)
            {
                FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromFile("C:/Users/Josep/source/repos/B&S Skyline/B&S Skyline/bsskyline-dc061-firebase-adminsdk-fbsvc-dc50c3d785.json")
                });
                _initialized = true;
            }
        }
        public static async Task SetCustomUserClaimsAsync(string uid, Dictionary<string, object> claims)
        {
            try
            {
                Initialize();
                await FirebaseAuth.GetAuth(_firebaseApp).SetCustomUserClaimsAsync(uid, claims);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to set custom claims: {ex.Message}", ex);
            }
        }
        public static async Task<IReadOnlyDictionary<string, object>> GetCustomUserClaimsAsync(string uid)
        {
            try
            {
                Initialize();
                var user = await FirebaseAuth.GetAuth(_firebaseApp).GetUserAsync(uid);
                return user.CustomClaims ?? new Dictionary<string, object>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get custom claims: {ex.Message}", ex);
            }
        }
        public static async Task UpdateCustomClaimsAsync(string uid, Action<Dictionary<string, object>> updateAction)
        {
            try
            {
                Initialize();
                var currentClaims = new Dictionary<string, object>(await GetCustomUserClaimsAsync(uid) ?? new Dictionary<string, object>());
                updateAction(currentClaims);
                await SetCustomUserClaimsAsync(uid, currentClaims);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update custom claims: {ex.Message}", ex);
            }
        }
        public static async Task DeleteUserAsync(string uid)
        {
            Initialize();
            await FirebaseAuth.DefaultInstance.DeleteUserAsync(uid);
        }
    }
}
