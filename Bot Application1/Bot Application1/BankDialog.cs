﻿
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
    using System.Net.Http;
    using System.Collections;

    [LuisModel("755a4c3b-9280-40b8-9b80-c71ff2c82514", "a2fcb42f381949c687817af4370dbecc")]
    [Serializable]
    public class BankDialog : LuisDialog<Object>, IDialog<object>
    {

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

        //WHERE INTENT = GREETING
        [LuisIntent("Greeting")]
        public async Task Greeting(IDialogContext context, LuisResult result)
        {
            string message = "Hello. Welcome to Contoso Bank, are you an existing member or new to Contoso Banking Bot?";
            await context.PostAsync(message);
            context.Wait(MessageReceived);
           
        }

        

        //WHERE INTENT = LOGIN
        [LuisIntent("Login")]
        public async Task Login(IDialogContext context, LuisResult result)
        {
            string message = "Please enter your username and password (separated with a space)";
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        //WHERE INTENT = LOGIN
        [LuisIntent("Signup")]
        public async Task Signup(IDialogContext context, LuisResult result)
        {
            string message = "Please enter your username and password (separated with a space)";
            await context.PostAsync(message);
            context.Wait(MessageReceived);
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
                String entity = personalDetail.Entity.ToLower();
                await context.PostAsync("Showing all personal details.");
                await context.PostAsync(rootObject[id]+"");
                context.Wait(MessageReceived);
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
        EntityRecommendation accountType1, accountType2;
        int a, b;
        ArrayList entList = new ArrayList();
        string temp;
        [LuisIntent("Transfer")]
        public async Task Transfer(IDialogContext context, LuisResult result)
        {
            
              
            ArrayList accountList = getAccounts();
            
           foreach (EntityRecommendation s in result.Entities)
            {
                temp = s.Entity;
                entList.Add(temp);
            }

            a = accountList.IndexOf(entList[0]);
            b = accountList.IndexOf(entList[1]);
            Double amountToTransfer = Convert.ToDouble(entList[2]);
            int num = accountList.Count / 3;
            Double balanceA = Convert.ToDouble(accountList[a + num]);
            Double balanceB = Convert.ToDouble(accountList[b + num]);

            balanceA -= amountToTransfer;
            balanceB += amountToTransfer;

            String endOutput = "";                
            foreach (String s in accountList)
            {
                endOutput += s + " ";
            }

            await context.PostAsync($"Transferring ${amountToTransfer} from {accountList[a]} (${accountList[a+num]}) to {accountList[b]} (${accountList[b+num]}).");
            await context.PostAsync($"Your {accountList[b]} account now has ${balanceB} and your {accountList[a]} account now has ${balanceA}.");
            context.Wait(MessageReceived);
           


            
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