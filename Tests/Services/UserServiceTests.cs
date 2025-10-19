using CrmBack.Core.Models.Entities;
using CrmBack.Core.Models.Payload.Activ;
using CrmBack.Core.Models.Payload.Plan;
using CrmBack.Core.Models.Payload.User;
using CrmBack.Repository;
using CrmBack.Services.Impl;
using FluentAssertions;
using Moq;

namespace CrmBack.Tests.Services;

public class UserServiceTests : BaseServiceTest
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IActivRepository> _mockActivRepository;
    private readonly Mock<IPlanRepository> _mockPlanRepository;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockActivRepository = new Mock<IActivRepository>();
        _mockPlanRepository = new Mock<IPlanRepository>();
        _userService = new UserService(_mockUserRepository.Object, _mockActivRepository.Object, _mockPlanRepository.Object, MockConfiguration.Object);
    }

    [Fact]
    public async Task GetById_WhenUserExists_ReturnsUserPayload()
    {
        // Arrange
        var userId = 1;
        var userEntity = new UserEntity(1, "John", "Doe", "Smith", "john.doe", "hashed_password", false);
        _mockUserRepository.Setup(r => r.GetByIdAsync(userId, CancellationToken))
            .ReturnsAsync(userEntity);

        // Act
        var result = await _userService.GetById(userId, CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(userId);
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Smith");
        result.MiddleName.Should().Be("Doe");
        result.Login.Should().Be("john.doe");
    }

    [Fact]
    public async Task GetById_WhenUserDoesNotExist_ReturnsNull()
    {
        // Arrange
        var userId = 1;
        _mockUserRepository.Setup(r => r.GetByIdAsync(userId, CancellationToken))
            .ReturnsAsync((UserEntity?)null);

        // Act
        var result = await _userService.GetById(userId, CancellationToken);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAll_ReturnsListOfUsers()
    {
        // Arrange
        var users = new List<UserEntity>
        {
            new(1, "John", "Doe", "Smith", "john.doe", "hashed_password", false),
            new(2, "Jane", "Smith", "Doe", "jane.smith", "hashed_password", false)
        };
        _mockUserRepository.Setup(r => r.GetAllAsync(false, 1, 10, CancellationToken))
            .ReturnsAsync(users);

        // Act
        var result = await _userService.GetAll(false, 1, 10, CancellationToken);

        // Assert
        result.Should().HaveCount(2);
        result[0].FirstName.Should().Be("John");
        result[1].FirstName.Should().Be("Jane");
    }

    [Fact]
    public async Task Create_ReturnsCreatedUser()
    {
        // Arrange
        var createPayload = new CreateUserPayload("John", "Doe", "Smith", "john.doe", "password123");
        var userId = 1;
        var createdUser = new UserEntity(1, "John", "Doe", "Smith", "john.doe", "hashed_password", false);

        _mockUserRepository.Setup(r => r.CreateAsync(It.IsAny<UserEntity>(), CancellationToken))
            .ReturnsAsync(userId);
        _mockUserRepository.Setup(r => r.GetByIdAsync(userId, CancellationToken))
            .ReturnsAsync(createdUser);

        // Act
        var result = await _userService.Create(createPayload, CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result!.FirstName.Should().Be("John");
        result.LastName.Should().Be("Smith");
        _mockUserRepository.Verify(r => r.CreateAsync(It.IsAny<UserEntity>(), CancellationToken), Times.Once);
    }

    [Fact]
    public async Task Update_WhenUserExists_ReturnsTrue()
    {
        // Arrange
        var userId = 1;
        var existingUser = new UserEntity(1, "John", "Doe", "Smith", "john.doe", "hashed_password", false);
        var updatePayload = new UpdateUserPayload("JohnUpdated", "DoeUpdated", "SmithUpdated", null, null);

        _mockUserRepository.Setup(r => r.GetByIdAsync(userId, CancellationToken))
            .ReturnsAsync(existingUser);
        _mockUserRepository.Setup(r => r.UpdateAsync(It.IsAny<UserEntity>(), CancellationToken))
            .ReturnsAsync(true);

        // Act
        var result = await _userService.Update(userId, updatePayload, CancellationToken);

        // Assert
        result.Should().BeTrue();
        _mockUserRepository.Verify(r => r.UpdateAsync(It.IsAny<UserEntity>(), CancellationToken), Times.Once);
    }

    [Fact]
    public async Task Update_WhenUserDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var userId = 1;
        var updatePayload = new UpdateUserPayload("JohnUpdated", "DoeUpdated", "SmithUpdated", null, null);

        _mockUserRepository.Setup(r => r.GetByIdAsync(userId, CancellationToken))
            .ReturnsAsync((UserEntity?)null);

        // Act
        var result = await _userService.Update(userId, updatePayload, CancellationToken);

        // Assert
        result.Should().BeFalse();
        _mockUserRepository.Verify(r => r.UpdateAsync(It.IsAny<UserEntity>(), CancellationToken), Times.Never);
    }

    [Fact]
    public async Task Delete_ReturnsTrue()
    {
        // Arrange
        var userId = 1;
        _mockUserRepository.Setup(r => r.SoftDeleteAsync(userId, CancellationToken))
            .ReturnsAsync(true);

        // Act
        var result = await _userService.Delete(userId, CancellationToken);

        // Assert
        result.Should().BeTrue();
        _mockUserRepository.Verify(r => r.SoftDeleteAsync(userId, CancellationToken), Times.Once);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsLoginResponse()
    {
        // Arrange
        var loginPayload = new LoginUserPayload("john.doe", "password123");
        var userEntity = new UserEntity(1, "John", "Doe", "Smith", "john.doe", BCrypt.Net.BCrypt.HashPassword("password123"), false);

        _mockUserRepository.Setup(r => r.FindByAsync("login", "john.doe", true, null, false, false, 1, 10, CancellationToken))
            .ReturnsAsync([userEntity]);

        // Act
        var result = await _userService.Login(loginPayload, CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().NotBeNullOrEmpty();
        result.User.Should().NotBeNull();
        result.User.Login.Should().Be("john.doe");
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var loginPayload = new LoginUserPayload("john.doe", "wrong_password");
        var userEntity = new UserEntity(1, "John", "Doe", "Smith", "john.doe", BCrypt.Net.BCrypt.HashPassword("correct_password"), false);

        _mockUserRepository.Setup(r => r.FindByAsync("login", "john.doe", true, null, false, false, 1, 10, CancellationToken))
            .ReturnsAsync(new[] { userEntity });

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _userService.Login(loginPayload, CancellationToken));
    }

    [Fact]
    public async Task GetActivs_ReturnsListOfActivs()
    {
        // Arrange
        var userId = 1;
        var activs = new List<HumReadActivPayload>
        {
            new(1, 1, "Org 1", "Status 1", DateTime.Now, TimeSpan.FromHours(9), TimeSpan.FromHours(10), "Description 1"),
            new(2, 1, "Org 2", "Status 2", DateTime.Now, TimeSpan.FromHours(10), TimeSpan.FromHours(11), "Description 2")
        };

        _mockActivRepository.Setup(r => r.GetAllHumActivsByUserIdAsync(userId, CancellationToken))
            .ReturnsAsync(activs);

        // Act
        var result = await _userService.GetActivs(userId, CancellationToken);

        // Assert
        result.Should().HaveCount(2);
        result[0].Description.Should().Be("Description 1");
        result[1].Description.Should().Be("Description 2");
    }

    [Fact]
    public async Task GetPlans_ReturnsListOfPlans()
    {
        // Arrange
        var userId = 1;
        var plans = new List<PlanEntity>
        {
            new(1, userId, 1, DateTime.Now, DateTime.Now.AddDays(1), false),
            new(2, userId, 2, DateTime.Now.AddDays(1), DateTime.Now.AddDays(2), false)
        };

        _mockPlanRepository.Setup(r => r.FindByAsync("usr_id", userId, true, null, false, false, 1, 10, CancellationToken))
            .ReturnsAsync(plans);

        // Act
        var result = await _userService.GetPlans(userId, CancellationToken);

        // Assert
        result.Should().HaveCount(2);
        result[0].UsrId.Should().Be(userId);
        result[1].UsrId.Should().Be(userId);
    }
}
