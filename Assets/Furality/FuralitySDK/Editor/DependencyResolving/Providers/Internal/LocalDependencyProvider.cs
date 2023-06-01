using System.Linq;
using System.Threading.Tasks;
using Furality.FuralitySDK.Editor;
using Furality.SDK.External.Assets;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using AsyncHelper = Furality.SDK.Helpers.AsyncHelper;

namespace Furality.SDK.DependencyResolving
{
    /**
     * Local dependency resolver is responsible for removing our requirement for the VCC.
     * In the future, I'd like to modify this to actually request on VRChat's official and curated package lists
     * and then just download the packages from there instead of hard-coding it
     */
    public class LocalDependencyProvider : IDependencyProvider
    {
        private static readonly FuralityPackage[] _fallbacks = new []
        {
            new FuralityPackage()
            {
                Id = "com.llealloo.audiolink",
                Version = "0.3.2",
                FallbackUrl = 
                    "https://github.com/llealloo/vrc-udon-audio-link/releases/download/0.3.2/AudioLink_0.3.2_minimal.unitypackage"
            },
        };
        
        private static void InstallPackage(string url, string path)
        {
            // Step 2, we need to download the package.
            using (var uwr = UnityWebRequest.Get(url))
            {
                uwr.SendWebRequest();
                        
                while (!uwr.isDone)
                {
                }

                if (uwr.isNetworkError || uwr.isHttpError)
                {
                    return;
                }

                // Save the package to disk.
                System.IO.File.WriteAllBytes(path, uwr.downloadHandler.data);
            }
                    
            // Step 2, we need to install the package.
            Debug.Log($"Installing package {path}");
            UnityPackageImportQueue.Add(path);
        }
        
        public async Task<bool> Resolve(string id, string version)
        {
            var package = new TestDataSource().FindPackage(id);

            if (package == null) package = _fallbacks.ToList().Find(x => x.Id == id && x.Version == version);
            
            if (package == null || package.FallbackUrl == null)
                return false;

            string path = await AsyncHelper.MainThread(() => $"{Application.temporaryCachePath}/{id}.unitypackage");
                
            // If the ID is badgemaker, we need to (for now) delete all assets matching the name "Magick.Native-Q8-x64", "Magick.NET-Q8-x64" and "Magick.NET.Core" as they are already incluyded
            if (id == "com.furality.badgemaker")
            {
                // Find all assets matching the name
                var assets = AssetDatabase.FindAssets("Magick.Native-Q8-x64");
                foreach (var asset in assets)
                {
                    var assetPath = AssetDatabase.GUIDToAssetPath(asset);
                    AssetDatabase.DeleteAsset(assetPath);
                }
                assets = AssetDatabase.FindAssets("Magick.NET-Q8-x64");
                foreach (var asset in assets)
                {
                    var assetPath = AssetDatabase.GUIDToAssetPath(asset);
                    AssetDatabase.DeleteAsset(assetPath);
                }

                assets = AssetDatabase.FindAssets("Magick.NET.Core");
                foreach (var asset in assets)
                {
                    var assetPath = AssetDatabase.GUIDToAssetPath(asset);
                    AssetDatabase.DeleteAsset(assetPath);
                }
            }

            // Install our dependencies first
            if (package.Dependencies != null)
            {
                foreach (var dependency in package.Dependencies)
                {
                    if (!await DependencyResolver.Resolve(dependency.Key, dependency.Value))
                    {
                        return false;
                    }
                }
            }
            
            // Then install the package
            InstallPackage(package.FallbackUrl, path);
            return true;
        }
    }
}