namespace MetaCompta.Application.Options;

public class PortalUrlsOptions
{
    public const string SectionName = "PortalUrls";

    public string BackOffice { get; set; } = "http://localhost:7101";
    public string Extranet { get; set; } = "http://localhost:7102";
    public string Admin { get; set; } = "http://localhost:7103";
}
