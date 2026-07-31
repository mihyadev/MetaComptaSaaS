using MetaCompta.Domain.Common;

namespace MetaCompta.Application.DTOs;

// ===== Payroll Engine DTOs (Sage-like) =====

public class RubriqueDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Libelle { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Section { get; set; } = string.Empty;
    public int OrdreExecution { get; set; }
    public int OrdreAffichage { get; set; }
    public bool Actif { get; set; }
    public bool Systeme { get; set; }
    public bool AfficherSurBulletin { get; set; }
    public bool Obligatoire { get; set; }
    public string? FormuleBase { get; set; }
    public string? FormuleTaux { get; set; }
    public string? FormuleMontant { get; set; }
    public string? FormuleGain { get; set; }
    public string? FormuleRetenue { get; set; }
    public string? DependsDe { get; set; }
    public string? Description { get; set; }
    public int? ClientId { get; set; }
}

public class RubriqueCreateDto
{
    public string Code { get; set; } = string.Empty;
    public string Libelle { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Section { get; set; } = string.Empty;
    public int OrdreExecution { get; set; }
    public int OrdreAffichage { get; set; }
    public bool Actif { get; set; } = true;
    public bool AfficherSurBulletin { get; set; } = true;
    public bool Obligatoire { get; set; }
    public string? FormuleBase { get; set; }
    public string? FormuleTaux { get; set; }
    public string? FormuleMontant { get; set; }
    public string? FormuleGain { get; set; }
    public string? FormuleRetenue { get; set; }
    public string? DependsDe { get; set; }
    public string? Description { get; set; }
    public int? ClientId { get; set; }
}

public class ConstanteGlobaleDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Libelle { get; set; } = string.Empty;
    public string Categorie { get; set; } = string.Empty;
    public decimal Valeur { get; set; }
    public DateTime DateEffet { get; set; }
    public DateTime? DateFin { get; set; }
    public bool Actif { get; set; }
    public string? Description { get; set; }
}

public class VariableEmployeDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public string RubriqueCode { get; set; } = string.Empty;
    public string? RubriqueLibelle { get; set; }
    public decimal Valeur { get; set; }
    public string? LibellePersonnalise { get; set; }
    public bool Actif { get; set; }
    public DateTime DateEffet { get; set; }
    public DateTime? DateFin { get; set; }
}

public class VariableEmployeCreateDto
{
    public int EmployeeId { get; set; }
    public string RubriqueCode { get; set; } = string.Empty;
    public decimal Valeur { get; set; }
    public string? LibellePersonnalise { get; set; }
    public bool Actif { get; set; } = true;
    public DateTime? DateEffet { get; set; }
    public DateTime? DateFin { get; set; }
}

public class BulletinLigneDto
{
    public int Id { get; set; }
    public string RubriqueCode { get; set; } = string.Empty;
    public string Libelle { get; set; } = string.Empty;
    public string Section { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int OrdreAffichage { get; set; }
    public decimal Base { get; set; }
    public decimal Taux { get; set; }
    public decimal Gain { get; set; }
    public decimal Retenue { get; set; }
}

public class BulletinCompletDto
{
    // Identification Employeur
    public string? EmployeurRaisonSociale { get; set; }
    public string? EmployeurICE { get; set; }
    public string? EmployeurIF { get; set; }
    public string? EmployeurCNSS { get; set; }
    public string? EmployeurAdresse { get; set; }

    // Identification Employe
    public int EmployeeId { get; set; }
    public string? EmployeeNom { get; set; }
    public string? EmployeeCIN { get; set; }
    public string? EmployeeMatricule { get; set; }
    public string? EmployeeCNSS { get; set; }
    public string? EmployeePoste { get; set; }
    public DateTime? EmployeeDateEmbauche { get; set; }
    public string? EmployeeSituationFamiliale { get; set; }
    public int EmployeeNbEnfants { get; set; }

    // Periode
    public int Mois { get; set; }
    public int Annee { get; set; }
    public string? PeriodeLabel { get; set; }

    // Lignes du bulletin groupees par section
    public List<BulletinSectionDto> Sections { get; set; } = new();

    // Totaux
    public decimal TotalGains { get; set; }
    public decimal TotalRetenues { get; set; }
    public decimal SalaireBrut { get; set; }
    public decimal SalaireNetImposable { get; set; }
    public decimal SalaireNet { get; set; }

    // Charges Patronales
    public decimal TotalChargesPatronales { get; set; }
    public decimal CoutTotalEmployeur { get; set; }

    // Cumuls Annuels
    public decimal CumulBrutAnnuel { get; set; }
    public decimal CumulNetImposableAnnuel { get; set; }
    public decimal CumulIRAnnuel { get; set; }
    public decimal CumulCNSSAnnuel { get; set; }
}

public class BulletinSectionDto
{
    public string Section { get; set; } = string.Empty;
    public string SectionLabel { get; set; } = string.Empty;
    public List<BulletinLigneDto> Lignes { get; set; } = new();
    public decimal TotalGains { get; set; }
    public decimal TotalRetenues { get; set; }
}

public class CumulEmployeDto
{
    public string RubriqueCode { get; set; } = string.Empty;
    public string? RubriqueLibelle { get; set; }
    public int Mois { get; set; }
    public int Annee { get; set; }
    public decimal Valeur { get; set; }
}
public class AffectationRubriqueDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public int RubriqueId { get; set; }
    public string RubriqueCode { get; set; } = string.Empty;
    public string? RubriqueLibelle { get; set; }
    public string? RubriqueType { get; set; }
    public string? RubriqueSection { get; set; }
    public decimal ValeurDefaut { get; set; }
    public bool Actif { get; set; }
    public DateTime? DateEffet { get; set; }
    public DateTime? DateFin { get; set; }
}

public class AffectationRubriqueCreateDto
{
    public int EmployeeId { get; set; }
    public int RubriqueId { get; set; }
    public string RubriqueCode { get; set; } = string.Empty;
    public decimal ValeurDefaut { get; set; }
    public bool Actif { get; set; } = true;
    public DateTime? DateEffet { get; set; }
    public DateTime? DateFin { get; set; }
}
