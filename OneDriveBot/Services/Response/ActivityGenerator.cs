using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OneDriveBot.Services.Response
{
    public class ActivityGenerator
    {
        public static readonly ActivityGenerator Default = new ActivityGenerator();

        public Activity GenerateTextualResponse(Activity activity, string text)
        {
            var reply = activity.CreateReply(text);
            reply.TextFormat = TextFormatTypes.Markdown;
            reply.Type = ActivityTypes.Message;
            return reply;
        }

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