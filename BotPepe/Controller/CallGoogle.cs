using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Google.Apis.Discovery.v1;
using Google.Apis.Discovery.v1.Data;
using Google.Apis.Services;
using Google.Apis.Customsearch.v1.Data;


using Microsoft.Graph;
using Newtonsoft.Json;
using System.Net;
using BotPepe.Models;
using Newtonsoft.Json.Linq;

namespace BotPepe.Controller
{
    public class CallGoogle
    {
        public async void GetGoogle(string consulta)
        {
            //https://cse.google.com/cse/all
            HttpClient client = new HttpClient();
            //client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _token);
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            var apiGoogleKey = "AIzaSyAnIxLWO9OgoZaZZb9RnHMsJaqMv9LtWn4";

            using (var response = await client.GetAsync("https://www.googleapis.com/customsearch/v1?key=" + apiGoogleKey + "&cx=011102882269195785315:fqiiyc32lh0&q=" + consulta + "&num=1"))
            //using (var response = await client.GetAsync("https://graph.microsoft.com/v1.0/me"))
            {
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Graph returned an invalid success code: {response.StatusCode}");
                }

                var stream = await response.Content.ReadAsStringAsync();
                //var groupGraphResponse = JsonConvert.DeserializeObject<GroupResponse>(stream).value;
                //stream.Read(bytes, 0, (int)stream.Length); 
            }
        }

        public async Task<IList<Result>> GoogleIt(string consulta)
        {
            string apiKey = "AIzaSyAnIxLWO9OgoZaZZb9RnHMsJaqMv9LtWn4";
            string cx = "011102882269195785315:fqiiyc32lh0";
            string query = consulta;

            var svc = new Google.Apis.Customsearch.v1.CustomsearchService(new BaseClientService.Initializer { ApiKey = apiKey });
            var listRequest = svc.Cse.List(query);
            listRequest.Num = 1;
            listRequest.PrettyPrint = true;

            listRequest.Cx = cx;
            var search = await listRequest.ExecuteAsync();

            foreach (var result in search.Items)
            {
                var asd = result.Title;
            }

            return search.Items;

        }

        public async Task<Answer[]> SerpWow(string query)
        {
            //
            try
            {

                //Related_Questions xdd = GetJsonData(query)["related_questions"].ToObject<Related_Questions>();
                //Rootobject xd = GetJsonData(query).ToObject<Rootobject>();

                Answer_Box asd = GetJsonData(query)["answer_box"].ToObject<Answer_Box>();


                return asd.answers;
            }
            catch (Exception ex)
            {
                return null;
            }
            //Answer_Box asdx = DownloadJson<Answer_Box>();

            //httpClient.BaseAddress = ;

        }
        public static JObject GetJsonData(string query)
        {

            using (var w = new WebClient())
            {
                var json_data = string.Empty;
                // attempt to download JSON data as a string
                try
                {
                    //json_data = w.DownloadString("https://api.serpwow.com/live/search?api_key=95594BE2&q=agregar+usuario+a+grupo+sharepoint&google_domain=google.cl&location=Chile&gl=cl&hl=es&output=json");
                    json_data = w.DownloadString("https://api.serpwow.com/live/search?api_key=95594BE2&q=" + query + "&google_domain=google.cl&location=Chile&gl=cl&hl=es&output=json");
                }
                catch (Exception) { }
                return JObject.Parse(json_data);

                // if string with JSON data is not empty, deserialize it to class and return its instance 

            }
        }
        public static T DownloadJson<T>() where T : new()
        {

            using (var w = new WebClient())
            {
                var json_data = string.Empty;
                // attempt to download JSON data as a string
                try
                {
                    json_data = w.DownloadString("https://api.serpwow.com/live/search?api_key=95594BE2&q=agregar+usuario+a+grupo+sharepoint&google_domain=google.cl&location=Chile&gl=cl&hl=es&output=json");
                }
                catch (Exception) { }
                //JObject googleSearch = JObject.Parse(json_data);

                // if string with JSON data is not empty, deserialize it to class and return its instance 
                return !string.IsNullOrEmpty(json_data) ? JsonConvert.DeserializeObject<T>(json_data) : new T();
            }
        }
    }
}
