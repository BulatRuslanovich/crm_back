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

    [Fact]
    public async Task QuerySingleAsync_WhenEntityExists_ReturnsEntity()
    {
        // Arrange
        var expectedEntity = new TestEntity { Id = 1, Name = "Test" };
        _mockConnection
            .SetupDapperAsync(c => c.QuerySingleOrDefaultAsync<TestEntity>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<IDbTransaction>(),
                null,
                null))
            .ReturnsAsync(expectedEntity);

        // Act
        var result = await _repository.TestQuerySingleAsync("SELECT * FROM test WHERE id = @id", 1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Test", result.Name);
    }

    [Fact]
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
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("not found")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
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

    [Fact]
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
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    // [Fact]
    // public async Task ExecuteScalarAsync_ReturnsGeneratedId()
    // {
    //     // Arrange
    //     var expectedId = 123;
    //     var entity = new TestEntity { Name = "New Entity" };

    //     _mockConnection
    //         .SetupDapperAsync(c => c.ExecuteScalarAsync<int>(
    //             It.IsAny<string>(), 
    //             It.IsAny<object>(), 
    //             It.IsAny<IDbTransaction>(), 
    //             null, 
    //             null))
    //         .ReturnsAsync(expectedId);

    //     // Act
    //     var result = await _repository.TestExecuteScalarAsync("INSERT INTO test ... RETURNING id", entity);

    //     // Assert
    //     Assert.Equal(expectedId, result);
    //     _mockLogger.Verify(
    //         x => x.Log(
    //             LogLevel.Debug,
    //             It.IsAny<EventId>(),
    //             It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Created")),
    //             null,
    //             It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
    //         Times.Once);
    // }

    [Fact]
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

    [Fact]
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

    [Fact]
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

        // Assert
        Assert.Equal(42, result);
        mockTransaction.Verify(t => t.Commit(), Times.Once);
        mockTransaction.Verify(t => t.Rollback(), Times.Never);
        mockTransaction.Verify(t => t.Dispose(), Times.Once);
    }

    [Fact]
    public async Task WithTransactionAsync_WhenExceptionThrown_RollsBackTransaction()
    {
        // Arrange
        var mockTransaction = new Mock<IDbTransaction>();
        _mockConnection.Setup(c => c.BeginTransaction()).Returns(mockTransaction.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await _repository.TestWithTransactionAsync<int>(async (transaction) =>
            {
                await Task.Delay(1);
                throw new InvalidOperationException("Test exception");
            });
        });

        mockTransaction.Verify(t => t.Rollback(), Times.Once);
        mockTransaction.Verify(t => t.Commit(), Times.Never);
        mockTransaction.Verify(t => t.Dispose(), Times.Once);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Transaction failed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task LogSql_WhenLoggingDisabled_DoesNotLog()
    {
        // Arrange
        _mockOptions.Setup(x => x.Value).Returns(new DatabaseLoggingOptions
        {
            EnableDatabaseLogging = false
        });

        var repositoryWithoutLogging = new TestRepository(
            _mockConnection.Object,
            _mockLogger.Object,
            _mockOptions.Object);

        _mockConnection
            .SetupDapperAsync(c => c.QueryAsync<TestEntity>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<IDbTransaction>(),
                null,
                null))
            .ReturnsAsync(Enumerable.Empty<TestEntity>());

        // Act
        await repositoryWithoutLogging.TestQueryAsync("SELECT * FROM test", null);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("[SQL]")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    //     [Fact]
    //     public async Task QuerySingleAsync_WithTransaction_PassesTransactionToQuery()
    //     {
    //         // Arrange
    //         var mockTransaction = new Mock<IDbTransaction>();
    //         var expectedEntity = new TestEntity { Id = 1, Name = "Test" };

    //         _mockConnection
    //             .SetupDapperAsync(c => c.QuerySingleOrDefaultAsync<TestEntity>(
    //                 It.IsAny<string>(), 
    //                 It.IsAny<object>(), 
    //                 mockTransaction.Object, 
    //                 null, 
    //                 null))
    //             .ReturnsAsync(expectedEntity);

    //         // Act
    //         var result = await _repository.TestQuerySingleAsync("SELECT * FROM test WHERE id = @id", 1, mockTransaction.Object);

    //         // Assert
    //         Assert.NotNull(result);
    //         _mockConnection.Verify(
    //             c => c.QuerySingleOrDefaultAsync<TestEntity>(
    //                 It.IsAny<string>(), 
    //                 It.IsAny<object>(), 
    //                 mockTransaction.Object, 
    //                 null, 
    //                 null),
    //             Times.Once);
    //     }
}

// Test helper classes
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
