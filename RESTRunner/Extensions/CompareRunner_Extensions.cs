﻿using RESTRunner.Domain.Models;
using System.Collections.Generic;

namespace RESTRunner.Extensions
{
    public static class CompareRunner_Extensions
    {
        private static List<CompareInstance> GetCompareInstances()
        {
            return new List<CompareInstance>
            {
                new CompareInstance() {Name="AZ", BaseUrl="https://mwhemployeemvccrud.azurewebsites.net/"},
                new CompareInstance() {Name="CO", BaseUrl="https://employeemvccrud.controlorigins.com/"}
            };
        }
        private static List<CompareRequest> GetCompareRequests()
        {
            return new List<CompareRequest>
            {
                new CompareRequest() { Path="status", RequestMethod = HttpVerb.GET, RequiresClientToken=false }
            };
        }
#pragma warning disable CRRSP06 // A misspelled word has been found
        private static List<CompareUser> GetCompareUsers()
        {
            var list = new List<CompareUser>();
            var user = new CompareUser()
            {
                UserName = "markhazleton",
                Password = "password"
            };
            user.Properties.Add("username", user.UserName);
            user.Properties.Add("user_firstname", "Mark");
            user.Properties.Add("user_lastname", "Hazleton");
            user.Properties.Add("user_zipcode", "76262");
            user.Properties.Add("user_email", "mark.hazleton@gmail.com");
            user.Properties.Add("today", "2021-10-01");
            user.Properties.Add("today5", "2021-10-05");
            user.Properties.Add("today15", "2021-10-15");
            list.Add(user);
            return list;
        }
#pragma warning restore CRRSP06 // A misspelled word has been found


        public static void InitializeCompareRunner(this CompareRunner runner)
        {
            runner.Requests = GetCompareRequests();
            runner.Instances = GetCompareInstances();
            runner.Users = GetCompareUsers();
        }
    }
}