namespace RESTRunner.Domain.Tests.Models
{
    /// <summary>
    /// 
    /// </summary>
    [TestClass]
    public class CompareResultTests
    {
        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void TestMethod_Expected()
        {
            // Arrange
            var compareResult = new CompareResult()
            {
                SessionId = "TestSessionId",
                StatusDescription = "TestStatusDescription",
                Success = true,
                Duration = 100,
                LastRunDate = DateTime.Now,
                Hash = 123,
                Instance = "TestInstance",
                Request = "TestRequest",
                ResultCode = "TestResultCode",
                Verb = "GET",
                UserName = "TestUserName"
            };
            // Act


            // Assert
            Assert.IsNotNull(compareResult);
        }
    }
}
