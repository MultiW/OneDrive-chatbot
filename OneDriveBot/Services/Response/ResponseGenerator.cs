using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OneDriveBot.Services.Response
{
    public class ResponseGenerator
    {
        public static readonly ResponseGenerator Default = new ResponseGenerator();

        /// <summary>
        /// Convert response string to a list of messages and sends it to the user.
        /// </summary>
        /// <param name="activity">Activity object user sent. Contains user and conversation id that bot can use to respond.</param>
        /// <param name="response">Resposne text to be sent.</param>
        /// <param name="context">Helps send and receive messages, contains conversation state data.</param>
        /// <returns></returns>
        public async Task SendResponseAsync(Activity activity, string response, IDialogContext context)
        {
            // Split response into different lines.
            string[] responseArray = response.Split(new string[] { "\r\n", "\n\n" }, StringSplitOptions.None);

            // Display response.
            try
            {
                await SendAllResponseMessages(activity, responseArray, context);
            }
            // Catch any formatting errors
            catch (Exception e) 
            {
            }
        }

        /// <summary>
        /// Converts response array to a list of activities and send them.
        /// </summary>
        /// <param name="activity">Activity object user sent. Contains user and conversation id that bot can use to respond.</param>
        /// <param name="responseArray">Array of all lines to be sent.</param>
        /// <param name="context">Helps send and receive messages, contains conversation state data.</param>
        private async Task SendAllResponseMessages(Activity activity, string[] responseArray, IDialogContext context)
        {
            // regex patterns for menu options and image patterns
            string menuPattern = @"^\s*\[M:(.*)\]\s*(.*)";
            string imagePattern = @"^\s*\!\[(.*)\]\s*\((.*)\)";

            // currently being created text messages and menu option lists
            string currentTextMessage = "";
            List<MenuOption> currentMenuList = new List<MenuOption>();

            // Loop each line of response array
            for (int i = 0; i < responseArray.Length; i++)
            {
                // Check the format of currentLine
                string currentLine = responseArray[i];
                Match menuMatch = Regex.Match(currentLine, menuPattern);
                Match imageMatch = Regex.Match(currentLine, imagePattern);

                // Act based on response line's format (menu option, image, or regular text)
                if (menuMatch.Success)
                {
                    // If previous line(s) are text messages, not menu options, then send those text messages
                    await SendResponseMessage(activity, context, currentTextMessage);
                    currentTextMessage = "";

                    // Add current line/menu option to currentMenuList
                    currentMenuList.Add(ParseMenuButtonConfig(menuMatch));
                }
                else if (imageMatch.Success)
                {
                    // If previous line(s) are text messages, not menu options, then send those text messages
                    await SendResponseMessage(activity, context, currentTextMessage);
                    currentTextMessage = "";

                    // Create and send image
                    string url = imageMatch.Groups[2].Value;
                    await this.SendImageAsync(activity, context, url, "image/png");
                }
                else
                {
                    // If previous line(s) are menu options, not text messages, then send those menu options
                    await SendResponseMessage(activity, context, menuOptions: currentMenuList);
                    currentMenuList = new List<MenuOption>();

                    // Add a line to currently being created text response
                    currentTextMessage += currentLine + "\n\n";
                }
            }
            // Send last batch of text or menu options.
            await SendResponseMessage(activity, context, currentTextMessage, currentMenuList);

        }

        private async Task SendResponseMessage(Activity activity, IDialogContext context, string text = "", List<MenuOption> menuOptions = null)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                var replyActivity = ActivityGenerator.Default.GenerateTextualResponse(activity, text);
                await context.PostAsync(replyActivity);
            }

            if (menuOptions != null && menuOptions.Count != 0)
            {
                var replyActivity = ActivityGenerator.Default.GenerateMenuOptions(activity, menuOptions);
                await context.PostAsync(replyActivity);
            }
        }

        private MenuOption ParseMenuButtonConfig(Match menuMatch)
        {
            // Parse menu option display text
            var menuOption = new MenuOption()
            {
                ReturnValue = menuMatch.Groups[1].Value,
                DisplayValue = menuMatch.Groups[2].Value
            };
            if (menuOption.ReturnValue == "")
            {
                menuOption.ReturnValue = menuOption.DisplayValue;
            }

            return menuOption;
        }

        private async Task SendImageAsync(Activity activity, IDialogContext context, string url, string imageType)
        {
            var reply = activity.CreateReply();

            reply.Attachments.Add(new Attachment()
            {
                ContentUrl = url,
                ContentType = imageType
            });

            await context.PostAsync(reply);
        }
    }
}