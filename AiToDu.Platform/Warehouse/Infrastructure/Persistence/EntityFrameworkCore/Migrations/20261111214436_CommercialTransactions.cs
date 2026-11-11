using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiToDu.Platform.Warehouse.Infrastructure.Persistence.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class CommercialTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "sales_customers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false),
                    organization_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    name = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_sales_customers", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "sales_orders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false),
                    organization_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    operation_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    customer_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    customer_name = table.Column<string>(type: "longtext", nullable: false),
                    request_hash = table.Column<string>(type: "longtext", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    actor_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    status = table.Column<string>(type: "longtext", nullable: false),
                    payment_reference = table.Column<string>(type: "longtext", nullable: false),
                    receipt = table.Column<string>(type: "longtext", nullable: true),
                    total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_sales_orders", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "sales_lines",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false),
                    item_id = table.Column<int>(type: "int", nullable: false),
                    lot_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    sku = table.Column<string>(type: "longtext", nullable: false),
                    name = table.Column<string>(type: "longtext", nullable: false),
                    unit_price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    quantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    returned = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    sale_id = table.Column<Guid>(type: "char(36)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_sales_lines", x => x.id);
                    table.ForeignKey(
                        name: "fk_line_sale",
                        column: x => x.sale_id,
                        principalTable: "sales_orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "sales_returns",
                columns: table => new
                {
                    operation_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    sale_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    line_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    quantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    disposition = table.Column<string>(type: "longtext", nullable: false),
                    reason = table.Column<string>(type: "longtext", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_sales_returns", x => new { x.sale_id, x.operation_id });
                    table.ForeignKey(
                        name: "fk_return_sale",
                        column: x => x.sale_id,
                        principalTable: "sales_orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "ux_customer_name",
                table: "sales_customers",
                columns: new[] { "organization_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_sales_lines_sale_id",
                table: "sales_lines",
                column: "sale_id");

            migrationBuilder.CreateIndex(
                name: "ux_sale_operation",
                table: "sales_orders",
                columns: new[] { "organization_id", "operation_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "sales_customers");

            migrationBuilder.DropTable(
                name: "sales_lines");

            migrationBuilder.DropTable(
                name: "sales_returns");

            migrationBuilder.DropTable(
                name: "sales_orders");
        }
    }
}
