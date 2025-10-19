using System.Data;
using CrmBack.Core.Models.Entities;
using CrmBack.Repository.Impl;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Xunit;

namespace Tests.Repository;

public class RepositoryTests
{
    [Fact]
    public void UserRepository_Constructor_ShouldCreateInstance()
    {
        // Arrange
        var mockDbConnection = new Mock<IDbConnection>();
        var mockCache = new Mock<IDistributedCache>();

        // Act
        var repository = new UserRepository(mockDbConnection.Object, mockCache.Object);

        // Assert
        repository.Should().NotBeNull();
        repository.Should().BeOfType<UserRepository>();
    }

    [Fact]
    public void ActivRepository_Constructor_ShouldCreateInstance()
    {
        // Arrange
        var mockDbConnection = new Mock<IDbConnection>();
        var mockCache = new Mock<IDistributedCache>();

        // Act
        var repository = new ActivRepository(mockDbConnection.Object, mockCache.Object);

        // Assert
        repository.Should().NotBeNull();
        repository.Should().BeOfType<ActivRepository>();
    }

    [Fact]
    public void OrgRepository_Constructor_ShouldCreateInstance()
    {
        // Arrange
        var mockDbConnection = new Mock<IDbConnection>();
        var mockCache = new Mock<IDistributedCache>();

        // Act
        var repository = new OrgRepository(mockDbConnection.Object, mockCache.Object);

        // Assert
        repository.Should().NotBeNull();
        repository.Should().BeOfType<OrgRepository>();
    }

    [Fact]
    public void PlanRepository_Constructor_ShouldCreateInstance()
    {
        // Arrange
        var mockDbConnection = new Mock<IDbConnection>();
        var mockCache = new Mock<IDistributedCache>();

        // Act
        var repository = new PlanRepository(mockDbConnection.Object, mockCache.Object);

        // Assert
        repository.Should().NotBeNull();
        repository.Should().BeOfType<PlanRepository>();
    }

    [Fact]
    public async Task BaseRepository_ShouldThrowArgumentException_WhenInvalidOrderByColumn()
    {
        // Arrange
        var mockDbConnection = new Mock<IDbConnection>();
        var mockCache = new Mock<IDistributedCache>();
        var repository = new TestRepository(mockDbConnection.Object, mockCache.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            repository.FindAllAsync(orderByColumn: "invalid_column"));
    }

    [Fact]
    public async Task BaseRepository_ShouldThrowArgumentException_WhenInvalidColumnInFindBy()
    {
        // Arrange
        var mockDbConnection = new Mock<IDbConnection>();
        var mockCache = new Mock<IDistributedCache>();
        var repository = new TestRepository(mockDbConnection.Object, mockCache.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            repository.FindByAsync("invalid_column", "test"));
    }

    [Fact]
    public async Task BaseRepository_ShouldThrowArgumentException_WhenNoMinOrMaxValueInFindByRange()
    {
        // Arrange
        var mockDbConnection = new Mock<IDbConnection>();
        var mockCache = new Mock<IDistributedCache>();
        var repository = new TestRepository(mockDbConnection.Object, mockCache.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            repository.FindByRangeAsync("usr_id"));
    }

    [Fact]
    public void UserEntity_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var user = new UserEntity(1, "John", "Middle", "Doe", "john.doe", "hash", false);

        // Assert
        user.usr_id.Should().Be(1);
        user.first_name.Should().Be("John");
        user.middle_name.Should().Be("Middle");
        user.last_name.Should().Be("Doe");
        user.login.Should().Be("john.doe");
        user.password_hash.Should().Be("hash");
        user.is_deleted.Should().BeFalse();
    }

    [Fact]
    public void ActivEntity_ShouldHaveCorrectProperties()
    {
        // Arrange
        var visitDate = DateTime.Today;
        var startTime = TimeSpan.FromHours(9);
        var endTime = TimeSpan.FromHours(17);

        // Act
        var activity = new ActivEntity(1, 1, 1, 1, visitDate, startTime, endTime, "Test activity", false);

        // Assert
        activity.activ_id.Should().Be(1);
        activity.usr_id.Should().Be(1);
        activity.org_id.Should().Be(1);
        activity.status_id.Should().Be(1);
        activity.visit_date.Should().Be(visitDate);
        activity.start_time.Should().Be(startTime);
        activity.end_time.Should().Be(endTime);
        activity.description.Should().Be("Test activity");
        activity.is_deleted.Should().BeFalse();
    }

    [Fact]
    public void OrgEntity_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var org = new OrgEntity(1, "Company A", "1234567890", 55.7558, 37.6176, "Moscow, Russia", false);

        // Assert
        org.org_id.Should().Be(1);
        org.name.Should().Be("Company A");
        org.inn.Should().Be("1234567890");
        org.latitude.Should().Be(55.7558);
        org.longitude.Should().Be(37.6176);
        org.address.Should().Be("Moscow, Russia");
        org.is_deleted.Should().BeFalse();
    }

    [Fact]
    public void PlanEntity_ShouldHaveCorrectProperties()
    {
        // Arrange
        var startDate = DateTime.Today;
        var endDate = DateTime.Today.AddDays(30);

        // Act
        var plan = new PlanEntity(1, 1, 1, startDate, endDate, false);

        // Assert
        plan.plan_id.Should().Be(1);
        plan.usr_id.Should().Be(1);
        plan.org_id.Should().Be(1);
        plan.start_date.Should().Be(startDate);
        plan.end_date.Should().Be(endDate);
        plan.is_deleted.Should().BeFalse();
    }

    [Fact]
    public void StatusEntity_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var status = new StatusEntity(1, "Pending", false);

        // Assert
        status.status_id.Should().Be(1);
        status.name.Should().Be("Pending");
        status.is_deleted.Should().BeFalse();
    }
}

// Тестовый репозиторий для тестирования BaseRepository
public class TestRepository(IDbConnection dbConnection, IDistributedCache cache) : BaseRepository<UserEntity, int>(dbConnection, cache)
{
}
