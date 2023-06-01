using UnityEngine;

namespace Furality.Furality_Updater.Editor
{
    public class Utils
    {
        public static void Log(string message) => Debug.Log($"[Furality Updater] {message}");
        public static void Warn(string message) => Debug.LogWarning($"[Furality Updater] {message}");
        public static void Error(string message) => Debug.LogError($"[Furality Updater] {message}");
    }
}