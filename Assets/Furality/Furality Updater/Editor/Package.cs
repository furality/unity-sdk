using System;
using System.Collections.Generic;

namespace Furality.FuralityUpdater.Editor
{
    public class Package
    {
        public string Id;
        public Version Version;
        public string DownloadUrl;
        public Dictionary<string, Version> Dependencies;    // Id, Version
    }
}