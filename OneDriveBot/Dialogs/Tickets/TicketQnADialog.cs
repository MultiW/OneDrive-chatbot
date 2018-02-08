using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Bot.Connector;
using OneDriveBot.Services.Response;

namespace OneDriveBot.Dialogs.Tickets
{
    [Serializable]
    public class TicketQnADialog : IDialog<object>
    {
        TicketFieldsTracker ticketFieldsTracker;

        public Task StartAsync(IDialogContext context)
        {
            context.Wait<TicketFieldsTracker>(SendTicketQnaMessageAsync);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Sends message to user asking them to fill in the next field.
        /// </summary>
        private async Task SendTicketQnaMessageAsync(IDialogContext context, IAwaitable<TicketFieldsTracker> result)
        {
            // Receive the object that tracks the progress of the ticket creation
            ticketFieldsTracker = await result as TicketFieldsTracker;
            var activity = context.Activity as Activity;

            // Send message asking user to fill in the next field.
            System.Diagnostics.Trace.TraceInformation($"CreateTicketDialog.PostNextQuestionAsync: {ticketFieldsTracker.CurrentField.Question}");
            await ResponseGenerator.Default.SendResponseAsync(activity, this.ticketFieldsTracker.CurrentField.Question, context);

            context.Wait(ReceiveResponseAsync);
        }

        /// <summary>
        /// Receives user's input to a field.
        /// </summary>
        private async Task ReceiveResponseAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            // Cancel this ticket.
            if (activity.Text.ToLowerInvariant() == "cancel" || activity.Text.ToLowerInvariant() == "abort")
            {
                context.Done("cancel");
                return;
            }
            // Receive user input to a field.
            else if (!ticketFieldsTracker.IsDone)
            {
                System.Diagnostics.Trace.TraceInformation($"CreateTicketDialog.MessageReceivedAsync receive answer: {activity.Text}");
                
                // Recieve user's answer.
                // Checks if user input fails ticket field's validation.
                if (!ticketFieldsTracker.ReceiveAnswer(activity.Text))
                {
                    // Ask until user gives correct answer format.
                    await context.PostAsync("Sorry we cannot process your answer. Please try again.");
                    context.Wait(ReceiveResponseAsync);
                    return;
                }

                context.Done<object>(null);
            }
        }
    }
}