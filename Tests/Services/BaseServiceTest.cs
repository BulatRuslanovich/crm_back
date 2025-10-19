using Microsoft.Extensions.Configuration;
using Moq;

namespace CrmBack.Tests.Services;

public abstract class BaseServiceTest
{
    protected readonly Mock<IConfiguration> MockConfiguration;
    protected readonly CancellationToken CancellationToken = CancellationToken.None;

    protected BaseServiceTest()
    {
        MockConfiguration = new Mock<IConfiguration>();
        SetupDefaultConfiguration();
    }

    private void SetupDefaultConfiguration()
    {
        MockConfiguration.Setup(c => c["Jwt:Key"]).Returns("test-jwt-key-that-is-long-enough-for-hmac-sha256");
        MockConfiguration.Setup(c => c["Jwt:Issuer"]).Returns("test-issuer");
        MockConfiguration.Setup(c => c["Jwt:Audience"]).Returns("test-audience");
    }
}
