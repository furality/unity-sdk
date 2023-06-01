using System.Collections.Generic;
using System.Linq;
using Furality.SDK.External.Api;
using Furality.SDK.External.Assets;
using Furality.SDK.External.Auth;
using UnityEditor;
using UnityEngine;

namespace Furality.SDK.Pages
{
    public class DownloadsPage : IMenuPage
    {
        private PrivilegeCategory[] _categories =
        {
            new PrivilegeCategory("Public Assets", new List<AssetClass>()),
            new PrivilegeCategory("Attendee Assets", new List<AssetClass>()),
            new PrivilegeCategory("Patreon Assets", new List<AssetClass>()),
        };

        private IPackageDataSource _packageDataSource;
        private IEnumerable<FuralityPackage> downloads;
        private PrivilegeCategory _currentPage;

        public DownloadsPage()
        {
            this._packageDataSource = new TestDataSource();
            this.downloads = _packageDataSource.GetPackages();
            
            _categories[0]._assetClasses = downloads.Where(d => d.IsPublic).GroupBy(d => d.Category)
                .Select(d => new AssetClass(d.Key, d, this._packageDataSource));
            
            _categories[1]._assetClasses = downloads.Where(d => !d.IsPublic && d.PatreonLevel == PatreonLevel.None).GroupBy(d => d.Category)
                .Select(d => new AssetClass(d.Key, d, this._packageDataSource));
            
            _categories[2]._assetClasses = downloads.Where(d => d.PatreonLevel != PatreonLevel.None).GroupBy(d => d.Category)
                .Select(d => new AssetClass(d.Key, d, this._packageDataSource));

            _currentPage = _categories[0];
        }
        
        public void Draw()
        {
            GUILayout.BeginHorizontal();
            foreach (var privilegeCategory in _categories)
            {
                bool isSelected = privilegeCategory == _currentPage;
                if (isSelected)
                    GUI.color = new Color(1.2f, 1.2f, 1.2f);
                
                if (GUILayout.Button(privilegeCategory.CategoryName))
                    _currentPage = privilegeCategory;
                
                if (isSelected)
                    GUI.color = Color.white;
            }
            GUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);

            if (AuthManager.CurrentUser == null)
            {
                EditorGUILayout.HelpBox("You must be logged in to download assets. Please log-in using the settings tab", MessageType.Warning);
                return;
            }
            
            _currentPage?.Draw();
        }
    }
}