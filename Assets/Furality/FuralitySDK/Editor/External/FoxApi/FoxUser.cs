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
        public static AttendanceLevel AttendanceLevel
        {
            get
            {
                if (_cachedProfile?.registrationLevel == null)
                    return AttendanceLevel.none;

                return Enum.TryParse(_cachedProfile.registrationLevel, true, out AttendanceLevel level)
                    ? level
                    : AttendanceLevel.none;
            }
        }

        public static PatreonLevel PatreonLevel
        {
            get
            {
                if (_cachedProfile?.patreon?.tier == null)
                    return PatreonLevel.None;

                return Enum.TryParse(_cachedProfile.patreon.tier, true, out PatreonLevel level) ? level :
                    PatreonLevel.None;
            }
        }

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