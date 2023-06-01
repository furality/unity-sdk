using System;
using System.Threading.Tasks;
using Furality.FuralitySDK.Editor;
using Furality.SDK.External.Api;
using Furality.SDK.Helpers;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Furality.SDK.DependencyResolving
{
    public class FoxFiles : IDependencyProvider
    {
        public class FileResponse
        {
            public string file;
            public string url;
        }
        
        public static async Task<string> PreSignDownload(string fileId)
        {
            // Request on https://api.fynn.ai/v1/file/{fileId} with bearer token
            // Returns a presigned download link
            var resp = await FoxApi.Get<FileResponse>($"file/{fileId}");
            // Replace to rationalized URL, this is a temporary fix until we can get the API to return the correct URL.
            return resp?.url?.Replace("media-furality-online.nyc3.digitaloceanspaces.com", "media.furality.online");
        }

        private static bool DownloadAndInstallFuralityPackage(string downloadUrl, string downloadPath)
        {
            using (var uwr = UnityWebRequest.Get(downloadUrl))
            {
                uwr.SendWebRequest();
                
                while (!uwr.isDone)
                {
                }

                if (uwr.isNetworkError || uwr.isHttpError)
                {
                    return false;
                }

                System.IO.File.WriteAllBytes(downloadPath, uwr.downloadHandler.data);
            }
            
            // Step 2, we need to install the package.
            UnityPackageImportQueue.Add(downloadPath);
            
            return true;
        }
        
        public async Task<bool> Resolve(string id, string version)
        {
            string path = await AsyncHelper.MainThread(() =>
                $"{Application.temporaryCachePath}/{id}-{version}.unitypackage");
            
            // Attempt to presign the download for this ID, if it fails, return false
            var url = await PreSignDownload(id);
            if (url == null)
                return false;

            Debug.Log("Downloading Package: "+id);
            return DownloadAndInstallFuralityPackage(url, path);
        }
    }
}