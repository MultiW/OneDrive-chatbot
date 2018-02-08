using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace OneDriveBot
{
    public class LuisClient
    {
        // Botstrap key
        // private string baseUri = "";
        // OneDrive Bot key
        private string baseUri = "";
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