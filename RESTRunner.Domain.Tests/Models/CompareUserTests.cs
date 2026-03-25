namespace RESTRunner.Domain.Tests.Models;

[TestClass]
public class CompareUserTests
{
    [TestMethod]
    public void CompareUser_DefaultConstructor_HasEmptyProperties()
    {
        var user = new CompareUser();

        Assert.IsNotNull(user.Properties);
        Assert.AreEqual(0, user.Properties.Count);
        Assert.IsNull(user.UserName);
        Assert.IsNull(user.Password);
    }

    [TestMethod]
    public void CompareUser_SetUserName_StoresValue()
    {
        var user = new CompareUser { UserName = "testuser" };

        Assert.AreEqual("testuser", user.UserName);
    }

    [TestMethod]
    public void CompareUser_SetPassword_StoresValue()
    {
        var user = new CompareUser { Password = "changeme" };

        Assert.AreEqual("changeme", user.Password);
    }

    [TestMethod]
    public void CompareUser_Properties_CanAddKeyValuePairs()
    {
        var user = new CompareUser();
        user.Properties.Add("email", "user@example.com");
        user.Properties.Add("role", "tester");

        Assert.AreEqual(2, user.Properties.Count);
        Assert.AreEqual("user@example.com", user.Properties["email"]);
        Assert.AreEqual("tester", user.Properties["role"]);
    }
}
