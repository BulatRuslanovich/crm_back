namespace Tests;

using CrmBack.Api.Controllers;
using CrmBack.Core.Models.Payload.Org;
using CrmBack.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

[Trait("Category", "Unit")]
[Trait("Layer", "Controller")]
public class OrgControllerTests
{
    private readonly Mock<IOrgService> _mockOrgService;
    private readonly Mock<IDistributedCache> _mockCache;
    private readonly Mock<ILogger<OrgController>> _mockLogger;
    private readonly Mock<ICacheEntry> _mockCacheEntry;
    private readonly OrgController _controller;

    public OrgControllerTests()
    {
        _mockOrgService = new Mock<IOrgService>();
        _mockCache = new Mock<IDistributedCache>();
        _mockLogger = new Mock<ILogger<OrgController>>();

        _controller = new OrgController(
            _mockOrgService.Object,
            _mockCache.Object);
    }

    #region GetById Tests

    [Fact(DisplayName = "GetById should return organization when found")]
    [Trait("Method", "GetById")]
    public async Task GetById_WhenOrgExists_ReturnsOrganization()
    {
        // Arrange - prepare test data
        var orgId = 1;
        var expectedOrg = new ReadOrgPayload(orgId, "Test Organization", "123 Test St", 45.0, -93.0, "1234567890");

        object? cachedValue = null;
        _mockCache
            .Setup(x => x.TryGetValue(It.IsAny<object>(), out cachedValue))
            .Returns(false);

        _mockOrgService
            .Setup(x => x.GetOrgById(orgId))
            .ReturnsAsync(expectedOrg);

        // Act - call the method under test
        var result = await _controller.GetById(orgId);

        // Assert - verify the result
        var actionResult = Assert.IsType<ActionResult<ReadOrgPayload>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnedOrg = Assert.IsType<ReadOrgPayload>(okResult.Value);

        Assert.Equal(orgId, returnedOrg.OrgId);
        Assert.Equal("Test Organization", returnedOrg.Name);
    }

    [Fact(DisplayName = "GetById should return BadRequest when ID is negative")]
    [Trait("Method", "GetById")]
    public async Task GetById_WhenIdIsNegative_ReturnsBadRequest()
    {
        // Arrange
        var invalidId = -1;

        // Act
        var result = await _controller.GetById(invalidId);

        // Assert
        var actionResult = Assert.IsType<ActionResult<ReadOrgPayload>>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);

        Assert.Equal("Organization ID must be positive", badRequestResult.Value);
    }

    [Fact(DisplayName = "GetById should return BadRequest when ID is zero")]
    [Trait("Method", "GetById")]
    public async Task GetById_WhenIdIsZero_ReturnsBadRequest()
    {
        // Arrange
        var invalidId = 0;

        // Act
        var result = await _controller.GetById(invalidId);

        // Assert
        var actionResult = Assert.IsType<ActionResult<ReadOrgPayload>>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);

        Assert.Equal("Organization ID must be positive", badRequestResult.Value);
    }

    [Fact(DisplayName = "GetById should return NotFound when organization does not exist")]
    [Trait("Method", "GetById")]
    public async Task GetById_WhenOrgNotFound_ReturnsNotFound()
    {
        // Arrange
        var orgId = 999;

        object? cachedValue = null;
        _mockCache
            .Setup(x => x.TryGetValue(It.IsAny<object>(), out cachedValue))
            .Returns(false);

        _mockOrgService
            .Setup(x => x.GetOrgById(orgId))
            .ReturnsAsync((ReadOrgPayload?)null);

        // Act
        var result = await _controller.GetById(orgId);

        // Assert
        var actionResult = Assert.IsType<ActionResult<ReadOrgPayload>>(result);
        Assert.IsType<NotFoundResult>(actionResult.Result);
    }

    [Fact(DisplayName = "GetById should use cache when organization is cached")]
    [Trait("Method", "GetById")]
    public async Task GetById_WhenOrgIsCached_ReturnsCachedValue()
    {
        // Arrange
        var orgId = 1;
        var cachedOrg = new ReadOrgPayload(orgId, "Cached Org", "", 0.0, 0.0, "");

        object? cachedValue = cachedOrg;
        _mockCache
            .Setup(x => x.TryGetValue($"org_{orgId}", out cachedValue))
            .Returns(true);

        // Act
        var result = await _controller.GetById(orgId);

        // Assert - verify cached value is returned
        var actionResult = Assert.IsType<ActionResult<ReadOrgPayload>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnedOrg = Assert.IsType<ReadOrgPayload>(okResult.Value);

        Assert.Equal("Cached Org", returnedOrg.Name);

        // Verify service was not called
        _mockOrgService.Verify(x => x.GetOrgById(It.IsAny<int>()), Times.Never);
    }

    #endregion

    #region GetAll Tests

    [Fact(DisplayName = "GetAll should return list of organizations")]
    [Trait("Method", "GetAll")]
    public async Task GetAll_WhenOrgsExist_ReturnsOrganizationList()
    {
        // Arrange - prepare test data
        var expectedOrgs = new List<ReadOrgPayload>
        {
            new(1, "Organization 1", "", 0.0, 0.0, ""),
            new(2, "Organization 2", "", 0.0, 0.0, ""),
            new(3, "Organization 3", "", 0.0, 0.0, "")
        };

        object? cachedValue = null;
        _mockCache
            .Setup(x => x.TryGetValue(It.IsAny<object>(), out cachedValue))
            .Returns(false);

        _mockOrgService
            .Setup(x => x.GetAllOrgs())
            .ReturnsAsync(expectedOrgs);

        // Act - call the method under test
        var result = await _controller.GetAll();

        // Assert - verify the result
        var actionResult = Assert.IsType<ActionResult<List<ReadOrgPayload>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnedOrgs = Assert.IsType<List<ReadOrgPayload>>(okResult.Value);

        Assert.Equal(3, returnedOrgs.Count);
        Assert.Equal("Organization 1", returnedOrgs[0].Name);
    }

    [Fact(DisplayName = "GetAll should return NotFound when no organizations exist")]
    [Trait("Method", "GetAll")]
    public async Task GetAll_WhenNoOrgsExist_ReturnsNotFound()
    {
        // Arrange
        object? cachedValue = null;
        _mockCache
            .Setup(x => x.TryGetValue(It.IsAny<object>(), out cachedValue))
            .Returns(false);

        _mockOrgService
            .Setup(x => x.GetAllOrgs())
            .ReturnsAsync(new List<ReadOrgPayload>());

        // Act
        var result = await _controller.GetAll();

        // Assert
        var actionResult = Assert.IsType<ActionResult<List<ReadOrgPayload>>>(result);
        Assert.IsType<NotFoundResult>(actionResult.Result);
    }

    [Fact(DisplayName = "GetAll should use cache when organizations are cached")]
    [Trait("Method", "GetAll")]
    public async Task GetAll_WhenOrgsAreCached_ReturnsCachedValue()
    {
        // Arrange
        var cachedOrgs = new List<ReadOrgPayload>
        {
            new(1, "Cached Org", "", 0.0, 0.0, "")
        };

        object? cachedValue = cachedOrgs;
        _mockCache
            .Setup(x => x.TryGetValue("all_orgs", out cachedValue))
            .Returns(true);

        // Act
        var result = await _controller.GetAll();

        // Assert - verify cached value is returned
        var actionResult = Assert.IsType<ActionResult<List<ReadOrgPayload>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnedOrgs = Assert.IsType<List<ReadOrgPayload>>(okResult.Value);

        Assert.Single(returnedOrgs);
        Assert.Equal("Cached Org", returnedOrgs[0].Name);

        // Verify service was not called
        _mockOrgService.Verify(x => x.GetAllOrgs(), Times.Never);
    }

    #endregion

    #region Create Tests

    [Fact(DisplayName = "Create should return created organization with location")]
    [Trait("Method", "Create")]
    public async Task Create_WithValidPayload_ReturnsCreatedOrganization()
    {
        // Arrange - prepare test data
        var createPayload = new CreateOrgPayload("New Organization", "", 0.0, 0.0, "");
        var createdOrg = new ReadOrgPayload(1, "New Organization", "", 0.0, 0.0, "");

        _mockOrgService
            .Setup(x => x.CreateOrg(createPayload))
            .ReturnsAsync(createdOrg);

        // Act - call the method under test
        var result = await _controller.Create(createPayload);

        // Assert - verify the result
        var actionResult = Assert.IsType<ActionResult<ReadOrgPayload>>(result);
        var createdResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);

        Assert.Equal(nameof(OrgController.GetById), createdResult.ActionName);
        Assert.NotNull(createdResult.RouteValues);
        Assert.Equal(1, createdResult.RouteValues["id"]);

        var returnedOrg = Assert.IsType<ReadOrgPayload>(createdResult.Value);
        Assert.Equal(1, returnedOrg.OrgId);
        Assert.Equal("New Organization", returnedOrg.Name);
    }

    [Fact(DisplayName = "Create should return BadRequest when model state is invalid")]
    [Trait("Method", "Create")]
    public async Task Create_WithInvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        var createPayload = new CreateOrgPayload("", "", 0.0, 0.0, "");
        _controller.ModelState.AddModelError("Name", "The Name field is required");

        // Act
        var result = await _controller.Create(createPayload);

        // Assert
        var actionResult = Assert.IsType<ActionResult<ReadOrgPayload>>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);

        Assert.IsType<SerializableError>(badRequestResult.Value);
    }

    [Fact(DisplayName = "Create should return BadRequest when service fails to create")]
    [Trait("Method", "Create")]
    public async Task Create_WhenServiceFails_ReturnsBadRequest()
    {
        // Arrange
        var createPayload = new CreateOrgPayload("New Organization", "", 0.0, 0.0, "");

        _mockOrgService
            .Setup(x => x.CreateOrg(createPayload))
            .ReturnsAsync((ReadOrgPayload?)null);

        // Act
        var result = await _controller.Create(createPayload);

        // Assert
        var actionResult = Assert.IsType<ActionResult<ReadOrgPayload>>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);

        Assert.Equal("Failed to create organization", badRequestResult.Value);
    }

    [Fact(DisplayName = "Create should invalidate cache on success")]
    [Trait("Method", "Create")]
    public async Task Create_OnSuccess_InvalidatesCache()
    {
        // Arrange
        var createPayload = new CreateOrgPayload("New Organization", "", 0.0, 0.0, "");
        var createdOrg = new ReadOrgPayload(5, "New Organization", "", 0.0, 0.0, "");

        _mockOrgService
            .Setup(x => x.CreateOrg(createPayload))
            .ReturnsAsync(createdOrg);

        // Act
        await _controller.Create(createPayload);

        // Assert - verify cache invalidation
        _mockCache.Verify(x => x.Remove(It.Is<string>(k => k.Contains("org_5"))), Times.Once);
        _mockCache.Verify(x => x.Remove("all_orgs"), Times.Once);
    }

    #endregion

    #region Update Tests

    [Fact(DisplayName = "Update should return true when organization is updated")]
    [Trait("Method", "Update")]
    public async Task Update_WithValidData_ReturnsTrue()
    {
        // Arrange - prepare test data
        var orgId = 1;
        var updatePayload = new UpdateOrgPayload("Updated Organization", "lol", 0.0, 0.0, "");

        _mockOrgService
            .Setup(x => x.UpdateOrg(orgId, updatePayload))
            .ReturnsAsync(true);

        // Act - call the method under test
        var result = await _controller.Update(orgId, updatePayload);

        // Assert - verify the result
        var actionResult = Assert.IsType<ActionResult<bool>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);

        Assert.True((bool?)okResult.Value);
    }

    [Fact(DisplayName = "Update should return BadRequest when ID is invalid")]
    [Trait("Method", "Update")]
    public async Task Update_WithInvalidId_ReturnsBadRequest()
    {
        // Arrange
        var invalidId = -1;
        var updatePayload = new UpdateOrgPayload("Updated Organization", "lol", 0.0, 0.0, "");

        // Act
        var result = await _controller.Update(invalidId, updatePayload);

        // Assert
        var actionResult = Assert.IsType<ActionResult<bool>>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);

        Assert.Equal("Organization ID must be positive", badRequestResult.Value);
    }

    [Fact(DisplayName = "Update should return BadRequest when model state is invalid")]
    [Trait("Method", "Update")]
    public async Task Update_WithInvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        var orgId = 1;
        var updatePayload = new UpdateOrgPayload("Updated Organization", "lol", 0.0, 0.0, "");
        _controller.ModelState.AddModelError("Name", "The Name field is required");

        // Act
        var result = await _controller.Update(orgId, updatePayload);

        // Assert
        var actionResult = Assert.IsType<ActionResult<bool>>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);

        Assert.Equal("Invalid data", badRequestResult.Value);
    }

    [Fact(DisplayName = "Update should return NotFound when organization does not exist")]
    [Trait("Method", "Update")]
    public async Task Update_WhenOrgNotFound_ReturnsNotFound()
    {
        // Arrange
        var orgId = 999;
        var updatePayload = new UpdateOrgPayload("Updated Organization", "lol", 0.0, 0.0, "");

        _mockOrgService
            .Setup(x => x.UpdateOrg(orgId, updatePayload))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Update(orgId, updatePayload);

        // Assert
        var actionResult = Assert.IsType<ActionResult<bool>>(result);
        Assert.IsType<NotFoundResult>(actionResult.Result);
    }

    [Fact(DisplayName = "Update should invalidate cache on success")]
    [Trait("Method", "Update")]
    public async Task Update_OnSuccess_InvalidatesCache()
    {
        // Arrange
        var orgId = 3;
        var updatePayload = new UpdateOrgPayload("Updated Organization", "lol", 0.0, 0.0, "");

        _mockOrgService
            .Setup(x => x.UpdateOrg(orgId, updatePayload))
            .ReturnsAsync(true);

        // Act
        await _controller.Update(orgId, updatePayload);

        // Assert - verify cache invalidation
        _mockCache.Verify(x => x.Remove(It.Is<string>(k => k.Contains("org_3"))), Times.Once);
        _mockCache.Verify(x => x.Remove("all_orgs"), Times.Once);
    }

    #endregion

    #region Delete Tests

    [Fact(DisplayName = "Delete should return NoContent when organization is deleted")]
    [Trait("Method", "Delete")]
    public async Task Delete_WithValidId_ReturnsNoContent()
    {
        // Arrange - prepare test data
        var orgId = 1;

        _mockOrgService
            .Setup(x => x.DeleteOrg(orgId))
            .ReturnsAsync(true);

        // Act - call the method under test
        var result = await _controller.Delete(orgId);

        // Assert - verify the result
        Assert.IsType<NoContentResult>(result);
    }

    [Fact(DisplayName = "Delete should return BadRequest when ID is invalid")]
    [Trait("Method", "Delete")]
    public async Task Delete_WithInvalidId_ReturnsBadRequest()
    {
        // Arrange
        var invalidId = -1;

        // Act
        var result = await _controller.Delete(invalidId);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Organization ID must be positive", badRequestResult.Value);
    }

    [Fact(DisplayName = "Delete should return NotFound when organization does not exist")]
    [Trait("Method", "Delete")]
    public async Task Delete_WhenOrgNotFound_ReturnsNotFound()
    {
        // Arrange
        var orgId = 999;

        _mockOrgService
            .Setup(x => x.DeleteOrg(orgId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(orgId);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact(DisplayName = "Delete should invalidate cache on success")]
    [Trait("Method", "Delete")]
    public async Task Delete_OnSuccess_InvalidatesCache()
    {
        // Arrange
        var orgId = 7;

        _mockOrgService
            .Setup(x => x.DeleteOrg(orgId))
            .ReturnsAsync(true);

        // Act
        await _controller.Delete(orgId);

        // Assert - verify cache invalidation
        _mockCache.Verify(x => x.Remove(It.Is<string>(k => k.Contains("org_7"))), Times.Once);
        _mockCache.Verify(x => x.Remove("all_orgs"), Times.Once);
    }

    #endregion
}