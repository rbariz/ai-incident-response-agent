using System.Net;
using System.Net.Http.Json;

using AiIncidentResponseAgent.Contracts.AgentEvents;
using AiIncidentResponseAgent.Contracts.Common;
using AiIncidentResponseAgent.Contracts.Ops;

using FluentAssertions;

namespace AiIncidentResponseAgent.IntegrationTests.Auth;

public sealed class RbacTests : IClassFixture<TestAgentApiFactory>
{
    private readonly HttpClient _client;

    public RbacTests(TestAgentApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Viewer_ShouldNotApproveExecution()
    {
        var adminToken = await TestAuthHelper.LoginAsync(_client, "admin", "Admin123!");
        _client.AddBearer(adminToken);

        var eventResponse = await _client.PostAsJsonAsync(
            "/api/agent-events",
            new CreateAgentEventRequest
            {
                Type = AgentEventTypeDto.SuspiciousBusinessActivity,
                Source = "rbac-test-monitor",
                PayloadJson = """{"userId":"USR-RBAC-001"}""",
                CorrelationId = "user:USR-RBAC-001",
                Lang = "en"
            });

        var createdEvent = await eventResponse.Content.ReadFromJsonAsync<AgentEventResponse>();

        await _client.PostAsync(
            $"/api/agent-events/{createdEvent!.Id}/process",
            null);

        var executionsResponse = await _client.GetAsync(
            "/api/agent-executions?correlationId=user:USR-RBAC-001&page=1&pageSize=10");

        var executions = await executionsResponse.Content
            .ReadFromJsonAsync<PagedResponse<AgentExecutionResponse>>();

        var executionId = executions!.Items.Single().Id;

        var viewerClient = _client;
        viewerClient.DefaultRequestHeaders.Authorization = null;

        var viewerToken = await TestAuthHelper.LoginAsync(
            viewerClient,
            "viewer",
            "Viewer123!");

        viewerClient.AddBearer(viewerToken);

        var approveResponse = await viewerClient.PostAsJsonAsync(
            $"/api/agent-executions/{executionId}/approve",
            new ApproveExecutionRequest
            {
                Reason = "Viewer should not approve."
            });

        approveResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
