﻿using System;
using System.Threading.Tasks;
using Furality.SDK.DependencyResolving;
using Furality.SDK.External.Assets;
using Furality.SDK.External.VCC;
using UnityEditor;
using UnityEngine;

namespace Furality.SDK.Helpers
{
    // DependencyManager's job is to handle the mixing and matching of our native Furality-style dependencies, 
    // as well as the VCC dependencies.
    public static class DependencyManager
    {
        private static DependencyResolver _dr = new DependencyResolver();

        public static void AddProvider(IDependencyProvider provider) => _dr.AddProvider(provider);
        public static async Task<bool> Resolve(Package package) => await _dr.Resolve(package);
        public static async Task<bool> Resolve(string id, Version version) => await _dr.Resolve(id, version);
        
        private static async Task<bool> IsPackageInstalled(Package package) => await ProjectManifest.IsDependencyInstalled(package.Id, package.Version);

        public static async Task UpgradeOrInstall(Package packageToInstall, bool interactive)
        {
            if (await IsPackageInstalled(packageToInstall)) return;
            
            // Step 1, attempt to install using the VCC. If this fails, we'll need to install it ourselves using the download link
            Debug.Log($"Attempting install VCC package {packageToInstall.Id} {packageToInstall.Version}");

            try
            {
                AssetDatabase.DisallowAutoRefresh();
                AssetDatabase.StartAssetEditing();
                // It's not installed (or at least the version we want isn't. We need to install it.
                var success = await _dr.Resolve(packageToInstall);
                new DownloadHelper().Execute();
            }
            finally
            {
                
            }
        }
    }
}