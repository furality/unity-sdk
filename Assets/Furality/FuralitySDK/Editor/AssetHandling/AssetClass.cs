using System.Collections.Generic;
using System.Linq;
using Furality.Editor.AssetHandling;
using Furality.FuralityUpdater.Editor;
using UnityEditor;
using UnityEngine;

namespace Furality.Editor
{
    public class AssetClass
    {
        public string Name;
        public IEnumerable<IGrouping<string, FuralityPackage>> _downloads;
        private Dictionary<string, bool> _foldOutStates = new Dictionary<string, bool>();

        public AssetClass(string name, IEnumerable<FuralityPackage> downloads = null)
        {
            Name = name;
            _downloads = downloads.GroupBy(d=>d.ConventionId);

            if (_downloads == null) return;

            foreach (var download in _downloads)
            {
                _foldOutStates.Add(download.Key, false);
            }
        }

        public void Draw()
        {
            if (_downloads == null)
            {
                return;
            }

            foreach (var category in _downloads)
            {
                _foldOutStates[category.Key] = EditorGUILayout.Foldout(_foldOutStates[category.Key], category.Key);

                if (!_foldOutStates[category.Key])
                {
                    continue;
                }

                foreach (var download in category)
                {
                    // Render the box
                    GUILayout.BeginVertical("box");
                    {
                        // Render the image in a horizontal layout with the desired constraints
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.FlexibleSpace();
                            GUILayout.BeginVertical();
                            {
                                GUILayout.Label(download.Name);
                                GUILayout.Label(download.Description);
                            }
                            GUILayout.EndVertical();

                            GUILayout.FlexibleSpace();
                            GUILayout.Box(download.Image, GUILayout.Width(Screen.width / 3),
                                GUILayout.Height(Screen.width / 3 * (9f / 16f)));
                            GUILayout.FlexibleSpace();
                        }
                        GUILayout.EndHorizontal();

                        // Render the download button
                        if (GUILayout.Button("Download"))
                        {
                            DependencyManager.UpgradeOrInstall(download, true, new TestDataSource());
                        }
                    }
                    GUILayout.EndVertical();
                }
            }
        }
    }
}