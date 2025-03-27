using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GuideMe.Migrations
{
    /// <inheritdoc />
    public partial class AddFileToChatMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Attachment",
                table: "ChatMessages",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Attachment",
                table: "ChatMessages");
        }
    }
}
