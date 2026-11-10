using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiToDu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class InitialWarehouse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "warehouse");

            migrationBuilder.EnsureSchema(
                name: "shared");

            migrationBuilder.CreateTable(
                name: "inventory_items",
                schema: "warehouse",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sku = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    minimum_stock = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inventory_items", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                schema: "shared",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_type = table.Column<string>(type: "text", nullable: false),
                    payload = table.Column<string>(type: "jsonb", nullable: false),
                    occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbox_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "inventory_movements",
                schema: "warehouse",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    inventory_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    operation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    lot_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    change = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    before = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    after = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    reason = table.Column<string>(type: "text", nullable: false),
                    actor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inventory_movements", x => new { x.inventory_item_id, x.id });
                    table.ForeignKey(
                        name: "FK_inventory_movements_inventory_items_inventory_item_id",
                        column: x => x.inventory_item_id,
                        principalSchema: "warehouse",
                        principalTable: "inventory_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "inventory_operations",
                schema: "warehouse",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    inventory_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    signature = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inventory_operations", x => new { x.inventory_item_id, x.id });
                    table.ForeignKey(
                        name: "FK_inventory_operations_inventory_items_inventory_item_id",
                        column: x => x.inventory_item_id,
                        principalSchema: "warehouse",
                        principalTable: "inventory_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "inventory_returns",
                schema: "warehouse",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    inventory_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    operation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_lot_id = table.Column<Guid>(type: "uuid", nullable: false),
                    destination_lot_id = table.Column<Guid>(type: "uuid", nullable: true),
                    quantity = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    disposition = table.Column<string>(type: "text", nullable: false),
                    reference = table.Column<string>(type: "text", nullable: false),
                    reason = table.Column<string>(type: "text", nullable: false),
                    actor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inventory_returns", x => new { x.inventory_item_id, x.id });
                    table.ForeignKey(
                        name: "FK_inventory_returns_inventory_items_inventory_item_id",
                        column: x => x.inventory_item_id,
                        principalSchema: "warehouse",
                        principalTable: "inventory_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "stock_counts",
                schema: "warehouse",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    inventory_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    operation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    lot_id = table.Column<Guid>(type: "uuid", nullable: false),
                    expected_quantity = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    counted_quantity = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    reason = table.Column<string>(type: "text", nullable: false),
                    actor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stock_counts", x => new { x.inventory_item_id, x.id });
                    table.ForeignKey(
                        name: "FK_stock_counts_inventory_items_inventory_item_id",
                        column: x => x.inventory_item_id,
                        principalSchema: "warehouse",
                        principalTable: "inventory_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "stock_lots",
                schema: "warehouse",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    inventory_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    location = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    expires_on = table.Column<DateOnly>(type: "date", nullable: true),
                    quantity = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    is_quarantined = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stock_lots", x => new { x.inventory_item_id, x.id });
                    table.ForeignKey(
                        name: "FK_stock_lots_inventory_items_inventory_item_id",
                        column: x => x.inventory_item_id,
                        principalSchema: "warehouse",
                        principalTable: "inventory_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ux_inventory_items_organization_sku",
                schema: "warehouse",
                table: "inventory_items",
                columns: new[] { "organization_id", "sku" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_stock_lots_inventory_item_id_code_location",
                schema: "warehouse",
                table: "stock_lots",
                columns: new[] { "inventory_item_id", "code", "location" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inventory_movements",
                schema: "warehouse");

            migrationBuilder.DropTable(
                name: "inventory_operations",
                schema: "warehouse");

            migrationBuilder.DropTable(
                name: "inventory_returns",
                schema: "warehouse");

            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "shared");

            migrationBuilder.DropTable(
                name: "stock_counts",
                schema: "warehouse");

            migrationBuilder.DropTable(
                name: "stock_lots",
                schema: "warehouse");

            migrationBuilder.DropTable(
                name: "inventory_items",
                schema: "warehouse");
        }
    }
}
