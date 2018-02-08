using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using OneDriveBot.Services.Response;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OneDriveBot.Dialogs.Tickets
{
    /// <summary>
    /// Dialog to guide user through the process of creating a ticket.
    /// </summary>
    [Serializable]
    public class CreateTicketDialog : IDialog<object>
    {
        /// <summary>
        /// Represents the state of ticket creation. Keeps track of the ticket fields that are filled and unfilled.
        /// </summary>
        public TicketFieldsTracker ticketFieldsTracker;

        /// <summary>
        /// Begins create ticket dialog cycle.
        /// Welcome message.
        /// </summary>
        /// <param name="context">Helps send and receive messages, contains conversation state data.</param>
        public Task StartAsync(IDialogContext context)
        {
            this.ticketFieldsTracker = new TicketFieldsTracker();
            context.Wait(MessageReceivedAsync);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Main method to handle messages in the CreateTicketDialog.
        /// </summary>
        /// <param name="context">Helps send and receive messages, contains conversation state data.</param>
        /// <param name="result">Contains the object that is sent to this method whenever it is called. In this case, contains messages from the user.</param>
        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            // Receive result from dialogs that this class calls.
            string dialogsResponse = await result as string;
            if (dialogsResponse == "cancel")
            {
                await CancelTicket(context);
                return;
            }
            
            // Start or end ticket creation process
            if (!this.ticketFieldsTracker.IsStarted)
            {
                System.Diagnostics.Trace.TraceInformation($"CreateTicketDialog.MessageReceivedAsync ticketFieldsTracker.Start");
                await context.PostAsync("Okay, let's create a support ticket! If you would like to cancel this ticket, please enter \"cancel\" at any time.");
                this.ticketFieldsTracker.Start();
            }
            else if (this.ticketFieldsTracker.IsDone)
            {
                await context.Forward(new ConfirmTicketDialog(), AfterConfirmAsync, ticketFieldsTracker, CancellationToken.None);
                return;
            }

            // Get user input for current ticket field.
            await context.Forward(new TicketQnADialog(), MessageReceivedAsync, ticketFieldsTracker, CancellationToken.None);
        }

        /// <summary>
        /// Method is called after confirm dialog. It reacts to the result of the confirm dialog.
        /// </summary>
        private async Task AfterConfirmAsync(IDialogContext context, IAwaitable<string> result)
        {
            var sendTicket = await result;

            if (sendTicket == "yes")
            {
                System.Diagnostics.Trace.TraceInformation($"CreateTicketDialog.AfterConfirmAsync ticketFieldsTracker.IsReadyToSend");
                await context.PostAsync("Please wait a moment, we are creating a ticket for you ...");
                Tuple<IActivity, TicketFieldsTracker> dialogReturn = new Tuple<IActivity, TicketFieldsTracker>(context.Activity, this.ticketFieldsTracker);
                context.Done(dialogReturn);
            }
            else if (sendTicket == "edit")
            {
                await context.Forward(new EditDialog(), MessageReceivedAsync, ticketFieldsTracker, CancellationToken.None);
            }
            else
            {
                await CancelTicket(context);
            }
        }

        /// <summary>
        /// Cancel this ticket and go back to root dialog.
        /// </summary>
        private async Task CancelTicket(IDialogContext context)
        {
            System.Diagnostics.Trace.TraceInformation($"CreateTicketDialog.CancelTicket cancel");

            await context.PostAsync("Alright, the ticket is canceled.");
            context.Done<object>(null);
        }
    }
}