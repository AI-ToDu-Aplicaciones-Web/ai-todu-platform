using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiToDu.Platform.Warehouse.Infrastructure.Persistence.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class SprintTwoAccessAndOperations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audit_entries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false),
                    organization_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    actor_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    method = table.Column<string>(type: "longtext", nullable: false),
                    path = table.Column<string>(type: "longtext", nullable: false),
                    occurred_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_audit_entries", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "notification_reads",
                columns: table => new
                {
                    organization_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    actor_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    alert_key = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    read_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_notification_reads", x => new { x.organization_id, x.actor_id, x.alert_key });
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "subscriptions",
                columns: table => new
                {
                    organization_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    plan = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_subscriptions", x => x.organization_id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "ix_audit_org_date",
                table: "audit_entries",
                columns: new[] { "organization_id", "occurred_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_entries");

            migrationBuilder.DropTable(
                name: "notification_reads");

            migrationBuilder.DropTable(
                name: "subscriptions");
        }
    }
}
