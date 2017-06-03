using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;

namespace K_MUG_TechDay_June_2017_Demo.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            // calculate something for us to return
            if (activity != null)
            {
                string Name = string.Empty;
                IMessageActivity reply = context.MakeMessage();
                if (!string.IsNullOrEmpty(reply.Recipient.Name))
                {
                    Name = reply.Recipient.Name;
                }

                var typingpush = context.MakeMessage();
                typingpush.Type = ActivityTypes.Typing;
                await context.PostAsync(typingpush);
                await Global.TypingDelay(3500);
                await context.PostAsync("Hi, " + Name);
                await context.PostAsync(typingpush);
                await Global.TypingDelay(3500);
                await context.PostAsync("Welcome to sales bot from KMUG!");


                await context.PostAsync(typingpush);
                await Global.TypingDelay(3500);

                await context.PostAsync("You seemed to be intersted in our product, chair");
                await context.PostAsync(typingpush);
                await Global.TypingDelay(2500);

                List<CardImage> cardImages = new List<CardImage>();
                cardImages.Add(new CardImage(url: "http://images.pier1.com/dis/dw/image/v2/AAID_PRD/on/demandware.static/-/Sites-pier1_master/default/dwcd617f89/images/2500470/2500470_1.jpg?sw=1600&sh=1600"));
                List<CardAction> cardButtons = new List<CardAction>();
                CardAction plButton = new CardAction()
                {
                    Value = "https://en.wikipedia.org/wiki/Pig_Latin",
                    Type = "openUrl",
                    Title = "Buy Me!"
                };
                cardButtons.Add(plButton);
                HeroCard plCard = new HeroCard()
                {
                    Title = "The best Chair",
                    Subtitle = "1200₹",
                    Images = cardImages,
                    Buttons = cardButtons
                };

                var message = context.MakeMessage();
                message.Attachments = new[] { plCard.ToAttachment() };

                await context.PostAsync(message);

            }

            context.Wait(MessageReceivedAsync);
        }
    }
}