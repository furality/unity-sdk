using System.Collections.Generic;
using System.Linq;
using Furality.Editor.AssetHandling;
using UnityEngine;

namespace Furality.Editor.Pages
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

            var enumerable = _assetClasses.ToList();
            if (!enumerable.Any())
                return;
            
            _selectedClass = enumerable.First();
        }

        public void Draw()
        {
            GUILayout.BeginHorizontal();
            foreach (var assetClass in _assetClasses)
            {
                bool isSelected = assetClass == _selectedClass;
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