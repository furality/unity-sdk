using System.Collections.Generic;
using ImageMagick;

namespace Furality.Editor.Tools.BadgeMaker
{
    internal class ConventionConfig
    {
        public enum PipelineType { Sylva, Umbra, Somna }

        public readonly int NameX, NameY, NameW, NameH;
        public readonly int PronounsX, PronounsY, PronounsW, PronounsH;

        public readonly string TitleBean,   TitleFont;
        public readonly string PronounsBean, PronounsFont;

        public readonly bool PronounsMatchTextColor;

        public readonly Dictionary<string, MagickColor> TierColors;

        public readonly PipelineType Pipeline;

        public ConventionConfig(
            int nameX, int nameY, int nameW, int nameH,
            int pronX,  int pronY,  int pronW,  int pronH,
            string titleBean,    string titleFont,
            string pronounsBean, string pronounsFont,
            bool pronounsMatchTextColor,
            PipelineType pipeline,
            Dictionary<string, MagickColor> tierColors = null)
        {
            NameX = nameX; NameY = nameY; NameW = nameW; NameH = nameH;
            PronounsX = pronX; PronounsY = pronY; PronounsW = pronW; PronounsH = pronH;
            TitleBean   = titleBean;    TitleFont   = titleFont;
            PronounsBean = pronounsBean; PronounsFont = pronounsFont;
            PronounsMatchTextColor = pronounsMatchTextColor;
            Pipeline   = pipeline;
            TierColors = tierColors;
        }

        public MagickColor GetTextColor(string tier) =>
            TierColors != null && TierColors.TryGetValue(tier, out var c) ? c : MagickColors.White;
    }
}