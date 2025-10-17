namespace CrmBack.Core.Utils.Mapper;

using CrmBack.Core.Models.Entities;
using CrmBack.Core.Models.Payload.User;

public static class UserMapper
{
    public static ReadUserPayload ToReadPayload(this UserEntity entity) => new(
        Id: entity.usr_id,
        FirstName: entity.first_name ?? "-",
        LastName: entity.last_name ?? "-",
        MiddleName: entity.middle_name ?? "-",
        Login: entity.login ?? "-"
    );

    public static List<ReadUserPayload> ToReadPayloads(this IEnumerable<UserEntity> entities) =>
        [.. entities.Select(ToReadPayload)];

    public static UserEntity ToEntity(this CreateUserPayload payload) => new(
        usr_id: 0,
        first_name: payload.FirstName,
        last_name: payload.LastName,
        middle_name: payload.MiddleName,
        login: payload.Login,
        password_hash: BCrypt.Net.BCrypt.HashPassword(payload.Password)
    );

    public static UserEntity ToEntity(this UpdateUserPayload payload, int id, string? existingLogin = null, string? existingHash = null) => new(
        usr_id: id,
        first_name: payload.FirstName,
        last_name: payload.LastName,
        middle_name: payload.MiddleName,
        login: payload.Login ?? existingLogin ?? "",
        password_hash: string.IsNullOrEmpty(payload.Password)
            ? existingHash ?? ""
            : BCrypt.Net.BCrypt.HashPassword(payload.Password)
    );
}