using RESTRunner.Domain.Outputs;

namespace RESTRunner.Domain.Tests.Outputs;

/// <summary>
/// Unit tests for the <see cref="CsvOutput"/> class.
/// </summary>
[TestClass]
public class CsvOutputTests
{
    /// <summary>
    /// Tests that CsvOutput correctly writes data to a CSV file.
    /// </summary>
    [TestMethod]
    public void CsvOutput_WritesDataToCsvFile()
    {
        // Arrange - Use unique file name to avoid conflicts with parallel tests
        var testFilePath = $"c:\\test\\UnitTestCsvOutput_{Guid.NewGuid()}.csv";
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

        try
        {
            // Act
            using (var csvOutput = new CsvOutput(testFilePath))
            {
                csvOutput.WriteInfo(testData);
            }

            // Assert
            Assert.IsTrue(File.Exists(testFilePath), "CSV file was not created.");

            var csvContent = File.ReadAllText(testFilePath);
            StringAssert.Contains(csvContent, "GET,TestInstance", "CSV file does not contain expected data.");
        }
        finally
        {
            // Cleanup
            if (File.Exists(testFilePath))
            {
                File.Delete(testFilePath);
            }
        }
    }

    /// <summary>
    /// Tests that CsvOutput properly disposes of resources when disposed.
    /// </summary>
    [TestMethod]
    public void CsvOutput_DisposesCorrectly()
    {
        // Arrange - Use unique file name to avoid conflicts with parallel tests
        var testFilePath = $"c:\\test\\UnitTestCsvOutput_{Guid.NewGuid()}.csv";
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

        try
        {
            // Act
            using (var csvOutput = new CsvOutput(testFilePath))
            {
                csvOutput.WriteInfo(testData);
                // Do some additional operations with csvOutput if needed.
            } // The Dispose method should be called when exiting the using block.

            // Assert
            Assert.IsTrue(File.Exists(testFilePath));
        }
        finally
        {
            // Cleanup
            if (File.Exists(testFilePath))
            {
                File.Delete(testFilePath);
            }
        }
    }
}
