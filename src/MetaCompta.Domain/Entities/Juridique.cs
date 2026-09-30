using MetaCompta.Domain.Common;

namespace MetaCompta.Domain.Entities;

/// <summary>Legal dossier (PV AGO, dépôt RC, etc.).</summary>
public class DossierJuridique : TenantEntity
{
    public int ClientId { get; set; }
    public virtual Client? Client { get; set; }

    /// <summary>PVAG | DepotLegal | RC | Creation | Modification | Other</summary>
    public string TypeDossier { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public string Objet { get; set; } = string.Empty;
    public string? Description { get; set; }
    /// <summary>EnCours | EnAttente | Cloture | Archive</summary>
    public string Statut { get; set; } = "EnCours";
    public DateTime DateOuverture { get; set; } = DateTime.UtcNow;
    public DateTime? DateCloture { get; set; }
    public DateTime? DateDepot { get; set; }
    public DateTime? DateEcheance { get; set; }
    public string? Responsable { get; set; }
    public decimal Montant { get; set; }
    public string? Notes { get; set; }
    /// <summary>Optional checklist JSON for AGO / dépôt RC steps.</summary>
    public string? ChecklistJson { get; set; }
}
