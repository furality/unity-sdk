using System;
using System.Collections.Generic;

namespace Furality.SDK
{
    public class Package
    {
        public virtual string Id { get; set; }
        public virtual Version Version { get; set; }
        public virtual Dictionary<string, Version> Dependencies { get; set; } = new Dictionary<string, Version>();    // Id, Version
    }
}