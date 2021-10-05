using Microsoft.VisualStudio.TestTools.UnitTesting;
using RESTRunner.Domain.Models;

namespace RESTRunner.Domain.Tests.Models
{
    [TestClass]
    public class CompareRequestTests
    {
        [TestMethod]
        public void CompareRequest_Test()
        {
            // Arrange
            var compareRequest = new CompareRequest();

            // Act

            // Assert
            Assert.IsNotNull(compareRequest);
        }
    }
}
