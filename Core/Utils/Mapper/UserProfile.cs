namespace CrmBack.Core.Utils.Mapper;

using CrmBack.Core.Models.Entities;
using CrmBack.Core.Models.Payload;

public static class UserMapper
{
    // UserEntity -> ReadUserPayload
    public static ReadUserPayload ToReadPayload(this UserEntity entity)
    {
        return new ReadUserPayload(
            Id: entity.usr_id,
            Name: entity.name,
            Login: entity.login
        );
    }

    // IEnumerable<UserEntity> -> IEnumerable<ReadUserPayload>
    public static IEnumerable<ReadUserPayload> ToReadPayloads(this IEnumerable<UserEntity> entities)
    {
        return entities.Select(ToReadPayload);
    }

    // CreateUserPayload -> UserEntity
    public static UserEntity ToEntity(this CreateUserPayload payload)
    {
        return new UserEntity(
            usr_id: 0,
            name: payload.Name,
            login: payload.Login
        );
    }

    // UpdateUserPayload -> UserEntity (для создания нового Entity)
    public static UserEntity ToEntity(this UpdateUserPayload payload, int id)
    {
        return new UserEntity(
            usr_id: id,
            name: payload.Name,
            login: payload.Login
        );
    }
}