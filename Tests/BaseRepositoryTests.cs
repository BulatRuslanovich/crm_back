namespace Tests;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using CrmBack.Core.Config;
using CrmBack.Data.Repositories;
using Dapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Dapper;
using Xunit;

[Trait("Category", "Unit")]
[Trait("Layer", "Repository")]
public class BaseRepositoryTests
{
    private readonly Mock<IDbConnection> _mockConnection;
    private readonly Mock<ILogger<TestRepository>> _mockLogger;
    private readonly Mock<IOptions<DatabaseLoggingOptions>> _mockOptions;
    private readonly TestRepository _repository;

    public BaseRepositoryTests()
    {
        _mockConnection = new Mock<IDbConnection>();
        _mockLogger = new Mock<ILogger<TestRepository>>();
        _mockOptions = new Mock<IOptions<DatabaseLoggingOptions>>();

        _mockOptions.Setup(x => x.Value).Returns(new DatabaseLoggingOptions
        {
            EnableDatabaseLogging = true
        });

        _repository = new TestRepository(
            _mockConnection.Object,
            _mockLogger.Object,
            _mockOptions.Object);
    }

    [Fact(DisplayName = "QuerySingleAsync should return entity when found")]
    [Trait("Method", "QuerySingleAsync")]
    public async Task QuerySingleAsync_WhenEntityExists_ReturnsEntity()
    {
        // Arrange - prepare test data
        var expectedEntity = new TestEntity { Id = 1, Name = "Test" };

        _mockConnection
            .SetupDapperAsync(c => c.QuerySingleOrDefaultAsync<TestEntity>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<IDbTransaction>(),
                null,
                null))
            .ReturnsAsync(expectedEntity);

        // Act - call the method under test
        var result = await _repository.TestQuerySingleAsync("SELECT * FROM test WHERE id = @id", 1);

        // Assert - verify the result
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Test", result.Name);
    }

    [Fact(DisplayName = "QuerySingleAsync should return null when entity not found")]
    [Trait("Method", "QuerySingleAsync")]
    public async Task QuerySingleAsync_WhenEntityNotFound_ReturnsNull()
    {
        // Arrange
        _mockConnection
            .SetupDapperAsync(c => c.QuerySingleOrDefaultAsync<TestEntity>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<IDbTransaction>(),
                null,
                null))
            .ReturnsAsync((TestEntity?)null);

        // Act
        var result = await _repository.TestQuerySingleAsync("SELECT * FROM test WHERE id = @id", 999);

        // Assert
        Assert.Null(result);
    }

    [Fact(DisplayName = "QueryAsync should return collection of entities")]
    [Trait("Method", "QueryAsync")]
    public async Task QueryAsync_WhenEntitiesExist_ReturnsCollection()
    {
        // Arrange
        var expectedEntities = new List<TestEntity>
        {
            new() { Id = 1, Name = "Test1" },
            new() { Id = 2, Name = "Test2" }
        };

        _mockConnection
            .SetupDapperAsync(c => c.QueryAsync<TestEntity>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<IDbTransaction>(),
                null,
                null))
            .ReturnsAsync(expectedEntities);

        // Act
        var result = await _repository.TestQueryAsync("SELECT * FROM test", null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact(DisplayName = "QueryAsync should return empty collection when no data")]
    [Trait("Method", "QueryAsync")]
    public async Task QueryAsync_WhenNoEntities_ReturnsEmptyCollection()
    {
        // Arrange
        _mockConnection
            .SetupDapperAsync(c => c.QueryAsync<TestEntity>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<IDbTransaction>(),
                null,
                null))
            .ReturnsAsync(Enumerable.Empty<TestEntity>());

        // Act
        var result = await _repository.TestQueryAsync("SELECT * FROM test", null);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact(DisplayName = "ExecuteAsync should return true when rows affected")]
    [Trait("Method", "ExecuteAsync")]
    public async Task ExecuteAsync_WhenRowsAffected_ReturnsTrue()
    {
        // Arrange
        _mockConnection
            .SetupDapperAsync(c => c.ExecuteAsync(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<IDbTransaction>(),
                null,
                null))
            .ReturnsAsync(1);

        // Act
        var result = await _repository.TestExecuteAsync("UPDATE test SET name = @name", new { name = "Updated" });

        // Assert
        Assert.True(result);
    }

    [Fact(DisplayName = "ExecuteAsync should return false when no rows affected")]
    [Trait("Method", "ExecuteAsync")]
    public async Task ExecuteAsync_WhenNoRowsAffected_ReturnsFalse()
    {
        // Arrange
        _mockConnection
            .SetupDapperAsync(c => c.ExecuteAsync(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<IDbTransaction>(),
                null,
                null))
            .ReturnsAsync(0);

        // Act
        var result = await _repository.TestExecuteAsync("UPDATE test SET name = @name WHERE id = 999", new { name = "Updated" });

        // Assert
        Assert.False(result);
    }

    [Fact(DisplayName = "WithTransactionAsync should commit transaction on success")]
    [Trait("Method", "WithTransactionAsync")]
    public async Task WithTransactionAsync_WhenSuccessful_CommitsTransaction()
    {
        // Arrange
        var mockTransaction = new Mock<IDbTransaction>();
        _mockConnection.Setup(c => c.BeginTransaction()).Returns(mockTransaction.Object);

        // Act
        var result = await _repository.TestWithTransactionAsync(async (transaction) =>
        {
            await Task.Delay(1);
            return 42;
        });

        // Assert - verify result
        Assert.Equal(42, result);

        // Verify transaction behavior
        mockTransaction.Verify(t => t.Commit(), Times.Once);
        mockTransaction.Verify(t => t.Rollback(), Times.Never);
    }

    [Fact(DisplayName = "WithTransactionAsync should rollback transaction on exception")]
    [Trait("Method", "WithTransactionAsync")]
    public async Task WithTransactionAsync_WhenExceptionThrown_RollsBackTransaction()
    {
        // Arrange
        var mockTransaction = new Mock<IDbTransaction>();
        _mockConnection.Setup(c => c.BeginTransaction()).Returns(mockTransaction.Object);

        // Act & Assert - verify exception is thrown
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await _repository.TestWithTransactionAsync<int>(async (transaction) =>
            {
                await Task.Delay(1);
                throw new InvalidOperationException("Test exception");
            });
        });

        // Assert - verify transaction behavior
        mockTransaction.Verify(t => t.Rollback(), Times.Once);
        mockTransaction.Verify(t => t.Commit(), Times.Never);
    }

    [Fact(DisplayName = "QueryAsync with parameters should return correct results")]
    [Trait("Method", "QueryAsync")]
    public async Task QueryAsync_WithParameters_ReturnsCorrectResults()
    {
        // Arrange
        var expectedEntities = new List<TestEntity>
        {
            new() { Id = 5, Name = "FilteredTest" }
        };

        _mockConnection
            .SetupDapperAsync(c => c.QueryAsync<TestEntity>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<IDbTransaction>(),
                null,
                null))
            .ReturnsAsync(expectedEntities);

        // Act
        var result = await _repository.TestQueryAsync(
            "SELECT * FROM test WHERE id > @minId",
            new { minId = 3 });

        // Assert
        Assert.Single(result);
        Assert.Equal(5, result.First().Id);
        Assert.Equal("FilteredTest", result.First().Name);
    }

    [Fact(DisplayName = "ExecuteAsync with multiple rows affected should return true")]
    [Trait("Method", "ExecuteAsync")]
    public async Task ExecuteAsync_WithMultipleRowsAffected_ReturnsTrue()
    {
        // Arrange
        _mockConnection
            .SetupDapperAsync(c => c.ExecuteAsync(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<IDbTransaction>(),
                null,
                null))
            .ReturnsAsync(10); // 10 rows affected

        // Act
        var result = await _repository.TestExecuteAsync("DELETE FROM test WHERE status = @status", new { status = "inactive" });

        // Assert
        Assert.True(result);
    }

    [Fact(DisplayName = "ExecuteScalarAsync should return generated ID")]
    [Trait("Method", "ExecuteScalarAsync")]
    public async Task ExecuteScalarAsync_ReturnsGeneratedId()
    {
        // Arrange
        _mockConnection
            .SetupDapperAsync(c => c.ExecuteScalarAsync<int>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                null,
                null,
                null))
            .ReturnsAsync(123); // Simulate generated ID

        // Act
        var result = await _repository.TestExecuteScalarAsync(
            "INSERT INTO test (name) VALUES (@name) RETURNING id",
            new { name = "NewEntity" });

        // Assert
        Assert.Equal(123, result);
    }
}

// Helper classes for testing

public class TestEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class TestRepository(
    IDbConnection dbConnection,
    ILogger<TestRepository> logger,
    IOptions<DatabaseLoggingOptions> options) : BaseRepository<TestEntity>(dbConnection, logger, options)
{
    public Task<TestEntity?> TestQuerySingleAsync<TId>(string sql, TId id, IDbTransaction? transaction = null)
        => QuerySingleAsync(sql, id, transaction);

    public Task<IEnumerable<TestEntity>> TestQueryAsync(string sql, object? parameters = null)
        => QueryAsync(sql, parameters);

    public Task<int> TestExecuteScalarAsync(string sql, object entity)
        => ExecuteScalarAsync(sql, entity);

    public Task<bool> TestExecuteAsync(string sql, object parameters, IDbTransaction? transaction = null)
        => ExecuteAsync(sql, parameters, transaction);

    public Task<TResult> TestWithTransactionAsync<TResult>(Func<IDbTransaction, Task<TResult>> action)
        => WithTransactionAsync(action);
}
