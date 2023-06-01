using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace Furality.SDK.External.VCC
{
    public static class ProjectManifest
    {
        [Serializable]
        public class Dependency
        {
            public string Id;
            public string Version;
        }

        [Serializable]
        public class ProjectManifestResponse
        {
            public List<Dependency> dependencies;
            public string path;
            public string name;
            public string type;
        }

        public class ProjectManifestRequest
        {
            public string projectPath;
        }
        
        [ItemCanBeNull]
        public static async Task<ProjectManifestResponse> GetProjectManifest(string projectPath)
        {
            var manifestRequest = new ProjectManifestRequest
            {
                projectPath = projectPath
            };
            
            var resp =  await VccComms.Request<ProjectManifestResponse>("projects/manifest", "POST", manifestRequest);
                
            if (resp == null || !resp.success)
            {
                Debug.LogError($"Failed to get project manifest for: {projectPath}");
                return null;
            }
            
            return resp.data;
        }

        public static async Task<bool> IsDependencyInstalled(string id, string version)
        {
            // Get the root of our project, right before the Assets folder including the slash
            var projectPath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("/Assets", StringComparison.Ordinal));
            // Replace the forward slashes with two backslashes
            var manifest = await GetProjectManifest(projectPath);
            if (manifest == null)
                return false;

            return manifest.dependencies.Any(d => d.Id == id && d.Version == version);
        }
    }
}