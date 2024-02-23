using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EncryptLocker.Database.Migrations
{
    /// <inheritdoc />
    public partial class _00001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CypherValues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Cypher = table.Column<byte[]>(type: "varbinary(2048)", maxLength: 2048, nullable: false),
                    Tag = table.Column<byte[]>(type: "varbinary(256)", maxLength: 256, nullable: false),
                    IV = table.Column<byte[]>(type: "varbinary(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CypherValues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Lockers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    KeyHash = table.Column<byte[]>(type: "varbinary(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lockers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RegisteredUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AzureID = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegisteredUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SafeBases",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Discriminator = table.Column<int>(type: "int", nullable: false),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    TitleId = table.Column<int>(type: "int", nullable: false),
                    LockerId = table.Column<int>(type: "int", nullable: true),
                    LoginId = table.Column<int>(type: "int", nullable: true),
                    PasswordId = table.Column<int>(type: "int", nullable: true),
                    UrlId = table.Column<int>(type: "int", nullable: true),
                    NoteId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SafeBases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SafeBases_CypherValues_LoginId",
                        column: x => x.LoginId,
                        principalTable: "CypherValues",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SafeBases_CypherValues_NoteId",
                        column: x => x.NoteId,
                        principalTable: "CypherValues",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SafeBases_CypherValues_PasswordId",
                        column: x => x.PasswordId,
                        principalTable: "CypherValues",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SafeBases_CypherValues_TitleId",
                        column: x => x.TitleId,
                        principalTable: "CypherValues",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SafeBases_CypherValues_UrlId",
                        column: x => x.UrlId,
                        principalTable: "CypherValues",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SafeBases_Lockers_LockerId",
                        column: x => x.LockerId,
                        principalTable: "Lockers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SafeBases_SafeBases_ParentId",
                        column: x => x.ParentId,
                        principalTable: "SafeBases",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LockerAccessRights",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LockerId = table.Column<int>(type: "int", nullable: false),
                    RegisteredUserId = table.Column<int>(type: "int", nullable: false),
                    AccessType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LockerAccessRights", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LockerAccessRights_Lockers_LockerId",
                        column: x => x.LockerId,
                        principalTable: "Lockers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LockerAccessRights_RegisteredUsers_RegisteredUserId",
                        column: x => x.RegisteredUserId,
                        principalTable: "RegisteredUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PasswordReadLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SafeEntryId = table.Column<int>(type: "int", nullable: false),
                    RegisteredUserId = table.Column<int>(type: "int", nullable: false),
                    DateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordReadLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PasswordReadLogs_RegisteredUsers_RegisteredUserId",
                        column: x => x.RegisteredUserId,
                        principalTable: "RegisteredUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PasswordReadLogs_SafeBases_SafeEntryId",
                        column: x => x.SafeEntryId,
                        principalTable: "SafeBases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LockerAccessRights_LockerId",
                table: "LockerAccessRights",
                column: "LockerId");

            migrationBuilder.CreateIndex(
                name: "IX_LockerAccessRights_RegisteredUserId",
                table: "LockerAccessRights",
                column: "RegisteredUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordReadLogs_RegisteredUserId",
                table: "PasswordReadLogs",
                column: "RegisteredUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordReadLogs_SafeEntryId",
                table: "PasswordReadLogs",
                column: "SafeEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_SafeBases_LockerId",
                table: "SafeBases",
                column: "LockerId");

            migrationBuilder.CreateIndex(
                name: "IX_SafeBases_LoginId",
                table: "SafeBases",
                column: "LoginId");

            migrationBuilder.CreateIndex(
                name: "IX_SafeBases_NoteId",
                table: "SafeBases",
                column: "NoteId");

            migrationBuilder.CreateIndex(
                name: "IX_SafeBases_ParentId",
                table: "SafeBases",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_SafeBases_PasswordId",
                table: "SafeBases",
                column: "PasswordId");

            migrationBuilder.CreateIndex(
                name: "IX_SafeBases_TitleId",
                table: "SafeBases",
                column: "TitleId");

            migrationBuilder.CreateIndex(
                name: "IX_SafeBases_UrlId",
                table: "SafeBases",
                column: "UrlId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LockerAccessRights");

            migrationBuilder.DropTable(
                name: "PasswordReadLogs");

            migrationBuilder.DropTable(
                name: "RegisteredUsers");

            migrationBuilder.DropTable(
                name: "SafeBases");

            migrationBuilder.DropTable(
                name: "CypherValues");

            migrationBuilder.DropTable(
                name: "Lockers");
        }
    }
}
