namespace RESTRunner.Domain.Tests.Models;

[TestClass]
public class CompareInstanceTests
{
    [TestMethod]
    public void ToString_WithNameAndUrl_ReturnsFormattedString()
    {
        var instance = new CompareInstance
        {
            BaseUrl = "https://www.controlorigins.com",
            Name = "Production"
        };

        var result = instance.ToString();

        Assert.AreEqual("Production:https://www.controlorigins.com", result);
    }

    [TestMethod]
    public void IsValid_WithNameAndBaseUrl_ReturnsTrue()
    {
        var instance = new CompareInstance
        {
            Name = "Local",
            BaseUrl = "https://localhost:7001/"
        };

        Assert.IsTrue(instance.IsValid());
    }

    [TestMethod]
    public void IsValid_MissingName_ReturnsFalse()
    {
        var instance = new CompareInstance { BaseUrl = "https://localhost:7001/" };

        Assert.IsFalse(instance.IsValid());
    }

    [TestMethod]
    public void IsValid_MissingBaseUrl_ReturnsFalse()
    {
        var instance = new CompareInstance { Name = "Local" };

        Assert.IsFalse(instance.IsValid());
    }

    [TestMethod]
    public void IsValid_BothMissing_ReturnsFalse()
    {
        var instance = new CompareInstance();

        Assert.IsFalse(instance.IsValid());
    }

    [TestMethod]
    public void BaseUrl_InvalidUrl_ThrowsArgumentException()
    {
        try
        {
            _ = new CompareInstance { BaseUrl = "not-a-valid-url" };
            Assert.Fail("Expected ArgumentException was not thrown");
        }
        catch (ArgumentException) { }
    }

    [TestMethod]
    public void BaseUrl_ValidAbsoluteUrl_Stores()
    {
        var instance = new CompareInstance { BaseUrl = "https://example.com/" };

        Assert.AreEqual("https://example.com/", instance.BaseUrl);
    }

    [TestMethod]
    public void Name_WhitespaceOnly_StoresNull()
    {
        var instance = new CompareInstance { Name = "   " };

        Assert.IsNull(instance.Name);
    }

    [TestMethod]
    public void Name_WithWhitespace_IsTrimmed()
    {
        var instance = new CompareInstance { Name = "  Demo  " };

        Assert.AreEqual("Demo", instance.Name);
    }
}
