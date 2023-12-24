﻿using System.Collections.Generic;
using System.Linq;
using Furality.SDK.Helpers;
using Furality.SDK.Pages;
using UnityEditor;
using UnityEngine;

namespace Furality.SDK.External.Assets
{
    public class AssetClass
    {
        public string Name;
        public IEnumerable<IGrouping<string, FuralityPackage>> _downloads;
        private Dictionary<string, bool> _foldOutStates = new Dictionary<string, bool>();
        private static Dictionary<string, Texture2D> _imageCache = new Dictionary<string, Texture2D>();
        private List<IPackageDataSource> _packageDataSources;
        private bool _isDownloading;
        private Vector2 _scrollPos;
        private readonly Dictionary<string, string> _downloadVersionCache = new Dictionary<string, string>();

        public AssetClass(string name, IEnumerable<FuralityPackage> downloads = null, List<IPackageDataSource> dataSources = null)
        {
            Name = name;
            _downloads = downloads.GroupBy(d=>d.ConventionId);
            _packageDataSources = dataSources;
            
            if (_downloads == null) return;

            foreach (var download in _downloads)
            {
                _foldOutStates.Add(download.Key, false);
                foreach (var item in download)
                {
                    if (_imageCache.ContainsKey(item.ImageUrl)) continue;
                    _imageCache.Add(item.ImageUrl, DownloadHelper.DownloadImage(item.ImageUrl));
                }
            }
            
            // Set the highest foldout to true
            _foldOutStates[_foldOutStates.Keys.First()] = true;
        }

        private void OnPackageImported(string packageName)
        {
            Debug.Log("Package Imported: "+packageName);

            _isDownloading = false;
            AssetDatabase.importPackageCompleted -= OnPackageImported;
        }
        
        private void OnPackageImportCancelled(string packageName)
        {
            Debug.Log("Package Import Cancelled");
            _isDownloading = false;
            AssetDatabase.importPackageCancelled -= OnPackageImportCancelled;
        }
        
        private void OnPackageImportFailed(string packageName, string errorMessage)
        {
            Debug.Log("Package Import Failed: "+errorMessage);
            _isDownloading = false;
            AssetDatabase.importPackageFailed -= OnPackageImportFailed;
        }

        private async void BeginInstall(FuralityPackage package)
        {
            _isDownloading = true;
            AssetDatabase.importPackageCompleted += OnPackageImported;
            AssetDatabase.importPackageCancelled += OnPackageImportCancelled;
            AssetDatabase.importPackageFailed += OnPackageImportFailed;
            await DependencyManager.UpgradeOrInstall(package, false);
        }
        
        public void Draw()
        {
            
            if (_downloads == null)
            {
                return;
            }

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.ExpandHeight(true));
            
            foreach (var category in _downloads)
            {
                _foldOutStates[category.Key] = EditorGUILayout.Foldout(_foldOutStates[category.Key], category.Key);

                if (!_foldOutStates[category.Key])
                {
                    continue;
                }
                

                // Sort by attendance lvel
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
                                GUIStyle myCustomStyle = new GUIStyle(GUI.skin.GetStyle("label"))
                                {
                                    wordWrap = true
                                };
                                GUILayout.Label(download.Description, myCustomStyle);
                            }
                            GUILayout.EndVertical();

                            if (_imageCache.TryGetValue(download.ImageUrl, out var value))
                            {
                                GUILayout.FlexibleSpace();
                                GUILayout.Box(value, GUILayout.Width(Screen.width / 3),
                                    GUILayout.Height(Screen.width / 3 * (9f / 16f)));
                                GUILayout.FlexibleSpace();
                            }
                        }
                        GUILayout.EndHorizontal();

                        GUI.enabled = !_isDownloading;
    
                        // Check if the version is cached. If not, add it to the cache
                        //TODO: Move this to a delegate so it only gets run after package install or on construct
                        if (!_downloadVersionCache.TryGetValue(download.Id, out var installedPackageVersion))
                        {
                            installedPackageVersion = _packageDataSources.Select(pds => pds.GetInstalledPackage(download.Id)).FirstOrDefault(p => !string.IsNullOrEmpty(p));
                            
                            if (!string.IsNullOrEmpty(installedPackageVersion))
                                _downloadVersionCache.Add(download.Id, installedPackageVersion);
                        }
                        if (installedPackageVersion != null)
                        {
                            if (download.Version != installedPackageVersion)
                            {
                                if (GUILayout.Button("Upgrade"))
                                {
                                    BeginInstall(download);
                                }
                            }
                            else
                            {
                                GUI.enabled = false;
                                GUILayout.Button("Installed");
                                GUI.enabled = !_isDownloading;
                            }
                        }
                        else
                        {
                            // Render the download button
                            if (GUILayout.Button(_isDownloading ? "Installing" : "Download"))
                            {
                                BeginInstall(download);
                            }
                        }

                        GUI.enabled = true;
                    }
                    GUILayout.EndVertical();
                }
            }
            
            EditorGUILayout.EndScrollView();
        }
    }
}