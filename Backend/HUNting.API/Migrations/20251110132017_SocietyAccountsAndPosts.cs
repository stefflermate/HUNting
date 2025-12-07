using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HUNting.API.Migrations
{
    /// <inheritdoc />
    public partial class SocietyAccountsAndPosts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OwnerUserId",
                table: "Societies",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AccountType",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "SocietyMembershipRequests",
                columns: table => new
                {
                    SocietyId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProcessedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SocietyMembershipRequests", x => new { x.SocietyId, x.UserId });
                    table.ForeignKey(
                        name: "FK_SocietyMembershipRequests_AspNetUsers_ProcessedByUserId",
                        column: x => x.ProcessedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SocietyMembershipRequests_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SocietyMembershipRequests_Societies_SocietyId",
                        column: x => x.SocietyId,
                        principalTable: "Societies",
                        principalColumn: "SocietyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SocietyPosts",
                columns: table => new
                {
                    PostId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SocietyId = table.Column<int>(type: "int", nullable: false),
                    AuthorUserId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SocietyPosts", x => x.PostId);
                    table.ForeignKey(
                        name: "FK_SocietyPosts_AspNetUsers_AuthorUserId",
                        column: x => x.AuthorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SocietyPosts_Societies_SocietyId",
                        column: x => x.SocietyId,
                        principalTable: "Societies",
                        principalColumn: "SocietyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Societies_OwnerUserId",
                table: "Societies",
                column: "OwnerUserId",
                unique: true,
                filter: "[OwnerUserId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SocietyMembershipRequests_ProcessedByUserId",
                table: "SocietyMembershipRequests",
                column: "ProcessedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SocietyMembershipRequests_UserId",
                table: "SocietyMembershipRequests",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SocietyPosts_AuthorUserId",
                table: "SocietyPosts",
                column: "AuthorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SocietyPosts_SocietyId",
                table: "SocietyPosts",
                column: "SocietyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Societies_AspNetUsers_OwnerUserId",
                table: "Societies",
                column: "OwnerUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Societies_AspNetUsers_OwnerUserId",
                table: "Societies");

            migrationBuilder.DropTable(
                name: "SocietyMembershipRequests");

            migrationBuilder.DropTable(
                name: "SocietyPosts");

            migrationBuilder.DropIndex(
                name: "IX_Societies_OwnerUserId",
                table: "Societies");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "Societies");

            migrationBuilder.DropColumn(
                name: "AccountType",
                table: "AspNetUsers");
        }
    }
}
