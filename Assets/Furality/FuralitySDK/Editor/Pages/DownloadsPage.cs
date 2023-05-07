using System.Collections.Generic;
using System.Linq;
using Furality.Editor.AssetHandling;
using Furality.FuralityUpdater.Editor;
using UnityEditor;
using UnityEngine;

namespace Furality.Editor.Pages
{
    public class DownloadsPage : IMenuPage
    {
        private PrivilegeCategory[] _categories =
        {
            new PrivilegeCategory("Public Assets", new List<AssetClass>()),
            new PrivilegeCategory("Attendee Assets", new List<AssetClass>()),
            new PrivilegeCategory("Patreon Assets", new List<AssetClass>()),
        };

        private IEnumerable<FuralityPackage> downloads;
        
        public DownloadsPage()
        {
            this.downloads = new TestDataSource().GetPackages();
        }
        
        private PrivilegeCategory _currentPage;

        public void Draw()
        {
            _categories[0]._assetClasses = downloads.Where(d => d.IsPublic).GroupBy(d => d.Category)
                .Select(d => new AssetClass(d.Key, d));
            
            _categories[1]._assetClasses = downloads.Where(d => !d.IsPublic && !d.IsPatreon).GroupBy(d => d.Category)
                .Select(d => new AssetClass(d.Key, d));

            _categories[2]._assetClasses = downloads.Where(d => d.IsPatreon).GroupBy(d => d.Category)
                .Select(d => new AssetClass(d.Key, d));


            if (_currentPage == null)
            {
                _currentPage = _categories[0];
            }

            GUILayout.BeginHorizontal();
            foreach (var privilegeCategory in _categories)
            {
                if (GUILayout.Button(privilegeCategory.CategoryName))
                {
                    _currentPage = privilegeCategory;
                }
            }
            GUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);

            _currentPage?.Draw();
        }
    }
}