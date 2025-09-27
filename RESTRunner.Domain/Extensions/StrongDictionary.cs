using System.Text.Json;

namespace RESTRunner.Domain.Extensions;

/// <summary>
/// Special Dictionary for Use with Restful / AJAX Calls
/// Provides null-safe operations and JSON serialization capabilities
/// </summary>
/// <typeparam name="TKey">The type of the key - must be non-null.</typeparam>
/// <typeparam name="TValue">The type of the value.</typeparam>
public sealed class StrongDictionary<TKey, TValue> : IDisposable where TKey : notnull
{
    /// <summary>
    /// The dictionary
    /// </summary>
    private readonly Dictionary<TKey, TValue> _dictionary;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="StrongDictionary{TKey, TValue}"/> class.
    /// </summary>
    public StrongDictionary() 
    { 
        _dictionary = new Dictionary<TKey, TValue>(); 
    }

    /// <summary>
    /// Initializes a new instance with an initial capacity
    /// </summary>
    /// <param name="capacity">Initial capacity of the dictionary</param>
    public StrongDictionary(int capacity) 
    { 
        _dictionary = new Dictionary<TKey, TValue>(capacity); 
    }

    /// <summary>
    /// Gets the number of key-value pairs in the dictionary
    /// </summary>
    public int Count => _dictionary.Count;

    /// <summary>
    /// Gets the keys in the dictionary
    /// </summary>
    public IEnumerable<TKey> Keys => _dictionary.Keys;

    /// <summary>
    /// Gets or sets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>The value associated with the key, or default if not found.</returns>
    public TValue? this[TKey key]
    {
        get
        {
            ArgumentNullException.ThrowIfNull(key);
            ThrowIfDisposed();
            return _dictionary.TryGetValue(key, out TValue? value) ? value : default;
        }
        set
        {
            ArgumentNullException.ThrowIfNull(key);
            ThrowIfDisposed();
            
            if (value is null)
            {
                _dictionary.Remove(key);
                return;
            }
            
            _dictionary[key] = value;
        }
    }

    /// <summary>
    /// Adds the specified dictionary into the current dictionary
    /// </summary>
    /// <param name="value">The dictionary to add.</param>
    public void Add(Dictionary<TKey, TValue> value)
    {
        ArgumentNullException.ThrowIfNull(value);
        ThrowIfDisposed();
        
        foreach (var (key, val) in value)
        {
            Add(key, val);
        }
    }

    /// <summary>
    /// Adds the specified key and value to the dictionary.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    public void Add(TKey key, TValue value)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(value);
        ThrowIfDisposed();
        
        _dictionary[key] = value;
    }

    /// <summary>
    /// Tries to get the value associated with the specified key
    /// </summary>
    /// <param name="key">The key to search for</param>
    /// <param name="value">The value if found</param>
    /// <returns>True if the key was found, false otherwise</returns>
    public bool TryGetValue(TKey key, out TValue? value)
    {
        ArgumentNullException.ThrowIfNull(key);
        ThrowIfDisposed();
        return _dictionary.TryGetValue(key, out value);
    }

    /// <summary>
    /// Removes the specified key from the dictionary
    /// </summary>
    /// <param name="key">The key to remove</param>
    /// <returns>True if the key was removed, false if it wasn't found</returns>
    public bool Remove(TKey key)
    {
        ArgumentNullException.ThrowIfNull(key);
        ThrowIfDisposed();
        return _dictionary.Remove(key);
    }

    /// <summary>
    /// Clears all items from the dictionary
    /// </summary>
    public void Clear()
    {
        ThrowIfDisposed();
        _dictionary.Clear();
    }

    /// <summary>
    /// Gets the list of key-value pairs as formatted strings.
    /// </summary>
    /// <returns>List of formatted key-value pair strings.</returns>
    public List<string> GetList()
    {
        ThrowIfDisposed();
        return _dictionary.Select(item => $"{item.Key} - {item.Value}").ToList();
    }

    /// <summary>
    /// Get Json string representation of the dictionary using System.Text.Json
    /// </summary>
    /// <returns>JSON string representation of the dictionary.</returns>
    public string GetJson()
    {
        ThrowIfDisposed();
        try
        {
            return JsonSerializer.Serialize(_dictionary, new JsonSerializerOptions 
            { 
                WriteIndented = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to serialize dictionary to JSON", ex);
        }
    }

    /// <summary>
    /// Gets the object data for serialization.
    /// </summary>
    /// <param name="info">The serialization information.</param>
    public void GetObjectData(SerializationInfo info)
    {
        ArgumentNullException.ThrowIfNull(info);
        ThrowIfDisposed();
        
        foreach (TKey key in _dictionary.Keys)
        {
            var keyString = key.ToString() ?? "unknown";
            info.AddValue(keyString, _dictionary[key]);
        }
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    /// <summary>
    /// Disposes the dictionary
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _dictionary.Clear();
            _disposed = true;
        }
    }
}
