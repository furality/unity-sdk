using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Furality.FuralityUpdater.Editor;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Networking;

namespace Furality.Furality_Updater.Editor
{
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
            var updater = await UpmManager.GetInstalledPackageMeta("com.furality.updater");
            if (updater != null && updater.version == latest.version)
            {
                Utils.Log($"Updater is up to date");
                return;
            }

            if (!await UpmManager.InstallRemoteTarGz(latest.downloadUrl))
            {
                Utils.Error($"Failed to install Furality Updater via UPM");
                return;
            }
            
            if (isBootstrapping)    // If we're running as bootstrapper and successfully installed the real updater, we can commit self destruct
            {
                Utils.Log("Furality Updater installed successfully");
                var packagePath = Path.Combine(Application.dataPath, "Furality", "Furality Updater");
                if (Directory.Exists(packagePath))
                {
                    Directory.Delete(packagePath, true);
                }
            }
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