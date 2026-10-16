using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using MetaCompta.Infrastructure.Data;

#nullable disable

namespace MetaCompta.API.Migrations;

[DbContext(typeof(MetaComptaDbContext))]
[Migration("20261015120000_WidenServicesActivesJson")]
public partial class WidenServicesActivesJson : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
IF COL_LENGTH(N'dbo.ContratsCommerciaux', N'ServicesActivesJson') IS NOT NULL
BEGIN
    ALTER TABLE [ContratsCommerciaux]
        ALTER COLUMN [ServicesActivesJson] nvarchar(max) NULL;
END
");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
IF COL_LENGTH(N'dbo.ContratsCommerciaux', N'ServicesActivesJson') IS NOT NULL
BEGIN
    ALTER TABLE [ContratsCommerciaux]
        ALTER COLUMN [ServicesActivesJson] nvarchar(4000) NULL;
END
");
    }
}
