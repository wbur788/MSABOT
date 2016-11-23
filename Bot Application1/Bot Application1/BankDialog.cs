namespace Bot_Application1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Luis;
    using Microsoft.Bot.Builder.Luis.Models;

    [LuisModel("755a4c3b-9280-40b8-9b80-c71ff2c82514", "a2fcb42f381949c687817af4370dbecc")]
    [Serializable]
    public class BankDialog : LuisDialog<Object>
    {
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
            //If entity cannot be found
            if (!result.TryFindEntity(Entity_PersonalDetails, out personalDetail))
            {
                await context.PostAsync("Showing all personal details.");
                context.Wait(MessageReceived);
            }
            else
            {
                //If asked "What is my address", it will print out 'Your address is '
                await context.PostAsync($"Your {personalDetail.Entity} is "); 
                context.Wait(MessageReceived);
            }
        }


    }
}