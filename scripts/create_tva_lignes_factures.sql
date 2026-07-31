IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'TVALignesFactures')
BEGIN
    CREATE TABLE [TVALignesFactures] (
        [Id] int IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [CabinetId] int NOT NULL,
        [DeclarationTVAId] int NOT NULL,
        [TypePiece] nvarchar(50) NOT NULL,
        [NumeroFacture] nvarchar(100) NULL,
        [DateFacture] datetime2 NOT NULL,
        [FournisseurClient] nvarchar(300) NULL,
        [ICE] nvarchar(20) NULL,
        [IdentifiantFiscal] nvarchar(50) NULL,
        [Description] nvarchar(500) NULL,
        [MontantHT] decimal(18,2) NOT NULL,
        [TauxTVA] decimal(5,2) NOT NULL,
        [MontantTVA] decimal(18,2) NOT NULL,
        [MontantTTC] decimal(18,2) NOT NULL,
        [TypeOperation] nvarchar(50) NOT NULL,
        [IsProrata] bit NOT NULL CONSTRAINT DF_TVALignes_IsProrata DEFAULT 0,
        [ProrataPercent] decimal(5,2) NULL,
        [Regime] nvarchar(50) NULL,
        [IsDeleted] bit NOT NULL CONSTRAINT DF_TVALignes_IsDeleted DEFAULT 0,
        [CreatedAt] datetime2 NOT NULL CONSTRAINT DF_TVALignes_CreatedAt DEFAULT GETUTCDATE(),
        [CreatedBy] nvarchar(max) NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [DeletedAt] datetime2 NULL,
        [DeletedBy] nvarchar(max) NULL
    );
    CREATE INDEX [IX_TVALignesFactures_CabinetId] ON [TVALignesFactures]([CabinetId]);
    CREATE INDEX [IX_TVALignesFactures_DeclarationTVAId] ON [TVALignesFactures]([DeclarationTVAId]);
    IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'DeclarationsTVA')
        ALTER TABLE [TVALignesFactures] ADD CONSTRAINT [FK_TVALignesFactures_DeclarationsTVA_DeclarationTVAId]
            FOREIGN KEY ([DeclarationTVAId]) REFERENCES [DeclarationsTVA]([Id]) ON DELETE CASCADE;
    PRINT 'CREATED';
END
ELSE
    PRINT 'EXISTS';
