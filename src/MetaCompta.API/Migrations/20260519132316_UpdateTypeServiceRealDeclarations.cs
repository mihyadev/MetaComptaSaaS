using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MetaCompta.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTypeServiceRealDeclarations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(7415));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(7815));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(7818));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(7821));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(7823));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(7827));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(7831));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(7834));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(7837));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(7839));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 11,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(7843));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 12,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(7895));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 13,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(7898));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 14,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(7901));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 15,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(7904));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 16,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(7907));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 17,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(7909));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 18,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(7912));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 19,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(7915));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 20,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(7918));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 21,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(7919));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 22,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(7921));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 23,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(7923));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 24,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(7925));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 25,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(7927));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 26,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(7929));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 27,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(7931));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 28,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(7934));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 29,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(7936));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 30,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(7938));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 31,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(7940));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 32,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(7942));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 33,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(7944));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 34,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(7947));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 35,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(7976));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 36,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(7978));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 37,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(7980));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 38,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(7982));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 39,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(7984));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 40,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(7987));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 41,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(7989));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 42,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(7991));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 43,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(7994));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 44,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(7996));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 45,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(7998));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 46,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(8001));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 47,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(8003));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 48,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(8005));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 49,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(8007));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 50,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(8009));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 51,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(8012));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 52,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(8014));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 53,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(8016));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 54,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(8018));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 55,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(8020));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 56,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(8022));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 57,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(8024));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 58,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(8025));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 59,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(8028));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 60,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(8030));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 61,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(8049));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 62,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(8052));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 63,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(8055));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 64,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(8057));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 65,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(8059));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 66,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(8062));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 67,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(8066));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 68,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(8068));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 69,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(8070));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 70,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(8072));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 71,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(8074));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 72,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(8077));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 73,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(8078));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 74,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(8081));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 75,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(8083));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 76,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(8085));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 77,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(8087));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 78,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(8089));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 79,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(8091));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 80,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(8092));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 81,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(8094));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(9067));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(9820));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(9837));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(9839));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(9841));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(9842));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(9869));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(9871));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(9872));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(9874));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 11,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(9875));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 12,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(25));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 13,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(40));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 14,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(42));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 15,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(44));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 16,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(46));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 17,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(48));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 18,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(91));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 19,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(93));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 20,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(95));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 21,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(96));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 22,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(98));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 23,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(270));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 24,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(272));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 25,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(274));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 26,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(275));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 27,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(277));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 28,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(285));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 29,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(286));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 30,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(288));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 31,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(298));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 32,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(278));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 33,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(280));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 34,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(282));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 35,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(283));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 36,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(300));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 37,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(301));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 38,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(303));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 39,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(304));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 40,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(306));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 41,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(307));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 42,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(309));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 43,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(378));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 44,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(380));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 45,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(382));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 46,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(384));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 47,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(386));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 48,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(396));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 49,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(398));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 50,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(400));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 51,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(401));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 52,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(403));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 53,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(436));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 54,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(438));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 55,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(439));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 56,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(441));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 57,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(443));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 58,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(444));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 59,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(455));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 60,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(457));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 61,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(465));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 62,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(467));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 63,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(469));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 64,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(470));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 65,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(471));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 66,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(473));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 67,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(483));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 68,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(484));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 69,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(485));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 70,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(487));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 71,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(488));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 72,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(490));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 73,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(491));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 74,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(492));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 75,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(494));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 76,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(495));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 77,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(496));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 78,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(498));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 79,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(499));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 80,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(501));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 81,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(502));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 82,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(511));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 83,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(513));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 84,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(514));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 85,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(542));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 86,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(543));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 87,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(545));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 88,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(546));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 89,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(548));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 90,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(549));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 91,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(550));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 92,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(552));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 93,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(553));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 94,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(555));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 95,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(557));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 96,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(567));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 97,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(569));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 98,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(570));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 99,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(572));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 100,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(574));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 101,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(575));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 102,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(577));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 103,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(578));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 104,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(580));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 105,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(582));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 106,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(583));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 107,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(585));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 108,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(595));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 109,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(596));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 110,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(598));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 111,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(600));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 112,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(601));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 113,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(603));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 114,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(605));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 115,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(606));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 116,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(608));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 117,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(609));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 118,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(610));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 119,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(612));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 120,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(647));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 121,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(649));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 122,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(650));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 123,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(652));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 124,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(653));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 125,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(654));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 126,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(656));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 127,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(657));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 128,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(658));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 129,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(660));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 130,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(662));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 131,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(663));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 132,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(665));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 133,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(676));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 200,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(677));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 201,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(679));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 202,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(681));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 203,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(682));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 204,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(684));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 205,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(686));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 210,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(687));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 211,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(689));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 212,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(690));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 213,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 220,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(694));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 221,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(703));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 222,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(705));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 223,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(707));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 224,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(708));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 225,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(710));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 226,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(711));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 227,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(713));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 228,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(715));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 230,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(716));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 231,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(718));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 232,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(747));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 233,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(758));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 234,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(760));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 235,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(761));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 236,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(763));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 237,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(765));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 238,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(767));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 240,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(768));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 241,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(770));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 242,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(772));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 250,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(773));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 251,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(775));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 255,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(777));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 256,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(786));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 257,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(788));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 260,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(790));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 261,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(792));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 262,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(793));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 263,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(795));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 264,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(796));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 270,
                columns: new[] { "Couleur", "CreatedAt", "CssClass", "Label" },
                values: new object[] { "#c2410c", new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(798), "bg-orange-subtle text-orange", "TVA – Taxe sur la Valeur Ajoutée" });

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 271,
                columns: new[] { "Code", "Couleur", "CreatedAt", "CssClass", "Icone", "Label" },
                values: new object[] { "CM", "#c2410c", new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(799), "bg-orange-subtle text-orange", "bi-calculator", "CM – Cotisation Minimale" });

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 272,
                columns: new[] { "Code", "Couleur", "CreatedAt", "CssClass", "Icone", "Label" },
                values: new object[] { "DRF", "#c2410c", new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(801), "bg-orange-subtle text-orange", "bi-file-earmark-text", "D.R.F – Déclaration du Résultat Fiscal" });

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 273,
                columns: new[] { "Code", "Couleur", "CreatedAt", "CssClass", "Icone", "Label" },
                values: new object[] { "ACOMPTES", "#c2410c", new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(803), "bg-orange-subtle text-orange", "bi-calendar-range", "Acomptes IS" });

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 274,
                columns: new[] { "Code", "Couleur", "CreatedAt", "CssClass", "Icone", "Label" },
                values: new object[] { "REG_IS", "#c2410c", new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(804), "bg-orange-subtle text-orange", "bi-arrow-repeat", "Régularisation IS – Reliquat d'Impôt" });

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 275,
                columns: new[] { "Code", "Couleur", "CreatedAt", "CssClass", "Icone", "Label" },
                values: new object[] { "IR_SAL", "#c2410c", new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(814), "bg-orange-subtle text-orange", "bi-person-lines-fill", "I.R/SALAIRE – IR Revenus Salariaux et assimilés" });

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 276,
                columns: new[] { "Code", "Couleur", "CreatedAt", "CssClass", "Icone", "Label" },
                values: new object[] { "DRG", "#c2410c", new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(816), "bg-orange-subtle text-orange", "bi-person-badge", "D.R.G – Déclaration du Revenu Global" });

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 277,
                columns: new[] { "Code", "Couleur", "CreatedAt", "CssClass", "Icone", "Label" },
                values: new object[] { "IR_COMP", "#c2410c", new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(818), "bg-orange-subtle text-orange", "bi-send", "IR/COMP – Versement de l'Impôt sur le Revenu" });

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 278,
                columns: new[] { "Code", "Couleur", "CreatedAt", "CssClass", "Icone", "Label" },
                values: new object[] { "RAS_FONC", "#c2410c", new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(819), "bg-orange-subtle text-orange", "bi-house", "R.A.S/FONCIER – Retenue à la source sur Revenus Fonciers" });

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 279,
                columns: new[] { "Code", "Couleur", "CreatedAt", "CssClass", "Icone", "Label" },
                values: new object[] { "RAS_DIV", "#c2410c", new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(821), "bg-orange-subtle text-orange", "bi-graph-up-arrow", "RAS/DIV – Retenue à la source sur Revenus de Capitaux" });

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 280,
                columns: new[] { "Code", "Couleur", "CreatedAt", "CssClass", "Icone", "Label" },
                values: new object[] { "DT", "#c2410c", new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(822), "bg-orange-subtle text-orange", "bi-stamp", "D.T – Droits de Timbre" });

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 285,
                columns: new[] { "Category", "Code", "Couleur", "CreatedAt", "CssClass", "Icone", "Label", "SortOrder" },
                values: new object[] { "TYPE_SERVICE", "CNSS", "#065f46", new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(830), "bg-success-subtle text-success", "bi-shield-shaded", "C.N.S.S – Caisse Nationale de Sécurité Sociale", 20 });

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 286,
                columns: new[] { "Category", "Code", "Couleur", "CreatedAt", "CssClass", "Icone", "Label", "SortOrder" },
                values: new object[] { "TYPE_SERVICE", "DTS", "#065f46", new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(858), "bg-success-subtle text-success", "bi-file-earmark-person", "D.T.S/État 9421 – Déclaration des Traitements et Salaires", 21 });

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 287,
                columns: new[] { "Category", "Code", "Couleur", "CreatedAt", "Icone", "Label", "SortOrder" },
                values: new object[] { "TYPE_SERVICE", "DCH_ETS", "#065f46", new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(869), "bi-building-x", "D.CH.ETS – Déclaration de Chômage d'Établissement", 22 });

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 288,
                columns: new[] { "Category", "Code", "Couleur", "CreatedAt", "Icone", "Label", "SortOrder" },
                values: new object[] { "TYPE_SERVICE", "PV_AGO", "#be123c", new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(871), "bi-card-checklist", "PV AGO – Procès Verbal Assemblée Générale Ordinaire", 30 });

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 289,
                columns: new[] { "Category", "Code", "Couleur", "CreatedAt", "CssClass", "Icone", "Label", "SortOrder" },
                values: new object[] { "TYPE_SERVICE", "COMPTABILITE", "#1d4ed8", new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(873), "bg-primary-subtle text-primary", "bi-journal-bookmark", "Saisie Comptable & Journaux", 40 });

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 290,
                columns: new[] { "Category", "Code", "Couleur", "CreatedAt", "CssClass", "Icone", "Label", "SortOrder" },
                values: new object[] { "TYPE_SERVICE", "TRESORERIE", "#1d4ed8", new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(874), "bg-primary-subtle text-primary", "bi-safe2", "Gestion de Trésorerie & Rapprochement Bancaire", 41 });

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 300,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(459));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 301,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(460));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 302,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(462));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 303,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(464));

            migrationBuilder.InsertData(
                table: "ReferenceData",
                columns: new[] { "Id", "CabinetId", "Category", "Code", "Couleur", "CreatedAt", "CreatedBy", "CssClass", "DeletedAt", "DeletedBy", "EffectiveFrom", "EffectiveTo", "Icone", "IsActive", "IsDeleted", "Label", "LabelAr", "LabelEn", "Metadata", "NumericValue", "SortOrder", "UpdatedAt", "UpdatedBy", "Value" },
                values: new object[,]
                {
                    { 281, null, "TYPE_SERVICE", "DEI_TP", "#c2410c", new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(824), null, "bg-orange-subtle text-orange", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-geo-alt", true, false, "D.E.I/TP – Déclaration des Éléments Imposables (Taxe Prof.)", null, null, null, null, 12, null, null, null },
                    { 282, null, "TYPE_SERVICE", "EG", "#c2410c", new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(826), null, "bg-orange-subtle text-orange", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-fuel-pump", true, false, "E.G – État Descriptif du Gasoil", null, null, null, null, 13, null, null, null },
                    { 283, null, "TYPE_SERVICE", "DRVT", "#c2410c", new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(827), null, "bg-orange-subtle text-orange", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-people", true, false, "D.R.V.T – Déclaration des Rémunérations versées à des Tiers", null, null, null, null, 14, null, null, null },
                    { 284, null, "TYPE_SERVICE", "CSS_SOL", "#c2410c", new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(829), null, "bg-orange-subtle text-orange", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-heart", true, false, "C.S.S – Contribution Sociale de Solidarité sur Bénéfices", null, null, null, null, 15, null, null, null },
                    { 291, null, "TYPE_SERVICE", "IMMOBILISATION", "#1d4ed8", new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(876), null, "bg-primary-subtle text-primary", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-box-seam", true, false, "Gestion des Immobilisations & Amortissements", null, null, null, null, 42, null, null, null },
                    { 292, null, "TYPE_SERVICE", "BILAN", "#6d28d9", new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(878), null, "bg-indigo-subtle text-indigo", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-clipboard-data", true, false, "Bilan & États de Synthèse", null, null, null, null, 43, null, null, null },
                    { 293, null, "TYPE_SERVICE", "ETATSFISCAUX", "#6d28d9", new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(879), null, "bg-indigo-subtle text-indigo", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-file-earmark-spreadsheet", true, false, "États Fiscaux (CPC, ESG, TFT)", null, null, null, null, 44, null, null, null },
                    { 294, null, "TYPE_SERVICE", "LIASSEFISCALE", "#6d28d9", new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(881), null, "bg-indigo-subtle text-indigo", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-files", true, false, "Liasse Fiscale", null, null, null, null, 45, null, null, null },
                    { 320, null, "CATEGORIE_SERVICE", "Fiscal", "#7c3aed", new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(883), null, "bg-purple-subtle text-purple", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-calculator", true, false, "Fiscal", null, null, null, null, 1, null, null, null },
                    { 321, null, "CATEGORIE_SERVICE", "Social", "#0891b2", new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(884), null, "bg-info-subtle text-info", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-people", true, false, "Social", null, null, null, null, 2, null, null, null },
                    { 322, null, "CATEGORIE_SERVICE", "Comptable", "#059669", new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(886), null, "bg-success-subtle text-success", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-journal-text", true, false, "Comptable", null, null, null, null, 3, null, null, null },
                    { 323, null, "CATEGORIE_SERVICE", "Juridique", "#dc2626", new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(887), null, "bg-danger-subtle text-danger", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-briefcase", true, false, "Juridique", null, null, null, null, 4, null, null, null },
                    { 324, null, "CATEGORIE_SERVICE", "Conseil", "#f59e0b", new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(889), null, "bg-warning-subtle text-warning", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-lightbulb", true, false, "Conseil", null, null, null, null, 5, null, null, null },
                    { 325, null, "CATEGORIE_SERVICE", "Autre", "#64748b", new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(891), null, "bg-secondary-subtle text-secondary", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-three-dots", true, false, "Autre", null, null, null, null, 99, null, null, null },
                    { 330, null, "FREQUENCE", "Mensuelle", "#2563eb", new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(892), null, "bg-primary-subtle text-primary", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-calendar-month", true, false, "Mensuelle", null, null, null, null, 1, null, null, null },
                    { 331, null, "FREQUENCE", "Trimestrielle", "#7c3aed", new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(894), null, "bg-purple-subtle text-purple", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-calendar3", true, false, "Trimestrielle", null, null, null, null, 2, null, null, null },
                    { 332, null, "FREQUENCE", "Annuelle", "#059669", new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(895), null, "bg-success-subtle text-success", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-calendar-event", true, false, "Annuelle", null, null, null, null, 3, null, null, null },
                    { 333, null, "FREQUENCE", "Ponctuelle", "#f59e0b", new DateTime(2026, 5, 19, 13, 23, 15, 638, DateTimeKind.Utc).AddTicks(897), null, "bg-warning-subtle text-warning", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-calendar-check", true, false, "Ponctuelle", null, null, null, null, 4, null, null, null }
                });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(1400));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(2252));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 19, 13, 23, 15, 637, DateTimeKind.Utc).AddTicks(2254));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 281);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 282);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 283);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 284);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 291);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 292);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 293);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 294);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 320);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 321);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 322);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 323);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 324);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 325);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 330);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 331);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 332);

            migrationBuilder.DeleteData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 333);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(7789));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8416));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8422));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8425));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8427));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8433));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8438));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8441));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8444));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8493));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 11,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8497));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 12,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8499));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 13,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8501));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 14,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8505));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 15,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8507));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 16,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8510));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 17,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8512));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 18,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8515));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 19,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8518));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 20,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8521));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 21,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8523));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 22,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8525));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 23,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8527));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 24,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8530));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 25,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8532));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 26,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8534));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 27,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8538));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 28,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8539));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 29,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8542));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 30,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8544));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 31,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8546));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 32,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8549));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 33,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8551));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 34,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8587));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 35,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8589));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 36,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8593));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 37,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8595));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 38,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8597));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 39,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8600));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 40,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8603));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 41,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8605));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 42,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8607));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 43,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8611));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 44,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8614));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 45,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8616));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 46,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8619));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 47,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8621));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 48,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8623));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 49,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8626));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 50,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8628));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 51,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8631));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 52,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8634));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 53,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8636));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 54,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8638));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 55,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8640));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 56,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8642));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 57,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8645));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 58,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8647));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 59,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8683));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 60,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8686));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 61,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8688));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 62,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8692));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 63,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8695));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 64,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8697));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 65,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8700));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 66,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8703));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 67,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8706));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 68,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8709));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 69,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8712));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 70,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8714));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 71,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8716));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 72,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8719));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 73,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8721));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 74,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8723));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 75,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8726));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 76,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8728));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 77,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8730));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 78,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8733));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 79,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8735));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 80,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8737));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 81,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(8768));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(9918));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(878));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(882));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(885));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(887));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(889));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(916));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(918));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(920));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(941));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 11,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(943));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 12,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1126));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 13,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1128));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 14,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1131));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 15,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1133));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 16,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1171));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 17,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1173));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 18,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1176));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 19,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1190));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 20,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1193));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 21,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1195));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 22,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1197));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 23,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1371));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 24,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1373));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 25,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1375));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 26,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1377));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 27,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1380));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 28,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1403));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 29,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1405));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 30,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1407));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 31,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1409));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 32,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1382));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 33,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1384));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 34,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1386));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 35,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1401));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 36,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1411));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 37,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1412));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 38,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1414));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 39,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1416));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 40,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1418));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 41,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1420));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 42,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1422));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 43,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1520));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 44,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1523));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 45,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1525));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 46,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1527));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 47,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1529));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 48,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1532));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 49,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1534));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 50,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1536));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 51,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1564));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 52,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1566));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 53,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1568));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 54,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1570));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 55,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1583));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 56,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1586));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 57,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1588));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 58,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1590));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 59,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1592));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 60,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1594));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 61,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1604));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 62,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1606));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 63,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1618));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 64,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1620));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 65,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1621));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 66,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1623));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 67,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1625));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 68,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1627));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 69,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1628));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 70,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1630));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 71,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1632));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 72,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1633));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 73,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1635));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 74,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1637));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 75,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1639));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 76,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1640));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 77,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1652));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 78,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1653));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 79,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1655));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 80,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1657));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 81,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1659));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 82,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1716));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 83,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1718));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 84,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1720));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 85,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1722));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 86,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1724));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 87,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1725));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 88,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1727));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 89,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1729));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 90,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1730));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 91,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1742));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 92,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1744));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 93,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1746));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 94,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1748));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 95,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1750));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 96,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1752));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 97,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1754));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 98,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1755));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 99,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1758));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 100,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1760));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 101,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1762));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 102,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1764));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 103,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1766));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 104,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1778));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 105,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1780));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 106,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1782));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 107,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1784));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 108,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1786));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 109,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1788));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 110,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1790));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 111,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1792));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 112,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1794));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 113,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1796));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 114,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1798));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 115,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1811));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 116,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1813));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 117,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1855));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 118,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1857));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 119,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1858));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 120,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1860));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 121,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1862));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 122,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1864));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 123,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1865));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 124,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1867));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 125,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1869));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 126,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1870));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 127,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1872));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 128,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1874));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 129,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1886));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 130,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1889));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 131,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1891));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 132,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1893));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 133,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1895));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 200,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1897));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 201,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1899));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 202,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1901));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 203,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1903));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 204,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1905));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 205,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1907));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 210,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1909));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 211,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1922));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 212,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1924));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 213,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1926));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 220,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1928));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 221,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1930));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 222,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1932));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 223,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1934));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 224,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1936));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 225,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1938));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 226,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1940));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 227,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1942));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 228,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1982));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 230,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1984));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 231,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1986));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 232,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1988));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 233,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1990));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 234,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1992));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 235,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1994));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 236,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1996));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 237,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1998));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 238,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(2001));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 240,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(2003));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 241,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(2005));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 242,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(2018));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 250,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(2020));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 251,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(2022));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 255,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(2024));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 256,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(2026));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 257,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(2028));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 260,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(2030));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 261,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(2032));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 262,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(2033));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 263,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(2035));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 264,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(2037));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 270,
                columns: new[] { "Couleur", "CreatedAt", "CssClass", "Label" },
                values: new object[] { "#2563eb", new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(2039), "bg-primary-subtle text-primary", "Taxe sur la Valeur Ajoutée" });

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 271,
                columns: new[] { "Code", "Couleur", "CreatedAt", "CssClass", "Icone", "Label" },
                values: new object[] { "IS", "#7c3aed", new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(2051), "bg-purple-subtle text-purple", "bi-building", "Impôt sur les Sociétés" });

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 272,
                columns: new[] { "Code", "Couleur", "CreatedAt", "CssClass", "Icone", "Label" },
                values: new object[] { "IR", "#059669", new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(2054), "bg-success-subtle text-success", "bi-person", "Impôt sur le Revenu" });

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 273,
                columns: new[] { "Code", "Couleur", "CreatedAt", "CssClass", "Icone", "Label" },
                values: new object[] { "CNSS", "#0891b2", new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(2056), "bg-info-subtle text-info", "bi-shield", "Caisse Nationale de Sécurité Sociale" });

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 274,
                columns: new[] { "Code", "Couleur", "CreatedAt", "CssClass", "Icone", "Label" },
                values: new object[] { "PAIE", "#d97706", new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(2058), "bg-warning-subtle text-warning", "bi-cash-stack", "Gestion de la Paie" });

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 275,
                columns: new[] { "Code", "Couleur", "CreatedAt", "CssClass", "Icone", "Label" },
                values: new object[] { "JURIDIQUE", "#dc2626", new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(2060), "bg-danger-subtle text-danger", "bi-briefcase", "Dossiers Juridiques" });

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 276,
                columns: new[] { "Code", "Couleur", "CreatedAt", "CssClass", "Icone", "Label" },
                values: new object[] { "COMPTABILITE", "#14b8a6", new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(2062), "bg-teal-subtle text-teal", "bi-journal-text", "Tenue Comptable" });

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 277,
                columns: new[] { "Code", "Couleur", "CreatedAt", "CssClass", "Icone", "Label" },
                values: new object[] { "BILAN", "#6366f1", new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(2064), "bg-indigo-subtle text-indigo", "bi-bar-chart-line", "Bilan & États de Synthèse" });

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 278,
                columns: new[] { "Code", "Couleur", "CreatedAt", "CssClass", "Icone", "Label" },
                values: new object[] { "ETATSFISCAUX", "#ec4899", new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(2066), "bg-pink-subtle text-pink", "bi-file-earmark-spreadsheet", "États Fiscaux" });

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 279,
                columns: new[] { "Code", "Couleur", "CreatedAt", "CssClass", "Icone", "Label" },
                values: new object[] { "AUDITCOMPTABLE", "#64748b", new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(2068), "bg-secondary-subtle text-secondary", "bi-search", "Audit Comptable" });

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 280,
                columns: new[] { "Code", "Couleur", "CreatedAt", "CssClass", "Icone", "Label" },
                values: new object[] { "CONSEILFISCAL", "#f59e0b", new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(2070), "bg-warning-subtle text-warning", "bi-lightbulb", "Conseil Fiscal" });

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 285,
                columns: new[] { "Category", "Code", "Couleur", "CreatedAt", "CssClass", "Icone", "Label", "SortOrder" },
                values: new object[] { "CATEGORIE_SERVICE", "Fiscal", "#7c3aed", new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(2072), "bg-purple-subtle text-purple", "bi-calculator", "Fiscal", 1 });

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 286,
                columns: new[] { "Category", "Code", "Couleur", "CreatedAt", "CssClass", "Icone", "Label", "SortOrder" },
                values: new object[] { "CATEGORIE_SERVICE", "Social", "#0891b2", new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(2074), "bg-info-subtle text-info", "bi-people", "Social", 2 });

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 287,
                columns: new[] { "Category", "Code", "Couleur", "CreatedAt", "Icone", "Label", "SortOrder" },
                values: new object[] { "CATEGORIE_SERVICE", "Comptable", "#059669", new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(2105), "bi-journal-text", "Comptable", 3 });

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 288,
                columns: new[] { "Category", "Code", "Couleur", "CreatedAt", "Icone", "Label", "SortOrder" },
                values: new object[] { "CATEGORIE_SERVICE", "Juridique", "#dc2626", new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(2108), "bi-briefcase", "Juridique", 4 });

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 289,
                columns: new[] { "Category", "Code", "Couleur", "CreatedAt", "CssClass", "Icone", "Label", "SortOrder" },
                values: new object[] { "CATEGORIE_SERVICE", "Conseil", "#f59e0b", new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(2110), "bg-warning-subtle text-warning", "bi-lightbulb", "Conseil", 5 });

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 290,
                columns: new[] { "Category", "Code", "Couleur", "CreatedAt", "CssClass", "Icone", "Label", "SortOrder" },
                values: new object[] { "CATEGORIE_SERVICE", "Autre", "#64748b", new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(2112), "bg-secondary-subtle text-secondary", "bi-three-dots", "Autre", 99 });

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 300,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1596));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 301,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1598));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 302,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1600));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 303,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(1602));

            migrationBuilder.InsertData(
                table: "ReferenceData",
                columns: new[] { "Id", "CabinetId", "Category", "Code", "Couleur", "CreatedAt", "CreatedBy", "CssClass", "DeletedAt", "DeletedBy", "EffectiveFrom", "EffectiveTo", "Icone", "IsActive", "IsDeleted", "Label", "LabelAr", "LabelEn", "Metadata", "NumericValue", "SortOrder", "UpdatedAt", "UpdatedBy", "Value" },
                values: new object[,]
                {
                    { 295, null, "FREQUENCE", "Mensuelle", "#2563eb", new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(2114), null, "bg-primary-subtle text-primary", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-calendar-month", true, false, "Mensuelle", null, null, null, null, 1, null, null, null },
                    { 296, null, "FREQUENCE", "Trimestrielle", "#7c3aed", new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(2115), null, "bg-purple-subtle text-purple", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-calendar3", true, false, "Trimestrielle", null, null, null, null, 2, null, null, null },
                    { 297, null, "FREQUENCE", "Annuelle", "#059669", new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(2118), null, "bg-success-subtle text-success", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-calendar-event", true, false, "Annuelle", null, null, null, null, 3, null, null, null },
                    { 298, null, "FREQUENCE", "Ponctuelle", "#f59e0b", new DateTime(2026, 5, 7, 21, 32, 18, 994, DateTimeKind.Utc).AddTicks(2120), null, "bg-warning-subtle text-warning", null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "bi-calendar-check", true, false, "Ponctuelle", null, null, null, null, 4, null, null, null }
                });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(1375));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(2445));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 21, 32, 18, 993, DateTimeKind.Utc).AddTicks(2458));
        }
    }
}
