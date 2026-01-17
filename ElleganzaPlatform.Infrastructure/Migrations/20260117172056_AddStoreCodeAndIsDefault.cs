using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElleganzaPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStoreCodeAndIsDefault : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Stores",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                table: "Stores",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Stores_Code",
                table: "Stores",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Stores_Code",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "IsDefault",
                table: "Stores");
        }
    }
}
