using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AiIncidentResponseAgent.Application.Abstractions.Repositories;
using AiIncidentResponseAgent.Application.Actions;
using AiIncidentResponseAgent.Application.Models;
using AiIncidentResponseAgent.Domain.Events;
using AiIncidentResponseAgent.Domain.Ticketing;
using AiIncidentResponseAgent.Infrastructure.Actions.Handlers;

using FluentAssertions;

using Microsoft.Extensions.Logging;

using Moq;



namespace AiIncidentResponseAgent.UnitTests.Actions;

public sealed class BlockTicketActionHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenTicketExists_ShouldBlockTicket()
    {
        var ticket = new Ticket("TCK-001");

        var repository = new Mock<ITicketRepository>();

        repository.Setup(x => x.GetByCodeAsync(
                "TCK-001",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        var logger = Mock.Of<ILogger<BlockTicketActionHandler>>();

        var sut = new BlockTicketActionHandler(
            repository.Object,
            logger);

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

        var result = await sut.HandleAsync(context);

        result.Success.Should().BeTrue();
        ticket.Status.Should().Be(TicketStatus.Blocked);
        ticket.BlockedAtUtc.Should().NotBeNull();
        result.ResultJson.Should().Contain("LocalTicketingModule");
    }

    [Fact]
    public async Task HandleAsync_WhenTicketDoesNotExist_ShouldFail()
    {
        var repository = new Mock<ITicketRepository>();

        repository.Setup(x => x.GetByCodeAsync(
                "TCK-404",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Ticket?)null);

        var logger = Mock.Of<ILogger<BlockTicketActionHandler>>();

        var sut = new BlockTicketActionHandler(
            repository.Object,
            logger);

        var agentEvent = new AgentEvent(
            AgentEventType.DuplicateScan,
            "scanner",
            """{"ticketId":"TCK-404"}""",
            "ticket:TCK-404",
            "en");

        var context = new AgentContext
        {
            Event = agentEvent,
            Lang = "en"
        };

        var result = await sut.HandleAsync(context);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("was not found");
    }

    [Fact]
    public async Task HandleAsync_WhenPayloadHasNoTicketId_ShouldFail()
    {
        var repository = new Mock<ITicketRepository>();
        var logger = Mock.Of<ILogger<BlockTicketActionHandler>>();

        var sut = new BlockTicketActionHandler(
            repository.Object,
            logger);

        var agentEvent = new AgentEvent(
            AgentEventType.DuplicateScan,
            "scanner",
            """{"gate":"A1"}""",
            "ticket:unknown",
            "en");

        var context = new AgentContext
        {
            Event = agentEvent,
            Lang = "en"
        };

        var result = await sut.HandleAsync(context);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("ticketId");
    }
}