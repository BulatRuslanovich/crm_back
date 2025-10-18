namespace CrmBack.Core.Services;

using CrmBack.Core.Models.Payload.User;
using System.Threading;

public interface IUserService : IService<ReadUserPayload, CreateUserPayload, UpdateUserPayload>
{
    public Task<LoginResponsePayload> Login(LoginUserPayload payload, CancellationToken ct = default);
}