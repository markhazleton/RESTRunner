namespace RESTRunner.Domain.Tests.Models;

[TestClass]
public class ComparePropertyTests
{
    [TestMethod]
    public void CompareProperty_Constructor_SetsAllProperties()
    {
        var prop = new CompareProperty("key1", "value1", "string", "Display Name", "A description");

        Assert.AreEqual("key1", prop.Key);
        Assert.AreEqual("value1", prop.Value);
        Assert.AreEqual("string", prop.Type);
        Assert.AreEqual("Display Name", prop.Name);
        Assert.AreEqual("A description", prop.Description);
    }

    [TestMethod]
    public void CompareProperty_Constructor_NullDescription_DefaultsToEmpty()
    {
        var prop = new CompareProperty("key1", "value1", "string", "Display Name");

        Assert.AreEqual(string.Empty, prop.Description);
    }

    [TestMethod]
    public void CompareProperty_Constructor_NullKey_ThrowsArgumentNullException()
    {
        try
        {
            _ = new CompareProperty(null!, "value", "string", "Name");
            Assert.Fail("Expected ArgumentNullException was not thrown");
        }
        catch (ArgumentNullException) { }
    }

    [TestMethod]
    public void CompareProperty_Constructor_NullValue_ThrowsArgumentNullException()
    {
        try
        {
            _ = new CompareProperty("key", null!, "string", "Name");
            Assert.Fail("Expected ArgumentNullException was not thrown");
        }
        catch (ArgumentNullException) { }
    }

    [TestMethod]
    public void CompareProperty_Constructor_NullType_ThrowsArgumentNullException()
    {
        try
        {
            _ = new CompareProperty("key", "value", null!, "Name");
            Assert.Fail("Expected ArgumentNullException was not thrown");
        }
        catch (ArgumentNullException) { }
    }

    [TestMethod]
    public void CompareProperty_Constructor_NullName_ThrowsArgumentNullException()
    {
        try
        {
            _ = new CompareProperty("key", "value", "string", null!);
            Assert.Fail("Expected ArgumentNullException was not thrown");
        }
        catch (ArgumentNullException) { }
    }
}
