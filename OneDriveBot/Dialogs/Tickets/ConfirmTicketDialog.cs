using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using OneDriveBot.Services.Response;
using Microsoft.Bot.Connector;

namespace OneDriveBot.Dialogs.Tickets
{
    /// <summary>
    /// Handles the confirmation part of creating tickets.
    /// Sends confirm message, and receives user reply. Sends user reply down to CreateTicketDialog.
    /// </summary>
    [Serializable]
    public class ConfirmTicketDialog : IDialog<string>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.Wait<TicketFieldsTracker>(ConfirmMessageAsync);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Send confirmation message.
        /// </summary>
        private async Task ConfirmMessageAsync(IDialogContext context, IAwaitable<TicketFieldsTracker> result)
        {
            var ticketFieldsTracker = await result as TicketFieldsTracker;
            var activity = context.Activity as Activity;

            // Create ticket summary
            var summaryMessage = ActivityGenerator.Default.GenerateTextualResponse(activity,
                "Here's a summary of your ticket:\n\n"
                + ticketFieldsTracker.Summary);
            summaryMessage.AddHeroCard("", new string[] { "Edit" });

            // Send confirmation summary and message
            await context.PostAsync(summaryMessage);
            await context.PostAsync("Would you like to send the ticket?");

            context.Wait(AfterConfirmMessageAsync);
        }

        /// <summary>
        /// Receive confirmation message.
        /// </summary>
        private async Task AfterConfirmMessageAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            switch (activity.Text.ToLowerInvariant())
            {
                case "yes":
                case "sure":
                    context.Done("yes");
                    break;
                case "no":
                case "cancel":
                case "abort":
                    context.Done("no");
                    break;
                case "edit":
                    context.Done("edit");
                    break;
                default:
                    await context.PostAsync("Sorry, I didn't get that. Please answer yes or no.");
                    context.Wait(AfterConfirmMessageAsync);
                    break;
            }
        }
    }
}