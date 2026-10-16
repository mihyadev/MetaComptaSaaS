using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using MetaCompta.Infrastructure.Data;

#nullable disable

namespace MetaCompta.API.Migrations;

[DbContext(typeof(MetaComptaDbContext))]
[Migration("20261003200000_AddCourrierPiecesJointes")]
public partial class AddCourrierPiecesJointes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
IF OBJECT_ID(N'[CourrierPiecesJointes]', N'U') IS NULL
BEGIN
    CREATE TABLE [CourrierPiecesJointes] (
        [Id] int NOT NULL IDENTITY(1,1),
        [CourrierId] int NOT NULL,
        [DocumentId] int NOT NULL,
        [IsPrimary] bit NOT NULL CONSTRAINT [DF_CourrierPiecesJointes_IsPrimary] DEFAULT(0),
        [SortOrder] int NOT NULL CONSTRAINT [DF_CourrierPiecesJointes_SortOrder] DEFAULT(0),
        [Label] nvarchar(200) NULL,
        [CabinetId] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(max) NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL CONSTRAINT [DF_CourrierPiecesJointes_IsDeleted] DEFAULT(0),
        [DeletedAt] datetime2 NULL,
        [DeletedBy] nvarchar(max) NULL,
        CONSTRAINT [PK_CourrierPiecesJointes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CourrierPiecesJointes_Courriers] FOREIGN KEY ([CourrierId]) REFERENCES [CourriersDomiciliation]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_CourrierPiecesJointes_Documents] FOREIGN KEY ([DocumentId]) REFERENCES [Documents]([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CourrierPiecesJointes_Cabinets] FOREIGN KEY ([CabinetId]) REFERENCES [Cabinets]([Id]) ON DELETE NO ACTION
    );
    CREATE UNIQUE INDEX [IX_CourrierPiecesJointes_Courrier_Document]
        ON [CourrierPiecesJointes]([CourrierId], [DocumentId]) WHERE [IsDeleted] = 0;
    CREATE INDEX [IX_CourrierPiecesJointes_Cabinet_Courrier]
        ON [CourrierPiecesJointes]([CabinetId], [CourrierId]);
END

-- Backfill existing single ScanDocumentId into photothèque
INSERT INTO [CourrierPiecesJointes]
    ([CourrierId], [DocumentId], [IsPrimary], [SortOrder], [Label], [CabinetId], [CreatedAt], [IsDeleted])
SELECT c.[Id], c.[ScanDocumentId], 1, 0, N'Scan principal', c.[CabinetId], SYSUTCDATETIME(), 0
FROM [CourriersDomiciliation] c
WHERE c.[ScanDocumentId] IS NOT NULL
  AND c.[IsDeleted] = 0
  AND NOT EXISTS (
      SELECT 1 FROM [CourrierPiecesJointes] p
      WHERE p.[CourrierId] = c.[Id] AND p.[DocumentId] = c.[ScanDocumentId] AND p.[IsDeleted] = 0
  );
");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
IF OBJECT_ID(N'[CourrierPiecesJointes]', N'U') IS NOT NULL
    DROP TABLE [CourrierPiecesJointes];
");
    }
}
