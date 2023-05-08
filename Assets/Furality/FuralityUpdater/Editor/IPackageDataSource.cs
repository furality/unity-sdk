using System.Collections;
using System.Collections.Generic;
using Furality.Editor.AssetHandling;

namespace Furality.FuralityUpdater.Editor
{
    public interface IPackageDataSource
    {
        Package GetPackage(string id);  // Returns a package
        
        IEnumerable<FuralityPackage> GetPackages();  // Returns all packages
        
        FuralityPackage GetSdkPackage();  // Returns the SDK package

        FuralityPackage GetUpdaterPackage();
    }
}