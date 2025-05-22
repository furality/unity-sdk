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

        private static readonly int MainTex = Shader.PropertyToID("_MainTex");
        private static readonly int MaskMap01 = Shader.PropertyToID("_MaskMap01");
        private static readonly int EmissionMap = Shader.PropertyToID("_EmissionMap");

        [DllImport("Gdi32.dll")]
        private static extern int AddFontResourceEx(string lpFileName, uint fl, IntPtr pdv);

        [DllImport("Gdi32.dll")]
        private static extern bool RemoveFontResourceEx(string lpFileName, uint fl, IntPtr pdv);
        
        private string _badgeName = "Your Name";
        private string _pronouns = "Title/Pronouns";
        private int _badgeTier = -1;
        private int _badgeConvention = -1;
        private bool _applyToMaterial = true;
        private List<string> _tierNames = new List<string>();
        private List<string> _conventionNames = new List<string>();
        
        // Name Bounds
        private const int NameX = 375, NameY = 700;
        private const int NameWidth = 610, NameHeight = 150;

        // Pronouns Bounds
        private const int PronounsX = 450, PronounsY = 810;
        private const int PronounsWidth = 449, PronounsHeight = 75;

        private static string FontPath => Path.Combine(Application.persistentDataPath, "Fonts");

        private const string FontFileName = "f8-font.bean";
        private const string TitleFontName = "Fraunces_72pt-SemiBold.ttf";
        private const string PronounsFontName = "Fraunces_72pt-SemiBold.ttf";

        void OnEnable()
        {
            var conventionFolders = AssetDatabase.GetSubFolders("Assets/Furality");
            foreach (var conventionFolder in conventionFolders)
            {
                var tiers = AssetDatabase.GetSubFolders(Path.Combine(conventionFolder, "Avatar Assets/Badges"));
                if (tiers.Length == 0) continue;

                var splitConventionFolder = conventionFolder.Split('/');
                _tierNames.AddRange(tiers.Select(tier => tier.Split('/')[^1]));
                _conventionNames.Add(splitConventionFolder[^1]);
            }
        }

        [MenuItem("Furality/Show Badge Maker")]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            BadgeMaker window = (BadgeMaker)GetWindow(typeof(BadgeMaker));
            window.titleContent = new GUIContent("Furality Badge Maker");
            window.minSize = new Vector2(350, 400);
            window.Show();
        }

        private void UnloadAndDeleteFontIfExists(string path)
        {
            if (!File.Exists(path)) return;

            try
            {
                RemoveFontResourceEx(path, 0, IntPtr.Zero);
                File.Delete(path);
            }
            catch
            {
            }
        }

        private void UnloadFonts()
        {
            UnloadAndDeleteFontIfExists(Path.Combine(FontPath, TitleFontName));
            UnloadAndDeleteFontIfExists(Path.Combine(FontPath, PronounsFontName));
        }

        private void OnDestroy() => UnloadFonts();

        string MakeBadgeFolder(string convention, string tier) =>
            Path.Combine("Assets", "Furality", convention, "Avatar Assets", "Badges", tier);

        void CopyAndLoadFont(string srcPath, string fontName)
        {
            var destPath = Path.Combine(FontPath, fontName);

            try
            {
                File.Copy(srcPath, destPath, true);

                int returnFontSize = AddFontResourceEx(destPath, 0, IntPtr.Zero);
                if (returnFontSize == 0)
                    Debug.LogError("Failed to add font resource: " + FontPath + TitleFontName);
            }
            catch
            {
            }
        }
        
        void LoadFonts()
        {
            string srcFontPath = Path.Combine(Application.dataPath, "Furality", "BadgeMaker", "Editor", FontFileName);

            // Ensure the font path exists and copy it to there, while ensuring the new name matches the FontName
            if (!Directory.Exists(FontPath))
               Directory.CreateDirectory(FontPath);

            UnloadFonts();
            
            CopyAndLoadFont(srcFontPath, TitleFontName);
            CopyAndLoadFont(srcFontPath, PronounsFontName);
        }

        void ConstructBadge()
        {
            if (!ConventionsToColors.ContainsKey(_conventionNames[_badgeConvention]))
            {
                Debug.LogError("Convention could not be found in color map. Quitting BadgeMaker");
                return;
            }

            if (!ConventionsToColors[_conventionNames[_badgeConvention]].ContainsKey(_tierNames[_badgeTier]))
            {
                Debug.LogError("Badge tier could not be found in color map. Quitting BadgeMaker");
                return;
            }
                
            var textColor = ConventionsToColors[_conventionNames[_badgeConvention]][_tierNames[_badgeTier]];
                
            EditorUtility.DisplayProgressBar("Creating Badge", "Loading Font...", 0.125f);

            // get the path to the currently selected folder + Textures
            string badgeTexturesDir = Path.Combine(MakeBadgeFolder(_conventionNames[_badgeConvention], _tierNames[_badgeTier]), "Textures");

            // By default (pin), we just need to select image name tierName+_Empty.png
            string fileName = "Badge" + Regex.Replace(_tierNames[_badgeTier], @"\s+", "");
                
            // Create a save path and ensure the folder exists. We want the image to be saved in a folder named "Custom" relative to the original image
            string outPath = Path.Combine(badgeTexturesDir, "Custom");
            if (!Directory.Exists(outPath))
                Directory.CreateDirectory(outPath);
            outPath = Path.Combine(outPath, "CUSTOM_" + Regex.Replace(_badgeName, @"[<>:""/\\|?*]", "_"));
            string metallicOutPath = outPath + "_Metallic";
            string emissionOutPath = outPath + "_Emission";

            LoadFonts();
                
            EditorUtility.DisplayProgressBar("Creating Badge", "Creating Name Text...", 0.25f);

            MagickImage nameImage = null;
            MagickImage pronounsImage = null;

            MagickImage nameImageWhite = null;
            MagickImage pronounsImageWhite = null;

            if (!string.IsNullOrEmpty(_badgeName))
            {
                var fontFamily = Path.Combine(FontPath, TitleFontName);
                nameImage = FindFontSize(fontFamily, _badgeName, NameWidth, NameHeight, textColor);
                nameImageWhite = FindFontSize(fontFamily, _badgeName, NameWidth, NameHeight, new MagickColor("#ffffff"));
            }

            EditorUtility.DisplayProgressBar("Creating Badge", "Creating Title Text...", 0.175f);

            if (!string.IsNullOrEmpty(_pronouns))
            {
                pronounsImage = FindFontSize(Path.Combine(FontPath, PronounsFontName), _pronouns, PronounsWidth, PronounsHeight,
                    new MagickColor("#ffffff"));
                pronounsImageWhite = pronounsImage; // We can reuse the image because the pronouns are always white anyway
            }

            EditorUtility.DisplayProgressBar("Creating Badge", "Compositing main texture...", 0.5f);

            // Create the badge
            CreateBadge(Path.Combine(badgeTexturesDir, fileName + "_DIF.png"), nameImage, pronounsImage, outPath + ".png");

            EditorUtility.DisplayProgressBar("Creating Badge", "Compositing emission texture...", 0.625f);

            // Another for the metallic
            CreateBadge(Path.Combine(badgeTexturesDir, "Others", "Material.001_Metallic.png"), nameImageWhite, pronounsImageWhite, metallicOutPath + ".png");

            // Now construct a new masks texture using the generated metallic alpha combined with the GBA of the existing masks targa
            CompositeMetallicSmoothnessMask(Path.Combine(badgeTexturesDir, fileName + "_MASKS.tga"), metallicOutPath + ".png", outPath + "_MASK.png");
            
            // Aaaaand another for emissino
            CreateBadge(Path.Combine(badgeTexturesDir, fileName + "_EMI.png"), nameImage, pronounsImage, emissionOutPath + ".png");
                
            AssetDatabase.Refresh();

            EditorUtility.DisplayProgressBar("Creating Badge", "Applying mipmaps...", 0.75f);

            // Apply mipmaps
            TextureImporter importer = AssetImporter.GetAtPath(outPath + ".png") as TextureImporter;
            if (!importer)
            {
                Debug.LogError("Failed to import base color texture. Quitting BadgeMaker");
                return;
            }
            importer.streamingMipmaps = true;
            importer.SaveAndReimport();
            
            TextureImporter emissionImporter = AssetImporter.GetAtPath(emissionOutPath + ".png") as TextureImporter;
            if (!emissionImporter)
            {
                Debug.LogError("Failed to import emission texture. Quitting BadgeMaker");
                return;
            }
            emissionImporter.streamingMipmaps = true;
            emissionImporter.SaveAndReimport();
                
            TextureImporter metallicImporter = AssetImporter.GetAtPath(outPath + "_MASK.png") as TextureImporter;
            if (!metallicImporter)
            {
                Debug.LogError("Failed to import metallic texture. Quitting BadgeMaker");
                return;
            }
            metallicImporter.streamingMipmaps = true;
            metallicImporter.sRGBTexture = false;   // Messes with colors n stuff
            metallicImporter.SaveAndReimport();

            if (_applyToMaterial)
            {
                EditorUtility.DisplayProgressBar("Creating Badge", "Applying to material...", 0.875f);
                
                ApplyTexturesToMaterial(outPath + ".png", outPath + "_MASK.png", emissionOutPath + ".png");
            }

            EditorUtility.DisplayProgressBar("Creating Badge", "Unloading Font...", 1);

            UnloadFonts();
        }

        void CompositeMetallicSmoothnessMask(string templateMask, string metallicPath, string outPath)
        {
            using MagickImage baseImage = new MagickImage(metallicPath);
            using MagickImage maskImage = new MagickImage(templateMask);
            
            maskImage.Flip();   // God literally why do I need to do this
                    
            var separated = maskImage.Separate().ToList();

            var newMaskImage = new MagickImageCollection()
            {
                baseImage,
                separated[1],
                separated[2],
                separated[3],
            };

            using var output = newMaskImage.Combine();
            output.Write(outPath);
        }

        void ApplyTexturesToMaterial(string baseColorPath, string maskPath, string emissionPath)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(Path.Combine(MakeBadgeFolder(_conventionNames[_badgeConvention], _tierNames[_badgeTier]), "Material", "Badge"+Regex.Replace(_tierNames[_badgeTier], @"\s+", "") + ".mat"));
            if (!material)
            {
                Debug.LogError("Failed to find material. Quitting BadgeMaker");
                return;
            }
            
            // Load the new texture
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(baseColorPath);
            if (!texture)
            {
                Debug.LogError($"Failed to load base color at {baseColorPath}. Stopping...");
                return;
            }
            
            var mask = AssetDatabase.LoadAssetAtPath<Texture2D>(maskPath);
            if (!mask)
            {
                Debug.LogError($"Failed to load mask texture at {maskPath}. Stopping...");
                return;
            }
            
            var emission = AssetDatabase.LoadAssetAtPath<Texture2D>(emissionPath);
            if (!mask)
            {
                Debug.LogError($"Failed to load emission texture at {emissionPath}. Stopping...");
                return;
            }
            
            // Set the texture to the material
            material.SetTexture(MainTex, texture);
            material.SetTexture(MaskMap01, mask);
            material.SetTexture(EmissionMap, emission);
            
            AssetDatabase.SaveAssets();
        }
        
        void OnGUI()
        { 
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Badge Maker", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            // If our selected tier is -1, this is the first time we open the window, so we select the highest tier (this doesn't work too well for people with multiple tiers but works well enough)
            if (_badgeTier == -1)
                _badgeTier = _tierNames.Count - 1;

            if (_badgeConvention == -1)
                _badgeConvention = _conventionNames.Count - 1;
            
            // If there were no folders found, show a warning saying that you need badges imported
            if (_tierNames.Count == 0 || _conventionNames.Count == 0)
            {
                EditorGUILayout.HelpBox("No badges found! Please download badges from the downloads tab.", MessageType.Warning);
                return;
            }
            
            // Text field for the badge name
            _badgeName = EditorGUILayout.TextField("Badge Name", _badgeName);
            _pronouns = EditorGUILayout.TextField("Title", _pronouns);
            
            // Create a dropdown menu for the badge type but only show the folder name
            _badgeTier = EditorGUILayout.Popup("Badge Type", _badgeTier, _tierNames.ToArray());
            _badgeConvention = EditorGUILayout.Popup("Convention", _badgeConvention, _conventionNames.ToArray());

            // Checkbox to apply the new texture to the material  
            _applyToMaterial = EditorGUILayout.Toggle("Auto-Apply to Base Material", _applyToMaterial);

            // Button to create the badge
            if (GUILayout.Button("Create Badge"))
            {
                try
                {
                    ConstructBadge();
                }
                finally
                {
                    // We definitely don't want to leave the user with a lingering progress bar
                    EditorUtility.ClearProgressBar();
                }
            }
        }

        private void CreateBadge(string filePath, MagickImage nameImage, MagickImage pronounsImage, string outPath, bool debug = false)
        {
            using MagickImage image = new MagickImage(filePath);
            if (debug)
            {
                image.Draw(new DrawableStrokeColor(MagickColors.Red));
                image.Draw(new DrawableFillColor(MagickColors.Transparent));
                image.Draw(new DrawableRectangle(NameX - NameWidth / 2, NameY - NameHeight / 2, NameX + NameWidth / 2, NameY + NameHeight / 2));
                image.Draw(new DrawableRectangle(PronounsX - PronounsWidth / 2, PronounsY - PronounsHeight / 2, PronounsX + PronounsWidth / 2, PronounsY + PronounsHeight / 2));
            }
                
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