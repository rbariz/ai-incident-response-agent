using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AiIncidentResponseAgent.Application.Abstractions;
using AiIncidentResponseAgent.Application.Services;

using Microsoft.Extensions.DependencyInjection;

namespace AiIncidentResponseAgent.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IAgentOrchestrator, AgentOrchestrator>();
            services.AddScoped<IAgentDecisionEngine, RuleBasedAgentDecisionEngine>();
            services.AddScoped<IAgentPolicyEngine, SafeAgentPolicyEngine>();
            services.AddScoped<IAgentFeedbackHandler, NoOpAgentFeedbackHandler>();
            services.AddScoped<IAgentMemoryService, AgentMemoryService>();

            return services;
        }
    }
}
