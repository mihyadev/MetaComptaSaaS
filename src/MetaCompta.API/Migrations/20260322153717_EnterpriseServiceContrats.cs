using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetaCompta.API.Migrations
{
    /// <inheritdoc />
    public partial class EnterpriseServiceContrats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ServiceContrats_ClientId_TypeService",
                table: "ServiceContrats");

            migrationBuilder.RenameColumn(
                name: "Honoraires",
                table: "ServiceContrats",
                newName: "HonorairesMensuels");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateDebut",
                table: "ServiceContrats",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AutoGenererDeclarations",
                table: "ServiceContrats",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Categorie",
                table: "ServiceContrats",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ConfigurationJson",
                table: "ServiceContrats",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DelaiDeclarationMois",
                table: "ServiceContrats",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExerciceDebut",
                table: "ServiceContrats",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExerciceFin",
                table: "ServiceContrats",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExerciceFiscal",
                table: "ServiceContrats",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "HonorairesAnnuels",
                table: "ServiceContrats",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "JourLimiteDeclaration",
                table: "ServiceContrats",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponsableId",
                table: "ServiceContrats",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(848));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1343));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1347));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1350));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1355));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1359));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1363));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1367));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1369));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1373));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 11,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1375));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 12,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1377));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 13,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1380));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 14,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1383));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 15,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1385));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 16,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1436));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 17,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1438));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 18,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1441));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 19,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1444));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 20,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1447));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 21,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1449));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 22,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1451));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 23,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1454));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 24,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1456));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 25,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1458));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 26,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1460));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 27,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1464));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 28,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1465));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 29,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1468));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 30,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1470));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 31,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1473));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 32,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1475));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 33,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1478));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 34,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1482));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 35,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1484));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 36,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1487));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 37,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1489));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 38,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1492));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 39,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1494));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 40,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1527));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 41,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1529));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 42,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1531));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 43,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1534));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 44,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1537));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 45,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1540));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 46,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1543));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 47,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1545));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 48,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1547));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 49,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1550));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 50,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1553));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 51,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1555));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 52,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1558));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 53,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1560));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 54,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1562));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 55,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1565));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 56,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1567));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 57,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1570));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 58,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1572));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 59,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1574));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 60,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1577));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 61,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1579));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 62,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1582));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 63,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1584));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 64,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1587));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 65,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1613));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 66,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(1616));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(2598));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3483));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3486));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3488));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3489));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3491));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3518));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3520));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3539));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3541));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 11,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3543));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 12,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3722));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 13,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3758));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 14,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3761));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 15,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3763));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 16,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3765));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 17,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3767));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 18,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3769));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 19,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3782));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 20,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3784));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 21,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3786));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 22,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3788));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 23,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3963));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 24,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3965));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 25,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3967));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 26,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3969));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 27,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3970));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 28,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3990));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 29,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3992));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 30,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3994));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 31,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3995));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 32,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3972));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 33,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3974));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 34,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3987));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 35,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3988));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 36,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3997));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 37,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(3999));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 38,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4001));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 39,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4002));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 40,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4004));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 41,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4006));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 42,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4008));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 43,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4097));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 44,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4099));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 45,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4101));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 46,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4103));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 47,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4105));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 48,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4134));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 49,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4136));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 50,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4138));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 51,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4140));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 52,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4142));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 53,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4144));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 54,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4156));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 55,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4158));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 56,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4160));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 57,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4162));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 58,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4164));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 59,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4166));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 60,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4168));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 61,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4170));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 62,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4172));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 63,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4174));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 64,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4175));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 65,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4177));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 66,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4189));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 67,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4190));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 68,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4192));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 69,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4193));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 70,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4195));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 71,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4196));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 72,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4198));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 73,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4199));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 74,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4201));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 75,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4203));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 76,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4204));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 77,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4206));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 78,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4207));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 79,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4209));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 80,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4210));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 81,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4223));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 82,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4224));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 83,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4226));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 84,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4245));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 85,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4247));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 86,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4249));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 87,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4250));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 88,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4252));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 89,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4254));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 90,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4255));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 91,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4257));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 92,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4259));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 93,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4260));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 94,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4262));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 95,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4274));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 96,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4276));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 97,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4278));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 98,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4279));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 99,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4281));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 100,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4283));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 101,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4285));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 102,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4287));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 103,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4288));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 104,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4290));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 105,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4292));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 106,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4294));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 107,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4305));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 108,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4307));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 109,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4308));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 110,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4310));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 111,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4312));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 112,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4314));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 113,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4316));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 114,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4318));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 115,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4319));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 116,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4321));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 117,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4323));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 118,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4369));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 119,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4381));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 120,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4382));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 121,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4384));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 122,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4385));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 123,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4387));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 124,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4388));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 125,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4390));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 126,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4392));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 127,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4393));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 128,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4394));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 129,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4396));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 130,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4398));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 131,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4400));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 132,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4402));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 133,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 187, DateTimeKind.Utc).AddTicks(4404));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 186, DateTimeKind.Utc).AddTicks(3496));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 186, DateTimeKind.Utc).AddTicks(4515));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 15, 37, 16, 186, DateTimeKind.Utc).AddTicks(4518));

            migrationBuilder.CreateIndex(
                name: "IX_ServiceContrats_ClientId_TypeService",
                table: "ServiceContrats",
                columns: new[] { "ClientId", "TypeService" },
                filter: "[ExerciceFiscal] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceContrats_ClientId_TypeService_ExerciceFiscal",
                table: "ServiceContrats",
                columns: new[] { "ClientId", "TypeService", "ExerciceFiscal" },
                unique: true,
                filter: "[ExerciceFiscal] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceContrats_ResponsableId",
                table: "ServiceContrats",
                column: "ResponsableId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceContrats_Users_ResponsableId",
                table: "ServiceContrats",
                column: "ResponsableId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceContrats_Users_ResponsableId",
                table: "ServiceContrats");

            migrationBuilder.DropIndex(
                name: "IX_ServiceContrats_ClientId_TypeService",
                table: "ServiceContrats");

            migrationBuilder.DropIndex(
                name: "IX_ServiceContrats_ClientId_TypeService_ExerciceFiscal",
                table: "ServiceContrats");

            migrationBuilder.DropIndex(
                name: "IX_ServiceContrats_ResponsableId",
                table: "ServiceContrats");

            migrationBuilder.DropColumn(
                name: "AutoGenererDeclarations",
                table: "ServiceContrats");

            migrationBuilder.DropColumn(
                name: "Categorie",
                table: "ServiceContrats");

            migrationBuilder.DropColumn(
                name: "ConfigurationJson",
                table: "ServiceContrats");

            migrationBuilder.DropColumn(
                name: "DelaiDeclarationMois",
                table: "ServiceContrats");

            migrationBuilder.DropColumn(
                name: "ExerciceDebut",
                table: "ServiceContrats");

            migrationBuilder.DropColumn(
                name: "ExerciceFin",
                table: "ServiceContrats");

            migrationBuilder.DropColumn(
                name: "ExerciceFiscal",
                table: "ServiceContrats");

            migrationBuilder.DropColumn(
                name: "HonorairesAnnuels",
                table: "ServiceContrats");

            migrationBuilder.DropColumn(
                name: "JourLimiteDeclaration",
                table: "ServiceContrats");

            migrationBuilder.DropColumn(
                name: "ResponsableId",
                table: "ServiceContrats");

            migrationBuilder.RenameColumn(
                name: "HonorairesMensuels",
                table: "ServiceContrats",
                newName: "Honoraires");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateDebut",
                table: "ServiceContrats",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(4162));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(4713));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(4717));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(4720));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(4723));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(4728));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(4732));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(4735));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(4738));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(4791));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 11,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(4794));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 12,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(4796));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 13,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(4799));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 14,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(4802));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 15,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(4804));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 16,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(4806));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 17,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(4809));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 18,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(4812));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 19,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(4815));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 20,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(4818));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 21,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(4820));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 22,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(4822));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 23,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(4824));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 24,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(4826));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 25,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(4829));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 26,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(4831));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 27,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(4834));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 28,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(4836));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 29,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(4838));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 30,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(4840));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 31,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(4842));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 32,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(4844));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 33,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(4847));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 34,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(4879));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 35,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(4881));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 36,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(4884));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 37,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(4886));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 38,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(4888));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 39,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(4891));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 40,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(4894));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 41,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(4896));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 42,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(4898));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 43,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(4901));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 44,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(4904));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 45,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(4906));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 46,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(4909));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 47,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(4911));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 48,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(4913));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 49,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(4915));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 50,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(4917));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 51,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(4920));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 52,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(4923));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 53,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(4925));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 54,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(4927));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 55,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(4929));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 56,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(4931));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 57,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(4933));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 58,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(4960));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 59,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(4963));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 60,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(4965));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 61,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(4968));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 62,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(4971));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 63,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(4974));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 64,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(4976));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 65,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(4979));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: 66,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(4982));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(6382));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(7321));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(7324));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(7358));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(7360));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(7362));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(7385));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(7387));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(7389));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(7409));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 11,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(7411));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 12,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(7592));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 13,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(7595));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 14,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(7597));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 15,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(7599));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 16,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(7601));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 17,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(7603));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 18,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(7604));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 19,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(7606));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 20,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(7619));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 21,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(7620));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 22,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(7622));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 23,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(7780));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 24,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(7782));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 25,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(7784));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 26,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(7786));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 27,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(7788));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 28,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(7808));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 29,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(7810));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 30,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(7812));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 31,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(7814));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 32,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(7790));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 33,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(7792));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 34,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(7793));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 35,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(7795));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 36,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(7816));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 37,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(7817));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 38,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(7819));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 39,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(7852));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 40,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(7854));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 41,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(7856));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 42,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(7858));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 43,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(7941));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 44,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(7955));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 45,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(7957));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 46,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(7959));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 47,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(7961));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 48,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(7963));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 49,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(7965));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 50,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(7967));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 51,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(7968));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 52,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(7970));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 53,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(7972));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 54,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(7974));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 55,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(7985));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 56,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(7987));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 57,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(7989));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 58,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(7991));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 59,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(7993));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 60,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(7995));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 61,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(7997));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 62,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(7998));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 63,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8000));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 64,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8002));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 65,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8003));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 66,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8005));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 67,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8006));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 68,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8019));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 69,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8020));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 70,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8022));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 71,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8023));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 72,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8025));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 73,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8026));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 74,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8053));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 75,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8055));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 76,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8056));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 77,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8058));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 78,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8059));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 79,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8061));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 80,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8062));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 81,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8064));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 82,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8065));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 83,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8077));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 84,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8079));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 85,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8081));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 86,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8082));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 87,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8084));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 88,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8085));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 89,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8087));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 90,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8088));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 91,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8090));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 92,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8092));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 93,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8093));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 94,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8095));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 95,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8097));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 96,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8107));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 97,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8109));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 98,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8111));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 99,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8113));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 100,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8115));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 101,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8117));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 102,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8118));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 103,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8120));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 104,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8122));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 105,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8124));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 106,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8125));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 107,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8127));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 108,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8139));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 109,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8141));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 110,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8159));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 111,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8161));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 112,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8163));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 113,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8165));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 114,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8166));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 115,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8168));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 116,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8170));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 117,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8172));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 118,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8173));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 119,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8175));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 120,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8187));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 121,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8189));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 122,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8190));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 123,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8192));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 124,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8193));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 125,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8195));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 126,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8196));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 127,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8198));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 128,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8199));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 129,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8201));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 130,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8203));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 131,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8205));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 132,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8207));

            migrationBuilder.UpdateData(
                table: "ReferenceData",
                keyColumn: "Id",
                keyValue: 133,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 198, DateTimeKind.Utc).AddTicks(8209));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 197, DateTimeKind.Utc).AddTicks(6912));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 197, DateTimeKind.Utc).AddTicks(8126));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 22, 14, 44, 16, 197, DateTimeKind.Utc).AddTicks(8128));

            migrationBuilder.CreateIndex(
                name: "IX_ServiceContrats_ClientId_TypeService",
                table: "ServiceContrats",
                columns: new[] { "ClientId", "TypeService" },
                unique: true);
        }
    }
}
