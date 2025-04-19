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
    }
}
