using System;
using System.Threading.Tasks;
using Furality.FuralitySDK.Editor;
using Furality.SDK.DependencyResolving;
using Furality.SDK.External.Api;
using Furality.SDK.External.Assets;
using Furality.SDK.External.VCC;
using Furality.SDK.Helpers;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Furality.SDK
{
    // DependencyManager's job is to handle the mixing and matching of our native Furality-style dependencies, 
    // as well as the VCC dependencies.
    public static class DependencyManager
    {
        private static async Task<bool> IsPackageInstalled(Package package)
        {
            return await ProjectManifest.IsDependencyInstalled(package.Id, package.Version);
        }

        public static async Task UpgradeOrInstall(Package packageToInstall, bool interactive, IPackageDataSource dataSource)
        {
            EditorUtility.DisplayProgressBar("Furality", $"Checking for VCC-based install", 0.0f);
            if (await IsPackageInstalled(packageToInstall)) return;
            
            // Step 1, attempt to install using the VCC. If this fails, we'll need to install it ourselves using the download link
            Debug.Log($"Installing VCC package {packageToInstall.Id} {packageToInstall.Version}");

            // It's not installed (or at least the version we want isn't. We need to install it.
            EditorUtility.DisplayProgressBar("Furality", $"Attempting VCC-based install", 0.2f);
            var success = await DependencyResolver.Resolve(packageToInstall);
            
            UnityPackageImportQueue.CheckQueue();
        }
    }
}