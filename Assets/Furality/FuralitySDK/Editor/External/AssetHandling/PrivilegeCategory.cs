using System.Collections.Generic;
using System.Linq;
using Furality.SDK.External.Api;
using UnityEditor;
using UnityEngine;

namespace Furality.SDK.External.Assets
{
    public class PrivilegeCategory
    {
        public readonly string CategoryName;
        public IEnumerable<AssetClass> _assetClasses;
        
        private AssetClass _selectedClass; 
        
        public PrivilegeCategory(string categoryName, IEnumerable<AssetClass> assetClasses)
        {
            CategoryName = categoryName;
            _assetClasses = assetClasses;
        }

        public void Draw()
        {
            if (_selectedClass == null)
                _selectedClass = _assetClasses.First();

            if (CategoryName == "Patreon Assets" && FoxUser.PatreonLevel <= PatreonLevel.Green)
            {
                EditorGUILayout.HelpBox("You need to be a Patreon to access this content", MessageType.Warning);
                return;
            }
            
            GUILayout.BeginHorizontal();
            foreach (var assetClass in _assetClasses)
            {
                bool isSelected = assetClass?.Name == _selectedClass?.Name;
                if (isSelected)
                {
                    GUI.color = new Color(1.2f, 1.2f, 1.2f);
                }
                
                if (GUILayout.Button(assetClass.Name, GUILayout.ExpandWidth(true)))
                {
                    _selectedClass = assetClass;
                }
                
                if (isSelected)
                {
                    GUI.color = Color.white;
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical();
            _selectedClass?.Draw();
            GUILayout.EndVertical();
        }
    }
}