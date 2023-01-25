using RESTRunner.Postman.Models;

namespace RESTRunner.Domain.Tests.Models;
/// <summary>
/// 
/// </summary>
[TestClass]
public class CompareBodyTests
{
    /// <summary>
    /// 
    /// </summary>    
    [TestMethod]
    public void TestMethod_Expected()
    {
        Urlencoded encode = new("MyKey", "MyValue", "MyDescription", "MyType", false);
        // Arrange
        var compareBody = new CompareBody()
        {
            Language = "Language",
            Mode = "Mode",
            Properties = new List<CompareProperty>()
            {
                new CompareProperty(key: encode.Key, value: encode.Value, type: encode.Type, name: encode.Description, description: encode.Description)
            },
            Raw = string.Empty
        };
        // Act

        // Assert
        Assert.IsNotNull(compareBody);
    }
}
