using System.IO;
using UnityEngine;

namespace Furality.Editor.Tools.BadgeMaker
{
    internal class Utils
    {
        public static string FontPath =>
            Path.Combine(Application.persistentDataPath, "Fonts");

        public static string BadgeMakerEditorPath =>
            Path.Combine(Application.dataPath, "Furality", "BadgeMaker", "Editor");

        public static string BadgeFolderRoot(string convention, string tier) =>
            Path.Combine("Assets", "Furality", convention, "Avatar Assets", "Badges", tier);
    }
}