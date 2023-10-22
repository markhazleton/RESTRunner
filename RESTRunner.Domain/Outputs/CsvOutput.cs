namespace RESTRunner.Domain.Outputs;

public class CsvOutput : IOutput, IDisposable
{
    private readonly TextWriter _writer;

    public CsvOutput(string filePath)
    {
        var fileMode = File.Exists(filePath) ? FileMode.Append : FileMode.Create;
        var fileStream = new FileStream(filePath, fileMode, FileAccess.Write);
        var streamWriter = new StreamWriter(fileStream, Encoding.UTF8) { AutoFlush = true };
        _writer = TextWriter.Synchronized(streamWriter);

        if (fileMode == FileMode.Create)
        {
            _writer.WriteLine(GetItemCSVHeader());
        }
    }

    private static string GetItemCSVHeader()
    {
        var sb = new StringBuilder();
        SetPropertyCSVColumn(sb, "Verb", true, false);
        SetPropertyCSVColumn(sb, "Instance", false, false);
        SetPropertyCSVColumn(sb, "LastRunDate", false, false);
        SetPropertyCSVColumn(sb, "Duration", false, false);
        SetPropertyCSVColumn(sb, "Request", false, false);
        SetPropertyCSVColumn(sb, "ResultCode", false, false);
        SetPropertyCSVColumn(sb, "SessionId", false, false);
        SetPropertyCSVColumn(sb, "StatusDescription", false, false);
        SetPropertyCSVColumn(sb, "Success", false, false);
        SetPropertyCSVColumn(sb, "UserName", false, false);
        SetPropertyCSVColumn(sb, "Content", false, true);
        return sb.ToString();
    }

    private static string GetItemCSVRow(CompareResult item)
    {
        var sb = new StringBuilder();
        SetPropertyCSVColumn(sb, item.Verb, true, false);
        SetPropertyCSVColumn(sb, item.Instance, false, false);
        SetPropertyCSVColumn(sb, item.LastRunDate.ToString("MM/dd/yyyy HH:mm:ss"), false, false);
        SetPropertyCSVColumn(sb, item.Duration.ToString(), false, false);
        SetPropertyCSVColumn(sb, item.Request, false, false);
        SetPropertyCSVColumn(sb, item.ResultCode, false, false);
        SetPropertyCSVColumn(sb, item.SessionId, false, false);
        SetPropertyCSVColumn(sb, item.StatusDescription, false, false);
        SetPropertyCSVColumn(sb, item.Success.ToString(), false, false);
        SetPropertyCSVColumn(sb, item.UserName, false, false);
        return sb.ToString();
    }

    private static void SetPropertyCSVColumn(StringBuilder sb, string? value, bool first = false, bool last = false)
    {
        if (value == null)
        {
            if (!first)
                sb.Append(',');

            sb.Append(String.Empty);

            if (last)
                sb.Append(string.Empty);
        }
        else if (!value.Contains('\"'))
        {
            if (!first)
                sb.Append(',');

            sb.Append(value);

            if (last)
                sb.Append(string.Empty);
        }
        else
        {
            if (!first)
                sb.Append(",\"");
            else
                sb.Append('\"');

            sb.Append(value.Replace("\"", "\"\""));

            if (last)
                sb.Append('"');
            else
                sb.Append('\"');
        }
    }

    private void Write(CompareResult result)
    {
        try
        {
            _writer?.WriteLine(GetItemCSVRow(result));
        }
        catch (Exception ex)
        {
            _writer?.WriteLine($"Exception-{ex.Message}");
        }
    }
    /// <inheritdoc/>

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _writer?.Close();
            _writer?.Dispose();
        }
    }

    public void WriteError(CompareResult result)
    {
        Write(result);
    }

    public void WriteInfo(CompareResult result)
    {
        Write(result);
    }

    public void WriteInfo(string[] Info)
    {
        // Do nothing - string info is only for console
    }
}