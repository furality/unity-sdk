using Furality.SDK.External.Api;
using Furality.SDK.External.Auth;
using UnityEditor;
using UnityEngine;

namespace Furality.SDK.Pages
{
    public class SettingsPage : IMenuPage
    {
        private bool _authExpanded = true;
        
        private GUIStyle _boxStyle;

        public void Draw()
        {
            if (_boxStyle == null)
            {
                _boxStyle = new GUIStyle(GUI.skin.box);
                // Create grey box tex2d
                _boxStyle.normal.background = Texture2D.grayTexture;
                _boxStyle.normal.textColor = Color.white;  
                _boxStyle.alignment = TextAnchor.MiddleCenter;
                _boxStyle.padding = new RectOffset(10, 10, 10, 10);
            }
            
            _authExpanded = EditorGUILayout.Foldout(_authExpanded, "Authentication");

            if (_authExpanded)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginVertical(_boxStyle);

                if (FoxApi.IsLoggedIn()) // If we're logged in and have a valid user
                {
                    // Slightly darker grey box
                    _boxStyle.normal.background = Texture2D.grayTexture;
                    _boxStyle.normal.textColor = Color.white;
                    _boxStyle.alignment = TextAnchor.MiddleCenter;
                    _boxStyle.padding = new RectOffset(10, 10, 10, 10);

                    EditorGUILayout.BeginHorizontal(_boxStyle);
                    var centerStyle = new GUIStyle(GUI.skin.label)
                    {
                        alignment = TextAnchor.MiddleCenter
                    };

                    EditorGUILayout.LabelField("Logged in as: ", centerStyle, GUILayout.Width(100));
                    EditorGUILayout.LabelField(AuthManager.CurrentUser?.preferred_username, centerStyle);

                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal(_boxStyle);
                    
                    EditorGUILayout.LabelField("Attendance: ", centerStyle, GUILayout.Width(100));
                    EditorGUILayout.LabelField(FoxUser.AttendanceLevel.ToString(), centerStyle);
                    EditorGUILayout.EndHorizontal();
                    
                    EditorGUILayout.BeginHorizontal(_boxStyle);
                    EditorGUILayout.LabelField("Patreon: ", centerStyle, GUILayout.Width(100));
                    EditorGUILayout.LabelField(FoxUser.PatreonLevel.ToString(), centerStyle);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space();

                    if (GUILayout.Button("Logout"))
                    {
                        AuthManager.Logout();
                    }
                }
                else
                {
                    bool isLoggingIn = AuthManager.IsLoggingIn;
                    GUI.enabled = !isLoggingIn;
                    if (GUILayout.Button(!isLoggingIn ? "Login" : "Logging in..."))
                    {
                        AuthManager.BeginLogin();
                    }
                    GUI.enabled = true;
                }
                
                EditorGUILayout.EndVertical();
                
                
                EditorGUI.indentLevel--;
            }
        }
    }
}