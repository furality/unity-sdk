using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Furality.Editor.AssetHandling;
using UnityEditor.PackageManager;
using UnityEditorInternal;

namespace Furality.FuralityUpdater.Editor
{
    // DependencyManager's job is to handle the mixing and matching of our native Furality-style dependencies, 
    // as well as the Unity Package Manager dependencies.
    public static class DependencyManager
    {
        public static bool IsUpmPackageInstalled(Package package)
        {
            return true;
        }
        
        public static async void UpgradeOrInstall(Package packageToInstall, IPackageDataSource dataSource)
        {
            // Step 1, we need to check to see if this is a VCC package or a Furality package.
            // If it's a UPM package, we need to install it using the VCC API.
            if (!(packageToInstall is FuralityPackage))
            {
                // Check to see if it's already installed. If it is, return.
                if (await ProjectManifest.IsDependencyInstalled(packageToInstall.Id, packageToInstall.Version)) return;
                
                // It's not installed (or at least the version we want isn't. We need to install it.
                var success = await ProjectPackage.AddPackage(packageToInstall.Id, packageToInstall.Version);
                if (!success)
                {
                    throw new Exception(
                        $"Failed to install package {packageToInstall.Id} {packageToInstall.Version}");
                }
    
                // We got this far, meaning we successfully installed the package. We can return.
                return;
            }
            
            // Step 2, we need to check to see if we need to install any dependencies.
            // If we do, we need to install them first.
            foreach (var dependency in packageToInstall.Dependencies.Select(d => dataSource.GetPackage(d.Key)))
            {
                if (!IsUpmPackageInstalled(dependency))
                {
                    UpgradeOrInstall(dependency, dataSource);
                }
            }
            
            // Step 3, now that we know all of our dependencies are installed, we can install this package.
            // TODO
        }
    }
}