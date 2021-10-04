using RESTRunner.Domain.Models;
using System.Collections.Generic;

namespace RESTRunner.Extensions
{
    public static class CompareRunner_Extensions
    {
        private static List<CompareInstance> GetCompareInstances()
        {
            return new List<CompareInstance>
            {
                new CompareInstance() {Name="TEXECON", BaseUrl= "https://mwhtexeconwebcore.azurewebsites.net/" },
            };
        }
        private static List<CompareRequest> GetCompareRequests()
        {
            return new List<CompareRequest>
            {
                new CompareRequest() {RequestMethod=HttpVerb.GET,Path="api/Domain" },
                new CompareRequest() {RequestMethod=HttpVerb.GET,Path="api/Menu" },
            };

            // JsonConvert.SerializeObject(new {})


        }
#pragma warning disable CRRSP06 // A misspelled word has been found
        private static List<CompareUser> GetCompareUsers()
        {
            var list = new List<CompareUser>();
            var user = new CompareUser()
            {
                UserName = "callieone",
                Password = "Model123"
            };
            user.Properties.Add("username", user.UserName);
            user.Properties.Add("user_patientId", "Z87");
            user.Properties.Add("user_patientIdType", "External");
            user.Properties.Add("user_myChartId", "callieone");
            user.Properties.Add("user_dob", "08-22-1988");
            user.Properties.Add("user_firstname", "Perry");
            user.Properties.Add("user_lastname", "Marie");
            user.Properties.Add("user_zipcode", "76504");
            user.Properties.Add("user_email", "techma20qa@gmail.com");
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
