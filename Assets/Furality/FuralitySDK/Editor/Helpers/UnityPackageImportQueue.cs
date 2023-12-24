using System;
using System.IO;
using System.Linq;
using Unity.Jobs;
using UnityEditor;
using UnityEngine;

namespace Furality.SDK.Helpers
{
    public class UnityPackageImportQueue : IJob
    {
        private static string[] ImportQueue
        {
            get
            {
                var queueStr = SessionState.GetString("furality:importQueue", "");
                return string.IsNullOrWhiteSpace(queueStr) ? Array.Empty<string>() : queueStr.Split('\n');
            }
            set => SessionState.SetString("furality:importQueue", string.Join("\n", value));
        }

        private static int SuccessCount
        {
            get => SessionState.GetInt("furality:importSuccessCount", 0);
            set => SessionState.SetInt("furality:importSuccessCount", value);
        }

        private static int ImportGoal
        {
            get => SessionState.GetInt("furality:importGoal", 0);
            set => SessionState.SetInt("furality:importGoal", value);
        }

        private void OnPackageImportComplete(string packageName)
        {
            SuccessCount++;

            if (SuccessCount == ImportGoal)
            {
                AssetDatabase.importPackageCompleted -= OnPackageImportComplete;
                
                SuccessCount = 0;
                ImportGoal = 0;
                ImportQueue = Array.Empty<string>();
                
                AssetDatabase.StopAssetEditing();
                AssetDatabase.Refresh();
                AssetDatabase.AllowAutoRefresh();
                
                Debug.Log("FINISHED IMPORT DONE");
                
                return;
            }
            
            ImportQueue = ImportQueue.Skip(1).ToArray();
            CheckQueue();
        }
        
        public void CheckQueue()
        {
            Debug.Log("Checking import queue...");
            
            if (ImportQueue.Length == 0)
                return;
            
            var package = ImportQueue[0];
            Debug.Log("Importing package: "+package);
            AssetDatabase.ImportPackage(package, false);
            
            Debug.Log("Import queue started. Importing "+ImportQueue.Length+" packages.");
        }
        
        public static void Add(string package)
        {
            Debug.Log("Adding package to import queue: "+package);
            ImportQueue = ImportQueue.Concat(new[] { package }).ToArray();
        }

        public void Execute()
        {
            AssetDatabase.importPackageCompleted += OnPackageImportComplete;
            ImportGoal = ImportQueue.Length;
            CheckQueue();
        }
    }
}