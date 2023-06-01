using System.Collections.Generic;
using System.Threading.Tasks;

namespace Furality.SDK.External.Assets
{
    public interface IPackageDataSource
    {
        IEnumerable<FuralityPackage> GetPackages();  // Returns all packages
        string GetInstalledPackage(string id);
    }
}