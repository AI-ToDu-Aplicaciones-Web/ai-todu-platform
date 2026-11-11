using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiToDu.Platform.Iam.Infrastructure.Persistence.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class SprintTwoAccessAndOperations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "inventory_override",
                table: "iam_users",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "sales_override",
                table: "iam_users",
                type: "tinyint(1)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "inventory_override",
                table: "iam_users");

            migrationBuilder.DropColumn(
                name: "sales_override",
                table: "iam_users");
        }
    }
}
