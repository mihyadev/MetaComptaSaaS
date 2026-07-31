SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

DELETE FROM ServiceContrats;
UPDATE ContratsCommerciaux SET IsDeleted=1, DeletedAt=GETUTCDATE() WHERE IsDeleted=0;

DECLARE @json NVARCHAR(MAX);
SET @json = N'[{"TypeService":"TVA","LibelleService":"TVA - Declaration mensuelle","AutoGenererDeclarations":true}]';

INSERT INTO ContratsCommerciaux (Reference,ClientId,CabinetId,Objet,Devise,DateEmission,DateDebutContrat,DateFinContrat,Statut,EstSigne,DateSignature,DateValidation,ModeleFacturation,FrequenceFacturation,MontantHT,MontantTVA,MontantTTC,MontantRemise,Version,IsDeleted,CreatedAt,CreatedBy,ServicesActivesJson)
VALUES('CTR-HAFSSA-TVA',55,1,'Contrat TVA uniquement','MAD',GETUTCDATE(),'2026-01-01','2026-12-31','Valide',1,GETUTCDATE(),GETUTCDATE(),'Mensuel','Mensuelle',0,0,0,0,1,0,GETUTCDATE(),'test',@json);

INSERT INTO ServiceContrats (ClientId,TypeService,LibelleService,Categorie,Frequence,Actif,AutoGenererDeclarations,DateDebut,DateFin,ExerciceFiscal,IsDeleted,CreatedAt,CreatedBy)
VALUES(55,'TVA','TVA - Declaration mensuelle','Fiscal','Mensuelle',1,1,'2026-01-01','2026-12-31',2026,0,GETUTCDATE(),'test');

SELECT Reference,Statut,CONVERT(varchar,DateDebutContrat,23) Debut,CONVERT(varchar,DateFinContrat,23) Fin FROM ContratsCommerciaux WHERE IsDeleted=0;
SELECT TypeService,Actif,CONVERT(varchar,DateDebut,23) Debut,CONVERT(varchar,DateFin,23) Fin FROM ServiceContrats;
