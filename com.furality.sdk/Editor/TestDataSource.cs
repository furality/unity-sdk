using System;
using System.Collections.Generic;
using Furality.SDK.Editor.External.AssetHandling;
using Furality.SDK.Editor.External.FoxApi;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace Furality.SDK.Editor
{
    [Obsolete]
    public class TestDataSource : IPackageDataSource
    {
        private readonly List<FuralityPackage> _packages = new List<FuralityPackage>() {
            // Shaders
            new FuralityPackage()
            {
                Id = "com.furality.sylvashader",
                Name = "Sylva Shader",
                Description = "Our main shader for Furality Sylva!",
                ImageUrl = "https://media.furality.online/image/f6/Furality_Sylva_Shader_thumb.jpg",
                Category = "Shaders",
                ConventionId = "Furality Sylva",
                Version = new Version(1, 3, 3),
                AttendanceLevel = AttendanceLevel.none,
                IsPublic = true,
                Dependencies = new List<Package>()
                {
                    new Package() {Id = "com.llealloo.audiolink", Version = new Version(0, 3, 2)}
                },
                FallbackUrl = "https://github.com/furality/vcc-furality-sylva-shader/releases/download/1.3.3/com.furality.sylvashader-1.3.3.unitypackage"
            },
            new FuralityPackage()
            {
                Id = "com.furality.umbrashader",
                Name = "Umbra Shader",
                Description = "Furality Umbra Avatar Shader for VRChat. Created by Naito @ Furality, Inc.",
                ImageUrl = "https://media.furality.online/image/f6/Furality_Sylva_Shader_thumb.jpg",
                Category = "Shaders",
                ConventionId = "Furality Umbra",
                Version = new Version(1, 6, 0),
                AttendanceLevel = AttendanceLevel.none,
                IsPublic = true,
                Dependencies = new List<Package>()
                {
                    new Package() {Id = "com.llealloo.audiolink", Version = new Version(0, 3, 2)}
                },
                FallbackUrl = "https://github.com/furality/vcc-furality-umbra-shader/releases/download/1.6.0/com.furality.umbrashader-1.6.0.unitypackage"
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
                Dependencies = new List<Package>(),
                Version = new Version(1, 0, 1),
                AttendanceLevel = AttendanceLevel.none,
                IsPublic = true,
                FallbackUrl = "https://github.com/furality/unity-sdk/releases/download/1.0.1/com.furality.badgemaker-1.0.1.unitypackage"
            },
        };
        
        public FuralityPackage FindPackage(string id) => _packages.Find(x => x.Id == id);
        
        public IEnumerable<FuralityPackage> GetPackages() => _packages;
        
        [CanBeNull] public Version GetInstalledPackage(string id)
        {
            // Query assetDatabase for any assets with the name {id}.json
            // If there are any, return the first one
            // If there are none, return null
            var foundVersion = PlayerPrefs.GetString("furality:packageVersion:" + id);
            return string.IsNullOrEmpty(foundVersion) ? null : new Version(foundVersion);
        }
    }
}