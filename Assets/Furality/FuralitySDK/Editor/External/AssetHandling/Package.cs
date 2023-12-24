using System.Collections.Generic;

namespace Furality.SDK
{
    public class Package
    {
        public virtual string Id { get; set; }
        public virtual string Version { get; set; }
        public virtual Dictionary<string, string> Dependencies { get; set; } = new Dictionary<string, string>();    // Id, Version
    }
}