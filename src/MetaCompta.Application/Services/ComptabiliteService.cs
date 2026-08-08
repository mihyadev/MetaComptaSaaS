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

        var year = DateTime.UtcNow.Year;
        var exercice = await _db.ExercicesComptables
            .FirstOrDefaultAsync(e => e.ClientId == clientId && e.CabinetId == cabinetId && e.Annee == year);
        if (exercice == null)
        {
            exercice = new ExerciceComptable
            {
                CabinetId = cabinetId,
                ClientId = clientId,
                Annee = year,
                DateDebut = new DateTime(year, 1, 1),
                DateFin = new DateTime(year, 12, 31),
                Statut = "Ouvert",
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            };
            _db.ExercicesComptables.Add(exercice);
            await _db.SaveChangesAsync();

            for (var m = 1; m <= 12; m++)
            {
                var debut = new DateTime(year, m, 1);
                _db.PeriodesComptables.Add(new PeriodeComptable
                {
                    CabinetId = cabinetId,
                    ExerciceComptableId = exercice.Id,
                    ClientId = clientId,
                    Annee = year,
                    Mois = m,
                    DateDebut = debut,
                    DateFin = debut.AddMonths(1).AddDays(-1),
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow
                });
            }
            await _db.SaveChangesAsync();
        }

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
        var q = _db.ComptesComptables.Where(c => c.ClientId == clientId && c.CabinetId == cabinetId && c.EstActif);
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
                NombreEcritures = await validated.CountAsync(),
                TotalDebit = await validated.SumAsync(e => (decimal?)e.TotalDebit) ?? 0,
                TotalCredit = await validated.SumAsync(e => (decimal?)e.TotalCredit) ?? 0
            });
        }
        return ApiResponse<List<JournalDto>>.Ok(result);
    }

    public async Task<ApiResponse<PagedResult<EcritureComptableDto>>> GetEcrituresAsync(
        int cabinetId, int? clientId, string? journal, int page, string? search)
    {
        if (clientId is > 0)
            await EnsureClientLedgerAsync(clientId.Value, cabinetId);

        page = Math.Max(1, page);
        const int pageSize = 20;

        var q = _db.LignesEcritures
            .Include(l => l.Ecriture)!.ThenInclude(e => e!.Client)
            .Where(l => l.CabinetId == cabinetId && l.Ecriture != null && !l.Ecriture.IsDeleted);

        if (clientId is > 0)
            q = q.Where(l => l.ClientId == clientId);
        if (!string.IsNullOrWhiteSpace(journal))
            q = q.Where(l => l.Ecriture!.JournalCode == journal);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            q = q.Where(l =>
                (l.Ecriture!.Libelle != null && l.Ecriture.Libelle.Contains(s)) ||
                l.NumCompte.Contains(s) ||
                l.Ecriture.NumeroPiece.Contains(s) ||
                (l.LibelleCompte != null && l.LibelleCompte.Contains(s)));
        }

        var total = await q.CountAsync();
        var rows = await q
            .OrderByDescending(l => l.Ecriture!.DateEcriture)
            .ThenByDescending(l => l.EcritureComptableId)
            .ThenBy(l => l.SortOrder)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = rows.Select(l => new EcritureComptableDto
        {
            Id = l.EcritureComptableId,
            ClientId = l.ClientId,
            ClientNom = l.Ecriture?.Client?.RaisonSociale,
            Journal = l.Ecriture?.JournalCode ?? "",
            NumeroPiece = l.Ecriture?.NumeroPiece ?? "",
            DateEcriture = l.Ecriture?.DateEcriture ?? DateTime.MinValue,
            NumCompte = l.NumCompte,
            LibelleCompte = l.LibelleCompte,
            Libelle = l.LibelleLigne ?? l.Ecriture?.Libelle,
            Debit = l.Debit,
            Credit = l.Credit,
            Reference = l.Ecriture?.Reference,
            Statut = MapStatutDisplay(l.Ecriture?.Statut),
            CreatedAt = l.Ecriture?.CreatedAt ?? l.CreatedAt
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

        var year = dto.DateEcriture.Year;
        var exercice = await _db.ExercicesComptables
            .FirstOrDefaultAsync(e => e.ClientId == dto.ClientId && e.CabinetId == cabinetId && e.Annee == year && e.Statut == "Ouvert");
        if (exercice == null)
            return ApiResponse<EcritureComptableDto>.Fail($"Aucun exercice ouvert pour {year}.");

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
            ? $"{journalCode}-{year}-{num:D4}"
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
        int clientId, int cabinetId, DateTime? dateDebut, DateTime? dateFin, string? numCompte = null)
    {
        await EnsureClientLedgerAsync(clientId, cabinetId);
        var q = ValidatedLines(clientId, cabinetId, dateDebut, dateFin);
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
        int clientId, int cabinetId, DateTime? dateDebut, DateTime? dateFin)
    {
        await EnsureClientLedgerAsync(clientId, cabinetId);
        var groups = await ValidatedLines(clientId, cabinetId, dateDebut, dateFin)
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

    private IQueryable<LigneEcriture> ValidatedLines(int clientId, int cabinetId, DateTime? dateDebut, DateTime? dateFin)
    {
        var q = _db.LignesEcritures
            .Include(l => l.Ecriture)
            .Where(l => l.ClientId == clientId && l.CabinetId == cabinetId
                        && l.Ecriture != null && l.Ecriture.Statut == "Validee");
        if (dateDebut.HasValue)
            q = q.Where(l => l.Ecriture!.DateEcriture >= dateDebut.Value.Date);
        if (dateFin.HasValue)
            q = q.Where(l => l.Ecriture!.DateEcriture <= dateFin.Value.Date);
        return q;
    }

    private static string MapStatutDisplay(string? statut) => statut switch
    {
        "Validee" => "Validé",
        "Brouillon" => "Brouillon",
        _ => statut ?? "Brouillon"
    };
}
