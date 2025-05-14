using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MentalHealthApis.Migrations
{
    /// <inheritdoc />
    public partial class SecoundryCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$12$Cz3TQXWv5kMOJ2LF5pnz/eQU7jsffTQOjhsYQkR0w7O7PRlC/X5Y6");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$bG.i5npcV.rrz.r82e2PRuZLpemEcRhd/mNATIPOA1DWHo/rQba.2");
        }
    }
}
