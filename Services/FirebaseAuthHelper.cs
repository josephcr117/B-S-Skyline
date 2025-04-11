using Firebase.Auth.Providers;
using Firebase.Auth;

namespace B_S_Skyline.Services
{
    public static class FirebaseAuthHelper
    {
        public const string firebaseAppId = "bsskyline-dc061";
        public const string firebaseApiKey = "";

        public static FirebaseAuthClient AuthClient => new(new FirebaseAuthConfig()
        {
            ApiKey = firebaseApiKey,
            AuthDomain = $"{firebaseAppId}.firebaseapp.com",
            Providers = new FirebaseAuthProvider[] { new EmailProvider() }
        });
        public static Dictionary<string, string> UserRoles = new()
        {
            {"brian@bsskyline.com", "Owner"},
            {"steven@bsskyline.com", "Resident"},
            {"mathias@bsskyline.com", "SecurityOfficer"}
        };
    }
}
