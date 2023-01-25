namespace RESTRunner.Domain.Tests.Extensions;

/// <summary>
/// CompareUser_ExtensionsTests
/// </summary>
[TestClass]
public class CompareUser_ExtensionsTests
{
    /// <summary>
    /// GetMergedString_StateUnderTest_ExpectedBehavior
    /// </summary>
    [TestMethod]
    public void GetMergedString_StateUnderTest_ExpectedBehavior()
    {
        // Arrange
        var compareUser = new CompareUser
        {
            UserName = "TestUserName",
            Password = "TestPassword",
            Properties = new Dictionary<string, string>
            {
                { "TestKey", "TestValue" }
            }
        };

        string path = "/api/test/{{UserName}}?{{TestKey}}";

        // Act
        var result = compareUser.GetMergedString(path);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("/api/test/TestUserName?TestValue", result);
    }

    /// <summary>
    /// GetMergedString_StateUnderTest_NullString
    /// </summary>
    [TestMethod]
    public void GetMergedString_StateUnderTest_NullString()
    {
        // Arrange
        var compareUser = new CompareUser
        {
            UserName = "TestUserName",
            Password = "TestPassword",
            Properties = new Dictionary<string, string>
            {
                { "TestKey", "TestValue" }
            }
        };
        string? path = null;
        // Act
        var result = compareUser.GetMergedString(path);
        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(string.Empty, result);
    }
}
