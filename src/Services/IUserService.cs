namespace CrmBack.Services;

using System.Threading;
using CrmBack.Core.Models.Dto;

public interface IUserService : IService<ReadUserDto, CreateUserDto, UpdateUserDto>
{
    public Task<LoginResponseDto> Login(LoginUserDto Dto, CancellationToken ct = default);
    public Task<List<HumReadActivDto>> GetActivs(int userId, CancellationToken ct = default);
    public Task<RefreshTokenResponseDto> RefreshToken(string refreshToken, CancellationToken ct = default);
    public Task<bool> Logout(int userId, CancellationToken ct = default);
}
