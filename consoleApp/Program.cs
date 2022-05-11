using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Linq;

namespace HelloWorld
{
    class Program
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private static readonly string _clientId = "gert";
        private static readonly string _clientSecret = "finland";


        static async Task Main(string[] args)
        {
            Console.WriteLine("Type een gemeente of postcode:");

            var input = Console.ReadLine();

            await GetAccessToken(input);
        }

        public static async Task GetAccessToken(string apiPath)
        {
            var baseUri = new Uri("http://localhost:5000/api/seatholders/");

            var requestToken = new HttpRequestMessage(HttpMethod.Post, "http://localhost:5002/connect/token")
            {
                Content = new FormUrlEncodedContent(new KeyValuePair<string?, string?>[]
                {
                    new("client_id", _clientId),
                    new("client_secret", _clientSecret),
                    new("scope", "krc-genk"),
                    new("grant_type", "client_credentials")
                })
            };

            var responseIDS = await _httpClient.SendAsync(requestToken);

            var bearerData = await responseIDS.Content.ReadAsStringAsync();

            var bearerToken = JObject.Parse(bearerData)["access_token"].ToString();            

            var requestData = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(baseUri, apiPath),
            };

            requestData.Headers.TryAddWithoutValidation("Authorization", String.Format("Bearer {0}", bearerToken));

            var responseAPI = await _httpClient.SendAsync(requestData);
            
            Console.WriteLine(await responseAPI.Content.ReadAsStringAsync());
        }
    }
}