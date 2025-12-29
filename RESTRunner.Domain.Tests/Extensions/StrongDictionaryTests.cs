
namespace RESTRunner.Domain.Tests.Extensions;

/// <summary>
/// Defines test class StrongDictionaryTests.
/// </summary>
[TestClass()]
public class StrongDictionaryTests
{
    /// <summary>
    /// Defines the test method AddTest.
    /// </summary>
    [TestMethod()]
    public void AddTest()
    {
        var myTest = new StrongDictionary<int, string>();
        myTest.Add(1, "test");
        Assert.AreEqual(1, myTest.GetList().Count);
    }

    /// <summary>
    /// Defines the test method AddTest1.
    /// </summary>
    [TestMethod()]
    public void AddTest_Duplicate()
    {
        var myTest = new StrongDictionary<int, string>();
        myTest.Add(1, "test");
        myTest[2] = "test";
        myTest.Add(3, "initial");

        var tempDic = new Dictionary<int, string>
            {
                { 3, "test3" },
                { 4, "test4" }
            };
        myTest.Add(tempDic);

        myTest.Add(1, "test1");
        myTest[2] = "test2";

        Assert.AreEqual("test1", myTest[1]);
        Assert.AreEqual("test2", myTest[2]);
        Assert.AreEqual("test3", myTest[3]);
        Assert.AreEqual("test4", myTest[4]);
    }

    /// <summary>
    /// Defines the test method GetListTest.
    /// </summary>
    [TestMethod()]
    public void GetJson_TestIntString()
    {
        var myTest = new StrongDictionary<int, string>();
        myTest.Add(1, "one");
        myTest.Add(2, "two");
        var myResult = myTest.GetJson();
        Assert.IsNotNull(myResult);
        Assert.AreEqual(@"{""1"":""one"",""2"":""two""}", myResult);
    }
    /// <summary>
    /// Defines the test method GetListTest.
    /// </summary>
    [TestMethod()]
    public void GetJson_TestStringString()
    {
        var myTest = new StrongDictionary<string, string>();
        myTest.Add("1", "one");
        myTest.Add("2", "two");
        var myResult = myTest.GetJson();
        Assert.IsNotNull(myResult);
        Assert.AreEqual(@"{""1"":""one"",""2"":""two""}", myResult);
    }

    /// <summary>
    /// Defines the test method GetListTest.
    /// </summary>
    [TestMethod()]
    public void GetListTest()
    {
        var myTest = new StrongDictionary<int, string>();
        myTest.Add(1, "one");
        myTest.Add(2, "two");
        var myResult = myTest.GetList().FirstOrDefault();
        Assert.AreEqual("1 - one", myResult);
    }
    /// <summary>
    /// Defines the test method GetObjectDataTest.
    /// </summary>
    [TestMethod()]
    public void GetObjectDataTest() { }

    /// <summary>
    /// Defines the test method StrongDictionaryTest.
    /// </summary>
    [TestMethod()]
    public void StrongDictionaryTest()
    {
        var myTest = new StrongDictionary<int, string>();
        Assert.AreNotEqual(null, myTest);
    }

    /// <summary>
    /// Defines the test method StrongDictionaryTest1.
    /// </summary>
    [TestMethod()]
    public void StrongDictionaryTest1()
    {
        var myTest = new StrongDictionary<int, string>();
        Assert.AreNotEqual(null, myTest);
    }
}
