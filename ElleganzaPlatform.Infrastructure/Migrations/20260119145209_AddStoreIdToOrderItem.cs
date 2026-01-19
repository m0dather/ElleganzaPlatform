using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElleganzaPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStoreIdToOrderItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StoreId",
                table: "OrderItems",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "OrderItems");
        }
    }
}
