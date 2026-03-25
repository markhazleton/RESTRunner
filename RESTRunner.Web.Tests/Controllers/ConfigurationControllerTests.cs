using Moq;

namespace RESTRunner.Web.Tests.Controllers;

[TestClass]
public class ConfigurationControllerTests
{
    [TestMethod]
    public async Task Details_WithoutId_ReturnsNotFound()
    {
        var controller = new ConfigurationController(
            Mock.Of<IConfigurationService>(),
            Mock.Of<ICollectionService>(),
            Mock.Of<IOpenApiService>(),
            Mock.Of<IApiDefinitionMappingService>(),
            Mock.Of<Microsoft.Extensions.Logging.ILogger<ConfigurationController>>());

        var result = await controller.Details(string.Empty);

        Assert.IsInstanceOfType<NotFoundResult>(result);
    }
}