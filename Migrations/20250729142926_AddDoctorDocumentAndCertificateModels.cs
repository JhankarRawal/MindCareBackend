using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MentalHealthApis.Migrations
{
    /// <inheritdoc />
    public partial class AddDoctorDocumentAndCertificateModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CitizenshipBackPath",
                table: "Doctors",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CitizenshipFrontPath",
                table: "Doctors",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocumentSubmissionStatus",
                table: "Doctors",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasAcceptedTerms",
                table: "Doctors",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PasswordSizedPhotoPath",
                table: "Doctors",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DoctorCertificates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FilePath = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    DoctorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorCertificates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DoctorCertificates_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DoctorCertificates_DoctorId",
                table: "DoctorCertificates",
                column: "DoctorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DoctorCertificates");

            migrationBuilder.DropColumn(
                name: "CitizenshipBackPath",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "CitizenshipFrontPath",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "DocumentSubmissionStatus",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "HasAcceptedTerms",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "PasswordSizedPhotoPath",
                table: "Doctors");
        }
    }
}
