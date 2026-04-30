using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiIncidentResponseAgent.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAgentActionLocks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "agent_action_locks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    action = table.Column<int>(type: "integer", nullable: false),
                    correlation_id = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    agent_event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    locked_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agent_action_locks", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_agent_action_locks_action_correlation_id",
                table: "agent_action_locks",
                columns: new[] { "action", "correlation_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_agent_action_locks_agent_event_id",
                table: "agent_action_locks",
                column: "agent_event_id");

            migrationBuilder.CreateIndex(
                name: "IX_agent_action_locks_locked_at_utc",
                table: "agent_action_locks",
                column: "locked_at_utc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "agent_action_locks");
        }
    }
}
