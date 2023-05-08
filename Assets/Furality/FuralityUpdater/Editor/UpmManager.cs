using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Networking;

namespace Furality.FuralityUpdater.Editor
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

        private class UpdateMetadata
        {
            public string id;
            public Version version;
            public string downloadUrl;
        }

        private class UpdateManifest
        {
            public int manifestVersion;
            public List<UpdateMetadata> packages;
        }

        static UpmManager()
        {
            if (!Application.isPlaying)
                Task.Run(UpdaterMain);
        }

        private static async void UpdaterMain()
        {
            var latest = await GetLatestMetadata("org.furality.updater");
            if (latest == null) return;
            
            var tempPath = await DownloadTempPackage(latest.downloadUrl);
            if (tempPath == null) return;

            Client.Add("file:" + Path.GetFileName(latest.downloadUrl));
        }

        private static async Task<UpdateMetadata> GetLatestMetadata(string id)
        {
            // Download the package list from the server
            using (var client = new System.Net.Http.HttpClient())
            {
                try
                {
                    var response = await client.GetAsync("https://raw.githubusercontent.com/furality/unity-sdk/master/furality_package.json");  //TODO: Don't forget, upon release we need to make this public
                    if (!response.IsSuccessStatusCode)
                    {
                        Debug.LogWarning($"Failed to download package list: {response.StatusCode}");
                        return null;
                    }
                    
                    var responseString = await response.Content.ReadAsStringAsync();
                    var metadata = JsonUtility.FromJson<List<UpdateMetadata>>(responseString);
                    return metadata.Find(m => m.id == id);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"Failed to download package list: {e.Message}");
                    return null;
                }
            }
        }
        
        private static async Task<string> DownloadTempPackage(string url)
        {
            var tempPath = await AsyncHelper.MainThread(() => Path.Combine(Application.dataPath, "..", Path.GetFileName(url)));
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
        
        private static void UpdatePackage(string id) => Client.Add(id);

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