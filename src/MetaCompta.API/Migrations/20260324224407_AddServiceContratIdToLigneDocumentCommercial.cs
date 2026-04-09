using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MetaCompta.API.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceContratIdToLigneDocumentCommercial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ServiceContratId",
                table: "LignesDocumentsCommerciaux",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 321, DateTimeKind.Utc).AddTicks(9765));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(415));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(427));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(432));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(435));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(525));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(531));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(536));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(540));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(544));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 11,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(562));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 12,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(567));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 13,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(570));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 14,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(574));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 15,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(578));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 16,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(585));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 17,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(588));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 18,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(592));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 19,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(599));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 20,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(604));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 21,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(606));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 22,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(609));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 23,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(612));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 24,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(615));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 25,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(617));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 26,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(621));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 27,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(629));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 28,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(632));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 29,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(635));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 30,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(681));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 31,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(700));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 32,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(705));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 33,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(708));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 34,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(714));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 35,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(717));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 36,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(720));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 37,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(723));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 38,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(726));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 39,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(729));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 40,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(733));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 41,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(735));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 42,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(739));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 43,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(743));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 44,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(747));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 45,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(750));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 46,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(754));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 47,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(757));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 48,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(761));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 49,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(764));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 50,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(767));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 51,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(770));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 52,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(774));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 53,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(776));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 54,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(813));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 55,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(817));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 56,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(821));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 57,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(824));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 58,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(827));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 59,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(832));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 60,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(836));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 61,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(838));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 62,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(842));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 63,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(847));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 64,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(851));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 65,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(855));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 66,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(860));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 67,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(864));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 68,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(867));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 69,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(870));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 70,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(873));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 71,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(875));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 72,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(878));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 73,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(881));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 74,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(884));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 75,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(888));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 76,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(922));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 77,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(925));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 78,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(929));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 79,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(933));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 80,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(937));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 81,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(940));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(2331));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(3401));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(3405));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(3407));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(3410));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(3412));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(3448));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(3474));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(3514));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(3517));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 11,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(3520));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 12,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(3727));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 13,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(3730));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 14,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(3733));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 15,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(3735));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 16,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(3737));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 17,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(3757));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 18,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(3759));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 19,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(3761));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 20,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(3763));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 21,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(3766));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 22,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(3768));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 23,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4004));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 24,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4007));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 25,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4009));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 26,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4011));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 27,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4013));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 28,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4061));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 29,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4064));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 30,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4066));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 31,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4068));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 32,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4016));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 33,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4054));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 34,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4057));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 35,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4059));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 36,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4070));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 37,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4072));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 38,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4074));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 39,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4089));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 40,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4091));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 41,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4109));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 42,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4111));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 43,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4248));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 44,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4251));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 45,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4289));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 46,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4292));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 47,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4294));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 48,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4297));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 49,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4299));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 50,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4302));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 51,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4304));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 52,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4306));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 53,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4323));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 54,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4326));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 55,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4328));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 56,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4331));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 57,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4333));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 58,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4335));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 59,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4338));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 60,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4340));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 61,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4364));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 62,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4366));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 63,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4368));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 64,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4370));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 65,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4372));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 66,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4374));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 67,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4376));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 68,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4378));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 69,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4380));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 70,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4382));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 71,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4384));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 72,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4385));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 73,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4387));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 74,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4400));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 75,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4402));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 76,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4438));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 77,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4440));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 78,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4442));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 79,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4444));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 80,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4446));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 81,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4448));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 82,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4450));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 83,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4451));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 84,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4453));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 85,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4455));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 86,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4457));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 87,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4459));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 88,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4474));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 89,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4476));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 90,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4478));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 91,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4480));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 92,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4482));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 93,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4484));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 94,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4487));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 95,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4489));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 96,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4491));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 97,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4493));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 98,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4495));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 99,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4497));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 100,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4500));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 101,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4514));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 102,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4516));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 103,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4519));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 104,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4521));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 105,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4523));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 106,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4526));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 107,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4528));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 108,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4530));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 109,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4532));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 110,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4535));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 111,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4537));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 112,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4566));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 113,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4580));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 114,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4583));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 115,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4585));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 116,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4587));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 117,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4589));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 118,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4591));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 119,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4593));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 120,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4595));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 121,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4597));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 122,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4599));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 123,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4601));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 124,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4603));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 125,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4604));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 126,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4606));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 127,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4620));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 128,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4622));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 129,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4625));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 130,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4627));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 131,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4629));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 132,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4631));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 133,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4634));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 200,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4636));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 201,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4638));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 202,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4640));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 203,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4643));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 204,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4645));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 205,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4659));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 210,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4661));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 211,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4663));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 212,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4666));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 213,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4668));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 220,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4670));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 221,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4672));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 222,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4720));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 223,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4723));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 224,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4726));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 225,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4728));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 226,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4742));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 227,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4745));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 228,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4747));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 230,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4749));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 231,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4752));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 232,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4754));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 233,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4756));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 234,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4758));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 235,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4761));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 236,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4763));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 237,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4765));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 238,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4767));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 240,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4782));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 241,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4785));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 242,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4787));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 250,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4789));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 251,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4791));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 255,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4794));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 256,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4796));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 257,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4798));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 260,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4800));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 261,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4802));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 262,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4804));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 263,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4806));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 264,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4821));

            migrationBuilder.InsertData(
                table: "ReferenceData",
                columns: new[] { "Id", "CabinetId", "Category", "Code", "Couleur", "CreatedAt", "CreatedBy", "CssClass", "DeletedAt", "DeletedBy", "EffectiveFrom", "EffectiveTo", "Icone", "IsActive", "IsDeleted", "Label", "LabelAr", "LabelEn", "Metadata", "NumericValue", "SortOrder", "UpdatedAt", "UpdatedBy", "Value" },
                values: new object[,]
                {
                    { 270, null, "TYPE_SERVICE", "TVA", "#2563eb", new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4823), null, "bg-primary-subtle text-primary", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-receipt", true, false, "Taxe sur la Valeur Ajoutée", null, null, null, null, 1, null, null, null },
                    { 271, null, "TYPE_SERVICE", "IS", "#7c3aed", new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4825), null, "bg-purple-subtle text-purple", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-building", true, false, "Impôt sur les Sociétés", null, null, null, null, 2, null, null, null },
                    { 272, null, "TYPE_SERVICE", "IR", "#059669", new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4827), null, "bg-success-subtle text-success", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-person", true, false, "Impôt sur le Revenu", null, null, null, null, 3, null, null, null },
                    { 273, null, "TYPE_SERVICE", "CNSS", "#0891b2", new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4830), null, "bg-info-subtle text-info", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-shield", true, false, "Caisse Nationale de Sécurité Sociale", null, null, null, null, 4, null, null, null },
                    { 274, null, "TYPE_SERVICE", "PAIE", "#d97706", new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4832), null, "bg-warning-subtle text-warning", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-cash-stack", true, false, "Gestion de la Paie", null, null, null, null, 5, null, null, null },
                    { 275, null, "TYPE_SERVICE", "JURIDIQUE", "#dc2626", new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4834), null, "bg-danger-subtle text-danger", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-briefcase", true, false, "Dossiers Juridiques", null, null, null, null, 6, null, null, null },
                    { 276, null, "TYPE_SERVICE", "COMPTABILITE", "#14b8a6", new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4876), null, "bg-teal-subtle text-teal", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-journal-text", true, false, "Tenue Comptable", null, null, null, null, 7, null, null, null },
                    { 277, null, "TYPE_SERVICE", "BILAN", "#6366f1", new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4879), null, "bg-indigo-subtle text-indigo", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-bar-chart-line", true, false, "Bilan & États de Synthèse", null, null, null, null, 8, null, null, null },
                    { 278, null, "TYPE_SERVICE", "ETATSFISCAUX", "#ec4899", new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4881), null, "bg-pink-subtle text-pink", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-file-earmark-spreadsheet", true, false, "États Fiscaux", null, null, null, null, 9, null, null, null },
                    { 279, null, "TYPE_SERVICE", "AUDITCOMPTABLE", "#64748b", new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4883), null, "bg-secondary-subtle text-secondary", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-search", true, false, "Audit Comptable", null, null, null, null, 10, null, null, null },
                    { 280, null, "TYPE_SERVICE", "CONSEILFISCAL", "#f59e0b", new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4886), null, "bg-warning-subtle text-warning", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-lightbulb", true, false, "Conseil Fiscal", null, null, null, null, 11, null, null, null },
                    { 285, null, "CATEGORIE_SERVICE", "Fiscal", "#7c3aed", new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4888), null, "bg-purple-subtle text-purple", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-calculator", true, false, "Fiscal", null, null, null, null, 1, null, null, null },
                    { 286, null, "CATEGORIE_SERVICE", "Social", "#0891b2", new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4890), null, "bg-info-subtle text-info", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-people", true, false, "Social", null, null, null, null, 2, null, null, null },
                    { 287, null, "CATEGORIE_SERVICE", "Comptable", "#059669", new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4893), null, "bg-success-subtle text-success", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-journal-text", true, false, "Comptable", null, null, null, null, 3, null, null, null },
                    { 288, null, "CATEGORIE_SERVICE", "Juridique", "#dc2626", new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4895), null, "bg-danger-subtle text-danger", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-briefcase", true, false, "Juridique", null, null, null, null, 4, null, null, null },
                    { 289, null, "CATEGORIE_SERVICE", "Conseil", "#f59e0b", new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4897), null, "bg-warning-subtle text-warning", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-lightbulb", true, false, "Conseil", null, null, null, null, 5, null, null, null },
                    { 290, null, "CATEGORIE_SERVICE", "Autre", "#64748b", new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4899), null, "bg-secondary-subtle text-secondary", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-three-dots", true, false, "Autre", null, null, null, null, 99, null, null, null },
                    { 295, null, "FREQUENCE", "Mensuelle", "#2563eb", new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4902), null, "bg-primary-subtle text-primary", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-calendar-month", true, false, "Mensuelle", null, null, null, null, 1, null, null, null },
                    { 296, null, "FREQUENCE", "Trimestrielle", "#7c3aed", new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4904), null, "bg-purple-subtle text-purple", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-calendar3", true, false, "Trimestrielle", null, null, null, null, 2, null, null, null },
                    { 297, null, "FREQUENCE", "Annuelle", "#059669", new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4906), null, "bg-success-subtle text-success", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-calendar-event", true, false, "Annuelle", null, null, null, null, 3, null, null, null },
                    { 298, null, "FREQUENCE", "Ponctuelle", "#f59e0b", new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4908), null, "bg-warning-subtle text-warning", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-calendar-check", true, false, "Ponctuelle", null, null, null, null, 4, null, null, null },
                    { 300, null, "STATUT_DECLARATION", "EnCours", "#3b82f6", new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4342), null, "badge-acompleter", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-arrow-repeat", true, false, "En Cours", null, null, null, null, 8, null, null, null },
                    { 301, null, "STATUT_DECLARATION", "EnPreparation", "#8b5cf6", new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4345), null, "badge-enrevision", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-gear", true, false, "En Préparation", null, null, null, null, 9, null, null, null },
                    { 302, null, "STATUT_DECLARATION", "Payee", "#047857", new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4347), null, "badge-soumise", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-cash-coin", true, false, "Payée", null, null, null, null, 10, null, null, null },
                    { 303, null, "STATUT_DECLARATION", "Rejetee", "#b91c1c", new DateTime(2026, 3, 24, 22, 44, 5, 322, DateTimeKind.Utc).AddTicks(4362), null, "badge-refusee", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-x-octagon", true, false, "Rejetée", null, null, null, null, 11, null, null, null }
                });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 321, DateTimeKind.Utc).AddTicks(1722));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 321, DateTimeKind.Utc).AddTicks(2893));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 22, 44, 5, 321, DateTimeKind.Utc).AddTicks(2896));

            migrationBuilder.CreateIndex(
                name: "IX_LignesDocumentsCommerciaux_ServiceContratId",
                table: "LignesDocumentsCommerciaux",
                column: "ServiceContratId");

            migrationBuilder.AddForeignKey(
                name: "FK_LignesDocumentsCommerciaux_ServiceContrats_ServiceContratId",
                table: "LignesDocumentsCommerciaux",
                column: "ServiceContratId",
                principalTable: "ServiceContrats",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LignesDocumentsCommerciaux_ServiceContrats_ServiceContratId",
                table: "LignesDocumentsCommerciaux");

            migrationBuilder.DropIndex(
                name: "IX_LignesDocumentsCommerciaux_ServiceContratId",
                table: "LignesDocumentsCommerciaux");

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 270);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 271);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 272);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 273);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 274);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 275);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 276);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 277);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 278);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 279);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 280);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 285);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 286);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 287);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 288);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 289);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 290);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 295);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 296);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 297);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 298);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 300);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 301);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 302);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 303);

            migrationBuilder.DropColumn(
                name: "ServiceContratId",
                table: "LignesDocumentsCommerciaux");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9087));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9618));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9622));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9626));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9628));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9632));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9637));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9641));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9644));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9647));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 11,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9651));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 12,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9653));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 13,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9655));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 14,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9659));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 15,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9709));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 16,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9712));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 17,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9715));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 18,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9718));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 19,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9721));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 20,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9724));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 21,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9726));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 22,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9729));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 23,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9731));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 24,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9733));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 25,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9735));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 26,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9738));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 27,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9741));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 28,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9743));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 29,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9745));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 30,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9748));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 31,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9750));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 32,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9753));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 33,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9755));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 34,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9759));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 35,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9761));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 36,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9763));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 37,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9766));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 38,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9768));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 39,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9803));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 40,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9807));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 41,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9809));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 42,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9811));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 43,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9815));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 44,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9818));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 45,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9821));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 46,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9825));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 47,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9826));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 48,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9829));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 49,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9831));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 50,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9833));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 51,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9836));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 52,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9839));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 53,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9841));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 54,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9843));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 55,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9846));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 56,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9848));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 57,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9850));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 58,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9853));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 59,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9855));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 60,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9858));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 61,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9860));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 62,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9863));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 63,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9866));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 64,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9886));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 65,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9890));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 66,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9893));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 67,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9897));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 68,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9900));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 69,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9902));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 70,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9905));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 71,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9907));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 72,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9909));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 73,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9912));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 74,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9914));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 75,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9918));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 76,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9920));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 77,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9922));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 78,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9924));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 79,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9927));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 80,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9929));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 81,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(9931));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(1035));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(1980));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(1983));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(1985));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(1987));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(1989));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2019));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2021));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2023));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2042));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 11,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2044));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 12,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2238));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 13,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2241));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 14,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2243));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 15,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2245));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 16,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2248));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 17,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2250));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 18,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2251));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 19,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2266));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 20,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2268));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 21,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2270));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 22,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2272));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 23,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2498));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 24,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2500));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 25,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2502));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 26,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2504));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 27,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2506));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 28,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2529));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 29,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2531));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 30,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2533));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 31,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2535));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 32,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2508));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 33,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2510));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 34,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2512));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 35,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2526));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 36,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2537));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 37,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2538));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 38,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2540));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 39,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2542));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 40,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2544));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 41,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2546));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 42,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2548));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 43,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2647));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 44,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2650));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 45,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2652));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 46,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2654));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 47,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2656));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 48,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2658));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 49,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2659));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 50,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2661));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 51,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2663));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 52,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2665));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 53,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2667));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 54,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2669));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 55,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2684));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 56,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2686));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 57,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2688));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 58,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2690));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 59,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2721));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 60,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2723));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 61,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2725));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 62,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2727));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 63,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2729));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 64,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2731));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 65,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2732));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 66,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2734));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 67,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2747));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 68,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2749));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 69,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2751));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 70,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2752));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 71,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2754));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 72,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2756));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 73,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2758));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 74,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2759));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 75,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2761));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 76,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2763));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 77,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2765));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 78,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2766));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 79,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2768));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 80,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2770));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 81,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2772));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 82,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2785));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 83,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2787));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 84,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2788));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 85,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2790));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 86,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2792));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 87,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2794));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 88,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2795));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 89,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2797));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 90,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2799));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 91,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2800));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 92,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2802));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 93,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2804));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 94,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2836));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 95,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2838));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 96,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2851));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 97,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2853));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 98,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2855));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 99,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2857));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 100,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2859));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 101,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2861));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 102,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2863));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 103,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2865));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 104,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2867));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 105,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2869));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 106,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2871));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 107,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2883));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 108,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2886));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 109,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2888));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 110,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2890));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 111,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2892));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 112,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2894));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 113,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2896));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 114,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2898));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 115,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2900));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 116,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2902));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 117,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2904));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 118,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2905));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 119,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2907));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 120,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2919));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 121,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2921));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 122,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2922));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 123,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2924));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 124,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2926));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 125,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2927));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 126,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2929));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 127,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2930));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 128,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2932));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 129,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2962));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 130,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2964));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 131,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2966));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 132,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2968));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 133,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2980));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 200,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2982));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 201,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2984));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 202,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2986));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 203,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2988));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 204,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2990));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 205,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2992));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 210,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2994));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 211,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2996));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 212,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(2998));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 213,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(3000));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 220,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(3002));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 221,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(3013));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 222,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(3015));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 223,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(3017));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 224,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(3019));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 225,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(3021));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 226,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(3023));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 227,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(3025));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 228,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(3027));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 230,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(3029));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 231,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(3031));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 232,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(3033));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 233,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(3044));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 234,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(3046));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 235,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(3048));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 236,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(3050));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 237,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(3053));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 238,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(3054));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 240,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(3056));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 241,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(3058));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 242,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(3060));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 250,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(3089));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 251,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(3091));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 255,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(3093));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 256,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(3095));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 257,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(3097));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 260,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(3099));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 261,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(3101));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 262,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(3103));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 263,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(3105));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 264,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 417, DateTimeKind.Utc).AddTicks(3106));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(1966));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(3029));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 20, 18, 48, 416, DateTimeKind.Utc).AddTicks(3043));
        }
    }
}
