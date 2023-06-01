using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Furality.FuralityUpdater.Editor;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Networking;

namespace Furality.Furality_Updater.Editor
{
    [InitializeOnLoad]
    public class Updater
    {
        [Serializable]
        public class UpdateMetadata
        {
            public string name;
            public string version;
            public string downloadUrl;
        }

        [Serializable]
        public class UpdateManifest
        {
            public int manifestVersion;
            public UpdateMetadata[] packages;
        }

        static Updater()
        {
            if (!Application.isPlaying)
                Task.Run(() =>
                {
                    AsyncHelper.EnqueueOnMainThread(UpdaterMain);
                });
        }

        private static async void UpdaterMain()
        {
            var name = Assembly.GetExecutingAssembly().GetName().Name;
            bool isBootstrapping = name != "FuralityUpdater";
            if (isBootstrapping)
                Utils.Log("Bootstrapping Furality Updater");
            
            var latest = await GetLatestMetadata("com.furality.updater");
            if (latest == null) return;
            
            // Now see if our installed package version is less than the latest version
            var updater = await UpmManager.GetInstalledPackageMeta("org.furality.updater");
            if (updater == null || updater.version != latest.version)
            {
                if (!await UpmManager.InstallRemoteTarGz(latest.downloadUrl))
                {
                    Utils.Error($"Failed to install Furality Updater via UPM");
                    //return;
                }
            }

            if (isBootstrapping)    // If we're running as bootstrapper and successfully installed the real updater, we can commit self destruct
            {
                Utils.Log("Furality Updater installed successfully");
                var packagePath = Path.Combine(Application.dataPath, "Furality", "Furality Updater");
                if (Directory.Exists(packagePath))
                {
                    Directory.Delete(packagePath, true);
                }

                // Return as we don't wanna update the actual furality sdk from the bootstrapper
                return;
            }
            
            // Now we wanna check for updates to the actual Furality SDK
            Utils.Log("Checking for updates to Furality SDK");
            var sdk = await GetLatestMetadata("com.furality.sdk");
            if (sdk == null)
            {
                Utils.Error("Remote metadata for Furality SDK not found");
                return;
            }
            
            var installed = await UpmManager.GetInstalledPackageMeta("org.furality.sdk");
            if (installed != null && installed.version == sdk.version)
            {
                Utils.Log($"Furality SDK is up to date");
                return;
            }
            
            Utils.Log($"Installing Furality SDK {sdk.version}");
            if (!await UpmManager.InstallRemoteTarGz(sdk.downloadUrl))
            {
                //Utils.Error($"Failed to install Furality SDK via UPM");
                return;
            }
            
            Utils.Log($"Furality SDK {sdk.version} installed successfully");
        }

        private static async Task<UpdateMetadata> GetLatestMetadata(string id)
        {
            // Download the package list from the server
            using (var client = new System.Net.Http.HttpClient())
            {
                try
                {
                    var response = await client.GetAsync("https://raw.githubusercontent.com/furality/unity-sdk/prod/furality_package.json");
                    if (!response.IsSuccessStatusCode)
                    {
                        Utils.Warn($"Failed to download package list: {response.StatusCode}");
                        return null;
                    }
                    
                    var responseString = await response.Content.ReadAsStringAsync();
                    var metadata = JsonUtility.FromJson<UpdateManifest>(responseString);
                    return metadata.packages.ToList().Find(m => m.name == id);
                }
                catch (Exception e)
                {
                    Utils.Warn($"Failed to download package list: {e.Message}");
                    return null;
                }
            }
        }
    }
}