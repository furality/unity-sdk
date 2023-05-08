using System;
using System.Linq;
using System.Threading.Tasks;
using Furality.Editor.AssetHandling;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Furality.FuralityUpdater.Editor
{
    // DependencyManager's job is to handle the mixing and matching of our native Furality-style dependencies, 
    // as well as the VCC dependencies.
    public static class DependencyManager
    {
        private static async Task<bool> IsPackageInstalled(Package package)
        {
            return await ProjectManifest.IsDependencyInstalled(package.Id, package.Version);
            //TODO: Check with our own managed package list.
        }

        private static async void DownloadAndInstallFuralityPackage(Package packageToInstall, bool interactive)
        {
            string path = await AsyncHelper.MainThread(() => $"{Application.temporaryCachePath}/{packageToInstall.Id}-{packageToInstall.Version}.unitypackage");

            // Step 1, we need to download the package.
            using (var uwr = UnityWebRequest.Get(packageToInstall.DownloadUrl))
            {
                uwr.SendWebRequest();
                while (!uwr.isDone) { }
                if (uwr.isNetworkError || uwr.isHttpError)
                {
                    throw new Exception($"Failed to download package {packageToInstall.Id} {packageToInstall.Version}");
                }
                
                // Save the package to disk.
                System.IO.File.WriteAllBytes(path, uwr.downloadHandler.data);
            }
            
            // Step 2, we need to install the package.
            AssetDatabase.ImportPackage(path, interactive);
        }

        public static async Task UpgradeOrInstall(Package packageToInstall, bool interactive, IPackageDataSource dataSource)
        {
            if (await IsPackageInstalled(packageToInstall)) return;
            
            // Step 1, we need to check to see if this is a VCC package or a Furality package.
            // If it's a UPM package, we need to install it using the VCC API.
            if (!(packageToInstall is FuralityPackage))
            {
                Debug.Log($"Installing VCC package {packageToInstall.Id} {packageToInstall.Version}");

                // It's not installed (or at least the version we want isn't. We need to install it.
                var success = await ProjectPackage.AddPackage(packageToInstall.Id, packageToInstall.Version);
                if (success)
                    return; // VCC was able to resolve this dependency for us.
            }
            
            // Step 2, given that the VCC was unable to resolve this dependency, we need to install it ourselves.
            if (packageToInstall.Dependencies != null)
            {
                foreach (var dependency in packageToInstall.Dependencies.Select(d => dataSource.GetPackage(d.Key)))
                {
                    if (dependency.Id == packageToInstall.Id) continue;

                    if (!await IsPackageInstalled(dependency))
                    {
                        await UpgradeOrInstall(dependency, interactive, dataSource);
                    }
                }
            }

            // Step 3, now that we know all of our dependencies are installed, we can install this package.
            DownloadAndInstallFuralityPackage(packageToInstall, interactive);   
        }
    }
}