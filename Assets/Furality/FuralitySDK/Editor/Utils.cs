using UnityEngine;
using UnityEngine.Networking;

namespace Furality.SDK.Utils
{
    public static class Utils
    {
        public static Texture2D DownloadImage(string url)
        {
#pragma warning disable CS0618
            var www = new WWW(url);
#pragma warning restore CS0618
            while (!www.isDone)
            {
            }

            return www.texture;
        }
    }
}