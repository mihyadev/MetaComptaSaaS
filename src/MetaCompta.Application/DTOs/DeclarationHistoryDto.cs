namespace MetaCompta.Application.DTOs;

public class DeclarationHistoryDto
{
    public int Id { get; set; }
    public int DeclarationId { get; set; }
    public string AncienStatut { get; set; } = string.Empty;
    public string NouveauStatut { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string? Commentaire { get; set; }
    public DateTime DateChangement { get; set; }
}
