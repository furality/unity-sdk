using System;
using Furality.Editor.Pages;
using Furality.FuralityUpdater.Editor;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Networking;

namespace Furality.Editor.Auth
{
    [Serializable]
    public class UserData
    {
        public string name;
        public string preferred_username;
        public string picture;
        public string sub;
    }

    [Serializable]
    public class TokenResponse
    {
        public string access_token;
        public int expires_in;
        public string id_token;
        public string scope;
        public string token_type;
    }

    // AuthManager is our one port of call for authentication, be it initiating a login or logout, or monitoring our login state
    public static class AuthManager
    {
        private const string ClientID = "f7203a2d-f938-4be2-be47-d4305d1542a8";
        private const string TokenEndpoint = "https://boop.fynn.ai/oidc/token";
        private static readonly string AuthURL = $"https://boop.fynn.ai/oidc/auth?response_type=code&client_id={ClientID}&scope=openid+profile&redirect_uri=http://localhost:8080/callback";

        [CanBeNull] public static UserData CurrentUser { get; private set; }
        [CanBeNull] public static FoxApi Api { get; private set; }
        
        private static HttpCallbackManager CallbackManager { get; set; }
        
        public static bool IsLoggingIn => CallbackManager != null;

        public static void Login(MainWindow callerWindow)
        {
            Application.OpenURL(AuthURL);
            CallbackManager = new HttpCallbackManager(s => callerWindow.Dispatch(() => HandleCodeCallback(s)));
        }

        private static void HandleCodeCallback(string code)
        {
            // Destroy the callback manager to kill the server
            CallbackManager = null;
            
            // POST a web request to the auth server with the code in x-www-form-urlencoded format using unitywebrequest
            TokenResponse response;
            using (var request = new UnityWebRequest(TokenEndpoint, "POST"))
            {
                var body = $"grant_type=authorization_code&code={code}&client_id={ClientID}&redirect_uri=http://localhost:8080/callback";
                request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(body));
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
                var sent = request.SendWebRequest();
                while (!sent.isDone)
                {
                    // Wait for the request to finish
                }
                if (request.isNetworkError || request.isHttpError)
                {
                    Debug.LogError(request.error);
                    return;
                }
                
                response = JsonUtility.FromJson<TokenResponse>(request.downloadHandler.text);
            }
            
            // Parse the user data from the jwt
            CurrentUser = ParseUserDataFromJwt(response.id_token);
            Api = new FoxApi(response.access_token);
        }

        private static UserData ParseUserDataFromJwt(string jwt)
        {
            // First, we split the jwt into three parts, the header, the payload, and the signature
            var parts = jwt.Split('.');
            
            // We'll need to append = to the end of the payload to make it a multiple of 4, as base64 requires that
            while (parts[1].Length % 4 != 0)
                parts[1] += "=";

            // We only care about the payload, so we decode the second part of the jwt from base64
            var b64 = Convert.FromBase64String(parts[1]);
            var payload = System.Text.Encoding.UTF8.GetString(b64);
            
            // Then we parse the json payload into a UserData object
            return JsonUtility.FromJson<UserData>(payload);
        }
        
        public static void Logout()
        {
            throw new NotImplementedException();
        }
    }
}