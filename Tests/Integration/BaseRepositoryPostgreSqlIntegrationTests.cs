using CrmBack.Core.Models.Entities;
using Dapper;
using FluentAssertions;
using Xunit;

namespace Tests.Integration;

public class BaseRepositoryPostgreSqlIntegrationTests : BasePostgreSqlIntegrationTest
{
    [Fact]
    public async Task CreateAsync_ShouldCreateEntityAndReturnId()
    {
        // Arrange
        var user = new UserEntity(0, "John", "Middle", "Doe", "johndoe", "hashed_password", false);

        // Act
        var userId = await UserRepository.CreateAsync(user);

        // Assert
        userId.Should().BeGreaterThan(0);
    }


    [Fact]
    public async Task GetAllAsync_ShouldReturnAllEntities()
    {
        // Arrange
        await CreateTestUserAsync("user1");
        await CreateTestUserAsync("user2");

        // Act
        var users = await UserRepository.GetAllAsync();

        // Assert
        users.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateEntity()
    {
        // Arrange
        var user = await CreateTestUserAsync();
        var updatedUser = user with { first_name = "Updated Name" };

        // Act
        var result = await UserRepository.UpdateAsync(updatedUser);

        // Assert
        result.Should().BeTrue();

        var retrievedUser = await UserRepository.GetByIdAsync(user.usr_id);
        retrievedUser!.first_name.Should().Be("Updated Name");
    }

    [Fact]
    public async Task SoftDeleteAsync_ShouldMarkEntityAsDeleted()
    {
        // Arrange
        var user = await CreateTestUserAsync();

        // Act
        var result = await UserRepository.SoftDeleteAsync(user.usr_id);

        // Assert
        result.Should().BeTrue();

        var retrievedUser = await UserRepository.GetByIdAsync(user.usr_id);
        retrievedUser.Should().BeNull();
    }

    [Fact]
    public async Task HardDeleteAsync_ShouldDeleteEntity()
    {
        // Arrange
        var user = await CreateTestUserAsync();

        // Act
        var result = await UserRepository.HardDeleteAsync(user.usr_id);

        // Assert
        result.Should().BeTrue();

        var retrievedUser = await UserRepository.GetByIdAsync(user.usr_id);
        retrievedUser.Should().BeNull();
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnTrueForExistingEntity()
    {
        // Arrange
        var user = await CreateTestUserAsync();

        // Act
        var exists = await UserRepository.ExistsAsync(user.usr_id);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task CountAsync_ShouldReturnCorrectCount()
    {
        // Arrange
        await CreateTestUserAsync("user1");
        await CreateTestUserAsync("user2");

        // Act
        var count = await UserRepository.CountAsync();

        // Assert
        count.Should().Be(2);
    }

    [Fact]
    public async Task FindByAsync_ShouldReturnMatchingEntities()
    {
        // Arrange
        await CreateTestUserAsync("alice", "Alice", "Smith");
        await CreateTestUserAsync("bob", "Bob", "Johnson");

        // Act
        var users = await UserRepository.FindByAsync("first_name", "Alice");

        // Assert
        users.Should().HaveCount(1);
        users.First().first_name.Should().Be("Alice");
    }

    [Fact]
    public async Task FindAllAsync_ShouldReturnEntitiesOrderedByColumn()
    {
        // Arrange
        await CreateTestUserAsync("charlie", "Charlie", "Brown");
        await CreateTestUserAsync("alice", "Alice", "Smith");
        await CreateTestUserAsync("bob", "Bob", "Johnson");

        // Act
        var users = await UserRepository.FindAllAsync(orderByColumn: "login");

        // Assert
        users.Should().HaveCount(3);
        var usersList = users.ToList();
        usersList[0].login.Should().Be("alice");
        usersList[1].login.Should().Be("bob");
        usersList[2].login.Should().Be("charlie");
    }

    [Fact]
    public async Task FindByRangeAsync_ShouldReturnEntitiesInRange()
    {
        // Arrange
        await CreateTestUserAsync("user1");
        await CreateTestUserAsync("user2");
        await CreateTestUserAsync("user3");

        // Act
        var users = await UserRepository.FindByRangeAsync("usr_id", 0, 2);

        // Assert
        users.Should().HaveCount(2);
    }

    [Fact]
    public async Task FindByAsync_WithExactMatchFalse_ShouldReturnPartialMatches()
    {
        // Arrange
        await CreateTestUserAsync("alice", "Alice", "Smith");
        await CreateTestUserAsync("bob", "Bob", "Johnson");

        // Act
        var users = await UserRepository.FindByAsync("first_name", "Ali", exactMatch: false);

        // Assert
        users.Should().HaveCount(1);
        users.First().first_name.Should().Be("Alice");
    }

    [Fact]
    public async Task GetAllAsync_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        await CreateTestUserAsync("user1");
        await CreateTestUserAsync("user2");
        await CreateTestUserAsync("user3");

        // Act
        var users = await UserRepository.GetAllAsync(page: 1, pageSize: 2);

        // Assert
        users.Should().HaveCount(2);
    }

    [Fact]
    public async Task FindAllAsync_WithFilters_ShouldReturnFilteredResults()
    {
        // Arrange
        await CreateTestUserAsync("alice", "Alice", "Smith");
        await CreateTestUserAsync("bob", "Bob", "Johnson");

        // Act
        var filters = new Dictionary<string, object> { { "first_name", "Alice" } };
        var users = await UserRepository.FindAllAsync(filters: filters);

        // Assert
        users.Should().HaveCount(1);
        users.First().first_name.Should().Be("Alice");
    }

    [Fact]
    public async Task FindAllAsync_WithIncludeDeleted_ShouldReturnDeletedEntities()
    {
        // Arrange
        var user = await CreateTestUserAsync();
        await UserRepository.SoftDeleteAsync(user.usr_id);

        // Act
        var users = await UserRepository.FindAllAsync(includeDeleted: true);

        // Assert
        users.Should().HaveCount(1);
        users.First().usr_id.Should().Be(user.usr_id);
    }

    [Fact]
    public async Task FindAllAsync_WithOrderByDescending_ShouldReturnDescendingOrder()
    {
        // Arrange
        await CreateTestUserAsync("alice", "Alice", "Smith");
        await CreateTestUserAsync("bob", "Bob", "Johnson");
        await CreateTestUserAsync("charlie", "Charlie", "Brown");

        // Act
        var users = await UserRepository.FindAllAsync(orderByColumn: "login", orderByDescending: true);

        // Assert
        users.Should().HaveCount(3);
        var usersList = users.ToList();
        usersList[0].login.Should().Be("charlie");
        usersList[1].login.Should().Be("bob");
        usersList[2].login.Should().Be("alice");
    }

    [Fact]
    public async Task FindByAsync_WithNonExistentColumn_ShouldThrowArgumentException()
    {
        // Arrange
        await CreateTestUserAsync();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            UserRepository.FindByAsync("non_existent_column", "value"));
    }

    [Fact]
    public async Task FindAllAsync_WithNonExistentOrderByColumn_ShouldThrowArgumentException()
    {
        // Arrange
        await CreateTestUserAsync();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            UserRepository.FindAllAsync(orderByColumn: "non_existent_column"));
    }
}
