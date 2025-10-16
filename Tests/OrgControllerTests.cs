using CrmBack.Api.Controllers;
using CrmBack.Core.Models.Payload.Org;
using CrmBack.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Xunit;

namespace Tests;

public class OrgControllerTests
{
    private readonly Mock<IOrgService> _mockOrgService;
    private readonly Mock<IDistributedCache> _mockCache;
    private readonly OrgController _controller;

    public OrgControllerTests()
    {
        _mockOrgService = new Mock<IOrgService>();
        _mockCache = new Mock<IDistributedCache>();
        _controller = new OrgController(_mockOrgService.Object, _mockCache.Object);
    }

    [Fact]
    public async Task GetById_ValidId_ReturnsOrg()
    {
        // Arrange
        var orgId = 1;
        var expectedOrg = new ReadOrgPayload(orgId, "Test Org", "Address", 45.0, -93.0, "1234567890");
        _mockOrgService.Setup(x => x.GetOrgById(orgId)).ReturnsAsync(expectedOrg);

        // Act
        var result = await _controller.GetById(orgId);

        // Assert
        var actionResult = Assert.IsType<ActionResult<ReadOrgPayload>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Equal(expectedOrg, okResult.Value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetById_InvalidId_ReturnsBadRequest(int invalidId)
    {
        // Act
        var result = await _controller.GetById(invalidId);

        // Assert
        var actionResult = Assert.IsType<ActionResult<ReadOrgPayload>>(result);
        Assert.IsType<BadRequestResult>(actionResult.Result);
    }

    [Fact]
    public async Task GetAll_ReturnsOrgs()
    {
        // Arrange
        var orgs = new List<ReadOrgPayload>
        {
            new(1, "Org1", "Addr1", 0, 0, ""),
            new(2, "Org2", "Addr2", 0, 0, "")
        };
        _mockOrgService.Setup(x => x.GetAllOrgs(false, 1, 10)).ReturnsAsync(orgs);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var actionResult = Assert.IsType<ActionResult<List<ReadOrgPayload>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Equal(orgs, okResult.Value);
    }

    [Fact]
    public async Task Create_ValidPayload_ReturnsCreated()
    {
        // Arrange
        var createPayload = new CreateOrgPayload("New Org", "Address", 0, 0, "");
        var readPayload = new ReadOrgPayload(1, "New Org", "Address", 0, 0, "");
        _mockOrgService.Setup(x => x.CreateOrg(createPayload)).ReturnsAsync(readPayload);

        // Act
        var result = await _controller.Create(createPayload);

        // Assert
        var actionResult = Assert.IsType<ActionResult<ReadOrgPayload>>(result);
        var createdResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
        Assert.Equal(readPayload, createdResult.Value);
    }

    [Fact]
    public async Task Create_InvalidModel_ReturnsBadRequest()
    {
        // Arrange
        var createPayload = new CreateOrgPayload("", "", 0, 0, "");
        _controller.ModelState.AddModelError("Name", "Required");

        // Act
        var result = await _controller.Create(createPayload);

        // Assert
        var actionResult = Assert.IsType<ActionResult<ReadOrgPayload>>(result);
        Assert.IsType<BadRequestObjectResult>(actionResult.Result);
    }

    [Fact]
    public async Task Create_ServiceReturnsNull_ReturnsBadRequest()
    {
        // Arrange
        var createPayload = new CreateOrgPayload("New Org", "Address", 0, 0, "");
        _mockOrgService.Setup(x => x.CreateOrg(createPayload)).ReturnsAsync((ReadOrgPayload?)null);

        // Act
        var result = await _controller.Create(createPayload);

        // Assert
        var actionResult = Assert.IsType<ActionResult<ReadOrgPayload>>(result);
        Assert.IsType<BadRequestObjectResult>(actionResult.Result);
    }

    [Fact]
    public async Task Update_ValidData_ReturnsTrue()
    {
        // Arrange
        var orgId = 1;
        var updatePayload = new UpdateOrgPayload("Updated", "Address", 0, 0, "");
        _mockOrgService.Setup(x => x.UpdateOrg(orgId, updatePayload)).ReturnsAsync(true);

        // Act
        var result = await _controller.Update(orgId, updatePayload);

        // Assert
        var actionResult = Assert.IsType<ActionResult<bool>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.True((bool?)okResult.Value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Update_InvalidId_ReturnsBadRequest(int invalidId)
    {
        // Arrange
        var updatePayload = new UpdateOrgPayload("Updated", "Address", 0, 0, "");

        // Act
        var result = await _controller.Update(invalidId, updatePayload);

        // Assert
        var actionResult = Assert.IsType<ActionResult<bool>>(result);
        Assert.IsType<BadRequestObjectResult>(actionResult.Result);
    }

    [Fact]
    public async Task Update_InvalidModel_ReturnsBadRequest()
    {
        // Arrange
        var orgId = 1;
        var updatePayload = new UpdateOrgPayload("", "", 0, 0, "");
        _controller.ModelState.AddModelError("Name", "Required");

        // Act
        var result = await _controller.Update(orgId, updatePayload);

        // Assert
        var actionResult = Assert.IsType<ActionResult<bool>>(result);
        Assert.IsType<BadRequestObjectResult>(actionResult.Result);
    }

    [Fact]
    public async Task Update_NotFound_ReturnsNotFound()
    {
        // Arrange
        var orgId = 999;
        var updatePayload = new UpdateOrgPayload("Updated", "Address", 0, 0, "");
        _mockOrgService.Setup(x => x.UpdateOrg(orgId, updatePayload)).ReturnsAsync(false);

        // Act
        var result = await _controller.Update(orgId, updatePayload);

        // Assert
        var actionResult = Assert.IsType<ActionResult<bool>>(result);
        Assert.IsType<NotFoundResult>(actionResult.Result);
    }

    [Fact]
    public async Task Delete_ValidId_ReturnsNoContent()
    {
        // Arrange
        var orgId = 1;
        _mockOrgService.Setup(x => x.DeleteOrg(orgId)).ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(orgId);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Delete_InvalidId_ReturnsBadRequest(int invalidId)
    {
        // Act
        var result = await _controller.Delete(invalidId);

        // Assert
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task Delete_NotFound_ReturnsNotFound()
    {
        // Arrange
        var orgId = 999;
        _mockOrgService.Setup(x => x.DeleteOrg(orgId)).ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(orgId);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
}