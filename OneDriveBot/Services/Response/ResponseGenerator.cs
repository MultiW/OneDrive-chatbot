using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace OneDriveBot.Services.Response
{
    public class ResponseGenerator
    {
        public static readonly ResponseGenerator Default = new ResponseGenerator();

        /// <summary>
        /// Convert response string from database to a list of activities to be sent to the user.
        /// </summary>
        /// <returns></returns>
        public async Task SendResponse(Activity activity, string response, IDialogContext context)
        {
            // Split response into different lines.
            string[] createActivitiesList = response.Split(new string[] { "\r\n", "\n\n" }, StringSplitOptions.None);

            // Display response.
            try
            {
                await CreateActivityList(activity, createActivitiesList, context);
            }
            catch (Exception e)
            {
                // catch any formatting errors
            }
        }

        private async Task CreateActivityList(Activity activity, string[] createActivitiesList, IDialogContext context)
        {
            string oneTextMessage = "";
            List<MenuOption> oneMenuList = new List<MenuOption>();
            foreach (string line in createActivitiesList)
            {
                string menuPattern = @"^\s*\[M:(.*)\]\s*(.*)";
                string imagePattern = @"^\s*\!\[(.*)\]\s*\((.*)\)";
                Match menuMatch = Regex.Match(line, menuPattern);
                Match imageMatch = Regex.Match(line, imagePattern);

                // Line is one menu option. Add to oneMenuList.
                if (line != null && line.Length > 2 && menuMatch.Success)
                {
                    // Send text message
                    await SendMessage(activity, context, oneTextMessage);
                    if (oneTextMessage != "")
                    {
                        oneTextMessage = "";
                    }

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

                    // Add to oneMenuList.
                    oneMenuList.Add(menuOption);
                }
                else if (line != null && line.Length > 2 && imageMatch.Success)
                {
                    await SendMessage(activity, context, oneTextMessage);
                    oneTextMessage = "";
                    string url = imageMatch.Groups[2].Value;
                    await this.SendImageAsync(activity, context, url, "image/png");
                }
                // Line is a line of a text response. Add to oneTextMessage.
                else
                {
                    await SendMessage(activity, context, menuOptions: oneMenuList);

                    if (oneMenuList.Count != 0)
                    {
                        oneMenuList = new List<MenuOption>();
                    }

                    // Add to oneTextMessage.
                    oneTextMessage += line + "\n\n";
                }
            }

            await SendMessage(activity, context, oneTextMessage, oneMenuList);
        }

        private async Task SendImageAsync(Activity activity, IDialogContext context, string url, string imageType)
        {
            var reply = activity.CreateReply();

            reply.Attachments.Add(new Attachment()
            {
                ContentUrl = url,
                ContentType = imageType
            });

            /*
            HeroCard imageCard = new HeroCard()
            {
                Images = new List<CardImage>()
                {
                    new CardImage(url)
                }
            };

            reply.Attachments.Add(imageCard.ToAttachment());
            */
            await context.PostAsync(reply);
        }

        private async Task SendMessage(Activity activity, IDialogContext context, string text = "", List<MenuOption> menuOptions = null)
        {
            if (text != "")
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
    }
}