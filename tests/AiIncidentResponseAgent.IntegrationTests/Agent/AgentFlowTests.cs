using System.Net;
using System.Net.Http.Json;

using AiIncidentResponseAgent.Contracts.AgentEvents;
using AiIncidentResponseAgent.Contracts.Common;
using AiIncidentResponseAgent.Contracts.Ops;
using AiIncidentResponseAgent.Contracts.Tickets;
using AiIncidentResponseAgent.Domain.Events;
using AiIncidentResponseAgent.IntegrationTests.Auth;

using FluentAssertions;


namespace AiIncidentResponseAgent.IntegrationTests.Agent;
public sealed class AgentFlowTests : IClassFixture<TestAgentApiFactory>
{
    private readonly HttpClient _client;

    public AgentFlowTests(TestAgentApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task DuplicateScan_WithExistingTicket_ShouldEventuallyBlockTicket()
    {
        var token = await TestAuthHelper.LoginAsync(_client);
        _client.AddBearer(token);

        var ticketResponse = await _client.PostAsJsonAsync(
            "/api/tickets",
            new CreateTicketRequest
            {
                TicketCode = "TCK-IT-001"
            });

        ticketResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var eventResponse = await _client.PostAsJsonAsync(
            "/api/agent-events",
            new CreateAgentEventRequest
            {
                Type = AgentEventTypeDto.DuplicateScan,
                Source = "integration-test-scanner",
                PayloadJson = """{"ticketId":"TCK-IT-001","gate":"A1"}""",
                CorrelationId = "ticket:TCK-IT-001",
                Lang = "en"
            });

        eventResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdEvent = await eventResponse.Content.ReadFromJsonAsync<AgentEventResponse>();
        createdEvent.Should().NotBeNull();

        var processResponse = await _client.PostAsync(
            $"/api/agent-events/{createdEvent!.Id}/process",
            content: null);

        processResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);

        var executionsResponse = await _client.GetAsync(
            "/api/agent-executions?correlationId=ticket:TCK-IT-001&page=1&pageSize=10");

        executionsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var executions = await executionsResponse.Content
            .ReadFromJsonAsync<PagedResponse<AgentExecutionResponse>>();

        executions.Should().NotBeNull();
        executions!.Items.Should().ContainSingle();

        var execution = executions.Items.Single();

        execution.Status.Should().Be("Succeeded");
        execution.Action.Should().Be("BlockTicket");
        execution.ResultJson.Should().Contain("LocalTicketingModule");

        var ticketsResponse = await _client.GetAsync("/api/tickets?page=1&pageSize=50");
        ticketsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var tickets = await ticketsResponse.Content.ReadFromJsonAsync<PagedResponse<TicketResponse>>();

        tickets.Should().NotBeNull();
        tickets!.Items.Should().Contain(x =>
            x.TicketCode == "TCK-IT-001" &&
            x.Status == "Blocked");
    }

    [Fact]
    public async Task SuspiciousBusinessActivity_ShouldCreatePendingApprovalExecution()
    {
        var token = await TestAuthHelper.LoginAsync(_client);
        _client.AddBearer(token);

        var eventResponse = await _client.PostAsJsonAsync(
            "/api/agent-events",
            new CreateAgentEventRequest
            {
                Type = AgentEventTypeDto.SuspiciousBusinessActivity,
                Source = "integration-test-monitor",
                PayloadJson = """{"userId":"USR-IT-001","signal":"unusual_activity"}""",
                CorrelationId = "user:USR-IT-001",
                Lang = "en"
            });

        eventResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdEvent = await eventResponse.Content.ReadFromJsonAsync<AgentEventResponse>();

        var processResponse = await _client.PostAsync(
            $"/api/agent-events/{createdEvent!.Id}/process",
            null);

        processResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);

        var executionsResponse = await _client.GetAsync(
            "/api/agent-executions?correlationId=user:USR-IT-001&page=1&pageSize=10");

        var executions = await executionsResponse.Content
            .ReadFromJsonAsync<PagedResponse<AgentExecutionResponse>>();

        executions!.Items.Should().ContainSingle();

        var execution = executions.Items.Single();

        execution.Status.Should().Be("PendingApproval");
        execution.Action.Should().Be("Escalate");
    }

}
