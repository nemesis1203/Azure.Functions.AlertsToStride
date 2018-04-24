using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using StrideWebHooks.Stride;

namespace Azure.Functions.AlertsToStride.Stride
{
    public static class Stride
    {
        private static readonly string Api = Environment.GetEnvironmentVariable("Stride:API");
        private static readonly string CloudId = Environment.GetEnvironmentVariable("Stride:CloudId");
        private static readonly string ClientId = Environment.GetEnvironmentVariable("Stride:ClientId");
        private static readonly string ClientSecret = Environment.GetEnvironmentVariable("Stride:ClientSecret");

        /// <summary>
        /// Test:
        ///    message=Server is down
        ///    conversationId=5d24ec29-228a-49eb-953f-91537b6f730a
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("PostToStride")]
        public static async Task<HttpResponseMessage> Send([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            string conversationId = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => String.Compare(q.Key, "conversationId", StringComparison.OrdinalIgnoreCase) == 0)
                .Value;

            string status = "Activated";
            string name = "Undefined availability alert";
            string description = string.Empty;

            //read body
            string body = await req.Content?.ReadAsStringAsync();
            log.Verbose(body);

            if (!string.IsNullOrEmpty(body))
            {
                dynamic data = JsonConvert.DeserializeObject<dynamic>(body);
                status = data?.status;
                name = data?.context.name;
                description = data?.context.description ?? string.Empty;
            }

            string text = $"Alert '{name}' has been '{status}'";

            string url = $"{Api}{CloudId}/conversation/{conversationId}/message";
            string panel = string.Equals(status, "Activated") ? "warning" : "info";
            string content = GetMessage(text, description, panel);
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("authorization", $"Bearer {await GetAppToken()}");

                var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(content, Encoding.UTF8, "application/json")
                };

                var response = await client.SendAsync(request);
                log.Info($"Post to stride: {response.StatusCode}");
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        private static string GetMessage(string text, string description, string status)
        {
            var stridemessage = new StrideRoot
            {
                Body = new StrideMessageModel
                {
                    Type = "doc",
                    Content = new List<StrideContentModel> {new StrideContentModel
                        {
                            Type = "panel",
                            Attributes = new Dictionary<string, string>
                            {
                                { "panelType", status },
                            },
                            Content = new List<StrideContentModel> { new StrideContentModel
                                {
                                    Type = "paragraph",
                                    Content = new List<StrideContentModel> {
                                        new StrideContentModel
                                        {
                                            Type = "text",
                                            Text = text,
                                            Marks = new []
                                            {
                                                new { type="strong" },
                                            }
                                        }
                                    }
                                },
                            }
                        },
                    }
                }
            };

            if (!String.IsNullOrEmpty(description))
                stridemessage.Body.Content[0].Content.Add(new StrideContentModel
                {
                    Type = "paragraph",
                    Content = new List<StrideContentModel> {
                        new StrideContentModel
                        {
                            Type = "text",
                            Text = description,
                        }
                    }
                });

            var settings = new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            return JsonConvert.SerializeObject(stridemessage, settings);
        }

        private static async Task<string> GetAppToken()
        {
            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.atlassian.com/oauth/token")
                {
                    Content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("grant_type", "client_credentials"),
                        new KeyValuePair<string, string>("client_id", ClientId),
                        new KeyValuePair<string, string>("client_secret", ClientSecret),
                    })
                };

                var response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());
                    return result.access_token;
                }

                return string.Empty;
            }
        }
    }
}
