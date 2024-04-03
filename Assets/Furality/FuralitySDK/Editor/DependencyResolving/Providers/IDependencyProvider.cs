using System;
using System.Threading.Tasks;

namespace Furality.SDK.DependencyResolving
{
    public interface IDependencyProvider
    {
        // Complete all steps necessary to download and install this package
        Task<bool> Resolve(Package package);
    }
}