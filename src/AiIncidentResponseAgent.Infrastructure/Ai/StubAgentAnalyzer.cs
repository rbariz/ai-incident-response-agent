using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AiIncidentResponseAgent.Application.Abstractions;
using AiIncidentResponseAgent.Application.Models;
using AiIncidentResponseAgent.Domain.Events;

namespace AiIncidentResponseAgent.Infrastructure.Ai;

public sealed class StubAgentAnalyzer : IAgentAnalyzer
{
    public Task<AgentAnalysisResult> AnalyzeAsync(
        AgentContext context,
        CancellationToken cancellationToken = default)
    {
        var result = context.Event.Type switch
        {
            AgentEventType.DuplicateScan => new AgentAnalysisResult
            {
                Summary = "Duplicate scan detected. This may indicate ticket reuse or fraud attempt.",
                Intent = "Antifraud",
                ConfidenceScore = 0.95m,
                SuggestedAction = "BlockTicket",
                RawResponseJson = """
                {
                  "provider": "stub",
                  "intent": "Antifraud",
                  "suggestedAction": "BlockTicket"
                }
                """
            },

            AgentEventType.ApiErrorSpike => new AgentAnalysisResult
            {
                Summary = "API error spike detected. Service degradation is likely.",
                Intent = "SystemMonitoring",
                ConfidenceScore = 0.90m,
                SuggestedAction = "RestartService",
                RawResponseJson = """
                {
                  "provider": "stub",
                  "intent": "SystemMonitoring",
                  "suggestedAction": "RestartService"
                }
                """
            },

            AgentEventType.FraudRiskDetected => new AgentAnalysisResult
            {
                Summary = "Fraud risk signal detected. Further investigation is recommended.",
                Intent = "FraudRisk",
                ConfidenceScore = 0.82m,
                SuggestedAction = "CreateIncident",
                RawResponseJson = """
                {
                  "provider": "stub",
                  "intent": "FraudRisk",
                  "suggestedAction": "CreateIncident"
                }
                """
            },

            AgentEventType.SuspiciousBusinessActivity => new AgentAnalysisResult
            {
                Summary = "Suspicious business activity detected. Operator review is recommended.",
                Intent = "BusinessAnomaly",
                ConfidenceScore = 0.70m,
                SuggestedAction = "Escalate",
                RawResponseJson = """
                {
                  "provider": "stub",
                  "intent": "BusinessAnomaly",
                  "suggestedAction": "Escalate"
                }
                """
            },

            _ => new AgentAnalysisResult
            {
                Summary = "No known risk pattern detected.",
                Intent = "Unknown",
                ConfidenceScore = 0.30m,
                SuggestedAction = "ObserveOnly",
                RawResponseJson = """
                {
                  "provider": "stub",
                  "intent": "Unknown",
                  "suggestedAction": "ObserveOnly"
                }
                """
            }
        };

        return Task.FromResult(result);
    }
}
