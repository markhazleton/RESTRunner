namespace RequestSpark.Domain.Tests.Models;

[TestClass]
public class ExecutionStatisticsTests
{
    [TestMethod]
    public void ExecutionStatistics_DefaultState_HasZeroCounters()
    {
        var stats = new ExecutionStatistics();

        Assert.AreEqual(0, stats.TotalRequests);
        Assert.AreEqual(0, stats.SuccessfulRequests);
        Assert.AreEqual(0, stats.FailedRequests);
        Assert.AreEqual(0, stats.MinResponseTime);
        Assert.AreEqual(0, stats.MaxResponseTime);
    }

    [TestMethod]
    public void IncrementTotalRequests_IncrementsCounter()
    {
        var stats = new ExecutionStatistics();

        stats.IncrementTotalRequests();
        stats.IncrementTotalRequests();

        Assert.AreEqual(2, stats.TotalRequests);
    }

    [TestMethod]
    public void IncrementSuccessfulRequests_IncrementsCounter()
    {
        var stats = new ExecutionStatistics();

        stats.IncrementSuccessfulRequests();

        Assert.AreEqual(1, stats.SuccessfulRequests);
    }

    [TestMethod]
    public void IncrementFailedRequests_IncrementsCounter()
    {
        var stats = new ExecutionStatistics();

        stats.IncrementFailedRequests();

        Assert.AreEqual(1, stats.FailedRequests);
    }

    [TestMethod]
    public void AddResponseTime_UpdatesMinAndMax()
    {
        var stats = new ExecutionStatistics();

        stats.AddResponseTime(100);
        stats.AddResponseTime(50);
        stats.AddResponseTime(200);

        Assert.AreEqual(50, stats.MinResponseTime);
        Assert.AreEqual(200, stats.MaxResponseTime);
    }

    [TestMethod]
    public void CurrentAverageResponseTime_ReturnsCorrectAverage()
    {
        var stats = new ExecutionStatistics();
        stats.IncrementTotalRequests();
        stats.IncrementTotalRequests();
        stats.AddResponseTime(100);
        stats.AddResponseTime(200);

        Assert.AreEqual(150.0, stats.CurrentAverageResponseTime, 0.001);
    }

    [TestMethod]
    public void CurrentAverageResponseTime_WithNoRequests_ReturnsZero()
    {
        var stats = new ExecutionStatistics();

        Assert.AreEqual(0.0, stats.CurrentAverageResponseTime, 0.001);
    }

    [TestMethod]
    public void SuccessRate_WithMixedResults_ReturnsCorrectPercentage()
    {
        var stats = new ExecutionStatistics();
        stats.IncrementTotalRequests();
        stats.IncrementTotalRequests();
        stats.IncrementTotalRequests();
        stats.IncrementTotalRequests();
        stats.IncrementSuccessfulRequests();
        stats.IncrementSuccessfulRequests();
        stats.IncrementSuccessfulRequests();

        Assert.AreEqual(75.0, stats.SuccessRate, 0.001);
    }

    [TestMethod]
    public void SuccessRate_WithNoRequests_ReturnsZero()
    {
        var stats = new ExecutionStatistics();

        Assert.AreEqual(0.0, stats.SuccessRate, 0.001);
    }

    [TestMethod]
    public void GetResponseTimePercentile_ReturnsCorrectValue()
    {
        var stats = new ExecutionStatistics();
        foreach (var t in new long[] { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 })
            stats.AddResponseTime(t);

        Assert.AreEqual(50, stats.GetResponseTimePercentile(50));
        Assert.AreEqual(100, stats.GetResponseTimePercentile(100));
    }

    [TestMethod]
    public void GetResponseTimePercentile_EmptyCollection_ReturnsZero()
    {
        var stats = new ExecutionStatistics();

        Assert.AreEqual(0, stats.GetResponseTimePercentile(50));
    }

    [TestMethod]
    public void GetResponseTimePercentile_PercentileAbove100_ThrowsArgumentOutOfRangeException()
    {
        var stats = new ExecutionStatistics();
        try
        {
            stats.GetResponseTimePercentile(101);
            Assert.Fail("Expected ArgumentOutOfRangeException was not thrown");
        }
        catch (ArgumentOutOfRangeException) { }
    }

    [TestMethod]
    public void GetResponseTimePercentile_NegativePercentile_ThrowsArgumentOutOfRangeException()
    {
        var stats = new ExecutionStatistics();
        try
        {
            stats.GetResponseTimePercentile(-1);
            Assert.Fail("Expected ArgumentOutOfRangeException was not thrown");
        }
        catch (ArgumentOutOfRangeException) { }
    }

    [TestMethod]
    public void FinalizeStatistics_SetsAverageResponseTime()
    {
        var stats = new ExecutionStatistics { StartTime = DateTime.UtcNow };
        stats.AddResponseTime(100);
        stats.AddResponseTime(200);

        stats.FinalizeStatistics();

        Assert.AreEqual(150.0, stats.AverageResponseTime, 0.001);
    }

    [TestMethod]
    public void FinalizeStatistics_SetsEndTime()
    {
        var before = DateTime.UtcNow;
        var stats = new ExecutionStatistics { StartTime = before };

        stats.FinalizeStatistics();

        Assert.IsTrue(stats.EndTime >= before);
    }

    [TestMethod]
    public void TotalDuration_ReturnsEndMinusStart()
    {
        var start = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var end = start.AddSeconds(10);
        var stats = new ExecutionStatistics { StartTime = start, EndTime = end };

        Assert.AreEqual(TimeSpan.FromSeconds(10), stats.TotalDuration);
    }

    [TestMethod]
    public void ToString_ContainsKeyMetrics()
    {
        var stats = new ExecutionStatistics();
        stats.IncrementTotalRequests();
        stats.IncrementSuccessfulRequests();
        stats.AddResponseTime(100);
        stats.FinalizeStatistics();

        var result = stats.ToString();

        Assert.IsTrue(result.Contains("Total Requests"));
        Assert.IsTrue(result.Contains("Success Rate"));
        Assert.IsTrue(result.Contains("Avg Response"));
    }
}

