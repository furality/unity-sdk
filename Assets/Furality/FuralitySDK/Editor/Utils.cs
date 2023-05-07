using UnityEngine;
using UnityEngine.Networking;

namespace Furality.Editor
{
    public static class Utils
    {
        public static Texture2D DownloadImage(string url)
        {
            var www = new WWW(url);
            while (!www.isDone)
            {
            }

            return www.texture;
        }
    }
}