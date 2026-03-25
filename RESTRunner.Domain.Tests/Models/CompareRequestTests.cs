
namespace RESTRunner.Domain.Tests.Models;

[TestClass]
public class CompareRequestTests
{
    [TestMethod]
    public void CompareRequest_DefaultConstructor_CreatesInstance()
    {
        var request = new CompareRequest();

        Assert.IsNotNull(request);
        Assert.IsNull(request.Path);
        Assert.AreEqual(HttpVerb.GET, request.RequestMethod);
        Assert.IsFalse(request.RequiresClientToken);
    }

    [TestMethod]
    public void CompareRequest_Path_TrimsWhitespace()
    {
        var request = new CompareRequest { Path = "  api/test  " };

        Assert.AreEqual("api/test", request.Path);
    }

    [TestMethod]
    public void CompareRequest_Path_WhitespaceOnly_StoresNull()
    {
        var request = new CompareRequest { Path = "   " };

        Assert.IsNull(request.Path);
    }

    [TestMethod]
    public void IsValid_WithValidPath_ReturnsTrue()
    {
        var request = new CompareRequest { Path = "api/status" };

        Assert.IsTrue(request.IsValid());
    }

    [TestMethod]
    public void IsValid_WithNullPath_ReturnsFalse()
    {
        var request = new CompareRequest();

        Assert.IsFalse(request.IsValid());
    }

    [TestMethod]
    public void IsValid_PathExceedsMaxLength_ReturnsFalse()
    {
        var request = new CompareRequest { Path = new string('a', 3000) };

        Assert.IsFalse(request.IsValid());
    }

    [TestMethod]
    public void RequiresBody_PostMethod_ReturnsTrue()
    {
        var request = new CompareRequest { Path = "api/test", RequestMethod = HttpVerb.POST };

        Assert.IsTrue(request.RequiresBody());
    }

    [TestMethod]
    public void RequiresBody_PutMethod_ReturnsTrue()
    {
        var request = new CompareRequest { Path = "api/test", RequestMethod = HttpVerb.PUT };

        Assert.IsTrue(request.RequiresBody());
    }

    [TestMethod]
    public void RequiresBody_PatchMethod_ReturnsTrue()
    {
        var request = new CompareRequest { Path = "api/test", RequestMethod = HttpVerb.PATCH };

        Assert.IsTrue(request.RequiresBody());
    }

    [TestMethod]
    public void RequiresBody_GetMethod_ReturnsFalse()
    {
        var request = new CompareRequest { Path = "api/test", RequestMethod = HttpVerb.GET };

        Assert.IsFalse(request.RequiresBody());
    }

    [TestMethod]
    public void RequiresBody_DeleteMethod_ReturnsFalse()
    {
        var request = new CompareRequest { Path = "api/test", RequestMethod = HttpVerb.DELETE };

        Assert.IsFalse(request.RequiresBody());
    }
}
