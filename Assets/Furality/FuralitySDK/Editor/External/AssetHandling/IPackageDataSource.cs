using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Furality.SDK.External.Assets
{
    public interface IPackageDataSource
    {
        IEnumerable<FuralityPackage> GetPackages();  // Returns all packages
        [CanBeNull] Version GetInstalledPackage(string id);
    }
}