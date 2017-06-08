using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;

namespace K_MUG_TechDay_June_2017_Demo.Utilities
{
    [Serializable]
    public class LuisCommunicator
    {
        public async Task<LuisJson> CallEngine(string Query)
        {
            Query = Uri.EscapeDataString(Query);
            LuisJson Data = new LuisJson();
            using (HttpClient client = new HttpClient())
            {
                string RequestURI =
                    "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/d697b4a3-4f76-4578-8d8d-96fefabb7b85?subscription-key=4b0e558d41dc47a4b5f4a568e0611bc8&verbose=true&timezoneOffset=0&q=" +
                    Query;
                HttpResponseMessage msg = await client.GetAsync(RequestURI);
                string JsonDataResponse = string.Empty;
                if (msg.IsSuccessStatusCode)
                {
                    JsonDataResponse = await msg.Content.ReadAsStringAsync();
                    Data = JsonConvert.DeserializeObject<LuisJson>(JsonDataResponse);
                }
                Data.Json = JsonDataResponse;
            }
            return Data;
        }
    }
}