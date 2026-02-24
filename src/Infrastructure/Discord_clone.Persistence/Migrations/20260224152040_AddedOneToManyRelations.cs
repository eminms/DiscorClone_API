using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Discord_clone.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddedOneToManyRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "OwnerId",
                table: "Servers",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Servers_OwnerId",
                table: "Servers",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Servers_AspNetUsers_OwnerId",
                table: "Servers",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Servers_AspNetUsers_OwnerId",
                table: "Servers");

            migrationBuilder.DropIndex(
                name: "IX_Servers_OwnerId",
                table: "Servers");

            migrationBuilder.AlterColumn<string>(
                name: "OwnerId",
                table: "Servers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
