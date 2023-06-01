using System.Threading.Tasks;

namespace Furality.SDK.DependencyResolving
{
    public interface IDependencyProvider
    {
        Task<bool> Resolve(string id, string version);
    }
}