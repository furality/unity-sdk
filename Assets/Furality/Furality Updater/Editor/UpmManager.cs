using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Furality.FuralityUpdater.Editor;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Networking;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Furality.FuralityUpdater.Bootstrap
{
    [InitializeOnLoad]
    public static class UpmManager
    {
        private class ScopedRegistry {
            public string name;
            public string url;
            public string[] scopes;
        }

        private class ManifestJson
        {
            public Dictionary<string, string> dependencies = new Dictionary<string, string>();

            public List<ScopedRegistry> scopedRegistries = new List<ScopedRegistry>();
        }

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

        static UpmManager()
        {
            if (!Application.isPlaying)
                Task.Run(() =>
                {
                    AsyncHelper.EnqueueOnMainThread(UpdaterMain);
                });
        }

        private static async Task<PackageInfo> GetInstalledPackageMeta(string id)
        {
            var installed = await AsyncHelper.MainThread(() =>
            {
                var req =  Client.List(true);
                while (!req.IsCompleted) {}

                return req.Result;
            });
            return installed.FirstOrDefault(p => p.name == "org.furality.updater");
        }

        private static async void UpdaterMain()
        {
            var name = Assembly.GetExecutingAssembly().GetName().Name;
            bool isBootstrapping = name != "FuralityUpdater";
            if (isBootstrapping)
                Debug.Log("Bootstrapping Furality Updater");
            
            var latest = await GetLatestMetadata("com.furality.updater");
            if (latest == null) return;
            
            // Now see if our installed package version is less than the latest version
            var updater = await GetInstalledPackageMeta("com.furality.updater");
            if (updater != null && updater.version == latest.version)
            {
                Debug.Log($"Updater is up to date");
                return;
            }
            
            var tempPath = await DownloadTempPackage(latest.downloadUrl);
            if (tempPath == null) return;

            
            var addReq = Client.Add("file:" + Path.GetFileName(latest.downloadUrl));
            while (!addReq.IsCompleted) {}
            Debug.Log($"Installing Furality Updater");

                // Now we double check to ensure the package was installed
            if (addReq.Status != StatusCode.Success)
            {
                Debug.LogError("Failed to install Furality Updater");
                return;
            }
            
            if (isBootstrapping)    // If we're running as bootstrapper and successfully installed the real updater, we can commit self destruct
            {
                Debug.Log("Furality Updater installed successfully");
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
                        Debug.LogWarning($"Failed to download package list: {response.StatusCode}");
                        return null;
                    }
                    
                    var responseString = await response.Content.ReadAsStringAsync();
                    var metadata = JsonUtility.FromJson<UpdateManifest>(responseString);
                    return metadata.packages.ToList().Find(m => m.name == id);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Failed to download package list: {e.Message}");
                    return null;
                }
            }
        }
        
        private static async Task<string> DownloadTempPackage(string url)
        {
            var tempPath = Path.Combine(Application.dataPath, "..", "Packages", Path.GetFileName(url));
            // Using unity web client to download the package
            using (var uwr = UnityWebRequest.Get(url))
            {
                var dh = new DownloadHandlerFile(tempPath);
                dh.removeFileOnAbort = true;
                uwr.downloadHandler = dh;
                
                var res = uwr.SendWebRequest();
                while (!res.isDone)
                {
                    await Task.Delay(100);
                }
                
                if (uwr.isNetworkError || uwr.isHttpError)
                {
                    Debug.LogError($"Failed to download package from {url}: {uwr.error}");
                    return null;
                }
                
                return tempPath;
            }
        }

        private static void AddScopedRegistry(ScopedRegistry pScopeRegistry) {
            var manifestPath = Path.Combine(Application.dataPath, "..", "Packages/manifest.json");
            var manifestJson = File.ReadAllText(manifestPath);
 
            var manifest = JsonUtility.FromJson<ManifestJson>(manifestJson);
 
            // If a registry with the same name already exists, replace it
            var existingRegistry = manifest.scopedRegistries.Find(registry => registry.name == pScopeRegistry.name);
            if (existingRegistry != null) {
                manifest.scopedRegistries.Remove(existingRegistry);
            }
            
            manifest.scopedRegistries.Add(pScopeRegistry);
 
            File.WriteAllText(manifestPath, JsonUtility.ToJson(manifest, true));
        }
    }
}