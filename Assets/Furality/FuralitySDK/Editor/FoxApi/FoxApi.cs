using System.Threading.Tasks;

namespace Furality.FuralityUpdater.Editor
{
    public class FoxApi
    {
        private string _token;
        public static FoxApi Instance;
        
        public FoxApi(string token)
        {
            _token = token;
            Instance = this;
        }
        
        public async Task<string> PreSignDownload(string fileId)
        {
            //TODO: Implement the GET req
            return "https://furality.org";
        }
    }
}