namespace CrmBack.Application.Users.Services;

using System.Threading;
using CrmBack.Application.Activities.Dto;
using CrmBack.Application.Common.Services;
using CrmBack.Application.Users.Dto;

public interface IUserService : IService<ReadUserDto, CreateUserDto, UpdateUserDto>
{
    public Task<LoginResponseDto> Login(LoginUserDto Dto, CancellationToken ct = default);
    public Task<List<HumReadActivDto>> GetActivs(int userId, CancellationToken ct = default);
    public Task<RefreshTokenResponseDto> RefreshToken(string? refreshToken = null, CancellationToken ct = default);
    public Task<bool> Logout(int userId, CancellationToken ct = default);
}
