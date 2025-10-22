namespace CrmBack.Services;

using System.Threading;
using CrmBack.Core.Models.Dto;


public interface IUserService : IService<ReadUserDto, CreateUserDto, UpdateUserDto>
{
    public Task<LoginResponseDto> Login(LoginUserDto Dto, CancellationToken ct = default);
    public Task<List<HumReadActivDto>> GetActivs(int userId, CancellationToken ct = default);
}
