using B_S_Skyline.Models;
using Firebase.Database;
using Firebase.Database.Query;

namespace B_S_Skyline.Services
{
    public static class FirebaseService
    {
        private static FirebaseClient _firebaseClient;
        private const string DatabaseUrl = "https://bsskyline-dc061-default-rtdb.firebaseio.com/";

        public static FirebaseClient GetFirebaseClient()
        {
            if (_firebaseClient == null)
            {
                _firebaseClient = new FirebaseClient(DatabaseUrl);
            }
            return _firebaseClient;
        }
        public static async Task<Dictionary<string, VehicleModel>> GetUserVehiclesAsync(string userId)
        {
            var result = await GetFirebaseClient()
                .Child("Users")
                .Child(userId)
                .Child("Vehicles")
                .OnceAsync<VehicleModel>();

            return result.ToDictionary(item => item.Key, item => item.Object);
        }
        public static async Task AddVehicleAsync(string userId, VehicleModel vehicle)
        {
            var client = GetFirebaseClient();
            await client
                .Child("Users")
                .Child(userId)
                .Child("Vehicles")
                .Child(vehicle.LicensePlate)
                .PutAsync(vehicle);
        }
        public static async Task UpdateVehicleAsync(string userId, string vehicleKey, VehicleModel vehicle)
        {
            await GetFirebaseClient()
                .Child("Users")
                .Child(userId)
                .Child("Vehicles")
                .Child(vehicleKey)
                .PutAsync(vehicle);
        }
    }
}
