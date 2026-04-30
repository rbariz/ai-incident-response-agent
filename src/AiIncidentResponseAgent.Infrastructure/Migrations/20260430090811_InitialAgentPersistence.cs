using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiIncidentResponseAgent.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialAgentPersistence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "agent_events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    source = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    payload = table.Column<string>(type: "jsonb", nullable: false),
                    correlation_id = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    processed = table.Column<bool>(type: "boolean", nullable: false),
                    processed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agent_events", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "agent_executions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    agent_event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    incident_id = table.Column<Guid>(type: "uuid", nullable: true),
                    idempotency_key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    correlation_id = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    decision = table.Column<int>(type: "integer", nullable: false),
                    action = table.Column<int>(type: "integer", nullable: false),
                    analysis_summary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    confidence_score = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: false),
                    result = table.Column<string>(type: "jsonb", nullable: false),
                    error_message = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    retry_count = table.Column<int>(type: "integer", nullable: false),
                    started_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agent_executions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "agent_memory",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    entity_id = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    entity_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    context = table.Column<string>(type: "jsonb", nullable: false),
                    last_updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agent_memory", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "incidents",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    agent_event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    severity = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    resolved_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_incidents", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_agent_events_correlation_id",
                table: "agent_events",
                column: "correlation_id");

            migrationBuilder.CreateIndex(
                name: "IX_agent_events_created_at_utc",
                table: "agent_events",
                column: "created_at_utc");

            migrationBuilder.CreateIndex(
                name: "IX_agent_events_processed",
                table: "agent_events",
                column: "processed");

            migrationBuilder.CreateIndex(
                name: "IX_agent_executions_agent_event_id",
                table: "agent_executions",
                column: "agent_event_id");

            migrationBuilder.CreateIndex(
                name: "IX_agent_executions_correlation_id",
                table: "agent_executions",
                column: "correlation_id");

            migrationBuilder.CreateIndex(
                name: "IX_agent_executions_created_at_utc",
                table: "agent_executions",
                column: "created_at_utc");

            migrationBuilder.CreateIndex(
                name: "IX_agent_executions_idempotency_key",
                table: "agent_executions",
                column: "idempotency_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_agent_executions_incident_id",
                table: "agent_executions",
                column: "incident_id");

            migrationBuilder.CreateIndex(
                name: "IX_agent_memory_entity_type_entity_id",
                table: "agent_memory",
                columns: new[] { "entity_type", "entity_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_agent_memory_last_updated_at_utc",
                table: "agent_memory",
                column: "last_updated_at_utc");

            migrationBuilder.CreateIndex(
                name: "IX_incidents_agent_event_id",
                table: "incidents",
                column: "agent_event_id");

            migrationBuilder.CreateIndex(
                name: "IX_incidents_created_at_utc",
                table: "incidents",
                column: "created_at_utc");

            migrationBuilder.CreateIndex(
                name: "IX_incidents_severity",
                table: "incidents",
                column: "severity");

            migrationBuilder.CreateIndex(
                name: "IX_incidents_status",
                table: "incidents",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "agent_events");

            migrationBuilder.DropTable(
                name: "agent_executions");

            migrationBuilder.DropTable(
                name: "agent_memory");

            migrationBuilder.DropTable(
                name: "incidents");
        }
    }
}
