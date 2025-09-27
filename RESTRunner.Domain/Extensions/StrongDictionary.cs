namespace RESTRunner.Domain.Extensions;

/// <summary>
/// Special Dictionary for Use with Restful / AJAX Calls
/// </summary>
/// <typeparam name="TKey">The type of the key - must be non-null.</typeparam>
/// <typeparam name="TValue">The type of the value.</typeparam>
public sealed class StrongDictionary<TKey, TValue> where TKey : notnull
{
    /// <summary>
    /// The dictionary
    /// </summary>
    private readonly Dictionary<TKey, TValue> _Dictionary;

    /// <summary>
    /// Initializes a new instance of the <see cref="StrongDictionary{TKey, TValue}"/> class.
    /// </summary>
    public StrongDictionary() { _Dictionary = new Dictionary<TKey, TValue>(); }

    /// <summary>
    /// Gets or sets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>The value associated with the key, or default if not found.</returns>
    public TValue? this[TKey key]
    {
        get
        {
            _Dictionary.TryGetValue(key, out TValue? vOut);
            return vOut;
        }
        set
        {
            _Dictionary.TryGetValue(key, out TValue? vOut);
            if (vOut is null)
            {
                if (value is null) return;
                _Dictionary.Add(key, value);
            }
            else
            {
                if (value is null) return;
                _Dictionary[key] = value;
            }
        }
    }

    /// <summary>
    /// Adds the specified dictionary into the current dictionary
    /// </summary>
    /// <param name="value">The dictionary to add.</param>
    public void Add(Dictionary<TKey, TValue> value)
    {
        foreach (var item in value.Keys)
        {
            Add(item, value[item]);
        }
    }

    /// <summary>
    /// Adds the specified key and value to the dictionary.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    public void Add(TKey key, TValue value)
    {
        if (_Dictionary.ContainsKey(key))
            _Dictionary[key] = value;
        else
            _Dictionary.Add(key, value);
    }

    /// <summary>
    /// Gets the list of key-value pairs as formatted strings.
    /// </summary>
    /// <returns>List of formatted key-value pair strings.</returns>
    public List<string> GetList()
    {
        List<string> list = new();
        foreach (var item in _Dictionary)
        {
            list.Add($"{item.Key} - {item.Value}");
        }
        return list;
    }

    /// <summary>
    /// Get Json string representation of the dictionary
    /// </summary>
    /// <returns>JSON string representation of the dictionary.</returns>
    public string GetJson()
    {
        string str_json = string.Empty;
        DataContractJsonSerializerSettings setting =
            new()
            {
                UseSimpleDictionaryFormat = true
            };

        DataContractJsonSerializer js =
            new(typeof(Dictionary<TKey, TValue>), setting);

        using (MemoryStream ms = new())
        {
            // Serializer the object to the stream.  
            js.WriteObject(ms, _Dictionary);
            str_json = Encoding.Default.GetString(ms.ToArray());
        }
        return str_json;
    }

    /// <summary>
    /// Gets the object data for serialization.
    /// </summary>
    /// <param name="info">The serialization information.</param>
    public void GetObjectData(SerializationInfo info)
    {
        foreach (TKey key in _Dictionary.Keys)
        {
            // Since TKey is constrained to notnull, we don't need the null check
            var theKey = key.ToString() ?? "unknown";
            info.AddValue(theKey, _Dictionary[key]);
        }
    }
}
