using System;
using System.Threading.Tasks;
using Furality.SDK.External.VCC;
using UnityEngine;

namespace Furality.SDK.DependencyResolving
{
    public class ProjectPackage : IDependencyProvider
    {
        public class AddPackageRequest : BasePackageRequest
        {
            public string version;
        }

        public class BasePackageRequest
        {
            public string projectPath;

            public string packageId;
        }

        public static async Task<bool> AddPackage(AddPackageRequest request)
        {
            var response = await VccComms.Request<string>("projects/packages", "POST", request);
            return response != null && response.success;
        }

        public async Task<bool> RemovePackage(BasePackageRequest request)
        {
            var response = await VccComms.Request<string>("projects/packages", "DELETE", request);
            return response.success;
        }
        
        public async Task RemovePackage(string projectPath, string packageId) => await RemovePackage(new BasePackageRequest
        {
            projectPath = projectPath,
            packageId = packageId
        });

        public async Task<bool> Resolve(string id, string version)
        {
            return await AddPackage(new AddPackageRequest
            {
                projectPath = Application.dataPath.Substring(0,
                    Application.dataPath.LastIndexOf("/Assets", StringComparison.Ordinal)),
                packageId = id,
                version = version
            });
        }
    }
}