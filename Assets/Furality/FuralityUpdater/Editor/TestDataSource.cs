using System;
using System.Collections.Generic;
using Furality.Editor;
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
        
        public Package GetPackage(string id)
        {
            if (id == "org.furality.updater")
            {
                return GetSdkPackage();
            }
            if (id == "org.furality.sdk")
            {
                return GetSdkPackage();
            }
            return _packages.Find(x => x.Id == id);
        }
        
        public IEnumerable<FuralityPackage> GetPackages()
        {
            return _packages;
        }

        public FuralityPackage GetSdkPackage()
        {
            return new FuralityPackage()
            {
                Id = "org.furality.updater",
                Version = new Version(0, 0, 1),
                DownloadUrl = "https://furality.online/assets/img/logo/furality.png",
                Dependencies = { }
            };
        }

        public FuralityPackage GetUpdaterPackage()
        {
            throw new System.NotImplementedException();
        }
    }
}