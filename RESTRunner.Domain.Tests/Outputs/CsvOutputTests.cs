using RESTRunner.Domain.Outputs;

namespace RESTRunner.Domain.Tests.Outputs;

[TestClass]
public class CsvOutputTests
{
    private const string TestFilePath = "c:\\test\\UnitTestCsvOutput.csv"; // Update this path with a valid test file path.

    [TestMethod]
    public void CsvOutput_WritesDataToCsvFile()
    {
        // Arrange
        var testData = new CompareResult
        {
            Verb = "GET",
            Instance = "TestInstance",
            LastRunDate = DateTime.Now,
            Duration = 200,
            Request = "/api/test",
            ResultCode = "200",
            SessionId = "ABC123",
            StatusDescription = "OK",
            Success = true,
            UserName = "testuser",
        };

        // Act
        using (var csvOutput = new CsvOutput(TestFilePath))
        {
            csvOutput.WriteInfo(testData);
        }

        // Assert
        Assert.IsTrue(File.Exists(TestFilePath), "CSV file was not created.");

        var csvContent = File.ReadAllText(TestFilePath);
        StringAssert.Contains(csvContent, "GET,TestInstance", "CSV file does not contain expected data.");
    }

    [TestMethod]
    public void CsvOutput_DisposesCorrectly()
    {
        // Arrange
        var testData = new CompareResult
        {
            Verb = "POST",
            Instance = "TestInstance2",
            LastRunDate = DateTime.Now,
            Duration = 200,
            Request = "/api/test2",
            ResultCode = "400",
            SessionId = "XYZ789",
            StatusDescription = "Bad Request",
            Success = false,
            UserName = "testuser2",
        };

        // Act
        using (var csvOutput = new CsvOutput(TestFilePath))
        {
            csvOutput.WriteInfo(testData);

            // Do some additional operations with csvOutput if needed.


        } // The Dispose method should be called when exiting the using block.

        // Assert
        Assert.IsTrue(File.Exists(TestFilePath));
    }
}
