// Copyright Furality, Inc. 2025

using System;
using System.Collections.Generic;
using System.IO;
using ImageMagick;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Furality.Editor.Tools.BadgeMaker
{	
    public class BadgeMaker : EditorWindow
    {
        private static readonly Dictionary<string, Dictionary<string, MagickColor>> ConventionsToColors = new Dictionary<string, Dictionary<string, MagickColor>>()
        {
            {
                "Furality Umbra", new Dictionary<string, MagickColor>()
                {
                    {"Attendee", new MagickColor("#37ff79")},
                    {"First Class", new MagickColor("#fe3fff")},
                    {"Sponsor", new MagickColor("#ffce49")}
                }
            },
            {
                "Furality Somna", new Dictionary<string, MagickColor>()
                {
                    {"Attendee", new MagickColor("#ffeead")},
                    {"First Class", new MagickColor("#ffeead")},
                    {"Sponsor", new MagickColor("#ffeead")},
                    {"Dream Maker", new MagickColor("#ffeead")},
                    {"Team", new MagickColor("#ffeead")}
                }
            },
        };
 
        [DllImport("Gdi32.dll")]
        private static extern int AddFontResourceEx(string lpFileName, uint fl, IntPtr pdv);

        [DllImport("Gdi32.dll")]
        private static extern bool RemoveFontResourceEx(string lpFileName, uint fl, IntPtr pdv);
        
        private string _badgeName = "Your Name";
        private string _pronouns = "Title/Pronouns";
        private int _badgeTier = -1;
        private int _badgeConvention = -1;
        private bool _applyToMaterial = true;

        // Name Bounds
        private const int NameX = 375, NameY = 700;
        private const int NameWidth = 610, NameHeight = 150;

        // Pronouns Bounds
        private const int PronounsX = 450, PronounsY = 810;
        private const int PronounsWidth = 449, PronounsHeight = 75;

        private string FontPath => Application.persistentDataPath + "/Fonts/";

        private const string TitleFontName = "Roboto-BoldItalic.ttf";
        private const string PronounsFontName = "Roboto-BoldItalic.ttf";


        [MenuItem("Furality/Show Badge Maker")]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            BadgeMaker window = (BadgeMaker)GetWindow(typeof(BadgeMaker));
            window.titleContent = new GUIContent("Furality Badge Maker");
            window.minSize = new Vector2(350, 400);
            window.Show();
        }

        private void UnloadFonts()
        {
            RemoveFontResourceEx(FontPath + TitleFontName, 0, IntPtr.Zero);
            RemoveFontResourceEx(FontPath + PronounsFontName, 0, IntPtr.Zero);
        }

        private void OnDestroy() => UnloadFonts();

        string MakeBadgeFolder(string convention, string tier) =>
            "Assets/Furality/" + convention + "/Avatar Assets/Badges/" + tier;

        void LoadFonts()
        {
            string titleFontPath = Path.Combine(Application.dataPath,
                "Furality\\BadgeMaker\\Editor\\f8-font.bean");
            titleFontPath = titleFontPath.Replace('/', '\\');
                
            string pronounsFontPath = Path.Combine(Application.dataPath,
                "Furality\\BadgeMaker\\Editor\\f8-font.bean");
            pronounsFontPath = titleFontPath.Replace('/', '\\');

            // Ensure the font path exists and copy it to there, while ensuring the new name matches the FontName
            if (!System.IO.Directory.Exists(FontPath))
                System.IO.Directory.CreateDirectory(FontPath);

            UnloadFonts();
                
            // Copy the file over
            File.Copy(
                Path.Combine(Application.dataPath, "Furality\\BadgeMaker\\Editor\\f8-font.bean"),
                FontPath + TitleFontName, true);
            File.Copy(
                Path.Combine(Application.dataPath, "Furality\\BadgeMaker\\Editor\\f8-font.bean"),
                FontPath + PronounsFontName, true);

            int returnFontSize = AddFontResourceEx(FontPath + TitleFontName, 0, IntPtr.Zero);
            if (returnFontSize == 0)
                Debug.LogError("Failed to add font resource: " + FontPath + TitleFontName);
                
            returnFontSize = AddFontResourceEx(FontPath + PronounsFontName, 0, IntPtr.Zero);
            if (returnFontSize == 0)
                Debug.LogError("Failed to add font resource: " + FontPath + PronounsFontName);
        }
        
        void OnGUI()
        { 
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Badge Maker", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            List<string> tierNames = new List<string>();
            List<string> conventionNames = new List<string>();
            var conventionFolders = AssetDatabase.GetSubFolders("Assets/Furality");
            foreach (var conventionFolder in conventionFolders)
            {
                var tiers = AssetDatabase.GetSubFolders(Path.Combine(conventionFolder, "Avatar Assets/Badges"));
                if (tiers.Length == 0) continue;

                var splitConventionFolder = conventionFolder.Split('/');
                tierNames.AddRange(tiers.Select(tier => tier.Split('/')[^1]));
                conventionNames.Add(splitConventionFolder[^1]);
            }
            
            // If our selected tier is -1, this is the first time we open the window, so we select the highest tier (this doesn't work too well for people with multiple tiers but works well enough)
            if (_badgeTier == -1)
                _badgeTier = tierNames.Count - 1;

            if (_badgeConvention == -1)
                _badgeConvention = conventionNames.Count - 1;
            
            // If there were no folders found, show a warning saying that you need badges imported
            if (tierNames.Count == 0 || conventionNames.Count == 0)
            {
                EditorGUILayout.HelpBox("No badges found! Please download badges from the downloads tab.", MessageType.Warning);
                return;
            }
            
            // Text field for the badge name
            _badgeName = EditorGUILayout.TextField("Badge Name", _badgeName);
            _pronouns = EditorGUILayout.TextField("Title", _pronouns);
            
            // Create a dropdown menu for the badge type but only show the folder name
            _badgeTier = EditorGUILayout.Popup("Badge Type", _badgeTier, tierNames.ToArray());
            _badgeConvention = EditorGUILayout.Popup("Convention", _badgeConvention, conventionNames.ToArray());

            // Checkbox to apply the new texture to the material  
            _applyToMaterial = EditorGUILayout.Toggle("Auto-Apply to Base Material", _applyToMaterial);

            // Button to create the badge
            if (GUILayout.Button("Create Badge"))
            {
                if (!ConventionsToColors.ContainsKey(conventionNames[_badgeConvention]))
                {
                    Debug.LogError("Convention could not be found in color map. Quitting BadgeMaker");
                    return;
                }

                if (!ConventionsToColors[conventionNames[_badgeConvention]].ContainsKey(tierNames[_badgeTier]))
                {
                    Debug.LogError("Badge tier could not be found in color map. Quitting BadgeMaker");
                    return;
                }
                
                var textColor = ConventionsToColors[conventionNames[_badgeConvention]][tierNames[_badgeTier]];
                
                EditorUtility.DisplayProgressBar("Creating Badge", "Loading Font...", 0.125f);

                // get the path to the currently selected folder + Textures
                string folderPath = System.IO.Path.Combine(MakeBadgeFolder(conventionNames[_badgeConvention], tierNames[_badgeTier]), "Textures");

                // By default (pin), we just need to select image name tierName+_Empty.png
                string fileName = "Badge" + Regex.Replace(tierNames[_badgeTier], @"\s+", "");
                
                // Create a save path and ensure the folder exists. We want the image to be saved in a folder named "Custom" relative to the original image
                string outPath = System.IO.Path.Combine(folderPath, "Custom");
                if (!System.IO.Directory.Exists(outPath))
                    System.IO.Directory.CreateDirectory(outPath);
                outPath += "CUSTOM_" + _badgeName;

                LoadFonts();
                
                EditorUtility.DisplayProgressBar("Creating Badge", "Creating Name Text...", 0.25f);

                MagickImage nameImage = null;
                MagickImage pronounsImage = null;

                MagickImage nameImageWhite = null;
                MagickImage pronounsImageWhite = null;

                if (!string.IsNullOrEmpty(_badgeName))
                {
                    var fontFamily = System.IO.Path.Combine(FontPath, TitleFontName);
                    nameImage = FindFontSize(fontFamily, _badgeName, NameWidth, NameHeight, textColor);
                    nameImageWhite = FindFontSize(fontFamily, _badgeName, NameWidth, NameHeight, new MagickColor("#ffffff"));
                }

                EditorUtility.DisplayProgressBar("Creating Badge", "Creating Title Text...", 0.175f);

                if (!string.IsNullOrEmpty(_pronouns))
                {
                    pronounsImage = FindFontSize(System.IO.Path.Combine(FontPath, PronounsFontName), _pronouns, PronounsWidth, PronounsHeight,
                        new MagickColor("#ffffff"));
                    pronounsImageWhite = pronounsImage; // We can reuse the image because the pronouns are always white anyway
                }

                EditorUtility.DisplayProgressBar("Creating Badge", "Compositing main texture...", 0.5f);

                // Create the badge
                CreateBadge(folderPath + fileName + "_DIF.png", nameImage, pronounsImage, outPath + ".png");

                EditorUtility.DisplayProgressBar("Creating Badge", "Compositing emission texture...", 0.625f);

                // Another for the metallic
                CreateBadge(folderPath + "Others/Material.001_Metallic.png", nameImageWhite, pronounsImageWhite, outPath + "_Metallic.png");

                // Now construct a new masks texture using the generated metallic alpha combined with the GBA of the existing masks targa
                using (MagickImage baseImage = new MagickImage(outPath + "_Metallic.png"))
                using (MagickImage maskImage = new MagickImage(folderPath + fileName + "_MASKS.tga"))
                {
                    maskImage.Flip();   // God literally why do I need to do this
                    
                    var separated = maskImage.Separate().ToList();

                    var newMaskImage = new MagickImageCollection()
                    {
                        baseImage,
                        separated[1],
                        separated[2],
                        separated[3],
                    };
                    
                    using (var output = newMaskImage.Combine())
                    {
                        output.Write(outPath + "_MASK.png");;
                    }
                }
                
                AssetDatabase.Refresh();

                EditorUtility.DisplayProgressBar("Creating Badge", "Applying mipmaps...", 0.75f);

                // Apply mipmaps
                TextureImporter importer = AssetImporter.GetAtPath(outPath + ".png") as TextureImporter;
                importer.streamingMipmaps = true;
                importer.SaveAndReimport();
                TextureImporter metallicImporter = AssetImporter.GetAtPath(outPath + "_MASK.png") as TextureImporter;
                metallicImporter.streamingMipmaps = true;
                metallicImporter.sRGBTexture = false;   // Messes with colors n stuff
                metallicImporter.SaveAndReimport();

                if (_applyToMaterial)
                {
                    EditorUtility.DisplayProgressBar("Creating Badge", "Applying to material...", 0.875f);

                    // Find the material named Attendee in the folders[_badgeTier]+Materials folder
                    Material material =
                        AssetDatabase.LoadAssetAtPath<Material>($"{MakeBadgeFolder(conventionNames[_badgeConvention], tierNames[_badgeTier])}/Material/Badge{ Regex.Replace(tierNames[_badgeTier], @"\s+", "")}.mat");
                    // Load the new texture
                    Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(outPath + ".png");
                    Texture2D mask = AssetDatabase.LoadAssetAtPath<Texture2D>(outPath + "_MASK.png");
                    // Set the texture to the material
                    material.SetTexture("_MainTex", texture);
                    material.SetTexture("_MaskMap01", mask);
                    //material.SetTexture("_EmissionMap", emission);
                    // Save the material
                    AssetDatabase.SaveAssets();
                }

                EditorUtility.DisplayProgressBar("Creating Badge", "Unloading Font...", 1);

                // Remove the font resource
                bool success = RemoveFontResourceEx(FontPath + TitleFontName, 0, IntPtr.Zero);
                if (!success)
                {
                    Debug.LogWarning("Failed to unload font resource");
                    return;
                }
                
                success = RemoveFontResourceEx(FontPath + PronounsFontName, 0, IntPtr.Zero);
                if (!success)
                {
                    Debug.LogWarning("Failed to unload font resource");
                    return;
                }

                // Delete the font file
                File.Delete(FontPath + TitleFontName);
                File.Delete(FontPath + PronounsFontName);

                EditorUtility.ClearProgressBar();
            }
        }

        private void CreateBadge(string filePath, MagickImage nameImage, MagickImage pronounsImage, string outPath)
        {
            using (MagickImage image = new MagickImage(filePath))
            {
                /*image.Draw(new DrawableStrokeColor(MagickColors.Red));
                image.Draw(new DrawableFillColor(MagickColors.Transparent));
                image.Draw(new DrawableRectangle(NameX - NameWidth / 2, NameY - NameHeight / 2, NameX + NameWidth / 2, NameY + NameHeight / 2));
                image.Draw(new DrawableRectangle(PronounsX - PronounsWidth / 2, PronounsY - PronounsHeight / 2, PronounsX + PronounsWidth / 2, PronounsY + PronounsHeight / 2));*/
                
                if (nameImage != null)
                {
                    // Figure out the position to draw the text given its current size and the fact NameX and NameY are where we want the center of the text to be
                    int tempNameX = NameX - (int)(nameImage.Width / 2);
                    int tempNameY = NameY - (int)(nameImage.Height / 2);

                    image.Composite(nameImage, tempNameX, tempNameY, CompositeOperator.Atop);
                }

                if (pronounsImage != null)
                {
                    int tempPronounsX = PronounsX - (int)(pronounsImage.Width / 2);
                    int tempPronounsY = PronounsY - (int)(pronounsImage.Height / 2);

                    image.Composite(pronounsImage, tempPronounsX, tempPronounsY, CompositeOperator.Atop);
                }

                // Draw a box to illustrate the bounds of NameX and NameY and PronounsX and PronounsY including their sizes
                image.Write(outPath);
            }
        }

        private static MagickImage FindFontSize(string fontFamily, string text, int desiredWidth, int desiredHeight, MagickColor color)
        {
            // Use imagemagick to find the font size that fits the text in the desired width and height
            // Using the equivalent of the following command:
            // convert -background none -fill white -font fontPath -pointsize 1 -size 100x100 caption:"text" -trim -format "%[fx:round(h)]" info:
            // Ensure the text doesn't go onto a new line
            MagickImage image = new MagickImage($"label:{text}", new MagickReadSettings
            {
                BackgroundColor = MagickColors.None,
                FillColor = color,
                Font = fontFamily,
                Width = desiredWidth,
                Height = desiredHeight,
            });

            image.Trim();
            return image;
        }
    }
}