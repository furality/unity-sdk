// Copyright Furality, Inc. 2026

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
        private const string CurrentConvention = "Furality Ultra";
        
        private static readonly Dictionary<string, ConventionConfig> Conventions = new()
        {
            ["Furality Sylva"] = new ConventionConfig(
                nameX: 2048, nameY: 1304, nameW: 3208, nameH: 855,
                pronX: 2048, pronY: 1717, pronW: 1554, pronH: 257,
                titleBean: "f6-name.bean", titleFont: "Rowdies-Light.ttf",
                pronounsBean: "f6-pronouns.bean", pronounsFont: "Rowdies-Regular.ttf",
                pipeline: ConventionConfig.PipelineType.Sylva
            ),

            ["Furality Umbra"] = new ConventionConfig(
                nameX: 2048, nameY: 1504, nameW: 3208, nameH: 855,
                pronX: 2048, pronY: 1917, pronW: 1554, pronH: 257,
                titleBean: "f7-font.bean", titleFont: "Roboto-BoldItalic.ttf",
                pronounsBean: "f7-font.bean", pronounsFont: "Roboto-BoldItalic.ttf",
                pipeline: ConventionConfig.PipelineType.Umbra,
                tierColors: new()
                {
                    ["Attendee"] = new Tuple<MagickColor, MagickColor>(new MagickColor("#37ff79"), new MagickColor("#37ff79")),
                    ["First Class"] = new Tuple<MagickColor, MagickColor>(new MagickColor("#fe3fff"), new MagickColor("#fe3fff")),
                    ["Sponsor"] = new Tuple<MagickColor, MagickColor>(new MagickColor("#ffce49"), new MagickColor("#ffce49")),
                }
            ),

            ["Furality Somna"] = new ConventionConfig(
                nameX: 375, nameY: 700, nameW: 610, nameH: 150,
                pronX: 450, pronY: 810, pronW: 449, pronH: 75,
                titleBean: "f8-font.bean", titleFont: "Fraunces_72pt-SemiBold.ttf",
                pronounsBean: "f8-font.bean", pronounsFont: "Fraunces_72pt-SemiBold.ttf",
                pipeline: ConventionConfig.PipelineType.Somna,
                tierColors: new()
                {
                    ["Attendee"] = new Tuple<MagickColor, MagickColor>(new MagickColor("#ffeead"), new MagickColor("#ffffff")),
                    ["First Class"] = new Tuple<MagickColor, MagickColor>(new MagickColor("#ffeead"), new MagickColor("#ffffff")),
                    ["Sponsor"] = new Tuple<MagickColor, MagickColor>(new MagickColor("#ffeead"), new MagickColor("#ffffff")),
                    ["Dream Maker"] = new Tuple<MagickColor, MagickColor>(new MagickColor("#ffeead"), new MagickColor("#ffffff")),
                    ["Team"] = new Tuple<MagickColor, MagickColor>(new MagickColor("#ffeead"), new MagickColor("#ffffff")),
                }
            ),

            ["Furality Ultra"] = new ConventionConfig(
                nameX: 490, nameY: 308, nameW: 830, nameH: 200,
                pronX: 560, pronY: 590, pronW: 700, pronH: 150,
                titleBean: "f9-font.bean", titleFont: "BRLNSR.TTF",
                pronounsBean: "f9-font.bean", pronounsFont: "BRLNSR.TTF",
                pipeline: ConventionConfig.PipelineType.Ultra,
                tierColors: new()
                {
                    ["Attendee"] = new Tuple<MagickColor, MagickColor>(new MagickColor("#ffffff"), new MagickColor("#dfff99")),
                    ["First Class"] = new Tuple<MagickColor, MagickColor>(new MagickColor("#ffffff"), new MagickColor("#ffa8ff")),
                    ["Sponsor"] = new Tuple<MagickColor, MagickColor>(new MagickColor("#ffffff"), new MagickColor("#ffff96")),
                    ["Game Changer"] = new Tuple<MagickColor, MagickColor>(new MagickColor("#ffffff"), new MagickColor("#cef48b")),
                    ["Team"] = new Tuple<MagickColor, MagickColor>(new MagickColor("#ffffff"), new MagickColor("#8bf4f4")),
                }
            ),
        };

        private static readonly int MainTex     = Shader.PropertyToID("_MainTex");
        private static readonly int EffectMask  = Shader.PropertyToID("_EffectMask");
        private static readonly int MaskMap01   = Shader.PropertyToID("_MaskMap01");
        private static readonly int EmissionMap = Shader.PropertyToID("_EmissionMap");

        // GDI font loading helps some ppl's installs find the font somehow
        [DllImport("Gdi32.dll")]
        private static extern int AddFontResourceEx(string lpFileName, uint fl, IntPtr pdv);

        [DllImport("Gdi32.dll")]
        private static extern bool RemoveFontResourceEx(string lpFileName, uint fl, IntPtr pdv);

        // gui state
        private string _badgeName = "Your Name";
        private string _pronouns  = "Title/Pronouns";
        private int _badgeTier       = -1;
        private int _badgeConvention = -1;
        private int _lastSelectedConvention = -1;
        private bool _applyToMaterial = true;
        private Dictionary<string, List<string>> _tierNames       = new();
        private List<string> _conventionNames = new();
        private ConventionConfig _activeConfig;

        private void OnEnable()
        {
            foreach (var conventionFolder in AssetDatabase.GetSubFolders("Assets/Furality"))
            {
                var tiers = AssetDatabase.GetSubFolders(Path.Combine(conventionFolder, "Avatar Assets/Badges"));
                if (tiers.Length == 0) continue;
                var conventionName = conventionFolder.Split('/')[^1];
                _tierNames.Add(conventionName, tiers.Select(t => t.Split('/')[^1]).ToList());
            }
            
            _conventionNames = _tierNames.Keys.ToList();
        }

        [MenuItem("Furality/Show Badge Maker")]
        private static void Init()
        {
            var window = (BadgeMaker)GetWindow(typeof(BadgeMaker));
            window.titleContent = new GUIContent("Furality Badge Maker");
            window.minSize = new Vector2(350, 400);
            window.Show();
        }

        private void OnDestroy()
        {
            if (_activeConfig != null) UnloadFonts(_activeConfig);
        }

        private void UnloadAndDeleteFontIfExists(string path)
        {
            if (!File.Exists(path)) return;
            try { RemoveFontResourceEx(path, 0, IntPtr.Zero); File.Delete(path); } catch { }
        }

        private void UnloadFonts(ConventionConfig config)
        {
            UnloadAndDeleteFontIfExists(Path.Combine(Utils.FontPath, config.TitleFont));
            if (config.PronounsFont != config.TitleFont)
                UnloadAndDeleteFontIfExists(Path.Combine(Utils.FontPath, config.PronounsFont));
        }

        private void CopyAndLoadFont(string srcPath, string fontName)
        {
            var destPath = Path.Combine(Utils.FontPath, fontName);
            try
            {
                File.Copy(srcPath, destPath, true);
                if (AddFontResourceEx(destPath, 0, IntPtr.Zero) == 0)
                    Debug.LogError("Failed to add font resource: " + destPath);
            }
            catch { }
        }

        private void LoadFonts(ConventionConfig config)
        {
            if (!Directory.Exists(Utils.FontPath)) Directory.CreateDirectory(Utils.FontPath);
            UnloadFonts(config);
            CopyAndLoadFont(Path.Combine(Utils.BadgeMakerEditorPath, config.TitleBean), config.TitleFont);
            if (config.PronounsFont != config.TitleFont)
                CopyAndLoadFont(Path.Combine(Utils.BadgeMakerEditorPath, config.PronounsBean), config.PronounsFont);
        }


        private void OnGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Badge Maker", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(10);

            if (_tierNames.Count == 0 || _tierNames.Values.All(x => x.Count == 0))
            {
                EditorGUILayout.HelpBox(
                    "No badges found! Please download badges from the downloads tab.",
                    MessageType.Warning);
                return;
            }

            if (_badgeConvention == -1)
            {
                _badgeConvention = _conventionNames.Contains(CurrentConvention)
                    ? _conventionNames.IndexOf(CurrentConvention)
                    : _conventionNames.Count - 1;
            }
            
            // Allows us to order and figure out the most significant badge
            var potentialTiers = Conventions[_conventionNames[_badgeConvention]].TierColors.Keys.ToList();
            var actualTiers = _tierNames[_conventionNames[_badgeConvention]].ToList();

            // If we haven't initialized yet, or our convention has changed, we should recalculate this
            if (_badgeTier == -1 || _lastSelectedConvention != _badgeConvention)
            {
                _badgeTier = actualTiers.IndexOf(potentialTiers.Where(actualTiers.Contains).Last());
                _lastSelectedConvention = _badgeConvention;
            }

            _badgeName       = EditorGUILayout.TextField("Badge Name", _badgeName);
            _pronouns        = EditorGUILayout.TextField("Title",      _pronouns);
            _badgeTier       = EditorGUILayout.Popup("Badge Type", _badgeTier, actualTiers.ToArray());
            _badgeConvention = EditorGUILayout.Popup("Convention",  _badgeConvention, _conventionNames.ToArray());
            _applyToMaterial = EditorGUILayout.Toggle("Auto-Apply to Base Material", _applyToMaterial);

            var selectedConvention = _conventionNames[_badgeConvention];
            if (!Conventions.ContainsKey(selectedConvention))
                EditorGUILayout.HelpBox(
                    $"'{selectedConvention}' is not supported by this version of Badge Maker.",
                    MessageType.Warning);

            if (GUILayout.Button("Create Badge"))
            {
                try   { ConstructBadge(); }
                finally { EditorUtility.ClearProgressBar(); }
            }
        }

        private void ConstructBadge()
        {
            var convention   = _conventionNames[_badgeConvention];
            var tier         = _tierNames[convention][_badgeTier];
            var tierNoSpaces = Regex.Replace(tier, @"\s+", "");
            var safeFileName = Regex.Replace(_badgeName, @"[<>:""/\\|?*]", "_");

            if (!Conventions.TryGetValue(convention, out var config))
            {
                Debug.LogError($"No configuration found for convention '{convention}'. Cannot create badge.");
                return;
            }

            if (config.TierColors != null && !config.TierColors.ContainsKey(tier))
            {
                Debug.LogError(
                    $"Tier '{tier}' is not configured for '{convention}'. " +
                    $"Expected one of: {string.Join(", ", config.TierColors.Keys)}");
                return;
            }

            _activeConfig = config;

            var textColor     = config.GetTextColor(tier);
            var titleColor = textColor.Item1;
            var pronounsColor = textColor.Item2;
            var badgeFolder   = Utils.BadgeFolderRoot(convention, tier);

            EditorUtility.DisplayProgressBar("Creating Badge", "Loading fonts...", 0.1f);
            LoadFonts(config);

            switch (config.Pipeline)
            {
                case ConventionConfig.PipelineType.Sylva:
                    RunSylvaPipeline(config, badgeFolder, tier, safeFileName, titleColor, pronounsColor);
                    break;
                case ConventionConfig.PipelineType.Umbra:
                    RunUmbraPipeline(config, badgeFolder, tierNoSpaces, safeFileName, titleColor, pronounsColor);
                    break;
                case ConventionConfig.PipelineType.Somna:
                    RunSomnaPipeline(config, badgeFolder, tierNoSpaces, safeFileName, titleColor, pronounsColor);
                    break;
                case ConventionConfig.PipelineType.Ultra:
                    RunUltraPipeline(config, badgeFolder, tierNoSpaces, safeFileName, titleColor, pronounsColor);
                    break;
            }

            UnloadFonts(config);
        }

        // Sylva only needs a base diffuse and an emission texture
        private void RunSylvaPipeline(ConventionConfig config, string badgeFolder,
            string tier, string safeFileName,
            MagickColor textColor, MagickColor pronounsColor)
        {
            var texDir       = Path.Combine(badgeFolder, "Texture");   // Sylva uses singular "Texture"
            var inputBase    = Path.Combine(texDir, $"{tier}_Empty.png");
            var inputEmission = Path.Combine(texDir, $"{tier}_Empty_EMI.png");

            var outDir      = Path.Combine(texDir, "Custom");
            if (!Directory.Exists(outDir)) Directory.CreateDirectory(outDir);
            var outBase     = Path.Combine(outDir, $"CUSTOM_{safeFileName}.png");
            var outEmission = Path.Combine(outDir, $"CUSTOM_{safeFileName}_EMI.png");

            EditorUtility.DisplayProgressBar("Creating Badge", "Rendering name text...", 0.25f);
            var nameImg     = MakeTextImage(config.TitleFont,   _badgeName, config.NameW,     config.NameH,     textColor);
            EditorUtility.DisplayProgressBar("Creating Badge", "Rendering title text...", 0.35f);
            var pronounsImg = MakeTextImage(config.PronounsFont, _pronouns, config.PronounsW, config.PronounsH, pronounsColor);

            EditorUtility.DisplayProgressBar("Creating Badge", "Compositing base texture...", 0.5f);
            CreateBadge(config, inputBase, nameImg, pronounsImg, outBase);
            EditorUtility.DisplayProgressBar("Creating Badge", "Compositing emission texture...", 0.65f);
            CreateBadge(config, inputEmission, nameImg, pronounsImg, outEmission);

            AssetDatabase.Refresh();
            EditorUtility.DisplayProgressBar("Creating Badge", "Importing textures...", 0.8f);
            SetStreamingMipmaps(outBase);
            SetStreamingMipmaps(outEmission);

            if (!_applyToMaterial) return;
            EditorUtility.DisplayProgressBar("Creating Badge", "Applying to material...", 0.9f);
            var matPath = Path.Combine(badgeFolder, "Material", $"{tier}.mat");
            if (TryLoadMaterial(matPath, out var mat))
            {
                mat.SetTexture(MainTex,     AssetDatabase.LoadAssetAtPath<Texture2D>(outBase));
                mat.SetTexture(EffectMask,  AssetDatabase.LoadAssetAtPath<Texture2D>(outBase));
                mat.SetTexture(EmissionMap, AssetDatabase.LoadAssetAtPath<Texture2D>(outEmission));
                AssetDatabase.SaveAssets();
            }
        }
        
        // Ultra only needs base texture (thank god)
        private void RunUltraPipeline(ConventionConfig config, string badgeFolder,
            string tier, string safeFileName,
            MagickColor textColor, MagickColor pronounsColor)
        {
            var texDir       = Path.Combine(badgeFolder, "Textures");
            var inputBase    = Path.Combine(texDir, $"Badge{tier}_DIF.png");
            var inputEmission = Path.Combine(texDir, $"Badge{tier}_EMI.png");

            var outDir      = Path.Combine(texDir, "Custom");
            if (!Directory.Exists(outDir)) Directory.CreateDirectory(outDir);
            var outBase     = Path.Combine(outDir, $"CUSTOM_{safeFileName}.png");
            var outEmission = Path.Combine(outDir, $"CUSTOM_{safeFileName}_EMI.png");

            EditorUtility.DisplayProgressBar("Creating Badge", "Rendering name text...", 0.25f);
            var nameImg     = MakeTextImage(config.TitleFont,   _badgeName, config.NameW,     config.NameH,     textColor);
            EditorUtility.DisplayProgressBar("Creating Badge", "Rendering title text...", 0.35f);
            var pronounsImg = MakeTextImage(config.PronounsFont, _pronouns, config.PronounsW, config.PronounsH, pronounsColor);

            EditorUtility.DisplayProgressBar("Creating Badge", "Compositing base texture...", 0.5f);
            CreateBadge(config, inputBase, nameImg, pronounsImg, outBase);
            EditorUtility.DisplayProgressBar("Creating Badge", "Compositing emission texture...", 0.65f);
            CreateBadge(config, inputEmission, nameImg, pronounsImg, outEmission);

            AssetDatabase.Refresh();
            EditorUtility.DisplayProgressBar("Creating Badge", "Importing textures...", 0.8f);
            SetStreamingMipmaps(outBase);
            SetStreamingMipmaps(outEmission);

            if (!_applyToMaterial) return;
            EditorUtility.DisplayProgressBar("Creating Badge", "Applying to material...", 0.9f);
            var matPath = Path.Combine(badgeFolder, "Materials", $"Badge{tier}.mat");
            if (TryLoadMaterial(matPath, out var mat))
            {
                mat.SetTexture(MainTex,     AssetDatabase.LoadAssetAtPath<Texture2D>(outBase));
                mat.SetTexture(EffectMask,  AssetDatabase.LoadAssetAtPath<Texture2D>(outBase));
                mat.SetTexture(EmissionMap, AssetDatabase.LoadAssetAtPath<Texture2D>(outEmission));
                AssetDatabase.SaveAssets();
            }
        }

        // Umbra only needs an emission texture and applies it to the material.
        private void RunUmbraPipeline(ConventionConfig config, string badgeFolder,
            string tierNoSpaces, string safeFileName,
            MagickColor textColor, MagickColor pronounsColor)
        {
            var texDir        = Path.Combine(badgeFolder, "Textures");
            var badgePrefix   = $"Badge {tierNoSpaces}";
            var inputEmission = Path.Combine(texDir, $"{badgePrefix}_EMI_BLANK.png");

            var outDir      = Path.Combine(texDir, "Custom");
            if (!Directory.Exists(outDir)) Directory.CreateDirectory(outDir);
            var outEmission = Path.Combine(outDir, $"CUSTOM_{safeFileName}_EMI_BLANK.png");

            EditorUtility.DisplayProgressBar("Creating Badge", "Rendering name text...", 0.3f);
            var nameImg     = MakeTextImage(config.TitleFont,   _badgeName, config.NameW,     config.NameH,     textColor);
            EditorUtility.DisplayProgressBar("Creating Badge", "Rendering title text...", 0.45f);
            var pronounsImg = MakeTextImage(config.PronounsFont, _pronouns, config.PronounsW, config.PronounsH, pronounsColor);

            EditorUtility.DisplayProgressBar("Creating Badge", "Compositing emission texture...", 0.65f);
            CreateBadge(config, inputEmission, nameImg, pronounsImg, outEmission);

            AssetDatabase.Refresh();
            EditorUtility.DisplayProgressBar("Creating Badge", "Importing textures...", 0.8f);
            SetStreamingMipmaps(outEmission);

            if (!_applyToMaterial) return;
            EditorUtility.DisplayProgressBar("Creating Badge", "Applying to material...", 0.9f);
            var matPath = Path.Combine(badgeFolder, "Materials", $"Badge{tierNoSpaces}.mat");
            if (TryLoadMaterial(matPath, out var mat))
            {
                mat.SetTexture(EmissionMap, AssetDatabase.LoadAssetAtPath<Texture2D>(outEmission));
                AssetDatabase.SaveAssets();
            }
        }

        // Somna is such a quirky lil guy. Needs base diffuse, metallic/smoothness mask composite, emission.
        private void RunSomnaPipeline(ConventionConfig config, string badgeFolder,
            string tierNoSpaces, string safeFileName,
            MagickColor textColor, MagickColor pronounsColor)
        {
            var texDir        = Path.Combine(badgeFolder, "Textures");
            var badgePrefix   = $"Badge{tierNoSpaces}";
            var inputDif      = Path.Combine(texDir, $"{badgePrefix}_DIF.png");
            var inputMetallic = Path.Combine(texDir, "Others", "Material.001_Metallic.png");
            var inputMasks    = Path.Combine(texDir, $"{badgePrefix}_MASKS.tga");
            var inputEmission = Path.Combine(texDir, $"{badgePrefix}_EMI.png");

            var outDir       = Path.Combine(texDir, "Custom");
            if (!Directory.Exists(outDir)) Directory.CreateDirectory(outDir);
            var outBase      = Path.Combine(outDir, $"CUSTOM_{safeFileName}.png");
            var outMetallic  = Path.Combine(outDir, $"CUSTOM_{safeFileName}_Metallic.png");
            var outMask      = Path.Combine(outDir, $"CUSTOM_{safeFileName}_MASK.png");
            var outEmission  = Path.Combine(outDir, $"CUSTOM_{safeFileName}_Emission.png");

            EditorUtility.DisplayProgressBar("Creating Badge", "Rendering name text...", 0.15f);
            var nameImg      = MakeTextImage(config.TitleFont,   _badgeName, config.NameW,     config.NameH,     textColor);
            var nameImgWhite = MakeTextImage(config.TitleFont,   _badgeName, config.NameW,     config.NameH,     MagickColors.White);
            EditorUtility.DisplayProgressBar("Creating Badge", "Rendering title text...", 0.25f);
            var pronounsImg  = MakeTextImage(config.PronounsFont, _pronouns, config.PronounsW, config.PronounsH, pronounsColor);

            EditorUtility.DisplayProgressBar("Creating Badge", "Compositing base texture...", 0.4f);
            CreateBadge(config, inputDif, nameImg, pronounsImg, outBase);
            EditorUtility.DisplayProgressBar("Creating Badge", "Compositing metallic texture...", 0.55f);
            CreateBadge(config, inputMetallic, nameImgWhite, pronounsImg, outMetallic);
            EditorUtility.DisplayProgressBar("Creating Badge", "Building mask texture...", 0.65f);
            CompositeMetallicSmoothnessMask(inputMasks, outMetallic, outMask);
            EditorUtility.DisplayProgressBar("Creating Badge", "Compositing emission texture...", 0.75f);
            CreateBadge(config, inputEmission, nameImg, pronounsImg, outEmission);

            AssetDatabase.Refresh();
            EditorUtility.DisplayProgressBar("Creating Badge", "Importing textures...", 0.85f);
            SetStreamingMipmaps(outBase);
            SetStreamingMipmaps(outEmission);
            SetStreamingMipmaps(outMask, sRGB: false);

            if (!_applyToMaterial) return;
            EditorUtility.DisplayProgressBar("Creating Badge", "Applying to material...", 0.95f);
            var matPath = Path.Combine(badgeFolder, "Material", $"Badge{tierNoSpaces}.mat");
            if (TryLoadMaterial(matPath, out var mat))
            {
                mat.SetTexture(MainTex,     AssetDatabase.LoadAssetAtPath<Texture2D>(outBase));
                mat.SetTexture(MaskMap01,   AssetDatabase.LoadAssetAtPath<Texture2D>(outMask));
                mat.SetTexture(EmissionMap, AssetDatabase.LoadAssetAtPath<Texture2D>(outEmission));
                AssetDatabase.SaveAssets();
            }
        }

        private MagickImage MakeTextImage(string fontName, string text, int w, int h, MagickColor color)
        {
            if (string.IsNullOrEmpty(text)) return null;
            return FindFontSize(Path.Combine(Utils.FontPath, fontName), text, w, h, color);
        }

        private void CreateBadge(ConventionConfig config,
            string templatePath, MagickImage nameImg, MagickImage pronounsImg,
            string outPath, bool debug = false)
        {
            using MagickImage image = new MagickImage(templatePath);

            if (debug)
            {
                image.Draw(new DrawableStrokeColor(MagickColors.Red));
                image.Draw(new DrawableFillColor(MagickColors.Transparent));
                image.Draw(new DrawableRectangle(
                    config.NameX - config.NameW / 2, config.NameY - config.NameH / 2,
                    config.NameX + config.NameW / 2, config.NameY + config.NameH / 2));
                image.Draw(new DrawableRectangle(
                    config.PronounsX - config.PronounsW / 2, config.PronounsY - config.PronounsH / 2,
                    config.PronounsX + config.PronounsW / 2, config.PronounsY + config.PronounsH / 2));
            }

            if (nameImg != null)
                image.Composite(nameImg,
                    config.NameX - (int)(nameImg.Width / 2),
                    config.NameY - (int)(nameImg.Height / 2),
                    CompositeOperator.Atop);

            if (pronounsImg != null)
                image.Composite(pronounsImg,
                    config.PronounsX - (int)(pronounsImg.Width / 2),
                    config.PronounsY - (int)(pronounsImg.Height / 2),
                    CompositeOperator.Atop);

            image.Write(outPath);
        }

        private void CompositeMetallicSmoothnessMask(string templateMask, string metallicPath, string outPath)
        {
            using MagickImage baseImage = new MagickImage(metallicPath);
            using MagickImage maskImage = new MagickImage(templateMask);
            maskImage.Flip();   // God literally why do I need to do this
            var separated = maskImage.Separate().ToList();
            using var output = new MagickImageCollection { baseImage, separated[1], separated[2], separated[3] }.Combine();
            output.Write(outPath);
        }

        private static void SetStreamingMipmaps(string assetPath, bool sRGB = true)
        {
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null) { Debug.LogError($"Failed to import texture at {assetPath}"); return; }
            importer.streamingMipmaps = true;
            if (!sRGB) importer.sRGBTexture = false;
            importer.SaveAndReimport();
        }

        private static bool TryLoadMaterial(string assetPath, out Material mat)
        {
            mat = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            if (mat == null) Debug.LogError($"Failed to find material at {assetPath}");
            return mat != null;
        }

        private static MagickImage FindFontSize(string fontFamily, string text, int w, int h, MagickColor color)
        {
            var image = new MagickImage($"label:{text}", new MagickReadSettings
            {
                BackgroundColor = MagickColors.None,
                FillColor       = color,
                Font            = fontFamily,
                Width           = w,
                Height          = h,
            });
            image.Trim();
            return image;
        }
    }
}
