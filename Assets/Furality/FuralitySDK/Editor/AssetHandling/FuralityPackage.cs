using System;
using System.Collections.Generic;
using Furality.FuralityUpdater.Editor;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;

namespace Furality.Editor.AssetHandling
{
    public class FuralityPackage : Package
    {
        public string Name;
        public string Description;
        public string ImageUrl;
        public string ConventionId;
        public string Category;
        public bool IsPublic;
        public bool IsPatreon;
        public DateTime CreatedAt;

        [JsonIgnore]
        public Texture2D Image;
    }
}