using System.Threading.Tasks;
using UnityEngine;

namespace Furality.SDK.DependencyResolving
{
    public static class DependencyResolver
    {
        private static readonly IDependencyProvider[] Resolvers = 
        {
            new ProjectPackage(),
            new LocalDependencyProvider(),
            new FoxFiles(),
        };

        public static async Task<bool> Resolve(Package package)
        {
            // Resolve our dependencies first, then install the package.
            if (package.Dependencies != null)
            {
                foreach (var dependency in package.Dependencies)
                {
                    // If we've declared this dependency as a package, we need to resolve it as if we were directly installing it.
                    // This means we also need to install its dependencies.
                    var foundPackage = new TestDataSource().FindPackage(dependency.Key);
                    if (foundPackage != null)
                    {
                        if (!await Resolve(foundPackage))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!await Resolve(dependency.Key, dependency.Value))
                        {
                            return false;
                        }
                    }
                }
            }
            
            if (!await Resolve(package.Id, package.Version))
                return false;
            
            Debug.Log("Writing Package: "+package.Id);
            var metaPath = $"Assets/Furality/{package.Id}.json";
            System.IO.File.WriteAllText(metaPath, package.Version);
            
            return true;
        }

        public static async Task<bool> Resolve(string id, string version)
        {
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