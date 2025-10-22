using BCrypt.Net;
using CrmBack.Core.Models.Dto;
using CrmBack.DAO;
using CrmBack.Data;
using Microsoft.EntityFrameworkCore;


namespace CrmBack.Services.Impl;
public class UserService(IUserDAO dao) : IUserService
{
    public async Task<ReadUserDto?> GetById(int id, CancellationToken ct = default) =>
        await dao.FetchById(id, ct);

    public async Task<List<ReadUserDto>> GetAll(bool isDeleted, int page, int pageSize, string? searchTerm = null, CancellationToken ct = default) =>
        await dao.FetchAll(isDeleted, page, pageSize, searchTerm, ct);

    public async Task<ReadUserDto?> Create(CreateUserDto dto, CancellationToken ct = default) =>
        await dao.Create(dto, ct);


    public async Task<bool> Delete(int id, CancellationToken ct = default) =>
        await dao.Delete(id, ct);


    public async Task<bool> Update(int id, UpdateUserDto dto, CancellationToken ct = default) =>
        await dao.Update(id, dto, ct);



    public async Task<LoginResponseDto> Login(LoginUserDto Dto, CancellationToken ct = default)
    {
        // var user = await context.User
        //     .FirstOrDefaultAsync(u => u.Login == Dto.Login && !u.IsDeleted, ct);

        // if (user == null || !BCrypt.Net.BCrypt.Verify(Dto.Password, user.PasswordHash))
        //     throw new UnauthorizedAccessException("Invalid login or password");

        // return new LoginResponseDto(
        //     UserId: user.UsrId,
        //     Token: "dummy_token", // Заменить на реальную генерацию JWT
        //     RefreshToken: "dummy_refresh_token" // Заменить на реальную генерацию
        // );

        return new LoginResponseDto();
    }

    public async Task<List<HumReadActivDto>> GetActivs(int userId, CancellationToken ct = default) =>
        await dao.FetchHumActivs(userId, ct);

}
