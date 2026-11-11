using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace AiToDu.Platform.Warehouse.Infrastructure.Persistence.EntityFrameworkCore.Migrations
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
                name: "warehouse_inventory_items",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    created_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    organization_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    sku = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    name = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false),
                    category = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    minimum_stock = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    unit_price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_warehouse_inventory_items", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "warehouse_inventory_movements",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false),
                    inventory_item_id = table.Column<int>(type: "int", nullable: false),
                    operation_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    lot_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    type = table.Column<string>(type: "longtext", nullable: false),
                    change = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    before = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    after = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    reason = table.Column<string>(type: "longtext", nullable: false),
                    actor_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    occurred_at = table.Column<DateTimeOffset>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_warehouse_inventory_movements", x => new { x.inventory_item_id, x.id });
                    table.ForeignKey(
                        name: "f_k_warehouse_inventory_movements_warehouse_inventory_items_inve~",
                        column: x => x.inventory_item_id,
                        principalTable: "warehouse_inventory_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "warehouse_inventory_operations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false),
                    inventory_item_id = table.Column<int>(type: "int", nullable: false),
                    signature = table.Column<string>(type: "longtext", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_warehouse_inventory_operations", x => new { x.inventory_item_id, x.id });
                    table.ForeignKey(
                        name: "f_k_warehouse_inventory_operations_warehouse_inventory_items_inv~",
                        column: x => x.inventory_item_id,
                        principalTable: "warehouse_inventory_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "warehouse_inventory_returns",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false),
                    inventory_item_id = table.Column<int>(type: "int", nullable: false),
                    operation_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    source_lot_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    destination_lot_id = table.Column<Guid>(type: "char(36)", nullable: true),
                    quantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    disposition = table.Column<string>(type: "longtext", nullable: false),
                    reference = table.Column<string>(type: "longtext", nullable: false),
                    reason = table.Column<string>(type: "longtext", nullable: false),
                    actor_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    occurred_at = table.Column<DateTimeOffset>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_warehouse_inventory_returns", x => new { x.inventory_item_id, x.id });
                    table.ForeignKey(
                        name: "f_k_warehouse_inventory_returns_warehouse_inventory_items_invent~",
                        column: x => x.inventory_item_id,
                        principalTable: "warehouse_inventory_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "warehouse_stock_counts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false),
                    inventory_item_id = table.Column<int>(type: "int", nullable: false),
                    operation_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    lot_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    expected_quantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    counted_quantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    reason = table.Column<string>(type: "longtext", nullable: false),
                    actor_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    occurred_at = table.Column<DateTimeOffset>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_warehouse_stock_counts", x => new { x.inventory_item_id, x.id });
                    table.ForeignKey(
                        name: "f_k_warehouse_stock_counts_warehouse_inventory_items_inventory_i~",
                        column: x => x.inventory_item_id,
                        principalTable: "warehouse_inventory_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "warehouse_stock_lots",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false),
                    inventory_item_id = table.Column<int>(type: "int", nullable: false),
                    code = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    location = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false),
                    expires_on = table.Column<DateOnly>(type: "date", nullable: true),
                    quantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    is_quarantined = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_warehouse_stock_lots", x => new { x.inventory_item_id, x.id });
                    table.ForeignKey(
                        name: "f_k_warehouse_stock_lots_warehouse_inventory_items_inventory_ite~",
                        column: x => x.inventory_item_id,
                        principalTable: "warehouse_inventory_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "ux_inventory_items_organization_sku",
                table: "warehouse_inventory_items",
                columns: new[] { "organization_id", "sku" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_warehouse_stock_lots_inventory_item_id_code_location",
                table: "warehouse_stock_lots",
                columns: new[] { "inventory_item_id", "code", "location" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "warehouse_inventory_movements");

            migrationBuilder.DropTable(
                name: "warehouse_inventory_operations");

            migrationBuilder.DropTable(
                name: "warehouse_inventory_returns");

            migrationBuilder.DropTable(
                name: "warehouse_stock_counts");

            migrationBuilder.DropTable(
                name: "warehouse_stock_lots");

            migrationBuilder.DropTable(
                name: "warehouse_inventory_items");
        }
    }
}
