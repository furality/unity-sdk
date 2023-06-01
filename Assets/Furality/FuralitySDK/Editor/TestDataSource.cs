using System.Collections.Generic;
using Furality.SDK.External.Api;
using Furality.SDK.External.Assets;
using UnityEditor;
using UnityEngine;

namespace Furality.SDK
{
    public class TestDataSource : IPackageDataSource
    {
        private readonly List<FuralityPackage> _packages = new List<FuralityPackage>() {
            new FuralityPackage()
            {
                Id = "com.furality.sylvashader",
                Name = "Sylva Shader",
                Description = "Our main shader for Furality Sylva!",
                ImageUrl = "https://media.furality.online/image/f6/Furality_Sylva_Shader_thumb.jpg",
                Category = "Shaders",
                ConventionId = "Furality Sylva",
                Version = "1.3.1",
                AttendanceLevel = AttendanceLevel.none,
                IsPublic = true,
                Dependencies = new Dictionary<string, string>()
                {
                    {"com.llealloo.audiolink", "0.3.2"}
                },
                FallbackUrl = "https://github.com/furality/vcc-furality-sylva-shader/releases/download/1.3.1/com.furality.sylvashader-1.3.1.unitypackage"
            },
            
            // Asset Packs
            new FuralityPackage()
            {
                Id = "2fcb8293-75c7-40f0-981e-cbd59cd06077",
                Name = "Patreon Asset Pack",
                Description = "Contains miscellaneous assets for use on your avatar, exclusive to Patreons!",
                ImageUrl = "https://media.furality.online/image/f6/Furality_Sylva_Patreon_thumb.jpg",
                Category = "Asset Packs",
                ConventionId = "Furality Sylva",
                Dependencies = new Dictionary<string, string>()
                {
                    {"com.furality.sylvashader", "1.3.1"}
                },
                Version = "1.0.0",
                AttendanceLevel = AttendanceLevel.none,
                PatreonLevel = PatreonLevel.Orange,
                IsPublic = false,

            },
            new FuralityPackage()
            {
                Id = "44096464-6239-4503-9b84-d0b948512aa9",
                Name = "Attendee Asset Pack",
                Description = "Contains miscellaneous assets for use on your avatar!",
                ImageUrl = "https://media.furality.online/image/f6/Furality_Sylva_Attendee_Asset_Pack.jpg",
                Category = "Asset Packs",
                ConventionId = "Furality Sylva",
                Dependencies = new Dictionary<string, string>()
                {
                    {"com.furality.sylvashader", "1.3.1"}
                },
                Version = "1.0.0",
                AttendanceLevel = AttendanceLevel.attendee,
                IsPublic = false,

            },
            new FuralityPackage()
            {
                Id = "fbed2d44-7925-4992-8bf9-8c37d88277f0",
                Name = "First Class Asset Pack",
                Description = "Contains miscellaneous assets for use on your avatar!",
                ImageUrl = "https://media.furality.online/image/f6/Furality_Sylva_First_Class_Asset_Pack.jpg",
                Category = "Asset Packs",
                ConventionId = "Furality Sylva",
                Dependencies = new Dictionary<string, string>()
                {
                    {"com.furality.sylvashader", "1.3.1"}
                },
                Version = "1.0.0",
                AttendanceLevel = AttendanceLevel.first_class,
                IsPublic = false
            },
            new FuralityPackage()
            {
                Id = "d7f487c5-c8f8-4746-bfdd-8585c711dc62",
                Name = "Sponsor Asset Pack",
                Description = "Contains miscellaneous assets for use on your avatar!",
                ImageUrl = "https://media.furality.online/image/f6/Furality_Sylva_Sponsor_thumb.jpg",
                Category = "Asset Packs",
                ConventionId = "Furality Sylva",
                Dependencies = new Dictionary<string, string>()
                {
                    {"com.furality.sylvashader", "1.3.1"}
                },
                Version = "1.0.0",
                AttendanceLevel = AttendanceLevel.sponsor,
                IsPublic = false
            },
            
            // Badges
            new FuralityPackage()
            {
                Id = "com.furality.badgemaker",
                Name = "Badge Maker",
                Description = "Allows you to customise the text displayed on badges!",
                ImageUrl = "https://media.furality.online/image/f6/Furality_Sylva_First_Class_Asset_Pack.jpg",
                Category = "Tools",
                ConventionId = "Furality Sylva",
                Dependencies = new Dictionary<string, string>(),
                Version = "1.0.1",
                AttendanceLevel = AttendanceLevel.none,
                IsPublic = true,
                FallbackUrl = "https://github.com/furality/unity-sdk/releases/download/1.0.1/com.furality.badgemaker-1.0.1.unitypackage"
            },
            new FuralityPackage()
            {
                Id = "7851e3bc-3b68-4b01-b774-7264ef94abeb",
                Name = "Attendee Badge",
                Description = "Badge for attendees!",
                ImageUrl = "https://media.furality.online/image/f6/Furality_Sylva_Badges_thumb.jpg",
                Category = "Badges",
                ConventionId = "Furality Sylva",
                Dependencies = new Dictionary<string, string>()
                {
                    {"com.furality.sylvashader", "1.3.1"},
                    {"com.furality.badgemaker", "1.0.1"}
                },
                Version = "1.0.0",
                AttendanceLevel = AttendanceLevel.attendee,
                IsPublic = false
            },
            new FuralityPackage()
            {
                Id = "b44f69f4-5735-4fc2-bb1a-f3df3f5e6921",
                Name = "First Class Badge",
                Description = "Badge for first class attendees!",
                ImageUrl = "https://media.furality.online/image/f6/Furality_Sylva_Badges_thumb.jpg",
                Category = "Badges",
                ConventionId = "Furality Sylva",
                Dependencies = new Dictionary<string, string>()
                {
                    {"com.furality.sylvashader", "1.3.1"},
                    {"com.furality.badgemaker", "1.0.1"}
                },
                Version = "1.0.0",
                AttendanceLevel = AttendanceLevel.first_class,
                IsPublic = false
            },
            new FuralityPackage()
            {
                Id = "39a517c7-d096-42b4-92f8-f057ad85c54f",
                Name = "Sponsor Badge",
                Description = "Badge for sponsors!",
                ImageUrl = "https://media.furality.online/image/f6/Furality_Sylva_Badges_thumb.jpg",
                Category = "Badges",
                ConventionId = "Furality Sylva",
                Dependencies = new Dictionary<string, string>()
                {
                    {"com.furality.sylvashader", "1.3.1"},
                    {"com.furality.badgemaker", "1.0.1"}
                },
                Version = "1.0.0",
                AttendanceLevel = AttendanceLevel.sponsor,
                IsPublic = false
            },
        };
        
        public FuralityPackage FindPackage(string id) => _packages.Find(x => x.Id == id);
        
        public IEnumerable<FuralityPackage> GetPackages() => _packages;
        public string GetInstalledPackage(string id)
        {
            // Query assetDatabase for any assets with the name {id}.json
            // If there are any, return the first one
            // If there are none, return null
            var foundFile = AssetDatabase.FindAssets($"{id}");
            if (foundFile.Length == 0) return null;
            
            var path = AssetDatabase.GUIDToAssetPath(foundFile[0]);
            var json = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            return json.text;
        }
    }
}