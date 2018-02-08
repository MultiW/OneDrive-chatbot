using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using OneDriveBot.Services.Response;
using Microsoft.Bot.Connector;
using System.Threading;

namespace OneDriveBot.Dialogs.Tickets
{
    /// <summary>
    /// Handles editing of ticket fields that have previously been filled.
    /// Sends editing message prompts and receives results.
    /// </summary>
    [Serializable]
    public class EditDialog : IDialog<object>
    {
        TicketFieldsTracker ticketFieldsTracker;

        public Task StartAsync(IDialogContext context)
        {
            context.Wait<TicketFieldsTracker>(SendEditMessageAsync);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Send message asking what field user wants to edit.
        /// </summary>
        private async Task SendEditMessageAsync(IDialogContext context, IAwaitable<TicketFieldsTracker> result)
        {
            ticketFieldsTracker = await result as TicketFieldsTracker;

            await context.PostAsync("What would you like to change?");

            // Get list of fields that user can edit.
            List<MenuOption> options = new List<MenuOption>();
            var fields = typeof(TicketFieldType).GetFields();
            for (int i = 2; i < fields.Length; i++)
            {
                var field = fields[i];
                options.Add(new MenuOption
                {
                    DisplayValue = field.Name,
                    ReturnValue = field.Name
                });
            }

            // Post menu options.
            var activity = context.Activity as Activity;
            await context.PostAsync(ActivityGenerator.Default.GenerateMenuOptions(activity, options));

            context.Wait(ReceiveEditChoice);
        }

        /// <summary>
        /// Send receives field user wants to edit and begins QnA in TicketQnADialog to fill in the field.
        /// </summary>
        private async Task ReceiveEditChoice(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            // Receive the ticket's field that the user wants to edit.
            TicketFieldType editField;
            if (activity.Text.ToLowerInvariant() == "cancel")
            {
                await context.PostAsync("Okay, back to your ticket summary.");
                context.Done<object>(null);
            }
            else if (Enum.TryParse(activity.Text, out editField))
            {
                ticketFieldsTracker.SetupEditAnswer(editField);
                await context.Forward(new TicketQnADialog(), ReceiveEditAnswer, ticketFieldsTracker, CancellationToken.None);
            }
            else
            {
                await context.PostAsync(@"Please select one of the options, or say ""cancel"" go back.");
                context.Wait(ReceiveEditChoice);
            }
        }

        /// <summary>
        /// Complete edit process.
        /// </summary>
        private async Task ReceiveEditAnswer(IDialogContext context, IAwaitable<object> result)
        {
            // Receives the result of user editing the ticket field.
            var qnaResult = await result as string;

            // Success
            if (qnaResult == null)
            {
                context.Done<object>(null);
            }
            else if (qnaResult == "cancel")
            {
                ticketFieldsTracker.CancelEditAnswer();
                await context.PostAsync("Okay, back to your ticket summary.");
                context.Done<object>(null);
            }
        }
    }
}