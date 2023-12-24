using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Unity.Jobs;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Furality.SDK.Helpers
{
    public class DownloadHelper : IJob
    {
        private static readonly Queue<(string id, string url)> _downloadQueue = new Queue<(string, string)>();
        private static bool _inProgress;

        public static void Enqueue(string id, string url)
        {
            Debug.Log("Enqueueing "+id);
            _downloadQueue.Enqueue((id, url));
        }

        private static async void DownloadQueue()
        {
            _inProgress = true;
            while (_downloadQueue.Any())
            {
                var (id, url) = _downloadQueue.Dequeue();
                var path = await DownloadFile(id, url, f => EditorUtility.DisplayProgressBar("Downloading File", id, f));
                Debug.Log("Downloaaded File path "+path);
                if (!string.IsNullOrEmpty(path))
                    UnityPackageImportQueue.Add(path);
                
                EditorUtility.ClearProgressBar();
            }
            
            new UnityPackageImportQueue().Execute();
            _inProgress = false;
        }

        private static async Task<string> DownloadFile(string id, string url, Action<float> onDownloadProgress)
        {
            string path = await AsyncHelper.MainThread(() =>
                $"{Application.temporaryCachePath}/{id}.unitypackage");

            using (var client = new WebClient())
            {
                client.DownloadProgressChanged += (sender, eventArgs) =>
                    onDownloadProgress.Invoke(eventArgs.ProgressPercentage / 100f);

                try
                {
                    await client.DownloadFileTaskAsync(url, path);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Error downloading file {id}: {e.Message}");
                    return null;
                }
            }

            return path;
        }
        
        public static Texture2D DownloadImage(string url)
        {
#pragma warning disable CS0618
            var www = new WWW(url);
#pragma warning restore CS0618
            while (!www.isDone)
            {
            }

            return www.texture;
        }

        public void Execute()
        {
            DownloadQueue();
        }
    }
}