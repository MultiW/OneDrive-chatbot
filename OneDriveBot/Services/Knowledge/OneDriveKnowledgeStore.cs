namespace OneDriveBot.Services.Knowledge
{
    using OneDriveBot.Model;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    public class OneDriveKnowledgeStore
    {
        public static readonly OneDriveKnowledgeStore Default = new OneDriveKnowledgeStore();
        private readonly OneDriveBotDbContext dbContext = new OneDriveBotDbContext();
        private readonly IDictionary<String, OneDriveKnowledge> knowledgeCache = new Dictionary<String, OneDriveKnowledge>();

        public OneDriveKnowledgeStore()
        {
            this.Load();
        }

        private void Load()
        {
            try
            {
                foreach(OneDriveKnowledge knowledge in dbContext.OneDriveKnowledgeSet)
                {
                    this.knowledgeCache[knowledge.Intent] = knowledge;
                }
            }
            catch(Exception ex)
            {
                // Log error.
            }
        }

        public bool ContainsIntent(String intent)
        {
            return this.knowledgeCache.ContainsKey(intent);
        }

        public OneDriveKnowledge this[String intent] => this.knowledgeCache[intent];
    }
}