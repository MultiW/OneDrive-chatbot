using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using OneDriveBot.Model;
using OneDriveBot.Services.Knowledge;
using OneDriveBot.Services.Response;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using OneDriveBot.Dialogs.Tickets;
using System.IO;

namespace OneDriveBot.Dialogs
{
    /// <summary>
    /// The main dialog class that handles all Q&A messages and redirects messages to other dialogs, like for ticket creation.
    /// </summary>
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        private IDictionary<string, string> intentCache = new Dictionary<string, string>();
        private IDictionary<string, OneDriveKnowledge> knowledgeCache = new Dictionary<string, OneDriveKnowledge>();

        /// <summary>
        /// Begins root dialog cycle.
        /// </summary>
        /// <param name="context">Helps send and receive messages, contains conversation state data.</param>
        /// <returns></returns>
        public Task StartAsync(IDialogContext context)
        {
            FillKnowledgeCache();
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Main method to handle messages in the RootDialog.
        /// </summary>
        /// <param name="context">Helps send and receive messages, contains conversation state data.</param>
        /// <param name="result">Contains the object that is sent to this method whenever it is called. In this case, contains messages from the user.</param>
        /// <returns></returns>
        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            // Extract user intent
            string intent = null;

            // Check if message is somehow empty
            if (string.IsNullOrEmpty(activity.Text))
            {
                context.Wait(MessageReceivedAsync);
                return;
            }

            System.Diagnostics.Trace.TraceInformation($"RootDialog.MessageReceivedAsync received message:{activity.Text}");
            // Conditional reacts to user messages using string comparisons.
            if (activity.Text.ToLowerInvariant().StartsWith("ticket #"))
            {
                // Get information on given ticket
                // ...
            }
            else
            {
                // Clears the cache if it is too full.
                if (this.intentCache.Count >= 1000)
                {
                    this.intentCache.Clear();
                }
                if (this.knowledgeCache.Count >= 1000)
                {
                    this.knowledgeCache.Clear();
                }

                // Parse user message to an intent
                // If user message is equal to an intent, then no need to query an intent from LUIS
                if (this.knowledgeCache.ContainsKey(activity.Text))
                {
                    intent = activity.Text;
                }
                //else if (OneDriveKnowledgeStore.Default.ContainsIntent(activity.Text))
                //{
                //    intent = activity.Text;
                //}
                // Get message's intent from cache if it contains it
                else if (this.intentCache.ContainsKey(activity.Text))
                {
                    intent = this.intentCache[activity.Text];
                }
                // Get the message's intent from LUIS
                else
                {
                    intent = await LuisClient.Default.ParseTopIntentAsync(activity.Text);
                    if (!string.IsNullOrEmpty(intent))
                    {
                        this.intentCache[activity.Text] = intent;
                    }
                }
                
                System.Diagnostics.Trace.TraceInformation($"RootDialog.MessageReceivedAsync intent:{intent}");

                // Conditional checks for certain intents where special action is required.
                // Else it finds the intent's mapped response from the database.
                if (intent == "create ticket")
                {
                    // Create ticket. Go to CreateTicketDialog
                    await context.Forward(new CreateTicketDialog(), ResumeAfterCreateTicketsDialog, null, CancellationToken.None);
                    return;
                }
                else
                {
                    // Conditional checks if intent is in database. If intent is None, return error message.
                    if (!string.IsNullOrEmpty(intent) && this.knowledgeCache.ContainsKey(intent))
                    {
                        // Generate response from cache
                        OneDriveKnowledge intentResponse = this.knowledgeCache[intent];
                        // Respond to user
                        await ResponseGenerator.Default.SendResponseAsync(activity, intentResponse.Response, context);
                    }
                    //else if (!string.IsNullOrEmpty(intent) && OneDriveKnowledgeStore.Default.ContainsIntent(intent))
                    //{
                    //    // Generate a response
                    //    OneDriveKnowledge intentResponse = OneDriveKnowledgeStore.Default[intent];

                    //    // store in cache
                    //    this.knowledgeCache[intent] = intentResponse;

                    //    System.Diagnostics.Trace.TraceInformation($"RootDialog.MessageReceivedAsync reply:{intentResponse.Response}");

                    //    // Respond to user
                    //    await ResponseGenerator.Default.SendResponseAsync(activity, intentResponse.Response, context);
                    //}
                    else
                    {
                        System.Diagnostics.Trace.TraceInformation($"RootDialog.MessageReceivedAsync reply: Sorry I don't understand the request....");
                        await context.PostAsync("Sorry I don't understand the request. OneDrive engineers are adding more capabilities to me. Please try again later.");
                    }
                }
            }
        
            context.Wait(MessageReceivedAsync);
        }

        /// <summary>
        /// TEST METHOD: outside of home, cannot access database, so fill in cache so that we don't need database.
        /// </summary>
        private void FillKnowledgeCache()
        {
            string[] lines = File.ReadAllLines(@"C:\Users\multi\Source\Repos\ApiLab\code\OneDriveBot\OneDriveBot\KnowledgeCacheTest.txt");

            int currentLineIndex = 0;
            while(currentLineIndex < lines.Length)
            {
                string intent = lines[currentLineIndex].Substring(3);
                intent = lines[currentLineIndex].Substring(3);
                currentLineIndex++;

                string response = "";
                while (currentLineIndex < lines.Length && (string.IsNullOrEmpty(lines[currentLineIndex]) || lines[currentLineIndex].Substring(0, 3) != "[I]"))
                {
                    response += lines[currentLineIndex] + "\n\n";
                    currentLineIndex++;
                }

                this.knowledgeCache[intent] = new OneDriveKnowledge()
                {
                    Intent = intent,
                    Response = response
                };
            }
        }

        /// <summary>
        /// Resumes after exiting ticket dialog, either because ticket was created or ticket was cancelled.
        /// </summary>
        private async Task ResumeAfterCreateTicketsDialog(IDialogContext context, IAwaitable<object> result)
        {
            var ticketResult = await result as Tuple<IActivity, TicketFieldsTracker>;

            if (ticketResult != null)
            {
                System.Diagnostics.Trace.TraceInformation($"RootDialog.ResumeAfterCreateTicketsDialog createTicketAsync.");
                // Create ticket
                var activity = ticketResult.Item1;
                var ticketFieldsTracker = ticketResult.Item2;
                // ... generate ticket
            }
            context.Wait(MessageReceivedAsync);
        }
    }
}