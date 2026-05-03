using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AiIncidentResponseAgent.Application.Models;
using AiIncidentResponseAgent.Application.Services;
using AiIncidentResponseAgent.Domain.Events;
using AiIncidentResponseAgent.Domain.Executions;

using FluentAssertions;

namespace AiIncidentResponseAgent.UnitTests.Decision;
public sealed class RuleBasedAgentDecisionEngineTests
{
    [Fact]
    public async Task DecideAsync_DuplicateScan_ShouldExecuteBlockTicket()
    {
        var sut = new RuleBasedAgentDecisionEngine();

        var agentEvent = new AgentEvent(
            AgentEventType.DuplicateScan,
            "scanner",
            """{"ticketId":"TCK-001"}""",
            "ticket:TCK-001",
            "en");

        var context = new AgentContext
        {
            Event = agentEvent,
            Lang = "en"
        };

        var analysis = new AgentAnalysisResult
        {
            Summary = "Duplicate scan detected.",
            ConfidenceScore = 0.95m,
            SuggestedAction = "BlockTicket",
            Provider = "stub"
        };

        var result = await sut.DecideAsync(context, analysis);

        result.Decision.Should().Be(AgentDecision.ExecuteAction);
        result.Action.Should().Be(AgentAction.BlockTicket);
        result.RequiresHumanApproval.Should().BeFalse();
    }

    [Fact]
    public async Task DecideAsync_SuspiciousBusinessActivity_ShouldRequireHumanApproval()
    {
        var sut = new RuleBasedAgentDecisionEngine();

        var agentEvent = new AgentEvent(
            AgentEventType.SuspiciousBusinessActivity,
            "monitor",
            """{"userId":"USR-001"}""",
            "user:USR-001",
            "en");

        var context = new AgentContext
        {
            Event = agentEvent,
            Lang = "en"
        };

        var analysis = new AgentAnalysisResult
        {
            Summary = "Suspicious activity.",
            ConfidenceScore = 0.70m,
            SuggestedAction = "Escalate",
            Provider = "stub"
        };

        var result = await sut.DecideAsync(context, analysis);

        result.Decision.Should().Be(AgentDecision.SuggestAction);
        result.Action.Should().Be(AgentAction.Escalate);
        result.RequiresHumanApproval.Should().BeTrue();
    }
}
