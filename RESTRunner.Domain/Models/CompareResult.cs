namespace RESTRunner.Domain.Models
{
    /// <summary>
    /// CompareResult
    /// </summary>
    public record CompareResult
    {
        public string SessionId { get; set; }
        public string UserName { get; set; }
        public string Verb { get; set; }
        public string Instance { get; set; }
        public bool Success { get; set; }
        public string ResultCode { get; set; }
        public long Duration { get; set; }
        public string LastRunDate { get; set; }
        public int Hash { get; set; }
        public string Request { get; set; }
        public string StatusDescription { get; set; }
        public string Content { get; set; }
    }
}
