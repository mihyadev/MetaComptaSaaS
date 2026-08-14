using MetaCompta.Application.DTOs;
using MetaCompta.Application.Interfaces;
using MetaCompta.Domain.Entities;
using MetaCompta.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MetaCompta.Application.Services;

public class ComptabiliteService : IComptabiliteService
{
    private readonly MetaComptaDbContext _db;
    private readonly ILogger<ComptabiliteService> _logger;

    private static readonly (string Code, string Libelle, string Type)[] DefaultJournals =
    {
        ("AC", "Journal des Achats", "Achats"),
        ("VE", "Journal des Ventes", "Ventes"),
        ("BQ", "Journal de Banque", "Banque"),
        ("CA", "Journal de Caisse", "Caisse"),
        ("OD", "Opérations Diverses", "OD"),
        ("AN", "À Nouveau", "AN"),
        ("SA", "Journal des Salaires", "Paie")
    };

    /// <summary>Core Moroccan PCGM accounts (classes 1–7) for seed.</summary>
    private static readonly (string Num, string Lib, string Cls)[] PcgmSeed =
    {
        ("1111", "Capital social", "1"),
        ("1140", "Réserve légale", "1"),
        ("1181", "Résultat net de l'exercice", "1"),
        ("1481", "Emprunts auprès des établissements de crédit", "1"),
        ("1486", "Fournisseurs d'immobilisations", "1"),
        ("2110", "Frais de constitution", "2"),
        ("2230", "Fonds commercial", "2"),
        ("2311", "Terrains nus", "2"),
        ("2321", "Bâtiments", "2"),
        ("2332", "Matériel et outillage", "2"),
        ("2340", "Matériel de transport", "2"),
        ("2351", "Mobilier de bureau", "2"),
        ("2355", "Matériel informatique", "2"),
        ("2810", "Amort. frais préliminaires", "2"),
        ("2832", "Amort. matériel et outillage", "2"),
        ("2834", "Amort. matériel de transport", "2"),
        ("2835", "Amort. MMB & aménagements", "2"),
        ("3111", "Marchandises", "3"),
        ("3121", "Matières premières", "3"),
        ("3151", "Produits finis", "3"),
        ("3421", "Clients", "3"),
        ("3424", "Clients douteux ou litigieux", "3"),
        ("3425", "Clients - effets à recevoir", "3"),
        ("3451", "Subventions à recevoir", "3"),
        ("3455", "État - TVA récupérable", "3"),
        ("3458", "État - crédit de TVA", "3"),
        ("4411", "Fournisseurs", "4"),
        ("4415", "Fournisseurs - effets à payer", "4"),
        ("4432", "Rémunérations dues au personnel", "4"),
        ("4441", "CNSS", "4"),
        ("4443", "Caisses de retraite", "4"),
        ("4452", "État - TVA facturée", "4"),
        ("4453", "État - TVA due", "4"),
        ("4455", "État - IS", "4"),
        ("4456", "État - Taxes sur CA", "4"),
        ("5141", "Banque", "5"),
        ("5143", "Trésorerie Générale", "5"),
        ("5161", "Caisse centrale", "5"),
        ("5520", "Crédits d'escompte", "5"),
        ("6111", "Achats de marchandises", "6"),
        ("6121", "Achats de matières premières", "6"),
        ("6125", "Achats non stockés", "6"),
        ("6131", "Locations et charges locatives", "6"),
        ("6133", "Entretien et réparations", "6"),
        ("6134", "Primes d'assurances", "6"),
        ("6141", "Transport du personnel", "6"),
        ("6142", "Déplacements et missions", "6"),
        ("6143", "Frais postaux", "6"),
        ("6144", "Publicité", "6"),
        ("6147", "Services bancaires", "6"),
        ("6161", "Impôts et taxes", "6"),
        ("6171", "Rémunérations du personnel", "6"),
        ("6174", "Charges sociales", "6"),
        ("6176", "Charges sociales diverses", "6"),
        ("6182", "Pertes sur créances irrécouvrables", "6"),
        ("6191", "DEA des immob. en non-valeur", "6"),
        ("6193", "DEA des immob. corporelles", "6"),
        ("6311", "Intérêts des emprunts", "6"),
        ("6386", "Escomptes accordés", "6"),
        ("6585", "Créances devenues irrécouvrables", "6"),
        ("7111", "Ventes de marchandises", "7"),
        ("7121", "Ventes de biens produits", "7"),
        ("7124", "Ventes de services", "7"),
        ("7127", "Ventes et produits accessoires", "7"),
        ("7129", "RRR accordés par l'entreprise", "7"),
        ("7141", "Subventions d'exploitation", "7"),
        ("7197", "Transferts de charges d'exploitation", "7"),
        ("7381", "Intérêts et produits assimilés", "7"),
        ("7386", "Escomptes obtenus", "7"),
        ("7585", "Rentrées sur créances soldées", "7")
    };

    public ComptabiliteService(MetaComptaDbContext db, ILogger<ComptabiliteService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task EnsureClientLedgerAsync(int clientId, int cabinetId, string? userId = null)
    {
        var client = await _db.Clients.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == clientId && c.CabinetId == cabinetId);
        if (client == null) return;

        // Do NOT auto-create exercices — user must create an open exercice explicitly (Sage-like gate).

        if (!await _db.ComptesComptables.AnyAsync(c => c.ClientId == clientId && c.CabinetId == cabinetId))
        {
            foreach (var (num, lib, cls) in PcgmSeed)
            {
                _db.ComptesComptables.Add(new CompteComptable
                {
                    CabinetId = cabinetId,
                    ClientId = clientId,
                    Numero = num,
                    Libelle = lib,
                    Classe = cls,
                    Nature = "General",
                    EstActif = true,
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow
                });
            }
            await _db.SaveChangesAsync();
            _logger.LogInformation("Seeded PCGM chart for client {ClientId}", clientId);
        }

        if (!await _db.JournauxComptables.AnyAsync(j => j.ClientId == clientId && j.CabinetId == cabinetId))
        {
            foreach (var (code, lib, type) in DefaultJournals)
            {
                _db.JournauxComptables.Add(new JournalComptable
                {
                    CabinetId = cabinetId,
                    ClientId = clientId,
                    Code = code,
                    Libelle = lib,
                    Type = type,
                    ProchainNumero = 1,
                    EstActif = true,
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow
                });
            }
            await _db.SaveChangesAsync();
        }

        if (!await _db.ComptesBancaires.AnyAsync(b => b.ClientId == clientId && b.CabinetId == cabinetId))
        {
            _db.ComptesBancaires.Add(new CompteBancaire
            {
                CabinetId = cabinetId,
                ClientId = clientId,
                Code = "BQ1",
                Libelle = "Banque principale",
                CompteComptableNumero = "5141",
                EstActif = true,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();
        }
    }

    public async Task<ApiResponse<List<CompteComptableDto>>> GetComptesAsync(int clientId, int cabinetId, string? search = null)
    {
        await EnsureClientLedgerAsync(clientId, cabinetId);
        var q = _db.ComptesComptables.Where(c => c.ClientId == clientId && c.CabinetId == cabinetId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            q = q.Where(c => c.Numero.Contains(s) || c.Libelle.Contains(s));
        }
        var list = await q.OrderBy(c => c.Numero)
            .Select(c => new CompteComptableDto
            {
                Id = c.Id,
                ClientId = c.ClientId,
                Numero = c.Numero,
                Libelle = c.Libelle,
                Classe = c.Classe,
                Nature = c.Nature,
                EstActif = c.EstActif
            }).ToListAsync();
        return ApiResponse<List<CompteComptableDto>>.Ok(list);
    }

    public async Task<ApiResponse<CompteComptableDto>> CreateCompteAsync(CompteComptableCreateDto dto, int cabinetId, string userId)
    {
        await EnsureClientLedgerAsync(dto.ClientId, cabinetId, userId);
        var numero = dto.Numero.Trim();
        if (string.IsNullOrEmpty(numero) || string.IsNullOrWhiteSpace(dto.Libelle))
            return ApiResponse<CompteComptableDto>.Fail("Numéro et libellé obligatoires.");

        if (await _db.ComptesComptables.AnyAsync(c => c.ClientId == dto.ClientId && c.Numero == numero))
            return ApiResponse<CompteComptableDto>.Fail("Ce numéro de compte existe déjà.");

        var classe = string.IsNullOrWhiteSpace(dto.Classe)
            ? numero[..1]
            : dto.Classe.Trim();

        var entity = new CompteComptable
        {
            CabinetId = cabinetId,
            ClientId = dto.ClientId,
            Numero = numero,
            Libelle = dto.Libelle.Trim(),
            Classe = classe,
            Nature = string.IsNullOrWhiteSpace(dto.Nature) ? "General" : dto.Nature,
            EstActif = true,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow
        };
        _db.ComptesComptables.Add(entity);
        await _db.SaveChangesAsync();
        return ApiResponse<CompteComptableDto>.Ok(new CompteComptableDto
        {
            Id = entity.Id,
            ClientId = entity.ClientId,
            Numero = entity.Numero,
            Libelle = entity.Libelle,
            Classe = entity.Classe,
            Nature = entity.Nature,
            EstActif = entity.EstActif
        }, "Compte créé.");
    }

    public async Task<ApiResponse<List<JournalDto>>> GetJournauxAsync(int clientId, int cabinetId)
    {
        await EnsureClientLedgerAsync(clientId, cabinetId);
        var journaux = await _db.JournauxComptables
            .Where(j => j.ClientId == clientId && j.CabinetId == cabinetId)
            .OrderBy(j => j.Code)
            .ToListAsync();

        var result = new List<JournalDto>();
        foreach (var j in journaux)
        {
            var validated = _db.EcrituresComptables
                .Where(e => e.ClientId == clientId && e.CabinetId == cabinetId && e.JournalComptableId == j.Id && e.Statut == "Validee");
            result.Add(new JournalDto
            {
                Id = j.Id,
                Code = j.Code,
                Nom = j.Libelle,
                Type = j.Type,
                ProchainNumero = j.ProchainNumero,
                EstActif = j.EstActif,
                NombreEcritures = await validated.CountAsync(),
                TotalDebit = await validated.SumAsync(e => (decimal?)e.TotalDebit) ?? 0,
                TotalCredit = await validated.SumAsync(e => (decimal?)e.TotalCredit) ?? 0
            });
        }
        return ApiResponse<List<JournalDto>>.Ok(result);
    }

    public async Task<ApiResponse<List<ExerciceComptableDto>>> GetExercicesAsync(int clientId, int cabinetId)
    {
        await EnsureClientLedgerAsync(clientId, cabinetId);
        var list = await _db.ExercicesComptables
            .AsNoTracking()
            .Where(e => e.ClientId == clientId && e.CabinetId == cabinetId)
            .OrderByDescending(e => e.Annee)
            .Select(e => new ExerciceComptableDto
            {
                Id = e.Id,
                ClientId = e.ClientId,
                Annee = e.Annee,
                DateDebut = e.DateDebut,
                DateFin = e.DateFin,
                Statut = e.Statut
            })
            .ToListAsync();
        return ApiResponse<List<ExerciceComptableDto>>.Ok(list);
    }

    public async Task<ApiResponse<ExerciceComptableDto>> GetExerciceByIdAsync(int id, int cabinetId)
    {
        var e = await _db.ExercicesComptables.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.CabinetId == cabinetId);
        if (e == null) return ApiResponse<ExerciceComptableDto>.Fail("Exercice introuvable.");
        return ApiResponse<ExerciceComptableDto>.Ok(MapExercice(e));
    }

    public async Task<ApiResponse<ExerciceComptableDto>> CreateExerciceAsync(ExerciceCreateDto dto, int cabinetId, string userId)
    {
        if (dto.ClientId <= 0)
            return ApiResponse<ExerciceComptableDto>.Fail("Client obligatoire.");
        if (dto.Annee < 1990 || dto.Annee > DateTime.UtcNow.Year + 5)
            return ApiResponse<ExerciceComptableDto>.Fail("Année d'exercice invalide.");

        var client = await _db.Clients.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == dto.ClientId && c.CabinetId == cabinetId);
        if (client == null)
            return ApiResponse<ExerciceComptableDto>.Fail("Client introuvable.");

        if (await _db.ExercicesComptables.AnyAsync(e => e.ClientId == dto.ClientId && e.CabinetId == cabinetId && e.Annee == dto.Annee))
            return ApiResponse<ExerciceComptableDto>.Fail($"Un exercice {dto.Annee} existe déjà pour ce client.");

        // Multiple open exercices are allowed (new year can open before previous is clôturé).

        DateTime dateDebut, dateFin;
        if (dto.DateDebut.HasValue && dto.DateFin.HasValue)
        {
            dateDebut = dto.DateDebut.Value.Date;
            dateFin = dto.DateFin.Value.Date;
        }
        else
        {
            (dateDebut, dateFin) = ExerciceClotureService.ComputeExerciceDates(dto.Annee, client.MoisClotureExercice);
        }
        if (dateFin <= dateDebut)
            return ApiResponse<ExerciceComptableDto>.Fail("La date de fin doit être postérieure à la date de début.");

        await EnsureClientLedgerAsync(dto.ClientId, cabinetId, userId);

        var exercice = await CreateExerciceInternalAsync(dto.ClientId, cabinetId, dto.Annee, dateDebut, dateFin, userId);

        // Attach contract services activated before any exercice existed
        var unbound = await _db.ServiceContrats
            .Where(s => s.ClientId == dto.ClientId && s.Actif && s.ExerciceComptableId == null)
            .ToListAsync();
        foreach (var s in unbound)
        {
            s.ExerciceComptableId = exercice.Id;
            s.ExerciceFiscal = exercice.Annee;
            s.UpdatedAt = DateTime.UtcNow;
            s.UpdatedBy = userId;
        }
        if (unbound.Count > 0)
            await _db.SaveChangesAsync();

        return ApiResponse<ExerciceComptableDto>.Ok(MapExercice(exercice), $"Exercice {dto.Annee} créé.");
    }

    public async Task<ApiResponse<PagedResult<EcritureComptableDto>>> GetEcrituresAsync(
        int cabinetId, int? clientId, string? journal, int page, string? search, string? statut = null, int? exerciceId = null)
    {
        if (clientId is > 0)
            await EnsureClientLedgerAsync(clientId.Value, cabinetId);

        page = Math.Max(1, page);
        const int pageSize = 20;

        var q = _db.EcrituresComptables
            .Include(e => e.Client)
            .Include(e => e.Lignes)
            .Where(e => e.CabinetId == cabinetId);

        if (clientId is > 0)
            q = q.Where(e => e.ClientId == clientId);
        if (exerciceId is > 0)
            q = q.Where(e => e.ExerciceComptableId == exerciceId);
        if (!string.IsNullOrWhiteSpace(journal))
            q = q.Where(e => e.JournalCode == journal);
        if (!string.IsNullOrWhiteSpace(statut))
        {
            var st = statut.Trim();
            if (st.Equals("Validé", StringComparison.OrdinalIgnoreCase) || st.Equals("Validee", StringComparison.OrdinalIgnoreCase))
                q = q.Where(e => e.Statut == "Validee");
            else if (st.Equals("Brouillon", StringComparison.OrdinalIgnoreCase))
                q = q.Where(e => e.Statut == "Brouillon");
        }
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            q = q.Where(e =>
                (e.Libelle != null && e.Libelle.Contains(s)) ||
                e.NumeroPiece.Contains(s) ||
                (e.Reference != null && e.Reference.Contains(s)) ||
                e.Lignes.Any(l => l.NumCompte.Contains(s)));
        }

        var total = await q.CountAsync();
        var rows = await q
            .OrderByDescending(e => e.DateEcriture)
            .ThenByDescending(e => e.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = rows.Select(e =>
        {
            var first = e.Lignes.OrderBy(l => l.SortOrder).FirstOrDefault();
            return new EcritureComptableDto
            {
                Id = e.Id,
                ClientId = e.ClientId,
                ClientNom = e.Client?.RaisonSociale,
                Journal = e.JournalCode,
                NumeroPiece = e.NumeroPiece,
                DateEcriture = e.DateEcriture,
                NumCompte = first?.NumCompte ?? "",
                LibelleCompte = first?.LibelleCompte ?? "",
                Libelle = e.Libelle,
                Debit = e.TotalDebit,
                Credit = e.TotalCredit,
                NombreLignes = e.Lignes.Count,
                Reference = e.Reference,
                Statut = MapStatutDisplay(e.Statut),
                CreatedAt = e.CreatedAt
            };
        }).ToList();

        return ApiResponse<PagedResult<EcritureComptableDto>>.Ok(new PagedResult<EcritureComptableDto>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        });
    }

    public async Task<ApiResponse<EcritureDetailDto>> GetEcritureByIdAsync(int id, int cabinetId)
    {
        var e = await _db.EcrituresComptables
            .Include(x => x.Lignes)
            .Include(x => x.Client)
            .FirstOrDefaultAsync(x => x.Id == id && x.CabinetId == cabinetId);
        if (e == null) return ApiResponse<EcritureDetailDto>.Fail("Écriture introuvable.");

        return ApiResponse<EcritureDetailDto>.Ok(new EcritureDetailDto
        {
            Id = e.Id,
            ClientId = e.ClientId,
            ClientNom = e.Client?.RaisonSociale,
            Journal = e.JournalCode,
            NumeroPiece = e.NumeroPiece,
            DateEcriture = e.DateEcriture,
            Libelle = e.Libelle,
            Reference = e.Reference,
            Statut = MapStatutDisplay(e.Statut),
            TotalDebit = e.TotalDebit,
            TotalCredit = e.TotalCredit,
            Lignes = e.Lignes.OrderBy(l => l.SortOrder).Select(l => new LigneEcritureDto
            {
                NumCompte = l.NumCompte,
                LibelleCompte = l.LibelleCompte,
                Debit = l.Debit,
                Credit = l.Credit
            }).ToList()
        });
    }

    public async Task<ApiResponse<EcritureComptableDto>> CreateEcritureAsync(EcritureCreateDto dto, int cabinetId, string userId)
    {
        if (dto.ClientId <= 0)
            return ApiResponse<EcritureComptableDto>.Fail("Client obligatoire.");
        if (string.IsNullOrWhiteSpace(dto.Journal))
            return ApiResponse<EcritureComptableDto>.Fail("Journal obligatoire.");
        if (string.IsNullOrWhiteSpace(dto.Libelle))
            return ApiResponse<EcritureComptableDto>.Fail("Libellé obligatoire.");

        var lignes = (dto.Lignes ?? new())
            .Where(l => !string.IsNullOrWhiteSpace(l.NumCompte) && (l.Debit > 0 || l.Credit > 0))
            .ToList();
        if (lignes.Count < 2)
            return ApiResponse<EcritureComptableDto>.Fail("Au moins deux lignes d'écriture sont requises.");

        var totalD = Math.Round(lignes.Sum(l => l.Debit), 2);
        var totalC = Math.Round(lignes.Sum(l => l.Credit), 2);
        if (totalD != totalC)
            return ApiResponse<EcritureComptableDto>.Fail($"Écriture non équilibrée (Débit {totalD:N2} ≠ Crédit {totalC:N2}).");
        if (totalD <= 0)
            return ApiResponse<EcritureComptableDto>.Fail("Montant total nul.");

        await EnsureClientLedgerAsync(dto.ClientId, cabinetId, userId);

        var journalCode = dto.Journal.Trim().ToUpperInvariant();
        // Normalize legacy UI codes
        journalCode = journalCode switch
        {
            "VT" => "VE",
            "CS" => "CA",
            _ => journalCode
        };

        var journal = await _db.JournauxComptables
            .FirstOrDefaultAsync(j => j.ClientId == dto.ClientId && j.CabinetId == cabinetId && j.Code == journalCode && j.EstActif);
        if (journal == null)
            return ApiResponse<EcritureComptableDto>.Fail($"Journal {journalCode} introuvable ou inactif.");

        ExerciceComptable? exercice = null;
        if (dto.ExerciceComptableId is > 0)
        {
            exercice = await _db.ExercicesComptables
                .FirstOrDefaultAsync(e => e.Id == dto.ExerciceComptableId && e.ClientId == dto.ClientId && e.CabinetId == cabinetId);
            if (exercice == null)
                return ApiResponse<EcritureComptableDto>.Fail("Exercice introuvable.");
        }
        else
        {
            exercice = await _db.ExercicesComptables
                .Where(e => e.ClientId == dto.ClientId && e.CabinetId == cabinetId
                            && e.DateDebut <= dto.DateEcriture.Date && e.DateFin >= dto.DateEcriture.Date)
                .OrderByDescending(e => e.Annee)
                .FirstOrDefaultAsync();
            if (exercice == null)
            {
                var year = dto.DateEcriture.Year;
                exercice = await _db.ExercicesComptables
                    .FirstOrDefaultAsync(e => e.ClientId == dto.ClientId && e.CabinetId == cabinetId && e.Annee == year);
            }
        }
        if (exercice == null)
            return ApiResponse<EcritureComptableDto>.Fail("Aucun exercice comptable pour cette date.");
        if (!string.Equals(exercice.Statut, "Ouvert", StringComparison.OrdinalIgnoreCase))
            return ApiResponse<EcritureComptableDto>.Fail($"L'exercice {exercice.Annee} est clôturé — saisie impossible.");
        if (dto.DateEcriture.Date < exercice.DateDebut.Date || dto.DateEcriture.Date > exercice.DateFin.Date)
            return ApiResponse<EcritureComptableDto>.Fail(
                $"La date doit être comprise dans l'exercice {exercice.Annee} ({exercice.DateDebut:dd/MM/yyyy} – {exercice.DateFin:dd/MM/yyyy}).");

        var comptes = await _db.ComptesComptables
            .Where(c => c.ClientId == dto.ClientId && c.CabinetId == cabinetId && c.EstActif)
            .ToDictionaryAsync(c => c.Numero, c => c);

        foreach (var l in lignes)
        {
            if (!comptes.ContainsKey(l.NumCompte.Trim()))
                return ApiResponse<EcritureComptableDto>.Fail($"Compte {l.NumCompte} introuvable ou inactif.");
            if (l.Debit > 0 && l.Credit > 0)
                return ApiResponse<EcritureComptableDto>.Fail($"Ligne {l.NumCompte}: débit et crédit ne peuvent pas être tous deux renseignés.");
        }

        var num = journal.ProchainNumero;
        journal.ProchainNumero++;
        var numeroPiece = string.IsNullOrWhiteSpace(dto.NumeroPiece)
            ? $"{journalCode}-{exercice.Annee}-{num:D4}"
            : dto.NumeroPiece.Trim();

        var statut = dto.Valider ? "Validee" : "Brouillon";
        var entity = new EcritureComptable
        {
            CabinetId = cabinetId,
            ClientId = dto.ClientId,
            ExerciceComptableId = exercice.Id,
            JournalComptableId = journal.Id,
            JournalCode = journalCode,
            NumeroPiece = numeroPiece,
            DateEcriture = dto.DateEcriture.Date,
            DatePiece = dto.DateEcriture.Date,
            Libelle = dto.Libelle.Trim(),
            Reference = dto.Reference?.Trim(),
            Statut = statut,
            TotalDebit = totalD,
            TotalCredit = totalC,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow
        };
        if (dto.Valider)
        {
            entity.DateValidation = DateTime.UtcNow;
            entity.ValideeParUserId = userId;
        }

        var order = 0;
        foreach (var l in lignes)
        {
            var numC = l.NumCompte.Trim();
            var compte = comptes[numC];
            entity.Lignes.Add(new LigneEcriture
            {
                CabinetId = cabinetId,
                ClientId = dto.ClientId,
                CompteComptableId = compte.Id,
                NumCompte = numC,
                LibelleCompte = string.IsNullOrWhiteSpace(l.LibelleCompte) ? compte.Libelle : l.LibelleCompte.Trim(),
                LibelleLigne = dto.Libelle.Trim(),
                Debit = Math.Round(l.Debit, 2),
                Credit = Math.Round(l.Credit, 2),
                SortOrder = order++,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            });
        }

        _db.EcrituresComptables.Add(entity);
        await _db.SaveChangesAsync();

        var first = entity.Lignes.OrderBy(x => x.SortOrder).First();
        return ApiResponse<EcritureComptableDto>.Ok(new EcritureComptableDto
        {
            Id = entity.Id,
            ClientId = entity.ClientId,
            Journal = entity.JournalCode,
            NumeroPiece = entity.NumeroPiece,
            DateEcriture = entity.DateEcriture,
            NumCompte = first.NumCompte,
            LibelleCompte = first.LibelleCompte,
            Libelle = entity.Libelle,
            Debit = entity.TotalDebit,
            Credit = entity.TotalCredit,
            Reference = entity.Reference,
            Statut = MapStatutDisplay(entity.Statut),
            CreatedAt = entity.CreatedAt
        }, dto.Valider ? "Écriture créée et validée." : "Écriture créée en brouillon.");
    }

    public async Task<ApiResponse<EcritureComptableDto>> ValidateEcritureAsync(int id, int cabinetId, string userId)
    {
        var e = await _db.EcrituresComptables
            .Include(x => x.Lignes)
            .FirstOrDefaultAsync(x => x.Id == id && x.CabinetId == cabinetId);
        if (e == null) return ApiResponse<EcritureComptableDto>.Fail("Écriture introuvable.");
        if (e.Statut == "Validee")
            return ApiResponse<EcritureComptableDto>.Fail("Écriture déjà validée.");
        var exoBlock = await EnsureExerciceOpenForMutationAsync(e.ExerciceComptableId, e.ClientId, cabinetId);
        if (exoBlock != null) return ApiResponse<EcritureComptableDto>.Fail(exoBlock);

        if (e.Lignes.Count < 2)
            return ApiResponse<EcritureComptableDto>.Fail("Au moins deux lignes requises.");
        var td = Math.Round(e.Lignes.Sum(l => l.Debit), 2);
        var tc = Math.Round(e.Lignes.Sum(l => l.Credit), 2);
        if (td != tc)
            return ApiResponse<EcritureComptableDto>.Fail($"Écriture non équilibrée (Débit {td:N2} ≠ Crédit {tc:N2}).");

        e.Statut = "Validee";
        e.DateValidation = DateTime.UtcNow;
        e.ValideeParUserId = userId;
        e.TotalDebit = td;
        e.TotalCredit = tc;
        e.UpdatedAt = DateTime.UtcNow;
        e.UpdatedBy = userId;
        await _db.SaveChangesAsync();

        var first = e.Lignes.OrderBy(l => l.SortOrder).First();
        return ApiResponse<EcritureComptableDto>.Ok(new EcritureComptableDto
        {
            Id = e.Id,
            ClientId = e.ClientId,
            Journal = e.JournalCode,
            NumeroPiece = e.NumeroPiece,
            DateEcriture = e.DateEcriture,
            NumCompte = first.NumCompte,
            LibelleCompte = first.LibelleCompte,
            Libelle = e.Libelle,
            Debit = e.TotalDebit,
            Credit = e.TotalCredit,
            Reference = e.Reference,
            Statut = MapStatutDisplay(e.Statut),
            CreatedAt = e.CreatedAt
        }, "Écriture validée.");
    }

    public async Task<ApiResponse<List<GrandLivreDto>>> GetGrandLivreAsync(
        int clientId, int cabinetId, DateTime? dateDebut, DateTime? dateFin, string? numCompte = null, int? exerciceId = null)
    {
        await EnsureClientLedgerAsync(clientId, cabinetId);
        (dateDebut, dateFin) = await ResolveExerciceDatesAsync(clientId, cabinetId, exerciceId, dateDebut, dateFin);
        var q = ValidatedLines(clientId, cabinetId, dateDebut, dateFin, exerciceId);
        if (!string.IsNullOrWhiteSpace(numCompte))
            q = q.Where(l => l.NumCompte == numCompte);

        var groups = await q
            .GroupBy(l => new { l.NumCompte, l.LibelleCompte })
            .Select(g => new
            {
                g.Key.NumCompte,
                g.Key.LibelleCompte,
                Debit = g.Sum(x => x.Debit),
                Credit = g.Sum(x => x.Credit)
            })
            .OrderBy(x => x.NumCompte)
            .ToListAsync();

        var result = groups.Select(g =>
        {
            var solde = g.Debit - g.Credit;
            return new GrandLivreDto
            {
                NumeroCompte = g.NumCompte,
                LibelleCompte = g.LibelleCompte,
                TotalDebit = g.Debit,
                TotalCredit = g.Credit,
                SoldeDebiteur = solde > 0 ? solde : 0,
                SoldeCrediteur = solde < 0 ? -solde : 0
            };
        }).ToList();

        return ApiResponse<List<GrandLivreDto>>.Ok(result);
    }

    public async Task<ApiResponse<List<BalanceDto>>> GetBalanceAsync(
        int clientId, int cabinetId, DateTime? dateDebut, DateTime? dateFin, int? exerciceId = null)
    {
        await EnsureClientLedgerAsync(clientId, cabinetId);
        (dateDebut, dateFin) = await ResolveExerciceDatesAsync(clientId, cabinetId, exerciceId, dateDebut, dateFin);
        var groups = await ValidatedLines(clientId, cabinetId, dateDebut, dateFin, exerciceId)
            .GroupBy(l => new { l.NumCompte, l.LibelleCompte })
            .Select(g => new
            {
                g.Key.NumCompte,
                g.Key.LibelleCompte,
                Debit = g.Sum(x => x.Debit),
                Credit = g.Sum(x => x.Credit)
            })
            .OrderBy(x => x.NumCompte)
            .ToListAsync();

        var result = groups.Select(g =>
        {
            var solde = g.Debit - g.Credit;
            return new BalanceDto
            {
                NumeroCompte = g.NumCompte,
                LibelleCompte = g.LibelleCompte,
                TotalDebit = g.Debit,
                TotalCredit = g.Credit,
                SoldeDebiteur = solde > 0 ? solde : 0,
                SoldeCrediteur = solde < 0 ? -solde : 0
            };
        }).ToList();

        return ApiResponse<List<BalanceDto>>.Ok(result);
    }

    public async Task<ApiResponse<List<BalanceAgeeDto>>> GetBalanceAgeeAsync(int clientId, int cabinetId)
    {
        await EnsureClientLedgerAsync(clientId, cabinetId);
        // Phase 1: stub empty — auxiliaires come in Phase 2
        var empty = new List<BalanceAgeeDto>();
        return ApiResponse<List<BalanceAgeeDto>>.Ok(empty);
    }

    public async Task<ApiResponse<List<RapprochementBancaireDto>>> GetRapprochementsAsync(int cabinetId, int? clientId)
    {
        if (clientId is > 0)
            await EnsureClientLedgerAsync(clientId.Value, cabinetId);

        var q = _db.RapprochementsBancaires
            .Include(r => r.Client)
            .Where(r => r.CabinetId == cabinetId);
        if (clientId is > 0)
            q = q.Where(r => r.ClientId == clientId);

        var list = await q.OrderByDescending(r => r.Periode).ThenByDescending(r => r.Id)
            .Select(r => new RapprochementBancaireDto
            {
                Id = r.Id,
                ClientId = r.ClientId,
                ClientNom = r.Client != null ? r.Client.RaisonSociale : null,
                CompteBancaire = r.CompteBancaire,
                Periode = r.Periode,
                SoldeCompta = r.SoldeCompta,
                SoldeBanque = r.SoldeBanque,
                Ecart = r.Ecart,
                EstRapproche = r.EstRapproche,
                Statut = r.Statut,
                NombreLignesNonRapprochees = r.NombreLignesNonRapprochees,
                DateRapprochement = r.DateRapprochement
            }).ToListAsync();

        return ApiResponse<List<RapprochementBancaireDto>>.Ok(list);
    }

    public async Task<ApiResponse<RapprochementBancaireDto>> CreateRapprochementAsync(
        RapprochementCreateDto dto, int cabinetId, string userId)
    {
        if (dto.ClientId <= 0)
            return ApiResponse<RapprochementBancaireDto>.Fail("Client obligatoire.");
        if (string.IsNullOrWhiteSpace(dto.Periode))
            return ApiResponse<RapprochementBancaireDto>.Fail("Période obligatoire.");

        await EnsureClientLedgerAsync(dto.ClientId, cabinetId, userId);

        var openExo = await _db.ExercicesComptables.AsNoTracking()
            .Where(e => e.ClientId == dto.ClientId && e.CabinetId == cabinetId && e.Statut == "Ouvert")
            .OrderByDescending(e => e.Annee)
            .FirstOrDefaultAsync();
        if (openExo == null)
            return ApiResponse<RapprochementBancaireDto>.Fail(ExerciceContextService.NoOpenExerciceMessage);

        var compteLabel = string.IsNullOrWhiteSpace(dto.CompteBancaire) ? "5141 - Banque" : dto.CompteBancaire.Trim();
        var compteNum = "5141";
        if (compteLabel.Contains("5141")) compteNum = "5141";
        else if (compteLabel.Length >= 4 && char.IsDigit(compteLabel[0]))
            compteNum = compteLabel.Split(' ', '-', '/')[0];

        DateTime? debut = null, fin = null;
        if (DateTime.TryParse(dto.Periode + "-01", out var pStart) ||
            DateTime.TryParseExact(dto.Periode, "yyyy-MM", null, System.Globalization.DateTimeStyles.None, out pStart))
        {
            debut = pStart;
            fin = pStart.AddMonths(1).AddDays(-1);
        }

        var soldeCompta = await ValidatedLines(dto.ClientId, cabinetId, debut, fin)
            .Where(l => l.NumCompte.StartsWith(compteNum) || l.NumCompte == "5141")
            .SumAsync(l => (decimal?)(l.Debit - l.Credit)) ?? 0;

        var soldeBanque = dto.SoldeBanque ?? soldeCompta;
        var ecart = soldeBanque - soldeCompta;
        var bank = await _db.ComptesBancaires
            .FirstOrDefaultAsync(b => b.ClientId == dto.ClientId && b.CabinetId == cabinetId && b.EstActif);

        var entity = new RapprochementBancaire
        {
            CabinetId = cabinetId,
            ClientId = dto.ClientId,
            ExerciceComptableId = openExo.Id,
            CompteBancaireId = bank?.Id,
            CompteBancaire = compteLabel,
            Periode = dto.Periode.Trim(),
            SoldeCompta = Math.Round(soldeCompta, 2),
            SoldeBanque = Math.Round(soldeBanque, 2),
            Ecart = Math.Round(ecart, 2),
            EstRapproche = Math.Abs(ecart) < 0.01m,
            Statut = Math.Abs(ecart) < 0.01m ? "Rapproche" : "EnCours",
            NombreLignesNonRapprochees = Math.Abs(ecart) < 0.01m ? 0 : 1,
            DateRapprochement = DateTime.UtcNow,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow
        };

        // Attach unmatched BQ lines for the period as rapprochement lines
        var bqLignes = await ValidatedLines(dto.ClientId, cabinetId, debut, fin)
            .Where(l => l.Ecriture!.JournalCode == "BQ" && (l.NumCompte.StartsWith("514") || l.NumCompte == compteNum))
            .OrderBy(l => l.Ecriture!.DateEcriture)
            .Take(50)
            .ToListAsync();

        foreach (var l in bqLignes)
        {
            entity.Lignes.Add(new RapprochementLigne
            {
                CabinetId = cabinetId,
                ClientId = dto.ClientId,
                LigneEcritureId = l.Id,
                DateOperation = l.Ecriture?.DateEcriture,
                Libelle = l.LibelleLigne ?? l.Ecriture?.Libelle,
                Montant = l.Debit - l.Credit,
                EstRapprochee = false,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            });
        }
        entity.NombreLignesNonRapprochees = entity.Lignes.Count(x => !x.EstRapprochee);

        _db.RapprochementsBancaires.Add(entity);
        await _db.SaveChangesAsync();

        var clientName = await _db.Clients.Where(c => c.Id == dto.ClientId).Select(c => c.RaisonSociale).FirstOrDefaultAsync();
        return ApiResponse<RapprochementBancaireDto>.Ok(new RapprochementBancaireDto
        {
            Id = entity.Id,
            ClientId = entity.ClientId,
            ClientNom = clientName,
            CompteBancaire = entity.CompteBancaire,
            Periode = entity.Periode,
            SoldeCompta = entity.SoldeCompta,
            SoldeBanque = entity.SoldeBanque,
            Ecart = entity.Ecart,
            EstRapproche = entity.EstRapproche,
            Statut = entity.Statut,
            NombreLignesNonRapprochees = entity.NombreLignesNonRapprochees,
            DateRapprochement = entity.DateRapprochement
        }, "Rapprochement créé.");
    }

    public async Task<ApiResponse<RapprochementBancaireDto>> UpdateRapprochementAsync(
        int id, RapprochementUpdateDto dto, int cabinetId, string userId)
    {
        var entity = await _db.RapprochementsBancaires
            .FirstOrDefaultAsync(r => r.Id == id && r.CabinetId == cabinetId);
        if (entity == null)
            return ApiResponse<RapprochementBancaireDto>.Fail("Rapprochement introuvable.");

        if (dto.SoldeBanque.HasValue) entity.SoldeBanque = dto.SoldeBanque.Value;
        if (dto.SoldeCompta.HasValue) entity.SoldeCompta = dto.SoldeCompta.Value;
        entity.Ecart = Math.Round(entity.SoldeBanque - entity.SoldeCompta, 2);
        if (dto.EstRapproche == true || Math.Abs(entity.Ecart) < 0.01m)
        {
            entity.EstRapproche = true;
            entity.Statut = "Rapproche";
            entity.NombreLignesNonRapprochees = 0;
        }
        if (!string.IsNullOrWhiteSpace(dto.Notes)) entity.Notes = dto.Notes;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync();

        return ApiResponse<RapprochementBancaireDto>.Ok(new RapprochementBancaireDto
        {
            Id = entity.Id,
            ClientId = entity.ClientId,
            CompteBancaire = entity.CompteBancaire,
            Periode = entity.Periode,
            SoldeCompta = entity.SoldeCompta,
            SoldeBanque = entity.SoldeBanque,
            Ecart = entity.Ecart,
            EstRapproche = entity.EstRapproche,
            Statut = entity.Statut,
            NombreLignesNonRapprochees = entity.NombreLignesNonRapprochees,
            DateRapprochement = entity.DateRapprochement
        }, "Rapprochement mis à jour.");
    }

    public async Task<ApiResponse<ComptaDashboardDto>> GetDashboardAsync(int clientId, int cabinetId, int? exerciceId = null)
    {
        await EnsureClientLedgerAsync(clientId, cabinetId);
        var ecritures = _db.EcrituresComptables.Where(e => e.ClientId == clientId && e.CabinetId == cabinetId);
        if (exerciceId is > 0)
            ecritures = ecritures.Where(e => e.ExerciceComptableId == exerciceId);

        ExerciceComptable? exo = null;
        if (exerciceId is > 0)
            exo = await _db.ExercicesComptables.AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == exerciceId && e.ClientId == clientId && e.CabinetId == cabinetId);

        var dto = new ComptaDashboardDto
        {
            ComptesCount = await _db.ComptesComptables.CountAsync(c => c.ClientId == clientId && c.CabinetId == cabinetId && c.EstActif),
            JournauxCount = await _db.JournauxComptables.CountAsync(j => j.ClientId == clientId && j.CabinetId == cabinetId && j.EstActif),
            EcrituresBrouillon = await ecritures.CountAsync(e => e.Statut == "Brouillon"),
            EcrituresValidees = await ecritures.CountAsync(e => e.Statut == "Validee"),
            RapprochementsEnCours = await _db.RapprochementsBancaires.CountAsync(r => r.ClientId == clientId && r.CabinetId == cabinetId && !r.EstRapproche),
            ExerciceId = exo?.Id,
            ExerciceAnnee = exo?.Annee
        };
        return ApiResponse<ComptaDashboardDto>.Ok(dto);
    }

    public async Task<ApiResponse<CompteComptableDto>> UpdateCompteAsync(int id, CompteComptableUpdateDto dto, int cabinetId, string userId)
    {
        var entity = await _db.ComptesComptables.FirstOrDefaultAsync(c => c.Id == id && c.CabinetId == cabinetId);
        if (entity == null) return ApiResponse<CompteComptableDto>.Fail("Compte introuvable.");
        if (!string.IsNullOrWhiteSpace(dto.Libelle)) entity.Libelle = dto.Libelle.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Nature)) entity.Nature = dto.Nature.Trim();
        if (dto.EstActif.HasValue) entity.EstActif = dto.EstActif.Value;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = userId;
        await _db.SaveChangesAsync();
        return ApiResponse<CompteComptableDto>.Ok(new CompteComptableDto
        {
            Id = entity.Id,
            ClientId = entity.ClientId,
            Numero = entity.Numero,
            Libelle = entity.Libelle,
            Classe = entity.Classe,
            Nature = entity.Nature,
            EstActif = entity.EstActif
        }, "Compte mis à jour.");
    }

    public async Task<ApiResponse<CompteImportResultDto>> ImportComptesAsync(int clientId, string csvContent, int cabinetId, string userId)
    {
        await EnsureClientLedgerAsync(clientId, cabinetId, userId);
        var result = new CompteImportResultDto();
        if (string.IsNullOrWhiteSpace(csvContent))
            return ApiResponse<CompteImportResultDto>.Fail("CSV vide.");

        var existing = await _db.ComptesComptables
            .Where(c => c.ClientId == clientId && c.CabinetId == cabinetId)
            .Select(c => c.Numero)
            .ToListAsync();
        var existingSet = new HashSet<string>(existing, StringComparer.OrdinalIgnoreCase);

        var lines = csvContent.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n', StringSplitOptions.RemoveEmptyEntries);
        foreach (var raw in lines)
        {
            var line = raw.Trim();
            if (line.Length == 0) continue;
            if (line.StartsWith("numero", StringComparison.OrdinalIgnoreCase) || line.StartsWith("n°", StringComparison.OrdinalIgnoreCase))
                continue;

            var parts = line.Split(';', ',');
            if (parts.Length < 2)
            {
                result.Errors++;
                result.Messages.Add($"Ligne ignorée: {line}");
                continue;
            }
            var numero = parts[0].Trim().Trim('"');
            var libelle = parts[1].Trim().Trim('"');
            var classe = parts.Length > 2 && !string.IsNullOrWhiteSpace(parts[2])
                ? parts[2].Trim().Trim('"')
                : (numero.Length > 0 ? numero[..1] : "");
            var nature = parts.Length > 3 && !string.IsNullOrWhiteSpace(parts[3])
                ? parts[3].Trim().Trim('"')
                : "General";
            var estActif = true;
            if (parts.Length > 4)
            {
                var a = parts[4].Trim().Trim('"');
                estActif = a is "1" or "true" or "oui" or "Oui" or "TRUE";
            }

            if (string.IsNullOrEmpty(numero) || string.IsNullOrEmpty(libelle))
            {
                result.Errors++;
                continue;
            }
            if (existingSet.Contains(numero))
            {
                result.Skipped++;
                continue;
            }

            _db.ComptesComptables.Add(new CompteComptable
            {
                CabinetId = cabinetId,
                ClientId = clientId,
                Numero = numero,
                Libelle = libelle,
                Classe = classe,
                Nature = string.IsNullOrWhiteSpace(nature) ? "General" : nature,
                EstActif = estActif,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            });
            existingSet.Add(numero);
            result.Created++;
        }
        await _db.SaveChangesAsync();
        return ApiResponse<CompteImportResultDto>.Ok(result, $"{result.Created} compte(s) importé(s).");
    }

    public async Task<ApiResponse<CompteImportResultDto>> ImportJournauxAsync(int clientId, string csvContent, int cabinetId, string userId)
    {
        await EnsureClientLedgerAsync(clientId, cabinetId, userId);
        var result = new CompteImportResultDto();
        if (string.IsNullOrWhiteSpace(csvContent))
            return ApiResponse<CompteImportResultDto>.Fail("CSV vide.");

        var existing = await _db.JournauxComptables
            .Where(j => j.ClientId == clientId && j.CabinetId == cabinetId)
            .ToDictionaryAsync(j => j.Code, j => j, StringComparer.OrdinalIgnoreCase);

        foreach (var raw in SplitCsvLines(csvContent))
        {
            if (IsCsvHeader(raw, "code", "journal"))
                continue;
            var parts = SplitCsvCols(raw);
            if (parts.Length < 2)
            {
                result.Errors++;
                result.Messages.Add($"Ligne ignorée: {raw}");
                continue;
            }
            var code = NormalizeJournalCode(parts[0]);
            var libelle = parts[1].Trim().Trim('"');
            var type = parts.Length > 2 && !string.IsNullOrWhiteSpace(parts[2]) ? parts[2].Trim().Trim('"') : code;
            var prochain = 1;
            if (parts.Length > 3 && int.TryParse(parts[3].Trim(), out var pn) && pn > 0) prochain = pn;
            var actif = true;
            if (parts.Length > 4)
            {
                var a = parts[4].Trim().Trim('"');
                actif = a is not ("0" or "false" or "non" or "Non" or "FALSE");
            }
            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(libelle))
            {
                result.Errors++;
                continue;
            }

            if (existing.TryGetValue(code, out var j))
            {
                j.Libelle = libelle;
                j.Type = type;
                j.ProchainNumero = prochain;
                j.EstActif = actif;
                j.UpdatedAt = DateTime.UtcNow;
                j.UpdatedBy = userId;
                result.Skipped++; // counted as updated via Skipped label — message clarifies
                result.Messages.Add($"Journal {code} mis à jour.");
            }
            else
            {
                var entity = new JournalComptable
                {
                    CabinetId = cabinetId,
                    ClientId = clientId,
                    Code = code,
                    Libelle = libelle,
                    Type = type,
                    ProchainNumero = prochain,
                    EstActif = actif,
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow
                };
                _db.JournauxComptables.Add(entity);
                existing[code] = entity;
                result.Created++;
            }
        }
        await _db.SaveChangesAsync();
        return ApiResponse<CompteImportResultDto>.Ok(result, $"{result.Created} créé(s), {result.Skipped} mis à jour.");
    }

    public async Task<ApiResponse<CompteImportResultDto>> ImportEcrituresAsync(
        int clientId, string csvContent, int cabinetId, string userId, int? exerciceId = null, bool valider = false)
    {
        await EnsureClientLedgerAsync(clientId, cabinetId, userId);
        var result = new CompteImportResultDto();
        if (string.IsNullOrWhiteSpace(csvContent))
            return ApiResponse<CompteImportResultDto>.Fail("CSV vide.");

        if (exerciceId is > 0)
        {
            var exo = await _db.ExercicesComptables.AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == exerciceId && e.ClientId == clientId && e.CabinetId == cabinetId);
            if (exo == null)
                return ApiResponse<CompteImportResultDto>.Fail("Exercice introuvable.");
            if (!string.Equals(exo.Statut, "Ouvert", StringComparison.OrdinalIgnoreCase))
                return ApiResponse<CompteImportResultDto>.Fail($"L'exercice {exo.Annee} est clôturé.");
        }

        var rows = new List<(string Piece, DateTime Date, string Journal, string Libelle, string Compte, decimal Debit, decimal Credit, string? Ref, bool Valider)>();
        var lineNo = 0;
        // Detect layout: plan = Piece;Date;Journal;Libelle;Reference;NumCompte;LibelleCompte;Debit;Credit;Valider
        // legacy = Piece;Date;Journal;Libelle;Compte;Debit;Credit;Reference;Valider
        var usePlanLayout = false;
        var headerSeen = false;
        foreach (var raw in SplitCsvLines(csvContent))
        {
            lineNo++;
            var parts = SplitCsvCols(raw);
            if (!headerSeen && IsCsvHeader(raw, "piece", "date", "journal"))
            {
                headerSeen = true;
                var headers = parts.Select(p => p.Trim().Trim('"').ToLowerInvariant()).ToArray();
                usePlanLayout = headers.Any(h => h is "numcompte" or "libellecompte")
                    || (headers.Length > 4 && headers[4] is "reference" or "référence" or "ref");
                continue;
            }
            if (parts.Length < 6)
            {
                result.Errors++;
                result.Messages.Add($"L{lineNo}: colonnes insuffisantes.");
                continue;
            }

            string piece, journal, libelle, compte;
            string? reference;
            decimal debit, credit;
            DateTime date;
            var rowValider = valider;

            if (usePlanLayout)
            {
                // Piece;Date;Journal;Libelle;Reference;NumCompte;LibelleCompte;Debit;Credit;Valider
                piece = parts[0].Trim().Trim('"');
                if (!DateTime.TryParse(parts[1].Trim().Trim('"'), out date))
                {
                    result.Errors++;
                    result.Messages.Add($"L{lineNo}: date invalide.");
                    continue;
                }
                journal = parts[2].Trim().Trim('"');
                libelle = parts[3].Trim().Trim('"');
                reference = parts.Length > 4 ? parts[4].Trim().Trim('"') : null;
                compte = parts.Length > 5 ? parts[5].Trim().Trim('"') : "";
                if (parts.Length < 8
                    || !TryParseDec(parts[7], out debit)
                    || !TryParseDec(parts[8], out credit))
                {
                    debit = 0; credit = 0;
                    if (parts.Length >= 9) { TryParseDec(parts[7], out debit); TryParseDec(parts[8], out credit); }
                }
                if (parts.Length > 9)
                {
                    var v = parts[9].Trim().Trim('"');
                    if (v is "1" or "true" or "oui" or "Oui") rowValider = true;
                }
            }
            else
            {
                // Piece;Date;Journal;Libelle;Compte;Debit;Credit;Reference;Valider
                piece = parts[0].Trim().Trim('"');
                if (!DateTime.TryParse(parts[1].Trim().Trim('"'), out date))
                {
                    result.Errors++;
                    result.Messages.Add($"L{lineNo}: date invalide.");
                    continue;
                }
                journal = parts[2].Trim().Trim('"');
                libelle = parts[3].Trim().Trim('"');
                compte = parts[4].Trim().Trim('"');
                TryParseDec(parts[5], out debit);
                credit = parts.Length > 6 && TryParseDec(parts[6], out var c) ? c : 0;
                reference = parts.Length > 7 ? parts[7].Trim().Trim('"') : null;
                if (parts.Length > 8)
                {
                    var v = parts[8].Trim().Trim('"');
                    if (v is "1" or "true" or "oui" or "Oui") rowValider = true;
                }
            }

            if (string.IsNullOrEmpty(compte) || (debit <= 0 && credit <= 0))
            {
                result.Errors++;
                result.Messages.Add($"L{lineNo}: compte/montant invalide.");
                continue;
            }
            rows.Add((piece, date.Date, journal, libelle, compte, debit, credit, string.IsNullOrEmpty(reference) ? null : reference, rowValider));
        }

        if (rows.Count == 0)
            return ApiResponse<CompteImportResultDto>.Fail("Aucune ligne valide dans le CSV.");

        // Group consecutive rows by Piece (+ Date/Journal); empty Piece → auto IMP-{n}
        var prepared = new List<(string Piece, DateTime Date, string Journal, string Libelle, List<(string Compte, decimal Debit, decimal Credit)> Lignes, string? Ref, bool Valider)>();
        string? curPiece = null;
        DateTime? curDate = null;
        string? curJnl = null;
        string? curLib = null;
        string? curRef = null;
        var curVal = false;
        var curLignes = new List<(string, decimal, decimal)>();
        var emptySeq = 1;
        void Flush()
        {
            if (curLignes.Count == 0 || curPiece == null || curDate == null || curJnl == null) return;
            prepared.Add((curPiece, curDate.Value, curJnl, curLib ?? "", curLignes.ToList(), curRef, curVal));
            curLignes.Clear();
        }

        foreach (var r in rows)
        {
            var jnl = NormalizeJournalCode(r.Journal);
            var piece = string.IsNullOrWhiteSpace(r.Piece) ? null : r.Piece.Trim();
            if (piece == null)
            {
                // start new auto piece when journal/date changes mid-stream without piece
                if (curPiece == null || curDate != r.Date || curJnl != jnl || curLignes.Count == 0)
                {
                    Flush();
                    piece = $"IMP-{emptySeq++:D4}";
                    curPiece = piece;
                    curDate = r.Date;
                    curJnl = jnl;
                    curLib = r.Libelle;
                    curRef = r.Ref;
                    curVal = r.Valider;
                }
            }
            else if (curPiece != piece || curDate != r.Date || curJnl != jnl)
            {
                Flush();
                curPiece = piece;
                curDate = r.Date;
                curJnl = jnl;
                curLib = r.Libelle;
                curRef = r.Ref;
                curVal = r.Valider;
            }
            if (string.IsNullOrWhiteSpace(curLib) && !string.IsNullOrWhiteSpace(r.Libelle)) curLib = r.Libelle;
            curLignes.Add((r.Compte, r.Debit, r.Credit));
            if (r.Valider) curVal = true;
        }
        Flush();

        foreach (var g in prepared)
        {
            var totalD = Math.Round(g.Lignes.Sum(l => l.Debit), 2);
            var totalC = Math.Round(g.Lignes.Sum(l => l.Credit), 2);
            if (g.Lignes.Count < 2 || totalD != totalC || totalD <= 0)
            {
                result.Errors++;
                result.Skipped++;
                result.Messages.Add($"Pièce {g.Piece}: non équilibrée ou < 2 lignes (D={totalD:N2} C={totalC:N2}).");
                continue;
            }

            var exists = await _db.EcrituresComptables.AnyAsync(e =>
                e.ClientId == clientId && e.CabinetId == cabinetId && e.NumeroPiece == g.Piece);
            if (exists)
            {
                result.Skipped++;
                result.Messages.Add($"Pièce {g.Piece}: déjà existante — ignorée.");
                continue;
            }

            var create = new EcritureCreateDto
            {
                ClientId = clientId,
                ExerciceComptableId = exerciceId,
                Journal = g.Journal,
                DateEcriture = g.Date,
                Libelle = string.IsNullOrWhiteSpace(g.Libelle) ? g.Piece : g.Libelle,
                NumeroPiece = g.Piece,
                Reference = g.Ref,
                Valider = g.Valider || valider,
                Lignes = g.Lignes.Select(l => new LigneEcritureDto
                {
                    NumCompte = l.Compte,
                    Debit = l.Debit,
                    Credit = l.Credit
                }).ToList()
            };
            var created = await CreateEcritureAsync(create, cabinetId, userId);
            if (created.Success)
            {
                result.Created++;
            }
            else
            {
                result.Errors++;
                result.Messages.Add($"Pièce {g.Piece}: {created.Message}");
            }
        }

        return ApiResponse<CompteImportResultDto>.Ok(result,
            $"{result.Created} écriture(s) importée(s), {result.Skipped} ignorée(s), {result.Errors} erreur(s).");
    }

    public async Task<ApiResponse<CompteImportResultDto>> ImportRapprochementsAsync(
        int clientId, string csvContent, int cabinetId, string userId)
    {
        await EnsureClientLedgerAsync(clientId, cabinetId, userId);
        var result = new CompteImportResultDto();
        if (string.IsNullOrWhiteSpace(csvContent))
            return ApiResponse<CompteImportResultDto>.Fail("CSV vide.");

        foreach (var raw in SplitCsvLines(csvContent))
        {
            if (IsCsvHeader(raw, "compte", "periode", "période"))
                continue;
            var parts = SplitCsvCols(raw);
            if (parts.Length < 2)
            {
                result.Errors++;
                result.Messages.Add($"Ligne ignorée: {raw}");
                continue;
            }
            var compte = parts[0].Trim().Trim('"');
            var periode = parts[1].Trim().Trim('"');
            decimal? soldeBanque = null;
            if (parts.Length > 2 && decimal.TryParse(parts[2].Trim().Replace(',', '.'),
                    System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var sb))
                soldeBanque = sb;

            if (string.IsNullOrEmpty(compte) || string.IsNullOrEmpty(periode))
            {
                result.Errors++;
                continue;
            }

            var exists = await _db.RapprochementsBancaires.AnyAsync(r =>
                r.ClientId == clientId && r.CabinetId == cabinetId
                && r.CompteBancaire == compte && r.Periode == periode);
            if (exists)
            {
                result.Skipped++;
                result.Messages.Add($"Rapprochement {compte}/{periode} déjà existant.");
                continue;
            }

            var created = await CreateRapprochementAsync(new RapprochementCreateDto
            {
                ClientId = clientId,
                CompteBancaire = compte,
                Periode = periode,
                SoldeBanque = soldeBanque
            }, cabinetId, userId);

            if (created.Success) result.Created++;
            else
            {
                result.Errors++;
                result.Messages.Add($"{compte}/{periode}: {created.Message}");
            }
        }

        return ApiResponse<CompteImportResultDto>.Ok(result, $"{result.Created} rapprochement(s) importé(s).");
    }

    public async Task<ApiResponse<CompteImportResultDto>> ImportSoldesOuvertureAsync(
        int clientId, string csvContent, int cabinetId, string userId, int? exerciceId = null)
    {
        await EnsureClientLedgerAsync(clientId, cabinetId, userId);
        var result = new CompteImportResultDto();
        if (string.IsNullOrWhiteSpace(csvContent))
            return ApiResponse<CompteImportResultDto>.Fail("CSV vide.");

        ExerciceComptable? exo = null;
        if (exerciceId is > 0)
        {
            exo = await _db.ExercicesComptables
                .FirstOrDefaultAsync(e => e.Id == exerciceId && e.ClientId == clientId && e.CabinetId == cabinetId);
            if (exo == null)
                return ApiResponse<CompteImportResultDto>.Fail("Exercice introuvable.");
        }
        else
        {
            exo = await _db.ExercicesComptables
                .Where(e => e.ClientId == clientId && e.CabinetId == cabinetId && e.Statut == "Ouvert")
                .OrderByDescending(e => e.Annee)
                .FirstOrDefaultAsync();
        }
        if (exo == null)
            return ApiResponse<CompteImportResultDto>.Fail("Aucun exercice ouvert.");
        if (!string.Equals(exo.Statut, "Ouvert", StringComparison.OrdinalIgnoreCase))
            return ApiResponse<CompteImportResultDto>.Fail($"L'exercice {exo.Annee} est clôturé.");

        var piece = $"AN-{exo.Annee}";
        if (await _db.EcrituresComptables.AnyAsync(e =>
                e.ClientId == clientId && e.CabinetId == cabinetId && e.NumeroPiece == piece))
        {
            return ApiResponse<CompteImportResultDto>.Fail(
                $"Pièce {piece} déjà existante — soldes d'ouverture déjà importés pour {exo.Annee}.");
        }

        var comptes = await _db.ComptesComptables.AsNoTracking()
            .Where(c => c.ClientId == clientId && c.CabinetId == cabinetId && c.EstActif)
            .ToDictionaryAsync(c => c.Numero, c => c, StringComparer.OrdinalIgnoreCase);

        var lignes = new List<LigneEcritureDto>();
        decimal totalD = 0, totalC = 0;
        foreach (var raw in SplitCsvLines(csvContent))
        {
            if (IsCsvHeader(raw, "numcompte", "numero", "compte"))
                continue;
            var parts = SplitCsvCols(raw);
            if (parts.Length < 2)
            {
                result.Errors++;
                result.Messages.Add($"Ligne ignorée: {raw}");
                continue;
            }
            var num = parts[0].Trim().Trim('"');
            var lib = parts.Length > 1 ? parts[1].Trim().Trim('"') : "";
            TryParseDec(parts.Length > 2 ? parts[2] : "0", out var sd);
            TryParseDec(parts.Length > 3 ? parts[3] : "0", out var sc);
            if (sd < 0) { sc += -sd; sd = 0; }
            if (sc < 0) { sd += -sc; sc = 0; }
            if (sd == 0 && sc == 0) continue;
            if (sd > 0 && sc > 0)
            {
                result.Errors++;
                result.Messages.Add($"Compte {num}: solde débit et crédit tous deux renseignés.");
                continue;
            }
            if (!comptes.ContainsKey(num))
            {
                result.Errors++;
                result.Messages.Add($"Compte {num} introuvable ou inactif.");
                continue;
            }
            var libCompte = string.IsNullOrWhiteSpace(lib) ? comptes[num].Libelle : lib;
            lignes.Add(new LigneEcritureDto
            {
                NumCompte = num,
                LibelleCompte = libCompte,
                Debit = sd,
                Credit = sc
            });
            totalD += sd;
            totalC += sc;
        }

        totalD = Math.Round(totalD, 2);
        totalC = Math.Round(totalC, 2);
        if (lignes.Count < 2)
            return ApiResponse<CompteImportResultDto>.Fail("Au moins 2 lignes de soldes requises.");
        if (totalD != totalC || totalD <= 0)
            return ApiResponse<CompteImportResultDto>.Fail(
                $"Soldes non équilibrés (D={totalD:N2} C={totalC:N2}). Aucune écriture créée.");

        var created = await CreateEcritureAsync(new EcritureCreateDto
        {
            ClientId = clientId,
            ExerciceComptableId = exo.Id,
            Journal = "OD",
            DateEcriture = exo.DateDebut,
            Libelle = "À-nouveaux / soldes d'ouverture",
            NumeroPiece = piece,
            Reference = "OUVERTURE",
            Valider = true,
            Lignes = lignes
        }, cabinetId, userId);

        if (!created.Success)
            return ApiResponse<CompteImportResultDto>.Fail(created.Message ?? "Échec création OD d'ouverture.");

        result.Created = 1;
        result.Messages.Add($"Pièce {piece} validée ({lignes.Count} lignes) — Balance / Grand livre mis à jour.");
        return ApiResponse<CompteImportResultDto>.Ok(result, $"Soldes d'ouverture importés ({piece}).");
    }

    private static bool TryParseDec(string? raw, out decimal value)
    {
        value = 0;
        if (string.IsNullOrWhiteSpace(raw)) return false;
        return decimal.TryParse(raw.Trim().Trim('"').Replace(',', '.'),
            System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out value);
    }

    private static IEnumerable<string> SplitCsvLines(string csvContent)
        => csvContent.Replace("\r\n", "\n").Replace('\r', '\n')
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Trim())
            .Where(l => l.Length > 0);

    private static string[] SplitCsvCols(string line)
        => line.Split(';', ',').Select(p => p.Trim()).ToArray();

    private static bool IsCsvHeader(string line, params string[] markers)
    {
        var lower = line.ToLowerInvariant();
        return markers.Any(m => lower.StartsWith(m, StringComparison.Ordinal));
    }

    private static string NormalizeJournalCode(string? code)
    {
        var journalCode = (code ?? "").Trim().ToUpperInvariant();
        return journalCode switch
        {
            "VT" => "VE",
            "CS" => "CA",
            _ => journalCode
        };
    }

    public async Task<ApiResponse<JournalDto>> UpdateJournalAsync(int id, JournalUpdateDto dto, int cabinetId, string userId)
    {
        var j = await _db.JournauxComptables.FirstOrDefaultAsync(x => x.Id == id && x.CabinetId == cabinetId);
        if (j == null) return ApiResponse<JournalDto>.Fail("Journal introuvable.");
        if (!string.IsNullOrWhiteSpace(dto.Libelle)) j.Libelle = dto.Libelle.Trim();
        if (dto.ProchainNumero is > 0) j.ProchainNumero = dto.ProchainNumero.Value;
        if (dto.EstActif.HasValue) j.EstActif = dto.EstActif.Value;
        j.UpdatedAt = DateTime.UtcNow;
        j.UpdatedBy = userId;
        await _db.SaveChangesAsync();
        return ApiResponse<JournalDto>.Ok(new JournalDto
        {
            Id = j.Id,
            Code = j.Code,
            Nom = j.Libelle,
            Type = j.Type,
            ProchainNumero = j.ProchainNumero,
            EstActif = j.EstActif
        }, "Journal mis à jour.");
    }

    public async Task<ApiResponse<EcritureComptableDto>> UpdateEcritureAsync(int id, EcritureCreateDto dto, int cabinetId, string userId)
    {
        var entity = await _db.EcrituresComptables
            .Include(e => e.Lignes)
            .FirstOrDefaultAsync(e => e.Id == id && e.CabinetId == cabinetId);
        if (entity == null) return ApiResponse<EcritureComptableDto>.Fail("Écriture introuvable.");
        if (entity.Statut == "Validee")
            return ApiResponse<EcritureComptableDto>.Fail("Impossible de modifier une écriture validée.");
        var exoBlock = await EnsureExerciceOpenForMutationAsync(entity.ExerciceComptableId, entity.ClientId, cabinetId);
        if (exoBlock != null) return ApiResponse<EcritureComptableDto>.Fail(exoBlock);

        var lignes = (dto.Lignes ?? new())
            .Where(l => !string.IsNullOrWhiteSpace(l.NumCompte) && (l.Debit > 0 || l.Credit > 0))
            .ToList();
        if (lignes.Count < 2)
            return ApiResponse<EcritureComptableDto>.Fail("Au moins deux lignes d'écriture sont requises.");
        var totalD = Math.Round(lignes.Sum(l => l.Debit), 2);
        var totalC = Math.Round(lignes.Sum(l => l.Credit), 2);
        if (totalD != totalC)
            return ApiResponse<EcritureComptableDto>.Fail($"Écriture non équilibrée (Débit {totalD:N2} ≠ Crédit {totalC:N2}).");

        var comptes = await _db.ComptesComptables
            .Where(c => c.ClientId == entity.ClientId && c.CabinetId == cabinetId && c.EstActif)
            .ToDictionaryAsync(c => c.Numero, c => c);

        foreach (var l in lignes)
        {
            if (!comptes.ContainsKey(l.NumCompte.Trim()))
                return ApiResponse<EcritureComptableDto>.Fail($"Compte {l.NumCompte} introuvable ou inactif.");
        }

        if (!string.IsNullOrWhiteSpace(dto.Libelle)) entity.Libelle = dto.Libelle.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Reference)) entity.Reference = dto.Reference.Trim();
        if (dto.DateEcriture != default) entity.DateEcriture = dto.DateEcriture.Date;
        entity.TotalDebit = totalD;
        entity.TotalCredit = totalC;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = userId;

        _db.LignesEcritures.RemoveRange(entity.Lignes);
        entity.Lignes.Clear();
        var order = 0;
        foreach (var l in lignes)
        {
            var numC = l.NumCompte.Trim();
            var compte = comptes[numC];
            entity.Lignes.Add(new LigneEcriture
            {
                CabinetId = cabinetId,
                ClientId = entity.ClientId,
                CompteComptableId = compte.Id,
                NumCompte = numC,
                LibelleCompte = string.IsNullOrWhiteSpace(l.LibelleCompte) ? compte.Libelle : l.LibelleCompte.Trim(),
                LibelleLigne = entity.Libelle,
                Debit = Math.Round(l.Debit, 2),
                Credit = Math.Round(l.Credit, 2),
                SortOrder = order++,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            });
        }

        if (dto.Valider)
        {
            entity.Statut = "Validee";
            entity.DateValidation = DateTime.UtcNow;
            entity.ValideeParUserId = userId;
        }

        await _db.SaveChangesAsync();
        return ApiResponse<EcritureComptableDto>.Ok(new EcritureComptableDto
        {
            Id = entity.Id,
            ClientId = entity.ClientId,
            Journal = entity.JournalCode,
            NumeroPiece = entity.NumeroPiece,
            DateEcriture = entity.DateEcriture,
            Libelle = entity.Libelle,
            Debit = entity.TotalDebit,
            Credit = entity.TotalCredit,
            NombreLignes = entity.Lignes.Count,
            Reference = entity.Reference,
            Statut = MapStatutDisplay(entity.Statut),
            CreatedAt = entity.CreatedAt
        }, "Écriture mise à jour.");
    }

    public async Task<ApiResponse<bool>> DeleteEcritureAsync(int id, int cabinetId, string userId)
    {
        var entity = await _db.EcrituresComptables
            .Include(e => e.Lignes)
            .FirstOrDefaultAsync(e => e.Id == id && e.CabinetId == cabinetId);
        if (entity == null) return ApiResponse<bool>.Fail("Écriture introuvable.");
        if (entity.Statut == "Validee")
            return ApiResponse<bool>.Fail("Impossible de supprimer une écriture validée. Annulez-la d'abord (non supporté en Phase 1).");
        var exoBlock = await EnsureExerciceOpenForMutationAsync(entity.ExerciceComptableId, entity.ClientId, cabinetId);
        if (exoBlock != null) return ApiResponse<bool>.Fail(exoBlock);

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        entity.DeletedBy = userId;
        foreach (var l in entity.Lignes)
        {
            l.IsDeleted = true;
            l.DeletedAt = DateTime.UtcNow;
            l.DeletedBy = userId;
        }
        await _db.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "Écriture supprimée.");
    }

    private IQueryable<LigneEcriture> ValidatedLines(int clientId, int cabinetId, DateTime? dateDebut, DateTime? dateFin, int? exerciceId = null)
    {
        var q = _db.LignesEcritures
            .Include(l => l.Ecriture)
            .Where(l => l.ClientId == clientId && l.CabinetId == cabinetId
                        && l.Ecriture != null && l.Ecriture.Statut == "Validee");
        if (exerciceId is > 0)
            q = q.Where(l => l.Ecriture!.ExerciceComptableId == exerciceId);
        if (dateDebut.HasValue)
            q = q.Where(l => l.Ecriture!.DateEcriture >= dateDebut.Value.Date);
        if (dateFin.HasValue)
            q = q.Where(l => l.Ecriture!.DateEcriture <= dateFin.Value.Date);
        return q;
    }

    private async Task<(DateTime? debut, DateTime? fin)> ResolveExerciceDatesAsync(
        int clientId, int cabinetId, int? exerciceId, DateTime? dateDebut, DateTime? dateFin)
    {
        if (exerciceId is not > 0 || (dateDebut.HasValue && dateFin.HasValue))
            return (dateDebut, dateFin);

        var exo = await _db.ExercicesComptables.AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == exerciceId && e.ClientId == clientId && e.CabinetId == cabinetId);
        if (exo == null) return (dateDebut, dateFin);
        return (dateDebut ?? exo.DateDebut, dateFin ?? exo.DateFin);
    }

    private async Task<ExerciceComptable> CreateExerciceInternalAsync(
        int clientId, int cabinetId, int annee, DateTime dateDebut, DateTime dateFin, string? userId)
    {
        var exercice = new ExerciceComptable
        {
            CabinetId = cabinetId,
            ClientId = clientId,
            Annee = annee,
            DateDebut = dateDebut.Date,
            DateFin = dateFin.Date,
            Statut = "Ouvert",
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow
        };
        _db.ExercicesComptables.Add(exercice);
        await _db.SaveChangesAsync();

        // 12 périodes calendaires basées sur l'année de l'exercice (décalage possible via DateDebut/DateFin)
        for (var m = 1; m <= 12; m++)
        {
            var debut = dateDebut.AddMonths(m - 1);
            if (debut > dateFin) break;
            var fin = debut.AddMonths(1).AddDays(-1);
            if (fin > dateFin) fin = dateFin;
            _db.PeriodesComptables.Add(new PeriodeComptable
            {
                CabinetId = cabinetId,
                ExerciceComptableId = exercice.Id,
                ClientId = clientId,
                Annee = annee,
                Mois = m,
                DateDebut = debut,
                DateFin = fin,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            });
        }
        await _db.SaveChangesAsync();
        return exercice;
    }

    private static ExerciceComptableDto MapExercice(ExerciceComptable e) => new()
    {
        Id = e.Id,
        ClientId = e.ClientId,
        Annee = e.Annee,
        DateDebut = e.DateDebut,
        DateFin = e.DateFin,
        Statut = e.Statut
    };

    private static string MapStatutDisplay(string? statut) => statut switch
    {
        "Validee" => "Validé",
        "Brouillon" => "Brouillon",
        _ => statut ?? "Brouillon"
    };

    private async Task<string?> EnsureExerciceOpenForMutationAsync(int? exerciceId, int clientId, int cabinetId)
    {
        if (exerciceId is > 0)
        {
            var exo = await _db.ExercicesComptables.AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == exerciceId && e.ClientId == clientId && e.CabinetId == cabinetId);
            if (exo == null) return "Exercice introuvable.";
            if (!string.Equals(exo.Statut, "Ouvert", StringComparison.OrdinalIgnoreCase))
                return ExerciceContextService.ClosedExerciceMessage;
            return null;
        }
        var hasOpen = await _db.ExercicesComptables.AnyAsync(e =>
            e.ClientId == clientId && e.CabinetId == cabinetId && e.Statut == "Ouvert");
        return hasOpen ? null : ExerciceContextService.NoOpenExerciceMessage;
    }
}
