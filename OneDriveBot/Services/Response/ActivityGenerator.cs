using Microsoft.Bot.Connector;
using System.Collections.Generic;

namespace OneDriveBot.Services.Response
{
    public class ActivityGenerator
    {
        public static readonly ActivityGenerator Default = new ActivityGenerator();

        /// <summary>
        /// Create a text in markdown format given a text.
        /// </summary>
        /// <param name="activity"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public Activity GenerateTextualResponse(Activity activity, string text)
        {
            var reply = activity.CreateReply(text);
            reply.TextFormat = TextFormatTypes.Markdown;
            reply.Type = ActivityTypes.Message;
            return reply;
        }

        /// <summary>
        /// Create a menu activity given a list of menu options.
        /// </summary>
        /// <param name="activity"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public Activity GenerateMenuOptions(Activity activity, List<MenuOption> options)
        {
            // Create button list
            List<CardAction> buttonList = new List<CardAction>();
            foreach (MenuOption option in options)
            {
                buttonList.Add(new CardAction(ActionTypes.ImBack, option.DisplayValue, value: option.ReturnValue));
            }

            // Create menu using button list
            HeroCard menu = new HeroCard()
            {
                Buttons = buttonList
            };

            // Create activity
            Activity reply = activity.CreateReply();
            reply.Attachments.Add(menu.ToAttachment());
            reply.Type = ActivityTypes.Message;

            return reply;
        }
    }
}