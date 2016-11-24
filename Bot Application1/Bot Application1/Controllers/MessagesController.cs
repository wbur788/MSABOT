using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using System.Collections.Generic;
using Bot_Application1.Models;

namespace Bot_Application1
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        /// 
        public int id;

        [ResponseType(typeof(void))]
        public virtual async Task<HttpResponseMessage> Post([FromBody] Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                StateClient stateClient = activity.GetStateClient();
                BotData userData = await stateClient.BotState.GetUserDataAsync(activity.ChannelId, activity.From.Id);
                var userMessage = activity.Text;
                String endOutput="";

                HttpClient client = new HttpClient();
                string URL = "http://shrek.azurewebsites.net/tables/BankData?zumo-api-version=2.0.0";
                string x = await client.GetStringAsync(new Uri(URL));

                BankObject.RootObject[] rootObject;
                rootObject = JsonConvert.DeserializeObject<BankObject.RootObject[]>(x);
                Boolean doIt = true;

                //Checks the user input and if it matches the database's user and pass, then it will record the ID for further use.
                foreach (BankObject.RootObject name in rootObject ){
                    if (userMessage == name.Username + " " + name.Password)
                    {
                        id = Convert.ToInt32(name.CustomerNo);
                        userData.SetProperty<int>("ID", id);
                        await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);

                        
                        Activity infoReply = activity.CreateReply($"Hello {name.FirstName}. What can we do for you today?");
                        await connector.Conversations.ReplyToActivityAsync(infoReply);
                        doIt = false;
                    }
                }

                if (userMessage.ToLower().Equals("hi") || userMessage.ToLower().Equals("hello") || userMessage.ToLower().Equals("sup"))
                {
                    Activity replyToConversation = activity.CreateReply("Contoso Banking Bot");
                    replyToConversation.Recipient = activity.From;
                    replyToConversation.Type = "message";
                    replyToConversation.Attachments = new List<Attachment>();

                    List<CardImage> cardImages = new List<CardImage>();
                    cardImages.Add(new CardImage(url: "https://s21.postimg.org/o1jioivnr/Untitled_1.png"));

                    List<CardAction> cardButtons = new List<CardAction>();

                    HeroCard plCard = new HeroCard()
                    {
                        Title = "Welcome to Contoso Bank!",
                        Subtitle = "We are please to see you using our service :)",
                        Images = cardImages

                    };

                    Attachment plAttachment = plCard.ToAttachment();
                    replyToConversation.Attachments.Add(plAttachment);
                    await connector.Conversations.SendToConversationAsync(replyToConversation);

                    return Request.CreateResponse(HttpStatusCode.OK);

                }

                if (userMessage.ToLower().Equals("show details"))
                {
                    Activity replyToConversation = activity.CreateReply("Showing Details");
                    replyToConversation.Recipient = activity.From;
                    replyToConversation.Type = "message";
                    replyToConversation.Attachments = new List<Attachment>();

                    List<CardAction> cardButtons = new List<CardAction>();

                    ReceiptItem lineItem1 = new ReceiptItem()
                    {
                        Text = $"First Name: {rootObject[id].FirstName} Last Name: {rootObject[id].LastName} \n\nAddress: {rootObject[id].Address} Number: {rootObject[id].ContactNo}",

                    };

                    List<ReceiptItem> receiptList = new List<ReceiptItem>();
                    receiptList.Add(lineItem1);

                    ReceiptCard plCard = new ReceiptCard()
                    {
                        Title = $"{rootObject[id].Username}'s Personal Bank Details",
                        Items = receiptList
                    };

                    Attachment plAttachment = plCard.ToAttachment();
                    replyToConversation.Attachments.Add(plAttachment);
                    await connector.Conversations.SendToConversationAsync(replyToConversation);

                    return Request.CreateResponse(HttpStatusCode.OK);

                }
                if (userMessage.ToLower().Contains("logout"))
                {
                    Activity infoReply = activity.CreateReply("Logging out, thank you for using Contoso Bank");
                    await connector.Conversations.ReplyToActivityAsync(infoReply);
                    await stateClient.BotState.DeleteStateForUserAsync(activity.ChannelId, activity.From.Id);
                    doIt = false;
                    id = -1;
                }

                //So that LUIS doesn't pick up on input that is meant for the state client
                if (doIt)
                {
                    await Conversation.SendAsync(activity, () => new BankDialog(rootObject, userData.GetProperty<int>("ID")));
                }
            }
            else
            {
                //add code to handle errors, or non-messaging activities
            }

            return new HttpResponseMessage(System.Net.HttpStatusCode.Accepted);
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
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