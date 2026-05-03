using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

using AiIncidentResponseAgent.Contracts.Auth;

namespace AiIncidentResponseAgent.IntegrationTests.Auth;

public static class TestAuthHelper
{
    public static async Task<string> LoginAsync(
        HttpClient client,
        string username = "admin",
        string password = "Admin123!")
    {
        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest
            {
                Username = username,
                Password = password
            });

        response.EnsureSuccessStatusCode();

        var login = await response.Content.ReadFromJsonAsync<LoginResponse>();

        return login!.AccessToken;
    }

    public static void AddBearer(this HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }
}
