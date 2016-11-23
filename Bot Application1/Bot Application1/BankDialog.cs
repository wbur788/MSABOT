
namespace Bot_Application1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.Http;
    using System.Web.Http.Description;
    using Microsoft.Bot.Connector;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Luis;
    using Microsoft.Bot.Builder.Luis.Models;
    using Newtonsoft.Json;
    using Bot_Application1.Models;

    [LuisModel("755a4c3b-9280-40b8-9b80-c71ff2c82514", "a2fcb42f381949c687817af4370dbecc")]
    [Serializable]
    public class BankDialog : LuisDialog<Object>
    {
        BankObject.RootObject[] rootObject;

        public BankDialog(BankObject.RootObject[] rootObject)
        {
            this.rootObject = rootObject;
        }
         
        [LuisIntent("")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            string message = $"Sorry I did not understand: "
                + string.Join(", ", result.Intents.Select(i => i.Intent));
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        public const string Entity_PersonalDetails = "PersonalDetails";
        [LuisIntent("DisplayDetails")]
        public async Task DisplayDetails(IDialogContext context, LuisResult result)
        {
            
            EntityRecommendation personalDetail;
            //If entity "PersonalDetails" cannot be found, also stores the entity in 'out personalDetail'
            if (!result.TryFindEntity(Entity_PersonalDetails, out personalDetail))
            {
                await context.PostAsync("Showing all personal details.");
                context.Wait(MessageReceived);
            }
            else
            {
                //If asked "What is my address", it will print out 'Your address is '
                await context.PostAsync($"Your {personalDetail.Entity} is " + rootObject[2].Address); 
                context.Wait(MessageReceived);
            }
        }


    }
}