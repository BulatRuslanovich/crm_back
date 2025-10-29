using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CrmBack.Core.Models.Dto;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CrmBack.Tests;

public class UserEndpointTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient client = factory.CreateDefaultClient();

    [Fact]
    public async Task UserLifecycle_Register_Login_Get_Delete()
    {
        var createDto = new CreateUserDto
        {
            FirstName = "Test",
            MiddleName = "Middle",
            LastName = "User",
            Login = $"testuser_{Guid.NewGuid().ToString("N")[..8]}",
            Password = "TestPass123"
        };

        var registerResponse = await client.PostAsJsonAsync("/api/usr/register", createDto);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var createdUser = await registerResponse.Content.ReadFromJsonAsync<ReadUserDto>();
        createdUser.Should().NotBeNull();
        createdUser!.Login.Should().Be(createDto.Login);

        var loginDto = new LoginUserDto
        {
            Login = createDto.Login,
            Password = createDto.Password
        };

        var loginResponse = await client.PostAsJsonAsync("/api/usr/login", loginDto);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        loginResult.Should().NotBeNull();
        loginResult!.Login.Should().Be(createDto.Login);

        createdUser.Should().NotBeNull();
        loginResult.Should().NotBeNull();

        var setCookieHeader = loginResponse.Headers.FirstOrDefault(h => h.Key == "Set-Cookie");
        string? accessToken = setCookieHeader.Value
            .FirstOrDefault(c => c.Contains("access_token"))
            ?.Split(';')[0].Split('=')[1];

        if (accessToken is not null)
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);
        }

        var getResponse = await client.GetAsync($"/api/usr/{createdUser!.UsrId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetchedUser = await getResponse.Content.ReadFromJsonAsync<ReadUserDto>();
        fetchedUser.Should().NotBeNull();
        fetchedUser!.Login.Should().Be(createDto.Login);

        var deleteResponse = await client.DeleteAsync($"/api/usr/{createdUser.UsrId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

}

