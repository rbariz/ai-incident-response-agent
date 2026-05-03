using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AiIncidentResponseAgent.Infrastructure.Persistence;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Testcontainers.PostgreSql;


namespace AiIncidentResponseAgent.IntegrationTests
{
    
    

    public sealed class TestAgentApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
            .WithImage("postgres:16")
            .WithDatabase("ai_agent_tests")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        public string ConnectionString => _postgres.GetConnectionString();

        public async Task InitializeAsync()
        {
            await _postgres.StartAsync();
        }

        public new async Task DisposeAsync()
        {
            await _postgres.DisposeAsync();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureAppConfiguration((_, config) =>
            {
                var settings = new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = ConnectionString,

                    ["OllamaAnalyzer:Enabled"] = "false",
                    ["OllamaAnalyzer:BaseUrl"] = "http://localhost:11434",
                    ["OllamaAnalyzer:Model"] = "llama3",

                    ["Realtime:Enabled"] = "false",

                    ["InternalApiKey:ApiKey"] = "TEST_INTERNAL_KEY",

                    ["Jwt:Issuer"] = "AiIncidentResponseAgent",
                    ["Jwt:Audience"] = "OpsCenter",
                    ["Jwt:Secret"] = "TEST_SECRET_123456789_32_CHARS_MINIMUM",
                    ["Jwt:ExpirationMinutes"] = "120",

                    ["AgentRetry:MaxRetries"] = "3",
                    ["AgentRetry:BaseDelaySeconds"] = "1",
                    ["AgentRetry:MaxDelaySeconds"] = "5"
                };

                config.AddInMemoryCollection(settings);
            });

            builder.ConfigureServices(services =>
            {
                using var scope = services.BuildServiceProvider().CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AgentDbContext>();
                db.Database.Migrate();
            });
        }
    }
}
