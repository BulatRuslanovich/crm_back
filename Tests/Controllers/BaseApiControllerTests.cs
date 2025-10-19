using CrmBack.Core.Models.Payload.Org;
using CrmBack.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Tests.Controllers;

public class BaseApiControllerTests
{
    private readonly Mock<IOrgService> _mockOrgService;
    private readonly TestController _controller;
    private readonly Mock<HttpContext> _mockHttpContext;
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    public BaseApiControllerTests()
    {
        _mockOrgService = new Mock<IOrgService>();
        _controller = new TestController(_mockOrgService.Object);

        // Setup HttpContext for controller
        _mockHttpContext = new Mock<HttpContext>();
        _mockHttpContext.Setup(c => c.RequestAborted).Returns(_cancellationToken);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = _mockHttpContext.Object
        };
    }

    [Fact]
    public async Task GetById_WithValidId_ReturnsOk()
    {
        // Arrange
        var orgId = 1;
        var orgPayload = new ReadOrgPayload(1, "Test Org", "1234567890", 55.7558, 37.6176, "Test Address");
        _mockOrgService.Setup(s => s.GetById(orgId, _cancellationToken))
            .ReturnsAsync(orgPayload);

        // Act
        var result = await _controller.GetById(orgId);

        // Assert
        result.Should().BeOfType<ActionResult<ReadOrgPayload>>();
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.Value.Should().BeEquivalentTo(orgPayload);
        _mockOrgService.Verify(s => s.GetById(orgId, _cancellationToken), Times.Once);
    }

    [Fact]
    public async Task GetById_WithInvalidId_ReturnsBadRequest()
    {
        // Arrange
        var invalidId = 0;

        // Act
        var result = await _controller.GetById(invalidId);

        // Assert
        result.Should().BeOfType<ActionResult<ReadOrgPayload>>();
        var badRequestResult = result.Result as BadRequestResult;
        badRequestResult.Should().NotBeNull();
        _mockOrgService.Verify(s => s.GetById(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetById_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var orgId = 999;
        _mockOrgService.Setup(s => s.GetById(orgId, _cancellationToken))
            .ReturnsAsync((ReadOrgPayload?)null);

        // Act
        var result = await _controller.GetById(orgId);

        // Assert
        result.Should().BeOfType<ActionResult<ReadOrgPayload>>();
        var notFoundResult = result.Result as NotFoundResult;
        notFoundResult.Should().NotBeNull();
        _mockOrgService.Verify(s => s.GetById(orgId, _cancellationToken), Times.Once);
    }

    [Fact]
    public async Task GetAll_WithValidParameters_ReturnsOk()
    {
        // Arrange
        var orgs = new List<ReadOrgPayload>
        {
            new(1, "Org 1", "1234567890", 55.7558, 37.6176, "Address 1"),
            new(2, "Org 2", "0987654321", 55.7558, 37.6176, "Address 2")
        };
        _mockOrgService.Setup(s => s.GetAll(false, 1, 10, _cancellationToken))
            .ReturnsAsync(orgs);

        // Act
        var result = await _controller.GetAll();

        // Assert
        result.Should().BeOfType<ActionResult<List<ReadOrgPayload>>>();
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.Value.Should().BeEquivalentTo(orgs);
        _mockOrgService.Verify(s => s.GetAll(false, 1, 10, _cancellationToken), Times.Once);
    }

    [Fact]
    public async Task GetAll_WithInvalidPage_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.GetAll(page: 0);

        // Assert
        result.Should().BeOfType<ActionResult<List<ReadOrgPayload>>>();
        var badRequestResult = result.Result as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
        badRequestResult!.Value.Should().Be("Invalid pagination parameters");
        _mockOrgService.Verify(s => s.GetAll(It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetAll_WithInvalidPageSize_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.GetAll(pageSize: 0);

        // Assert
        result.Should().BeOfType<ActionResult<List<ReadOrgPayload>>>();
        var badRequestResult = result.Result as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
        badRequestResult!.Value.Should().Be("Invalid pagination parameters");
        _mockOrgService.Verify(s => s.GetAll(It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetAll_WithPageSizeTooLarge_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.GetAll(pageSize: 1001);

        // Assert
        result.Should().BeOfType<ActionResult<List<ReadOrgPayload>>>();
        var badRequestResult = result.Result as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
        badRequestResult!.Value.Should().Be("Invalid pagination parameters");
        _mockOrgService.Verify(s => s.GetAll(It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Create_WithValidPayload_ReturnsCreatedAtAction()
    {
        // Arrange
        var createPayload = new CreateOrgPayload("New Org", "1234567890", 55.7558, 37.6176, "New Address");
        var createdOrg = new ReadOrgPayload(1, "New Org", "1234567890", 55.7558, 37.6176, "New Address");

        _mockOrgService.Setup(s => s.Create(createPayload, _cancellationToken))
            .ReturnsAsync(createdOrg);

        // Act
        var result = await _controller.Create(createPayload);

        // Assert
        result.Should().BeOfType<ActionResult<ReadOrgPayload>>();
        var createdResult = result.Result as CreatedResult;
        createdResult.Should().NotBeNull();
        createdResult!.Value.Should().BeEquivalentTo(createdOrg);
        _mockOrgService.Verify(s => s.Create(createPayload, _cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Create_WithInvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        var createPayload = new CreateOrgPayload("", "1234567890", 55.7558, 37.6176, "New Address");
        _controller.ModelState.AddModelError("Name", "Name is required");

        // Act
        var result = await _controller.Create(createPayload);

        // Assert
        result.Should().BeOfType<ActionResult<ReadOrgPayload>>();
        var badRequestResult = result.Result as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
        _mockOrgService.Verify(s => s.Create(It.IsAny<CreateOrgPayload>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Update_WithValidIdAndPayload_ReturnsOk()
    {
        // Arrange
        var orgId = 1;
        var updatePayload = new UpdateOrgPayload("Updated Org", "0987654321", 55.7558, 37.6176, "Updated Address");
        _mockOrgService.Setup(s => s.Update(orgId, updatePayload, _cancellationToken))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Update(orgId, updatePayload);

        // Assert
        result.Should().BeOfType<ActionResult<bool>>();
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.Value.Should().Be(true);
        _mockOrgService.Verify(s => s.Update(orgId, updatePayload, _cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Update_WithInvalidId_ReturnsBadRequest()
    {
        // Arrange
        var invalidId = 0;
        var updatePayload = new UpdateOrgPayload("Updated Org", "0987654321", 55.7558, 37.6176, "Updated Address");

        // Act
        var result = await _controller.Update(invalidId, updatePayload);

        // Assert
        result.Should().BeOfType<ActionResult<bool>>();
        var badRequestResult = result.Result as BadRequestResult;
        badRequestResult.Should().NotBeNull();
        _mockOrgService.Verify(s => s.Update(It.IsAny<int>(), It.IsAny<UpdateOrgPayload>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Update_WithInvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        var orgId = 1;
        var updatePayload = new UpdateOrgPayload("", "0987654321", 55.7558, 37.6176, "Updated Address");
        _controller.ModelState.AddModelError("Name", "Name is required");

        // Act
        var result = await _controller.Update(orgId, updatePayload);

        // Assert
        result.Should().BeOfType<ActionResult<bool>>();
        var badRequestResult = result.Result as BadRequestResult;
        badRequestResult.Should().NotBeNull();
        _mockOrgService.Verify(s => s.Update(It.IsAny<int>(), It.IsAny<UpdateOrgPayload>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Update_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var orgId = 999;
        var updatePayload = new UpdateOrgPayload("Updated Org", "0987654321", 55.7558, 37.6176, "Updated Address");
        _mockOrgService.Setup(s => s.Update(orgId, updatePayload, _cancellationToken))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Update(orgId, updatePayload);

        // Assert
        result.Should().BeOfType<ActionResult<bool>>();
        var notFoundResult = result.Result as NotFoundResult;
        notFoundResult.Should().NotBeNull();
        _mockOrgService.Verify(s => s.Update(orgId, updatePayload, _cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Delete_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var orgId = 1;
        _mockOrgService.Setup(s => s.Delete(orgId, _cancellationToken))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(orgId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _mockOrgService.Verify(s => s.Delete(orgId, _cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Delete_WithInvalidId_ReturnsBadRequest()
    {
        // Arrange
        var invalidId = 0;

        // Act
        var result = await _controller.Delete(invalidId);

        // Assert
        result.Should().BeOfType<BadRequestResult>();
        _mockOrgService.Verify(s => s.Delete(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Delete_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var orgId = 999;
        _mockOrgService.Setup(s => s.Delete(orgId, _cancellationToken))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(orgId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
        _mockOrgService.Verify(s => s.Delete(orgId, _cancellationToken), Times.Once);
    }

    [Fact]
    public void ValidateId_WithPositiveId_ReturnsTrue()
    {
        // Arrange
        var validId = 1;

        // Act
        var result = _controller.ValidateId(validId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ValidateId_WithZero_ReturnsFalse()
    {
        // Arrange
        var invalidId = 0;

        // Act
        var result = _controller.ValidateId(invalidId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidateId_WithNegativeId_ReturnsFalse()
    {
        // Arrange
        var invalidId = -1;

        // Act
        var result = _controller.ValidateId(invalidId);

        // Assert
        result.Should().BeFalse();
    }
}
