
namespace RESTRunner.Domain.Extensions;

/// <summary>
/// String_Extensions
/// </summary>
public static class String_Extensions
{
    /// <summary>
    /// CaseInsensitiveContains
    /// </summary>
    /// <param name="text"></param>
    /// <param name="value"></param>
    /// <param name="stringComparison"></param>
    /// <returns></returns>
    public static bool CaseInsensitiveContains(this string text, string value, StringComparison stringComparison = StringComparison.CurrentCultureIgnoreCase)
    {
        return text.Contains(value, stringComparison);
    }

    /// <summary>
    /// GetDeterministicHashCode
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static int GetDeterministicHashCode(this string? str)
    {
        if (str == null) return -1;
        unchecked
        {
            var hash1 = (5381 << 16) + 5381;
            var hash2 = hash1;
            for (var i = 0; i < str.Length; i += 2)
            {
                hash1 = ((hash1 << 5) + hash1) ^ str[i];
                if (i == str.Length - 1)
                {
                    break;
                }
                hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
            }
            return hash1 + (hash2 * 1566083941);
        }
    }
}
