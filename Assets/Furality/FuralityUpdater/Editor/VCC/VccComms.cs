using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Furality.FuralityUpdater.Editor
{
    public static class VccComms
    {
        public class VccResponse<T>
        {
            public bool success { get; set; }
            public T data { get; set; }
        }

        private const string VccUrl = "http://localhost:5477/api/";

        public static async Task<VccResponse<T>> Request<T>(string endpoint, string method, dynamic body = null)
        {
            // Send an HTTP request to the VCC
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(VccUrl);
                client.DefaultRequestHeaders.Add("Origin", "http://localhost:5477/");
                client.DefaultRequestHeaders.Host = "localhost";

                var request = new HttpRequestMessage(new HttpMethod(method), endpoint);
                if (body != null)
                    request.Content = new StringContent(JsonConvert.SerializeObject(body), System.Text.Encoding.UTF8, "application/json");
                
                var response = await client.SendAsync(request);
                
                string responseBody = await response.Content.ReadAsStringAsync();

                // Deserialize the response content into VccResponse<T> and return
                return JsonConvert.DeserializeObject<VccResponse<T>>(responseBody);
            }
        }
    }
}