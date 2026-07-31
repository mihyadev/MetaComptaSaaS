using MetaCompta.Domain.Common;

namespace MetaCompta.Domain.Entities;

/// <summary>
/// Employee of a client company
/// </summary>
public class Employee : TenantEntity
{
    // Identity
    public string CIN { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string FullName => $"{Prenom} {Nom}";
    public DateTime? DateNaissance { get; set; }
    public string? LieuNaissance { get; set; }
    public string? Adresse { get; set; }
    public string? TelephonePortable { get; set; }
    public string? TelephoneFixe { get; set; }
    public string? Email { get; set; }
    public string? SituationFamiliale { get; set; } // Marié, Célibataire, Divorcé
    public int NombreEnfantsACharge { get; set; } = 0;
    public DateTime? DateMariage { get; set; }
    public string? Sexe { get; set; } // M, F
    public string? PhotoUrl { get; set; }

    // Professional
    public int ClientId { get; set; }
    public virtual Client? Client { get; set; }
    public string? NumeroCNSS { get; set; }
    public DateTime DateEmbauche { get; set; }
    public DateTime? DateSortie { get; set; }
    public string? TypeContrat { get; set; } // CDI, CDD, Intérim, Stage
    public int? DureeContratMois { get; set; } // For CDD
    public string? Poste { get; set; }
    public string? Departement { get; set; }
    public string? Qualification { get; set; }
    public string? CategorieSocioProfessionnelle { get; set; }
    public string? MatriculeEmployeur { get; set; }
    public string Statut { get; set; } = RefCodes.StatutEmploye.Actif; // Actif, Inactif, Suspendu
    public string? MotifSortie { get; set; }

    // Remuneration
    public decimal SalaireBase { get; set; }
    public decimal? PrimeAnciennete { get; set; }
    public decimal? PrimeResponsabilite { get; set; }
    public decimal? PrimePresence { get; set; }
    public decimal? IndemniteTransport { get; set; }
    public decimal? IndemniteRepresentation { get; set; }
    public decimal? AvantagesNature { get; set; }
    public decimal? TauxHoraire { get; set; }
    public string? ModePaiement { get; set; } // Virement, Espèces, Chèque

    // Bank info
    public string? RIB { get; set; } // 24 digits
    public string? NomBanque { get; set; }
    public string? AgenceBancaire { get; set; }

    // Emergency contact
    public string? ContactUrgenceNom { get; set; }
    public string? ContactUrgenceRelation { get; set; }
    public string? ContactUrgenceTelephone { get; set; }
    public string? NotesInternes { get; set; }

    // Navigation
    public virtual ICollection<PaySlip> PaySlips { get; set; } = new List<PaySlip>();
    public virtual ICollection<EmployeeAbsence> Absences { get; set; } = new List<EmployeeAbsence>();
    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
    public virtual ICollection<VariableEmploye> Variables { get; set; } = new List<VariableEmploye>();
    public virtual ICollection<CumulEmploye> Cumuls { get; set; } = new List<CumulEmploye>();
    public virtual ICollection<AffectationRubrique> Affectations { get; set; } = new List<AffectationRubrique>();
}

/// <summary>
/// Employee absence/leave tracking
/// </summary>
public class EmployeeAbsence : TenantEntity
{
    public int EmployeeId { get; set; }
    public virtual Employee? Employee { get; set; }
    public string TypeAbsence { get; set; } = string.Empty; // Maladie, Annuel, Sans Solde, etc.
    public DateTime DateDebut { get; set; }
    public DateTime DateFin { get; set; }
    public int NombreJours { get; set; }
    public string? Motif { get; set; }
    public string Statut { get; set; } = "EnAttente"; // EnAttente, Approuvé, Refusé
    public string? DocumentJustificatifUrl { get; set; }
}
