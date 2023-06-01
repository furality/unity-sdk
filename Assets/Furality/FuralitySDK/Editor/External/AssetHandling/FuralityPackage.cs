using System;
using Furality.SDK.External.Api;

namespace Furality.SDK.External.Assets
{
    public class FuralityPackage : Package
    {
        public string Name;
        public string Description;
        public string ImageUrl;
        public string ConventionId;
        public string Category;
        public bool IsPublic;
        public DateTime CreatedAt;
        public AttendanceLevel AttendanceLevel;
        public PatreonLevel PatreonLevel = PatreonLevel.None;
        public string FallbackUrl;
    }
}