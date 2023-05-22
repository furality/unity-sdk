using System;
using Furality.FuralityUpdater.Editor;
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
    }
}