using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResolveAI.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddRiskToInvestigation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RiskLevel",
                table: "Investigations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RiskScore",
                table: "Investigations",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RiskLevel",
                table: "Investigations");

            migrationBuilder.DropColumn(
                name: "RiskScore",
                table: "Investigations");
        }
    }
}
