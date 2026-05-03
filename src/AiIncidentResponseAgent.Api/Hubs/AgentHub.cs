using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AiIncidentResponseAgent.Api.Hubs;

[Authorize(Policy = "CanViewOps")]
public sealed class AgentHub : Hub
{
}
