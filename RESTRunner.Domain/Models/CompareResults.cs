using FileHelpers;
using System;
using System.Collections.Generic;

namespace RESTRunner.Domain.Models
{

    [DelimitedRecord(",")]
    public class CompareResults
    {
        public string Verb;
        public string Instance;
        public bool Success;
        public string ResultCode;
        public long Duration;
        public DateTime LastRunDate;
        public int Hash;
        public string Request;
        public string StatusDescription;
        public string Content;

        public override string ToString()
        {
            return $"{Success}: {Instance}:  {Verb}: {Request}:  {Hash}";
        }
    }
}
