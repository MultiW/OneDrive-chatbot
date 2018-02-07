using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace OneDriveBot
{
    public class LuisClient
    {
        // Botstrap key
        // private string baseUri = "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/b7f6cb7c-170a-43f6-b297-5208bfe2264d?subscription-key=0a1d71c4b1204f8aa2635884335c17de&verbose=true&timezoneOffset=-480&q=";
        // OneDrive Bot key
        private string baseUri = "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/b7f6cb7c-170a-43f6-b297-5208bfe2264d?subscription-key=49caed0be2924ca498f1f55476096636&timezoneOffset=-480&verbose=true&q=";
        public static LuisClient Default = new LuisClient(); 

        /// <summary>
        /// Extracts the top intent from a given message. Returns null if no intents were found.
        /// </summary>
        /// <param name="input">User message to be parsed.</param>
        /// <returns>Top intent extracted from message.</returns>
        public async Task<string> ParseTopIntentAsync(string input)
        {
            LuisResult lresult = await ParseUserInputAsync(input);

            if (lresult.intents.Length != 0)
            {
                return lresult.intents[0].intent;
            }

            return null;
        }

        /// <summary>
        /// Extracts the intents and entities from a user's message.
        /// </summary>
        /// <param name="input">User message to be parsed.</param>
        /// <returns>Top intent extracted from message.</returns>
        public async Task<LuisResult> ParseUserInputAsync(string input)
        {
            string strEscaped = Uri.EscapeDataString(input);

            using (var client = new HttpClient())
            {
                string requestUri = baseUri + strEscaped;
                HttpResponseMessage msg = await client.GetAsync(requestUri);

                if (msg.IsSuccessStatusCode)
                {
                    string jsonResponse = await msg.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<LuisResult>(jsonResponse);
                }
            }

            return null;
        }
    }

    /// <summary>
    /// The Luis interpretations of a message come back in this format.
    /// </summary>
    public class LuisResult
    {
        public string query { get; set; }
        public Intent topScoringIntent { get; set; }
        public Intent[] intents { get; set; }
        public Entity[] entities { get; set; }
    }

    public class Intent
    {
        public string intent { get; set; }
        public float score { get; set; }
    }

    public class Entity
    {
        public string entity { get; set; }
        public string type { get; set; }
        public int startIndex { get; set; }
        public int endIndex { get; set; }
        public float score { get; set; }
    }
}