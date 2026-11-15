using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiToDu.Platform.Warehouse.Infrastructure.Persistence.EntityFrameworkCore.Migrations;

public partial class AddAuditStatusCode : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) =>
        migrationBuilder.AddColumn<int>(
            name: "status_code",
            table: "audit_entries",
            type: "int",
            nullable: false,
            defaultValue: 200);

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropColumn(name: "status_code", table: "audit_entries");
}
