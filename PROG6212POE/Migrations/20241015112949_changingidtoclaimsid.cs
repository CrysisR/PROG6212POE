using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PROG6212POE.Migrations
{
    /// <inheritdoc />
    public partial class changingidtoclaimsid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Files_Claims_ClaimsId",
                table: "Files");

            migrationBuilder.RenameColumn(
                name: "ClaimsId",
                table: "Files",
                newName: "ClaimsClaimId");

            migrationBuilder.RenameIndex(
                name: "IX_Files_ClaimsId",
                table: "Files",
                newName: "IX_Files_ClaimsClaimId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Claims",
                newName: "ClaimId");

            migrationBuilder.AddForeignKey(
                name: "FK_Files_Claims_ClaimsClaimId",
                table: "Files",
                column: "ClaimsClaimId",
                principalTable: "Claims",
                principalColumn: "ClaimId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Files_Claims_ClaimsClaimId",
                table: "Files");

            migrationBuilder.RenameColumn(
                name: "ClaimsClaimId",
                table: "Files",
                newName: "ClaimsId");

            migrationBuilder.RenameIndex(
                name: "IX_Files_ClaimsClaimId",
                table: "Files",
                newName: "IX_Files_ClaimsId");

            migrationBuilder.RenameColumn(
                name: "ClaimId",
                table: "Claims",
                newName: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Files_Claims_ClaimsId",
                table: "Files",
                column: "ClaimsId",
                principalTable: "Claims",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
