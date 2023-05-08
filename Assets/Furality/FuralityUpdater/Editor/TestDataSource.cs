using System;
using System.Collections.Generic;
using Furality.Editor.AssetHandling;

namespace Furality.FuralityUpdater.Editor
{
    public class TestDataSource : IPackageDataSource
    {
        private List<FuralityPackage> _packages = new List<FuralityPackage>() {
            new FuralityPackage()
            {
                Id = "reee",
                Name = "Reee title",
                Description = "ree description",
                ImageUrl = "https://furality.online/assets/img/logo/furality.png",
                Category = "Shaders",
                ConventionId = "SampleConvention01",
                Dependencies = new Dictionary<string, Version>(),
                //Image = Utils.DownloadImage("https://furality.online/assets/img/logo/furality.png")
            },
            new FuralityPackage()
            {
                Id = "reee",
                Name = "Reee title",
                Description = "ree description",
                ImageUrl = "https://furality.online/assets/img/logo/furality.png",
                Category = "Shaders",
                ConventionId = "SampleConvention02",
                Dependencies = new Dictionary<string, Version>(),
                //Image = Utils.DownloadImage("https://furality.online/assets/img/logo/furality.png")
            }
        };
        
        public Package GetPackage(string id) => _packages.Find(x => x.Id == id);
        
        
        public IEnumerable<FuralityPackage> GetPackages() => _packages;
    }
}