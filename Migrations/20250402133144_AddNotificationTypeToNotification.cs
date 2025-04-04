using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GuideMe.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationTypeToNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PrivateMessages_Users_ReceiverId",
                table: "PrivateMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_PrivateMessages_Users_SenderId",
                table: "PrivateMessages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PrivateMessages",
                table: "PrivateMessages");

            migrationBuilder.DropIndex(
                name: "IX_PrivateMessages_ReceiverId",
                table: "PrivateMessages");

            migrationBuilder.DropIndex(
                name: "IX_PrivateMessages_SenderId",
                table: "PrivateMessages");

            migrationBuilder.DropColumn(
                name: "Attachment",
                table: "PrivateMessages");

            migrationBuilder.DropColumn(
                name: "IsRead",
                table: "PrivateMessages");

            migrationBuilder.AddColumn<int>(
                name: "CommentsCount",
                table: "UserPost",
                type: "int",
                nullable: true,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LikesCount",
                table: "UserPost",
                type: "int",
                nullable: true,
                defaultValue: 0);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "SentAt",
                table: "PrivateMessages",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddPrimaryKey(
                name: "PK__PrivateM__F64E74D7662052F2",
                table: "PrivateMessages",
                column: "PrivateMessageId");

            migrationBuilder.CreateTable(
                name: "Ratings",
                columns: table => new
                {
                    RatingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    UrbanTreasureId = table.Column<int>(type: "int", nullable: false),
                    RatingValue = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Ratings__FCCDF87CFA16EE1A", x => x.RatingId);
                    table.ForeignKey(
                        name: "FK__Ratings__UrbanTr__2BFE89A6",
                        column: x => x.UrbanTreasureId,
                        principalTable: "UrbanTreasures",
                        principalColumn: "UrbanTreasureId");
                    table.ForeignKey(
                        name: "FK__Ratings__UserId__2B0A656D",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_UrbanTreasureId",
                table: "Ratings",
                column: "UrbanTreasureId");

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_UserId",
                table: "Ratings",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Ratings");

            migrationBuilder.DropPrimaryKey(
                name: "PK__PrivateM__F64E74D7662052F2",
                table: "PrivateMessages");

            migrationBuilder.DropColumn(
                name: "CommentsCount",
                table: "UserPost");

            migrationBuilder.DropColumn(
                name: "LikesCount",
                table: "UserPost");

            migrationBuilder.AlterColumn<DateTime>(
                name: "SentAt",
                table: "PrivateMessages",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Attachment",
                table: "PrivateMessages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRead",
                table: "PrivateMessages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PrivateMessages",
                table: "PrivateMessages",
                column: "PrivateMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_PrivateMessages_ReceiverId",
                table: "PrivateMessages",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_PrivateMessages_SenderId",
                table: "PrivateMessages",
                column: "SenderId");

            migrationBuilder.AddForeignKey(
                name: "FK_PrivateMessages_Users_ReceiverId",
                table: "PrivateMessages",
                column: "ReceiverId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_PrivateMessages_Users_SenderId",
                table: "PrivateMessages",
                column: "SenderId",
                principalTable: "Users",
                principalColumn: "UserId");
        }
    }
}
