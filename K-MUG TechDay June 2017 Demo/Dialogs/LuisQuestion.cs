using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using K_MUG_TechDay_June_2017_Demo.Utilities;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Connector;

namespace K_MUG_TechDay_June_2017_Demo.Dialogs
{
    [Serializable]
    public class LuisQuestion : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            var typingpush = context.MakeMessage();
            typingpush.Type = ActivityTypes.Typing;
            await context.PostAsync(typingpush);
            var questionFormDialog = FormDialog.FromForm(BuildquestionForm, FormOptions.PromptInStart);
            //await context.PostAsync("Processing...");
            context.Call(questionFormDialog, ResumeAfterquestionformDailog);
        }

        private IForm<Question> BuildquestionForm()
        {
            OnCompletionAsyncDelegate<Question> processquestion = async (context, state) =>
            {
                state.questionFromUser = state.questionFromUser.Trim();
                context.UserData.SetValue("Question", state.questionFromUser);
                await context.PostAsync("Looking to my warehouse...");
            };

            return new FormBuilder<Question>()
                .Field(nameof(Question.questionFromUser))
                .AddRemainingFields()
                .OnCompletion(processquestion)
                .Build();
        }

        private async Task ResumeAfterquestionformDailog(IDialogContext context, IAwaitable<Question> result)
        {
            //    await context.PostAsync("In resume form dialog...");
            var typingpush = context.MakeMessage();
            typingpush.Type = ActivityTypes.Typing;

            await context.PostAsync(typingpush);
            var tempQuestion = "";
            if (context.UserData.TryGetValue("Question", out tempQuestion))
            {
                if (tempQuestion.Length > 1)
                {
                    LuisJson lj = new LuisJson();
                    //call luis
                    try
                    {
                        LuisCommunicator lc = new LuisCommunicator();
                        lj = await lc.CallEngine(tempQuestion);
                        // await context.PostAsync("json:" + lj.Json.ToString());
                        if (!string.IsNullOrEmpty(lj.query))
                        {
                            //has query
                            if (lj.intents.Length > 0)
                            {
                                if (!string.IsNullOrEmpty(lj.intents[0].intent) && !string.IsNullOrEmpty(lj.topScoringIntent.intent.ToString()))
                                {
                                    switch (lj.topScoringIntent.intent.ToString())
                                    {
                                        case "getPrice":
                                            if (lj.entities.Length > 0 && !string.IsNullOrEmpty(lj.entities[0].entity) &&
                                                !string.IsNullOrEmpty(lj.entities[0].type))
                                            {
                                                string product = string.Empty;
                                                string from = string.Empty;
                                                string to = string.Empty;
                                                //  await context.PostAsync("entity:" + lj.entities[0].type.ToString());
                                                int i = 0;
                                                foreach (var x in lj.entities)
                                                {
                                                    string type = x.type.ToString();
                                                    string entity = x.entity.ToString();
                                                    switch (type)
                                                    {
                                                        case "Product":
                                                            product = entity;
                                                            //   await context.PostAsync("product is: " + product);
                                                            break;
                                                        case "builtin.currency":
                                                            switch (i)
                                                            {
                                                                case 0:
                                                                    @from = entity;
                                                                    //     await context.PostAsync("from: " + @from);
                                                                    break;
                                                                case 1:
                                                                    to = entity;
                                                                    //    await context.PostAsync("to: " + to);
                                                                    break;
                                                            }
                                                            i++;
                                                            break;
                                                    }
                                                }
                                                await context.PostAsync("Here are the 2 " + product + ", from: " + @from + " ~ to: " + to);
                                                await context.PostAsync(typingpush);
                                                await Global.TypingDelay(3500);
                                                //slide horizontally
                                                //    await context.PostAsync("Should go to conversation, with a hero card");
                                                var heroCard2 = context.MakeMessage();
                                                heroCard2.Recipient = context.Activity.From;
                                                heroCard2.Type = "message";
                                                heroCard2.Attachments = new List<Attachment>();
                                                List<CardImage> cardImages2 = new List<CardImage>();
                                                cardImages2.Add(new CardImage(url: "http://images.pier1.com/dis/dw/image/v2/AAID_PRD/on/demandware.static/-/Sites-pier1_master/default/dwf6aeaaf4/images/2248859/2248859_1.jpg?sw=1600&sh=1600"));
                                                cardImages2.Add(new CardImage(url: "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSSN_Tq5ow8TlxUA8HnHAF0o107XkDIy46T9uKjYcBSvwRPXz-1"));
                                                //List<CardAction> cardButtons = new List<CardAction>();
                                                //CardAction plButton = new CardAction()
                                                //{
                                                //    Value = "https://en.wikipedia.org/wiki/Pig_Latin",
                                                //    Type = "openUrl",
                                                //    Title = "WikiPedia Page"
                                                //};
                                                //cardButtons.Add(plButton);
                                                HeroCard plCard2 = new HeroCard()
                                                {
                                                    Title = "Here are the available ones",
                                                    Subtitle = "You get gray and black",
                                                    Images = cardImages2
                                                    //Buttons = cardButtons
                                                };
                                                Attachment plAttachment2 = plCard2.ToAttachment();
                                                heroCard2.Attachments.Add(plAttachment2);
                                                heroCard2.AttachmentLayout = "carousel";
                                                await context.PostAsync(heroCard2);
                                                await context.PostAsync(typingpush);
                                                await Global.TypingDelay(3500);
                                                await StartAsync(context);
                                            }
                                            else
                                            {
                                                await context.PostAsync("get price, intent empty. Json: " + lj.Json.ToString());
                                            }
                                            break;
                                        case "colorChange":
                                            await context.PostAsync("Yes, of course");
                                            await context.PostAsync(typingpush);
                                            await Global.TypingDelay(3500);
                                            //slide horizontally
                                            //    await context.PostAsync("Should go to conversation, with a hero card");
                                            var heroCard = context.MakeMessage();
                                            heroCard.Recipient = context.Activity.From;
                                            heroCard.Type = "message";
                                            heroCard.Attachments = new List<Attachment>();
                                            List<CardImage> cardImages = new List<CardImage>();
                                            cardImages.Add(new CardImage(url: "http://images.pier1.com/dis/dw/image/v2/AAID_PRD/on/demandware.static/-/Sites-pier1_master/default/dwf6aeaaf4/images/2248859/2248859_1.jpg?sw=1600&sh=1600"));
                                            cardImages.Add(new CardImage(url: "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSSN_Tq5ow8TlxUA8HnHAF0o107XkDIy46T9uKjYcBSvwRPXz-1"));
                                            //List<CardAction> cardButtons = new List<CardAction>();
                                            //CardAction plButton = new CardAction()
                                            //{
                                            //    Value = "https://en.wikipedia.org/wiki/Pig_Latin",
                                            //    Type = "openUrl",
                                            //    Title = "WikiPedia Page"
                                            //};
                                            //cardButtons.Add(plButton);
                                            HeroCard plCard = new HeroCard()
                                            {
                                                Title = "Here are the color changes available",
                                                Subtitle = "You get black and blue",
                                                Images = cardImages
                                                //Buttons = cardButtons
                                            };
                                            Attachment plAttachment = plCard.ToAttachment();
                                            heroCard.Attachments.Add(plAttachment);
                                            heroCard.AttachmentLayout = "carousel";
                                            await context.PostAsync(heroCard);
                                            await context.PostAsync(typingpush);
                                            await Global.TypingDelay(3500);
                                            await StartAsync(context);
                                            break;
                                    }

                                }
                                else
                                {
                                    await context.PostAsync("No intent and top scoring intent. Json:" + lj.Json.ToString());
                                }
                            }
                            else
                            {
                                await context.PostAsync("Json:" + lj.Json.ToString());
                            }
                        }
                        else
                        {
                            await context.PostAsync("Json:" + lj.Json.ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        await context.PostAsync(ex.ToString());
                    }
                }
                else
                {
                    await context.PostAsync("Could not read that question");
                    await Global.TypingDelay(4500);
                    await StartAsync(context);
                }
            }


        }
    }
}