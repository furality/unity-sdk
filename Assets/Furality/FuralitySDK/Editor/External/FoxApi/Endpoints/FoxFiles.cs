﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Furality.SDK.DependencyResolving;
using Furality.SDK.External.Api.Models.Files;
using Furality.SDK.External.Assets;
using Furality.SDK.Helpers;
using UnityEditor;
using UnityEngine;

namespace Furality.SDK.External.Api.Endpoints
{
    public class FoxFiles : FoxResource, IDependencyProvider, IPackageDataSource
    {
        public class FileResponse
        {
            public string file;
            public string url;
        }

        private FoxFileDto[] _files;

        public FoxFiles(FoxApi api) : base(api)
        {
            DependencyManager.AddProvider(this);
        }

        public FuralityPackage FindPackage(string id) => _files.ToList().Find(x => x.Id == id);

        public async Task<string> PreSignDownload(string fileId)
        {
            // Request on https://api.fynn.ai/v1/file/{fileId} with bearer token
            // Returns a presigned download link
            var resp = await API.Get<FileResponse>($"file/{fileId}");
            // Replace to rationalized URL, this is a temporary fix until we can get the API to return the correct URL.
            return resp?.url?.Replace("media-furality-online.nyc3.digitaloceanspaces.com", "media.furality.online");
        }
        
        public async Task<bool> Resolve(string id, string version)
        {
            // Attempt to presign the download for this ID, if it fails, return false
            var url = await PreSignDownload(id);
            if (url == null)
                return false;

            Debug.Log("Downloading Package: " + id);
            DownloadHelper.Enqueue(id, url);
            return true;
        }

        public override async void OnPostLogin()
        {
            var filesResp = await API.Get<FoxFilesDto>("file");
            _files = filesResp?.files;
            
            base.OnPostLogin();
        }

        public override void Dispose()
        {
            _files = null;
        }

        public IEnumerable<FuralityPackage> GetPackages()
        {
            return _files;
        }

        public string GetInstalledPackage(string id)
        {
            // Query assetDatabase for any assets with the name {id}.json
            // If there are any, return the first one
            // If there are none, return nul
            var foundFile = AssetDatabase.FindAssets($"{id}");
            if (foundFile.Length == 0) return null;
            
            var path = AssetDatabase.GUIDToAssetPath(foundFile[0]);
            var json = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            return json.text;
        }
    }
}