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
                    if (!await Resolve(dependency)) 
                    { 
                        return false;
                    }
                }
            }
            
            // First, we check that we don't already have this installed
            if (await ProjectManifest.IsDependencyInstalled(package.Id, package.Version))
            {
                Debug.Log($"{package.Id} {package.Version} is already installed!");
                return true;
            }
            
            Debug.Log($"Attempting to resolve {package.Id} {package.Version}");
            var resolved = false;
            foreach (var resolver in Resolvers)
            {
                if (await resolver.Resolve(package))
                {
                    Debug.Log($"Resolved {package.Id} {package.Version} using {resolver.GetType()}");
                    resolved = true;
                    break;
                }
            }

            if (!resolved)
                return false;

            AsyncHelper.EnqueueOnMainThread(() =>
                PlayerPrefs.SetString("furality:packageVersion:" + package.Id, package.Version.ToString()));
            
            return true;
        }
    }
}