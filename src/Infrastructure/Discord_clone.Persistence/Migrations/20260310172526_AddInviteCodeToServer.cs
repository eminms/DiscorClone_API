using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Discord_clone.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInviteCodeToServer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InviteCode",
                table: "Servers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InviteCode",
                table: "Servers");
        }
    }
}
