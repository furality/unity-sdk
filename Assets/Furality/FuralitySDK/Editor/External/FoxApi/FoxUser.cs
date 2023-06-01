using System;
using System.Threading.Tasks;

namespace Furality.SDK.External.Api
{
    public static class FoxUser
    {
        [Serializable]
        public class ProfileResponse
        {
            public ApiUser user;
        }

        [Serializable]
        public class PatreonUserData
        {
            public string tier;
        }

        [Serializable]
        public class ApiUser
        {
            public string id;
            public string displayName;
            public string registrationLevel;
            public bool registered;
            public PatreonUserData patreon;
        }
        
        private static ApiUser _cachedProfile;
        public static AttendanceLevel AttendanceLevel => _cachedProfile == null ? AttendanceLevel.none : (AttendanceLevel) Enum.Parse(typeof(AttendanceLevel), _cachedProfile.registrationLevel);
        public static PatreonLevel PatreonLevel => _cachedProfile?.patreon?.tier == null ? PatreonLevel.None : (PatreonLevel) Enum.Parse(typeof(PatreonLevel), _cachedProfile.patreon.tier);
        
        public static void Destroy()
        {
            _cachedProfile = null;
        }
        
        // Gets the profile of currently signed in user
        public static async Task<ApiUser> GetProfile()
        {
            if (_cachedProfile != null)
                return _cachedProfile;

                // Request on user/profile endpoint
            var resp = await FoxApi.Get<ProfileResponse>("user/profile");
            _cachedProfile = resp?.user;
            return _cachedProfile;
        }
    }
}