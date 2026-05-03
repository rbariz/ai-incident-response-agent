using System.Net;
using System.Net.Http.Json;

using AiIncidentResponseAgent.Contracts.Auth;

using FluentAssertions;

namespace AiIncidentResponseAgent.IntegrationTests.Auth;

public sealed class AuthTests : IClassFixture<TestAgentApiFactory>
{
    private readonly HttpClient _client;

    public AuthTests(TestAgentApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_WithAdminCredentials_ShouldReturnToken()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest
            {
                Username = "admin",
                Password = "Admin123!"
            });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeNullOrWhiteSpace();
        result.Role.Should().Be("Admin");
    }
}
