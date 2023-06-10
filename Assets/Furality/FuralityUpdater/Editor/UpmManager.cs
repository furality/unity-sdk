using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Furality.FuralityUpdater.Editor;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Networking;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Furality.Furality_Updater.Editor
{
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
        
        public static async Task<PackageInfo> GetInstalledPackageMeta(string id)
        {
            var installed = await AsyncHelper.MainThread(() =>
            {
                var req =  Client.List(true);
                while (!req.IsCompleted) {}

                return req.Result;
            });
            return installed.FirstOrDefault(p => p.name == id);
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
                    Utils.Error($"Failed to download package from {url}: {uwr.error}");
                    return null;
                }
                
                return tempPath;
            }
        }

        public static async Task<bool> InstallRemoteTarGz(string url)
        {
            var tempPath = await DownloadTempPackage(url);
            if (tempPath == null) return false;
            
            var addReq = Client.Add("file:" + Path.GetFileName(url));
            while (!addReq.IsCompleted) {}
            Utils.Log($"Installing Furality Updater");

            // Now we double check to ensure the package was installed
            if (addReq.Status != StatusCode.Success)
            {
                Utils.Error("Failed to install Furality Updater");
                return false;
            }
            
            // Now remove the temp file
            File.Delete(tempPath);

            return false;
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