using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HUNting.API.Migrations
{
    /// <inheritdoc />
    public partial class AddMapEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MapIconTypes",
                columns: table => new
                {
                    IconTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DefaultColor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Glyph = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MapIconTypes", x => x.IconTypeId);
                });

            migrationBuilder.CreateTable(
                name: "MapVisibilities",
                columns: table => new
                {
                    VisibilityCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MapVisibilities", x => x.VisibilityCode);
                });

            migrationBuilder.CreateTable(
                name: "MapMarkers",
                columns: table => new
                {
                    MarkerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    SocietyId = table.Column<int>(type: "int", nullable: true),
                    IconTypeId = table.Column<int>(type: "int", nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    Longitude = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    VisibilityCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "sysdatetime()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MapMarkers", x => x.MarkerId);
                    table.ForeignKey(
                        name: "FK_MapMarkers_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MapMarkers_MapIconTypes_IconTypeId",
                        column: x => x.IconTypeId,
                        principalTable: "MapIconTypes",
                        principalColumn: "IconTypeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MapMarkers_MapVisibilities_VisibilityCode",
                        column: x => x.VisibilityCode,
                        principalTable: "MapVisibilities",
                        principalColumn: "VisibilityCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MapMarkers_Societies_SocietyId",
                        column: x => x.SocietyId,
                        principalTable: "Societies",
                        principalColumn: "SocietyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MapPaths",
                columns: table => new
                {
                    PathId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    SocietyId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    VisibilityCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StrokeColor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "sysdatetime()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MapPaths", x => x.PathId);
                    table.ForeignKey(
                        name: "FK_MapPaths_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MapPaths_MapVisibilities_VisibilityCode",
                        column: x => x.VisibilityCode,
                        principalTable: "MapVisibilities",
                        principalColumn: "VisibilityCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MapPaths_Societies_SocietyId",
                        column: x => x.SocietyId,
                        principalTable: "Societies",
                        principalColumn: "SocietyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MapPolygons",
                columns: table => new
                {
                    PolygonId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    SocietyId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    VisibilityCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StrokeColor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FillColor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "sysdatetime()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MapPolygons", x => x.PolygonId);
                    table.ForeignKey(
                        name: "FK_MapPolygons_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MapPolygons_MapVisibilities_VisibilityCode",
                        column: x => x.VisibilityCode,
                        principalTable: "MapVisibilities",
                        principalColumn: "VisibilityCode",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MapPolygons_Societies_SocietyId",
                        column: x => x.SocietyId,
                        principalTable: "Societies",
                        principalColumn: "SocietyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MapPathPoints",
                columns: table => new
                {
                    PathPointId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PathId = table.Column<int>(type: "int", nullable: false),
                    Seq = table.Column<int>(type: "int", nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    Longitude = table.Column<decimal>(type: "decimal(9,6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MapPathPoints", x => x.PathPointId);
                    table.ForeignKey(
                        name: "FK_MapPathPoints_MapPaths_PathId",
                        column: x => x.PathId,
                        principalTable: "MapPaths",
                        principalColumn: "PathId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MapPolygonPoints",
                columns: table => new
                {
                    PolygonPointId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PolygonId = table.Column<int>(type: "int", nullable: false),
                    Seq = table.Column<int>(type: "int", nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    Longitude = table.Column<decimal>(type: "decimal(9,6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MapPolygonPoints", x => x.PolygonPointId);
                    table.ForeignKey(
                        name: "FK_MapPolygonPoints_MapPolygons_PolygonId",
                        column: x => x.PolygonId,
                        principalTable: "MapPolygons",
                        principalColumn: "PolygonId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MapMarkers_CreatedByUserId",
                table: "MapMarkers",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MapMarkers_IconTypeId",
                table: "MapMarkers",
                column: "IconTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_MapMarkers_SocietyId",
                table: "MapMarkers",
                column: "SocietyId");

            migrationBuilder.CreateIndex(
                name: "IX_MapMarkers_VisibilityCode",
                table: "MapMarkers",
                column: "VisibilityCode");

            migrationBuilder.CreateIndex(
                name: "IX_MapPathPoints_PathId_Seq",
                table: "MapPathPoints",
                columns: new[] { "PathId", "Seq" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MapPaths_CreatedByUserId",
                table: "MapPaths",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MapPaths_SocietyId",
                table: "MapPaths",
                column: "SocietyId");

            migrationBuilder.CreateIndex(
                name: "IX_MapPaths_VisibilityCode",
                table: "MapPaths",
                column: "VisibilityCode");

            migrationBuilder.CreateIndex(
                name: "IX_MapPolygonPoints_PolygonId_Seq",
                table: "MapPolygonPoints",
                columns: new[] { "PolygonId", "Seq" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MapPolygons_CreatedByUserId",
                table: "MapPolygons",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MapPolygons_SocietyId",
                table: "MapPolygons",
                column: "SocietyId");

            migrationBuilder.CreateIndex(
                name: "IX_MapPolygons_VisibilityCode",
                table: "MapPolygons",
                column: "VisibilityCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MapMarkers");

            migrationBuilder.DropTable(
                name: "MapPathPoints");

            migrationBuilder.DropTable(
                name: "MapPolygonPoints");

            migrationBuilder.DropTable(
                name: "MapIconTypes");

            migrationBuilder.DropTable(
                name: "MapPaths");

            migrationBuilder.DropTable(
                name: "MapPolygons");

            migrationBuilder.DropTable(
                name: "MapVisibilities");
        }
    }
}
