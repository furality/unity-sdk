using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Furality.SDK.Editor.External.VCC;
using Furality.SDK.Editor.Helpers;
using UnityEngine;

namespace Furality.SDK.DependencyResolving
{
    public class DependencyResolver
    {
        private readonly List<IDependencyProvider> Resolvers = new List<IDependencyProvider>
        {
            new ProjectPackage(),
            new LocalDependencyProvider()
        };

        public void AddProvider(IDependencyProvider provider) => Resolvers.Add(provider);

        public async Task<bool> Resolve(Package package)
        {
            // Resolve our dependencies first, then install the package.
            if (package.Dependencies != null)
            {
                foreach (var dependency in package.Dependencies)
                {
                    if (!await Resolve(dependency.Key, dependency.Value)) 
                    { 
                        return false;
                    }
                }
            }
            
            if (!await Resolve(package.Id, package.Version))
                return false;

            AsyncHelper.EnqueueOnMainThread(() =>
                PlayerPrefs.SetString("furality:packageVersion:" + package.Id, package.Version.ToString()));
            
            return true;
        }

        public async Task<bool> Resolve(string id, Version version)
        {
            // First, we check that we don't already have this installed
            if (await ProjectManifest.IsDependencyInstalled(id, version))
            {
                Debug.Log($"{id} {version} is already installed!");
                return true;
            }
            
            Debug.Log($"Attempting to resolve {id} {version}");
            foreach (var resolver in Resolvers)
            {
                if (await resolver.Resolve(id, version))
                {
                    Debug.Log($"Resolved {id} {version} using {resolver.GetType()}");
                    return true;
                }
            }

            return false;
        }
    }
}