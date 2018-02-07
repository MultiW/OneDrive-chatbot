namespace OneDriveBot.Dialogs
{
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using OneDriveBot.Model;
    using OneDriveBot.Services.Response;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Web;

    [Serializable]
    public class CreateTicketDialog : IDialog<object>
    {
        public TicketFieldsTracker ticketFieldsTracker;

        public async Task StartAsync(IDialogContext context)
        {
            this.ticketFieldsTracker = new TicketFieldsTracker();
            await context.PostAsync("Okay, let's create a support ticket! If you would like to cancel this ticket, please enter \"cancel\" at any time.");

            context.Wait(MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
            
            // Cancel this ticket.
            if (activity.Text.ToLowerInvariant() == "cancel" || activity.Text.ToLowerInvariant() == "abort")
            {
                System.Diagnostics.Trace.TraceInformation($"CreateTicketDialog.MessageReceivedAsync cancel");
                await context.PostAsync("Alright, the ticket is canceled.");
                context.Done<object>(null);
                return;
            }

            // Receive user's answer and post next question
            if (!this.ticketFieldsTracker.IsStarted)
            {
                System.Diagnostics.Trace.TraceInformation($"CreateTicketDialog.MessageReceivedAsync ticketFieldsTracker.Start");
                this.ticketFieldsTracker.Start();
            }
            else
            {
                System.Diagnostics.Trace.TraceInformation($"CreateTicketDialog.MessageReceivedAsync receive answer: {activity.Text}");
                this.ticketFieldsTracker.ReceiveAnswer(activity.Text);
            }

            if (this.ticketFieldsTracker.IsDone)
            {
                System.Diagnostics.Trace.TraceInformation($"CreateTicketDialog.MessageReceivedAsync ticketFieldsTracker.IsDone");
                await context.PostAsync("Please wait a moment, we are creating a ticket for you ...");
                Tuple<Activity, TicketFieldsTracker> dialogReturn = new Tuple<Activity, TicketFieldsTracker>(activity, this.ticketFieldsTracker);
                context.Done(dialogReturn);
            }
            else
            {
                await PostNextQuestion(activity, context);
                context.Wait(MessageReceivedAsync);
            }
        }

        private async Task PostNextQuestion(Activity activity, IDialogContext context)
        {
            if (!this.ticketFieldsTracker.IsDone)
            {
                System.Diagnostics.Trace.TraceInformation($"CreateTicketDialog.PostNextQuestion: {ticketFieldsTracker.CurrentField.Question}");
                await ResponseGenerator.Default.SendResponse(activity, this.ticketFieldsTracker.CurrentField.Question, context);
            }
        }
    }
}