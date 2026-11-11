using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace AiToDu.Platform.Iam.Infrastructure.Persistence.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class InitialMySql : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "iam_users",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    public_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    organization_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    organization_name = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false),
                    full_name = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false),
                    email = table.Column<string>(type: "varchar(254)", maxLength: 254, nullable: false),
                    password_hash = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    role = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_iam_users", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "i_x_iam_users_organization_id",
                table: "iam_users",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "i_x_iam_users_public_id",
                table: "iam_users",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_users_email",
                table: "iam_users",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "iam_users");
        }
    }
}
