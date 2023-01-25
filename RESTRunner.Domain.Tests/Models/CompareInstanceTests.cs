namespace RESTRunner.Domain.Tests.Models;

[TestClass]
public class CompareInstanceTests
{
    [TestMethod]
    public void ToString_StateUnderTest_ExpectedBehavior()
    {
        // Arrange
        var compareInstance = new CompareInstance()
        {
            BaseUrl = "https://www.controlorigins.com",
            SessionId = "SessionId",
            ClientToken = "ClientToken",
            Name = "Name",
            UserToken = "UserToken"
        };

        // Act
        var result = compareInstance.ToString();

        // Assert
        Assert.IsNotNull(result);
    }
}
