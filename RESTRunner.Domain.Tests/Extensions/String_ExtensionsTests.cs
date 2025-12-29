
namespace RESTRunner.Domain.Tests.Extensions;

/// <summary>
/// 
/// </summary>
[TestClass]
public class String_ExtensionsTests
{
    /// <summary>
    /// 
    /// </summary>
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
    /// <summary>
    /// 
    /// </summary>
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
    /// <summary>
    /// 
    /// </summary>
    [TestMethod]
    public void GetDeterministicHashCode_NullString()
    {
        // Arrange
        string? str = null;

        // Act
        var result = str.GetDeterministicHashCode();

        // Assert
        Assert.AreEqual(0, result);
    }
    /// <summary>
    /// 
    /// </summary>
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
}
