using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Furality.FuralityUpdater.Editor
{
    [InitializeOnLoad]
    public static class AutoUpdater
    {
        static AutoUpdater()
        {
            if (Application.isPlaying) return;
            Task.Run(Check);
        }

        private static async void Check()
        {
            var dataSource = new TestDataSource();
            var package = dataSource.GetSdkPackage();
            await DependencyManager.UpgradeOrInstall(package, false, dataSource);
        }
    }
}