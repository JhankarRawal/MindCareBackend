using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MentalHealthApis.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusToDoctor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdminNotes",
                table: "Doctors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApplicationStatus",
                table: "Doctors",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdminNotes",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "ApplicationStatus",
                table: "Doctors");
        }
    }
}
