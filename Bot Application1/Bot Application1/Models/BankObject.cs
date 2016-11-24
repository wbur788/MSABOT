using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bot_Application1.Models
{
    public class BankObject
    {
        public class RootObject
        {
            public string id { get; set; }
            public string createdAt { get; set; }
            public string updatedAt { get; set; }
            public string version { get; set; }
            public string CustomerNo { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string DOB { get; set; }
            public string ContactNo { get; set; }
            public string Address { get; set; }
            public string AccountNo { get; set; }
            public string AccountName { get; set; }
            public string AccountType { get; set; }
            public string AccountBalance { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public bool deleted { get; set; }
        }
    }
}