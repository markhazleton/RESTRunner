namespace RequestSpark.Domain.Tests.Models;

[TestClass]
public class CompareRunnerTests
{
    private static CompareInstance ValidInstance() =>
        new() { Name = "Test", BaseUrl = "https://example.com/" };

    private static CompareRequest ValidRequest() =>
        new() { Path = "api/test" };

    [TestMethod]
    public void CompareRunner_DefaultConstructor_CreatesInstance()
    {
        var runner = new CompareRunner();

        Assert.IsNotNull(runner);
        Assert.IsNotNull(runner.Instances);
        Assert.IsNotNull(runner.Requests);
        Assert.IsNotNull(runner.Users);
        Assert.IsNotNull(runner.SessionId);
    }

    [TestMethod]
    public void IsValid_WithInstanceAndRequest_ReturnsTrue()
    {
        var runner = new CompareRunner();
        runner.Instances.Add(ValidInstance());
        runner.Requests.Add(ValidRequest());

        Assert.IsTrue(runner.IsValid());
    }

    [TestMethod]
    public void IsValid_NoInstances_ReturnsFalse()
    {
        var runner = new CompareRunner();
        runner.Requests.Add(ValidRequest());

        Assert.IsFalse(runner.IsValid());
    }

    [TestMethod]
    public void IsValid_NoRequests_ReturnsFalse()
    {
        var runner = new CompareRunner();
        runner.Instances.Add(ValidInstance());

        Assert.IsFalse(runner.IsValid());
    }

    [TestMethod]
    public void IsValid_InvalidInstance_ReturnsFalse()
    {
        var runner = new CompareRunner();
        runner.Instances.Add(new CompareInstance()); // missing name and url
        runner.Requests.Add(ValidRequest());

        Assert.IsFalse(runner.IsValid());
    }

    [TestMethod]
    public void GetTotalTestCount_OneEach_ReturnsOne()
    {
        var runner = new CompareRunner();
        runner.Instances.Add(ValidInstance());
        runner.Requests.Add(ValidRequest());
        runner.Users.Add(new CompareUser { UserName = "u1" });

        Assert.AreEqual(1, runner.GetTotalTestCount());
    }

    [TestMethod]
    public void GetTotalTestCount_MultipleInstances_MultipliesCorrectly()
    {
        var runner = new CompareRunner();
        runner.Instances.Add(ValidInstance());
        runner.Instances.Add(new CompareInstance { Name = "Test2", BaseUrl = "https://example2.com/" });
        runner.Requests.Add(ValidRequest());
        runner.Requests.Add(new CompareRequest { Path = "api/other" });
        runner.Users.Add(new CompareUser { UserName = "u1" });

        // 2 instances × 2 requests × 1 user = 4
        Assert.AreEqual(4, runner.GetTotalTestCount());
    }

    [TestMethod]
    public void GetTotalTestCount_NoUsers_UsesSingleUserMultiplier()
    {
        var runner = new CompareRunner();
        runner.Instances.Add(ValidInstance());
        runner.Requests.Add(ValidRequest());
        // No users — Math.Max(0, 1) = 1

        Assert.AreEqual(1, runner.GetTotalTestCount());
    }

    [TestMethod]
    public void DefaultIterations_Is100()
    {
        var runner = new CompareRunner();

        Assert.AreEqual(100, runner.Iterations);
    }

    [TestMethod]
    public void DefaultMaxConcurrency_Is10()
    {
        var runner = new CompareRunner();

        Assert.AreEqual(10, runner.MaxConcurrency);
    }
}

