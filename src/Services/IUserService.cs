namespace CrmBack.Services;

using System.Threading;
using CrmBack.Core.Models.Payload.Activ;
using CrmBack.Core.Models.Payload.Plan;
using CrmBack.Core.Models.Payload.User;

public interface IUserService : IService<ReadUserPayload, CreateUserPayload, UpdateUserPayload>
{
    public Task<LoginResponsePayload> Login(LoginUserPayload payload, CancellationToken ct = default);
    public Task<RefreshTokenPayload> RefreshTokenAsync(string refreshToken, CancellationToken ct = default);
    public Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken ct = default);
    public Task<List<HumReadActivPayload>> GetActivs(int userId, CancellationToken ct = default);
    public Task<List<ReadPlanPayload>> GetPlans(int userId, CancellationToken ct = default);
}
