using System;
using System.Collections.Generic;
using Furality.SDK.External.Assets;

namespace Furality.SDK.External.Api.Models.Files
{
    [Serializable]
    public class FoxFileDto : FuralityPackage
    {
        public string fileUuid;
        public string name;
        public string description;
        public string category;
        public string type;
        public bool supporterFile;
        public string thumbnailUrl;
        public string conventionId;

        public override string Name => name;
        public override string Id => fileUuid;
        public override string Description => description;
        public override string Category => category;
        public override AttendanceLevel AttendanceLevel => Enum.TryParse(type, true, out AttendanceLevel lvl) ? lvl : AttendanceLevel.none;
        public override PatreonLevel PatreonLevel => supporterFile ? PatreonLevel.Blue : PatreonLevel.None;
        public override bool IsPublic => !supporterFile;
        public override string ImageUrl => thumbnailUrl;
        public override string ConventionId => conventionId;

        // Supplementary overrides. This data may not be entirely correct but needs to exist to function properly
        public override Dictionary<string, string> Dependencies => new Dictionary<string, string>
        {
            { "com.furality.sylvashader", "1.3.3" },
        };

        public override string Version => "1.0.0";
    }
}