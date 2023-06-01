using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Furality.FuralitySDK.Editor
{
    public static class UnityPackageImportQueue
    {
        private static readonly Queue<string> ImportQueue = new Queue<string>();
        private static int SuccessCount = 0;

        private static void OnPackageImportComplete(string packageName)
        {
            SuccessCount++;
        }
        
        public static void CheckQueue()
        {
            Debug.Log("Checking import queue...");
            
            if (ImportQueue.Count == 0)
                return;

            AssetDatabase.importPackageCompleted += OnPackageImportComplete;
            foreach (var package in ImportQueue)
            {
                Debug.Log("Importing package: "+Path.GetFileNameWithoutExtension(package));
                AssetDatabase.ImportPackage(package, false);
            }
            AssetDatabase.importPackageCompleted -= OnPackageImportComplete;
            
            Debug.Log("Import queue started. Importing "+ImportQueue.Count+" packages.");
            ImportQueue.Clear();
        }
        
        public static void Add(string package)
        {
            Debug.Log("Adding package to import queue: "+package);
            ImportQueue.Enqueue(package);
        }
    }
}