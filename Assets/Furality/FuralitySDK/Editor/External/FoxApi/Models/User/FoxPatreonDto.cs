using System;

namespace Furality.SDK.External.Api.Models.User
{
    [Serializable]
    public class FoxPatreonDto
    {
        public string tier;

        public PatreonLevel GetTier() => Enum.TryParse(tier, true, out PatreonLevel level) ? level : PatreonLevel.None;
    }
}