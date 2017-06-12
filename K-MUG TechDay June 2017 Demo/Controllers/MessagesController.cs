using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using K_MUG_TechDay_June_2017_Demo.Dialogs;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace K_MUG_TechDay_June_2017_Demo
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                if ((activity.Text ?? string.Empty) == "//restart" || Global.DeleteData || (activity.Text ?? string.Empty) == "debug")
                {
                    activity.GetStateClient().BotState.DeleteStateForUser(activity.ChannelId, activity.From.Id);
                    await activity.GetStateClient()
                        .BotState.DeleteStateForUserAsync(activity.ChannelId, activity.From.Id);
                    Global.DeleteData = false;
                    //Controllers.Global.NextTask = "Welcome";
                }
                try
                {
                    await Conversation.SendAsync(activity, () => new Dialogs.RootDialog());
                }
                catch (Exception ex)
                {
                    await connector.Conversations.ReplyToActivityAsync(activity.CreateReply(ex.ToString()));
                }
            }
            else
            {
                await HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private async Task<Activity> HandleSystemMessage(Activity message)
        {
            bool msgSent = false;
            StateClient stateClient = null;
            BotData userData = null;
            ConnectorClient connector = new ConnectorClient(new Uri(message.ServiceUrl));
            try
            {
                stateClient = message.GetStateClient();

                userData = await stateClient.BotState.GetUserDataAsync(message.ChannelId, message.From.Id);
                if (userData.GetProperty<bool>("SentGreeting"))
                {
                    msgSent = true;
                }
            }
            catch (Exception ex)
            {
                await connector.Conversations.ReplyToActivityAsync(message.CreateReply(ex.ToString()));
            }
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                //await connector.Conversations.ReplyToActivityAsync(
                //                message.CreateReply(
                //                    "member added is : " + message.MembersAdded+", msg sent:"+msgSent));
                if (message.MembersAdded != null && message.MembersAdded.Any())
                {
                    foreach (ChannelAccount newMember in message.MembersAdded)
                    {
                        if (newMember.Id == message.Recipient.Id)
                        {
                            if (msgSent) continue;
                            await connector.Conversations.ReplyToActivityAsync(message.CreateReply("Hey..."));
                            msgSent = true;
                        }
                        //else
                        //{
                        //    if (msgSent) continue;
                        //    await connector.Conversations.ReplyToActivityAsync(message.CreateReply("Hey..."));
                        //    msgSent = true;
                        //}
                    }
                }

                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}