using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Intertwine.Repositories.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonalityTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PersonalityTypeId",
                table: "UserProfiles",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PersonalityTypes",
                columns: table => new
                {
                    PersonalityTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(4)", unicode: false, maxLength: 4, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    DateUpdated = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonalityTypes", x => x.PersonalityTypeId);
                });

            migrationBuilder.InsertData(
                table: "PersonalityTypes",
                columns: new[] { "Code", "CreatedBy" },
                values: new object[,]
                {
                    { "INTJ", "AddPersonalityTypes" },
                    { "INTP", "AddPersonalityTypes" },
                    { "ENTJ", "AddPersonalityTypes" },
                    { "ENTP", "AddPersonalityTypes" },
                    { "INFJ", "AddPersonalityTypes" },
                    { "INFP", "AddPersonalityTypes" },
                    { "ENFJ", "AddPersonalityTypes" },
                    { "ENFP", "AddPersonalityTypes" },
                    { "ISTJ", "AddPersonalityTypes" },
                    { "ISFJ", "AddPersonalityTypes" },
                    { "ESTJ", "AddPersonalityTypes" },
                    { "ESFJ", "AddPersonalityTypes" },
                    { "ISTP", "AddPersonalityTypes" },
                    { "ISFP", "AddPersonalityTypes" },
                    { "ESTP", "AddPersonalityTypes" },
                    { "ESFP", "AddPersonalityTypes" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_PersonalityTypeId",
                table: "UserProfiles",
                column: "PersonalityTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonalityTypes_Code",
                table: "PersonalityTypes",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UserProfiles_PersonalityTypes_PersonalityTypeId",
                table: "UserProfiles",
                column: "PersonalityTypeId",
                principalTable: "PersonalityTypes",
                principalColumn: "PersonalityTypeId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserProfiles_PersonalityTypes_PersonalityTypeId",
                table: "UserProfiles");

            migrationBuilder.DropTable(
                name: "PersonalityTypes");

            migrationBuilder.DropIndex(
                name: "IX_UserProfiles_PersonalityTypeId",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "PersonalityTypeId",
                table: "UserProfiles");
        }
    }
}
