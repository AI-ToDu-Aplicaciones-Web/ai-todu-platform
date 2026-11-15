using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiToDu.Platform.Warehouse.Infrastructure.Persistence.EntityFrameworkCore.Migrations;

public partial class EnrichAuditEntries : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "module",
            table: "audit_entries",
            type: "varchar(32)",
            maxLength: 32,
            nullable: false,
            defaultValue: "api");

        migrationBuilder.AlterColumn<string>(
            name: "method",
            table: "audit_entries",
            type: "varchar(10)",
            maxLength: 10,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "longtext");

        migrationBuilder.AlterColumn<string>(
            name: "path",
            table: "audit_entries",
            type: "varchar(512)",
            maxLength: 512,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "longtext");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "method",
            table: "audit_entries",
            type: "longtext",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "varchar(10)",
            oldMaxLength: 10);

        migrationBuilder.AlterColumn<string>(
            name: "path",
            table: "audit_entries",
            type: "longtext",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "varchar(512)",
            oldMaxLength: 512);

        migrationBuilder.DropColumn(name: "module", table: "audit_entries");
    }
}
