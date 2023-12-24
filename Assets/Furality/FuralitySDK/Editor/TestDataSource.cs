using System;
using System.Collections.Generic;
using Furality.SDK.External.Api;
using Furality.SDK.External.Assets;
using UnityEditor;
using UnityEngine;

namespace Furality.SDK
{
    [Obsolete]
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
                Version = "1.3.3",
                AttendanceLevel = AttendanceLevel.none,
                IsPublic = true,
                Dependencies = new Dictionary<string, string>()
                {
                    {"com.llealloo.audiolink", "0.3.2"}
                },
                FallbackUrl = "https://github.com/furality/vcc-furality-sylva-shader/releases/download/1.3.3/com.furality.sylvashader-1.3.3.unitypackage"
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