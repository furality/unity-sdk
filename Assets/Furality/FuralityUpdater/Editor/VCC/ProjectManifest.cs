using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Furality.FuralityUpdater.Editor
{
    public static class ProjectManifest
    {
        public class Dependency
        {
            public string Id { get; set; }
            public Version Version { get; set; }
            public string Description { get; set; }
            public string Title { get; set; }
            public Dictionary<string, Version> Dependencies { get; set; }
            public object GitDependencies { get; set; }
            public Dictionary<string, Version> VPMDependencies { get; set; }
            public string Url { get; set; }
            public string LocalPath { get; set; }
            public object Repo { get; set; }
            public object ZipSHA256 { get; set; }
            public object Headers { get; set; }
        }

        public class ProjectManifestResponse
        {
            public List<Dependency> dependencies { get; set; }
            public string path { get; set; }
            public string name { get; set; }
            public string type { get; set; }
        }

        private class ProjectManifestRequest
        {
            public string projectPath { get; set; }
        }
        
        public static async Task<ProjectManifestResponse> GetProjectManifest(string projectPath)
        {
            var manifestRequest = new ProjectManifestRequest
            {
                projectPath = projectPath
            };
            
            return await VccComms.Request<ProjectManifestResponse>("projects/manifest", "POST", manifestRequest);
        }
    }
}