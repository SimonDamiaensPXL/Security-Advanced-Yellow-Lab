using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Linq;

namespace ConsoleApp
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

            string seatholders = await GetSeatholders(input);

            Console.Write(seatholders);
        }

        public static async Task<string> GetAccessToken()
        {
            //Request aan de identity server voor een access token met client credentials
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

            //Verzenden van de request
            var responseIDS = await _httpClient.SendAsync(requestToken);

            //De response inlezen
            var bearerData = await responseIDS.Content.ReadAsStringAsync();

            //Access token uit response halen en returnen
            return JObject.Parse(bearerData)["access_token"].ToString();            
        }

        public static async Task<string> GetSeatholders(string gemeente)
        {
            //URL van de api waar de data moet gehaald worden
            var baseUri = new Uri("http://localhost:5000/api/seatholders/");

            //Request aan de api voor de data met de ingelezen naam of postcode van de gemeente
            var requestData = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(baseUri, gemeente),
            };

            var bearerToken = await GetAccessToken();

            //Onze access token aan de request Authorization header toevoegen 
            requestData.Headers.TryAddWithoutValidation("Authorization", String.Format("Bearer {0}", bearerToken));

            //Verzenden van de request
            var responseAPI = await _httpClient.SendAsync(requestData);
            
            //Dit is de response met de date van de api
            return await responseAPI.Content.ReadAsStringAsync();
        }
    }
}