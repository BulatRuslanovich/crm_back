using CrmBack.Application.Activities.Dto;
using CrmBack.Application.Auth.Services;
using CrmBack.Application.Common.Dto;
using CrmBack.Application.Users.Dto;
using CrmBack.Infrastructure.Persistence.Auth;
using CrmBack.Infrastructure.Persistence.Users;


namespace CrmBack.Application.Users.Services;

/// <summary>
/// User service implementation
/// Handles business logic for user operations including authentication
/// Coordinates between DAO layer and JWT/Cookie services
/// </summary>
public class UserService(IUserDAO dao, IJwtService jwt, IRefreshTokenDAO refDao, ICookieService cookie) : IUserService
{
    /// <summary>Get user by ID (delegates to DAO)</summary>
    public async Task<ReadUserDto?> GetById(int id, CancellationToken ct = default) =>
        await dao.FetchById(id, ct);

    /// <summary>Get all users with pagination (delegates to DAO)</summary>
    public async Task<List<ReadUserDto>> GetAll(PaginationDto pagination, CancellationToken ct = default) =>
        await dao.FetchAll(pagination, ct);

    /// <summary>Create new user (delegates to DAO)</summary>
    public async Task<ReadUserDto?> Create(CreateUserDto dto, CancellationToken ct = default) =>
        await dao.Create(dto, ct);


    /// <summary>Delete user (soft delete via DAO)</summary>
    public async Task<bool> Delete(int id, CancellationToken ct = default) =>
        await dao.Delete(id, ct);


    /// <summary>Update user (delegates to DAO)</summary>
    public async Task<bool> Update(int id, UpdateUserDto dto, CancellationToken ct = default) =>
        await dao.Update(id, dto, ct);

    /// <summary>
    /// Authenticate user and issue JWT tokens
    /// Security: Verifies password, generates access/refresh tokens, stores refresh token hash
    /// </summary>
    public async Task<LoginResponseDto> Login(LoginUserDto dto, CancellationToken ct = default)
    {
        // Verify user credentials (password verification happens in DAO)
        var user = await dao.FetchByLogin(dto, ct) ?? throw new UnauthorizedAccessException("Invalid login or password");

        // Extract user roles for JWT claims
        var roles = user.Policies.Select(p => p.PolicyName).ToList();
        // Create and set tokens (access token in cookie, refresh token in cookie)
        await CreateTokensAsync(user, ct);

        return new LoginResponseDto(user.UsrId, user.Login, roles);
    }

    /// <summary>Get user activities (delegates to DAO)</summary>
    public async Task<List<HumReadActivDto>> GetActivs(int userId, CancellationToken ct = default) =>
        await dao.FetchHumActivs(userId, ct);

    /// <summary>
    /// Refresh access token using refresh token
    /// Security: Verifies refresh token hash, invalidates old token, issues new tokens
    /// Flow: Extract refresh token from cookie -> Verify hash -> Generate new tokens -> Store new refresh token
    /// </summary>
    public async Task<RefreshTokenResponseDto> RefreshToken(string? refreshToken = null, CancellationToken ct = default)
    {
        // Get refresh token from cookie if not provided
        refreshToken ??= cookie.GetRefreshTkn()
            ?? throw new UnauthorizedAccessException("Refresh token not found in cookies");

        // Extract user ID from refresh token (JWT payload)
        int userId = jwt.GetUsrId(refreshToken) ?? throw new UnauthorizedAccessException("Invalid refresh token format");

        // Get stored refresh token hash from database
        var tkn = await refDao.GetUserToken(userId, ct)
            ?? throw new UnauthorizedAccessException("Refresh token not found");

        // Verify refresh token hash using BCrypt
        if (!BCrypt.Net.BCrypt.Verify(refreshToken, tkn.TokenHash))
            throw new UnauthorizedAccessException("Invalid refresh token");

        // Fetch user with policies for new token generation
        var user = await dao.FetchByIdWithPolicies(userId, ct) ?? throw new UnauthorizedAccessException("User not found");

        // Create new tokens (invalidates old refresh token, creates new one)
        await CreateTokensAsync(user, ct);

        var roles = user.Policies.Select(p => p.PolicyName).ToList();
        return new RefreshTokenResponseDto(user.UsrId, user.Login, roles);
    }

    /// <summary>
    /// Create access and refresh tokens for user
    /// Security: Refresh token is hashed with BCrypt before storage
    /// Tokens are set as HTTP-only cookies (XSS protection)
    /// </summary>
    private async Task CreateTokensAsync(UserWithPoliciesDto user, CancellationToken ct)
    {
        var roles = user.Policies.Select(p => p.PolicyName).ToList();
        // Generate JWT tokens
        string accessTkn = jwt.GenerateAccessTkn(user.UsrId, user.Login, roles);
        string refreshTkn = jwt.GenerateRefreshTkn(user.UsrId);
        // Hash refresh token before storage (security best practice)
        string refreshTknHash = BCrypt.Net.BCrypt.HashPassword(refreshTkn);

        // Token expiration times
        var expiresAt = DateTime.UtcNow.AddDays(7);        // Refresh token: 7 days
        var accessTokenExpiresAt = DateTime.UtcNow.AddHours(1);  // Access token: 1 hour

        // Store refresh token hash in database (old tokens are invalidated automatically)
        await refDao.CreateAsync(user.UsrId, refreshTknHash, expiresAt, ct);

        // Set tokens as HTTP-only cookies (prevents JavaScript access, XSS protection)
        cookie.SetAccessTkn(accessTkn, accessTokenExpiresAt);
        cookie.SetRefreshTkn(refreshTkn, expiresAt);
    }

    /// <summary>
    /// Logout user: invalidate all refresh tokens
    /// Security: Deletes all refresh tokens for user, clears cookies
    /// </summary>
    public async Task<bool> Logout(int userId, CancellationToken ct = default)
    {
        // Delete all refresh tokens for user (prevents token reuse)
        bool success = await refDao.DeleteAll(userId, ct);
        // Clear authentication cookies
        cookie.Clear();
        return success;
    }
}
