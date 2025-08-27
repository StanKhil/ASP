using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASP.Migrations
{
    /// <inheritdoc />
    public partial class ItemImages_Key : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ItemsImages",
                table: "ItemsImages");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "ItemsImages",
                newName: "ItemId");

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "ItemsImages",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ItemsImages",
                table: "ItemsImages",
                columns: new[] { "ItemId", "ImageUrl" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ItemsImages",
                table: "ItemsImages");

            migrationBuilder.RenameColumn(
                name: "ItemId",
                table: "ItemsImages",
                newName: "Id");

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "ItemsImages",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ItemsImages",
                table: "ItemsImages",
                column: "Id");
        }
    }
}
