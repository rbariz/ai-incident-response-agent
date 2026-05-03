using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiIncidentResponseAgent.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExecutionRetryScheduling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "last_retry_at_utc",
                table: "agent_executions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "next_retry_at_utc",
                table: "agent_executions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_agent_executions_next_retry_at_utc",
                table: "agent_executions",
                column: "next_retry_at_utc");

            migrationBuilder.CreateIndex(
                name: "IX_agent_executions_status",
                table: "agent_executions",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_agent_executions_next_retry_at_utc",
                table: "agent_executions");

            migrationBuilder.DropIndex(
                name: "IX_agent_executions_status",
                table: "agent_executions");

            migrationBuilder.DropColumn(
                name: "last_retry_at_utc",
                table: "agent_executions");

            migrationBuilder.DropColumn(
                name: "next_retry_at_utc",
                table: "agent_executions");
        }
    }
}
