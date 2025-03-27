using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GuideMe.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Groups",
                columns: table => new
                {
                    GroupId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    TravelStartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    TravelEndDate = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Groups__149AF36A67D6DE83", x => x.GroupId);
                });

            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    LocationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LocationName = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    LocationCreatedAt = table.Column<DateOnly>(type: "date", nullable: true),
                    Latitude = table.Column<double>(type: "float", nullable: true),
                    Longitude = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Location__E7FEA497411B474C", x => x.LocationId);
                });

            migrationBuilder.CreateTable(
                name: "Province",
                columns: table => new
                {
                    ProvinceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProvinceName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ProvinceImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProvinceDescription = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Provienc__D72D03A0712A4C13", x => x.ProvinceId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LastLogin = table.Column<DateTime>(type: "datetime", nullable: true),
                    UserImage = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Users__1788CC4CD2E2DA80", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "WeatherForecasts",
                columns: table => new
                {
                    WeatherId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Location = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Temperature = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Condition = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__WeatherF__0BF97BF533F44185", x => x.WeatherId);
                });

            migrationBuilder.CreateTable(
                name: "ChatMessages",
                columns: table => new
                {
                    ChatMessageId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    MessageText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ChatMess__9AB6103552931227", x => x.ChatMessageId);
                    table.ForeignKey(
                        name: "FK__ChatMessa__Group__3BFFE745",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "GroupId");
                    table.ForeignKey(
                        name: "FK__ChatMessa__UserI__3B0BC30C",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    EventId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventTitle = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    EventLocation = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    EventStartDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    EventEndDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    IsApproved = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    EventDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsAdded = table.Column<bool>(type: "bit", nullable: false),
                    EventTime = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IsExpired = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Events__7944C810DE62B743", x => x.EventId);
                    table.ForeignKey(
                        name: "FK__Events__UserId__6FE99F9F",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "GroupMembers",
                columns: table => new
                {
                    GroupMemberId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    LeaveReason = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__GroupMem__34481292F805F964", x => x.GroupMemberId);
                    table.ForeignKey(
                        name: "FK__GroupMemb__Group__373B3228",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "GroupId");
                    table.ForeignKey(
                        name: "FK__GroupMemb__UserI__36470DEF",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    NotificationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    IsRead = table.Column<bool>(type: "bit", nullable: true, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Notifica__20CF2E122FC975B5", x => x.NotificationId);
                    table.ForeignKey(
                        name: "FK__Notificat__IsRea__3F115E1A",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "UrbanTreasures",
                columns: table => new
                {
                    UrbanTreasureId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Image = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ProvinceId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__UrbanTre__79281DDF042F0DFC", x => x.UrbanTreasureId);
                    table.ForeignKey(
                        name: "FK_UrbanTreasures_Province",
                        column: x => x.ProvinceId,
                        principalTable: "Province",
                        principalColumn: "ProvinceId");
                    table.ForeignKey(
                        name: "FK_UrbanTreasures_Users",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "UserPost",
                columns: table => new
                {
                    PostId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    MediaPath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Caption = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__UserPost__AA12601833B44BC8", x => x.PostId);
                    table.ForeignKey(
                        name: "FK_User",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "WeeklyContest",
                columns: table => new
                {
                    ContestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContestType = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Instructions = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ContestPhase = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    WinnerUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__WeeklyCo__87DE0B1A91583540", x => x.ContestId);
                    table.ForeignKey(
                        name: "FK__WeeklyCon__UserI__76619304",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "PostLike",
                columns: table => new
                {
                    LikeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__PostLike__A2922C143D89E62B", x => x.LikeId);
                    table.ForeignKey(
                        name: "FK_PostLike",
                        column: x => x.PostId,
                        principalTable: "UserPost",
                        principalColumn: "PostId");
                    table.ForeignKey(
                        name: "FK_UserLike",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "UserComment",
                columns: table => new
                {
                    CommentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__UserComm__C3B4DFCA370A8030", x => x.CommentId);
                    table.ForeignKey(
                        name: "FK_Post",
                        column: x => x.PostId,
                        principalTable: "UserPost",
                        principalColumn: "PostId");
                    table.ForeignKey(
                        name: "FK_UserComment",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "ContestEntry",
                columns: table => new
                {
                    ContestEntryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContestId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Submission = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VoteCount = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Descriptions = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ContestE__C402F8B8BF22DB27", x => x.ContestEntryId);
                    table.ForeignKey(
                        name: "FK_ContestEntry_User",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContestEntry_WeeklyContest",
                        column: x => x.ContestId,
                        principalTable: "WeeklyContest",
                        principalColumn: "ContestId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserVote",
                columns: table => new
                {
                    UserVoteId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ContestEntryId = table.Column<int>(type: "int", nullable: false),
                    VoteDate = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__UserVote__0F0C36EC326F3BEC", x => x.UserVoteId);
                    table.ForeignKey(
                        name: "FK__UserVote__Contes__4865BE2A",
                        column: x => x.ContestEntryId,
                        principalTable: "ContestEntry",
                        principalColumn: "ContestEntryId");
                    table.ForeignKey(
                        name: "FK__UserVote__UserId__477199F1",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_GroupId",
                table: "ChatMessages",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_UserId",
                table: "ChatMessages",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ContestEntry_ContestId",
                table: "ContestEntry",
                column: "ContestId");

            migrationBuilder.CreateIndex(
                name: "IX_ContestEntry_UserId",
                table: "ContestEntry",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_UserId",
                table: "Events",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupMembers_GroupId",
                table: "GroupMembers",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupMembers_UserId",
                table: "GroupMembers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PostLike_PostId",
                table: "PostLike",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_PostLike_UserId",
                table: "PostLike",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UrbanTreasures_ProvinceId",
                table: "UrbanTreasures",
                column: "ProvinceId");

            migrationBuilder.CreateIndex(
                name: "IX_UrbanTreasures_UserId",
                table: "UrbanTreasures",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserComment_PostId",
                table: "UserComment",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_UserComment_UserId",
                table: "UserComment",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPost_UserId",
                table: "UserPost",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "UQ__Users__A9D1053431CFB59C",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserVote_ContestEntryId",
                table: "UserVote",
                column: "ContestEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_UserVote_UserId",
                table: "UserVote",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyContest_UserId",
                table: "WeeklyContest",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatMessages");

            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropTable(
                name: "GroupMembers");

            migrationBuilder.DropTable(
                name: "Locations");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "PostLike");

            migrationBuilder.DropTable(
                name: "UrbanTreasures");

            migrationBuilder.DropTable(
                name: "UserComment");

            migrationBuilder.DropTable(
                name: "UserVote");

            migrationBuilder.DropTable(
                name: "WeatherForecasts");

            migrationBuilder.DropTable(
                name: "Groups");

            migrationBuilder.DropTable(
                name: "Province");

            migrationBuilder.DropTable(
                name: "UserPost");

            migrationBuilder.DropTable(
                name: "ContestEntry");

            migrationBuilder.DropTable(
                name: "WeeklyContest");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
