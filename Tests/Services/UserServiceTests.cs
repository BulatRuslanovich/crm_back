namespace Tests.Services;

using Xunit;
using Moq;
using FluentAssertions;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using CrmBack.Services;
using CrmBack.Core.Repositories;
using CrmBack.Core.Models.Payload.User;
using CrmBack.Core.Models.Entities;
using System.Data.SqlTypes;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _userService = new UserService(_mockUserRepository.Object);
    }

    [Fact]
    public async Task GetUserById_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        var userId = 1;
        var userEntity = new UserEntity(
            usr_id: userId,
            first_name: "John",
            middle_name: "Michael",
            last_name: "Doe",
            login: "johndoe",
            password_hash: "hashed_password"
        );

        _mockUserRepository.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(userEntity);

        // Act
        var result = await _userService.GetUserById(userId).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(userId);
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        result.MiddleName.Should().Be("Michael");
        result.Login.Should().Be("johndoe");

        _mockUserRepository.Verify(r => r.GetByIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task GetUserById_ShouldThrowException_WhenUserNotFound()
    {
        // Arrange
        var userId = 999;
        _mockUserRepository.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync((UserEntity?)null);

        // Act & Assert
        var res = await _userService.GetUserById(userId);

        res.Should().BeNull();

        _mockUserRepository.Verify(r => r.GetByIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task GetAllUsers_ShouldReturnUsers_WhenUsersExist()
    {
        // Arrange
        var userEntities = new List<UserEntity>
            {
                new UserEntity(
                    usr_id: 1,
                    first_name: "John",
                    middle_name: "Michael",
                    last_name: "Doe",
                    login: "johndoe",
                    password_hash: "hashed_password1"
                ),
                new UserEntity(
                    usr_id: 2,
                    first_name: "Jane",
                    middle_name: "Elizabeth",
                    last_name: "Smith",
                    login: "janesmith",
                    password_hash: "hashed_password2"
                )
            };

        _mockUserRepository.Setup(r => r.GetAllAsync(false))
            .ReturnsAsync(userEntities);

        // Act
        var result = await _userService.GetAllUsers().ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        var firstUser = result.First();
        firstUser.Id.Should().Be(1);
        firstUser.FirstName.Should().Be("John");
        firstUser.LastName.Should().Be("Doe");

        _mockUserRepository.Verify(r => r.GetAllAsync(false), Times.Once);
    }

    [Fact]
    public async Task GetAllUsers_ShouldThrowException_WhenNoUsersExist()
    {
        // Arrange
        _mockUserRepository.Setup(r => r.GetAllAsync(false))
            .ReturnsAsync([]);

        // Act & Assert
        var result = await _userService.GetAllUsers();

        result.Should().BeEmpty();

        _mockUserRepository.Verify(r => r.GetAllAsync(false), Times.Once);
    }

    [Fact]
    public async Task CreateUser_ShouldReturnCreatedUser_WhenUserIsValid()
    {
        // Arrange
        var createPayload = new CreateUserPayload(
            FirstName: "John",
            LastName: "Doe",
            MiddleName: "Michael",
            Login: "johndoe",
            Password: "password123"
        );

        var userId = 1;
        var createdUserEntity = new UserEntity(
            usr_id: userId,
            first_name: "John",
            middle_name: "Michael",
            last_name: "Doe",
            login: "johndoe",
            password_hash: "hashed_password"
        );

        _mockUserRepository.Setup(r => r.CreateAsync(It.IsAny<UserEntity>()))
            .ReturnsAsync(userId);

        _mockUserRepository.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(createdUserEntity);

        // Act
        var result = await _userService.CreateUser(createPayload).ConfigureAwait(false);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(userId);
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        result.MiddleName.Should().Be("Michael");
        result.Login.Should().Be("johndoe");

        _mockUserRepository.Verify(r => r.CreateAsync(It.IsAny<UserEntity>()), Times.Once);
        _mockUserRepository.Verify(r => r.GetByIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task UpdateUser_ShouldReturnTrue_WhenUserIsUpdated()
    {
        // Arrange
        var userId = 1;
        var updatePayload = new UpdateUserPayload(
            FirstName: "John",
            LastName: "Updated",
            MiddleName: "Michael",
            Login: "johndoe",
            Password: "newpassword"
        );

        _mockUserRepository.Setup(r => r.UpdateAsync(It.IsAny<UserEntity>()))
            .ReturnsAsync(true);

        // Act
        var result = await _userService.UpdateUser(userId, updatePayload).ConfigureAwait(false);

        // Assert
        result.Should().BeTrue();

        _mockUserRepository.Verify(r => r.UpdateAsync(It.Is<UserEntity>(u =>
            u.usr_id == userId &&
            u.first_name == "John" &&
            u.last_name == "Updated")), Times.Once);
    }

    [Fact]
    public async Task DeleteUser_ShouldReturnTrue_WhenUserIsSoftDeleted()
    {
        // Arrange
        var userId = 1;

        _mockUserRepository.Setup(r => r.SoftDeleteAsync(userId))
            .ReturnsAsync(true);

        // Act
        var result = await _userService.DeleteUser(userId).ConfigureAwait(false);

        // Assert
        result.Should().BeTrue();

        _mockUserRepository.Verify(r => r.SoftDeleteAsync(userId), Times.Once);
    }
}