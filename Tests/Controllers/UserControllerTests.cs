namespace Tests.Controllers;

using CrmBack.Api.Controllers;
using CrmBack.Core.Models.Payload.User;
using CrmBack.Core.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

public class UserControllerTests
{
    private readonly Mock<IUserService> _userServiceMock;
    private readonly UserController _controller;

    public UserControllerTests()
    {
        _userServiceMock = new Mock<IUserService>();
        _controller = new UserController(_userServiceMock.Object);
    }

    [Fact]
    public async Task GetById_UserExists_ReturnsOkResult()
    {
        // Arrange
        var userId = 1;
        var userPayload = new ReadUserPayload(userId, "Lol", "Lol2", "LOl3", "LOL");
        _userServiceMock
            .Setup(service => service.GetUserById(userId))
            .ReturnsAsync(userPayload);

        // Act
        var result = await _controller.GetById(userId);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(userPayload);
        _userServiceMock.VerifyAll();
    }

    [Fact]
    public async Task GetById_UserNotFound_ReturnsNotFound()
    {
        // Arrange
        var userId = 1;
        _userServiceMock
            .Setup(service => service.GetUserById(userId))
            .ReturnsAsync((ReadUserPayload?)null);

        // Act
        var result = await _controller.GetById(userId);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
        _userServiceMock.VerifyAll();
    }


    [Fact]
    public async Task GetAllUsers_UsersExist_ReturnsOkResult()
    {
        // Arrange
        var users = new List<ReadUserPayload> { new(1, "Lol", "Lol2", "LOl3", "LOL") };
        _userServiceMock
            .Setup(service => service.GetAllUsers())
            .ReturnsAsync(users);

        // Act
        var result = await _controller.GetAll();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(users);
    }

    [Fact]
    public async Task GetAllUsers_NoUsers_ReturnsNotFound()
    {
        // Arrange
        _userServiceMock
            .Setup(service => service.GetAllUsers())
            .ReturnsAsync([]);

        // Act
        var result = await _controller.GetAll();

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }


    [Fact]
    public async Task Create_ValidPayload_ReturnsCreatedResult()
    {
        // Arrange
        var createPayload = new CreateUserPayload("Lol", "Lol2", "LOl3", "LOL", "1234");
        var readPayload = new ReadUserPayload(1, "Lol", "Lol2", "LOl3", "LOL"); ;
        _userServiceMock
            .Setup(service => service.CreateUser(createPayload))
            .ReturnsAsync(readPayload);

        // Act
        var result = await _controller.Create(createPayload);

        // Assert
        result.Result.Should().BeOfType<CreatedAtActionResult>()
            .Which.Value.Should().BeEquivalentTo(readPayload);
    }


    [Fact]
    public async Task DeleteUser_UserExists_ReturnsNoContent()
    {
        // Arrange
        var userId = 1;
        _userServiceMock
            .Setup(service => service.DeleteUser(userId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(userId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteUser_UserNotFound_ReturnsNotFound()
    {
        // Arrange
        var userId = 1;
        _userServiceMock
            .Setup(service => service.DeleteUser(userId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(userId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }
}
