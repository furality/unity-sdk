using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Furality.FuralityUpdater.Editor
{
    public class ProjectPackage
    {
        public class AddPackageRequest : BasePackageRequest
        {
            public Version version { get; set; }
        }

        public class BasePackageRequest
        {
            public string projectPath { get; set; }

            public string packageId { get; set; }
        }

        public static async Task<bool> AddPackage(AddPackageRequest request)
        {
            var response = await VccComms.Request<string>("projects/packages", "POST", request);
            return response.success;
        }
        
        public static async Task<bool> AddPackage(string packageId, Version version) => await AddPackage(new AddPackageRequest
        {
            projectPath = Application.dataPath,
            packageId = packageId,
            version = version
        });
        
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
    }
}