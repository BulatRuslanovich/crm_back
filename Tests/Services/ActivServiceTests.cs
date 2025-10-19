using CrmBack.Core.Models.Entities;
using CrmBack.Core.Models.Payload.Activ;
using CrmBack.Core.Models.Status;
using CrmBack.Repository;
using CrmBack.Services.Impl;
using FluentAssertions;
using Moq;

namespace CrmBack.Tests.Services;

public class ActivServiceTests : BaseServiceTest
{
    private readonly Mock<IActivRepository> _mockActivRepository;
    private readonly ActivService _activService;

    public ActivServiceTests()
    {
        _mockActivRepository = new Mock<IActivRepository>();
        _activService = new ActivService(_mockActivRepository.Object);
    }

    [Fact]
    public async Task GetById_WhenActivExists_ReturnsActivPayload()
    {
        // Arrange
        var activId = 1;
        var activEntity = new ActivEntity(1, 1, 1, 1, DateTime.Now, TimeSpan.FromHours(9), TimeSpan.FromHours(10), "Test Description", false);
        _mockActivRepository.Setup(r => r.GetByIdAsync(activId, CancellationToken))
            .ReturnsAsync(activEntity);

        // Act
        var result = await _activService.GetById(activId, CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result!.ActivId.Should().Be(activId);
        result.UsrId.Should().Be(1);
        result.OrgId.Should().Be(1);
        result.Description.Should().Be("Test Description");
    }

    [Fact]
    public async Task GetById_WhenActivDoesNotExist_ReturnsNull()
    {
        // Arrange
        var activId = 1;
        _mockActivRepository.Setup(r => r.GetByIdAsync(activId, CancellationToken))
            .ReturnsAsync((ActivEntity?)null);

        // Act
        var result = await _activService.GetById(activId, CancellationToken);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAll_ReturnsListOfActivs()
    {
        // Arrange
        var activs = new List<ActivEntity>
        {
            new(1, 1, 1, 1, DateTime.Now, TimeSpan.FromHours(9), TimeSpan.FromHours(10), "Description 1", false),
            new(2, 1, 1, 1, DateTime.Now, TimeSpan.FromHours(10), TimeSpan.FromHours(11), "Description 2", false)
        };
        _mockActivRepository.Setup(r => r.GetAllAsync(false, 1, 10, CancellationToken))
            .ReturnsAsync(activs);

        // Act
        var result = await _activService.GetAll(false, 1, 10, CancellationToken);

        // Assert
        result.Should().HaveCount(2);
        result[0].Description.Should().Be("Description 1");
        result[1].Description.Should().Be("Description 2");
    }

    [Fact]
    public async Task Create_ReturnsCreatedActiv()
    {
        // Arrange
        var createPayload = new CreateActivPayload(1, 1, DateTime.Now, TimeSpan.FromHours(9), TimeSpan.FromHours(10), "Test Description");
        var activId = 1;
        var createdActiv = new ActivEntity(1, 1, 1, 1, DateTime.Now, TimeSpan.FromHours(9), TimeSpan.FromHours(10), "Test Description", false);

        _mockActivRepository.Setup(r => r.CreateAsync(It.IsAny<ActivEntity>(), CancellationToken))
            .ReturnsAsync(activId);
        _mockActivRepository.Setup(r => r.GetByIdAsync(activId, CancellationToken))
            .ReturnsAsync(createdActiv);

        // Act
        var result = await _activService.Create(createPayload, CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result!.Description.Should().Be("Test Description");
        result.UsrId.Should().Be(1);
        result.OrgId.Should().Be(1);
        _mockActivRepository.Verify(r => r.CreateAsync(It.IsAny<ActivEntity>(), CancellationToken), Times.Once);
    }

    [Fact]
    public async Task Update_WhenActivExists_ReturnsTrue()
    {
        // Arrange
        var activId = 1;
        var existingActiv = new ActivEntity(1, 1, 1, 1, DateTime.Now, TimeSpan.FromHours(9), TimeSpan.FromHours(10), "Original Description", false);
        var updatePayload = new UpdateActivPayload(2, DateTime.Now.AddDays(1), TimeSpan.FromHours(10), TimeSpan.FromHours(11), "Updated Description");

        _mockActivRepository.Setup(r => r.GetByIdAsync(activId, CancellationToken))
            .ReturnsAsync(existingActiv);
        _mockActivRepository.Setup(r => r.UpdateAsync(It.IsAny<ActivEntity>(), CancellationToken))
            .ReturnsAsync(true);

        // Act
        var result = await _activService.Update(activId, updatePayload, CancellationToken);

        // Assert
        result.Should().BeTrue();
        _mockActivRepository.Verify(r => r.UpdateAsync(It.IsAny<ActivEntity>(), CancellationToken), Times.Once);
    }

    [Fact]
    public async Task Update_WhenActivDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var activId = 1;
        var updatePayload = new UpdateActivPayload(2, DateTime.Now.AddDays(1), TimeSpan.FromHours(10), TimeSpan.FromHours(11), "Updated Description");

        _mockActivRepository.Setup(r => r.GetByIdAsync(activId, CancellationToken))
            .ReturnsAsync((ActivEntity?)null);

        // Act
        var result = await _activService.Update(activId, updatePayload, CancellationToken);

        // Assert
        result.Should().BeFalse();
        _mockActivRepository.Verify(r => r.UpdateAsync(It.IsAny<ActivEntity>(), CancellationToken), Times.Never);
    }

    [Fact]
    public async Task Delete_ReturnsTrue()
    {
        // Arrange
        var activId = 1;
        _mockActivRepository.Setup(r => r.SoftDeleteAsync(activId, CancellationToken))
            .ReturnsAsync(true);

        // Act
        var result = await _activService.Delete(activId, CancellationToken);

        // Assert
        result.Should().BeTrue();
        _mockActivRepository.Verify(r => r.SoftDeleteAsync(activId, CancellationToken), Times.Once);
    }

    [Fact]
    public async Task GetByUserId_ReturnsListOfActivs()
    {
        // Arrange
        var userId = 1;
        var activs = new List<ActivEntity>
        {
            new(1, userId, 1, 1, DateTime.Now, TimeSpan.FromHours(9), TimeSpan.FromHours(10), "Description 1", false),
            new(2, userId, 1, 1, DateTime.Now, TimeSpan.FromHours(10), TimeSpan.FromHours(11), "Description 2", false)
        };

        _mockActivRepository.Setup(r => r.FindAllAsync(It.IsAny<Dictionary<string, object>>(), null, false, false, 1, 10, CancellationToken))
            .ReturnsAsync(activs);

        // Act
        var result = await _activService.GetByUserId(userId, CancellationToken);

        // Assert
        result.Should().HaveCount(2);
        result[0].UsrId.Should().Be(userId);
        result[1].UsrId.Should().Be(userId);
    }

    [Fact]
    public async Task GetAllStatus_ReturnsListOfStatuses()
    {
        // Arrange
        var statuses = new List<StatusEntity>
        {
            new(1, "Active", false),
            new(2, "Completed", false),
            new(3, "Cancelled", false)
        };

        _mockActivRepository.Setup(r => r.GetAllStatusAsync(CancellationToken))
            .ReturnsAsync(statuses);

        // Act
        var result = await _activService.GetAllStatus(CancellationToken);

        // Assert
        result.Should().HaveCount(3);
        result[0].Name.Should().Be("Active");
        result[1].Name.Should().Be("Completed");
        result[2].Name.Should().Be("Cancelled");
    }
}
