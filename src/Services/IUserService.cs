namespace CrmBack.Services;

using System.Threading;
using CrmBack.Core.Models.Payload.User;

public interface IUserService : IService<ReadUserPayload, CreateUserPayload, UpdateUserPayload>
{
    public Task<LoginResponsePayload> Login(LoginUserPayload payload, CancellationToken ct = default);
}
