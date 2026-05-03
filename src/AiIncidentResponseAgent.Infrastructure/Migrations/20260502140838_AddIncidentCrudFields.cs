using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiIncidentResponseAgent.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIncidentCrudFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_archived",
                table: "incidents",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_incidents_is_archived",
                table: "incidents",
                column: "is_archived");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_incidents_is_archived",
                table: "incidents");

            migrationBuilder.DropColumn(
                name: "is_archived",
                table: "incidents");
        }
    }
}
