using CrmBack.Data.Repositories;
using Dapper;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Dapper;
using System.Data;
using System.Data.Common;
using Xunit;

namespace Tests;

public class BaseRepositoryTests
{
    private readonly Mock<IDbConnection> _mockConnection;
    private readonly TestRepository _repository;

    public BaseRepositoryTests()
    {
        _mockConnection = new Mock<IDbConnection>();
        _repository = new TestRepository(_mockConnection.Object);
    }


    [Fact]
    public async Task QuerySingleAsync_EntityExists_ReturnsEntity()
    {
        // Arrange
        var expected = new TestEntity { Id = 1, Name = "Test" };
        _mockConnection
            .SetupDapperAsync(c => c.QuerySingleOrDefaultAsync<TestEntity>(It.IsAny<string>(), It.IsAny<object>(), null, null, null))
            .ReturnsAsync(expected);

        // Act
        var result = await _repository.TestQuerySingleAsync("SELECT * FROM test", 1);

        // Assert
        Assert.Equal(expected.Name, result?.Name);
    }

    [Fact]
    public async Task QuerySingleAsync_EntityNotFound_ReturnsNull()
    {
        // Arrange
        _mockConnection
            .SetupDapperAsync(c => c.QuerySingleOrDefaultAsync<TestEntity>(It.IsAny<string>(), It.IsAny<object>(), null, null, null))
            .ReturnsAsync((TestEntity?)null);

        // Act
        var result = await _repository.TestQuerySingleAsync("SELECT * FROM test", 1);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task QueryAsync_ReturnsEntities()
    {
        // Arrange
        var expected = new[] { new TestEntity { Id = 1 }, new TestEntity { Id = 2 } };
        _mockConnection
            .SetupDapperAsync(c => c.QueryAsync<TestEntity>(It.IsAny<string>(), It.IsAny<object>(), null, null, null))
            .ReturnsAsync(expected);

        // Act
        var result = await _repository.TestQueryAsync("SELECT * FROM test", null);

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task ExecuteAsync_RowsAffected_ReturnsTrue()
    {
        // Arrange
        _mockConnection
            .SetupDapperAsync(c => c.ExecuteAsync(It.IsAny<string>(), It.IsAny<object>(), null, null, null))
            .ReturnsAsync(1);

        // Act
        var result = await _repository.TestExecuteAsync("UPDATE test SET name = 'test'", new { });

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ExecuteAsync_NoRowsAffected_ReturnsFalse()
    {
        // Arrange
        _mockConnection
            .SetupDapperAsync(c => c.ExecuteAsync(It.IsAny<string>(), It.IsAny<object>(), null, null, null))
            .ReturnsAsync(0);

        // Act
        var result = await _repository.TestExecuteAsync("UPDATE test SET name = 'test'", new { });

        // Assert
        Assert.False(result);
    }
}

public class TestEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class TestException : Exception { }

public class TestRepository(IDbConnection dbConnection) : BaseRepository<TestEntity>(dbConnection)
{
    public Task<TestEntity?> TestQuerySingleAsync<TId>(string sql, TId id)
        => QuerySingleAsync(sql, id);

    public Task<IEnumerable<TestEntity>> TestQueryAsync(string sql, object? parameters = null)
        => QueryAsync(sql, parameters);

    public Task<int> TestExecuteScalarAsync(string sql, object entity)
        => ExecuteScalarAsync(sql, entity);

    public Task<bool> TestExecuteAsync(string sql, object parameters)
        => ExecuteAsync(sql, parameters);
}