using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using UnityEngine;

namespace Furality.SDK.External.Api
{
    public static class FoxApi
    {
        private const string BaseUrl = "https://api.fynn.ai/v1/";
        
        private static string _authToken;

        public static void Instantiate(string token)
        {
            _authToken = token;
#pragma warning disable CS4014  //shut up compiler
            FoxUser.GetProfile();
#pragma warning restore CS4014
        }

        public static void Destroy()
        {
            _authToken = null;
            FoxUser.Destroy();
        }
        
        public static bool IsLoggedIn() => !string.IsNullOrEmpty(_authToken);

        public static async Task<T> Get<T>(string endpoint)
        {
            if (!IsLoggedIn())
            {
                return default;
            }
            
            // Send a GET request to the endpoint
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
                var response = client.GetAsync(BaseUrl + endpoint);
                while (!response.IsCompleted)
                {
                    await Task.Delay(100);
                }
                if (!response.Result.IsSuccessStatusCode)
                {
                    Debug.LogError($"Failed to GET {endpoint}: {response.Result.StatusCode}");
                    return default;
                }

                var json = await response.Result.Content.ReadAsStringAsync();
                return JsonUtility.FromJson<T>(json);
            }
        }
    }
}