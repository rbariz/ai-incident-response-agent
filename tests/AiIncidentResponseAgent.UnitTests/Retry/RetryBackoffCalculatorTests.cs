using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AiIncidentResponseAgent.Application.Models;
using AiIncidentResponseAgent.Application.Services;
using FluentAssertions;



namespace AiIncidentResponseAgent.UnitTests.Retry;


public sealed class RetryBackoffCalculatorTests
{
    [Fact]
    public void CalculateNextRetryUtc_ShouldIncreaseDelayExponentially()
    {
        var options = new RetryOptions
        {
            BaseDelaySeconds = 10,
            MaxDelaySeconds = 300
        };

        var before = DateTime.UtcNow;

        var next = RetryBackoffCalculator.CalculateNextRetryUtc(
            retryCount: 2,
            options);

        var after = DateTime.UtcNow;

        next.Should().BeOnOrAfter(before.AddSeconds(40));
        next.Should().BeOnOrBefore(after.AddSeconds(41));
    }

    [Fact]
    public void CalculateNextRetryUtc_ShouldRespectMaxDelay()
    {
        var options = new RetryOptions
        {
            BaseDelaySeconds = 10,
            MaxDelaySeconds = 30
        };

        var before = DateTime.UtcNow;

        var next = RetryBackoffCalculator.CalculateNextRetryUtc(
            retryCount: 10,
            options);

        var after = DateTime.UtcNow;

        next.Should().BeOnOrAfter(before.AddSeconds(30));
        next.Should().BeOnOrBefore(after.AddSeconds(31));
    }
}
