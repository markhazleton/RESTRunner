
namespace RESTRunner.Domain.Extensions;

/// <summary>
/// 
/// </summary>
public static class CompareUser_Extensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="user"></param>
    /// <param name="StringToMerge"></param>
    /// <returns></returns>
    public static string GetMergedString(this CompareUser user, string? StringToMerge)
    {
        if (!string.IsNullOrEmpty(StringToMerge))
        {
            StringToMerge = StringToMerge.Trim();
            StringToMerge = StringToMerge.Replace(@"{{encoded_user_name}}", user.UserName);
            StringToMerge = StringToMerge.Replace(@"{{username}}", user.UserName);
            StringToMerge = StringToMerge.Replace(@"{{UserName}}", user.UserName);
            StringToMerge = StringToMerge.Replace(@"{{password}}", user.Password);
            foreach (var prop in user.Properties)
            {
                StringToMerge = StringToMerge.Replace($"{{{{{prop.Key}}}", prop.Value);
            }
        }
        return StringToMerge ?? String.Empty;
    }
}
