using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;

namespace B_S_Skyline.Services
{
    public static class FirebaseAdminHelper
    {
        private static bool _initialized = false;
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
            Initialize();
            await FirebaseAuth.DefaultInstance.SetCustomUserClaimsAsync(uid, claims);
        }
    }
}
