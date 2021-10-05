using Microsoft.VisualStudio.TestTools.UnitTesting;
using RESTRunner.Domain.Extensions;
using System;

namespace RESTRunner.Domain.Tests.Extensions
{
    [TestClass]
    public class String_ExtensionsTests
    {
        [TestMethod]
        public void GetDeterministicHashCode_NullString()
        {
            // Arrange
            string str = null;

            // Act
            var result = str.GetDeterministicHashCode();

            // Assert
            Assert.AreEqual(result,-1);
        }
        [TestMethod]
        public void GetDeterministicHashCode_SMALL()
        {
            // Arrange
            string str = "A";

            // Act
            var result1 = str.GetDeterministicHashCode();
            var result2 = str.GetDeterministicHashCode();

            // Assert
            Assert.AreEqual(result1, result2);
        }
        [TestMethod]
        public void GetDeterministicHashCode_GUID()
        {
            // Arrange
            string str = new Guid().ToString();

            // Act
            var result1 = str.GetDeterministicHashCode();
            var result2 = str.GetDeterministicHashCode();

            // Assert
            Assert.AreEqual(result1, result2);
        }
        [TestMethod]
        public void GetDeterministicHashCode_GUID3()
        {
            // Arrange
            string str = $"{new Guid()}{new Guid()}{new Guid()}";

            // Act
            var result1 = str.GetDeterministicHashCode();
            var result2 = str.GetDeterministicHashCode();

            // Assert
            Assert.AreEqual(result1, result2);
        }
    }
}
