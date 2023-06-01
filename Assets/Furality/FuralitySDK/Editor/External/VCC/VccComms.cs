﻿using System;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

namespace Furality.SDK.External.VCC
{
    public static class VccComms
    {
        public class VccResponse<T>
        {
            public bool success;
            public T data;
        }

        private const string VccUrl = "http://localhost:5477/api/";

        public static async Task<VccResponse<T>> Request<T>(string endpoint, string method, dynamic body = null)
        {
            // Send an HTTP request to the VCC
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Origin", "http://localhost:5477/");
                client.DefaultRequestHeaders.Host = "localhost";
                
                client.Timeout = TimeSpan.FromSeconds(3);

                var request = new HttpRequestMessage(new HttpMethod(method), VccUrl+endpoint);
                if (body != null)
                    request.Content = new StringContent(JsonUtility.ToJson(body), System.Text.Encoding.UTF8, "application/json");

                HttpResponseMessage response;
                try
                {
                    response = await client.SendAsync(request);
                }
                catch (Exception)
                {
                    return null;
                }

                string responseBody = await response.Content.ReadAsStringAsync();

                // Deserialize the response content into VccResponse<T> and return
                return JsonUtility.FromJson<VccResponse<T>>(responseBody);
            }
        }
    }
}