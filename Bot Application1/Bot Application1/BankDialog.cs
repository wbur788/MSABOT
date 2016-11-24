
namespace Bot_Application1
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Luis;
    using Microsoft.Bot.Builder.Luis.Models;
    using Models;
    using System.Net.Http;
    using System.Collections;
    using System.Diagnostics;
    using System.Text;
    using System.Web.Script.Serialization;
    using Microsoft.Bot.Connector;
    using System.Collections.Generic;

    public static class HttpClientExtensions
    {
        public static async Task<HttpResponseMessage> PatchAsync(this HttpClient client, Uri requestUri, HttpContent iContent)
        {
            var method = new HttpMethod("PATCH");
            var request = new HttpRequestMessage(method, requestUri)
            {
                Content = iContent
            };

            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                response = await client.SendAsync(request);
            }
            catch (TaskCanceledException e)
            {
                Debug.WriteLine("ERROR: " + e.ToString());
            }

            return response;
        }
    }

    [LuisModel("755a4c3b-9280-40b8-9b80-c71ff2c82514", "a2fcb42f381949c687817af4370dbecc")]
    [Serializable]
    public class BankDialog : LuisDialog<Object>, IDialog<object>
    {
        HttpClient client = new HttpClient();
        string URL = "http://shrek.azurewebsites.net/tables/BankData?zumo-api-version=2.0.0";

        BankObject.RootObject[] rootObject;
        int id=-1;
        string detail;

        public BankDialog(BankObject.RootObject[] rootObject, int id)
        {
            this.rootObject = rootObject;
            this.id = id;
            
        }

        //WHERE INTENT = NONE 
        [LuisIntent("")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            string message = $"Sorry I did not understand: "
                + string.Join(", ", result.Intents.Select(i => i.Intent));
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        

        //WHERE INTENT = CONVERT CURRENCY
        string CCTemp;
        ArrayList entListCC;
        [LuisIntent("ConvertCurrency")]
        public async Task ConvertCurrency(IDialogContext context, LuisResult result)
        {
            foreach (EntityRecommendation s in result.Entities)
            {
                CCTemp = s.Entity;
                entListCC.Add(temp);
            }
            string x = await client.GetStringAsync(new Uri("http://api.fixer.io/latest?base=" + entList[1]));



        }



        //WHERE INTENT = DISPLAYDETAILS
        public const string Entity_PersonalDetails = "PersonalDetails";
        [LuisIntent("DisplayDetails")]
        public async Task DisplayDetails(IDialogContext context, LuisResult result)
        { 

            EntityRecommendation personalDetail;
            //If no entity is found, it will display all details, otherwise just the specific detail
            if (!result.TryFindEntity(Entity_PersonalDetails, out personalDetail))
            {
                /*String entity = personalDetail.Entity.ToLower();
                await context.PostAsync("Showing all personal details.");
                await context.PostAsync(rootObject[id]+"");
                context.Wait(MessageReceived);*/
            }
            else
            {
                String entity = personalDetail.Entity.ToLower();
                

                //determines what entity is required and stores in a variable
                switch (entity)
                {
                    case "address":
                        detail = rootObject[id].Address;
                        break;
                    case "number":
                        detail = rootObject[id].ContactNo;
                        break;
                    case "phone":
                        detail = rootObject[id].ContactNo;
                        break;
                    case "contact":
                        detail = rootObject[id].ContactNo;
                        break;
                    case "account":
                        detail = rootObject[id].AccountNo;
                        break;
                    case "accounts":
                        detail = rootObject[id].AccountNo;
                        break;
                    case "username":
                        detail = rootObject[id].Username;
                        break;
                    case "password":
                        detail = rootObject[id].Password;
                        break;
                }

                //Return the entity details
                if (id != -1 )
                {
                    await context.PostAsync($"Your {entity} is " + detail);
                    context.Wait(MessageReceived);
                }
                else
                {
                    await context.PostAsync("No address found. Please log in.");
                    context.Wait(MessageReceived);
                }


            }
        }

        //WHERE INTENT = TRANSFER
        public const string Entity_AccountType = "AccountType";
        int a, b;
        ArrayList entList = new ArrayList();
        string temp, numString;
        Double amountToTransfer;
        [LuisIntent("Transfer")]
        public async Task Transfer(IDialogContext context, LuisResult result)
        {
              
           ArrayList accountList = getAccounts();
            if (accountList.Count > 3)
            {
                //Taking each entity found in the user message and stores in an ArrayList
                foreach (EntityRecommendation s in result.Entities)
                {
                    temp = s.Entity;
                    entList.Add(temp);
                }

                a = accountList.IndexOf(entList[0]);
                b = accountList.IndexOf(entList[1]);

                //LUIS takes numbers with decimal points and puts a space in between the numbers Eg. $12.24 becomes $12 . 24
                //This takes that value and removes the white space
                if (entList[2].ToString().Contains("."))
                {
                    string[] tempList = entList[2].ToString().Split('.');
                    numString += tempList[0].Trim();
                    numString += '.';
                    numString += tempList[1].Trim();                   
                    amountToTransfer = Convert.ToDouble(numString);
                }
                else
                {
                    amountToTransfer = Convert.ToDouble(entList[2]);

                }
                int num = accountList.Count / 3;
                Double balanceA = Convert.ToDouble(accountList[a + num]);
                Double balanceB = Convert.ToDouble(accountList[b + num]);

                if (balanceA - amountToTransfer > 0)
                {
                    balanceA -= amountToTransfer;
                    balanceB += amountToTransfer;

                    await context.PostAsync($"Transferring ${amountToTransfer} from {accountList[a]} (${accountList[a + num]}) to {accountList[b]} (${accountList[b + num]}).");
                    await context.PostAsync($"Your {accountList[b]} account now has ${balanceB} and your {accountList[a]} account now has ${balanceA}.");

                    accountList[a + num] = balanceA;
                    accountList[b + num] = balanceB;

                    //Creating a string for the updated account balances to send back to the database
                    String accountBalanceStr = "{";
                    for (int i = num+2; i < accountList.Count; i++)
                    {
                        accountBalanceStr += accountList[i] + ",";
                    }
                    accountBalanceStr = accountBalanceStr.Trim(',') + "}";

                    rootObject[id].AccountBalance = accountBalanceStr;
                   

                    var json = new JavaScriptSerializer().Serialize(rootObject[id]);
                    HttpContent httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                    var responseMessage = await client.PatchAsync(new Uri(URL), httpContent);

                    
                }
                else
                {
                    await context.PostAsync("You do not have enough money to transfer, please try again.");
                }
                
                context.Wait(MessageReceived);
            }
            else
            {
                await context.PostAsync("Sorry you do not have another account to transfer to.");
                context.Wait(MessageReceived);
            }

            

        }

        //Getting all the account info into one arraylist
        public ArrayList getAccounts()
        {
            ArrayList al = new ArrayList();
            

            detail = rootObject[id].AccountNo;
            detail = detail.Trim(new char[] { '{', '}' });
            String[] accountNoList = detail.Split(',');
            string x;

            foreach(String s in accountNoList)
            {
                x = s.Replace(" ", String.Empty);
                al.Add(x);
            }

            detail = rootObject[id].AccountType;
            detail = detail.Trim(new char[] { '{', '}'});
            String[] accountTypeList = detail.Split(',');

            foreach (String s in accountTypeList)
            {
                x = s.Replace(" ", String.Empty);
                x = x.ToLower();
                al.Add(x);
            }

            detail = rootObject[id].AccountBalance;
            detail = detail.Trim(new char[] { '{', '}' });
            String[] accountBalanceList = detail.Split(',');

            foreach (String s in accountBalanceList)
            {
                x = s.Replace(" ", String.Empty);
                al.Add(x);
            }

            return al;
        }
    }
}