using System;

namespace CrmBack.Core.Models.Payload.User;

public record LoginUserPayload(
    string Login,
    string Password
);
