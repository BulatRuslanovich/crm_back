using CrmBack.Core.Models.Entities;
using CrmBack.Core.Models.Payload.Plan;
using CrmBack.Repository;
using CrmBack.Services.Impl;
using FluentAssertions;
using Moq;

namespace CrmBack.Tests.Services;

public class PlanServiceTests : BaseServiceTest
{
    private readonly Mock<IPlanRepository> _mockPlanRepository;
    private readonly PlanService _planService;

    public PlanServiceTests()
    {
        _mockPlanRepository = new Mock<IPlanRepository>();
        _planService = new PlanService(_mockPlanRepository.Object);
    }

    [Fact]
    public async Task GetById_WhenPlanExists_ReturnsPlanPayload()
    {
        // Arrange
        var planId = 1;
        var planEntity = new PlanEntity(1, 1, 1, DateTime.Now, DateTime.Now.AddDays(1), false);
        _mockPlanRepository.Setup(r => r.GetByIdAsync(planId, CancellationToken))
            .ReturnsAsync(planEntity);

        // Act
        var result = await _planService.GetById(planId, CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result!.PlanId.Should().Be(planId);
        result.UsrId.Should().Be(1);
        result.OrgId.Should().Be(1);
    }

    [Fact]
    public async Task GetById_WhenPlanDoesNotExist_ReturnsNull()
    {
        // Arrange
        var planId = 1;
        _mockPlanRepository.Setup(r => r.GetByIdAsync(planId, CancellationToken))
            .ReturnsAsync((PlanEntity?)null);

        // Act
        var result = await _planService.GetById(planId, CancellationToken);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAll_ReturnsListOfPlans()
    {
        // Arrange
        var plans = new List<PlanEntity>
        {
            new(1, 1, 1, DateTime.Now, DateTime.Now.AddDays(1), false),
            new(2, 1, 2, DateTime.Now.AddDays(1), DateTime.Now.AddDays(2), false)
        };
        _mockPlanRepository.Setup(r => r.GetAllAsync(false, 1, 10, CancellationToken))
            .ReturnsAsync(plans);

        // Act
        var result = await _planService.GetAll(false, 1, 10, CancellationToken);

        // Assert
        result.Should().HaveCount(2);
        result[0].PlanId.Should().Be(1);
        result[1].PlanId.Should().Be(2);
    }

    [Fact]
    public async Task Create_ReturnsCreatedPlan()
    {
        // Arrange
        var createPayload = new CreatePlanPayload(1, 1, DateTime.Now, DateTime.Now.AddDays(1));
        var planId = 1;
        var createdPlan = new PlanEntity(1, 1, 1, DateTime.Now, DateTime.Now.AddDays(1), false);

        _mockPlanRepository.Setup(r => r.CreateAsync(It.IsAny<PlanEntity>(), CancellationToken))
            .ReturnsAsync(planId);
        _mockPlanRepository.Setup(r => r.GetByIdAsync(planId, CancellationToken))
            .ReturnsAsync(createdPlan);

        // Act
        var result = await _planService.Create(createPayload, CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result!.UsrId.Should().Be(1);
        result.OrgId.Should().Be(1);
        _mockPlanRepository.Verify(r => r.CreateAsync(It.IsAny<PlanEntity>(), CancellationToken), Times.Once);
    }

    [Fact]
    public async Task Update_WhenPlanExists_ReturnsTrue()
    {
        // Arrange
        var planId = 1;
        var existingPlan = new PlanEntity(1, 1, 1, DateTime.Now, DateTime.Now.AddDays(1), false);
        var updatePayload = new UpdatePlanPayload(2, 2, DateTime.Now.AddDays(1), DateTime.Now.AddDays(2));

        _mockPlanRepository.Setup(r => r.GetByIdAsync(planId, CancellationToken))
            .ReturnsAsync(existingPlan);
        _mockPlanRepository.Setup(r => r.UpdateAsync(It.IsAny<PlanEntity>(), CancellationToken))
            .ReturnsAsync(true);

        // Act
        var result = await _planService.Update(planId, updatePayload, CancellationToken);

        // Assert
        result.Should().BeTrue();
        _mockPlanRepository.Verify(r => r.UpdateAsync(It.IsAny<PlanEntity>(), CancellationToken), Times.Once);
    }

    [Fact]
    public async Task Update_WhenPlanDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var planId = 1;
        var updatePayload = new UpdatePlanPayload(2, 2, DateTime.Now.AddDays(1), DateTime.Now.AddDays(2));

        _mockPlanRepository.Setup(r => r.GetByIdAsync(planId, CancellationToken))
            .ReturnsAsync((PlanEntity?)null);

        // Act
        var result = await _planService.Update(planId, updatePayload, CancellationToken);

        // Assert
        result.Should().BeFalse();
        _mockPlanRepository.Verify(r => r.UpdateAsync(It.IsAny<PlanEntity>(), CancellationToken), Times.Never);
    }

    [Fact]
    public async Task Update_WithPartialData_UpdatesOnlyProvidedFields()
    {
        // Arrange
        var planId = 1;
        var existingPlan = new PlanEntity(1, 1, 1, DateTime.Now, DateTime.Now.AddDays(1), false);
        var updatePayload = new UpdatePlanPayload(2, null, null, null);

        _mockPlanRepository.Setup(r => r.GetByIdAsync(planId, CancellationToken))
            .ReturnsAsync(existingPlan);
        _mockPlanRepository.Setup(r => r.UpdateAsync(It.IsAny<PlanEntity>(), CancellationToken))
            .ReturnsAsync(true);

        // Act
        var result = await _planService.Update(planId, updatePayload, CancellationToken);

        // Assert
        result.Should().BeTrue();
        _mockPlanRepository.Verify(r => r.UpdateAsync(It.Is<PlanEntity>(e =>
            e.usr_id == 2 &&
            e.org_id == 1 &&
            e.start_date == existingPlan.start_date &&
            e.end_date == existingPlan.end_date
        ), CancellationToken), Times.Once);
    }

    [Fact]
    public async Task Delete_ReturnsTrue()
    {
        // Arrange
        var planId = 1;
        _mockPlanRepository.Setup(r => r.SoftDeleteAsync(planId, CancellationToken))
            .ReturnsAsync(true);

        // Act
        var result = await _planService.Delete(planId, CancellationToken);

        // Assert
        result.Should().BeTrue();
        _mockPlanRepository.Verify(r => r.SoftDeleteAsync(planId, CancellationToken), Times.Once);
    }
}
