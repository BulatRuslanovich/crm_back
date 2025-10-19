using CrmBack.Core.Models.Entities;
using CrmBack.Core.Models.Payload.Org;
using CrmBack.Repository;
using CrmBack.Services.Impl;
using FluentAssertions;
using Moq;

namespace CrmBack.Tests.Services;

public class OrgServiceTests : BaseServiceTest
{
    private readonly Mock<IOrgRepository> _mockOrgRepository;
    private readonly OrgService _orgService;

    public OrgServiceTests()
    {
        _mockOrgRepository = new Mock<IOrgRepository>();
        _orgService = new OrgService(_mockOrgRepository.Object);
    }

    [Fact]
    public async Task GetById_WhenOrgExists_ReturnsOrgPayload()
    {
        // Arrange
        var orgId = 1;
        var orgEntity = new OrgEntity(1, "Test Organization", "1234567890", 55.7558, 37.6176, "Test Address", false);
        _mockOrgRepository.Setup(r => r.GetByIdAsync(orgId, CancellationToken))
            .ReturnsAsync(orgEntity);

        // Act
        var result = await _orgService.GetById(orgId, CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result!.OrgId.Should().Be(orgId);
        result.Name.Should().Be("Test Organization");
        result.INN.Should().Be("1234567890");
        result.Latitude.Should().Be(55.7558);
        result.Longitude.Should().Be(37.6176);
        result.Address.Should().Be("Test Address");
    }

    [Fact]
    public async Task GetById_WhenOrgDoesNotExist_ReturnsNull()
    {
        // Arrange
        var orgId = 1;
        _mockOrgRepository.Setup(r => r.GetByIdAsync(orgId, CancellationToken))
            .ReturnsAsync((OrgEntity?)null);

        // Act
        var result = await _orgService.GetById(orgId, CancellationToken);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAll_ReturnsListOfOrgs()
    {
        // Arrange
        var orgs = new List<OrgEntity>
        {
            new(1, "Organization 1", "1234567890", 55.7558, 37.6176, "Address 1", false),
            new(2, "Organization 2", "0987654321", 55.7558, 37.6176, "Address 2", false)
        };
        _mockOrgRepository.Setup(r => r.GetAllAsync(false, 1, 10, CancellationToken))
            .ReturnsAsync(orgs);

        // Act
        var result = await _orgService.GetAll(false, 1, 10, CancellationToken);

        // Assert
        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Organization 1");
        result[1].Name.Should().Be("Organization 2");
    }

    [Fact]
    public async Task Create_ReturnsCreatedOrg()
    {
        // Arrange
        var createPayload = new CreateOrgPayload("New Organization", "1234567890", 55.7558, 37.6176, "New Address");
        var orgId = 1;
        var createdOrg = new OrgEntity(1, "New Organization", "1234567890", 55.7558, 37.6176, "New Address", false);

        _mockOrgRepository.Setup(r => r.CreateAsync(It.IsAny<OrgEntity>(), CancellationToken))
            .ReturnsAsync(orgId);
        _mockOrgRepository.Setup(r => r.GetByIdAsync(orgId, CancellationToken))
            .ReturnsAsync(createdOrg);

        // Act
        var result = await _orgService.Create(createPayload, CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("New Organization");
        result.INN.Should().Be("1234567890");
        result.Address.Should().Be("New Address");
        _mockOrgRepository.Verify(r => r.CreateAsync(It.IsAny<OrgEntity>(), CancellationToken), Times.Once);
    }

    [Fact]
    public async Task Update_WhenOrgExists_ReturnsTrue()
    {
        // Arrange
        var orgId = 1;
        var existingOrg = new OrgEntity(1, "Original Organization", "1234567890", 55.7558, 37.6176, "Original Address", false);
        var updatePayload = new UpdateOrgPayload("Updated Organization", "0987654321", 55.7558, 37.6176, "Updated Address");

        _mockOrgRepository.Setup(r => r.GetByIdAsync(orgId, CancellationToken))
            .ReturnsAsync(existingOrg);
        _mockOrgRepository.Setup(r => r.UpdateAsync(It.IsAny<OrgEntity>(), CancellationToken))
            .ReturnsAsync(true);

        // Act
        var result = await _orgService.Update(orgId, updatePayload, CancellationToken);

        // Assert
        result.Should().BeTrue();
        _mockOrgRepository.Verify(r => r.UpdateAsync(It.IsAny<OrgEntity>(), CancellationToken), Times.Once);
    }

    [Fact]
    public async Task Update_WhenOrgDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var orgId = 1;
        var updatePayload = new UpdateOrgPayload("Updated Organization", "0987654321", 55.7558, 37.6176, "Updated Address");

        _mockOrgRepository.Setup(r => r.GetByIdAsync(orgId, CancellationToken))
            .ReturnsAsync((OrgEntity?)null);

        // Act
        var result = await _orgService.Update(orgId, updatePayload, CancellationToken);

        // Assert
        result.Should().BeFalse();
        _mockOrgRepository.Verify(r => r.UpdateAsync(It.IsAny<OrgEntity>(), CancellationToken), Times.Never);
    }

    [Fact]
    public async Task Update_WithPartialData_UpdatesOnlyProvidedFields()
    {
        // Arrange
        var orgId = 1;
        var existingOrg = new OrgEntity(1, "Original Organization", "1234567890", 55.7558, 37.6176, "Original Address", false);
        var updatePayload = new UpdateOrgPayload("Updated Organization", null, null, null, null);

        _mockOrgRepository.Setup(r => r.GetByIdAsync(orgId, CancellationToken))
            .ReturnsAsync(existingOrg);
        _mockOrgRepository.Setup(r => r.UpdateAsync(It.IsAny<OrgEntity>(), CancellationToken))
            .ReturnsAsync(true);

        // Act
        var result = await _orgService.Update(orgId, updatePayload, CancellationToken);

        // Assert
        result.Should().BeTrue();
        _mockOrgRepository.Verify(r => r.UpdateAsync(It.Is<OrgEntity>(e =>
            e.name == "Updated Organization" &&
            e.inn == "1234567890" &&
            e.latitude == 55.7558 &&
            e.longitude == 37.6176 &&
            e.address == "Original Address"
        ), CancellationToken), Times.Once);
    }

    [Fact]
    public async Task Delete_ReturnsTrue()
    {
        // Arrange
        var orgId = 1;
        _mockOrgRepository.Setup(r => r.SoftDeleteAsync(orgId, CancellationToken))
            .ReturnsAsync(true);

        // Act
        var result = await _orgService.Delete(orgId, CancellationToken);

        // Assert
        result.Should().BeTrue();
        _mockOrgRepository.Verify(r => r.SoftDeleteAsync(orgId, CancellationToken), Times.Once);
    }
}
