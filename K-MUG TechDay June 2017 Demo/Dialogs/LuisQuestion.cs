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
                await context.PostAsync("Thinking.!");
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
                                            await context.PostAsync("Looking to my warehouse...");
                                            await context.PostAsync(typingpush);
                                            await Global.TypingDelay(3500);
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
                                                                    from = entity;
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
                                                string response = string.Empty;
                                                if (from.Length > 0 && to.Length > 0)
                                                {
                                                    response = "Here are the 2 " + product + ", from " + from + " to " + to;
                                                }
                                                else
                                                {
                                                    response = "Here are the 2 " + product;
                                                }
                                                await context.PostAsync(response);
                                                await context.PostAsync(typingpush);
                                                await Global.TypingDelay(3500);

                                                await SendAttachment(context, "getPrice");
                                            }
                                            else
                                            {
                                                await context.PostAsync("get price, intent empty. Json: " + lj.Json.ToString());
                                            }
                                            break;
                                        case "colorChange":
                                            await context.PostAsync("Looking to my warehouse...");
                                            await context.PostAsync(typingpush);
                                            await Global.TypingDelay(3500);
                                            await context.PostAsync("Here it goes...");
                                            await context.PostAsync(typingpush);
                                            await Global.TypingDelay(3500);
                                            await SendAttachment(context, "colorchange");
                                            break;
                                        case "thankYou":
                                            await context.PostAsync(typingpush);
                                            await Global.TypingDelay(3500);
                                            await context.PostAsync("Thanks to you too. Have a great day");
                                            Global.DeleteData = true;
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

        private async Task SendAttachment(IDialogContext context, string mode)
        {
            var typingpush = context.MakeMessage();
            typingpush.Type = ActivityTypes.Typing;
            var heroCard = context.MakeMessage();
            heroCard.Recipient = context.Activity.From;
            heroCard.Type = "message";
            heroCard.Attachments = new List<Attachment>();
            string Title = "Here are the color changes available";
            string subtitle = "You get black, blue, red, wooden and white";
            switch (mode)
            {
                case "colorchange":
                    List<CardImage> cardImageWooden = new List<CardImage>();
                    List<CardImage> cardImageBlack = new List<CardImage>();
                    List<CardImage> cardImageBlue = new List<CardImage>();
                    List<CardImage> cardImageWhite = new List<CardImage>();
                    List<CardImage> cardImageRed = new List<CardImage>();
                    cardImageWooden.Add(new CardImage(url: "http://www.ikea.com/PIAimages/0173544_PE327678_S5.JPG"));
                    cardImageBlack.Add(new CardImage(url: "http://www.ikea.com/PIAimages/0122106_PE278491_S5.JPG"));
                    cardImageWhite.Add(new CardImage(url: "https://d39vwtwllgful3.cloudfront.net/products/SKU244D/2890x1500/image15187.jpg"));
                    cardImageRed.Add(new CardImage(url: "http://www.ikea.com/PIAimages/0125495_PE283075_S5.JPG"));
                    cardImageBlue.Add(new CardImage(url: "data:image/jpeg;base64,/9j/4AAQSkZJRgABAQAAAQABAAD/2wCEAAkGBxASDxIQERAQEhAXEhAWEBAPEBAQEhMRFREWFhYVFhUYHSggGB0mGxYYITEhJSkuLi4uGB8zODMtNygtLisBCgoKDg0OGhAQGislICUtMC8tKzAtLS0tLS0vLSstLS0tLS0tLS0tLS0tLy0tLS0tLS0tLS0tLS0tLS0tLS0tLf/AABEIAOEA4QMBEQACEQEDEQH/xAAcAAEAAQUBAQAAAAAAAAAAAAAABgEDBQcIBAL/xABMEAACAgEBBAQICgMNCQAAAAAAAQIDBBEFBgcSEyExUUFSYXGRkqHCIiMycoGTorGywUJzghQVJCUzQ2JjZIOjw9E0U1R0hKSz8PH/xAAaAQEAAwEBAQAAAAAAAAAAAAAAAwQFAQIG/8QAMBEBAAICAQEGBgICAQUAAAAAAAECAwQRIRIiMVFhcSMyM0GB8AUTQsGxFBUkUtH/2gAMAwEAAhEDEQA/AN4gAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAEM3034eDfCiNCtbrVkua116RcpRilpGWr1hL2FnBrf21meUGTNFJ4eLE4qYz/lMe+Hlg67I+1xfsPdtK8eEw5GxVmMTf8A2bP+fcH3W12QXraae0itrZa/ZJGWk/dmcLbWLd1VZNFj7q7a5P0J6kU0tHjD3Fonwe7U8uqgAAAAAAAAAAAAAAAAAAAAAAAAAAYGj+K1/NtSa8SqmHsdn+YbGlHGL8yy9ufifhDy2qq6s7xDvMqSlr1NJruaTOcOxks9+FtvJp06K+6tLsjC2cY+rrp7CO2ClvGISV2Lwz+FxG2jX22xsXddVB+2HK/aQW0scpo3LfdnsHizLqV2LB98qrZQf0Qkn+Igt/Hz/jKau5WfFLN2t+cXNu6CuN0LeSU+WyMdOWLSb5otr9JdpVy698ccz4Jseat54hKCBMAAAAAAAAAAAAAAAAAAAAAAGBonivy/vpa49T6OnpNPDPk7X+zyr6DZ0ef6uvnLI3Zj+38LW9e7deJg4WTGyyVt8ISshPk5F8SpycdEn8prtbOa2e2S0xPhD1sYa46xMeMre826dmFj0ZE7YWK1xXJGMoyi3W5vtbTS001PWHZ/ttNYhzNg/qr2uWP2hsPJooqybaXCi1VuqfNW9eeHPFOKbknyp9q8BJTPS9prE9Ud8N6RzMdHgspnFRlKE4xkk4OUJRUotapxbXWmvCiWL1nwlHNZjxhbPTwow5KfcFMfXOybfEx1D6yyL/yjO/kJ7sQv6EdZluUy2mAAAAAAAAAAAAAAAAAAAAAAGBz1xKv5tpZsteySiv2KYR09KNzVjjDDH2OuaUn4wUt/vfjR6mqrYpeWXQwj+FlXRniL2/fusbnWaR6vRxvt0hiVR7siWi/oquMfvZz+P8bSb3hWvq+uMcujw8KheDnf1VSivxjQjm9reju78la+q5xVSp2VhY/dKpNLs0qx5L72jzoxzlm374u7s8Y4hit/9j4+NszZ/JTXXe1V0tsIRjZNQx/hKbXyvhST1fcSad7Xy25no8bla1xxxHVH969jV4scOMHPpbMSq3IU2mlZNJfBSXV1qXV5ixrZbZO1NvtKDZxVpMRVsLgzsjo8SeXzau+WnLppyxpnZFdfh1bbKG7k7V+z5LunSK0582wymtgAAAAAAAAAAAAAAAAAAAAAFGBzhtz4/aFyb6rMycdfJLIcV7Ddr3cEe3+mLPez/lOd/wBO3b+DV4F+5NV/1MpS+yinr93WvK1n67FIfHFGXSbWwqPJQn/e5PK/ZFHdTphvb98Da65qVfXFp9LtHBx9e1RWn66+MPcOafTFextdctKq8Z27L8LHXbJWdXlsnXCP3MaPdrezu51tSvqtcaU53YeNB6S6O1RXc7ZwhFv6Yfed0elL3NvrelWC4oZCe07Yr5NVdNUfIlWpffMn044xc+cq25POT8NscP6FDZWGl1a0Qn9b8Z7xl555y292phjikQkJEkAAAAAAAAAAAAAAAAAAAAAAPmyWib7k36AObtga27Qw+rrlmY0pLydPGUvZqb2fu4Z9mJgntZo909yvjd64LtVbjr+xiOf4pIpR3dP3XJ67ftC3tx9LvTRDX5E8debkqd2ntO4+7qWn98nL9dqI8ofW8Wl29GNXrryPGXmdcZZH5nMfd1LT5mTvbVY8nzvlHpt48OvVfAeEmu35Fsr5L6Ys7h7urafMy97ZrBvdHp95MSrr+BLDTXzJyyH7Gcwz2dW0+buTvbNY8oQPezLdmXl2+Nfdy/NU5KP2Ui9hr2cVY9FLLPayz7ujNmY6qoqqXZCuuC07owS/IwbTzPLbiOI4eo46AAAAAAAAAAAAAAAAAAAAAAY7ePI6PCyrPEx75erVJnvHHN4j1ebzxWZaI3Br5trYMf62T9SmcvdNndnjFLH0uuVMt1X0u8uZPt5P3V19zhOFP+qKmbpq1haw9dm8rWw/jt6bp668k8l/V1dBp7TuTu6lY8zH3tq0+T62IldvTfZ29G8h+Z11RxveGTu6lY83cfe2bT5KYC6beuyfgrna/q8VU/exfu6kR5uU721PpCxXkJ7x5WQ+tUQyrPMqcZVfezto/wDGrXzkr12bW8oQPZNHPkY1T6+a+iL18PNZGL19JdzTxSfZRw97JHu6cR8+3lQAAAAAAAAAAAAAAAAAAAAAAEc4h3qGystvw18n1k41+8T60c5ax6odieMVp9GpeF9SltjHfixvl/hSj7xpb8/DZ2hHf5SfhJpbn7Qv7detS8l185+4V93pjpVY1Ot729VnhbHpdr5uRrqnHIfn6bKjNP7D9J3c6YaV/fBzV65byucKkrdqZ+R29Vj1/X5Dn7hzc6YqV/fA1I5yXt6vnhh8btjOyOvTS96/rclNeyDG53cNK/vgavXLeyO7OyU1tnL8auUF4f8Aa8vwPzRZPevexU/eiGkxxlu8W4dPPtbDj3W83qQlP3STbnjFKLTjnJDokw22AAAAAAAAAAAAAAAAAAAAApqBhttb1YWL1XZEFP8A3UdbLPUjq1531EtMN7/LCO+WlPGWv9+d/cXLwbMamN6nKVT1shGMXGFkZPrUm/0e4va2rkpki1lLZ2aXxzWqL8O9oVY+fK66ca4rFvUJS7Ha3Dlj52tSfcpa9Yisc9UGnetOeZ4Svg7OFWLm2uUVNcq0bWrjVVKeunbp8Nlbf5m9YWdLiKWn1WeDy6PH2hkP9GupeX4uFs3969B3f62pV50fltZd4Q/FYe0Ml6dSim/1VUpv8Zzf65K1etLpjtb1WeEr6HE2llP9CqHwv1VVlj/Eju93slKR+9XnS6UtZEsT4Gybmu23NoqflhRjys/FbH0luY52I9I/5VueNeZ85ZPhLTzbXg/Eqvl9lQ98i35+H+UmjHfb4MdrAAAAAAAAAAAAAAAAAAA8e0dqUY8Oe+2uqPgdklHXyJPtfkR6rS1p4rHLza0VjmZQjbPFLHhrHFqnfLx5601+1cz9C85bx6N5+aeFa+3WPl6oLtjfXaGTqp3uuD/m8ZOmPpTcn5nJou01cVPtz7qt897ffj2R5Iswg4UaHLkwtuB65RTXqo613Dk7L1Ym0b6oWV1XWQhYmrIRk1CaceV80ex9XUeLY6WmJmOr1XJesTET0ZPZG9F2Ph5GFGup1Xq1Sk+ZWRlZWq209dGtF2afSRZNat8kX58EuPYmmOacMju9vFj0bIzsSXOsi52OGkG4OM666+XmXZ1KT69O0iy4L3z1vHhHCXDnpXDNfv1YbaE0sHCgn1t5ts14fhXKqLf0UP6GTYo5y3n2hFmnjFSv5SXgnTrn32eCONKP0ztra/Ayrvz3Yj1WNCO9Mt1GW0wAAAAAAAAAAAAAAAAAowOft88PLhm5FmRVbpK2xwtkpSg6nN9Goz7NFHTq16u42te2OaRFZj2ZOft1tM2jowSsXm85Y7MwhjJWX2jj30k0Bwpodc4U0DnCmh1zg0OucKaBzhTlHLz2XntsihNuHmKTLZnAqv4zNn3QxYr6ZWt/hRmb8/LDT0Y6S26Zy+AAAAAAAAAAAAAAAAAACP7/AC/irN/5ez2olwfUr7o8s8Un2c48r5lo32Pq8Hajd4ntMTmJpPMLilJf+6Hvqj6fZcjkef6es8zw9xa0fddjkL/4x2Xr+3zh9qa7xxL1GSsq6HHoHJwt2TS7fR4TrxPR5bL2/MeZlxYmutef8jzaPBJjniLcNycDK/4PlT07b4R1+bWnp9v2mdvfPHsvaUdz8tnFFcAAAAAAAAAAAAAAAAAABHOIctNlZf6rT0zivzJ9b6tfdDsfSt7Occ25wTkktVFvr185tZLTWejKwY4v0luarhLiyrjJZOTGTjF9fRSSbSfZyp+0zf8AuGTyhd/6DH5yxufwhuWrpy6p90bq5V/ai5fcS1/kY/yqjt/H/wDrZB94d3MjCnGGTWoOXN0clOMozUdOZxaevVzLtSfWW8ebHk+WVTJhyY/FiuVrw/mS8T9kPMfeFVY14PQxzPk7EeUqSyn2fC9h4m0eSTs2455W4xlJpJNybSSScpNvsSXa2cmenMkRzPENg7q8LMi7SzLbx6upqtaO+S8qfVX9Or8iKWbciOlOvqu4tOZ63Q7evErp2jlU1Rcaq7XCuLlKWijGKfW3q9Xq/pJde02iJlzYitYmIht7gpTy7NnLxsmx+iuuPulLdnnL+FjTj4bYBUWgAAAAAAAAAAAAAAAAAARfibLTZOV82r231osav1qoNn6VnOW0/kS+ZL7ma+bxZ2r/ALdZwjoku5JegwGw+gNQcef5TB+Zl/ioL+l4W/H+1XZ+zycMN0sbP2dkO5SVkcucK7oSanCKx6JaadklrJ9TXhfYdzbF8d47M/ZyNemSvWEb3u3cngZPQTnCzWCnCcE46wcpRXNF/JesX1avzl/BmjNXtccMzPhnFbjlY3S3eln5f7mjZGv4E5ynKLlpCLinol2vWS8KPGfL/VXtcJdfF/ZPDee6+5uHgrWqvmt00lkW6Stffo+yC8kUvLr2mRlzXyeLUx4q0jokOhElcw7z28+0c2T/AOLyl6t0o/kbOvHFY9mZtT4t28JKuXZFL005p5EvP8dNL2JGdtzzllb1vpQmRXWAAAAAAAAAAAAAAAAAAARPilLTZGT/AHC/7iss6kfGhX2vpWc+W1c8lDxtI+s9DVzSztb/AG6vMFsgGnuO0vjsJf1eR7ZVf6GhpeFlPanwZzgbVps21+NmWv0U0x90g2p7/wCE2D5EU4wz12n5selfasf5mh/H/Tn3/wDjO3/nj2XOCVLe0LrPBHFnH6Z3VNfgZDvz3Yj1S6MdZbsMxpBwcs7XlzZeTLvyMiXrXSf5m7hjux7MjYnmZ93QXDqrl2ThrvpUvXk5+8ZGeeclvdpYY4xwkhElAAAAAAAAAAAAAAAAAABD+LD/AIov+fj/APngWtP60fv2V9v6NmisBa5VC/rqF6bYmnsfLPsz9b7e7qUwmwAab46P+E4n6m38cf8AQ0dH5bfhR2/GqU8GI/xTF99+R+PT8its/UlYwfJCAcVLNdrX+SNK/wAKL/M09GPgszennKzfAmvW3Nl3Qxl60rX7pV35+X8rWj4S2+Zy+owOVs2Wttr77LH6Ztm/jjisezEyzzafd0fuTDTZmEv7Lj+2qLMTN9S3u2MfyQzZG9gAAAAAAAAAAAAAAAAAAhPF+3TZc1411C9Eub3S3pRzmj8/8K25PwpaV2JDXOxI9+XiL05EEaWx8s+yjqeMOoDCa4BBOIu4120LKrarq4SrrlHksjLSTctdeeOund8llnX2P6omJjxQZsP9nHXwZzcjYDwMGvGlNTmnOU5RWkeec3JqOvXotdFr3eDsIcl+3btJaV7McNM8RrNdrZj/AKcF6KYL8ja0+mGGNt/WlNOBdfxOXPvsqj6sJP3ihvz3oj0XtKO5LaJRXQDlLI+XP50vvZ9BX5YYeT5p93TW7ENMDEXdjY69FUTByfNLar4QyZ5egAAAAAAAAAAAAAAAAAAa941W6YFMfGyY6/RVYy7oR8X8Ke9PGL8tV7oUdJtTDj/aaJfVzVnul7anilvZV1I6w6WMRrAACjA5w32s5to5j/tFq9WXL+Rv60cYa+zC2Z5zS2XwQq0wLpeNlT08ypqX36mZvT8SPZpacfDbFKa2owOULpfKfg62fQV8IYdo735dT7MhpRVHurrXogjAnrLbjwek46AAAAAAAAAAAAAAAAAADVnHDJ0WHV3yvn6qhH3zS/jo71pZ+/PdiEb4Q7Odu1Ol0fJRXZJy0einJdHFa9+kpP8AZPW9fivHm5p0/wAm9zLaIAAAcybxWc2XlS78nJfpukz6HFHGKvswMvXLPu3Jwdq02VB+Nde/RPl90x9ufiy19WPhQm5WWFGgIJk8KNmzv6TS6Fb15sauzSqT8j054rt6lJeDTTw2Y28kV4QTrY+12uE6hHRaLsWiXmRWTvoAAAAAAAAAAAAAAAAAAALGXh12xcLa4WQfbGyEZx9DOxMxPMOTET4vnAwKaIKumquqtatQqhGEdX2vReETMzPMkRx0h6TjoAAowOV8i7n1m+2UnJ/tPX8z6PjisQ+emebzLoXhzidFsrEj4XUrH/eydnvGDntzktPq3cMcUiEkIkgAAAAAAAAAAAAAAAAAAAAAAAAAAAABayteSXKtZcstF5dHoI8XJ8HOmzt0cy7IrxJY99bbgrZSrlBV1apSnzNadS1072urU3MuzjinMSx8WvecnWHRtUFGKjFaRSSil4ElokYbZfYAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABTQCoAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAf/2Q=="));
                    HeroCard plCardBlack = new HeroCard()
                    {
                        Title = Title,
                        Subtitle = subtitle,
                        Text = "This is the black one",
                        Images = cardImageBlack
                    };
                    HeroCard plCardBlue = new HeroCard()
                    {
                        Title = Title,
                        Subtitle = subtitle,
                        Text = "This is the blue one",
                        Images = cardImageBlue
                    }; HeroCard plCardWooden = new HeroCard()
                    {
                        Title = Title,
                        Subtitle = subtitle,
                        Text = "This is the wooden one",
                        Images = cardImageWooden
                    }; HeroCard plCardWhite = new HeroCard()
                    {
                        Title = Title,
                        Subtitle = subtitle,
                        Text = "This is the white one",
                        Images = cardImageWhite
                    }; HeroCard plCardRed = new HeroCard()
                    {
                        Title = Title,
                        Subtitle = subtitle,
                        Text = "This is the red one",
                        Images = cardImageRed
                    };
                    Attachment plAttachmentBlack = plCardBlack.ToAttachment();
                    Attachment plAttachmentBlue = plCardBlue.ToAttachment();
                    Attachment plAttachmentWooden = plCardWooden.ToAttachment();
                    Attachment plAttachmentWhite = plCardWhite.ToAttachment();
                    Attachment plAttachmentRed = plCardRed.ToAttachment();
                    heroCard.Attachments.Add(plAttachmentBlack);
                    heroCard.Attachments.Add(plAttachmentBlue);
                    heroCard.Attachments.Add(plAttachmentRed);
                    heroCard.Attachments.Add(plAttachmentWhite);
                    heroCard.Attachments.Add(plAttachmentWooden);
                    heroCard.AttachmentLayout = "carousel";
                    await context.PostAsync(heroCard);
                    await context.PostAsync(typingpush);
                    await Global.TypingDelay(3500);
                    await StartAsync(context);
                    break;

                case "getPrice":

                    List<CardImage> cardImages1 = new List<CardImage>();
                    List<CardImage> cardImages2 = new List<CardImage>();
                    cardImages1.Add(new CardImage(url: "http://images.pier1.com/dis/dw/image/v2/AAID_PRD/on/demandware.static/-/Sites-pier1_master/default/dwf6aeaaf4/images/2248859/2248859_1.jpg?sw=1600&sh=1600"));
                    cardImages2.Add(new CardImage(url: "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSSN_Tq5ow8TlxUA8HnHAF0o107XkDIy46T9uKjYcBSvwRPXz-1"));

                    HeroCard plCard1 = new HeroCard()
                    {
                        Title = "Here are the available ones",
                        Subtitle = "You get gray and black",
                        Images = cardImages1
                    };
                    HeroCard plCard2 = new HeroCard()
                    {
                        Title = "Here are the available ones",
                        Subtitle = "You get gray and black",
                        Images = cardImages2
                    };
                    Attachment plAttachment1 = plCard1.ToAttachment(); Attachment plAttachment2 = plCard1.ToAttachment();
                    heroCard.Attachments.Add(plAttachment1);
                    heroCard.Attachments.Add(plAttachment2);
                    heroCard.AttachmentLayout = "carousel";
                    await context.PostAsync(heroCard);
                    await context.PostAsync(typingpush);
                    await Global.TypingDelay(3500);
                    await StartAsync(context);
                    break;

            }
        }
    }
}