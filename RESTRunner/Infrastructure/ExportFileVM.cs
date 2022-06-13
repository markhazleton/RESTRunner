namespace RESTRunner.Infrastructure
{

    /// <summary>
    /// Export File Name
    /// </summary>
    public class ExportFileVM
    {
        /// <summary>
        /// Export File Name
        /// </summary>
        public string ExportFileName = string.Empty;
        /// <summary>
        /// Content Type
        /// </summary>
        public string ContentType = string.Empty;
        /// <summary>
        /// Data to export
        /// </summary>
        public byte[]? Data;
    }
}
