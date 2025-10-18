namespace CrmBack.Core.Services;

using CrmBack.Core.Models.Payload.Org;

public interface IOrgService : IService<ReadOrgPayload, CreateOrgPayload, UpdateOrgPayload> { }