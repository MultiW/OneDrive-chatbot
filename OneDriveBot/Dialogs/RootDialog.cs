using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using OneDriveBot.Model;
using OneDriveBot.Services.Knowledge;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using OneDriveBot.Services.Response;

namespace OneDriveBot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        private IDictionary<string, string> intentCache = new Dictionary<string, string>();

        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            // TODO: Assuming that the user sent a message in text form always. This is a wrong assumption. 
            // Extract user intent
            string intent = null;

            if (string.IsNullOrEmpty(activity.Text))
            {
                context.Wait(MessageReceivedAsync);
                return;
            }

            System.Diagnostics.Trace.TraceInformation($"RootDialog.MessageReceivedAsync received message:{activity.Text}");
            if (activity.Text.ToLowerInvariant().StartsWith("ticket #"))
            {
                // Get information about ticket
                // ...
            }
            else
            {
                if (this.intentCache.Count >= 1000)
                {
                    this.intentCache.Clear();
                }

                if (OneDriveKnowledgeStore.Default.ContainsIntent(activity.Text))
                {
                    intent = activity.Text;
                }
                else if (this.intentCache.ContainsKey(activity.Text))
                {
                    intent = this.intentCache[activity.Text];
                }
                else
                {
                    intent = await LuisClient.Default.ParseTopIntentAsync(activity.Text);
                    if (!string.IsNullOrEmpty(intent))
                    {
                        this.intentCache[activity.Text] = intent;
                    }
                }
                
                System.Diagnostics.Trace.TraceInformation($"RootDialog.MessageReceivedAsync intent:{intent}");

                // Create Ticket intent maps here.
                if (intent == "create ticket")
                {
                    await context.Forward(new CreateTicketDialog(), ResumeAfterCreateTicketsDialog, activity, CancellationToken.None);
                    return;
                }
                else
                {
                    if (!string.IsNullOrEmpty(intent) && OneDriveKnowledgeStore.Default.ContainsIntent(intent))
                    {
                        // Generate a response
                        OneDriveKnowledge intentResponse = OneDriveKnowledgeStore.Default[intent];
                        
                        System.Diagnostics.Trace.TraceInformation($"RootDialog.MessageReceivedAsync reply:{intentResponse.Response}");

                        // Respond to user
                        await ResponseGenerator.Default.SendResponse(activity, intentResponse.Response, context);
                    }
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
        /// Resumes after exiting ticket dialog, either because ticket was created or ticket was cancelled.
        /// </summary>
        private async Task ResumeAfterCreateTicketsDialog(IDialogContext context, IAwaitable<object> result)
        {
            var ticketResult = await result as Tuple<Activity, TicketFieldsTracker>;

            if (ticketResult != null)
            {
                System.Diagnostics.Trace.TraceInformation($"RootDialog.ResumeAfterCreateTicketsDialog createTicketAsync.");
                // Create ticket
                //...... 
            }
            context.Wait(MessageReceivedAsync);
        }
    }
}