using System.Collections.Generic;

namespace Furality.SDK
{
    public class Package
    {
        public string Id;
        public string Version;
        public Dictionary<string, string> Dependencies = new Dictionary<string, string>();    // Id, Version
    }
}