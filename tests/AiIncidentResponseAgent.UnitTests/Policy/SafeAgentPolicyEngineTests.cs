using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AiIncidentResponseAgent.Application.Abstractions.Repositories;
using AiIncidentResponseAgent.Application.Models;
using AiIncidentResponseAgent.Application.Services;
using AiIncidentResponseAgent.Domain.Autonomy;
using AiIncidentResponseAgent.Domain.Events;
using AiIncidentResponseAgent.Domain.Executions;

using FluentAssertions;

using Moq;



namespace AiIncidentResponseAgent.UnitTests.Policy;



public sealed class SafeAgentPolicyEngineTests
{
    [Fact]
    public async Task CheckAsync_WhenActionAlreadyLocked_ShouldDeny()
    {
        var locks = new Mock<IAgentActionLockRepository>();

        locks.Setup(x => x.ExistsAsync(
                AgentAction.BlockTicket,
                "ticket:TCK-001",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var sut = new SafeAgentPolicyEngine(locks.Object);

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

        var decision = new AgentDecisionResult
        {
            Decision = AgentDecision.ExecuteAction,
            Action = AgentAction.BlockTicket,
            AutonomyLevel = AutonomyLevel.High,
            RequiresHumanApproval = false
        };

        var result = await sut.CheckAsync(context, decision);

        result.Allowed.Should().BeFalse();
        result.Reason.Should().Contain("already successfully executed");
    }

    [Fact]
    public async Task CheckAsync_WhenRequiresHumanApproval_ShouldDeny()
    {
        var locks = new Mock<IAgentActionLockRepository>();
        var sut = new SafeAgentPolicyEngine(locks.Object);

        var agentEvent = new AgentEvent(
            AgentEventType.SuspiciousBusinessActivity,
            "monitor",
            "{}",
            "user:USR-001",
            "en");

        var context = new AgentContext
        {
            Event = agentEvent,
            Lang = "en"
        };

        var decision = new AgentDecisionResult
        {
            Decision = AgentDecision.SuggestAction,
            Action = AgentAction.Escalate,
            AutonomyLevel = AutonomyLevel.Medium,
            RequiresHumanApproval = true
        };

        var result = await sut.CheckAsync(context, decision);

        result.Allowed.Should().BeFalse();
        result.Reason.Should().Contain("Human approval");
    }

    [Fact]
    public async Task CheckAsync_WhenSafeBlockTicketAndNoLock_ShouldAllow()
    {
        var locks = new Mock<IAgentActionLockRepository>();

        locks.Setup(x => x.ExistsAsync(
                AgentAction.BlockTicket,
                "ticket:TCK-001",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var sut = new SafeAgentPolicyEngine(locks.Object);

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

        var decision = new AgentDecisionResult
        {
            Decision = AgentDecision.ExecuteAction,
            Action = AgentAction.BlockTicket,
            AutonomyLevel = AutonomyLevel.High,
            RequiresHumanApproval = false
        };

        var result = await sut.CheckAsync(context, decision);

        result.Allowed.Should().BeTrue();
    }
}
