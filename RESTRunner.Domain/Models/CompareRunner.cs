using RESTRunner.Domain.Interfaces;
using System;
using System.Collections.Generic;

namespace RESTRunner.Domain.Models
{
    public class CompareRunner
    {
        public List<CompareInstance> Instances;
        public DateTime LastRunTime;
        public List<CompareRequest> Requests;
        public string SessionId;
        public List<CompareUser> Users;
        public IStoreResults StoreResults;

        public CompareRunner(IStoreResults results)
        {
            Instances = new List<CompareInstance>();
            Requests = new List<CompareRequest>();
            Users = new List<CompareUser>();
            SessionId = $"RESTRunner-{DateTime.Now.ToShortDateString()}";
            LastRunTime = DateTime.Now;
            StoreResults = results;
        }
    }
}
