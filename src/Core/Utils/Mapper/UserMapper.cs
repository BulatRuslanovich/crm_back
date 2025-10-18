namespace CrmBack.Core.Utils.Mapper;

using CrmBack.Core.Models.Entities;
using CrmBack.Core.Models.Payload.User;

public static class UserMapper
{
    public static ReadUserPayload ToReadPayload(this UserEntity entity) => new(
        Id: entity.usr_id,
        FirstName: entity.first_name,
        LastName: entity.last_name,
        MiddleName: entity.middle_name,
        Login: entity.login
    );

    public static UserEntity ToEntity(this CreateUserPayload payload) => new(
        usr_id: 0,
        first_name: payload.FirstName,
        last_name: payload.LastName,
        middle_name: payload.MiddleName,
        login: payload.Login,
        password_hash: BCrypt.Net.BCrypt.HashPassword(payload.Password),
        is_deleted: false
    );
}
