using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Furality.FuralityUpdater.Editor;
using UnityEditor;
using UnityEngine;

namespace Furality.Editor.Pages
{
    public class MainWindow : EditorWindow
    {
        private Queue<Action> _dispatchQueue = new Queue<Action>();
        private Dictionary<string, IMenuPage> _pages = new Dictionary<string, IMenuPage>();
        
        private string _currentPage;
        private Texture2D _logo;

        [MenuItem("Window/Furality/Show SDK")]
        private static void ShowWindow()
        {
            var window = GetWindow<MainWindow>();
            window.titleContent = new GUIContent("Furality SDK");
            window.minSize = new Vector2(350, 500);
            window.Show();
        }

        private void OnEnable()
        {
            _pages = new Dictionary<string, IMenuPage>();
            foreach (var type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()))
            {
                // Check if it implements the IMenuPage interface
                if (type.GetInterfaces().Contains(typeof(IMenuPage)))
                { 
                    // Create an instance of the page
                    _pages.Add(type.Name, Activator.CreateInstance(type) as IMenuPage);
                }
            }
            
            if (_currentPage == null)
            {
                _currentPage = _pages.Keys.First();
            }
        
            _logo = Resources.Load<Texture2D>("furality-logo");
        }

        private void OnGUI()
        {
            var maxLogoWidth = 200;
            var maxLogoHeight = 200;

            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            GUILayout.FlexibleSpace();
            if (_logo != null)
            {
                var aspect = (float)_logo.width / _logo.height;
                var logoWidth = Mathf.Min(maxLogoWidth, maxLogoHeight * aspect);
                var logoHeight = logoWidth / aspect;
                GUILayout.Label(_logo, GUILayout.Width(logoWidth), GUILayout.Height(logoHeight));
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            foreach (var page in _pages)
            {
                bool isSelected = (page.Key == _currentPage);
                if (isSelected)
                {
                    GUI.color = new Color(1.2f, 1.2f, 1.2f);
                }

                if (GUILayout.Button(page.Key.Replace("Page", ""), GUILayout.ExpandWidth(true)))
                {
                    _currentPage = page.Key;
                }

                if (isSelected)
                {
                    GUI.color = Color.white;
                }
            }
            EditorGUILayout.EndHorizontal();

            // I would use if/else here, but unity runs a quick BOM check before displaying the window, and throws an error if something has changed
            if (_currentPage != null)
            {
                EditorGUILayout.Space(10);
                _pages[_currentPage].Draw();
            }
        }
        
        public void Dispatch(Action action)
        {
            _dispatchQueue.Enqueue(action);
        }
        
        private void Update()
        {
            while (_dispatchQueue.Count > 0)
            {
                _dispatchQueue.Dequeue().Invoke();
            }
        }
    }
}
