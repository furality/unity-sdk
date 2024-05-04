/**
https://github.com/VRCFury/vrcfury-installer?tab=License-1-ov-file#readme
Note: This license applies to vrcfury-installer ONLY. VRCFury itself contains a separate license.

Copyright VRCFury Contributors

Permission is hereby granted, free of charge, to any person obtaining
a copy of this software and associated documentation files (the
"Software"), to deal in the Software without restriction, including
without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to
permit persons to whom the Software is furnished to do so, subject to
the following conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

[InitializeOnLoad]
public class FuralitySDKInstaller { 
    
    private static readonly HttpClient HttpClient = new HttpClient();
    
    static FuralitySDKInstaller() {
        Task.Run(async () => {
            try {
                await InstallUnsafe();
            } catch(Exception e) {
                Debug.LogException(e);
                await DisplayDialog(
                    "Furality encountered an error while installing." +
                    " If the issue repeats, try re-downloading or ask on the" +
                    " discord: https://discord.gg/furality\n\n" +
                    e.Message + "\nCheck the unity console for details.");
            }
        });
    }

    private static async Task InstallUnsafe() {
        if (HasLocalDirectoryFuralityPackage()) {
            Log("Not running, because you have a Furality SDK package installed in development mode (local directory)");
            return;
        }
        
        Log("Starting ...");
        
        var restarting = await InMainThread(() => {
            var changed = false;
            changed |= CleanManifest(true);
            return changed;
        });
        if (restarting) {
            await InMainThread(() => { RefreshPackages(); });
            // Unity will probably unload us during this pause, but that's fine, we'll just start over.
            // We need to make sure unity has totally forgotten about com.furality.sdk before we install
            // the new one, otherwise it will delete our new com.furality.sdk folder when it cleans up
            // the upm package.
            await Task.Delay(10000);
        }

        var url = "https://github.com/furality/unity-sdk/releases/latest/download/com.furality.sdk.zip";
        Log("Downloading ...");
        var tempFile = await InMainThread(FileUtil.GetUniqueTempPathInProject) + ".zip";
        try {
            using (var response = await HttpClient.GetAsync(url)) {
                response.EnsureSuccessStatusCode();
                using (var fs = new FileStream(tempFile, FileMode.CreateNew)) {
                    await response.Content.CopyToAsync(fs);
                }
            }
        } catch (Exception e) {
            throw new Exception($"Failed to download {url}\n{e.Message}", e);
        }

        Log("Extracting ...");
        var tmpDir = await InMainThread(FileUtil.GetUniqueTempPathInProject);
        using (var stream = File.OpenRead(tempFile)) {
            using (var archive = new ZipArchive(stream)) {
                foreach (var entry in archive.Entries) {
                    if (string.IsNullOrWhiteSpace(entry.Name)) continue;
                    var outPath = tmpDir+"/"+entry.FullName;
                    var outDir = Path.GetDirectoryName(outPath);
                    if (!Directory.Exists(outDir)) Directory.CreateDirectory(outDir);
                    using (var entryStream = entry.Open()) {
                        using (var outFile = new FileStream(outPath, FileMode.Create, FileAccess.Write)) {
                            await entryStream.CopyToAsync(outFile);
                        }
                    }
                }
            }
        }

        await InMainThread(() => {
            var appRootDir = Path.GetDirectoryName(Application.dataPath);
            Directory.CreateDirectory(appRootDir + "/Temp/furalityInstalling");
            
            CleanManifest(false);
            Delete("Assets/FuralitySDK-installer");
            Delete("Packages/com.furality.sdk-1.2.5.tgz");
            Delete("Packages/com.furality.updater-1.0.3.tgz");
            Delete("Packages/com.furality.updater-1.0.4.tgz");

            Log($"Moving {tmpDir} to Packages/com.furality.sdk");
            Directory.Move(tmpDir, "Packages/com.furality.sdk");

            RefreshPackages();
        });
    }

    private static void RefreshPackages() {
        Log("Triggering Package Resolve ...");
        MethodInfo method = typeof(Client).GetMethod("Resolve",
            BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public,
            null,
            new Type[] {},
            null
        );
        method.Invoke(null, null); 
    }

    private static bool HasLocalDirectoryFuralityPackage() {
        var manifestPath = "Packages/manifest.json";
        if (!File.Exists(manifestPath)) return false;
        var lines = File.ReadLines(manifestPath).ToArray();
        return lines.Any(line => line.Contains("com.furality.") && line.Contains("file:") && !line.Contains("tgz"));
    }

    private static bool CleanManifest(bool mainOnly) {
        var manifestPath = "Packages/manifest.json";
        if (!File.Exists(manifestPath)) return false;
        var lines = File.ReadLines(manifestPath).ToArray();
        bool ShouldRemoveLine(string line) {
            var remove = line.Contains("org.furality.") && (!mainOnly || line.Contains("org.furality.sdk"));
            if (remove) {
                Log($"Removing manifest line: {line}");
            }
            return remove;
        }
        var linesToKeep = lines.Where(l => !ShouldRemoveLine(l)).ToArray();
        if (lines.Length == linesToKeep.Length) return false;
        var tempManifestPath = FileUtil.GetUniqueTempPathInProject();
        File.WriteAllLines(tempManifestPath, linesToKeep);
        File.Delete(manifestPath);
        File.Move(tempManifestPath, manifestPath);
        return true;
    }

    private static bool Delete(string path) {
        if (string.IsNullOrWhiteSpace(path)) return false;
        if (Directory.Exists(path)) {
            Log("Deleting directory: " + path);
            Directory.Delete(path, true);
            return true;
        }
        if (File.Exists(path)) {
            Log("Deleting file: " + path);
            File.Delete(path);
            return true;
        }

        return false;
    }

    private static async Task DisplayDialog(string msg) {
        await InMainThread(() => {
            EditorUtility.DisplayDialog(
                "Furality SDK Installer",
                msg,
                "Ok"
            );
        });
    }
    
    private static async Task InMainThread(Action fun) {
        await InMainThread<object>(() => { fun(); return null; });
    }
    private static Task<T> InMainThread<T>(Func<T> fun) {
        var promise = new TaskCompletionSource<T>();
        void Callback() {
            try {
                promise.SetResult(fun());
            } catch (Exception e) {
                promise.SetException(e);
            }
        }
        EditorApplication.delayCall += Callback;

        return promise.Task;
    }

    private static void Log(string message) {
        Debug.Log($"Furality SDK Installer > {message}");
    }
}