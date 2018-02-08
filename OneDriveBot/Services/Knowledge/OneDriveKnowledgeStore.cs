namespace OneDriveBot.Services.Knowledge
{
    using OneDriveBot.Model;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class OneDriveKnowledgeStore
    {
        // TODO: Cache database
        public static readonly OneDriveKnowledgeStore Default = new OneDriveKnowledgeStore();
        private readonly OneDriveBotDbContext dbContext = new OneDriveBotDbContext();
        private readonly IDictionary<String, OneDriveKnowledge> knowledgeCache = new Dictionary<String, OneDriveKnowledge>();

        public OneDriveKnowledgeStore()
        {
            // TODO: check if database changed, if so, clear cache.
        }

        public bool ContainsIntent(String intent)
        {
            // Finds whether the given intent is in the database
            var a = bool.Parse(dbContext.Database.SqlQuery<string>(
                "SELECT CASE WHEN " +
                "(SELECT COUNT (*) FROM OneDriveKnowledges WHERE Intent=@p0) = 1 " +
                "Then 'true' " +
                "Else 'false' END",
                intent).ToList()[0]);
            return a;
        }

        // Query's the database for the given intent's reponse
        public OneDriveKnowledge this[String intent] => dbContext.Database.SqlQuery<OneDriveKnowledge>("SELECT * FROM dbo.OneDriveKnowledges WHERE Intent=@p0", intent).ToList()[0];
    }
}